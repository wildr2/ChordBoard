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
        if (instrument != null)
        {
            bool btn_thumbstick_down = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, controller);
            if (btn_thumbstick_down)
            {
                StopAllCoroutines();
                StartCoroutine(UpdateGrabInstrument());
                //instrument.transform.rotation = Quaternion.Euler(Time.time * 360f, Time.time * 360f, Time.time * 360f);
            }
        }

        // Fingers
        for (int i = 0; i < fingers.Length; ++i)
        {
            fingers[i].UpdateFinger();
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
