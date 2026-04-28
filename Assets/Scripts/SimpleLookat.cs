using UnityEngine;

public class SimpleLookat : MonoBehaviour
{
    [SerializeField] private bool autoFindPlayer = true;
    [SerializeField] private Transform target;

    private void LateUpdate()
    {


        if (target == null )
        {
            if (autoFindPlayer)
            {
                target= GameObject.FindGameObjectWithTag("Player").transform;
            }
            if(target == null) return;
        }

        Vector3 direction = target.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.LookRotation(direction);
    }
}
