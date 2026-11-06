using System.Collections.Generic;
using UnityEngine;

namespace JungleRun
{
    public class TrackSpawner : MonoBehaviour
    {
        public static TrackSpawner Instance { get; private set; }

        [Header("References")]
        [SerializeField] private Transform playerTransform;

        [Header("Chunk Settings")]
        [SerializeField] private float chunkLength = 30f;
        [SerializeField] private int chunksOnScreen = 7;
        [SerializeField] private int safeStartChunks = 2;

        [Header("Chunk Pool Tags")]
        [SerializeField] private string[] chunkTags =
        {
            "Chunk_Path",
            "Chunk_Bridge",
            "Chunk_Ruins",
            "Chunk_Tunnel"
        };

        private float spawnZ = 0f;

        private Queue<GameObject> activeChunks =
            new Queue<GameObject>();

        private int safeChunksRemaining;

        public bool IsSpawningSafeChunk { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");

                if (player != null)
                    playerTransform = player.transform;
            }

            SpawnInitialChunks();
        }

        private void Update()
        {
            if (playerTransform == null)
                return;

            if (GameManager.Instance == null ||
                !GameManager.Instance.IsGameActive)
                return;

            MaintainTrack();
            RecycleOldChunks();
        }

        private void MaintainTrack()
        {
            float requiredDistance =
                Mathf.Max(1, chunksOnScreen) * chunkLength;

            int attempts = 0;
            int maximumAttempts = Mathf.Max(1, chunksOnScreen) * 4;

            while (spawnZ - playerTransform.position.z < requiredDistance)
            {
                if (attempts >= maximumAttempts)
                {
                    Debug.LogError(
                        "Track spawning stopped: too many failed attempts."
                    );
                    break;
                }

                bool spawned;

                if (safeChunksRemaining > 0)
                {
                    spawned = SpawnChunk("Chunk_Path", true);
                }
                else
                {
                    spawned = SpawnRandomChunk();
                }

                attempts++;

                if (!spawned)
                    break;
            }
        }

        private void RecycleOldChunks()
        {
            while (activeChunks.Count > 1)
            {
                GameObject oldestChunk = activeChunks.Peek();

                if (oldestChunk == null)
                {
                    activeChunks.Dequeue();
                    continue;
                }

                float chunkEndZ = GetChunkEndZ(oldestChunk);

                float distancePassed =
                    playerTransform.position.z - chunkEndZ;

                if (distancePassed > chunkLength + 10f)
                {
                    activeChunks.Dequeue();

                    if (ObjectPooler.Instance != null)
                    {
                        ObjectPooler.Instance.ReturnToPool(oldestChunk);
                    }
                    else
                    {
                        oldestChunk.SetActive(false);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private float GetChunkEndZ(GameObject chunk)
        {
            if (chunk == null)
                return 0f;

            TrackChunk trackChunk = chunk.GetComponent<TrackChunk>();

            if (trackChunk != null)
                return trackChunk.EndPosition.z;

            return chunk.transform.position.z + chunkLength;
        }

        private void SpawnInitialChunks()
        {
            spawnZ = 0f;

            activeChunks.Clear();

            safeChunksRemaining = Mathf.Max(0, safeStartChunks);

            IsSpawningSafeChunk = false;

            if (ObstacleSpawner.Instance != null)
                ObstacleSpawner.Instance.ResetSpawnSchedule();

            for (int i = 0; i < Mathf.Max(1, chunksOnScreen); i++)
            {
                bool spawned;

                if (safeChunksRemaining > 0)
                {
                    spawned = SpawnChunk("Chunk_Path", true);
                }
                else
                {
                    spawned = SpawnRandomChunk();
                }

                if (!spawned)
                    break;
            }
        }

        private bool SpawnRandomChunk()
        {
            if (chunkTags == null || chunkTags.Length == 0)
            {
                Debug.LogError("No chunk tags assigned in TrackSpawner!");
                return false;
            }

            // Try the available tags instead of getting stuck
            // repeatedly trying one missing pool.
            List<string> availableTags = new List<string>();

            foreach (string tag in chunkTags)
            {
                if (!string.IsNullOrEmpty(tag) &&
                    !availableTags.Contains(tag))
                {
                    availableTags.Add(tag);
                }
            }

            while (availableTags.Count > 0)
            {
                int index = Random.Range(0, availableTags.Count);
                string selectedTag = availableTags[index];

                availableTags.RemoveAt(index);

                if (SpawnChunk(selectedTag, false))
                    return true;
            }

            Debug.LogError(
                "Could not spawn any track chunk. Check ObjectPooler tags."
            );

            return false;
        }

        private bool SpawnChunk(string poolTag, bool safeStart)
        {
            if (ObjectPooler.Instance == null)
            {
                Debug.LogError("ObjectPooler instance is missing!");
                return false;
            }

            if (string.IsNullOrEmpty(poolTag))
            {
                Debug.LogError("Chunk pool tag is empty!");
                return false;
            }

            Vector3 spawnPosition =
                new Vector3(0f, 0f, spawnZ);

            GameObject chunk = null;

            IsSpawningSafeChunk = safeStart;

            try
            {
                chunk = ObjectPooler.Instance.SpawnFromPool(
                    poolTag,
                    spawnPosition,
                    Quaternion.identity
                );
            }
            finally
            {
                IsSpawningSafeChunk = false;
            }

            if (chunk == null)
            {
                Debug.LogError("Failed to spawn chunk: " + poolTag);
                return false;
            }

            activeChunks.Enqueue(chunk);

            // Use the actual endpoint of the spawned chunk.
            TrackChunk trackChunk = chunk.GetComponent<TrackChunk>();

            float nextSpawnZ = trackChunk != null
                ? trackChunk.EndPosition.z
                : spawnZ + chunkLength;

            // Prevent a chunk with an incorrect endpoint from
            // stopping the endless track from progressing.
            if (nextSpawnZ <= spawnZ + 0.01f)
            {
                Debug.LogWarning(
                    "Chunk " + poolTag +
                    " has an invalid EndPoint. Using chunkLength instead."
                );

                nextSpawnZ = spawnZ + chunkLength;
            }

            spawnZ = nextSpawnZ;

            if (safeStart && safeChunksRemaining > 0)
                safeChunksRemaining--;

            return true;
        }

        public void ResetTrack()
        {
            while (activeChunks.Count > 0)
            {
                GameObject chunk = activeChunks.Dequeue();

                if (chunk != null)
                {
                    if (ObjectPooler.Instance != null)
                        ObjectPooler.Instance.ReturnToPool(chunk);
                    else
                        chunk.SetActive(false);
                }
            }

            IsSpawningSafeChunk = false;

            SpawnInitialChunks();
        }

        public void SetPlayer(Transform player)
        {
            playerTransform = player;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}