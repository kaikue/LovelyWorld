using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Holdable : MonoBehaviour
{
    private const float gravityForce = 40;
    private const float maxFallSpeed = 50;
    private const float throwForceSide = 5;
    private const float throwForceUp = 8;

    private Rigidbody2D rb;
    [HideInInspector]
    public Vector3 holdOffset;
    private Vector3 dropOffset;
    private BoxCollider2D col;
    private bool held = false;
    [HideInInspector]
    public string id;
    private SpriteRenderer sr;
    private bool groundSnapMute;

    public bool snapToGround = true;

    private SoundManager soundManager;
    public AudioClip landSound;
    public AudioClip throwSound;
    public AudioClip dropSound;
    public AudioClip failSound;
    public AudioClip pickupSound;

    private void Awake()
    {
        id = name + " " + SceneManager.GetActiveScene().name + " " + transform.position;

        Persistent[] persistents = FindObjectsOfType<Persistent>();
        foreach(Persistent p in persistents)
        {
            if (p.heldItem != null)
            {
                if (p.heldItem.id == id)
                {
                    Destroy(gameObject);
                }
                break;
            }
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        Vector2 size = sr.sprite.bounds.size;
        holdOffset = new Vector3(0, size.y / 2, 0);
        dropOffset = new Vector3(size.x / 2, 0, 0);

        Persistent persistent = Persistent.GetPersistent();
        soundManager = persistent.GetComponent<SoundManager>();

        groundSnapMute = true;
        StartCoroutine(DisableGroundSnapMute());
        if (snapToGround)
        {
            SnapToGround();
        }
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
            /*if (rb.velocity.y < 0)
            {
                soundManager.PlaySound(landSound);
            }*/
        }
        else
        {
            yVel = Mathf.Max(rb.velocity.y - gravityForce * Time.fixedDeltaTime, -maxFallSpeed);
        }

        if (onCeiling && yVel > 0)
        {
            yVel = 0;
            //soundManager.PlaySound(landSound);
        }

        Vector2 vel = new Vector2(rb.velocity.x, yVel);
        rb.velocity = vel;
        //rb.MovePosition(rb.position + vel * Time.fixedDeltaTime);
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
            if (!groundSnapMute)
            {
                soundManager.PlaySound(landSound); //this won't play when sliding down a wall onto ground of same tileset
            }
        }
    }

    public void PickUp(bool sound = true)
    {
        held = true;
        col.enabled = false;
        rb.isKinematic = true;
        sr.sortingLayerName = "Items";
        if (sound)
        {
            soundManager.PlaySound(pickupSound);
        }
    }

    private bool CanDropAt(Vector2 pos)
    {
        Collider2D overlap = Physics2D.OverlapBox(pos, col.bounds.size, 0, LayerMask.GetMask("Solid"));
        return overlap == null;
    }

    private void Release()
    {
        held = false;
        col.enabled = true;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.parent = null;
    }

    public bool Throw(bool left, Vector2 parentVel)
    {
        if (CanDropAt(rb.position))
        {
            Release();
            int dir = left ? -1 : 1;
            Vector2 vel = new Vector2(dir * throwForceSide, throwForceUp) + parentVel;
            rb.velocity = vel;
            soundManager.PlaySound(throwSound);
            return true;
        }
        else
        {
            soundManager.PlaySound(failSound);
            return false;
        }
    }

    public bool Drop(bool left, Vector3 dropPos)
    {
        int dir = left ? -1 : 1;
        Vector3 newPos = dropPos + dir * dropOffset;
        if (CanDropAt(newPos))
        {
            Release();
            transform.localPosition = newPos;
            rb.velocity = Vector2.zero;
            soundManager.PlaySound(dropSound);
            return true;
        }
        else
        {
            soundManager.PlaySound(failSound);
            return false;
        }
    }

    private void SnapToGround()
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, col.bounds.size, 0, Vector2.down, 1, LayerMask.GetMask("Solid"));
        transform.position += Vector3.down * hit.distance;
    }

    private IEnumerator DisableGroundSnapMute()
    {
        yield return new WaitForFixedUpdate();
        groundSnapMute = false;
    }
}
