using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinEnemy : Enemy
{
    [SerializeField] private float frequency = 20f;
    [SerializeField] private float magnitude = 0.5f;
    private Vector2 pos;

    protected override void Start()
    {
        base.Start();
        pos = transform.position;
    }
    protected override void Update()
    {
        FindDirection();

        if (isFacingRight)
        {
            SinMove(1.0f);
        }
        else
        {
            SinMove(-1.0f);
        }
    }

    private void FindDirection()
    {
        if (!isFacingRight && transform.position.x <= globalWaypoints[0].x)
        {
            Flip();
        }
        else if (isFacingRight && transform.position.x >= globalWaypoints[1].x)
        {
            Flip();
        }
    }

    private void SinMove(float moveDirection)
    {
        pos += moveDirection * Vector2.right * Time.deltaTime * speed;
        transform.position = pos + Vector2.up * Mathf.Sin(Time.time * frequency) * magnitude;
    }
}
