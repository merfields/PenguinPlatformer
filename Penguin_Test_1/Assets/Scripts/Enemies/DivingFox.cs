using System.Collections;
using UnityEngine;

public class DivingFox : Enemy
{
    [SerializeField] private Player player;
    private Vector2 enemyVelocity;
    private bool foxMovementAllowed = false;

    protected override void Update()
    {
        UpdateRaycastOrigins();

        if (foxMovementAllowed)
        {
            EnemyMovement();
        }
    }

    protected override void Start()
    {
        base.Start();
        animator.SetInteger("FoxAnimationOrder", 0);
        StartCoroutine("DivingFoxMovementCoroutine");
    }

    protected override void EnemyMovement()
    {
        enemyVelocity = CalculateObjectMovement();
        controller.Move(enemyVelocity);
    }

    IEnumerator DivingFoxMovementCoroutine()
    {
        while (true)
        {
            Vector2 destination;

            destination.x = globalWaypoints[2].x;
            destination.y = globalWaypoints[2].y;

            //While the player is close enough, perform jumping attack
            while (Vector2.Distance(transform.position, player.transform.position) <= 5)
            {
                UpdateGlobalWaypoints();

                if (isFacingRight)
                {
                    globalWaypoints[2].x += 10;
                    globalWaypoints[2].y -= 10;
                }
                else
                {
                    globalWaypoints[2].x -= 10;
                    globalWaypoints[2].y -= 10;
                }

                animator.SetInteger("FoxAnimationOrder", 1);
                foxMovementAllowed = true;

                //When unity reaches its destination continue coroutine
                while (!(controller.collisions.Below))
                {
                    if (fromWaypointIndex == 1)
                    {
                        animator.SetInteger("FoxAnimationOrder", 2);
                    }
                    yield return null;
                }
                foxMovementAllowed = false;
                animator.SetInteger("FoxAnimationOrder", 3);

                //Change jump points, dependent on player position
                float playerDirection = Mathf.Sign(transform.position.x - player.transform.position.x);
                UpdateJumpingPoints(playerDirection);

                yield return new WaitForSeconds(1f);
                animator.SetInteger("FoxAnimationOrder", 4);

                TurnTowardsThePlayer(playerDirection);

                yield return new WaitForSeconds(0.3f);

                percentBetweenWaypoints = 0;

                foxMovementAllowed = true;

                destination.x = globalWaypoints[2].x;
                destination.y = globalWaypoints[2].y;

                yield return null;

            }
            if (fromWaypointIndex == 0)
            {
                foxMovementAllowed = false;
                percentBetweenWaypoints = 0;

                animator.SetInteger("FoxAnimationOrder", 4);
            }
            yield return null;
        }
    }

    private void TurnTowardsThePlayer(float playerDirection)
    {
        if (playerDirection >= 0)
        {
            if (isFacingRight)
            {
                Flip();
            }

            isFacingRight = false;
        }
        else
        {
            if (!isFacingRight)
            {
                Flip();
            }

            isFacingRight = true;
        }
    }

    private void UpdateJumpingPoints(float playerDirection)
    {
        localWayponts[2].x = localWayponts[0].x - playerDirection * (Mathf.Abs(localWayponts[2].x));
        localWayponts[1].x = localWayponts[0].x - playerDirection * (Mathf.Abs(localWayponts[1].x));
        UpdateGlobalWaypoints();
        fromWaypointIndex = 0;
        toWaypointIndex = 1;
    }
}
