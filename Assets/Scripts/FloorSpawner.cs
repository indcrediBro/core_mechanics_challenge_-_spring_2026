using UnityEngine;

public class FloorSpawner : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private GameObject floorPrefab;

    [SerializeField] private int floorHeight = 15;
    [SerializeField] private int initialSpawnedFloors = 5;

    [SerializeField] private int spawnBuffer = 3;

    private float _highestSpawnedY;

    private void Start()
    {
        for (int i = 1; i <= initialSpawnedFloors; i++)
        {
            float y = floorHeight * i;
            SpawnFloor(y);
            _highestSpawnedY = y;
        }
    }

    private void Update()
    {
        while (player.position.y + floorHeight * spawnBuffer >= _highestSpawnedY)
        {
            _highestSpawnedY += floorHeight;
            SpawnFloor(_highestSpawnedY);
        }
    }

    private void SpawnFloor(float height)
    {
        Instantiate(floorPrefab, Vector3.up * height, floorPrefab.transform.rotation);
    }
}