using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] GameObject virtualCamera;


    /*На уровне установлено множество камер, каждая относится к определенному триггеру,
    при переходе из одного в другой меняется активная камера.*/
    

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
