using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinEnemy : Enemy
{
    [SerializeField]
    float frequency = 20f;

    [SerializeField]
    float magnitude = 0.5f;

    Vector2 pos;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        pos = transform.position;
    }

    // Update is called once per frame
    protected override void Update()
    {
        FindDirection();

        if (isFacingRight)
        {
            MoveRight();
        }
        else
        {
            MoveLeft();
        }
    }

    private void FindDirection()
    {
        if (!isFacingRight && transform.position.x <= globalWaypoints[0].x)
        {
            Flip();
        }else if(isFacingRight && transform.position.x >= globalWaypoints[1].x)
        {
            Flip();
        }
    }

    private void MoveRight()
    {
        pos += Vector2.right * Time.deltaTime * speed;
        transform.position = pos + Vector2.up * Mathf.Sin(Time.time * frequency) * magnitude;
    }

    private void MoveLeft()
    {
        pos -= Vector2.right * Time.deltaTime * speed;
        transform.position = pos + Vector2.up * Mathf.Sin(Time.time * frequency) * magnitude;
    }

}
