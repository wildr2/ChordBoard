using UnityEngine;
using System.Collections;
using System;

public class InstrumentKey : MonoBehaviour
{
    public AudioSource AudioSource { get; private set; }
    public NoteName Note { get; private set; }

    private SpriteRenderer highlight;
    new private BoxCollider collider;
    private Color highlight_normal_color;

    private static HandController[] hands;
    private Finger playing_finger;


    public Bounds GetBounds()
    {
        return collider.bounds;
    }


    public void Initialize(NoteName note, AudioClip clip)
    {
        AudioSource = GetComponent<AudioSource>();

        highlight = GetComponentInChildren<SpriteRenderer>();
        highlight_normal_color = highlight.color;

        collider = GetComponentInChildren<BoxCollider>();

        if (hands == null)
        {
            hands = FindObjectsOfType<HandController>();
        }

        Note = note;
        AudioSource.clip = clip;
    }
    public void Play(Finger finger)
    {
        StopAllCoroutines();
        StartCoroutine(FlashHighlight());

        if (!AudioSource.isPlaying) AudioSource.Play();
        playing_finger = finger;
        UpdateControl();
    }
    public void Release()
    {
        //AudioSource.Stop();
        playing_finger = null;
    }

    private void Awake()
    {
        
    }
    private void Update()
    {
        UpdateControl();
    }
    private void UpdateControl()
    {
        if (playing_finger == null)
        {
            if (AudioSource.isPlaying)
            {
                // Diminish volume quickly
                AudioSource.volume *= 0.9f;
                if (AudioSource.volume <= 0.001f)
                {
                    AudioSource.Stop();
                }
            }
        }
        else
        {
            float deadzone = 0.05f;

            Debug.Log(playing_finger.GetVelocity().magnitude);
            //AudioSource.volume *= 0.95f;
            AudioSource.volume = Mathf.Clamp01(
                (playing_finger.GetVelocity().magnitude - deadzone) / (1 - deadzone));
            //Vector3 v = playing_finger.transform.position - playing_finger.LastPos;
            //AudioSource.volume = Mathf.Clamp01(v.magnitude / 0.01f); 
        }
    }

    private IEnumerator FlashHighlight()
    {
        //highlight.enabled = true;

        Color c1 = new Color(1, 1, 1, 0.2f);

        for (float t = 0; t < 1; t += Time.deltaTime)
        {
            //highlight.material.SetColor("_Color", Color.Lerp(c1, Color.clear, t));
            highlight.color = Color.Lerp(c1, highlight_normal_color, t);
            yield return null;
        }

        //highlight.enabled = false;
    }
}
