using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class Instrument2 : MonoBehaviour
{
    // Sound Parameters
    public float gain = 0.05f;
    public int samplerate = 44100;

    // Playback
    // emiter for each possible note across all octaves
    private AudioClip clip;
    public AudioSource AudioSource { get; private set; }

    // Control
    private HandController2 controller;


    // PUBLIC ACCESSORS

    public Plane GetPlane()
    {
        return new Plane(transform.forward, transform.position);
    }


    // PUBLIC MODIFIERS


    // PRIVATE MODIFIERS

    private void Awake()
    {
        CreateNoteClips();
        AudioSource.Play();
        AudioSource.loop = true;
    }
    private void Update()
    {
        if (controller != null)
        {

        }
        else
        {
            AudioSource.volume *= 0.9f;
        }

        controller = null;
    }
    private void OnTriggerStay(Collider collider)
    {
        HandController2 hand = collider.GetComponent<HandController2>();
        if (hand != null)
        {
            float speed = hand.GetVelocity().magnitude / 2f;
            AudioSource.volume = AudioSource.volume * 0.4f + speed * 0.6f;
            controller = hand;
        }
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
    private void CreateNoteClips()
    {
        clip = AudioClip.Create("Note", samplerate * 5, 1, samplerate, false);
        SetData(clip, 246.942f);
        AudioSource = GetComponent<AudioSource>();
        AudioSource.clip = clip;
    }

    // PRIVATE HELPERS

}
