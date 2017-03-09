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
    public float DownValue { get; private set; }
    public Vector3 LastPos { get; private set; }

    // Input
    private Vector2 input_stick;
    private int stick_area;
    private float input_index;
    private float input_hand;
    private float stick_boundary = 0.75f;
    private float idex_boundary = 0f;
    private float hand_boundary = 0;

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

        // DEBUG
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    //Bounds b = instrument.Keys[0][0][0].GetBounds();
        //    //Tools.Log("center " + b.center);
        //    //Tools.Log("size " + b.size);
        //    //Tools.Log("min " + b.min);
        //    //Tools.Log("max " + b.max);

        //    PlayKey(instrument.Keys[0][0][0], 0);
        //}
        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    Release();
        //}
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
        int prev_stick_area = stick_area;
        float prev_index = input_index;
        float prev_hand = input_hand;

        input_index = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, Hand.controller);
        input_stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, Hand.controller);
        input_hand = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, Hand.controller);

        float stick_angle = Mathf.Atan2(input_stick.y, input_stick.x) * Mathf.Rad2Deg;
        stick_angle = Tools.PosifyAngleDeg(stick_angle);
        stick_area = stick_angle < 45 || stick_angle > 315 ? 0 :
                     stick_angle < 135 ? 1 :
                     stick_angle < 225 ? 2 : 3;

        float twist = Mathf.DeltaAngle(0, transform.rotation.eulerAngles.z) / 90f;
        twist = -Mathf.Sign(twist) * Mathf.Pow(twist, 2);

        //Tools.Log(intensity);
        //bool arpegiated = Mathf.Abs(twist) > 20f;
        //if (arpegiated) Hand.DebugFlash(Color.red);

        if (in_key != null)
        {
            bool stick = input_stick.magnitude > stick_boundary;
            bool index = input_index > idex_boundary;
            bool hand = input_hand > hand_boundary;
            bool stick_down = stick && prev_stick.magnitude <= stick_boundary;
            bool index_down = index && prev_index <= idex_boundary;
            bool hand_down = hand && prev_hand <= idex_boundary;

            if (stick_down || (stick && stick_area != prev_stick_area))
            {
                SetDown(stick_area + 1, twist);
            }
            if (index_down || hand_down)
            {
                if (in_key.ControlFinger == this && in_key.LastChordNum == 0)
                {
                    // Don't play key already held in chord 0 by this finger
                    // - down will trigger on next key touched if input still held
                    // - allows fast playing of adjacent keys
                    if (index_down) input_index = prev_index;
                    if (hand_down) input_hand = prev_hand;
                }
                else
                {
                    SetDown(0, twist);
                }
            }
            if (!stick && !index && !hand)
            {
                if (Down) SetUp();
            }

            DownValue = Mathf.Max(
                (input_index - idex_boundary) / (1 - idex_boundary),
                (input_hand - hand_boundary) / (1 - hand_boundary),
                (input_stick.magnitude - stick_boundary) / (1 - stick_boundary));
        }
    }
    private void SetDown(int chord, float twist)
    {
        Down = true;

        trail.time = 1.5f;
        meshr.material.SetColor("_Color", down_color);

        Release();

        if (in_key != null)
        {
            PlayKey(in_key, chord, twist);
        }
    }
    private void SetUp()
    {
        if (Down) Down = false;

        trail.time = 0;
        meshr.material.SetColor("_Color", new Color(0.2f, 0.2f, 0.2f));
        Release();
    }
    private void UpdateLine()
    {
        Plane plane = new Plane(instrument.transform.forward, instrument.transform.position);
        Vector3 dir = instrument.transform.forward;
        Ray ray = new Ray(transform.position, dir);
        float dist;

        if (plane.Raycast(ray, out dist))
        {
            line.SetPosition(0, transform.position);
            line.SetPosition(1, transform.position + dir * dist);
        }
        else
        {
        }
    }
    private void PlayKey(InstrumentKey key, int chord, float twist)
    {
        key.Play(this, chord, twist);
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
            //if (key != in_key)
            //{
            //    if (play_next)
            //    {
            //        Release();
            //        PlayKey(in_key, 0);
            //    }
            //}

            in_key = key;
            
            //if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, Hand.controller))
            //{
            //    PlayKey(in_key, 0);
            //}
        }
    }
}
