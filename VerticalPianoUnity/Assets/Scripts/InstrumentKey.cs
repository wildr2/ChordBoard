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
    public int LastPlayedMode { get; private set; }

    public InstrumentEmiter Emiter
    {
        get { return Sharp ? emiter_sharp : emiter_natural; }
    }
    private InstrumentEmiter emiter_natural, emiter_sharp;

    // [mode][i]
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
    public void Play(Finger finger, int mode)
    {
        if (mode < 0 || mode > ChordKeys.Length)
        {
            Debug.LogError("Invalid mode");
            return;
        }

        PlayPrimary(finger);
        LastPlayedMode = mode;

        if (ControlFinger != null)
        {
            OnFingerRelease();
        }

        ControlFinger = finger;
        finger.on_release += OnFingerRelease;


        // Chord
        if (ChordKeys != null && mode < ChordKeys.Length && ChordKeys[mode] != null)
        {
            foreach (InstrumentKey key in ChordKeys[mode])
            {
                key.PlayPrimary(finger);
            }
        }   
    }
    private void PlayPrimary(Finger finger)
    {
        Emiter.Play(finger);
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
