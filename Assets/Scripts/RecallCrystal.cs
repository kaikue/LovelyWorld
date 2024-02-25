using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RecallCrystal : Holdable
{
    public AudioClip smashSound;
    private string recallScene;
    private Vector3 recallPos;

    protected override void OnGrabbed()
    {
        Player player = FindObjectOfType<Player>();
        recallScene = SceneManager.GetActiveScene().name;
        recallPos = player.transform.position;
    }

    protected override void OnThrownLand()
    {
        Player player = FindObjectOfType<Player>();
        soundManager.PlaySound(smashSound);
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == recallScene)
        {
            player.transform.position = recallPos;
            Destroy(gameObject);
        }
        else
        {
            persistent.recallPos = recallPos;
            player.ChangeSceneWithHeld(recallScene);
        }
    }
}
