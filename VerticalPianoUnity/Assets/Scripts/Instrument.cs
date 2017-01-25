using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum NoteName { A, As, B, C, Cs, D, Ds, E, F, Fs, G, Gs }

public class Instrument : MonoBehaviour
{
    // Sound Parameters
    public float gain = 0.05f;
    public float offset;
    public AnimationCurve curve;
    public float curve_scale = 1;
    public int samplerate = 44100;

    // Frequencies
    private int num_octaves = 3;
    private float[] mid_frequencies = new float[] // for the middle octave
    {
        220.000f,233.082f,246.942f,261.626f,
        277.183f,293.665f,311.127f,329.628f,
        349.228f,369.994f,391.995f,415.305f
    };
    private float[] frequencies;

    // Playback
    private AudioClip[] clips; // clip for each note across all octaves
    public InstrumentKey[] Keys { get; private set; }
    public AudioClip test_clip;

    // Control
    private Vector2 avg_velocity;
    private float avg_angle;
    private AudioSource ctrl_source;
    private NoteName[] key_sig = new NoteName[]
    { NoteName.A, NoteName.B, NoteName.Cs, NoteName.D, NoteName.E, NoteName.Fs, NoteName.G };


    // Events
    //public Action<InstrumentKey> on_key_hit;


    // TEST
    InstrumentKey test_key;
    HandController test_hand;


    // PUBLIC ACCESSORS



    // PUBLIC HELPERS

    public int ClampOctave(int octave)
    {
        return Mathf.Clamp(octave, 0, num_octaves - 1);
    }
    public bool IsAccidental(NoteName note)
    {
        return note == NoteName.As
            || note == NoteName.Cs
            || note == NoteName.Ds
            || note == NoteName.Fs
            || note == NoteName.Gs;
    }


    // PRIVATE MODIFIERS

    private void Awake()
    {
        DefineNoteFrequencies();
        CreateNoteClips();
        InitializeKeys();

        test_key = Keys[8];
        test_hand = FindObjectOfType<HandController>();
    }
    private float PosifyAngle(float a)
    {
        a = a % 360f;
        return a > 0 ? a : a + 360f;
    }
    private void Update()
    {
        //if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        //{
        //    float r = test_hand.transform.localRotation.eulerAngles.z;
        //    int note_i = (int)((6) *
        //        Mathf.Clamp01((Mathf.DeltaAngle(r, 0) + 90) / 180f));
        //    test_key = Keys[note_i];

        //    if (!test_key.AudioSource.isPlaying) test_key.AudioSource.Play();
        //}
        //if (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0)
        //{
        //    if (test_key != null)
        //    {
        //        Vector3 v = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch); //test_hand.transform.position - test_hand.LastPos;
        //        Quaternion av = OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.RTouch);
        //        test_key.AudioSource.volume = v.magnitude / 3f;

        //        //test_key.AudioSource.pitch = 1 + (Mathf.DeltaAngle(r, 0) / 180f) * 0.1f;

        //        //test_key.AudioSource.volume =
        //        //    Mathf.Lerp(test_key.AudioSource.volume, 1, Time.deltaTime * 30f);
        //    }
        //}
        //else
        //{
        //    test_key = null;
        //}

        //foreach (InstrumentKey key in Keys)
        //{
        //    if (key != test_key)
        //    {
        //        key.AudioSource.volume *= 0.8f;
        //    }
        //}


        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    SetData(test_key.AudioSource.clip, mid_frequencies[(int)test_key.Note]);
        //}

    }

