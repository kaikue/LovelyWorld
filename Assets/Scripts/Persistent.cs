using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Persistent : MonoBehaviour
{
    [HideInInspector]
    public bool destroying = false;

    [HideInInspector]
    public string destinationZone = null;
    [HideInInspector]
    public Holdable heldItem = null;

    private const float musicFadeTime = 2;
    private const float musicEndTime = 1;
    private AudioSource audioSource;

    private Player player;

    private void Awake()
    {
        Persistent[] persistents = FindObjectsOfType<Persistent>();

        if (persistents.Length > 1)
        {
            destroying = true;
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        if (!destroying)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        player = FindObjectOfType<Player>();

        if (destinationZone != null)
        {
            SceneTransition[] transitions = FindObjectsOfType<SceneTransition>();
            foreach (SceneTransition transition in transitions)
            {
                if (transition.zoneName == destinationZone)
                {
                    player.transform.position = transition.spawnPoint.position;
                    break;
                }
            }
            destinationZone = null;
        }

        if (heldItem != null)
        {
            player.PickUpItem(heldItem, false);
            heldItem = null;
        }

        BGMHolder bgm = FindObjectOfType<BGMHolder>();
        if (bgm != null)
        {
            AudioClip music = bgm.music;
            if (audioSource.clip == null)
            {
                audioSource.clip = music;
                audioSource.Play();
            }
            else if (audioSource.clip != music)
            {
                StartCoroutine(FadeMusic(music));
            }
        }
    }

    private IEnumerator FadeMusic(AudioClip music)
    {
        float startVol = audioSource.volume;
        for (float t = 0; t < musicFadeTime; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVol, 0, t / musicFadeTime);
            yield return null;
        }
        yield return new WaitForSeconds(musicEndTime);

        audioSource.volume = startVol;
        audioSource.clip = music;
        audioSource.Play();
    }

    public static Persistent GetPersistent()
    {
        Persistent[] persistents = FindObjectsOfType<Persistent>();
        foreach (Persistent p in persistents)
        {
            if (!p.destroying)
            {
                return p;
            }
        }
        return null;
    }

}
