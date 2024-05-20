using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulToken : Holdable
{
    public AudioClip smashSound;
    public GameObject soulGatewayPrefab;

    protected override void OnThrownLand()
    {
        Vector3 spawnPos = new Vector3(Mathf.Round(transform.position.x * 2) / 2, Mathf.Round(transform.position.y), 0); //TODO check for nearby clear tiles, if not don't break
        persistent.soulGatewayPos = spawnPos;
        Instantiate(soulGatewayPrefab, spawnPos, Quaternion.identity);
        soundManager.PlaySound(smashSound);
        Destroy(gameObject);
    }
}
