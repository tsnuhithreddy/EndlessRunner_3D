using System.Collections.Generic;
using UnityEngine;

namespace JungleRun
{
    public class ObstacleSpawner : MonoBehaviour
    {
        public static ObstacleSpawner Instance { get; private set; }

        [Header("Obstacle Pool Tags")]
        [SerializeField] private string[] lowObstacleTags =
            { "Obs_Log", "Obs_Root" };

        [SerializeField] private string[] highObstacleTags =
            { "Obs_Vine", "Obs_Arch" };

        [SerializeField] private string[] blockObstacleTags =
            { "Obs_Statue", "Obs_Rock" };

        [Header("Collectible Pool Tags")]
        [SerializeField] private string coinTag = "Relic_SunCoin";
        [SerializeField] private string gemTag = "Relic_Gem";

        [Header("Obstacle Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float obstacleChance = 0.40f;

        [Header("Coin Settings")]
        [SerializeField] private int minimumCoinsPerChunk = 4;

        [Header("Gem Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float gemChance = 0.06f;

        [Header("Power-up Spacing")]
        [SerializeField] private float minimumPowerupDistance = 150f;
        [SerializeField] private float maximumPowerupDistance = 250f;

        [Header("Obstacle Spacing")]
        [SerializeField] private float minimumSpecialObstacleSpacing = 45f;
        [SerializeField] private float maximumSpecialObstacleSpacing = 75f;

        [Header("Cleanup Settings")]
        [SerializeField] private float cleanupDistance = 45f;

        private float nextPowerupDistance;
        private float nextHighObstacleDistance;
        private float nextLowObstacleDistance;

        private Transform playerTransform;

        private readonly List<GameObject> spawnedObjects =
            new List<GameObject>();

        // ---------------- INITIALIZATION ----------------

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            ResetSpawnSchedule();
        }

        private void Start()
        {
            GameObject player =
                GameObject.FindGameObjectWithTag("Player");

            if (player != null)
                playerTransform = player.transform;
            else
                Debug.LogWarning(
                    "ObstacleSpawner: Player object not found.");
        }

        public void SetPlayer(Transform player)
        {
            playerTransform = player;
        }

        private void Update()
        {
            CleanupOldObjects();
        }

        // ---------------- RESET SCHEDULE ----------------

        public void ResetSpawnSchedule()
        {
            ClearSpawnedObjects();

            nextPowerupDistance = Random.Range(
                minimumPowerupDistance,
                maximumPowerupDistance);

            nextHighObstacleDistance = Random.Range(
                minimumSpecialObstacleSpacing,
                maximumSpecialObstacleSpacing);

            nextLowObstacleDistance = Random.Range(
                minimumSpecialObstacleSpacing,
                maximumSpecialObstacleSpacing);
        }

        // ---------------- POPULATE CHUNK ----------------

        public void PopulateChunk(
            Transform[] obstaclePoints,
            Transform[] relicPoints,
            float chunkStartZ,
            bool safeStart,
            List<GameObject> trackedSpawnedList = null)
        {
            if (!safeStart)
            {
                SpawnObstacles(
                    obstaclePoints,
                    chunkStartZ,
                    trackedSpawnedList);
            }

            SpawnCollectibles(
                relicPoints,
                chunkStartZ,
                trackedSpawnedList);
        }

        // ---------------- OBSTACLES ----------------

        private void SpawnObstacles(
            Transform[] points,
            float chunkStartZ,
            List<GameObject> trackedList)
        {
            if (points == null || points.Length == 0)
                return;

            bool spawnedHigh = false;
            bool spawnedLow = false;

            for (int i = 0; i < points.Length; i++)
            {
                Transform point = points[i];

                if (point == null)
                    continue;

                float distance = point.position.z;

                bool forceHigh =
                    !spawnedHigh &&
                    distance >= nextHighObstacleDistance;

                bool forceLow =
                    !spawnedLow &&
                    distance >= nextLowObstacleDistance;

                if (!forceHigh &&
                    !forceLow &&
                    Random.value > obstacleChance)
                    continue;

                string selectedTag;

                if (forceHigh)
                {
                    selectedTag = PickTag(highObstacleTags);

                    spawnedHigh = true;

                    nextHighObstacleDistance =
                        distance + Random.Range(
                            minimumSpecialObstacleSpacing,
                            maximumSpecialObstacleSpacing);
                }
                else if (forceLow)
                {
                    selectedTag = PickTag(lowObstacleTags);

                    spawnedLow = true;

                    nextLowObstacleDistance =
                        distance + Random.Range(
                            minimumSpecialObstacleSpacing,
                            maximumSpecialObstacleSpacing);
                }
                else
                {
                    float roll = Random.value;

                    if (roll < 0.35f)
                    {
                        selectedTag = PickTag(highObstacleTags);

                        spawnedHigh = true;

                        nextHighObstacleDistance =
                            distance + Random.Range(
                                minimumSpecialObstacleSpacing,
                                maximumSpecialObstacleSpacing);
                    }
                    else if (roll < 0.70f)
                    {
                        selectedTag = PickTag(lowObstacleTags);

                        spawnedLow = true;

                        nextLowObstacleDistance =
                            distance + Random.Range(
                                minimumSpecialObstacleSpacing,
                                maximumSpecialObstacleSpacing);
                    }
                    else
                    {
                        selectedTag = PickTag(blockObstacleTags);
                    }
                }

                SpawnObject(
                    selectedTag,
                    point,
                    trackedList);
            }
        }

        private string PickTag(string[] tags)
        {
            if (tags == null || tags.Length == 0)
                return string.Empty;

            return tags[Random.Range(0, tags.Length)];
        }

        // ---------------- COLLECTIBLES ----------------

        private void SpawnCollectibles(
            Transform[] points,
            float chunkStartZ,
            List<GameObject> trackedList)
        {
            if (points == null || points.Length == 0)
                return;

            // Find the actual beginning and end of this chunk.
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;

            for (int i = 0; i < points.Length; i++)
            {
                if (points[i] == null)
                    continue;

                minZ = Mathf.Min(minZ, points[i].position.z);
                maxZ = Mathf.Max(maxZ, points[i].position.z);
            }

            if (minZ == float.MaxValue)
                return;

            // Find the closest position for the next power-up.
            int powerupIndex = -1;

            if (nextPowerupDistance <= maxZ)
            {
                float closestDistance = float.MaxValue;

                for (int i = 0; i < points.Length; i++)
                {
                    if (points[i] == null)
                        continue;

                    float difference = Mathf.Abs(
                        points[i].position.z - nextPowerupDistance);

                    if (difference < closestDistance)
                    {
                        closestDistance = difference;
                        powerupIndex = i;
                    }
                }
            }

            int coinsSpawned = 0;

            // Every valid relic point gets a collectible.
            // Coins are no longer randomly skipped.
            for (int i = 0; i < points.Length; i++)
            {
                Transform point = points[i];

                if (point == null)
                    continue;

                // Special power-up position
                if (i == powerupIndex)
                {
                    CollectibleType powerupType =
                        (CollectibleType)Random.Range(
                            (int)CollectibleType.Magnet,
                            (int)CollectibleType.Shield + 1);

                    SpawnCollectible(
                        gemTag,
                        point,
                        powerupType,
                        trackedList);

                    nextPowerupDistance = point.position.z + Random.Range(
                        minimumPowerupDistance,
                        maximumPowerupDistance);

                    continue;
                }

                // Keep the minimum number of coins guaranteed.
                if (coinsSpawned < minimumCoinsPerChunk)
                {
                    SpawnCollectible(
                        coinTag,
                        point,
                        CollectibleType.Coin,
                        trackedList);

                    coinsSpawned++;
                    continue;
                }

                // After the minimum coins, occasionally spawn gems.
                if (Random.value < gemChance)
                {
                    SpawnCollectible(
                        gemTag,
                        point,
                        CollectibleType.Gem,
                        trackedList);
                }
                else
                {
                    SpawnCollectible(
                        coinTag,
                        point,
                        CollectibleType.Coin,
                        trackedList);

                    coinsSpawned++;
                }
            }

            if (coinsSpawned < minimumCoinsPerChunk)
            {
                Debug.LogWarning(
                    "Not enough relic points in chunk starting at " +
                    chunkStartZ + " to spawn all guaranteed coins.");
            }
        }

        // ---------------- SPAWN OBJECT ----------------

        private void SpawnObject(
            string tag,
            Transform point,
            List<GameObject> trackedList)
        {
            if (point == null || string.IsNullOrEmpty(tag))
                return;

            if (ObjectPooler.Instance == null)
            {
                Debug.LogError(
                    "ObjectPooler.Instance is missing.");
                return;
            }

            GameObject obj =
                ObjectPooler.Instance.SpawnFromPool(
                    tag,
                    point.position,
                    point.rotation);

            if (obj == null)
                return;

            TrackObject(obj, trackedList);
        }

        private void SpawnCollectible(
            string tag,
            Transform point,
            CollectibleType type,
            List<GameObject> trackedList)
        {
            if (point == null || string.IsNullOrEmpty(tag))
                return;

            if (ObjectPooler.Instance == null)
                return;

            GameObject item =
                ObjectPooler.Instance.SpawnFromPool(
                    tag,
                    point.position,
                    point.rotation);

            if (item == null)
                return;

            Collectible collectible =
                item.GetComponent<Collectible>();

            if (collectible == null)
                collectible = item.AddComponent<Collectible>();

            collectible.SetType(type);

            TrackObject(item, trackedList);
        }

        // ---------------- OBJECT TRACKING ----------------

        private void TrackObject(
            GameObject obj,
            List<GameObject> trackedList)
        {
            if (obj == null)
                return;

            if (!spawnedObjects.Contains(obj))
                spawnedObjects.Add(obj);

            if (trackedList != null &&
                !trackedList.Contains(obj))
            {
                trackedList.Add(obj);
            }
        }

        // ---------------- CLEANUP ----------------

        private void CleanupOldObjects()
        {
            if (playerTransform == null)
                return;

            float playerZ = playerTransform.position.z;

            for (int i = spawnedObjects.Count - 1; i >= 0; i--)
            {
                GameObject obj = spawnedObjects[i];

                if (obj == null)
                {
                    spawnedObjects.RemoveAt(i);
                    continue;
                }

                // Only return objects that are safely behind the player.
                if (playerZ - obj.transform.position.z >
                    cleanupDistance)
                {
                    ObjectPooler.Instance?.ReturnToPool(obj);

                    spawnedObjects.RemoveAt(i);
                }
            }
        }

        private void ClearSpawnedObjects()
        {
            for (int i = spawnedObjects.Count - 1; i >= 0; i--)
            {
                if (spawnedObjects[i] != null)
                {
                    ObjectPooler.Instance?.ReturnToPool(
                        spawnedObjects[i]);
                }
            }

            spawnedObjects.Clear();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}