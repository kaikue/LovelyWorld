using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWalkSimple : EnemyMove
{
    private bool left = true;

    public float moveSpeed = 3;
    private const float EPSILON = 0.05f;

    private bool SolidToSide()
    {
        int dir = left ? -1 : 1;
        Vector2 origin = col.bounds.center + new Vector3((col.bounds.extents.x + EPSILON) * dir, col.bounds.extents.y - EPSILON);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, col.bounds.size.y - 2 * EPSILON, LayerMask.GetMask("Solid"));
        Debug.DrawLine(origin, origin + (Vector2.down * (col.bounds.size.y - 2 * EPSILON)));
        return hit.collider != null;
    }

    private bool EmptyBelowSide()
    {
        int dir = left ? -1 : 1;
        Vector2 pt = col.bounds.center + new Vector3((col.bounds.extents.x + EPSILON) * dir, -col.bounds.extents.y - EPSILON);
        Collider2D collider = Physics2D.OverlapPoint(pt, LayerMask.GetMask("Solid"));
        return collider == null;
    }

    public override Vector2 GetMovement(Vector2 baseVel)
    {
        if (SolidToSide() || EmptyBelowSide())
        {
            left = !left;
        }
        
        if (SolidToSide() || EmptyBelowSide())
        {
            //stuck in place
            left = true;
            return baseVel;
        }

        int dir = left ? -1 : 1;
        Vector2 newVel = baseVel + dir * moveSpeed * Vector2.right;
        return newVel;
    }

    public override bool GetFlipX()
    {
        return !left;
    }
}
