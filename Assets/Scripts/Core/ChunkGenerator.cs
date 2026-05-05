using UnityEngine;
using System.Collections.Generic;

public class ChunkGenerator : MonoBehaviour
{
    [Header("Chunk Setup")]
    [SerializeField] private GameObject[] chunkPrefabs = new GameObject[3];
    [SerializeField] private int initialChunkCount = 4;
    [SerializeField] private int maxActiveChunks = 20;

    [Header("Marker Names (inside each chunk prefab)")]
    [SerializeField] private string startMarkerName = "Start";
    [SerializeField] private string endMarkerName = "End";

    [Header("Player Trigger")]
    [SerializeField] private Transform player;
    [SerializeField] private int spawnWhenPlayerReachesChunkIndex = 1;
    [SerializeField] private int chunksPerSpawn = 3;
    [SerializeField] private float movementThreshold = 0.01f;

    [Header("Seam Tuning")]
    [SerializeField] private float chunkOverlapOffset = 0.1f;

    private readonly List<GameObject> activeChunks = new List<GameObject>();
    private Transform nextAttachPoint;
    private Vector3 lastPlayerPosition;
    private bool hasSeededChunks;
    private GameObject lastTriggeredChunk;

    void Start()
    {
        if (chunkPrefabs == null || chunkPrefabs.Length == 0)
        {
            Debug.LogError("ChunkGenerator: No chunk prefabs assigned.");
            enabled = false;
            return;
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        // First chunk attaches at the generator's own transform.
        nextAttachPoint = transform;

        if (player != null)
        {
            lastPlayerPosition = player.position;
        }
    }

    void Update()
    {
        if (player == null || nextAttachPoint == null)
        {
            return;
        }

        float movedDistance = Vector3.Distance(player.position, lastPlayerPosition);
        lastPlayerPosition = player.position;

        if (movedDistance < movementThreshold)
        {
            return;
        }

        if (!hasSeededChunks)
        {
            SpawnChunks(initialChunkCount);
            hasSeededChunks = true;
            TrimOldChunks(GetPlayerCurrentChunkIndex());
            return;
        }

        int currentChunkIndex = GetPlayerCurrentChunkIndex();
        if (currentChunkIndex < spawnWhenPlayerReachesChunkIndex || currentChunkIndex >= activeChunks.Count)
        {
            return;
        }

        GameObject currentChunk = activeChunks[currentChunkIndex];
        if (currentChunk == null || currentChunk == lastTriggeredChunk)
        {
            return;
        }

        SpawnChunks(chunksPerSpawn);
        TrimOldChunks(currentChunkIndex);
        lastTriggeredChunk = currentChunk;
    }

    private void SpawnNextChunk()
    {
        GameObject prefab = GetRandomChunkPrefab();
        if (prefab == null)
        {
            return;
        }

        GameObject newChunk = Instantiate(prefab);
        AlignChunkToAttachPoint(newChunk);
        activeChunks.Add(newChunk);

        Transform endMarker = FindMarkerRecursive(newChunk.transform, endMarkerName);
        if (endMarker != null)
        {
            nextAttachPoint = endMarker;
        }
        else
        {
            Debug.LogWarning($"ChunkGenerator: Chunk '{newChunk.name}' is missing an '{endMarkerName}' marker. Using chunk root as attach point.");
            nextAttachPoint = newChunk.transform;
        }
    }

    private void SpawnChunks(int count)
    {
        int spawnCount = Mathf.Max(0, count);
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnNextChunk();
        }
    }

    private void AlignChunkToAttachPoint(GameObject chunk)
    {
        Transform startMarker = FindMarkerRecursive(chunk.transform, startMarkerName);

        if (startMarker == null)
        {
            Debug.LogWarning($"ChunkGenerator: Chunk '{chunk.name}' is missing a '{startMarkerName}' marker. Snapping chunk root directly to attach point.");
            chunk.transform.SetPositionAndRotation(nextAttachPoint.position, nextAttachPoint.rotation);
            return;
        }

        Quaternion rotationOffset = nextAttachPoint.rotation * Quaternion.Inverse(startMarker.rotation);
        chunk.transform.rotation = rotationOffset * chunk.transform.rotation;

        Vector3 startToRootOffset = chunk.transform.position - startMarker.position;
        chunk.transform.position = nextAttachPoint.position + startToRootOffset;
    }

    private void TrimOldChunks(int protectedChunkIndex)
    {
        while (activeChunks.Count > maxActiveChunks)
        {
            bool isProtectingOldest = protectedChunkIndex == 0 || protectedChunkIndex == 1;
            if (isProtectingOldest)
            {
                break;
            }

            GameObject oldestChunk = activeChunks[0];

            activeChunks.RemoveAt(0);
            if (oldestChunk != null)
            {
                Destroy(oldestChunk);
            }

            if (protectedChunkIndex >= 0)
            {
                protectedChunkIndex--;
            }
        }
    }

    private int GetPlayerCurrentChunkIndex()
    {
        if (activeChunks.Count == 0)
        {
            return -1;
        }

        int closestIndex = -1;
        float closestSqrDistance = float.MaxValue;

        for (int i = 0; i < activeChunks.Count; i++)
        {
            GameObject chunk = activeChunks[i];
            if (chunk == null)
            {
                continue;
            }

            float sqrDistance = (player.position - chunk.transform.position).sqrMagnitude;
            if (sqrDistance < closestSqrDistance)
            {
                closestSqrDistance = sqrDistance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    private GameObject GetRandomChunkPrefab()
    {
        List<GameObject> validPrefabs = new List<GameObject>();
        for (int i = 0; i < chunkPrefabs.Length; i++)
        {
            if (chunkPrefabs[i] != null)
            {
                validPrefabs.Add(chunkPrefabs[i]);
            }
        }

        if (validPrefabs.Count == 0)
        {
            Debug.LogError("ChunkGenerator: All chunk prefab references are null.");
            return null;
        }

        int randomIndex = Random.Range(0, validPrefabs.Count);
        return validPrefabs[randomIndex];
    }

    private Transform FindMarkerRecursive(Transform root, string markerName)
    {
        if (root.name == markerName)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindMarkerRecursive(root.GetChild(i), markerName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
