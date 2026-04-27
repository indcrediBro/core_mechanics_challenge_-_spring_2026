using System;
using IncredibleAttributes;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class Platform : MonoBehaviour
{
    [SerializeField, ShowAssetPreview(64,64)] private Texture[] platformTextures;
    [SerializeField] private Material platformMaterial;
    [SerializeField] private bool shouldMoveToRandomHeight;
    [SerializeField] private bool shouldDestroyRandomlyOnStart;

    [SerializeField] private UnityEvent onBounceTaken;

    private int bounceCount = 0;
    private float cooldown = .15f;

    private void Awake()
    {
        platformMaterial = GetComponentInChildren<MeshRenderer>().material;
        platformMaterial.mainTexture = platformTextures[0];
    }

    private void OnEnable()
    {
        if (shouldMoveToRandomHeight)
        {
            float random = Random.Range(0f, 15f);
            transform.position = new Vector3(transform.position.x, transform.position.y + random, transform.position.z);
        }
    }

    private void Start()
    {
        if (shouldDestroyRandomlyOnStart)
        {
            int r = Random.Range(0, 5);
            if (r == 0)
            {
                Destroy(gameObject);
            }
        }
    }

    public void HandleBounce()
    {
        onBounceTaken?.Invoke();
        if(platformMaterial == null) return;

        if (bounceCount <= platformTextures.Length-1)
        {
            DamageTile();
        }
    }

    private void DamageTile()
    {
        bounceCount+=1;
        if(bounceCount >= platformTextures.Length)
        {
            BreakTile();
        }
        else
        {
            platformMaterial.SetTexture("_EmissionMap", platformTextures[bounceCount]);
        }
    }

    private void BreakTile()
    {
        Destroy(gameObject);
    }
}
