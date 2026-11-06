using System.Collections.Generic;
using UnityEngine;

namespace JungleRun
{
    public class TrackChunk : MonoBehaviour
    {
        [Header("Chunk Settings")]
        [SerializeField] private Transform endPoint;
        [SerializeField] private Transform[] obstaclePoints;
        [SerializeField] private Transform[] relicPoints;

        private readonly List<GameObject> spawnedItems = new List<GameObject>();

        private const float DEFAULT_LENGTH = 30f;

        public Vector3 EndPosition
        {
            get
            {
                return endPoint != null
                    ? endPoint.position
                    : transform.position + transform.forward * DEFAULT_LENGTH;
            }
        }

        public Transform[] ObstaclePoints => obstaclePoints;
        public Transform[] RelicPoints => relicPoints;

        private void OnEnable()
        {
            spawnedItems.Clear();

            bool safeStart = TrackSpawner.Instance != null &&
                             TrackSpawner.Instance.IsSpawningSafeChunk;

            if (ObstacleSpawner.Instance != null)
            {
                ObstacleSpawner.Instance.PopulateChunk(
                    obstaclePoints,
                    relicPoints,
                    transform.position.z,
                    safeStart,
                    spawnedItems
                );
            }
        }

        private void OnDisable()
        {
            ReturnSpawnedItems();
        }

        private void ReturnSpawnedItems()
        {
            float startZ = transform.position.z - 0.5f;
            float endZ = EndPosition.z + 0.5f;

            for (int i = spawnedItems.Count - 1; i >= 0; i--)
            {
                GameObject item = spawnedItems[i];

                if (item == null || !item.activeSelf)
                    continue;

                float z = item.transform.position.z;

                // Item was reused by another chunk, so don't touch it
                if (z < startZ || z > endZ)
                    continue;

                if (ObjectPooler.Instance != null)
                    ObjectPooler.Instance.ReturnToPool(item);
                else
                    item.SetActive(false);
            }

            spawnedItems.Clear();
        }
    }
}