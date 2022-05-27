using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParabolaMovement : MovingPlatform
{
    public AnimationCurve curve;

    Vector2 end;
    Vector2 start;
    float time;

    bool arrived = true;

    [SerializeField]
    ParabolaMovement slowest;

    protected override void Start()
    {
        base.Start();
        StartCoroutine(ParabolaLoop());
    }


    protected override void Update()
    {
        UpdateRaycastOrigins();
    }

    IEnumerator ParabolaLoop()
    {
        while (true)
        {
            transform.position = globalWaypoints[0];
            start = transform.position;
            end = globalWaypoints[1];
            time = 0;

            yield return new WaitForSeconds(waitTime);

            arrived = false;
            while (Vector2.Distance(transform.position, globalWaypoints[1]) > 0.1f)
            {
                Swing();
                yield return null;
            }
            arrived = true;
            
            while (slowest.arrived != true)
            {
                yield return null;
            }


            time = 0;
            transform.position = globalWaypoints[1];
            start = transform.position;
            end = globalWaypoints[0];

            yield return new WaitForSeconds(waitTime);

            arrived = false;
            while (Vector2.Distance(transform.position, globalWaypoints[0]) > 0.1f)
            {
                Swing();
                yield return null;
            }
            arrived = true;

            while (slowest.arrived != true)
            {
                yield return null;
            }


            yield return null;
        }
    }

    private void Swing()
    {
        UpdateRaycastOrigins();
        time += speed * Time.deltaTime;
        float easedTime = Ease(time);

        Vector3 pos = Vector3.Lerp(start, end, easedTime);
        pos.y += curve.Evaluate(easedTime);

        CalculatePassengerMovement(pos - transform.position);
        MovePassengers(true);
        transform.Translate(pos - transform.position);
        MovePassengers(false);
    }
}
