using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum Note { A, As, B, C, Cs, D, Ds, E, F, Fs, G, Gs }
public enum NaturalNote { A, B, C, D, E, F, G }

public class Instrument : MonoBehaviour
{
    public const int NotesPerOctave = 12;
    public const int NaturalNotesPerOctave = 7;

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
    public InstrumentEmiter emiter_prefab;
    private AudioClip[] clips; // clip for each possible note across all octaves
    public InstrumentEmiter[] Emiters { get; private set; } // emiters corresponding to clips
    public InstrumentKey[] Keys { get; private set; }

    // Graphics
    public Color[] note_colors;

    // Control
    private List<Note> key_sig = new List<Note> // DEBUG
    { Note.A, Note.B, Note.Cs, Note.D, Note.E, Note.Fs, Note.G };


    // PUBLIC ACCESSORS



    // PUBLIC HELPERS

    public int ClampOctave(int octave)
    {
        return Mathf.Clamp(octave, 0, num_octaves - 1);
    }
    public static bool IsAccidental(Note note)
    {
        return note == Note.As
            || note == Note.Cs
            || note == Note.Ds
            || note == Note.Fs
            || note == Note.Gs;
    }
    public static Note ToggleAccidental(Note note)
    {
        switch (note)
        {
            case Note.A: return Note.As;
            case Note.As: return Note.A;
            case Note.C: return Note.Cs;
            case Note.Cs: return Note.C;
            case Note.D: return Note.Ds;
            case Note.Ds: return Note.D;
            case Note.F: return Note.Fs;
            case Note.Fs: return Note.F;
            case Note.G: return Note.Gs;
            case Note.Gs: return Note.G;
            default: return note;
        }
    }
    public static Note NatNoteToNote(NaturalNote nat_note)
    {
        switch (nat_note)
        {
            case NaturalNote.A: return Note.A;
            case NaturalNote.B: return Note.B;
            case NaturalNote.C: return Note.C;
            case NaturalNote.D: return Note.D;
            case NaturalNote.E: return Note.E;
            case NaturalNote.F: return Note.F;
            case NaturalNote.G: return Note.G;

            default: return Note.A;
        }
    }


    // PRIVATE MODIFIERS

    private void Awake()
    {
        DefineNoteFrequencies();
        CreateNoteClips();
        CreateEmiters();
        InitializeKeys();

        // DEBUG key sig
        for (int i = 0; i < Keys.Length; ++i)
        {
            Note note = Keys[i].Emiter.Note;
            if (!key_sig.Contains(note))
            {
                Keys[i].Sharp = !Keys[i].Sharp; 
            }
        }
        
    }
    private float PosifyAngle(float a)
    {
        a = a % 360f;
        return a > 0 ? a : a + 360f;
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
            for (int i = 0; i < mid_frequencies.Length; ++i)
            {
                frequencies[note_i] = mid_frequencies[i] *
                    Mathf.Pow(2, octave - mid_octave);
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
    private void CreateEmiters()
    {
        GameObject folder = new GameObject("Emiters");
        folder.transform.SetParent(transform);
        folder.transform.SetAsFirstSibling();

        Emiters = new InstrumentEmiter[frequencies.Length];

        // Create audio sources for each note
        for (int i = 0; i < frequencies.Length; ++i)
        {
            Note note = (Note)(i % Tools.EnumLength(typeof(Note)));
            int octave = Mathf.FloorToInt(i / (float)NotesPerOctave);

            InstrumentEmiter emiter = Instantiate(emiter_prefab);
            emiter.transform.SetParent(folder.transform);
            emiter.Initialize(clips[i], note, octave);
            emiter.transform.name = "Emiter " + emiter.NoteName;
            Emiters[i] = emiter;
        }
    }
    private void InitializeKeys()
    {
        Keys = GetComponentsInChildren<InstrumentKey>();

        // Create audio sources for each note
        for (int i = 0; i < Keys.Length; ++i)
        {
            int octave = Mathf.FloorToInt(i / (float)NaturalNotesPerOctave);
            Note note = NatNoteToNote((NaturalNote)(i % NaturalNotesPerOctave));
            Note note_sharp = ToggleAccidental(note);

            Keys[i].Initialize(
                Emiters[(int)note + NotesPerOctave * octave],
                Emiters[(int)note_sharp + NotesPerOctave * octave]);
        }

        // Setup chords
        for (int i = 0; i < Keys.Length; ++i)
        {
            InstrumentKey[][] chord_keys = new InstrumentKey[5][];

            chord_keys[0] = new InstrumentKey[0];
            chord_keys[1] = new InstrumentKey[1] { Keys[(i + 1) % Keys.Length] };
            chord_keys[2] = new InstrumentKey[1] { Keys[(i + 2) % Keys.Length] };
            chord_keys[3] = new InstrumentKey[1] { Keys[(i + 3) % Keys.Length] };
            chord_keys[4] = new InstrumentKey[1] { Keys[(i + 4) % Keys.Length] };

            Keys[i].ChordKeys = chord_keys;
        }
    }

}