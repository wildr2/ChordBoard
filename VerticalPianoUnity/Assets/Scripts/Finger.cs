using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Finger : MonoBehaviour
{
    public Color down_color;

    private MeshRenderer meshr;
    private TrailRenderer trail;
    private LineRenderer line;

    private Instrument instrument;
    public HandController Hand { get; private set; }

    // General State 
    private InstrumentKey in_key;
    public bool Down { get; private set; }
    public Vector3 LastPos { get; private set; }

    // Input
    private Vector2 input_stick;
    private float input_index; 

    // Events
    public Action<InstrumentKey> on_hit_key;
    public Action on_release;


    public Vector3 GetVelocity()
    {
        return (transform.position - LastPos) / Time.deltaTime;
    }


    public void Initialize(HandController Hand, Instrument instrument)
    {
        this.Hand = Hand;
        this.instrument = instrument;

        meshr = GetComponent<MeshRenderer>();
        line = GetComponent<LineRenderer>();
    }
    public void UpdateFinger()
    {
        UpdateInput();
        UpdateLine();

        //if (instrument != null)
        //    UpdateKeyCollision();

        // DEBUG
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Bounds b = instrument.Keys[0][0][0].GetBounds();
            //Tools.Log("center " + b.center);
            //Tools.Log("size " + b.size);
            //Tools.Log("min " + b.min);
            //Tools.Log("max " + b.max);

            PlayKey(instrument.Keys[0][0][0], 0);
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
    private void UpdateInput()
    {
        Vector2 prev_stick = input_stick;
        float prev_index = input_index;

        input_index = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, Hand.controller);
        input_stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, Hand.controller);
        bool sustain = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, Hand.controller);

        //bool indexdown = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller);
        //bool indexup = OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, controller);
        //bool sticktouchdown = OVRInput.GetDown(OVRInput.Touch.PrimaryThumbstick, controller);
        //bool sticktouchup = OVRInput.GetUp(OVRInput.Touch.PrimaryThumbstick, controller);
        //bool thumbrestdown = OVRInput.GetDown(OVRInput.Touch.PrimaryThumbRest, controller);
        //bool thumbrestup = OVRInput.GetUp(OVRInput.Touch.PrimaryThumbRest, controller);
        //bool thumbrest = OVRInput.Get(OVRInput.Touch.PrimaryThumbRest, controller);
        //float hand = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, controller);
        //bool a_down = OVRInput.GetDown(OVRInput.Button.One, controller);
        //bool a_up = OVRInput.GetUp(OVRInput.Button.One, controller);
        //bool b_down = OVRInput.GetDown(OVRInput.Button.Two, controller);
        //bool b_up = OVRInput.GetUp(OVRInput.Button.Two, controller);

        float stick_point = 0.75f;
        float trigger_point = 0f;

        bool stick = input_stick.magnitude > stick_point;
        bool index = input_index > trigger_point;
        bool stick_down = stick && prev_stick.magnitude <= stick_point;
        bool index_down = index && prev_index <= trigger_point;

        if (stick_down || index_down)
        {
            // Down
            Down = true;

            trail.time = 1.5f;
            meshr.material.SetColor("_Color", down_color);

            if (!sustain) Release();
            if (in_key != null)
            {
                int mode = 0;
                if (stick_down)
                {
                    int stick_area = 0;
                    float angle = Mathf.Atan2(input_stick.y, input_stick.x) * Mathf.Rad2Deg;
                    angle = Tools.PosifyAngleDeg(angle);
                    stick_area = angle < 45 || angle > 315 ? 0 :
                                 angle < 135 ? 1 :
                                 angle < 225 ? 2 : 3;

                    mode = stick_area + 1;
                }
                PlayKey(in_key, mode);
            }
        }
        if (!stick && !index)
        {
            // Up
            if (Down) Down = false;

            trail.time = 0;
            meshr.material.SetColor("_Color", new Color(0.2f, 0.2f, 0.2f));
            Release();
        }
    }
    private void UpdateLine()
    {
        Plane plane = new Plane(instrument.transform.forward, instrument.transform.position);
        Vector3 dir = -instrument.transform.forward;
        Ray ray = new Ray(transform.position, dir);
        float dist;

        if (plane.Raycast(ray, out dist))
        {
            line.enabled = true;
            line.SetPosition(0, transform.position);
            line.SetPosition(1, transform.position + dir * dist);
        }
        else
        {
            line.enabled = false;
        }
    }
    private void UpdateKeyCollision()
    {
        //float closest_dist = float.MaxValue;
        //InstrumentKey closest = null;

        //for (int pi = 0; pi < instrument.Keys.Length; ++pi)
        //{
        //    Transform panel = instrument.Keys[pi][0][0].transform.parent.parent;

        //    for (int i = 0; i < instrument.Keys[pi].Length; ++i)
        //    {
        //        for (int j = 0; j < instrument.Keys[pi][i].Length; ++j)
        //        {
        //            InstrumentKey key = instrument.Keys[pi][i][j];

        //            // Proximity Calculation
        //            float d = Vector3.Distance(transform.position, key.transform.position);
        //            if (d < closest_dist)
        //            {
        //                closest_dist = d;
        //                closest = key;
        //            }

        //            // Collision
        //            if (Down)
        //            {
        //                if (key.IntersectLine(transform.position, LastPos))
        //                {
        //                    OnTouchKey(key);
        //                }
        //            }                
        //        }
        //    }
        //}

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
    private void PlayKey(InstrumentKey key, int mode)
    {
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
