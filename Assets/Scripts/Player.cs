using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private enum AnimState
    {
        Stand,
        Run,
        Jump,
        Fall,
    }

    private const float runAcceleration = 15;
    private const float maxRunSpeed = 7;
    private const float defaultJumpForce = 8;
    private const float walljumpUpForce = 8 / 1.414f;
    private const float walljumpSideForce = 8 / 1.414f;
    private const float jarLaunchForce = 4;
    private const float gravityForce = 40;
    private const float maxFallSpeed = 50;
    private const float maxJumpTime = 0.3f;
    private const float maxWalljumpTime = 0.3f;
    private const float groundForceFriction = 0.8f;

    private Rigidbody2D rb;
    private EdgeCollider2D ec;

    private bool triggerWasHeld = false;
    private bool jumpQueued = false;
    private bool jumpReleaseQueued = false;
    private bool grabQueued = false;
    private bool downHeld = false;
    private bool downPressQueued = false;
    private bool upHeld = false;
    private bool upPressQueued = false;
    private float xForce = 0;
    private float prevXVel = 0;

    private float jumpForce = defaultJumpForce;
    private bool canJump = false;
    private bool wasOnGround = false;
    private bool jumpRising = false;
    private int walljumpDir = 0;
    private float jumpTimer = 0;
    private Coroutine crtCancelQueuedJump;
    private const float jumpBufferTime = 0.1f; //time before hitting ground a jump will still be queued
    private const float jumpGraceTime = 0.1f; //time after leaving ground player can still jump (coyote time)
    private bool groundSnapMute;

    private const float runFrameTime = 0.1f;
    private SpriteRenderer sr;
    private AnimState animState = AnimState.Stand;
    private int animFrame = 0;
    private float frameTime; //max time of frame
    private float frameTimer; //goes from frameTime down to 0
    public bool facingLeft = false; //for animation (images face right)
    public Sprite standSprite;
    public Sprite jumpSprite;
    public Sprite fallSprite;
    public Sprite downSprite;
    public Sprite[] runSprites;
    public Sprite standHoldSprite;
    public Sprite jumpHoldSprite;
    public Sprite fallHoldSprite;
    public Sprite downHoldSprite;
    public Sprite[] runHoldSprites;
    public float holdSpotHeight;
    public float holdSpotFallHeight;
    public float holdSpotDownHeight;
    public float[] holdSpotRunHeights;

    private SoundManager soundManager;
    public AudioClip jumpSound;
    public AudioClip landSound;

    private Persistent persistent;

    public Transform holdSpot;
    private List<Holdable> validHoldables = new List<Holdable>();
    private Holdable heldItem = null;

    private List<EnterDoor> validDoors = new List<EnterDoor>();

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        ec = gameObject.GetComponent<EdgeCollider2D>();
        sr = gameObject.GetComponent<SpriteRenderer>();

        persistent = Persistent.GetPersistent();
        soundManager = persistent.GetComponent<SoundManager>();

        SnapToGround();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            TryStopCoroutine(crtCancelQueuedJump);
            jumpQueued = true;
            crtCancelQueuedJump = StartCoroutine(CancelQueuedJump());
        }

        if (Input.GetButtonUp("Jump"))
        {
            jumpReleaseQueued = true;
        }

        if (Input.GetButtonDown("Grab"))
        {
            grabQueued = true;
        }

        bool triggerHeld = Input.GetAxis("LTrigger") > 0 || Input.GetAxis("RTrigger") > 0;
        bool triggerPressed = !triggerWasHeld && triggerHeld;
        if (triggerPressed)
        {
            grabQueued = true;
        }
        triggerWasHeld = triggerHeld;

        bool downWasHeld = downHeld;
        downHeld = Input.GetAxis("Vertical") < 0;
        if (downHeld && !downWasHeld)
        {
            downPressQueued = true;
        }

        bool upWasHeld = upHeld;
        upHeld = Input.GetAxis("Vertical") > 0;
        if (upHeld && !upWasHeld)
        {
            upPressQueued = true;
        }

        sr.flipX = facingLeft;
        AdvanceAnim();
        sr.sprite = GetAnimSprite();
        UpdateHoldSpot();
    }

    private Vector2 GetGroundVelocity()
    {
        Vector2 startPoint = rb.position + ec.points[0] + Vector2.down * 0.02f;
        Vector2 endPoint = rb.position + ec.points[1] + Vector2.down * 0.02f;
        RaycastHit2D[] hits = Physics2D.RaycastAll(startPoint, endPoint - startPoint, Vector2.Distance(startPoint, endPoint), LayerMask.GetMask("Solid"));
        Vector2 totalVel = Vector2.zero;
        int totalGrounds = 0;
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                Rigidbody2D hitRB = hit.collider.GetComponent<Rigidbody2D>();
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

    private Collider2D RaycastCollision(Vector2 startPoint, Vector2 endPoint)
    {
        RaycastHit2D hit = Physics2D.Raycast(startPoint, endPoint - startPoint, Vector2.Distance(startPoint, endPoint), LayerMask.GetMask("Solid"));
        return hit.collider;
    }

    private bool CheckSide(int point0, int point1, Vector2 direction)
    {
        Vector2 startPoint = rb.position + ec.points[point0] + direction * 0.02f;
        Vector2 endPoint = rb.position + ec.points[point1] + direction * 0.02f;
        Collider2D collider = RaycastCollision(startPoint, endPoint);
        return collider != null;
    }

    private void FixedUpdate()
    {
        float xInput = Input.GetAxis("Horizontal");
        float xVel;
        float dx = runAcceleration * Time.fixedDeltaTime * xInput;
        if (prevXVel != 0 && Mathf.Sign(xInput) != Mathf.Sign(prevXVel))
        {
            xVel = 0;
        }
        else
        {
            xVel = prevXVel + dx;
            float speedCap = Mathf.Abs(xInput * maxRunSpeed);
            xVel = Mathf.Clamp(xVel, -speedCap, speedCap);
        }

        if (xForce != 0)
        {
            //if not moving: keep xForce
            if (xInput == 0)
            {
                xVel = xForce;
            }
            else
            {
                if (Mathf.Sign(xInput) == Mathf.Sign(xForce))
                {
                    //moving in same direction
                    if (Mathf.Abs(xVel) >= Mathf.Abs(xForce))
                    {
                        //xVel has higher magnitude: set xForce to 0 (replace little momentum push)
                        xForce = 0;
                    }
                    else
                    {
                        //xForce has higher magnitude: set xVel to xForce (pushed by higher momentum)
                        xVel = xForce;
                    }
                }
                else
                {
                    //moving in other direction
                    //decrease xForce by dx (stopping at 0)
                    float prevSign = Mathf.Sign(xForce);
                    xForce += dx;
                    if (Mathf.Sign(xForce) != prevSign)
                    {
                        xForce = 0;
                    }
                    xVel = xForce;
                }
            }
        }

        prevXVel = xVel;

        if (xInput != 0)
        {
            facingLeft = xInput < 0;
        }
        else if (xVel != 0)
        {
            //facingLeft = xVel < 0;
        }

        float yVel;

        bool onGround = CheckSide(0, 1, Vector2.down); //BoxcastTiles(Vector2.down, 0.15f) != null;
        bool onCeiling = CheckSide(2, 3, Vector2.up); //BoxcastTiles(Vector2.up, 0.15f) != null;

        Vector2 groundVel = Vector2.zero;
        if (onGround)
        {
            canJump = true;

            if (xForce != 0)
            {
                xForce *= groundForceFriction;
                if (Mathf.Abs(xForce) < 0.05f)
                {
                    xForce = 0;
                }
            }

            if (rb.velocity.y < 0)
            {
                if (!groundSnapMute)
                {
                    soundManager.PlaySound(landSound, true);
                }
            }
            yVel = 0;

            SetAnimState(xVel == 0 ? AnimState.Stand : AnimState.Run);

            groundVel = GetGroundVelocity();
            xVel += groundVel.x;
            yVel += groundVel.y;
        }
        else
        {
            yVel = Mathf.Max(rb.velocity.y - gravityForce * Time.fixedDeltaTime, -maxFallSpeed);

            if (wasOnGround)
            {
                StartCoroutine(LeaveGround());
            }

            if (yVel < 0)
            {
                SetAnimState(AnimState.Fall);
            }
        }
        wasOnGround = onGround;

        if (onCeiling && yVel > 0)
        {
            yVel = 0;
            //soundManager.PlaySound(landSound, true);
        }

        if (jumpQueued)
        {
            /*
            //walljump
            bool onRight = CheckSide(1, 2, Vector2.right);
            bool onLeft = CheckSide(3, 4, Vector2.left);
            if (!onGround)
            {
                if (onRight)
                {
                    jumpRising = false;
                    StopCancelQueuedJump();
                    jumpQueued = false;
                    canJump = false;
                    xForce = 0;
                    walljumpDir = -1;
                    jumpTimer = 0;
                    soundManager.PlaySound(jumpSound, true);
                    SetAnimState(AnimState.Jump);
                }
                else if (onLeft)
                {
                    jumpRising = false;
                    StopCancelQueuedJump();
                    jumpQueued = false;
                    canJump = false;
                    xForce = 0;
                    walljumpDir = 1;
                    jumpTimer = 0;
                    soundManager.PlaySound(jumpSound, true);
                    SetAnimState(AnimState.Jump);
                }
            }
            */

            //normal jump
            if (canJump)
            {
                StopCancelQueuedJump();
                jumpQueued = false;
                canJump = false;
                xForce = groundVel.x;
                jumpForce = defaultJumpForce + groundVel.y;
                jumpRising = true;
                jumpTimer = 0;
                soundManager.PlaySound(jumpSound, true);
                SetAnimState(AnimState.Jump);
            }
        }

        if (jumpRising)
        {
            yVel = jumpForce;
            jumpTimer += Time.fixedDeltaTime;
            if (jumpTimer >= maxJumpTime)
            {
                jumpRising = false;
            }
        }

        if (walljumpDir != 0)
        {
            yVel = walljumpUpForce;
            xVel = walljumpSideForce * walljumpDir;
            jumpTimer += Time.fixedDeltaTime;
            if (jumpTimer >= maxWalljumpTime)
            {
                walljumpDir = 0;
            }
        }

        if (jumpReleaseQueued)
        {
            if (jumpRising)
            {
                jumpRising = false;
                jumpReleaseQueued = false;
            }

            if (walljumpDir != 0)
            {
                walljumpDir = 0;
            }

            if (!jumpQueued)
            {
                jumpReleaseQueued = false;
            }
        }

        if (grabQueued)
        {
            if (heldItem)
            {
                ThrowHeldItem(downHeld);
            }
            else
            {
                PickUpItem();
            }
        }
        grabQueued = false;

        if (upPressQueued)
        {
            if (validDoors.Count > 0)
            {
                validDoors[0].Enter();
            }
        }
        upPressQueued = false;

        if (downPressQueued)
        {
            TeleportJar jar = GetJarBelow();
            if (jar)
            {
                EnterJar(jar);
            }
        }
        downPressQueued = false;

        Vector2 vel = new Vector2(xVel, yVel);
        rb.velocity = vel;
        rb.MovePosition(rb.position + vel * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!gameObject.activeSelf) return;

        GameObject collider = collision.collider.gameObject;

        if (collider.layer == LayerMask.NameToLayer("Solid"))
        {
            if (collision.GetContact(0).normal.x != 0)
            {
                //against wall, not ceiling
                //PlaySound(bonkSound);
                if (xForce != 0)
                {
                    soundManager.PlaySound(landSound, true);
                }
                xForce = 0;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!gameObject.activeSelf) return;

        GameObject collider = collision.gameObject;

        GrabArea grabArea = collider.GetComponent<GrabArea>();
        if (grabArea != null)
        {
            validHoldables.Add(grabArea.GetComponentInParent<Holdable>());
        }

        SceneTransition sceneTransition = collider.GetComponent<SceneTransition>();
        if (sceneTransition != null)
        {
            EnterSceneTransition(sceneTransition);
        }

        LimboExit limboExit = collider.GetComponent<LimboExit>();
        if (limboExit != null)
        {
            ChangeSceneWithHeld(persistent.limboExitScene);
        }

        EnterDoor enterDoor = collider.GetComponent<EnterDoor>();
        if (enterDoor != null)
        {
            enterDoor.ShowEnterable();
            validDoors.Add(enterDoor);
        }

        /*Gem gem = collider.GetComponent<Gem>();
        if (gem != null)
        {
            Destroy(collider);
            PlaySound(collectGemSound);
            Instantiate(collectParticlePrefab, collider.transform.position, Quaternion.identity);
            levelGems++;
        }*/
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!gameObject.activeSelf) return;

        GameObject collider = collision.gameObject;

        GrabArea grabArea = collider.GetComponent<GrabArea>();
        if (grabArea != null)
        {
            validHoldables.Remove(grabArea.GetComponentInParent<Holdable>());
        }

        EnterDoor enterDoor = collider.GetComponent<EnterDoor>();
        if (enterDoor != null)
        {
            enterDoor.HideEnterable();
            validDoors.Remove(enterDoor);
        }
    }

    private Sprite GetAnimSprite()
    {
        bool holding = heldItem != null;
        switch (animState)
        {
            case AnimState.Stand:
                return holding ? 
                    (downHeld ? downHoldSprite : standHoldSprite) :
                    (downHeld ? downSprite : standSprite);
            case AnimState.Run:
                return holding ? runHoldSprites[animFrame] : runSprites[animFrame];
            case AnimState.Jump:
                return holding ? jumpHoldSprite : jumpSprite;
            case AnimState.Fall:
                return holding ? fallHoldSprite : fallSprite;
        }
        return standSprite;
    }

    private void TryStopCoroutine(Coroutine crt)
    {
        if (crt != null)
        {
            StopCoroutine(crt);
        }
    }

    private void StopCancelQueuedJump()
    {
        TryStopCoroutine(crtCancelQueuedJump);
    }

    private IEnumerator CancelQueuedJump()
    {
        yield return new WaitForSeconds(jumpBufferTime);
        jumpQueued = false;
    }

    private IEnumerator LeaveGround()
    {
        yield return new WaitForSeconds(jumpGraceTime);
        canJump = false;
    }

    private void SetAnimState(AnimState state)
    {
        animState = state;
    }

    private void AdvanceAnim()
    {
        if (animState == AnimState.Run)
        {
            frameTime = runFrameTime;
            AdvanceFrame(runSprites.Length);
        }
        else
        {
            animFrame = 0;
            frameTimer = frameTime;
        }
    }

    private void AdvanceFrame(int numFrames)
    {
        if (animFrame >= numFrames)
        {
            animFrame = 0;
        }

        frameTimer -= Time.deltaTime;
        if (frameTimer <= 0)
        {
            frameTimer = frameTime;
            animFrame = (animFrame + 1) % numFrames;
        }
    }

    private void ThrowHeldItem(bool drop)
    {
        bool success;
        if (drop)
        {
            Vector2 offset = ec.bounds.size.x / 2 * Vector2.right * (facingLeft ? -1 : 1);
            success = heldItem.Drop(facingLeft, rb.position + offset);
        }
        else
        {
            success = heldItem.Throw(facingLeft, rb.velocity);
        }
        if (success)
        {
            heldItem = null;
        }
    }

    private void PickUpItem()
    {
        if (validHoldables.Count > 0)
        {
            PickUpItem(validHoldables[0]);
        }
    }

    public void PickUpItem(Holdable holdable, bool isRoomTransition = false)
    {
        heldItem = holdable;
        heldItem.PickUp(isRoomTransition);
        heldItem.transform.parent = holdSpot;
        heldItem.transform.localPosition = heldItem.holdOffset;
    }

    private void UpdateHoldSpot()
    {
        float h;
        if (animState == AnimState.Fall)
        {
            h = holdSpotFallHeight;
        }
        else if (animState == AnimState.Run)
        {
            h = holdSpotRunHeights[animFrame];
        }
        else if (animState == AnimState.Stand && downHeld)
        {
            h = holdSpotDownHeight;
        }
        else
        {
            h = holdSpotHeight;
        }
        holdSpot.transform.localPosition = Vector2.up * h;
    }

    public void SnapToGround()
    {
        groundSnapMute = true;
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, ec.bounds.size, 0, Vector2.down, 1, LayerMask.GetMask("Solid"));
        if (hit)
        {
            transform.position += Vector3.down * hit.distance;
        }
        StartCoroutine(DisableGroundSnapMute());
    }

    private IEnumerator DisableGroundSnapMute()
    {
        yield return new WaitForFixedUpdate();
        groundSnapMute = false;
    }

    public void ChangeSceneWithHeld(string sceneName)
    {
        if (heldItem)
        {
            heldItem.transform.parent = persistent.transform;
            persistent.heldItem = heldItem;
        }
        SceneManager.LoadScene(sceneName);
    }

    private void EnterSceneTransition(SceneTransition sceneTransition)
    {
        persistent.destinationZone = sceneTransition.zoneName;
        ChangeSceneWithHeld(sceneTransition.destinationScene);
    }

    private TeleportJar GetJarBelow()
    {
        Vector2 mid = ec.points[0] + ec.points[1] / 2;
        Vector2 pos = rb.position + mid + Vector2.down * 0.02f;
        Collider2D[] cols = Physics2D.OverlapBoxAll(pos, new Vector2(0.1f, 0.1f), 0, LayerMask.GetMask("Solid"));
        foreach (Collider2D col in cols)
        {
            TeleportJar jar = col.GetComponent<TeleportJar>();
            if (jar)
            {
                return jar;
            }
        }
        return null;
    }

    private void EnterJar(TeleportJar jar)
    {
        soundManager.PlaySound(jar.teleportSound);
        string jarID = jar.gameObject.GetComponent<Holdable>().id;
        if (jar.destScene == SceneManager.GetActiveScene().name)
        {
            TeleportToJarPartner(jarID, jar.jarType);
        }
        else
        {
            persistent.sendingJarID = jarID;
            persistent.sendingJarType = jar.jarType;
            ChangeSceneWithHeld(jar.destScene);
        }
    }

    public void TeleportToJarPartner(string jarID, string jarType)
    {
        TeleportJar[] jars = FindObjectsOfType<TeleportJar>();
        foreach (TeleportJar otherJar in jars)
        {
            Holdable otherHoldable = otherJar.GetComponent<Holdable>();
            if (jarType == otherJar.jarType && jarID != otherHoldable.id)
            {
                if (otherHoldable == heldItem)
                {
                    persistent.sendingJarID = otherHoldable.id;
                    persistent.sendingJarType = otherJar.jarType;
                    persistent.limboExitScene = otherJar.destScene;
                    SceneManager.LoadScene("Limbo");
                }
                else
                {
                    transform.position = otherJar.spawnPoint.position;
                }
                break;
            }
        }
    }
}
