using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileEnemy : Enemy
{
    public Player player;
    [SerializeField] private Rigidbody2D projectile;

    Vector2 enemyVelocity;
    private Rigidbody2D bullet;

    [SerializeField]
    float projectileSpeed;

    float bulletTimer;

    [SerializeField]
    float timeToShoot;

    [SerializeField]
    float distanceToPlayerToShoot;

    bool shootingCoroutineStarted = false;
    bool knockbackCouroutineStarted = false;


    protected override void Start()
    {
        base.Start();
        animator.SetInteger("MonkeyAnimOrder", 1);
    }

    // Update is called once per frame
    protected override void Update()
    {
        UpdateRaycastOrigins();
        bulletTimer -= Time.deltaTime;

        EnemyMovement();

        LookForPlayer();
    }


    protected override void EnemyMovement()
    {

        if (controller.collisions.Below && !knockbackCouroutineStarted)
        {
            enemyVelocity.x = 0;
            enemyVelocity.y = 0;
        }

        enemyVelocity.y += enemyGravity * Time.deltaTime;
        controller.Move(enemyVelocity * Time.deltaTime);

        if (player.transform.position.x < transform.position.x && isFacingRight)
        {
            Flip();

        }
        else if (!isFacingRight && player.transform.position.x >= transform.position.x)
        {
            Flip();
        }
    }

    private void LookForPlayer()
    {
        if (Vector3.Distance(player.transform.position, transform.position) < distanceToPlayerToShoot)
        {
            //Debug.Log("Distance");
            if (bulletTimer <= 0 && !shootingCoroutineStarted) {
                //Debug.Log("Timer ready");
                StartCoroutine("ShootProjectile");
            }
        }
    }


    IEnumerator ShootProjectile()
    {
        shootingCoroutineStarted = true;

        animator.SetInteger("MonkeyAnimOrder", 2);

        yield return new WaitForSeconds(1f);

        animator.SetInteger("MonkeyAnimOrder", 3);

        bullet = Instantiate(projectile, new Vector3(transform.position.x + 1 * Mathf.Sign(transform.localScale.x) * -1, transform.position.y + 0.6f, 0), transform.rotation);
        bullet.velocity = new Vector3(projectileSpeed * Mathf.Sign(transform.localScale.x) * -1, 0);

        bulletTimer = timeToShoot;

        yield return new WaitForSeconds(1f);

        animator.SetInteger("MonkeyAnimOrder", 1);

        shootingCoroutineStarted = false;
        //yield return null;
    }


    protected override IEnumerator EnemyKnockback()
    {
        knockbackCouroutineStarted = true;

        enemyVelocity.y = 16;
        enemyVelocity.x = 4 * Mathf.Sign(transform.localScale.x);

        yield return new WaitForSeconds(0.4f);

        knockbackCouroutineStarted = false;
        
    }
}
