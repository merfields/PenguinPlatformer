using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class FallingSpike : WaypointController
{
    private Controller2D controller;
    [SerializeField] private Vector2 fallVelocity;
    [SerializeField] private Vector2 returnVelocity;
    [SerializeField] private float waitTimeAfterFall;
    [SerializeField] private float waitTimeBeforeFall;

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
