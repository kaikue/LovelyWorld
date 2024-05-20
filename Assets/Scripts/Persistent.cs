using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Persistent : MonoBehaviour
{
    public GameObject soulExitGatewayPrefab;
    public AudioClip soulWorldMusic;

    [HideInInspector]
    public bool destroying = false;

    [HideInInspector]
    public string destinationZone = null;
    [HideInInspector]
    public Holdable heldItem = null;
    [HideInInspector]
    public string sendingJarID = null;
    [HideInInspector]
    public string sendingJarType = null;
    [HideInInspector]
    public string limboExitScene;
    [HideInInspector]
    public Vector3? recallPos = null;
    [HideInInspector]
    public bool loadingSoulWorld = false;
    [HideInInspector]
    public Vector3? soulGatewayPos = null;

    private const float musicFadeTime = 2;
    private const float musicEndTime = 0.5f;
    private float baseVol;
    private AudioSource audioSource;
    private Coroutine crtFadeMusic;

    [HideInInspector]
    public Player player;

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
            baseVol = audioSource.volume;
        }
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        player = FindObjectOfType<Player>();

        BGMHolder bgm = FindObjectOfType<BGMHolder>();
        if (bgm != null)
        {
            SetMusic(bgm.music);
        }

        if (loadingSoulWorld)
        {
            SetupSoulWorld();
            loadingSoulWorld = false;
        }

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

        if (recallPos != null)
        {
            player.transform.position = recallPos.Value;
            recallPos = null;
        }

        if (heldItem != null)
        {
            player.PickUpItem(heldItem, true);
            heldItem = null;
        }

        if (sendingJarID != null && SceneManager.GetActiveScene().name != "Limbo")
        {
            player.TeleportToJarPartner(sendingJarID, sendingJarType);
            sendingJarID = null;
            sendingJarType = null;
        }
    }

    public void SetMusic(AudioClip music)
    {
        if (audioSource.clip == null)
        {
            audioSource.clip = music;
            audioSource.Play();
        }
        else if (audioSource.clip != music || crtFadeMusic != null)
        {
            if (crtFadeMusic != null)
            {
                StopCoroutine(crtFadeMusic);
            }
            crtFadeMusic = StartCoroutine(FadeMusic(music));
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
        audioSource.volume = 0;
        yield return new WaitForSeconds(musicEndTime);

        audioSource.volume = baseVol;
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

    private void SetupSoulWorld()
    {
        //spawn exit soul gateway
        //TODO make sure it flips x
        GameObject soulGateway = Instantiate(soulExitGatewayPrefab, soulGatewayPos.Value, Quaternion.identity);

        //flip each object x
        //TODO make sure this is only parents and not children (but holdable check should be on children too)
        
        GameObject[] levelObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        //GameObject.FindGameObjectsWithTag("Untagged"); //excludes player, camera, persistent
        print(levelObjects.Length);
        foreach (GameObject go in levelObjects)
        {
            if (go.CompareTag("Player") || go.CompareTag("MainCamera") || go.CompareTag("Persistent"))
            {
                continue;
            }
            print(go);
            go.transform.localScale = new Vector3(-go.transform.localScale.x, go.transform.localScale.y, go.transform.localScale.z);
            
            Holdable holdable = go.GetComponent<Holdable>();
            if (holdable != null)
            {
                holdable.InstantiateAsSoul();
            }
        }

        //move player to soul gateway pos
        player.transform.position = soulGateway.transform.position;

        //TODO disable screen transitions

        //TODO add border

        //set soul world music
        SetMusic(soulWorldMusic);

        //TODO silhouette palette
    }
}
