using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public GameObject soundEffectPrefab;

    public void PlaySound(AudioClip sound, bool randomizePitch = false)
    {
        GameObject sfx = Instantiate(soundEffectPrefab);
        sfx.GetComponent<SoundEffect>().PlaySound(sound, randomizePitch);
    }
}
