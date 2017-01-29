using UnityEngine;
using System.Collections;
using System;

public class InstrumentKey : MonoBehaviour
{
    new private BoxCollider collider;
    private SpriteRenderer spriter;
    public MeshRenderer shape;
    private Color color;

    public bool Sharp { get; set; }
    public Finger ControlFinger { get; private set; }
    public int LastChordNum { get; private set; }

    private int arpeg_index = -1;

    public InstrumentEmiter Emiter
    {
        get { return Sharp ? emiter_sharp : emiter_natural; }
    }
    private InstrumentEmiter emiter_natural, emiter_sharp;

    // [chord][i]
    public InstrumentKey[][] ChordKeys { get; set; }

    public Bounds GetBounds()
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

    public void Initialize(InstrumentEmiter emiter_natural, InstrumentEmiter emiter_sharp, Color color)
    {
        this.emiter_natural = emiter_natural;
        this.emiter_sharp = emiter_sharp;
        this.color = color;

        shape.material.SetColor("_Color", color);
        shape.gameObject.SetActive(false);

        spriter.color = color;        
    }
    public void Play(Finger finger, int chord, float twist, float intensity)
    {
        if (chord < 0 || chord > ChordKeys.Length)
        {
            Debug.LogError("Invalid chord number");
            return;
        }

        PlayChord(finger, chord, twist, intensity);        
    }

    private void PlayChord(Finger finger, int chord, float twist, float intensity)
    {
        float[] delays = new float[ChordKeys[chord].Length + 1];
        for (int i = 0; i < delays.Length; ++i)
        {
            delays[i] = Mathf.Abs(twist) * 0.25f * i;
        }

        float delay = delays[twist > 0 ? 0 : delays.Length - 1]; 
        if (delay == 0)
        {
            Emiter.Play(finger, intensity);
        }
        else
        {
            StartCoroutine(CoroutineUtil.DoAfterDelay(
                    () => Emiter.Play(finger, intensity), delay));
        }
        
        for (int i = 0; i < ChordKeys[chord].Length; ++i)
        {
            InstrumentKey key = ChordKeys[chord][i];
            delay = delays[twist > 0 ? i+1 : delays.Length - (i+2)];

            StartCoroutine(CoroutineUtil.DoAfterDelay(
                () => key.Emiter.Play(finger, intensity), delay));
        }

        SetNewControlFinger(finger);
        LastChordNum = chord;
    }
    //private void StartArpegChord(Finger finger, int chord)
    //{
    //    Emiter.Play(finger);
    //    SetNewControlFinger(finger);
    //    LastChordNum = chord;

    //    if (ChordKeys[chord] == null || ChordKeys[chord].Length == 0)
    //    {
    //        // Not possible
    //        arpeg_index = -1;
    //    }
    //    else
    //    {
    //        arpeg_index = 0;
    //    }
    //}
    //private void PlayNextArpeg(Finger finger)
    //{
    //    ChordKeys[LastChordNum][arpeg_index].Emiter.Play(finger);

    //    ++arpeg_index;
    //    if (arpeg_index == ChordKeys[LastChordNum].Length)
    //    {
    //        // No more arpegiated chord notes to play
    //        arpeg_index = -1;
    //    }
    //}
    //private void CancelArpeg()
    //{
    //    arpeg_index = -1;
    //}

    private void SetNewControlFinger(Finger finger)
    {
        if (ControlFinger != null)
        {
            OnFingerRelease();
        }
        ControlFinger = finger;
        finger.on_release += OnFingerRelease;
    }
    

    private void Awake()
    {
        collider = GetComponentInChildren<BoxCollider>();
        spriter = GetComponentInChildren<SpriteRenderer>();
    }
    private void Update()
    {
        if (Emiter.AudioSource.isPlaying)
        {
            if (!shape.gameObject.activeInHierarchy)
                shape.gameObject.SetActive(true);

            Vector3 s = shape.transform.localScale;
            s.z = Emiter.AudioSource.volume * 0.25f;
            shape.transform.localScale = s;

            Vector3 p = shape.transform.localPosition;
            p.z = -s.z / 2f;
            shape.transform.localPosition = p; 
        }
        else
        {
            if (shape.gameObject.activeInHierarchy)
                shape.gameObject.SetActive(false);
        }
    }
    private void OnFingerRelease()
    {
        ControlFinger.on_release -= OnFingerRelease;
        ControlFinger = null;
    }
}
