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

    protected Rigidbody2D rb;
    [HideInInspector]
    public Vector3 holdOffset;
    private Vector3 dropOffset;
    private BoxCollider2D col;
    protected bool held = false;
    [HideInInspector]
    public string id;
    protected SpriteRenderer sr;
    private bool groundSnapMute;
    protected bool throwing = false;
    protected bool dropping = false;
    protected Persistent persistent;

    public bool snapToGround = true;

    protected SoundManager soundManager;
    public AudioClip landSound;
    public AudioClip throwSound;
    public AudioClip dropSound;
    public AudioClip failSound;
    public AudioClip pickupSound;
    public GameObject soulPrefab;

    private void Awake()
    {
        id = name + " " + SceneManager.GetActiveScene().name + " " + transform.position;

        Persistent[] persistents = FindObjectsOfType<Persistent>();
        foreach (Persistent p in persistents)
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

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        Vector2 size = sr.sprite.bounds.size;
        holdOffset = new Vector3(0, size.y / 2, 0);
        dropOffset = new Vector3(size.x / 2, 0, 0);

        persistent = Persistent.GetPersistent();
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


    private Vector2 GetGroundVelocity()
    {
        Vector2 startPoint = new Vector2(col.bounds.min.x, col.bounds.min.y) + Vector2.down * 0.02f;
        Vector2 endPoint = new Vector2(col.bounds.max.x, col.bounds.min.y) + Vector2.down * 0.02f;
        Debug.DrawLine(startPoint, endPoint);
        RaycastHit2D[] hits = Physics2D.RaycastAll(startPoint, endPoint - startPoint, Vector2.Distance(startPoint, endPoint), LayerMask.GetMask("Solid"));
        Vector2 totalVel = Vector2.zero;
        int totalGrounds = 0;
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                Rigidbody2D hitRB = hit.collider.gameObject.GetComponent<Rigidbody2D>();
                if (hitRB != null)
                {
                    totalVel += hitRB.velocity;
                    totalGrounds++;
                }
            }
        }

        if (totalGrounds == 0)
        {
            return Vector2.zero;
        }
        return totalVel / totalGrounds;
    }

    protected bool CheckSide(Vector2 pos1, Vector2 pos2, Vector2 direction)
    {
        Vector2 startPoint = pos1 + direction * 0.02f;
        Vector2 endPoint = pos2 + direction * 0.02f;
        Collider2D collider = RaycastCollision(startPoint, endPoint);
        return collider != null;
    }

    protected virtual void FixedUpdate()
    {
        if (held)
        {
            return;
        }

        float xVel = rb.velocity.x;
        float yVel;

        bool onGround = CheckSide(new Vector2(col.bounds.min.x, col.bounds.min.y), new Vector2(col.bounds.max.x, col.bounds.min.y), Vector2.down);
        bool onCeiling = CheckSide(new Vector2(col.bounds.min.x, col.bounds.max.y), new Vector2(col.bounds.max.x, col.bounds.max.y), Vector2.up);

        if (onGround)
        {
            /*if (rb.velocity.y < 0)
            {
                soundManager.PlaySound(landSound);
            }*/
            Vector2 groundVel = GetGroundVelocity();
            xVel = groundVel.x;
            yVel = groundVel.y;
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

        Vector2 vel = new Vector2(xVel, yVel);
        rb.velocity = vel;
        //rb.MovePosition(rb.position + vel * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject collider = collision.collider.gameObject;

        Enemy enemy = collider.GetComponent<Enemy>();
        if (enemy != null)
        {
            if (throwing || dropping)
            {
                enemy.Defeat();
            }
        }

        if (collider.layer == LayerMask.NameToLayer("Solid"))
        {
            if (rb.constraints == RigidbodyConstraints2D.FreezeRotation)
            {
                //rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
                //rb.velocity = Vector2.zero;
            }
            if (!groundSnapMute && rb.velocity.y < 0)
            {
                soundManager.PlaySound(landSound); //this won't play when sliding down a wall onto ground of same tileset
            }
            if (throwing)
            {
                OnThrownLand();
                throwing = false;
            }
            if (dropping)
            {
                OnDroppedLand();
                dropping = false;
            }
        }
    }

    public void PickUp(bool isRoomTransition = false)
    {
        held = true;
        col.enabled = false;
        rb.isKinematic = true;
        sr.sortingLayerName = "Items";
        if (!isRoomTransition)
        {
            soundManager.PlaySound(pickupSound);
            OnGrabbed();
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
            throwing = true;
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
            dropping = true;
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
        yield return new WaitForSeconds(0.2f);
        groundSnapMute = false;
    }

    protected virtual void OnGrabbed()
    {

    }

    protected virtual void OnThrownLand()
    {

    }

    protected virtual void OnDroppedLand()
    {

    }

    public void InstantiateAsSoul()
    {
        //TODO
    }
}
