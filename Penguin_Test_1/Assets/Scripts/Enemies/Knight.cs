using System.Collections;
using UnityEngine;

public class Knight : Enemy
{
    private Vector2 enemyVelocity;
    private float knockbackCount = 0;
    private bool hitBySlide = false;

    private bool isAttacking = false;

    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float attackRadius;

    [SerializeField] private Player player;
    [SerializeField] private AudioManager audioManager;

    [SerializeField] private float distanceToPlayerToAttack = 3f;
    private float distanceToPlayer;

    private bool knockbackCouroutineStarted = false;

    protected override void Update()
    {
        knockbackCount -= Time.deltaTime;

        if (knockbackCouroutineStarted == false)
        {
            enemyVelocity.x = 0;
        }
        UpdateRaycastOrigins();

        if (knockbackCount <= 0)
        {
            animator.SetBool("isStunned", false);
        }

        distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        if (!isAttacking && !knockbackCouroutineStarted)
        {
            EnemyMovement();
        }
        else
        {
            enemyVelocity.y += enemyGravity * Time.deltaTime;
            controller.Move(enemyVelocity * Time.deltaTime);
        }
    }

    protected override void EnemyMovement()
    {
        if (distanceToPlayer >= distanceToPlayerToAttack)
        {
            if (knockbackCount <= 0)
            {
                CheckForFall();
                CheckForWalls();

                if (controller.collisions.Below || controller.collisions.Above)
                {
                    enemyVelocity.y = 0;
                }

                if (!hitBySlide)
                {
                    enemyVelocity = CalculateObjectMovement();
                }
            }

            if (hitBySlide)
            {
                enemyVelocity.y += enemyGravity * Time.deltaTime;
            }
            controller.Move(enemyVelocity);
        }
        else
        {
            if (knockbackCount <= 0)
            {
                StartCoroutine(Attack());
            }
        }

        if (knockbackCount <= 0)
        {
            hitBySlide = false;
            if ((transform.position.x > player.transform.position.x) && isFacingRight)
                Flip();
            else if ((transform.position.x < player.transform.position.x) && !isFacingRight)
                Flip();
        }
    }

    public void SlideIntoKnight()
    {
        if (hitBySlide == false)
        {
            animator.SetBool("isStunned", true);
            hitBySlide = true;
            enemyVelocity.x = 0;
            enemyVelocity.y = 16;
            knockbackCount = 2f;
        }
    }

    private IEnumerator Attack()
    {
        isAttacking = true;
        animator.SetTrigger("isAttacking");

        yield return new WaitForSeconds(0.4f);

        audioManager.PlayClip("WolfAttack");

        Collider2D[] attackedPlayer = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);

        foreach (Collider2D enemy in attackedPlayer)
        {
            if (enemy.CompareTag("Player"))
            {
                player.PlayerTakeDamage();
            }
        }

        yield return new WaitForSeconds(1.5f);
        isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) { return; }

        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }

    protected override IEnumerator EnemyKnockback()
    {
        Vector3 positionBeforeKnockback = transform.position;
        knockbackCouroutineStarted = true;

        enemyVelocity.y = 16;
        enemyVelocity.x = 4 * Mathf.Sign(transform.position.x - player.transform.position.x);

        yield return new WaitForSeconds(0.4f);
        while (!controller.collisions.Below)
        {
            yield return null;
        }
        globalWaypoints[0] += transform.position - positionBeforeKnockback;
        globalWaypoints[1] += transform.position - positionBeforeKnockback;

        knockbackCouroutineStarted = false;
    }

    private void CheckForFall()
    {
        Vector2 rayOriginRight = raycastOrigins.bottomRight;
        Vector2 rayOriginLeft = raycastOrigins.bottomLeft;
        RaycastHit2D hitRight = Physics2D.Raycast(rayOriginRight, -Vector2.up, 1, collisionMask);
        RaycastHit2D hitleft = Physics2D.Raycast(rayOriginLeft, -Vector2.up, 1, collisionMask);

        Debug.DrawRay(rayOriginLeft, -Vector2.up * 1, Color.green);
        Debug.DrawRay(rayOriginRight, -Vector2.up * 1, Color.green);

        if (!hitRight || !hitleft)
        {
            ChangeToReachableWaypoints();
        }
    }

    private void ChangeToReachableWaypoints()
    {
        globalWaypoints[0] -= globalWaypoints[1] - transform.position;
        globalWaypoints[1] = transform.position;
        NoMoreWaypoints();
    }

    private void CheckForWalls()
    {
        if (controller.collisions.Right || controller.collisions.Left)
        {
            ChangeToReachableWaypoints();
        }
    }
}