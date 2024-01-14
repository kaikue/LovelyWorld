using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Holdable : MonoBehaviour
{
    [HideInInspector]
    public Rigidbody2D rb;
    [HideInInspector]
    public Vector3 offset;
    private BoxCollider2D col;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        offset = new Vector3(0, GetComponent<BoxCollider2D>().bounds.size.y / 2, 0);
    }

    public void PickUp()
    {
        col.enabled = false;
        rb.isKinematic = true;
    }

    public void Throw(bool left)
    {
        col.enabled = true;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        //TODO velocity
        rb.AddForce(new Vector2(500, 500));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //if solid: lock x
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    }
}
