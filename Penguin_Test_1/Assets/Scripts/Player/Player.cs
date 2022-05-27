using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    #region  Components references

    [Header("Components references")]
    Controller2D controller;
    [SerializeField] private AudioManager audioManager;
    Animator animator;
    CheckpointManager cm;
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
    Timer coyoteTime;
    Timer stickToWallTime;
    Timer playerKnockbackTimer;
    Timer attackTimer;
    float defaultKnockbackTime = 0.4f;
    Timer InvincibilityCounter;
    [SerializeField]
    float invincibilityTime;
    #endregion

    #region Movement

    [Header("Movement")]
    [SerializeField] private Vector2 MoveInput;
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

    int wallDirXMemory;
    int wallDirX;
    bool wallSliding;
    #endregion

    #region Sliding
    [Header("Sliding")]
    bool isFacingRightDuringSlide;
    bool isSliding = false;
    bool cooldownAnimationPlayed = false;
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
    
    // Start is called before the first frame update
    void Start()
    {
        cm = GameObject.Find("CheckpointManager" + SceneManager.GetActiveScene().buildIndex).GetComponent<CheckpointManager>();
        transform.position = cm.LastCheckpointPosition;

        controller = GetComponent<Controller2D>();
        animator = GetComponent<Animator>();
        slideCounter = gameObject.AddComponent<Timer>();
        jumpBuffer = gameObject.AddComponent<Timer>();
        coyoteTime = gameObject.AddComponent<Timer>();
        stickToWallTime = gameObject.AddComponent<Timer>();
        InvincibilityCounter = gameObject.AddComponent<Timer>();
        playerKnockbackTimer = gameObject.AddComponent<Timer>();
        attackTimer = gameObject.AddComponent<Timer>();
        slideCooldown = gameObject.AddComponent<Timer>();

        //По формуле выводим гравитацию и велосити от высоты прыжка и времени до высшей точки

        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity * timeToJumpApex);
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);

        //См. выше, но для слайда

        slidingGravity = -(2 * slidingJumpHeight) / Mathf.Pow(slidingTimeToJumpApex, 2);
        slidingJumpVelocity = Mathf.Abs(slidingGravity * slidingTimeToJumpApex);
        minSlidingJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(slidingGravity) * minJumpHeight);
    }

    // Update is called once per frame
    void Update()
    {
        DoChecks();

        if (isAbleToAct)
        {
            if (attackTimer.TimeLeft <= 0)
            {
                if (Input.GetButtonDown("Fire2"))
                {
                    isAttacking = true;
                    StartCoroutine(Attack());

                }
            }

            //Бег

            MoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            wallDirX = (controller.collisions.Left == true) ? -1 : 1;

            wallSliding = false;

            //Sliding

            Slide();

            //WallSliding

            if ((controller.collisions.Left || controller.collisions.Right) && !controller.collisions.Below && velocity.y < 0)
            {
                wallSliding = true;

                isSliding = false;

                if (velocity.y < -wallSlideSpeedMax)
                {
                    velocity.y = -wallSlideSpeedMax;
                }

                stickToWallTime.TimeLeft = 0.1f;
                wallDirXMemory = wallDirX;

            }
            animator.SetBool("isWallSliding", wallSliding);

            if (stickToWallTime.TimeLeft == 0)
            {
                wallDirXMemory = wallDirX;
            }

            //CoyoteTime

            if (controller.collisions.Above || controller.collisions.Below)
            {
                velocity.y = 0;
                if (controller.collisions.Below)
                {
                    coyoteTime.TimeLeft = 0.1f;
                }
            }

            //В зависимости от значения isSliding находим velocity

            float targetVelocityX = MoveInput.x * prevTargetSpeed;

            if (isSliding == true && controller.collisions.Below)
            {
                targetVelocityX = MoveInput.x * slidingMoveSpeed;
                prevTargetSpeed = slidingMoveSpeed;
            }
            else if (controller.collisions.Below || wallSliding)
            {
                targetVelocityX = MoveInput.x * moveSpeed;
                prevTargetSpeed = moveSpeed;
            }

            if (controller.collisions.Below || wallSliding)
            {
                animator.SetBool("IsSliding", isSliding);
            }

            //Jumping

            Jump();

            //Horizontal smoothing, в полете и на земле
            if (controller.collisions.Below)
            {
                if (isSliding)
                {
                    accelerationTimeChoice = accelerationTimeSliding;
                }
                else
                {
                    accelerationTimeChoice = accelerationTimeGrounded;
                }
            }
            else
            {
                accelerationTimeChoice = accelerationTimeAirborn;
            }

            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, accelerationTimeChoice);

            //Поворот модели

            if (!(isAttacking && !controller.collisions.Below) && (playerKnockbackTimer.TimeLeft <= 0))
            {
                if (velocity.x < -0.5 && isFacingRight)
                    Flip();
                else if (velocity.x > 0.5 && !isFacingRight)
                    Flip();
            }
        }

        //Падение

        if (velocity.y < 0 && !controller.collisions.Below)
        {
            animator.SetBool("isFalling", true);
        }
        else
        {
            animator.SetBool("isFalling", false);
        }

        //Гравитация

        if (!isSliding)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else velocity.y += slidingGravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        animator.SetFloat("speed", Mathf.Abs(velocity.x));
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

        if (!(playerKnockbackTimer.TimeLeft > 0 || (isAttacking && controller.collisions.Below)))
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


    private void Slide()
    {
        if (Input.GetButtonDown("Fire1") && !isSliding && slideCooldown.TimeLeft == 0)
        {
            slideCounter.TimeLeft = 0.5f;
            audioManager.PlayClip("Slide");

            isSliding = true;

            isFacingRightDuringSlide = isFacingRight;
        }

        if (isSliding && isFacingRightDuringSlide != isFacingRight)
        {
            if (cooldownAnimationPlayed == false && isSliding == true)
            {
                StartCoroutine(SlideCooldownAnimation());
            }

            isSliding = false;
            slideCounter.TimeLeft = 0;
        }

        if (slideCounter.TimeLeft == 0)
        {

            if (cooldownAnimationPlayed == false && isSliding == true)
            {
                StartCoroutine(SlideCooldownAnimation());
            }

            isSliding = false;
        }
    }

    IEnumerator SlideCooldownAnimation()
    {
        cooldownAnimationPlayed = true;
        slideCooldown.TimeLeft = 0.6f;

        while (slideCooldown.TimeLeft > 0)
        {
            yield return null;
        }

        SpriteRenderer spriteRenderer = this.GetComponent<SpriteRenderer>();

        spriteRenderer.color = new Color(0.3804919f, 1, 0.2311321f, 1);
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = new Color(1, 1, 1, 1);

        cooldownAnimationPlayed = false;
    }

    private void Jump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            jumpBuffer.TimeLeft = 0.2f;

            //Wall jumping

            if (wallSliding || stickToWallTime.TimeLeft > 0)
            {
                audioManager.PlayClip("Jump");
                if (wallSliding && wallDirX == MoveInput.x)
                {
                    velocity.x = -wallDirX * wallJumpClimb.x;
                    velocity.y = wallJumpClimb.y;
                }
                else if (wallSliding && MoveInput.x == 0)
                {
                    velocity.x = -wallDirX * wallJumpOff.x;
                    velocity.y = wallJumpOff.y;
                }
                else if (stickToWallTime.TimeLeft > 0)
                {
                    velocity.x = -wallDirXMemory * wallJumpLeap.x;
                    velocity.y = wallJumpLeap.y;

                    stickToWallTime.TimeLeft = 0;
                }
            }
        }

        //Jumping

        if (jumpBuffer.TimeLeft > 0 && coyoteTime.TimeLeft > 0)
        {
            audioManager.PlayClip("Jump");
            jumpBuffer.TimeLeft = 0;
            coyoteTime.TimeLeft = 0;
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
        if (Input.GetKeyUp(KeyCode.Space))
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
    }

    private IEnumerator Attack()
    {
        audioManager.PlayClip("Attack");

        if (controller.collisions.Below)
        {
            velocity.x = 0;
        }

        animator.SetTrigger("isAttacking");
        attackTimer.TimeLeft = 0.4f;

        yield return new WaitForSeconds(0.1f);

        if (controller.collisions.Below)
        {
            velocity.x = 0;
        }

        Collider2D[] attackedEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);

        foreach (Collider2D enemy in attackedEnemies)
        {
            if (enemy.gameObject.GetComponent<SpikeyThing>() != null)
            {
                if (enemy.gameObject.GetComponent<SpikeyThing>().hitBySlide == true)
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
                StartCoroutine("ChangeAlpha");
            }

        }
    }

    IEnumerator ChangeAlpha()
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
        if (controller.collisions.Below)
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
        //Перезагружаю сцену если коснулся препятствия
        if (collision.CompareTag("DeathHazard"))
        {
            StartCoroutine("PlayerDeath");
        }

        //Наношу дамаг если коснулся препятствия
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
