using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetCrystal : Holdable
{
    public AudioClip smashSound;

    protected override void OnThrownLand()
    {
        Player player = FindObjectOfType<Player>();
        persistent.recallPos = player.transform.position;
        soundManager.PlaySound(smashSound);
        player.ChangeSceneWithHeld(SceneManager.GetActiveScene().name);
    }
}
