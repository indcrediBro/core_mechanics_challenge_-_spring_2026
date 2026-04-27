using System;
using IncredibleAttributes;
using UnityEngine;

public class Platform : MonoBehaviour
{
    [SerializeField, ShowAssetPreview(64,64)] private Texture[] platformTextures;
    [SerializeField] private Material platformMaterial;
    [SerializeField] private bool shouldMoveToRandomHeight;

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
            float random = UnityEngine.Random.Range(0f, 15f);
            transform.position = new Vector3(transform.position.x, transform.position.y + random, transform.position.z);
        }
    }

    public void HandleBounce()
    {
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
