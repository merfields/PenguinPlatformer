using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class FallingSpike : WaypointController
{
    Controller2D controller;
    [SerializeField] Vector2 fallVelocity;

    [SerializeField] Vector2 returnVelocity;

    [SerializeField] float waitTimeAfterFall;

    [SerializeField]
    float waitTimeBeforeFall;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        controller = GetComponent<Controller2D>();
        StartCoroutine(FallCouroutine());
    }

    IEnumerator FallCouroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTimeBeforeFall);

            while (Vector2.Distance(transform.position, globalWaypoints[1]) > 0.5f)
            {
                controller.Move(fallVelocity * Time.deltaTime);
                yield return null;
            }

            yield return new WaitForSeconds(waitTimeAfterFall);

            while (Vector2.Distance(transform.position, globalWaypoints[0]) > 0.5f)
            {
                controller.Move(returnVelocity * Time.deltaTime);
                yield return null;
            }

            yield return null;
        }
    }

    void OnBecameInvisible()
    {
        enabled = false;
    }
}
