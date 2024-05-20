using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoulGateway : EnterDoor
{
    public bool isEntrance = true;
    private Persistent persistent;

    private void Awake()
    {
        persistent = Persistent.GetPersistent();
    }

    public override void Enter()
    {
        if (isEntrance)
        {
            persistent.loadingSoulWorld = true;
            persistent.player.ChangeSceneWithHeld(SceneManager.GetActiveScene().name);
        }
        else
        {
            persistent.player.ChangeSceneWithHeld(SceneManager.GetActiveScene().name);
        }
    }
}
