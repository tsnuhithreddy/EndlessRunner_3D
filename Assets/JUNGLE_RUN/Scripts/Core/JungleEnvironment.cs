using System.Collections.Generic;
using UnityEngine;

namespace JungleRun
{
    /// <summary>
    /// Procedurally spawns decorative jungle scenery (trees, rocks, bushes)
    /// on both sides of the track as the player moves forward.
    /// Resets cleanly when player restarts run.
    /// </summary>
    public class JungleEnvironment : MonoBehaviour
    {
        public static JungleEnvironment Instance { get; private set; }

        [Header("References")]
        [SerializeField] private Transform playerTransform;

        [Header("Scenery Materials (Auto-loaded if empty)")]
        [SerializeField] private Material foliageMat;   // Mat_JungleFoliage
        [SerializeField] private Material barkMat;      // Mat_JungleBark
        [SerializeField] private Material rockMat;      // Mat_TempleStone

        [Header("Scenery Settings")]
        [SerializeField] private float spawnInterval = 8f;
        [SerializeField] private int   rowsAhead     = 20;
        [SerializeField] private float sideOffset    = 7f;

        [Header("Tree Size")]
        [SerializeField] private float trunkMinH = 3.5f;
        [SerializeField] private float trunkMaxH = 8f;

        private readonly Queue<GameObject> activeRows = new Queue<GameObject>();
        private float nextSpawnZ = 0f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (playerTransform == null)
            {
                var pc = Object.FindAnyObjectByType<PlayerController>();
                if (pc != null) playerTransform = pc.transform;
            }

            EnsureMaterials();
            InitScenery();
        }

        public void ResetEnvironment()
        {
            // Clear all active rows
            while (activeRows.Count > 0)
            {
                GameObject row = activeRows.Dequeue();
                if (row != null) Destroy(row);
            }

            InitScenery();
        }

        private void InitScenery()
        {
            nextSpawnZ = -spawnInterval * 2f;
            for (int i = 0; i < rowsAhead; i++)
            {
                SpawnRow();
            }
        }

        private void Update()
        {
            if (playerTransform == null) return;

            float playerZ = playerTransform.position.z;

            while (nextSpawnZ < playerZ + rowsAhead * spawnInterval)
                SpawnRow();

            while (activeRows.Count > 0)
            {
                GameObject oldest = activeRows.Peek();
                if (oldest == null || oldest.transform.position.z < playerZ - spawnInterval * 4f)
                {
                    if (oldest != null) Destroy(oldest);
                    activeRows.Dequeue();
                }
                else break;
            }
        }

        private void SpawnRow()
        {
            GameObject row = new GameObject($"EnvRow_{nextSpawnZ:F0}");
            row.transform.SetParent(transform);
            row.transform.position = new Vector3(0f, 0f, nextSpawnZ);

            float leftX  = -(sideOffset + Random.Range(0f, 2f));
            float rightX =   sideOffset + Random.Range(0f, 2f);

            SpawnTree(row.transform, leftX,  nextSpawnZ);
            SpawnTree(row.transform, rightX, nextSpawnZ);

            if (Random.value < 0.6f)
                SpawnTree(row.transform, leftX  - Random.Range(2f, 4f), nextSpawnZ + Random.Range(-3f, 3f));
            if (Random.value < 0.6f)
                SpawnTree(row.transform, rightX + Random.Range(2f, 4f), nextSpawnZ + Random.Range(-3f, 3f));

            if (Random.value < 0.35f) SpawnRock(row.transform, leftX  + 1f, nextSpawnZ);
            if (Random.value < 0.35f) SpawnRock(row.transform, rightX - 1f, nextSpawnZ);

            activeRows.Enqueue(row);
            nextSpawnZ += spawnInterval;
        }

        private void SpawnTree(Transform parent, float x, float z)
        {
            float trunkH = Random.Range(trunkMinH, trunkMaxH);
            float trunkR = Random.Range(0.14f, 0.30f);

            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(parent);
            trunk.transform.localPosition = new Vector3(x, trunkH * 0.5f, 0f);
            trunk.transform.localScale    = new Vector3(trunkR * 2f, trunkH * 0.5f, trunkR * 2f);
            ApplyMaterial(trunk, barkMat);
            RemoveCollider(trunk);

            float cr = Random.Range(1.6f, 3.2f);
            var canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.name = "Canopy";
            canopy.transform.SetParent(parent);
            canopy.transform.localPosition = new Vector3(x, trunkH + cr * 0.55f, 0f);
            canopy.transform.localScale    = new Vector3(cr * 2f, cr * 1.4f, cr * 2f);
            ApplyMaterial(canopy, foliageMat);
            RemoveCollider(canopy);

            float cr2 = cr * 0.70f;
            var canopy2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy2.name = "Canopy2";
            canopy2.transform.SetParent(parent);
            canopy2.transform.localPosition = new Vector3(x + Random.Range(-0.5f, 0.5f), trunkH - 0.4f + cr2, Random.Range(-0.4f, 0.4f));
            canopy2.transform.localScale    = new Vector3(cr2 * 2f, cr2 * 1.2f, cr2 * 2f);
            ApplyMaterial(canopy2, foliageMat);
            RemoveCollider(canopy2);

            if (Random.value < 0.5f)
            {
                float br = Random.Range(0.4f, 0.9f);
                var bush = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bush.name = "Bush";
                bush.transform.SetParent(parent);
                bush.transform.localPosition = new Vector3(x + Random.Range(-0.6f, 0.6f), br * 0.5f, Random.Range(-0.3f, 0.3f));
                bush.transform.localScale    = new Vector3(br * 2f, br, br * 1.8f);
                ApplyMaterial(bush, foliageMat);
                RemoveCollider(bush);
            }
        }

        private void SpawnRock(Transform parent, float x, float z)
        {
            float h = Random.Range(0.3f, 1.0f);
            float w = Random.Range(0.5f, 1.4f);

            var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = "Rock";
            rock.transform.SetParent(parent);
            rock.transform.localPosition    = new Vector3(x, h * 0.5f, 0f);
            rock.transform.localScale       = new Vector3(w, h, w * 0.85f);
            rock.transform.localEulerAngles = new Vector3(0f, Random.Range(0f, 360f), Random.Range(-15f, 15f));
            ApplyMaterial(rock, rockMat);
            RemoveCollider(rock);
        }

        private void ApplyMaterial(GameObject go, Material mat)
        {
            var rend = go.GetComponent<Renderer>();
            if (rend != null && mat != null) rend.sharedMaterial = mat;
        }

        private void RemoveCollider(GameObject go)
        {
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }

        private void EnsureMaterials()
        {
            if (foliageMat == null) foliageMat = Resources.Load<Material>("Materials/Mat_JungleFoliage");
            if (barkMat    == null) barkMat    = Resources.Load<Material>("Materials/Mat_JungleBark");
            if (rockMat    == null) rockMat    = Resources.Load<Material>("Materials/Mat_TempleStone");

            if (foliageMat == null) foliageMat = CreateFallbackURP(new Color(0.12f, 0.45f, 0.15f));
            if (barkMat    == null) barkMat    = CreateFallbackURP(new Color(0.32f, 0.19f, 0.10f));
            if (rockMat    == null) rockMat    = CreateFallbackURP(new Color(0.38f, 0.42f, 0.36f));
        }

        private Material CreateFallbackURP(Color col)
        {
            Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(sh);
            mat.SetColor("_BaseColor", col);
            mat.SetColor("_Color", col);
            return mat;
        }
    }
}
