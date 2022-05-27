using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raven : Enemy
{

    Vector2 enemyVelocity;

    // Update is called once per frame
    protected override void Update()
    {
        UpdateRaycastOrigins();
        enemyVelocity = CalculateObjectMovement();
        controller.Move(enemyVelocity);


        if (enemyVelocity.x < 0 && isFacingRight)
            Flip();
        else if (enemyVelocity.x > 0 && !isFacingRight)
            Flip();
    }


}
