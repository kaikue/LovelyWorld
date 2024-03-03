using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected Collider2D col;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    public virtual Vector2 GetMovement(Vector2 baseVel)
    {
        return baseVel;
    }

    public virtual bool GetFlipX()
    {
        return false;
    }
}
