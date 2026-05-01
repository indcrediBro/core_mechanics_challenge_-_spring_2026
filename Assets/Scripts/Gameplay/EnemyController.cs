using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // Inspector Fields
    // ─────────────────────────────────────────────

    [Header("References")]
    public Transform player;
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Jump Settings")]
    public float jumpForce = 6f;
    public float jumpIntervalMin = 1.5f;
    public float jumpIntervalMax = 4f;

    [Header("Detection & Turning")]
    public float detectionRange = 10f;
    public float turnSpeed = 180f;

    [Header("Shooting")]
    public float fireRate = 1.5f;
    public float projectileSpeed = 12f;
    public float projectileLifetime = 4f;

    private Rigidbody rb;
    private bool isGrounded = false;
    private float nextFireTime = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogWarning("[EnemyController] No Player assigned and none found with tag 'Player'.");
        }

        if (firePoint == null)
            firePoint = transform;
    }

    private void Start()
    {
        // StartCoroutine(JumpRoutine());
    }

    private void Update()
    {
        if (player == null || GameManager.Instance.CurrentState != GameState.Playing) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool playerInRange = distanceToPlayer <= detectionRange;

        if (playerInRange)
        {
            TurnTowardsPlayer();
            TryShoot();
        }
    }

    private IEnumerator JumpRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(jumpIntervalMin, jumpIntervalMax);
            yield return new WaitForSeconds(waitTime);

            if (isGrounded)
                Jump();
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Simple ground check: any collision from below counts as ground
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                Jump();
                break;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    private void TurnTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position);
        direction.y = 0f;

        if (direction == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );
    }

    private void TryShoot()
    {
        if (Time.time < nextFireTime) return;
        if (projectilePrefab == null)
        {
            Debug.LogWarning("[EnemyController] No projectile prefab assigned!");
            return;
        }

        Shoot();
        nextFireTime = Time.time + fireRate;
    }

    private void Shoot()
    {
        Vector3 aimDirection = (player.position - firePoint.position).normalized;

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(aimDirection));

        Rigidbody projRb = projectile.GetComponent<Rigidbody>();
        if (projRb != null)
        {
            projRb.useGravity = false;
            projRb.linearVelocity = aimDirection * projectileSpeed;
        }

        Destroy(projectile, projectileLifetime);
    }

    // ─────────────────────────────────────────────
    // Editor Gizmos
    // ─────────────────────────────────────────────

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.25f);
        Gizmos.DrawSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (firePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(firePoint.position, 0.1f);
        }
    }
#endif
}