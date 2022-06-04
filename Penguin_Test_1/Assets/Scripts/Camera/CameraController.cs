using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] GameObject virtualCamera;

    /*There are several cameras on the level, each has its own trigger.
    When player enters a different trigger camera changes*/

    private void OnTriggerEnter2D(Collider2D enteringObject)
    {
        if (enteringObject.CompareTag("Player") && !enteringObject.isTrigger)
        {
            virtualCamera.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D enteringObject)
    {
        if (enteringObject.CompareTag("Player") && !enteringObject.isTrigger)
        {
            virtualCamera.SetActive(false);
        }
    }
}
