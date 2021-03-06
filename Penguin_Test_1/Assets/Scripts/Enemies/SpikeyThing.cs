using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SpikeyThing : Enemy
{
    private Vector2 enemyVelocity;
    private float knockbackCount = 0;
    public bool HitBySlide { get; private set; } = false;
    private bool shellActivated = false;

    // Update is called once per frame
    protected override void Update()
    {
        UpdateRaycastOrigins();

        if (!shellActivated)
        {
            EnemyMovement();
        }
    }

    protected override void EnemyMovement()
    {

        if (knockbackCount <= 0)
        {
            if (controller.collisions.Below || controller.collisions.Above)
            {
                enemyVelocity.y = 0;
            }
        }

        if (knockbackCount <= 0 && !HitBySlide)
        {
            enemyVelocity = CalculateObjectMovement();
        }
        else
        {
            if (controller.collisions.Below)
            {
                enemyVelocity.x = 0;
            }
            knockbackCount -= Time.deltaTime;
        }
        enemyVelocity.y += enemyGravity * Time.deltaTime;
        controller.Move(enemyVelocity * Time.deltaTime);

        if (enemyVelocity.x < 0 && isFacingRight)
            Flip();
        else if (enemyVelocity.x > 0 && !isFacingRight)
            Flip();
    }

    public void SlideIntoSpikey()
    {
        if (HitBySlide == false)
        {
            HitBySlide = true;
            enemyVelocity.x = 0;
            enemyVelocity.y = 16;
            knockbackCount = 0.4f;


            Vector3 theScale = transform.localScale;
            theScale.y *= -1;
            transform.localScale = theScale;
        }
    }

    public IEnumerator ActivateShell()
    {
        shellActivated = true;
        animator.SetBool("shellActivated", true);

        yield return new WaitForSeconds(1f);

        animator.SetBool("shellActivated", false);
        shellActivated = false;
    }

}
