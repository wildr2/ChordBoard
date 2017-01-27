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

    private InstrumentKey in_key;
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
                if (in_key != null) OnTouchKey(in_key);
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
        //if (instrument != null)
        //    UpdateKeyCollision();

        Vector3 dir = instrument.transform.forward;
        DebugLineDrawer.Draw(transform.position, transform.position + dir * 0.1f, Color.white, 0, 0.001f);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Bounds b = instrument.Keys[0][0][0].GetBounds();
            //Tools.Log("center " + b.center);
            //Tools.Log("size " + b.size);
            //Tools.Log("min " + b.min);
            //Tools.Log("max " + b.max);

            PlayKey(instrument.Keys[0][0][0]);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Release();
        }
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
        float closest_dist = float.MaxValue;
        InstrumentKey closest = null;

        for (int pi = 0; pi < instrument.Keys.Length; ++pi)
        {
            Transform panel = instrument.Keys[pi][0][0].transform.parent.parent;

            for (int i = 0; i < instrument.Keys[pi].Length; ++i)
            {
                for (int j = 0; j < instrument.Keys[pi][i].Length; ++j)
                {
                    InstrumentKey key = instrument.Keys[pi][i][j];

                    // Proximity Calculation
                    float d = Vector3.Distance(transform.position, key.transform.position);
                    if (d < closest_dist)
                    {
                        closest_dist = d;
                        closest = key;
                    }

                    // Collision
                    if (Down)
                    {
                        if (key.IntersectLine(transform.position, LastPos))
                        {
                            OnTouchKey(key);
                        }
                    }                
                }
            }
        }

        // Vibration
        //float dist_y = Mathf.Abs(transform.position.y - closest.transform.position.y);
        //float x = Mathf.Pow(Mathf.Clamp01(dist_y * 50f), 2);
        //if (dist_y > 0.1f || closest_dist > 0.5f) x = 0;

        //Color key_c = closest.GetColor();
        //key_c.a = 1;
        //Color c = key_c;
        //float width = Mathf.Lerp(0.005f, 0.001f, x);

        //Vector3 p0 = transform.position;
        //Vector3 p1 = closest.transform.position;
        //p1.y = p0.y;
        //p1.x = p0.x;
        //Vector3 dir = (p1 - p0).normalized;
        //p0 -= dir * 10f;
        //p1 += dir * 10f;

        //DebugLineDrawer.Draw(p0, p1, c, 0, width);
        //OVRInput.SetControllerVibration(10f, x);

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

        int stick_area = 0;
        float angle = Mathf.Atan2(stick.y, stick.x) * Mathf.Rad2Deg;
        angle = Tools.PosifyAngleDeg(angle);
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

    private void OnTriggerEnter(Collider collider)
    {
        InstrumentKey key = collider.GetComponentInParent<InstrumentKey>();
        if (key != null)
        {
            in_key = key;
            //if (Down) OnTouchKey(key);
        }
    }
    private void OnTriggerExit(Collider collider)
    {
        InstrumentKey key = collider.GetComponentInParent<InstrumentKey>();
        if (key != null)
        {
            if (key == in_key) in_key = null;
        }
    }
    private void OnTriggerStay(Collider collider)
    {
        InstrumentKey key = collider.GetComponentInParent<InstrumentKey>();
        if (key != null)
        {
            in_key = key;
        }
    }
}
