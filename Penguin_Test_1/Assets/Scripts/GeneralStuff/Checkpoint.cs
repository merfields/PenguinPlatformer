using UnityEngine;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{

    private CheckpointManager cm;
    [SerializeField] private AudioManager am;
    private SpriteRenderer sr;
    [SerializeField] private Sprite greenFlag;
    private bool alreadyChecked = false;


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
