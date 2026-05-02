using System.Collections;
using UnityEngine;

public class LavaController : MonoBehaviour
{
    [SerializeField] private float moveSpeed        = 1f;
    [SerializeField] private float accelerationRate = 0.05f; // units/s² added over time
    [SerializeField] private float maxSpeed         = 8f;
    [SerializeField] private float initialWaitTime  = 5f;

    private bool canMove = false;

    private void OnEnable()
    {
        canMove = false;
        StartCoroutine(InitialWait());
    }

    private void FixedUpdate()
    {
        if (!canMove) return;

        // Gradually increase speed up to the cap
        moveSpeed  = Mathf.Min(moveSpeed + accelerationRate * Time.fixedDeltaTime, maxSpeed);

        transform.Translate(Vector3.up * moveSpeed * Time.fixedDeltaTime);
    }

    private IEnumerator InitialWait()
    {
        yield return new WaitForSeconds(initialWaitTime);
        canMove = true;
    }
}