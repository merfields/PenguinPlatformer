using UnityEngine;

public class WaypointController : RaycastController
{
    public Vector3[] localWayponts;
    protected Vector3[] globalWaypoints;

    public float speed;
    public bool cyclic;
    public float waitTime;
    [Range(0,2)]
    public float easeAmount;

    protected int fromWaypointIndex = 0;
    protected int toWaypointIndex = 1;
    
    protected float percentBetweenWaypoints;
    protected float nextMoveTime;


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        globalWaypoints = new Vector3[localWayponts.Length];
        UpdateGlobalWaypoints();
    }

    protected void UpdateGlobalWaypoints()
    {
        for (int i = 0; i < localWayponts.Length; i++)
        {
            globalWaypoints[i] = localWayponts[i] + transform.position;
        }
    }

    // Update is called once per frame
    protected virtual void Update(){}

    protected float Ease(float x)
    {
        float a = easeAmount+1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    protected virtual Vector3 CalculateObjectMovement()
    {
        if(Time.time < nextMoveTime)
        {
            return Vector3.zero;
        }

        fromWaypointIndex %= globalWaypoints.Length;
        toWaypointIndex = (fromWaypointIndex + 1)%globalWaypoints.Length;
        float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
        
        percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;


        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
        float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);


        Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

        if (percentBetweenWaypoints >= 1)
        {
            percentBetweenWaypoints = 0;
            fromWaypointIndex++;

            if (!cyclic)
            {
                if (fromWaypointIndex >= globalWaypoints.Length - 1)
                {
                    fromWaypointIndex = 0;
                    System.Array.Reverse(globalWaypoints);
                }
            }
            nextMoveTime = Time.time + waitTime;
        }

        return newPos - transform.position;
    }

   

    protected void OnDrawGizmos()
    {
        if (localWayponts != null)
        {
            Gizmos.color = Color.red;
            float size = .3f;

            for (int i = 0; i < localWayponts.Length; i++)
            {
                Vector3 globalWaypointPos = (Application.isPlaying) ? globalWaypoints[i]: localWayponts[i] + transform.position;
                Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
            }
        }
    }

}