    private void SetData(AudioClip clip, float frequency)
    {
        float[] data = new float[clip.samples];

        int harmonics_n = 5;
        int inharmonics_n = 0;
        float num_waves = harmonics_n + inharmonics_n;

        float[] h_weights = new float[harmonics_n];
        float[] h_offsets = new float[harmonics_n];
        float[] inh_freqs = new float[inharmonics_n];
        float[] inh_weights = new float[inharmonics_n];

        for (int j = 0; j < harmonics_n; ++j)
        {
            h_weights[j] = UnityEngine.Random.value * 4f;
            h_offsets[j] = 0; // UnityEngine.Random.value * Mathf.Lerp(0, 5f, (float)j / harmonics_n);
        }
        //for (int j = 0; j < inharmonics_n; ++j)
        //{
        //    inh_freqs[j] = UnityEngine.Random.value * 500f;
        //    inh_weights[j] = UnityEngine.Random.value * 4f;
        //}

        for (int i = 0; i < data.Length; i = i + clip.channels)
        {
            // Position in radians of data point i along 1 Hz wave 
            float x = i * clip.channels * 2f * Mathf.PI / samplerate;

            //Find harmonic wave heights
            float[] hs = new float[harmonics_n];
            for (int j = 0; j < hs.Length; ++j)
            {
                float freq = (frequency + h_offsets[j]) * (j + 1);
                hs[j] = Mathf.Sin(x * freq);
            }

            //data[i] = hs[0] * (0.2f + 0.05f * Mathf.Sin(2f * x)) +
            //          hs[1] * (0.1f + 0.05f * Mathf.Sin(1f * x)) +
            //          hs[2] * (0.1f + 0.05f * Mathf.Sin(1.5f * x)) +
            //          hs[3] * (0.1f + 0.05f * Mathf.Sin(1.25f * x));

            //data[i] = Mathf.Sin(x * frequency + 0.5f * (Mathf.Sin(x * 30f)) + 1) * 0.2f +
            //Mathf.Sin(x * frequency * 2f + 0.5f * Mathf.Sin(x * 10f)) * 0.2f +
            //Mathf.Sin(x * frequency * 3f + 0.5f * Mathf.Sin(x * 20f)) * 0.2f;

            // Find inharmonic wave heights
            //float[] inhs = new float[inharmonics_n];
            //for (int j = 0; j < inhs.Length; ++j)
            //{
            //    float freq = frequency + inh_freqs[j];
            //    inhs[j] = Mathf.Sin(x * freq);
            //}

            // Combine harmonic and inharmonic waves
            data[i] = 0;
            for (int j = 0; j < harmonics_n; ++j)
            {
                data[i] += hs[j] * Mathf.Clamp01(0.5f / num_waves + 0.05f * Mathf.Sin(h_weights[j] * x));
            }
            //for (int j = 0; j < inharmonics_n; ++j)
            //{
            //    data[i] += inhs[j] * Mathf.Clamp01(0.1f / inharmonics_n + 0.01f * Mathf.Sin(inh_weights[j] * x));
            //}


            // Set 2nd channel data
            if (clip.channels == 2) data[i + 1] = data[i];
        }

        clip.SetData(data, 0);
    }
    private void DefineNoteFrequencies()
    {
        // Calculate note frequencies for all octaves  
        frequencies = new float[mid_frequencies.Length * num_octaves];
        int mid_octave = num_octaves / 2;

        int note_i = 0;
        for (int octave = 0; octave < num_octaves; ++octave)
        {
            foreach (float freq in mid_frequencies)
            {
                frequencies[note_i] = freq * Mathf.Pow(2, octave - mid_octave);
                ++note_i;
            }
        }
    }
    private void CreateNoteClips()
    {
        // Create audio clips for each note
        clips = new AudioClip[frequencies.Length];

        for (int i = 0; i < frequencies.Length; ++i)
        {
            AudioClip clip = AudioClip.Create("Note " + i, samplerate * 5, 1, samplerate, false);
            SetData(clip, frequencies[i]);
            clips[i] = clip;
        }
    }
    private void InitializeKeys()
    {
        Keys = GetComponentsInChildren<InstrumentKey>();

        //AudioClip clip = AudioClip.Create("Note", samplerate * 10, 1, samplerate, false);
        //SetData(clip, frequencies[0]);

        // Create audio sources for each note
        for (int i = 0; i < Keys.Length; ++i)
        {
            Keys[i].Initialize(
                (NoteName)(i % Tools.EnumLength(typeof(NoteName))),
                clips[i]);
            //float pitch = frequencies[i] / frequencies[0];
            //Keys[i].AudioSource.pitch = pitch; 
        }
    }

}