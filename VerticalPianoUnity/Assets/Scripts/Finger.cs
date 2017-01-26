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
                Release();
            }
        }
    }
    public Vector3 LastPos { get; private set; }

    public Action<InstrumentKey> on_hit_key;
    public Action on_release;


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
    

    private void Awake()
    {
        trail = GetComponent<TrailRenderer>();
    }
    private void LateUpdate()
    {
        LastPos = transform.position;
    }
    private void UpdateKeyCollision()
    {
        Vector3 p = instrument.transform.InverseTransformPoint(transform.position);
        Vector3 lastp = instrument.transform.InverseTransformPoint(LastPos);


        if (Mathf.Sign(p.z) != Mathf.Sign(lastp.z))
        {
            for (int i = 0; i < instrument.Keys.Length; ++i)
            {
                InstrumentKey key = instrument.Keys[i];

                float dist = Vector3.Distance(transform.position, LastPos);
                float intersect_dist;

                Ray ray = new Ray(LastPos, transform.position - LastPos);

                if (key.GetBounds().IntersectRay(ray, out intersect_dist))
                {
                    if (intersect_dist < dist)
                    {
                        OnTouchKey(key);
                        break;
                    }
                }
            }
        }
    }
    private void OnTouchKey(InstrumentKey key)
    {
        bool sustain = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, Hand.controller);
        if (!sustain)
        {
            Release();
        }

        PlayKey(key);
    }
    private void PlayKey(InstrumentKey key)
    {
        Vector2 stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, Hand.controller);
        bool stick_touch = OVRInput.Get(OVRInput.Touch.PrimaryThumbstick, Hand.controller);

        int stick_area = 0;
        float angle = Mathf.Atan2(stick.y, stick.x) * Mathf.Rad2Deg;
        angle = Tools.PosifyAngle(angle);
        stick_area = angle < 45 || angle > 315 ? 0 :
                     angle < 135 ? 1 :
                     angle < 225 ? 2 : 3;

        int mode = stick.magnitude <= 0.5f ? 0 : stick_area + 1;

        key.Play(this, mode);
        if (on_hit_key != null) on_hit_key(key);
    }
    private void Release()
    {
        if (on_release != null)
            on_release();
    }
}
