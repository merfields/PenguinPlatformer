using UnityEngine;

public class Parallax : MonoBehaviour
{

    [SerializeField] private Camera camera;
    [SerializeField] private Transform player;

    private Vector2 startPos;
    private float startZ;

    private Vector2 travel => (Vector2)camera.transform.position - startPos;

    private float distanceFromPlayer;
    private float clippingPlaneZ;
    [SerializeField] private float parallax;

    void Start()
    {
        startPos = transform.position;
        startZ = transform.position.z;
    }

    void Update()
    {
        distanceFromPlayer = transform.position.z - player.position.z;
        clippingPlaneZ = (camera.transform.position.z + (distanceFromPlayer > 0 ? camera.farClipPlane : camera.nearClipPlane));

        Vector2 newPos = startPos + travel * parallax;
        transform.position = new Vector3(newPos.x, newPos.y, startZ);
    }
}
