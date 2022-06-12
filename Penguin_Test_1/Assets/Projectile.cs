using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private LayerMask collisionLayers;
    private void OnEnable()
    {
        Debug.Log("Created projectile");
    }
    private void OnBecameInvisible()
    {
        GameObject.Destroy(gameObject);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("WhatIsGround"))
            GameObject.Destroy(gameObject);
    }
}
