using UnityEngine;
using System.Collections;
using System;

public class Finger : MonoBehaviour
{
    private TrailRenderer trail;
    public HandController Hand { get; private set; }
    private Instrument instrument;

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
            }
            else
            {
                trail.time = 0;
                if (LastKey != null)
                {
                    LastKey.Release();
                    LastKey = null;
                }
            }
        }
    }
    public InstrumentKey LastKey { get; private set; }
    public Vector3 LastPos { get; private set; }

    public Action<InstrumentKey> on_hit_key;


    public Vector3 GetVelocity()
    {
        return (transform.position - LastPos) / Time.deltaTime;
    }


    public void Initialize(HandController Hand, Instrument instrument)
    {
        this.Hand = Hand;
        this.instrument = instrument;
    }
    public void UpdateFinger()
    {
        if (Down) UpdateKeyCollision();
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
                if (LastKey == key) continue;

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

        if (LastKey != null && LastKey != key)
        {
            LastKey.Release();
        }
        LastKey = key;
    }


}
