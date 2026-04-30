using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class CircleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private int spawnCount = 8;
    [SerializeField] private float radius = 5f;

    [Header("Options")]
    [SerializeField] private bool autoUpdate = true;
    [SerializeField] private bool lookAtCenter = true;

    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;

    private Transform container;

    private void OnEnable()
    {
        EnsureContainer();
        if (autoUpdate)
            Generate();
    }

    private void OnValidate()
    {
        spawnCount = Mathf.Max(1, spawnCount);
        radius = Mathf.Max(0f, radius);

        if (autoUpdate && !Application.isPlaying)
        {
            EnsureContainer();
            Generate();
        }
    }

    private void EnsureContainer()
    {
        if (container == null)
        {
            Transform existing = transform.Find("Generated");
            if (existing != null)
                container = existing;
            else
            {
                GameObject go = new GameObject("Generated");
                go.transform.SetParent(transform);
                go.transform.localPosition = Vector3.zero;
                container = go.transform;
            }
        }
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        if (prefab == null || container == null)
            return;

        int currentCount = container.childCount;

        // 1. REMOVE extra objects
        if (currentCount > spawnCount)
        {
            for (int i = currentCount - 1; i >= spawnCount; i--)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(container.GetChild(i).gameObject);
                else
                    Destroy(container.GetChild(i).gameObject);
#else
            Destroy(container.GetChild(i).gameObject);
#endif
            }
        }

        // 2. ADD missing objects
        if (currentCount < spawnCount)
        {
            for (int i = currentCount; i < spawnCount; i++)
            {
#if UNITY_EDITOR
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
#else
            GameObject instance = Instantiate(prefab);
#endif
                instance.transform.SetParent(container);
            }
        }

        // 3. POSITION everything correctly
        float angleStep = 360f / spawnCount;

        for (int i = 0; i < spawnCount; i++)
        {
            Transform child = container.GetChild(i);

            float angle = i * angleStep * Mathf.Deg2Rad;

            Vector3 position = new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );

            child.localPosition = position;

            if (lookAtCenter)
                child.LookAt(transform.position);
        }
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        if (container == null) return;

        for (int i = container.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(container.GetChild(i).gameObject);
            else
                Destroy(container.GetChild(i).gameObject);
#else
            Destroy(container.GetChild(i).gameObject);
#endif
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);

        float angleStep = 360f / Mathf.Max(1, spawnCount);

        for (int i = 0; i < spawnCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;

            Vector3 pos = transform.position + new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );

            Gizmos.DrawSphere(pos, 0.1f);
        }
    }
}