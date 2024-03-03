using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Holdable
{
    private EnemyMove movement;
    public Blinker mainAnimation;
    public Blinker stunnedAnimation;
    public GameObject defeatedPrefab;

    protected override void Start()
    {
        movement = GetComponent<EnemyMove>();
        base.Start();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!held && !throwing && !dropping)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.velocity = movement.GetMovement(rb.velocity);
            sr.flipX = movement.GetFlipX();
        }
    }

    protected override void OnGrabbed()
    {
        mainAnimation.enabled = false;
        stunnedAnimation.enabled = true;
    }

    protected override void OnThrownLand()
    {
        Defeat();
    }

    protected override void OnDroppedLand()
    {
        stunnedAnimation.enabled = false;
        mainAnimation.enabled = true;
    }

    public void Defeat()
    {
        Instantiate(defeatedPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
