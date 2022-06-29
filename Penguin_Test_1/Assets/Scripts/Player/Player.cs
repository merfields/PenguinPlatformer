using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//To Do: Change to state machine?

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    #region  Components references

    [Header("Components references")]
    Controller2D controller;
    [SerializeField] private AudioManager audioManager;
    Animator animator;
    CheckpointManager cm;
    PlayerInput playerInput;
    #endregion

    #region Attacking

    [Header("Attacking")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float attackRadius;
    bool isAttacking = false;
    #endregion

    #region Timers

    [Header("Timers")]
    Timer slideCounter;
    Timer slideCooldown;
    Timer jumpBuffer;
    Timer coyoteTimeTimer;
    Timer stickToWallTimer;
    Timer playerKnockbackTimer;
    Timer attackTimer;
    float defaultKnockbackTime = 0.4f;
    Timer InvincibilityCounter;
    [SerializeField]
    float invincibilityTime;
    #endregion

    #region Movement

    [Header("Movement")]
    [SerializeField] private Vector2 moveInput;
    [SerializeField] private float moveSpeed = 6;
    [SerializeField] private float slidingMoveSpeed = 40;
    float prevTargetSpeed = 0;
    #endregion

    #region Jumping

    [Header("Jumping")]
    [SerializeField] private float jumpHeight = 4;
    [SerializeField] private float minJumpHeight = 1;
    [SerializeField] private float timeToJumpApex = 0.4f;
    [SerializeField] private float slidingJumpHeight = 6;
    [SerializeField] private float slidingTimeToJumpApex = 0.6f;
    [SerializeField] private const float CoyoteTime = 0.1f;
    float gravity;
    float slidingGravity;

    float jumpVelocity;
    float minJumpVelocity;

    float minSlidingJumpVelocity;
    float slidingJumpVelocity;
    #endregion

    #region Acceleration
    [Header("Acceleration")]
    [SerializeField] private float accelerationTimeAirborn = .1f;
    [SerializeField] private float accelerationTimeGrounded = .05f;
    [SerializeField] private float accelerationTimeSliding;
    float accelerationTimeChoice;
    #endregion

    #region Wall sliding
    [Header("WallSliding")]
    [SerializeField] private float wallSlideSpeedMax = 3;
    [SerializeField] private Vector2 wallJumpClimb;
    [SerializeField] private Vector2 wallJumpOff;
    [SerializeField] private Vector2 wallJumpLeap;
    [SerializeField] private const float WallCoyoteTime = 0.15f;

    int wallDirXMemory;
    int wallDirX => (controller.collisions.Left == true) ? -1 : 1;
    bool wallSliding;
    #endregion

    #region Sliding
    [Header("Sliding")]
    //To check if the character changed direction during slide, if he did then we will stop the slide
    bool isFacingRightDuringSlide;
    bool isSliding = false;
    bool slideCooldownAnimationPlayed = false;
    #endregion

    #region Health System
    [Header("Health system")]
    [SerializeField]
    Image[] hearts;

    float PlayerHP { get; set; } = 3;

    [SerializeField]
    Sprite heartSprite;
    #endregion

    Vector3 velocity;
    float velocityXSmoothing;
    bool isFacingRight = true;
    bool isAbleToAct = false;
    bool isGrounded => controller.collisions.Below;

    void Start()
    {
        cm = GameObject.Find("CheckpointManager" + SceneManager.GetActiveScene().buildIndex).GetComponent<CheckpointManager>();
        transform.position = cm.LastCheckpointPosition;

        controller = GetComponent<Controller2D>();
        animator = gameObject.GetComponent<Animator>();

        slideCounter = gameObject.AddComponent<Timer>();
        jumpBuffer = gameObject.AddComponent<Timer>();
        coyoteTimeTimer = gameObject.AddComponent<Timer>();
        stickToWallTimer = gameObject.AddComponent<Timer>();
        InvincibilityCounter = gameObject.AddComponent<Timer>();
        playerKnockbackTimer = gameObject.AddComponent<Timer>();
        attackTimer = gameObject.AddComponent<Timer>();
        slideCooldown = gameObject.AddComponent<Timer>();
        playerInput = new PlayerInput();

        //Calculating gravitation, dependent on jump height and time to reach the highest jump point
        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity * timeToJumpApex);
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);

        //The same as above but for slide

        slidingGravity = -(2 * slidingJumpHeight) / Mathf.Pow(slidingTimeToJumpApex, 2);
        slidingJumpVelocity = Mathf.Abs(slidingGravity * slidingTimeToJumpApex);
        minSlidingJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(slidingGravity) * minJumpHeight);
    }

    void Update()
    {
        DoChecks();

        if (isAbleToAct)
        {
            //Attacking
            if (playerInput.CheckIfPressedAttack() && attackTimer.TimeLeft <= 0 && isGrounded)
            {
                isAttacking = true;
                velocity.x = 0;
                attackTimer.TimeLeft = 0.4f;

                audioManager.PlayClip("Attack");
                animator.SetTrigger("isAttacking");

                StartCoroutine(SwordAttack());
            }

            //Sliding

            if (playerInput.CheckIfPressedSlide() && !isSliding && slideCooldown.TimeLeft == 0 && isGrounded)
            {
                isSliding = true;
                slideCounter.TimeLeft = 0.5f;
                audioManager.PlayClip("Slide");
                isFacingRightDuringSlide = isFacingRight;
            }

            if (isSliding)
            {
                if (CheckIfShouldStopSliding() == true)
                {
                    isSliding = false;
                    slideCounter.TimeLeft = 0;

                    if (slideCooldownAnimationPlayed == false)
                    {
                        StartCoroutine(SlideCooldownAnimation());
                    }
                }
            }

            //WallSliding

            wallSliding = false;

            if ((controller.collisions.Left || controller.collisions.Right) && !isGrounded && velocity.y < 0)
            {
                wallSliding = true;
                isSliding = false;

                if (velocity.y < -wallSlideSpeedMax)
                {
                    velocity.y = -wallSlideSpeedMax;
                }

                stickToWallTimer.TimeLeft = WallCoyoteTime;
                wallDirXMemory = wallDirX;
            }

            if (stickToWallTimer.TimeLeft == 0)
            {
                wallDirXMemory = wallDirX;
            }

            //CoyoteTime

            if (controller.collisions.Above || isGrounded)
            {
                velocity.y = 0;
                if (isGrounded)
                {
                    coyoteTimeTimer.TimeLeft = CoyoteTime;
                }
            }

            //Jumping

            if (playerInput.CheckIfPressedJump())
            {
                jumpBuffer.TimeLeft = 0.2f;

                if (wallSliding || stickToWallTimer.TimeLeft > 0)
                {
                    WallJump();
                }
            }

            if (jumpBuffer.TimeLeft > 0 && coyoteTimeTimer.TimeLeft > 0)
            {
                RegularJump();
            }
            if (playerInput.CheckIfJumpCancelled())
            {
                JumpCancel();
            }

            //Movement

            //Horizontal smoothing, в полете и на земле

            accelerationTimeChoice = FindAccelerationTime();

            //Find velocity, different for sliding

            moveInput = playerInput.GetMovementInputVector();
            if (!isAttacking)
            {
                velocity.x = FindVeloctiyX();
            }

            //Поворот модели

            if (velocity.x < -0.5 && isFacingRight)
                Flip();
            else if (velocity.x > 0.5 && !isFacingRight)
                Flip();

            if (isGrounded || wallSliding)
            {
                animator.SetBool("IsSliding", isSliding);
            }

            animator.SetBool("isWallSliding", wallSliding);
        }

        //Falling animation

        if (velocity.y < 0 && !isGrounded)
        {
            animator.SetBool("isFalling", true);
        }
        else
        {
            animator.SetBool("isFalling", false);
        }

        ApplyGravity();

        controller.Move(velocity * Time.deltaTime);

        animator.SetFloat("speed", Mathf.Abs(velocity.x));
    }

    private void JumpCancel()
    {
        if (isSliding)
        {
            if (velocity.y > minSlidingJumpVelocity)
            {
                velocity.y = minSlidingJumpVelocity;
            }
        }
        else
        {
            if (velocity.y > minJumpVelocity)
            {
                velocity.y = minJumpVelocity;
            }
        }
    }

    private void RegularJump()
    {
        audioManager.PlayClip("Jump");
        jumpBuffer.TimeLeft = 0;
        coyoteTimeTimer.TimeLeft = 0;
        if (isSliding)
        {
            velocity.y = slidingJumpVelocity;
        }
        else
        {
            velocity.y = jumpVelocity;
        }
        animator.SetBool("isJumping", true);
    }

    private void WallJump()
    {
        audioManager.PlayClip("Jump");
        if (wallSliding && wallDirX == moveInput.x)
        {
            //Climb jump
            velocity.x = -wallDirX * wallJumpClimb.x;
            velocity.y = wallJumpClimb.y;
        }
        else if (wallSliding && moveInput.x == 0)
        {
            //Small jump off the wall
            velocity.x = -wallDirX * wallJumpOff.x;
            velocity.y = wallJumpOff.y;
        }
        else if (stickToWallTimer.TimeLeft > 0)
        {
            //Regular wall jump
            velocity.x = -wallDirXMemory * wallJumpLeap.x;
            velocity.y = wallJumpLeap.y;

            stickToWallTimer.TimeLeft = 0;
        }
    }

    private void ApplyGravity()
    {
        if (!isSliding)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else velocity.y += slidingGravity * Time.deltaTime;
    }

    private float FindAccelerationTime()
    {
        if (isGrounded)
        {
            if (isSliding)
            {
                return accelerationTimeSliding;
            }
            else
            {
                return accelerationTimeGrounded;
            }
        }
        else
        {
            return accelerationTimeAirborn;
        }
    }

    private bool CheckIfShouldStopSliding()
    {
        return (isFacingRightDuringSlide != isFacingRight || slideCounter.TimeLeft == 0);
    }

    private float FindVeloctiyX()
    {
        float targetVelocityX = FindTargetVelocityX();

        return Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, accelerationTimeChoice);
    }

    private float FindTargetVelocityX()
    {
        float targetVelocityX = moveInput.x * prevTargetSpeed;

        if (isSliding == true && isGrounded)
        {
            targetVelocityX = moveInput.x * slidingMoveSpeed;
            prevTargetSpeed = slidingMoveSpeed;
        }
        else if (isGrounded || wallSliding)
        {
            targetVelocityX = moveInput.x * moveSpeed;
            prevTargetSpeed = moveSpeed;
        }

        return targetVelocityX;
    }

    private void DoChecks()
    {
        if (attackTimer.TimeLeft <= 0)
        {
            isAttacking = false;
        }

        if (playerKnockbackTimer.TimeLeft <= 0)
        {
            animator.SetBool("isKnocked", false);
        }

        if (!(playerKnockbackTimer.TimeLeft > 0 || (isAttacking && isGrounded)))
        {
            isAbleToAct = true;
        }
        else
        {
            isAbleToAct = false;
        }
    }

    public void OnLanding()
    {
        audioManager.PlayClip("Land");
        animator.SetBool("isJumping", false);
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;

        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    IEnumerator SlideCooldownAnimation()
    {
        slideCooldownAnimationPlayed = true;
        slideCooldown.TimeLeft = 0.6f;

        while (slideCooldown.TimeLeft > 0)
        {
            yield return null;
        }

        SpriteRenderer spriteRenderer = this.GetComponent<SpriteRenderer>();

        spriteRenderer.color = new Color(0.3804919f, 1, 0.2311321f, 1);
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = new Color(1, 1, 1, 1);

        slideCooldownAnimationPlayed = false;
    }

    private IEnumerator SwordAttack()
    {
        yield return new WaitForSeconds(0.1f);

        Collider2D[] attackedEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);

        foreach (Collider2D enemy in attackedEnemies)
        {
            if (enemy.gameObject.GetComponent<SpikeyThing>() != null)
            {
                if (enemy.gameObject.GetComponent<SpikeyThing>().HitBySlide == true)
                {
                    enemy.GetComponent<Enemy>().TakeDamage(1);
                    audioManager.PlayClip("EnemyDamaged");
                }
                else
                {
                    this.Knockback(8, 0, 0.05f, false);
                    enemy.gameObject.GetComponent<SpikeyThing>().StartCoroutine("ActivateShell");
                }
            }
            else
            {
                audioManager.PlayClip("EnemyDamaged");
                enemy.GetComponent<Enemy>().TakeDamage(1);

            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) { return; }

        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }

    public void PlayerTakeDamage()
    {
        if (InvincibilityCounter.TimeLeft <= 0)
        {
            InvincibilityCounter.TimeLeft = invincibilityTime;
            audioManager.PlayClip("CharacterDamaged");
            PlayerHP -= 1;

            for (int i = 0; i < hearts.Length; i++)
            {
                if (i < PlayerHP)
                {
                    hearts[i].enabled = true;
                }
                else
                {
                    hearts[i].enabled = false;
                }
            }

            if (PlayerHP <= 0)
            {
                StartCoroutine("PlayerDeath");
            }
            else
            {
                Knockback(8, 16, defaultKnockbackTime, true);
                StartCoroutine(ChangeAlphaOnTakingDamage());
            }

        }
    }

    IEnumerator ChangeAlphaOnTakingDamage()
    {
        SpriteRenderer spriteRenderer = this.GetComponent<SpriteRenderer>();
        while (InvincibilityCounter.TimeLeft > 0)
        {
            spriteRenderer.color = new Color(1, 1, 1, 0.5f);
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = new Color(1, 1, 1, 1);
            yield return new WaitForSeconds(0.1f);
        }
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }

    private void Knockback(float xKnockback, float yKnockback, float knockBackDuration, bool hitKnockback)
    {
        if (isGrounded)
        {
            velocity.x = 0;
        }

        if (hitKnockback)
            animator.SetBool("isKnocked", true);

        playerKnockbackTimer.TimeLeft = knockBackDuration;
        if (isFacingRight)
        {
            velocity.y = yKnockback;
            velocity.x = -xKnockback;
        }
        else
        {
            velocity.y = yKnockback;
            velocity.x = xKnockback;
        }
    }

    IEnumerator PlayerDeath()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        animator.SetTrigger("isDead");
        audioManager.PlayClip("Death");
        audioManager.StopClip("LevelTheme");
        yield return new WaitForSeconds(0.2f);
        GetComponent<SpriteRenderer>().enabled = false;
        this.enabled = false;
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(currentScene.name);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        if (enemy != null && !(isSliding && (collision.gameObject.GetComponent<SpikeyThing>() != null || collision.gameObject.GetComponent<Knight>() != null)))
        {
            PlayerTakeDamage();
            InvincibilityCounter.TimeLeft = invincibilityTime;
        }

        if (collision.gameObject.GetComponent<SpikeyThing>() != null)
        {
            if (isSliding)
            {
                collision.gameObject.GetComponent<SpikeyThing>().SlideIntoSpikey();
            }
        }

        if (collision.gameObject.GetComponent<Knight>() != null)
        {
            if (isSliding)
            {
                collision.gameObject.GetComponent<Knight>().SlideIntoKnight();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Reload the scene if touched killing object(spikes etc)
        if (collision.CompareTag("DeathHazard"))
        {
            StartCoroutine("PlayerDeath");
        }

        if (collision.CompareTag("DamageHazard"))
        {
            PlayerTakeDamage();
        }

        if (collision.CompareTag("NextLevel"))
        {
            Destroy(cm);
            Destroy(gameObject);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
