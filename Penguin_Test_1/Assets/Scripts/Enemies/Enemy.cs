using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Controller2D))]
public class Enemy : WaypointController
{

    protected Animator animator;
    protected Controller2D controller;
    [SerializeField]
    protected bool isFacingRight;
    [SerializeField]
    protected float enemyGravity = -9.81f;

    [SerializeField]
    protected float enemyHP = 1;


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        controller = GetComponent<Controller2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    protected override void Update()
    {
    }

    protected void Flip()
    {
        isFacingRight = !isFacingRight;

        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public virtual void TakeDamage(float damage) {
        enemyHP -= damage;
        Debug.Log(enemyHP);

        StartCoroutine("EnemyKnockback");

        if (enemyHP <= 0)
        {
            StartCoroutine(Die());
        }
    }

    protected virtual IEnumerator Die()
    {
        animator.SetTrigger("isDead");
        yield return new WaitForSeconds(0.2f);
        Destroy(gameObject);
        yield return null;
    }

    protected virtual void EnemyMovement()
    {

    }

    protected virtual IEnumerator EnemyKnockback()
    {
        yield return null;
    }
}
