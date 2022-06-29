using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : RaycastController
{
    public struct CollisionInfo
    {
        public bool Above, Below;
        public bool Left, Right;
        public int FaceDir;

        public void Reset()
        {
            Above = Below = false;
            Left = Right = false;
        }
    }

    public CollisionInfo collisions;
    private bool wasGroundedLastFrame = true;
    public UnityEvent OnLandEvent;

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

        if (velocity.x != 0)
        {
            collisions.FaceDir = (int)Mathf.Sign(velocity.x);
        }

        HorizontalCollisions(ref velocity);

        if (velocity.y != 0)
        {
            VerticalCollisions(ref velocity);
        }

        if (wasGroundedLastFrame == false && collisions.Below)
        {
            OnLandEvent.Invoke();
        }

        transform.Translate(velocity);

        if (standingOnPlatform == true)
        {
            collisions.Below = true;
        }

        wasGroundedLastFrame = collisions.Below;
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
                velocity.x = (hit.distance - skinWidth) * directionX;
                rayLength = hit.distance;
                //Если идем налево и попали лучем, то True
                collisions.Left = directionX == -1;
                collisions.Right = directionX == 1;
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

                collisions.Above = directionY == 1;
                collisions.Below = directionY == -1;
            }
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