﻿using System.Collections;
using UnityEngine;

public class Knight : Enemy
{
    Vector2 enemyVelocity;
    float knockbackCount = 0;
    [HideInInspector]
    public bool hitBySlide = false;

    bool isAttacking = false;

    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float attackRadius;

    [SerializeField] Player player;
    [SerializeField] AudioManager audioManager;

    [SerializeField] float distanceToPlayer = 3f;
    [SerializeField] float distanceToPlayerToMove = 10f;

    bool knockbackCouroutineStarted = false;

    // Update is called once per frame
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

        if (!isAttacking && Vector2.Distance(transform.position, player.transform.position) <= distanceToPlayerToMove && !knockbackCouroutineStarted)
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
        if (Vector2.Distance(transform.position, player.transform.position) >= distanceToPlayer)
        {

            if (knockbackCount <= 0)
            {

                if (controller.collisions.Below || controller.collisions.Above)
                {
                    enemyVelocity.y = 0;
                }
            }

            if (knockbackCount <= 0 && !hitBySlide)
            {
                enemyVelocity = CalculateObjectMovement();
                enemyVelocity.y += enemyGravity * Time.deltaTime;
                controller.Move(enemyVelocity);
            }

            else
            {
                enemyVelocity.y += enemyGravity * Time.deltaTime;
                controller.Move(enemyVelocity * Time.deltaTime);
            }
        }
        else
        {
            if (knockbackCount <= 0)
                StartCoroutine(Attack());
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
}
