using System;
using System.Collections;
using UnityEngine;

public class LavaController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float initialWaitTime = 5f;
    bool canMove = false;

    private void OnEnable()
    {
        canMove = false;
        StartCoroutine(InitialWait());
    }

    private void FixedUpdate()
    {
        if(!canMove) return;

        transform.Translate(Vector3.up * moveSpeed * Time.fixedDeltaTime);
    }

    private IEnumerator InitialWait()
    {
        yield return new WaitForSeconds(initialWaitTime);
        canMove = true;
    }
}
