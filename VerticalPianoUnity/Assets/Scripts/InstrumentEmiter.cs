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

    private Instrument instrument;
    private float play_dist;
    private const float control_deadzone = 0.025f;

    private float vibrato_deadzone = 0.05f;
    private float vibrato_intensity = 0.006f;
    private float vibrato_speed = 20f;


    public void Initialize(AudioClip clip, Note note, int octave, Instrument instrument)
    {
        Octave = octave;
        Note = note;
        NoteName = Note.ToString() + octave;

        AudioSource = GetComponent<AudioSource>();
        AudioSource.clip = clip;

        this.instrument = instrument;
    }
    public void Play(Finger finger, float intensity)
    {
        ControlFinger = finger;

        if (!AudioSource.isPlaying)
        {
            AudioSource.Play();
        }
        AudioSource.volume = intensity;

        play_dist = instrument.GetPlane().GetDistanceToPoint(
                ControlFinger.transform.position);

        UpdateControl();
    }
    public void Stop()
    {
        ControlFinger = null;
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
            // Intensity
            float intensity = Mathf.DeltaAngle(-45, ControlFinger.transform.rotation.eulerAngles.x) / 90f;
            AudioSource.volume = Mathf.Clamp01(intensity);

            // Vibrato
            float dist = instrument.GetPlane().GetDistanceToPoint(
                ControlFinger.transform.position);

            float travel = Mathf.Max(0, Mathf.Abs(play_dist - dist) - vibrato_deadzone);
            float vibrato = Mathf.Sin(travel * Mathf.PI * 2f * vibrato_speed);
            AudioSource.pitch = 1 + vibrato * vibrato_intensity;

            //DebugLineDrawer.Draw(ControlFinger.transform.position,
            //    ControlFinger.transform.position + instrument.transform.forward * Mathf.Abs(play_dist - dist),
            //    Color.Lerp(Color.blue, Color.red, str), 0, 0.001f);
        }
    }
}
