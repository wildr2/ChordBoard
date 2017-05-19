using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class Instrument : MonoBehaviour
{
    public bool debug_mute = false;
    public const int KeysPerOctave = 12;

    // Sound Parameters
    public float gain = 0.05f;
    public int samplerate = 44100;

    // Frequencies
    private int num_octaves = 5;
    private int octave_shift = -1;
    private float[] mid_frequencies = new float[] // for the middle octave
    {
        220.000f,233.082f,246.942f,261.626f,
        277.183f,293.665f,311.127f,329.628f,
        349.228f,369.994f,391.995f,415.305f
    };
    private float[] frequencies;

    // Playback
    // emiter for each possible note across all octaves
    public InstrumentEmiter[] Emiters { get; private set; }
    public InstrumentEmiter emiter_prefab;
    private AudioClip[] clips; // clip for each emiter

    // Control
    //public Vector3 CurveCenter { get; private set; }
    public InstrumentKey[] AllKeys { get; private set; }
    public InstrumentKey[][][] Keys { get; private set; } // [panel i][board i][key i]
    public InstrumentKey key_prefab;
    private List<string> key_sig = new List<string> // DEBUG
    { Note.Af, Note.Bf, Note.C, Note.D, Note.Ef, Note.F, Note.G };

    // Graphics
    public Color[] note_colors;


    // PUBLIC ACCESSORS

    public Plane GetPlane()
    {
        return new Plane(transform.forward, transform.position);
    }


    // PUBLIC HELPERS

    public int ClampOctave(int octave)
    {
        return Mathf.Clamp(octave, 0, num_octaves - 1);
    }


    // PRIVATE MODIFIERS

    private void Awake()
    {
        DefineNoteFrequencies();

        if (!debug_mute)
        {
            CreateNoteClips();
        }
        
        CreateEmiters();
        CreateKeys();

        // DEBUG key sig
        for (int pi = 0; pi < Keys.Length; ++pi)
        {
            for (int i = 0; i < Keys[pi].Length; ++i)
            {
                for (int j = 0; j < Keys[pi][i].Length; ++j)
                {
                    string sig_note = key_sig[j % Note.Natural.Length];
                    Keys[pi][i][j].NoteType = Note.GetNoteType(sig_note);
                }
            }
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
                    Mathf.Pow(2, octave - mid_octave + octave_shift);
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
            string note = Note.Unique[i % Note.Unique.Length];
            int octave = Mathf.FloorToInt(i / (float)KeysPerOctave);

            InstrumentEmiter emiter = Instantiate(emiter_prefab);
            emiter.transform.SetParent(folder.transform);
            emiter.Initialize(debug_mute ? null : clips[i], note, octave, this);
            emiter.transform.name = "Emiter " + emiter.NoteName;
            Emiters[i] = emiter;
        }
    }
    private void CreateKeys()
    {
        int panels = 1;
        int boards = 4;
        int keys_per_board = Note.Natural.Length * num_octaves;

        Keys = new InstrumentKey[panels][][];
        AllKeys = new InstrumentKey[panels * boards * keys_per_board];

        float panel_spacing = 0.2f;
        float board_spacing = 0f;
        float key_spacing = 0f;

        InstrumentKey query_key = Instantiate(key_prefab);
        float key_w = query_key.GetCollisionBounds().size.x;
        float key_h = query_key.GetCollisionBounds().size.y;
        Destroy(query_key.gameObject);


        // Create
        int key_num = 0;
        for (int p = 0; p < panels; ++p)
        {
            //Panel
            GameObject panel = new GameObject("Panel " + p);
            panel.transform.SetParent(transform);
            panel.transform.localPosition = new Vector3(0, 0, panel_spacing * -p);
            panel.transform.localRotation = Quaternion.identity;

            Keys[p] = new InstrumentKey[boards][];

            for (int b = 0; b < boards; ++b)
            {
                // Board
                GameObject board = new GameObject("Board " + b);
                board.transform.SetParent(panel.transform);
                board.transform.localPosition = new Vector3(0, (key_h + board_spacing) * -b, 0);
                board.transform.localRotation = Quaternion.identity;

                // Keys
                Keys[p][b] = new InstrumentKey[keys_per_board];
                for (int k = 0; k < keys_per_board; ++k)
                {
                    int octave = Mathf.FloorToInt(k / (float)Note.Natural.Length);
                    int nat_note_i = k % Note.Natural.Length;
                    string note = Note.Natural[nat_note_i];

                    InstrumentKey key = Instantiate(key_prefab);
                    key.transform.SetParent(board.transform);
                    key.transform.localPosition = new Vector3(k * (key_w + key_spacing), 0, 0);
                    key.transform.localRotation = Quaternion.identity;

                    key.Initialize(
                        EmitterFromNote(note, octave),
                        EmitterFromNote(Note.SetNoteType(note, NoteType.Flat), octave),
                        EmitterFromNote(Note.SetNoteType(note, NoteType.Sharp), octave),
                        note_colors[nat_note_i]);

                    key.name = "Key " + key.Emiter.NoteName;

                    Keys[p][b][k] = key;
                    AllKeys[key_num] = key;
                    ++key_num;
                }

                // Setup chords
                for (int k = 0; k < Keys[p][b].Length; ++k)
                {
                    AssignChordKeys(p, b, k);
                }
            }
        }
    }
    private void AssignChordKeys(int p, int b, int k)
    {
        /*
        0 - center
        1 - right
        2 - up
        3 - left
        4 - down

        x|x|x 
        x||x|x     
        x||||x 
        x|x||x     
        x|||||x

        x 
        xx         
        x|x        
        x||x       
        x|||x  

        x||x||x
        x|||x|x
        x||||||x
        x|x|||x
        x|||||||x

        x|||x|||x
        x||||x|x
        x|||x||x
        x|x||||x
        x||x|||x
            
        */

        int n = Keys[p][b].Length;
        int chords_per_key = 5;

        InstrumentKey[][] chord_keys = new InstrumentKey[chords_per_key][];
        int[][] offsets = new int[chords_per_key][];

        if (b == 0)
        {
            offsets[0] = new int[] { 0, 2, 4 };
            offsets[1] = new int[] { 0, 3, 5 };
            offsets[2] = new int[] { 0, 5 };
            offsets[3] = new int[] { 0, 2, 5 };
            offsets[4] = new int[] { 0, 6 }; 
        }
        else if (b == 1)
        {
            offsets[0] = new int[] { 0 };
            offsets[1] = new int[] { 0, 1 };
            offsets[2] = new int[] { 0, 2 };
            offsets[3] = new int[] { 0, 3 };
            offsets[4] = new int[] { 0, 4 };
        }
        else if (b == 2)
        {
            offsets[0] = new int[] { 0, 3, 6 };
            offsets[1] = new int[] { 0, 4, 6 };
            offsets[2] = new int[] { 0, 7 };
            offsets[3] = new int[] { 0, 2, 6 };
            offsets[4] = new int[] { 0, 8 };
        }
        else if (b == 3)
        {
            offsets[0] = new int[] { 0, 4, 8 };
            offsets[1] = new int[] { 0, 5, 7 };
            offsets[2] = new int[] { 0, 4, 7 };
            offsets[3] = new int[] { 0, 2, 7 };
            offsets[4] = new int[] { 0, 3, 7 };
        }

        for (int i = 0; i < offsets.Length; ++i)
        {
            chord_keys[i] = new InstrumentKey[offsets[i].Length];
            for (int j = 0; j < offsets[i].Length; ++j)
            {
                chord_keys[i][j] = Keys[p][b][(k + offsets[i][j]) % n];
            }
        }
        Keys[p][b][k].ChordKeys = chord_keys;
    }


    // PRIVATE HELPERS

    private InstrumentEmiter EmitterFromNote(string note, int octave)
    {
        int i = 0;
        switch (note)
        {
            case Note.Af: i = octave == 0 ? 0 : -1; break;
            case Note.A: i = 0; break;
            case Note.As: i = 1; break;
            case Note.Bf: i = 1; break;
            case Note.B: i = 2; break;
            case Note.C: i = 3; break;
            case Note.Cs: i = 4; break;
            case Note.Df: i = 4; break;
            case Note.D: i = 5; break;
            case Note.Ds: i = 6; break;
            case Note.Ef: i = 6; break;
            case Note.E: i = 7; break;
            case Note.F: i = 8; break;
            case Note.Fs: i = 9; break;
            case Note.Gf: i = 9; break;
            case Note.G: i = 10; break;
            case Note.Gs: i = 11; break;
            default: return null;
        }

        return Emiters[i + KeysPerOctave * octave];
    }


}

public enum NoteType { Flat, Natural, Sharp }
public static class Note
{
    public const string Af = "Af";
    public const string A = "A";
    public const string As = "As";
    public const string Bf = "Bf";
    public const string B = "B";
    public const string C = "C";
    public const string Cs = "Cs";
    public const string Df = "Df";
    public const string D = "D";
    public const string Ds = "Ds";
    public const string Ef = "Ef";
    public const string E = "E";
    public const string F = "F";
    public const string Fs = "Fs";
    public const string Gf = "Gf";
    public const string G = "G";
    public const string Gs = "Gs";

    public static readonly string[] Natural = new string[]
    { A, B, C, D, E, F, G };
    public static readonly string[] Unique = new string[]
    { A, As, B, C, Cs, D, Ds, E, F, Fs, G, Gs };

    public static NoteType GetNoteType(string note)
    {
        return note.Length <= 1 ? NoteType.Natural :
            note[1] == 'f' ? NoteType.Flat : NoteType.Sharp;
    }
    public static string SetNoteType(string note, NoteType type)
    {
        return type == NoteType.Natural ? note.Substring(0, 1) :
               type == NoteType.Sharp ? note.Substring(0, 1) + "s" :
               note.Substring(0, 1) + "f";
    }
}