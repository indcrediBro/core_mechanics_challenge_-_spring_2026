using System;
using UnityEngine;
using UnityEngine.Events;

public class WallBreakingSFXPlayer : MonoBehaviour
{
    public UnityEvent onHitWall;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Platform"))
        {
            onHitWall.Invoke();
        }
    }
}
