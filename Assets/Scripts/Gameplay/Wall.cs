using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Wall : MonoBehaviour
{
    [SerializeField] private bool shouldMoveRandomlyUp = true;

    private void OnEnable()
    {
        if (shouldMoveRandomlyUp)
        {
            int r = Random.Range(0, 5);
            transform.position = new Vector3(transform.position.x, transform.position.y + r, transform.position.z);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }
}
