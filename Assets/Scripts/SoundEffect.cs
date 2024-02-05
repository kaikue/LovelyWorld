using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffect : MonoBehaviour
{
    private const float pitchVariation = 0.15f;

    private AudioSource audioSource;

    public void PlaySound(AudioClip sound, bool randomizePitch = false)
    {
        audioSource = GetComponent<AudioSource>();
        if (sound == null)
        {
            print("missing sound!");
            return;
        }
        if (randomizePitch)
        {
            audioSource.pitch = Random.Range(1 - pitchVariation, 1 + pitchVariation);
        }
        else
        {
            audioSource.pitch = 1;
        }
        audioSource.PlayOneShot(sound);
        Destroy(gameObject, sound.length);
    }
}
