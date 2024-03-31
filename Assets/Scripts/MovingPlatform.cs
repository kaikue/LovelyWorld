using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class MovingPlatform : MonoBehaviour
{
    public float speed;
    public bool reverseAtEnd = false;

    private Node[] nodes;
    private Rigidbody2D rb;
    private int prevNode = 0;
    private int nextNode = 0;
    private int dir = 1;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        nodes = transform.parent.GetComponentsInChildren<Node>();
    }

    private void UpdateNextNode()
    {
        prevNode = nextNode;
        if (reverseAtEnd)
        {
            int t = nextNode + dir;
            if (t < 0 || t > nodes.Length)
            {
                dir *= -1;
                t += 2 * dir;
            }
            nextNode = t;
        }
        else {
            nextNode = (nextNode + 1) % nodes.Length;
        }
    }

    private void FixedUpdate()
    {
        if (nodes.Length < 2) return;

        if (Vector3.Distance(transform.position, nodes[nextNode].transform.position) < speed * Time.fixedDeltaTime)
        {
            transform.position = nodes[nextNode].transform.position;
            UpdateNextNode();
            Vector2 diff = nodes[nextNode].transform.position - nodes[prevNode].transform.position;
            rb.velocity = diff.normalized * speed;
        }
    }
}
