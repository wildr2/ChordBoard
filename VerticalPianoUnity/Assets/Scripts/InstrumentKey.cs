using UnityEngine;
using System.Collections;
using System;

public class InstrumentKey : MonoBehaviour
{
    new private BoxCollider collider;
    private SpriteRenderer spriter;
    private Color color;

    public bool Sharp { get; set; }

    public InstrumentEmiter Emiter
    {
        get { return Sharp ? emiter_sharp : emiter_natural; }
    }
    private InstrumentEmiter emiter_natural, emiter_sharp;

    // [mode][i]
    public InstrumentKey[][] ChordKeys { get; set; }


    public Bounds GetBounds()
    {
        return collider.bounds;
    }

    public void Initialize(InstrumentEmiter emiter_natural, InstrumentEmiter emiter_sharp, Color color)
    {
        this.emiter_natural = emiter_natural;
        this.emiter_sharp = emiter_sharp;

        spriter = GetComponentInChildren<SpriteRenderer>();
        this.color = color;
        spriter.color = color;
        collider = GetComponentInChildren<BoxCollider>();
    }
    public void Play(Finger finger, int mode)
    {
        if (mode < 0 || mode > ChordKeys.Length)
        {
            Debug.LogError("Invalid mode");
            return;
        }

        PlayPrimary(finger);

        // Chord
        if (ChordKeys != null && mode < ChordKeys.Length && ChordKeys[mode] != null)
        {
            foreach (InstrumentKey key in ChordKeys[mode])
            {
                key.PlayPrimary(finger);
            }
        }   
    }
    private void PlayPrimary(Finger finger)
    {
        Emiter.Play(finger);

        // Graphics
        StopAllCoroutines();
        StartCoroutine(FlashHighlight());
    }

    private IEnumerator FlashHighlight()
    {
        Color c1 = new Color(1, 1, 1, 0.2f);

        for (float t = 0; t < 1; t += Time.deltaTime)
        {
            //spriter.material.SetColor("_Color", Color.Lerp(c1, Color.clear, t));
            spriter.color = Color.Lerp(c1, color, t);
            yield return null;
        }
    }
}
