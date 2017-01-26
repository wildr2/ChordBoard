using UnityEngine;
using System.Collections;
using System;

public class HandController : MonoBehaviour
{
    private Coroutine debug_flash_routine;
    public MeshRenderer debug_sphere;

    public OVRInput.Controller controller;
    private Instrument instrument;
    private Finger[] fingers;

    public Vector3 LastPos { get; private set; }
    
   
    public Vector3 GetVelocity()
    {
        return OVRInput.GetLocalControllerVelocity(controller);
    }

    private void Awake()
    {
        instrument = FindObjectOfType<Instrument>();

        // Init Fingers
        fingers = GetComponentsInChildren<Finger>();
        for (int i = 0; i < fingers.Length; ++i)
        {
            fingers[i].Initialize(this, instrument);
        }
    }
    private void Update()
    {
        // Position
        LastPos = transform.position;
        transform.position =
            OVRInput.GetLocalControllerPosition(controller);

        // Rotation
        transform.rotation = OVRInput.GetLocalControllerRotation(controller);

        // Grab instrument
        bool btn_thumbstick_down = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, controller);
        if (btn_thumbstick_down)
        {
            StopAllCoroutines();
            StartCoroutine(UpdateGrabInstrument());
        }

        // Fingers
        UpdateFingerInput();
        for (int i = 0; i < fingers.Length; ++i)
        {
            fingers[i].UpdateFinger();
        }

        // Instrument attach
        //if (LastKey != null)
        //{
        //    Vector3 pos = transform.position;
        //    Vector3 instr_pos = instrument.transform.position;

        //    Vector3 instr_pos_ls = instrument.transform.InverseTransformPoint(instr_pos);
        //    Vector3 hand_pos_ls = instrument.transform.InverseTransformPoint(pos);

        //    //instrument.transform.rotation = Quaternion.LookRotation(GetVelocity().normalized);


        //    float dif = instr_pos_ls.z - hand_pos_ls.z;
        //    float max_dist = 0.005f;

        //    if (Mathf.Abs(dif) > max_dist)
        //    {
        //        Vector3 new_instr_pos_ls = instr_pos_ls;
        //        new_instr_pos_ls.z = hand_pos_ls.z + Mathf.Sign(dif) * max_dist;

        //        instrument.transform.position = instrument.transform.TransformPoint(new_instr_pos_ls);
        //    }
        //}
    }
    private void UpdateFingerInput()
    {
        //float index = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controller);
        bool indexdown = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller);
        bool indexup = OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, controller);
        //bool thumbrestdown = OVRInput.GetDown(OVRInput.Touch.PrimaryThumbRest, controller);
        //bool thumbrestup = OVRInput.GetUp(OVRInput.Touch.PrimaryThumbRest, controller);
        //float hand = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, controller);
        //bool a_down = OVRInput.GetDown(OVRInput.Button.One, controller);
        //bool a_up = OVRInput.GetUp(OVRInput.Button.One, controller);
        //bool b_down = OVRInput.GetDown(OVRInput.Button.Two, controller);
        //bool b_up = OVRInput.GetUp(OVRInput.Button.Two, controller);
        
        // Finger 0
        if (indexdown)
        {
            fingers[0].Down = true;
        }
        else if (indexup)
        {
            fingers[0].Down = false;
        }
    }
    private IEnumerator UpdateGrabInstrument()
    {
        Vector3 instr_p0 = instrument.transform.position;
        Quaternion instr_r0 = instrument.transform.rotation;

        Vector3 p0 = transform.position;
        Quaternion r0 = transform.rotation;

        while (OVRInput.Get(OVRInput.Button.PrimaryThumbstick, controller))
        {
            instrument.transform.position = instr_p0 + transform.position - p0;
            instrument.transform.rotation = (Quaternion.Inverse(r0) * transform.rotation) * instr_r0;
            

            yield return null;
        }
    }

    // Debug
    public void DebugFlash(Color color)
    {
        if (debug_flash_routine != null)
            StopCoroutine(debug_flash_routine);

        debug_flash_routine = StartCoroutine(DebugFlashRoutine(color));
    }
    private IEnumerator DebugFlashRoutine(Color color)
    {
        debug_sphere.enabled = true;
        for (float t = 0; t < 1; t += Time.deltaTime * 2f)
        {
            debug_sphere.material.SetColor("_Color", Color.Lerp(color, Color.clear, t));
            yield return null;
        }
        debug_sphere.enabled = false;
    }
}
