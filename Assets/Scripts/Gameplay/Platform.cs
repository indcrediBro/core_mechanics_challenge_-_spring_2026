using IncredibleAttributes;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class Platform : MonoBehaviour
{
    [SerializeField, ShowAssetPreview(64, 64)] private Texture[] platformTextures;
    [SerializeField] private bool shouldMoveToRandomHeight;
    [SerializeField] private bool shouldDestroyRandomlyOnStart;
    [SerializeField] private GameObject enemyPrefab;

    [SerializeField] private UnityEvent onBounceTaken;

    private Material platformMaterial;
    private HealthSystem.PlatformHealth platformHealth;

    // Tracks how many times this platform has been hit (drives texture index)
    private int hitCount = 0;

    private void Awake()
    {
        platformMaterial = GetComponentInChildren<MeshRenderer>().material;
        platformHealth   = GetComponentInChildren<HealthSystem.PlatformHealth>();

        // Seed base texture
        if (platformTextures.Length > 0)
            platformMaterial.mainTexture = platformTextures[0];
    }

    private void OnEnable()
    {
        if (shouldMoveToRandomHeight)
        {
            float random = Random.Range(0f, 15f);
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y + random,
                transform.position.z);

            SpawnEnemyRandomly();
        }
    }

    private void Start()
    {
        if (shouldDestroyRandomlyOnStart && Random.Range(0, 5) == 0)
            Destroy(gameObject);
    }

    // ─────────────────────────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────────────────────────

    public void HandleBounce()
    {
        onBounceTaken?.Invoke();
        if (platformMaterial == null) return;

        hitCount++;

        // Update visuals BEFORE calling TakeDamage so Destroy() can't race the texture swap
        UpdateDamageVisual();

        // Delegate health/death to PlatformHealth
        platformHealth?.TakeDamage(GetDamagePerHit());
    }

    // ─────────────────────────────────────────────────────────────
    // Visuals
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Steps through platformTextures using the emission slot — same slot the
    /// original code used. Index 0 = clean (no extra emission), 1+ = damage states.
    /// </summary>
    private void UpdateDamageVisual()
    {
        if (platformTextures.Length <= 1) return;

        int textureIndex = Mathf.Clamp(hitCount, 0, platformTextures.Length - 1);
        platformMaterial.SetTexture("_EmissionMap", platformTextures[textureIndex]);

        // Keep emission keyword enabled so the texture is actually visible
        platformMaterial.EnableKeyword("_EMISSION");
    }

    // ─────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Scales damage so platformTextures.Length hits kill the platform,
    /// matching the original bounce-count-based destruction logic.
    /// </summary>
    private int GetDamagePerHit()
    {
        if (platformHealth == null || platformTextures.Length == 0) return 0;
        return Mathf.CeilToInt((float)platformHealth.StartingHealth / platformTextures.Length);
    }

    private void SpawnEnemyRandomly()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        if (Random.Range(0, 15) == 0)
            Instantiate(enemyPrefab, transform.position + Vector3.up, Quaternion.identity);
    }
}