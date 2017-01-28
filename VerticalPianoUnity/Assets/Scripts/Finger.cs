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
        UpdateKeyCollision();

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
        Vector3 fingerpos = transform.position;
        Vector3 curve_center = instrument.transform.position +
            instrument.transform.forward * 0.5f;

        curve_center = instrument.transform.InverseTransformPoint(curve_center);
        fingerpos = instrument.transform.InverseTransformPoint(fingerpos);
        curve_center.y = fingerpos.y;
        curve_center = instrument.transform.TransformPoint(curve_center);
        fingerpos = instrument.transform.TransformPoint(fingerpos);

        Vector3 ray_dir = (transform.position - curve_center).normalized;

        Ray ray = new Ray(transform.position, ray_dir);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100))
        {
            InstrumentKey key = hit.collider.GetComponentInParent<InstrumentKey>();
            if (key != null)
            {
                // Collision
                in_key = key;

                line.SetPosition(0, transform.position);
                line.SetPosition(1, transform.position + ray_dir * hit.distance);
            }
        }
        else
        {
            in_key = null;
        }
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

    //private void OnTriggerEnter(Collider collider)
    //{
    //    InstrumentKey key = collider.GetComponentInParent<InstrumentKey>();
    //    if (key != null)
    //    {
    //        in_key = key;
    //    }
    //}
    //private void OnTriggerExit(Collider collider)
    //{
    //    InstrumentKey key = collider.GetComponentInParent<InstrumentKey>();
    //    if (key != null)
    //    {
    //        if (key == in_key) in_key = null;
    //    }
    //}
    //private void OnTriggerStay(Collider collider)
    //{
    //    InstrumentKey key = collider.GetComponentInParent<InstrumentKey>();
    //    if (key != null)
    //    {
    //        in_key = key;
    //    }
    //}
}
