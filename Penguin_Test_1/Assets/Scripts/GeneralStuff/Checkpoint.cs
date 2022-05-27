using UnityEngine;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{

    private CheckpointManager cm;
    [SerializeField] AudioManager am;
    SpriteRenderer sr;
    [SerializeField] Sprite greenFlag;
    bool alreadyChecked = false;


    void Start()
    {
        cm = GameObject.Find("CheckpointManager" + SceneManager.GetActiveScene().buildIndex).GetComponent<CheckpointManager>();
        sr = gameObject.GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !alreadyChecked)
        {
            am.PlayClip("Checkpoint");
            cm.LastCheckpointPosition = transform.position;
            sr.sprite = greenFlag;
            alreadyChecked = true;
        }
    }
}
