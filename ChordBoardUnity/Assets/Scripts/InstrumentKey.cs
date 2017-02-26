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

    public NoteType[] NoteTypes { get; set; }
    public Finger ControlFinger { get; private set; }
    public int LastChordNum { get; private set; }

    private float play_timestamp;
    private float[] play_delays;

    //public InstrumentEmiter[] Emiter
    //{
    //    get { return NoteType == NoteType.Natural ? emiter_natural :
    //                 NoteType == NoteType.Sharp ? emiter_sharp :
    //                 emiter_flat; }
    //}
    private InstrumentEmiter[] emiter_natural, emiter_flat, emiter_sharp;

    // [chord][i]
    public InstrumentKey[][] ChordKeys { get; set; }


    // PUBLIC ACCESSORS

    public InstrumentEmiter GetEmiter(int btn_id)
    {
        return NoteTypes[btn_id] == NoteType.Natural ? emiter_natural[btn_id] :
                     NoteTypes[btn_id] == NoteType.Sharp ? emiter_sharp[btn_id] :
                     emiter_flat[btn_id];
    }
    public InstrumentEmiter[] GetEmiters()
    {
        int n = emiter_natural.Length;
        InstrumentEmiter[] emiters = new InstrumentEmiter[n];
        for (int i = 0; i < n; ++i)
        {
            emiters[i] = GetEmiter(i);
        }
        return emiters;
    }
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

    public void Initialize(InstrumentEmiter[] emiter_natural, InstrumentEmiter[] emiter_flat,
        InstrumentEmiter[] emiter_sharp, Color color)
    {
        NoteTypes = new NoteType[emiter_natural.Length];

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
    public void Play(Finger finger, int btn_id, float twist, float intensity)
    {
        if (GetEmiter(btn_id) != null)
        {
            GetEmiter(btn_id).Play(finger, intensity);
            SetNewControlFinger(finger);
        }
    }
    //public void Play2(Finger finger, int chord, float twist, float intensity)
    //{
    //    if (chord < 0 || chord > ChordKeys.Length)
    //    {
    //        Debug.LogError("Invalid chord number");
    //        return;
    //    }

    //    play_timestamp = Time.time;

    //    ChordKeys[chord][0].Emiter.Play(finger, intensity);
        
    //    SetNewControlFinger(finger);
    //    LastChordNum = chord;
    //}


    // PRIVATE MODIFIERS

    private void Awake()
    {
        collider = GetComponentInChildren<BoxCollider>();
        //spriter = GetComponentInChildren<SpriteRenderer>();
    }
    private void Update()
    {
        bool highlighted = false;

        foreach (InstrumentEmiter emiter in GetEmiters())
        {
            if (emiter == null) continue;
            if (emiter.AudioSource.isPlaying)
            {
                if (!highlight.gameObject.activeInHierarchy)
                    highlight.gameObject.SetActive(true);

                Vector3 s = highlight.transform.localScale;
                s.z = emiter.AudioSource.volume * 0.25f;
                highlight.transform.localScale = s;

                Vector3 p = highlight.transform.localPosition;
                p.z = -s.z / 2f - base_thickness / 2f;
                highlight.transform.localPosition = p;

                highlighted = true;
            }
        }

        if (!highlighted)
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


        foreach (InstrumentEmiter emiter in GetEmiters())
        {
            if (emiter != null)
            {
                emiter.Stop();
            }
        }

        //float dur = Time.time - play_timestamp;

        //// Stop chord keys at right times
        //for (int i = 0; i < ChordKeys[LastChordNum].Length; ++i)
        //{
        //    InstrumentKey key = ChordKeys[LastChordNum][i];

        //    float end_time = play_timestamp + play_delays[i] + dur;
        //    StartCoroutine(CoroutineUtil.DoAfterDelay(
        //        () => key.Emiter.Stop(), end_time - Time.time));
        //}
    }
}
