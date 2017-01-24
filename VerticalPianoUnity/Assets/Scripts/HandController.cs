using UnityEngine;
using System.Collections;
using System;

public class HandController : MonoBehaviour
{
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
        // Position / Orientation
        LastPos = transform.localPosition;
        transform.localPosition =
            OVRInput.GetLocalControllerPosition(controller);

        transform.rotation = OVRInput.GetLocalControllerRotation(controller);

        // Grab instrument
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            StopAllCoroutines();
            StartCoroutine(UpdateGrabInstrument());
        }

        // Trigger
        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            foreach (Finger finger in fingers)
                finger.Down = true;
        }
        else if (OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
        {
            foreach (Finger finger in fingers)
                finger.Down = false;
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
    private IEnumerator UpdateGrabInstrument()
    {
        Vector3 instr_p0 = instrument.transform.position;
        Quaternion instr_r0 = instrument.transform.rotation;

        Vector3 p0 = transform.position;
        Quaternion r0 = transform.rotation;

        while (OVRInput.Get(OVRInput.Button.One))
        {
            instrument.transform.position = instr_p0 + transform.position - p0;
            instrument.transform.rotation = (Quaternion.Inverse(r0) * transform.rotation) * instr_r0;
            

            yield return null;
        }
    }
}
