using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Holdable : MonoBehaviour
{
    private const float gravityForce = 40;
    private const float maxFallSpeed = 50;
    private const float throwForceSide = 5;
    private const float throwForceUp = 8;

    private Rigidbody2D rb;
    [HideInInspector]
    public Vector3 offset;
    private BoxCollider2D col;
    private bool held = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        offset = new Vector3(0, GetComponent<BoxCollider2D>().bounds.size.y / 2, 0);
    }

    private Collider2D RaycastCollision(Vector2 startPoint, Vector2 endPoint)
    {
        RaycastHit2D hit = Physics2D.Raycast(startPoint, endPoint - startPoint, Vector2.Distance(startPoint, endPoint), LayerMask.GetMask("Solid"));
        return hit.collider;
    }

    protected bool CheckSide(Vector2 pos1, Vector2 pos2, Vector2 direction)
    {
        Vector2 startPoint = pos1 + direction * 0.02f;
        Vector2 endPoint = pos2 + direction * 0.02f;
        Collider2D collider = RaycastCollision(startPoint, endPoint);
        return collider != null;
    }

    private void FixedUpdate()
    {
        if (held)
        {
            return;
        }

        float yVel;

        bool onGround = CheckSide(new Vector2(col.bounds.min.x, col.bounds.min.y), new Vector2(col.bounds.max.x, col.bounds.min.y), Vector2.down);
        bool onCeiling = CheckSide(new Vector2(col.bounds.min.x, col.bounds.max.y), new Vector2(col.bounds.max.x, col.bounds.max.y), Vector2.up);

        if (onGround)
        {
            yVel = 0;
            if (rb.velocity.y < 0)
            {
                //PlaySound(landSound);
            }
        }
        else
        {
            yVel = Mathf.Max(rb.velocity.y - gravityForce * Time.fixedDeltaTime, -maxFallSpeed);
        }

        if (onCeiling && yVel > 0)
        {
            yVel = 0;
            //PlaySound(landSound);
        }

        Vector2 vel = new Vector2(rb.velocity.x, yVel);
        rb.velocity = vel;
        rb.MovePosition(rb.position + vel * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject collider = collision.collider.gameObject;

        if (collider.layer == LayerMask.NameToLayer("Solid"))
        {
            if (rb.constraints == RigidbodyConstraints2D.FreezeRotation)
            {
                rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
                //rb.velocity = Vector2.zero;
            }
            //TODO play sound
        }
    }

    public void PickUp()
    {
        held = true;
        col.enabled = false;
        rb.isKinematic = true;
    }

    public void Throw(bool left, Vector2 parentVel)
    {
        held = false;
        col.enabled = true;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        int dir = left ? -1 : 1;
        Vector2 vel = new Vector2(dir * throwForceSide, throwForceUp) + parentVel;
        //rb.AddForce(force + parentVel);
        rb.velocity = vel;
    }
}
