using UnityEngine;
using System.Collections;
using System;

public class InstrumentKey : MonoBehaviour
{
    new private BoxCollider collider;
    public MeshRenderer base_mesh;
    public MeshRenderer highlight;
    private Color color;
    private float base_thickness;

    public NoteType NoteType { get; set; }
    public Finger ControlFinger { get; private set; }
    public int LastChordNum { get; private set; }

    private float play_timestamp;
    private float[] play_delays;

    public InstrumentEmiter Emiter
    {
        get { return NoteType == NoteType.Natural ? emiter_natural :
                     NoteType == NoteType.Sharp ? emiter_sharp :
                     emiter_flat; }
    }
    private InstrumentEmiter emiter_natural, emiter_flat, emiter_sharp;

    // [chord][i]
    public InstrumentKey[][] ChordKeys { get; set; }


    // PUBLIC ACCESSORS

    public Bounds GetCollisionBounds()
    {
        return collider.bounds;
    }
    public Color GetColor()
    {
        return color;
    }
    public bool IntersectLine(Vector3 p0, Vector3 p1)
    {
        Ray ray = new Ray(p0, (p1 - p0).normalized);
        float dist;
        bool x = collider.bounds.IntersectRay(ray, out dist);

        return x && dist < Vector3.Distance(p0, p1);
    }


    // PUBLIC MODIFIERS

    public void Initialize(InstrumentEmiter emiter_natural, InstrumentEmiter emiter_flat,
        InstrumentEmiter emiter_sharp, Color color)
    {
        NoteType = NoteType.Natural;

        this.emiter_natural = emiter_natural;
        this.emiter_flat = emiter_flat;
        this.emiter_sharp = emiter_sharp;
        this.color = color;

        base_mesh.material.SetColor("_Color", color);
        base_thickness = base_mesh.bounds.size.z * base_mesh.transform.lossyScale.z;

        Color highlight_color = Color.Lerp(color, Color.white, 0.3f);
        highlight_color.a = 0.3f;
        highlight.material.SetColor("_Color", highlight_color);
        highlight.gameObject.SetActive(false);    
    }
    public void Play(Finger finger, int chord, float twist)
    {
        if (chord < 0 || chord > ChordKeys.Length)
        {
            Debug.LogError("Invalid chord number");
            return;
        }

        play_timestamp = Time.time;

        play_delays = new float[ChordKeys[chord].Length];
        for (int i = 0; i < play_delays.Length; ++i)
        {
            if (twist > 0)
            {
                play_delays[i] = Mathf.Abs(twist) * 0.25f * i;
            }
            else
            {
                play_delays[i] = Mathf.Abs(twist) * 0.25f *
                    (play_delays.Length - 1 - i);
            }
            
        }

        for (int i = 0; i < ChordKeys[chord].Length; ++i)
        {
            InstrumentKey key = ChordKeys[chord][i];
            StartCoroutine(CoroutineUtil.DoAfterDelay(
                () => key.Emiter.Play(finger), play_delays[i]));
        }

        SetNewControlFinger(finger);
        LastChordNum = chord;
    }


    // PRIVATE MODIFIERS

    private void Awake()
    {
        collider = GetComponentInChildren<BoxCollider>();
        //spriter = GetComponentInChildren<SpriteRenderer>();
    }
    private void Update()
    {
        if (Emiter.AudioSource.isPlaying)
        {
            if (!highlight.gameObject.activeInHierarchy)
                highlight.gameObject.SetActive(true);

            Vector3 s = highlight.transform.localScale;
            s.z = Emiter.AudioSource.volume * 0.25f;
            highlight.transform.localScale = s;

            Vector3 p = highlight.transform.localPosition;
            p.z = -s.z / 2f - base_thickness / 2f;
            highlight.transform.localPosition = p; 
        }
        else
        {
            if (highlight.gameObject.activeInHierarchy)
                highlight.gameObject.SetActive(false);
        }
    }

    private void SetNewControlFinger(Finger finger)
    {
        if (ControlFinger != null)
        {
            OnFingerRelease();
        }
        ControlFinger = finger;
        finger.on_release += OnFingerRelease;
    }
    private void OnFingerRelease()
    {
        ControlFinger.on_release -= OnFingerRelease;
        ControlFinger = null;

        float dur = Time.time - play_timestamp;

        // Stop chord keys at right times
        for (int i = 0; i < ChordKeys[LastChordNum].Length; ++i)
        {
            InstrumentKey key = ChordKeys[LastChordNum][i];

            float end_time = play_timestamp + play_delays[i] + dur;
            StartCoroutine(CoroutineUtil.DoAfterDelay(
                () => key.Emiter.Stop(), end_time - Time.time));
        }
    }
    private void OnFingerRelease2()
    {
        float dur = Time.time - play_timestamp;

        Emiter.Stop();

        for (int i = 1; i < ChordKeys[LastChordNum].Length; ++i)
        {
            Finger finger = ControlFinger;

            InstrumentKey key = ChordKeys[LastChordNum][i];
            StartCoroutine(CoroutineUtil.DoAfterDelay(
                () => key.Emiter.Play(finger), (i-1) * dur));

            StartCoroutine(CoroutineUtil.DoAfterDelay(
                () => key.Emiter.Stop(), i * dur));
        }

        ControlFinger.on_release -= OnFingerRelease;
        ControlFinger = null;
    }
}
