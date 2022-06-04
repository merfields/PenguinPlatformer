using UnityEngine;

public class Parallax : MonoBehaviour
{

    [SerializeField] Camera camera;
    [SerializeField] Transform player;

    private Vector2 startPos;
    private float startZ;

    Vector2 travel => (Vector2)camera.transform.position - startPos;

    private float distanceFromPlayer;
    private float clippingPlaneZ;
    [SerializeField] float parallax;

    //Эффект передвижения фонов вместе с игроком, с разной скоростью, создающий иллюзию глубины картинки

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
