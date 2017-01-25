using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Finger : MonoBehaviour
{
    private MeshRenderer meshr;
    private TrailRenderer trail;
    public Color down_color;

    public HandController Hand { get; private set; }
    private Instrument instrument;

    private List<InstrumentKey> held_keys = new List<InstrumentKey>();
    private bool down;
    public bool Down
    {
        get
        {
            return down;
        }
        set
        {
            down = value;
            if (down)
            {
                trail.time = 1.5f;
                meshr.material.SetColor("_Color", down_color);
            }
            else
            {
                trail.time = 0;
                meshr.material.SetColor("_Color", new Color(0.2f, 0.2f, 0.2f));
                ReleaseHeldKeys();
            }
        }
    }
    public Vector3 LastPos { get; private set; }

    public Action<InstrumentKey> on_hit_key;


    public Vector3 GetVelocity()
    {
        return (transform.position - LastPos) / Time.deltaTime;
    }


    public void Initialize(HandController Hand, Instrument instrument)
    {
        meshr = GetComponent<MeshRenderer>();
        this.Hand = Hand;
        this.instrument = instrument;
    }
    public void UpdateFinger()
    {
        if (instrument != null && Down)
            UpdateKeyCollision();
    }
    public void LateUpdate()
    {
        LastPos = transform.position;
    }

    private void Awake()
    {
        trail = GetComponent<TrailRenderer>();
    }
    private void UpdateKeyCollision()
    {
        Vector3 p = instrument.transform.InverseTransformPoint(transform.position);
        Vector3 lastp = instrument.transform.InverseTransformPoint(LastPos);

        if (Mathf.Sign(p.z) != Mathf.Sign(lastp.z))
        {
            foreach (InstrumentKey key in instrument.Keys)
            {
                float dist = Vector3.Distance(transform.position, LastPos);
                float intersect_dist;

                Ray ray = new Ray(LastPos, transform.position - LastPos);

                if (key.GetBounds().IntersectRay(ray, out intersect_dist))
                {
                    if (intersect_dist < dist)
                    {
                        PlayKey(key);
                        break;
                    }
                }
            }
        }
    }
    private void PlayKey(InstrumentKey key)
    {
        key.Play(this);

        if (on_hit_key != null) on_hit_key(key);

        bool sustain = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, Hand.controller);
        if (!sustain)
        {
            ReleaseHeldKeys(key);
        }
        if (!held_keys.Contains(key))
        {
            held_keys.Add(key);
        }
    }
    private void ReleaseHeldKeys(InstrumentKey exception_key = null)
    {
        foreach (InstrumentKey key in held_keys)
        {
            if (key == exception_key) continue;
            key.Release();
        }

        held_keys.Clear();

        if (exception_key != null)
        {
            held_keys.Add(exception_key);
        }    
    }


}
