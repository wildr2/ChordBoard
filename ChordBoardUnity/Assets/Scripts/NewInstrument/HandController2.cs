using UnityEngine;
using System.Collections;
using System;

public class HandController2 : MonoBehaviour
{
    private Coroutine debug_flash_routine;
    public MeshRenderer debug_sphere;

    private Instrument2 instrument;

    public OVRInput.Controller controller;
    public OVRInput.Button button_grab = OVRInput.Button.PrimaryThumbstick;
    public Vector3 LastPos { get; private set; }
    

   
    public Vector3 GetVelocity()
    {
        return OVRInput.GetLocalControllerVelocity(controller);
    }

    private void Awake()
    {
        instrument = FindObjectOfType<Instrument2>();
    }
    private void Update()
    {
        // Position
        LastPos = transform.position;
        transform.position =
            OVRInput.GetLocalControllerPosition(controller);

        // Rotation
        transform.rotation = OVRInput.GetLocalControllerRotation(controller);

        // Instrument
        if (instrument != null)
        {
            UpdatePlayingInstrument();
            if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, controller))
            {
                StartCoroutine(GrabObject(instrument.transform));
            }
        }
    }
    private void UpdatePlayingInstrument()
    {
        if (controller == OVRInput.Controller.LTouch)
        {
            instrument.AudioSource.pitch = 3.0f / (transform.position.magnitude * 1.5f + 1);
            // Mathf.DeltaAngle(-90, transform.rotation.eulerAngles.x) / 180f;
        }
    }

    private IEnumerator GrabObject(Transform target) 
    {
        Vector3 pivot = transform.position;
        Vector3 to_pivot = target.position - pivot;
        Quaternion ihand_r0 = Quaternion.Inverse(transform.rotation);
        Quaternion target_r0 = target.rotation;

        while (OVRInput.Get(OVRInput.Button.PrimaryThumbstick, controller))
        {
            Quaternion rel_hand_r = ihand_r0 * transform.rotation;
            Quaternion hand_r = transform.rotation;
            Quaternion ihand_r = Quaternion.Inverse(transform.rotation);

            Quaternion rot = hand_r * rel_hand_r * ihand_r;
            target.rotation = rot * target_r0;
            target.position = transform.position + rot * to_pivot;
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
