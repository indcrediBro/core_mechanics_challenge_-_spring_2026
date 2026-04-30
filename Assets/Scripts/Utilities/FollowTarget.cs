using System;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [SerializeField] private bool unparentOnAwake = true;
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;

    private void Awake()
    {
        if (unparentOnAwake)
            transform.parent = null;

        MoveToPosition();
    }

    private void Update()
    {
        MoveToPosition();
    }

    private void MoveToPosition()
    {
        transform.position = target.position + offset;
    }
}
