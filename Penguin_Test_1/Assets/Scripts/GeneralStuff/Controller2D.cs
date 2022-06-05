using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : RaycastController
{
    public struct CollisionInfo
    {
        public bool Above, Below;
        public bool Left, Right;
        public bool ClimbingSlope;
        public float SlopeAngle, SlopeAngleOld;

        public bool DescendingSlope;
        public Vector2 VelocityOld;
        public int FaceDir;

        public void Reset()
        {
            Above = Below = false;
            Left = Right = false;
            ClimbingSlope = false;
            DescendingSlope = false;
            SlopeAngleOld = SlopeAngle;
            SlopeAngle = 0;
        }
    }

    public CollisionInfo collisions;
    private bool wasGrounded = true;
    public UnityEvent OnLandEvent;

    private float maxClimbAngle = 80;
    private float maxDescendAngle = 75;

    protected override void Start()
    {
        base.Start();
        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        collisions.FaceDir = 1;
    }

    public void Move(Vector2 velocity, bool standingOnPlatform = false)
    {
        UpdateRaycastOrigins();
        collisions.Reset();
        collisions.VelocityOld = velocity;

        if (velocity.x != 0)
        {
            collisions.FaceDir = (int)Mathf.Sign(velocity.x);
        }

        if (velocity.y < 0)
        {
            DescendSlope(ref velocity);
        }

        HorizontalCollisions(ref velocity);


        if (velocity.y != 0)
        {
            VerticalCollisions(ref velocity);
        }

        if (wasGrounded == false && collisions.Below)
        {
            OnLandEvent.Invoke();
        }

        transform.Translate(velocity);

        if (standingOnPlatform == true)
        {
            collisions.Below = true;
        }

        wasGrounded = collisions.Below;
    }



    void HorizontalCollisions(ref Vector2 velocity)
    {
        float directionX = collisions.FaceDir;
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;


        if (Mathf.Abs(velocity.x) < skinWidth)
        {
            rayLength = 2 * skinWidth;
        }
        //Создаем лучи
        //Вычисляем откуда начинают идти лучи, если идем влево, то отрисовываем слева, если направо, то отрисовываем справа
        //Если луч врезается в маску, то приравниваем velocity к расстоянию от rayOrigin до места столкновения

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (hit)
            {

                //Если необходимо сделать так, чтобы платформа проходила сквозь тебя сверху, смотри здесь

                float SlopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                //Если нижний луч
                if (i == 0 && SlopeAngle <= maxClimbAngle)
                {
                    if (collisions.DescendingSlope)
                    {
                        collisions.DescendingSlope = false;
                        velocity = collisions.VelocityOld;
                    }
                    float distanceToSlopeStart = 0;
                    if (SlopeAngle != collisions.SlopeAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        velocity.x -= distanceToSlopeStart * directionX;

                    }
                    ClimbSlope(ref velocity, SlopeAngle);
                    velocity.x += distanceToSlopeStart * directionX;
                }


                if (!collisions.ClimbingSlope || SlopeAngle > maxClimbAngle)
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance;

                    //Для горизонтальных столкновений на наклонных поверхностях

                    if (collisions.ClimbingSlope)
                    {
                        velocity.y = Mathf.Tan(collisions.SlopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                    }

                    //Если идем налево и попали лучем, то True
                    collisions.Left = directionX == -1;
                    collisions.Right = directionX == 1;
                }
            }

        }
    }

    void VerticalCollisions(ref Vector2 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (hit)
            {
                velocity.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                if (collisions.ClimbingSlope)
                {
                    velocity.x = velocity.y / Mathf.Tan(collisions.SlopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }

                collisions.Above = directionY == 1;
                collisions.Below = directionY == -1;
            }
        }
        if (collisions.ClimbingSlope)
        {
            float directionX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + skinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) * Vector2.up * velocity.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit)
            {
                float SlopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (SlopeAngle != collisions.SlopeAngle)
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    collisions.SlopeAngle = SlopeAngle;
                }
            }
        }
    }

    void ClimbSlope(ref Vector2 velocity, float SlopeAngle)
    {
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sin(SlopeAngle * Mathf.Deg2Rad) * moveDistance;

        //Т.к. сначала высчитываются горизонтальные столкновения, то чтобы не обнулять velocity.y (не получится прыгать), введем climbVelocityY

        if (velocity.y <= climbVelocityY)
        {
            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos(SlopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            collisions.Below = true;
            collisions.ClimbingSlope = true;
            collisions.SlopeAngle = SlopeAngle;
        }

    }

    void DescendSlope(ref Vector2 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

        if (!hit)
        {
            return;
        }

        float SlopeAngle = Vector2.Angle(hit.normal, Vector2.up);
        if (SlopeAngle == 0 || SlopeAngle > maxDescendAngle)
        {
            return;
        }

        if (hit.distance - skinWidth <= Mathf.Tan(SlopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
        {
            float moveDistance = Mathf.Abs(velocity.x);
            float descendVelocityY = Mathf.Sin(SlopeAngle * Mathf.Deg2Rad) * moveDistance;
            velocity.x = Mathf.Cos(SlopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            velocity.y -= descendVelocityY;

            collisions.SlopeAngle = SlopeAngle;
            collisions.DescendingSlope = true;
            collisions.Below = true;
        }
    }

    private void OnDrawGizmos()
    {
        Vector2 rayOrigin = raycastOrigins.bottomRight;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(rayOrigin, hit.point);

        Gizmos.DrawLine(hit.point, hit.point + hit.normal);
    }
}