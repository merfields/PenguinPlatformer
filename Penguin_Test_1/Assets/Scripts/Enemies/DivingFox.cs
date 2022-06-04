using System.Collections;
using UnityEngine;

public class DivingFox : Enemy
{
    [SerializeField] Player player;

    Vector2 enemyVelocity;
    bool foxMovementAllowed = false;

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
        animator.SetInteger("AnimationOrder", 0);
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

                animator.SetInteger("AnimationOrder", 1);
                foxMovementAllowed = true;

                //Юнит достигает точки назначения
                while (!(controller.collisions.Below))
                {
                    if (fromWaypointIndex == 1)
                    {
                        animator.SetInteger("AnimationOrder", 2);
                    }
                    yield return null;
                }
                foxMovementAllowed = false;
                animator.SetInteger("AnimationOrder", 3);

                //Меняем точки в зависимости от расположения игрока
                if ((transform.position.x - player.transform.position.x) >= 0)
                {
                    localWayponts[2].x = localWayponts[0].x - Mathf.Abs(localWayponts[2].x);
                    localWayponts[1].x = localWayponts[0].x - Mathf.Abs(localWayponts[1].x);
                    UpdateGlobalWaypoints();
                    fromWaypointIndex = 0;
                    toWaypointIndex = 1;

                    yield return new WaitForSeconds(1f);
                    animator.SetInteger("AnimationOrder", 4);

                    if (isFacingRight)
                    {
                        Flip();
                    }
                    isFacingRight = false;

                    yield return new WaitForSeconds(0.3f);
                }
                else
                {
                    localWayponts[2].x = localWayponts[0].x + Mathf.Abs(localWayponts[2].x);
                    localWayponts[1].x = localWayponts[0].x + Mathf.Abs(localWayponts[1].x);
                    UpdateGlobalWaypoints();
                    fromWaypointIndex = 0;
                    toWaypointIndex = 1;

                    yield return new WaitForSeconds(1f);
                    animator.SetInteger("AnimationOrder", 4);

                    if (!isFacingRight)
                    {
                        Flip();
                    }
                    isFacingRight = true;

                    yield return new WaitForSeconds(0.3f);
                }

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


                animator.SetInteger("AnimationOrder", 4);
            }
            yield return null;
        }
    }
}
