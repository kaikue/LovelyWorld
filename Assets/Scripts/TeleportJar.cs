using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportJar : MonoBehaviour
{
    public AudioClip teleportSound;
    public string destScene = "current";
    [HideInInspector]
    public string jarType;
    public Transform spawnPoint;

    private void Awake()
    {
        if (destScene == "current")
        {
            destScene = SceneManager.GetActiveScene().name;
        }
        jarType = GetComponent<SpriteRenderer>().sprite.name;
    }
}
