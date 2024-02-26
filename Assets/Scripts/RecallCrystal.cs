using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RecallCrystal : Holdable
{
    public AudioClip smashSound;
    public GameObject echoPrefab;
    private string recallScene;
    private Vector3 recallPos;
    private GameObject echo;

    protected override void OnGrabbed()
    {
        Player player = FindObjectOfType<Player>();
        recallScene = SceneManager.GetActiveScene().name;
        recallPos = player.transform.position;

        if (echo)
        {
            Destroy(echo);
        }
        echo = Instantiate(echoPrefab, recallPos, Quaternion.identity);
        echo.GetComponent<SpriteRenderer>().flipX = player.facingLeft;
    }

    protected override void OnThrownLand()
    {
        Player player = FindObjectOfType<Player>();
        soundManager.PlaySound(smashSound);
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == recallScene)
        {
            player.transform.position = recallPos;
            if (echo)
            {
                Destroy(echo);
            }
            Destroy(gameObject);
        }
        else
        {
            persistent.recallPos = recallPos;
            player.ChangeSceneWithHeld(recallScene);
        }
    }
}
