using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstrumentEmiter : MonoBehaviour
{
    public Note Note { get; private set; }
    public int Octave { get; private set; }
    public string NoteName { get; private set; }
    
    public AudioSource AudioSource { get; private set; }
    public Finger ControlFinger { get; private set; }

    private const float control_deadzone = 0.025f;


    public void Initialize(AudioClip clip, Note note, int octave)
    {
        Octave = octave;
        Note = note;
        NoteName = Note.ToString() + octave;

        AudioSource = GetComponent<AudioSource>();
        AudioSource.clip = clip;
    }
    public void Play(Finger finger)
    {
        ControlFinger = finger;
        finger.on_release += OnFingerRelease;

        if (!AudioSource.isPlaying)
        {
            AudioSource.Play();
        }
        AudioSource.volume = 1;
        UpdateControl();
    }

    private void Update()
    {
        UpdateControl();
    }
    private void UpdateControl()
    {
        if (ControlFinger == null)
        {
            if (AudioSource.isPlaying)
            {
                // Diminish volume
                AudioSource.volume *= 0.9f;
                if (AudioSource.volume <= 0.001f)
                {
                    AudioSource.Stop();
                }
            }
        }
        else
        {
            //float v = ControlFinger.Hand.GetVelocity().magnitude;
            //AudioSource.volume = Mathf.Clamp01(
            //    (v - control_deadzone) / (1.5f - control_deadzone));
            AudioSource.volume *= 0.99f;
        }
    }
    private void OnFingerRelease()
    {
        ControlFinger.on_release -= OnFingerRelease;
        ControlFinger = null;
    }
}
