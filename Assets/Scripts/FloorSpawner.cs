using System;
using UnityEngine;

public class FloorSpawner : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private GameObject floorPrefab;

    [SerializeField] private float floorHeight = 15f;
    [SerializeField] private int initialSpawnedFloors = 5;
    private void Start()
    {
        for(int i = 1; i <= initialSpawnedFloors; i++)
            SpawnFloor(floorHeight * i);
    }

    //TODO: Spawn Floor at certain distance and soon as player crosses certain height spawn more floors on top.
    private void SpawnFloor(float height)
    {
        Instantiate(floorPrefab, Vector3.up * height, floorPrefab.transform.rotation);
    }
}
