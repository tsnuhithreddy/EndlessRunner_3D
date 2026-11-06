
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JungleRun
{
    public enum CollectibleType
    {
        Coin,
        Gem,
        Magnet,
        DoubleCoins,
        Shield
    }

    public class Collectible : MonoBehaviour, IPoolable
    {
        [Header("Collectible Settings")]
        [SerializeField] private CollectibleType type =
            CollectibleType.Coin;

        [SerializeField] private float rotateSpeed = 140f;

        private Vector3 initialScale;

        private bool isCollected;

        private Collider col;

        private Transform playerTarget;

        private Transform powerupVisual;

        private CollectibleType visualType;

        private bool visualInitialized;

        private static readonly Dictionary<Color, Material>
            powerMaterials = new Dictionary<Color, Material>();

        public CollectibleType Type => type;

        private void Awake()
        {
            initialScale = transform.localScale;

            col = GetComponent<Collider>();

            SetupVisualAppearance();
        }

        // Called every time the object becomes active.
        private void OnEnable()
        {
            ResetCollectible();
        }

        private void ResetCollectible()
        {
            StopAllCoroutines();

            isCollected = false;

            transform.localScale = initialScale;

            playerTarget = null;

            if (col == null)
                col = GetComponent<Collider>();

            if (col != null)
                col.enabled = true;
        }

        // Called by ObjectPooler after activation.
        public void OnSpawnedFromPool()
        {
            ResetCollectible();
        }

        // Called by ObjectPooler before deactivation.
        public void OnReturnedToPool()
        {
            StopAllCoroutines();

            isCollected = false;

            playerTarget = null;

            transform.localScale = initialScale;

            if (col != null)
                col.enabled = true;
        }

        public void SetType(CollectibleType newType)
        {
            if (type == newType && visualInitialized)
                return;

            type = newType;

            SetupVisualAppearance();
        }

        // ---------------- VISUAL APPEARANCE ----------------

        private void SetupVisualAppearance()
        {
            if (visualInitialized && visualType == type)
                return;

            Transform old = transform.Find("PowerupVisual");

            if (old != null)
            {
                if (Application.isPlaying)
                    Destroy(old.gameObject);
                else
                    DestroyImmediate(old.gameObject);
            }

            powerupVisual = null;

            MeshRenderer baseRenderer =
                GetComponent<MeshRenderer>();

            bool powerup =
                type == CollectibleType.Magnet ||
                type == CollectibleType.DoubleCoins ||
                type == CollectibleType.Shield;

            if (baseRenderer != null)
                baseRenderer.enabled = !powerup;

            if (!powerup)
            {
                if (baseRenderer != null)
                {
                    string materialName =
                        type == CollectibleType.Coin
                            ? "Mat_GoldRelic"
                            : "Mat_RedGem";

                    Material source =
                        Resources.Load<Material>(
                            "Materials/" + materialName
                        );

                    if (source != null)
                        baseRenderer.sharedMaterial = source;
                }

                visualType = type;
                visualInitialized = true;

                return;
            }

            GameObject visual = new GameObject(
                "PowerupVisual"
            );

            visual.transform.SetParent(transform, false);

            visual.transform.localPosition = Vector3.zero;

            visual.transform.localScale =
                Vector3.one * 1.5f;

            powerupVisual = visual.transform;

            Color main =
                type == CollectibleType.Magnet
                    ? new Color(0.08f, 0.55f, 1f)
                    : type == CollectibleType.DoubleCoins
                        ? new Color(1f, 0.72f, 0.05f)
                        : new Color(0.12f, 0.95f, 0.62f);

            Material mat = MakePowerMaterial(main);

            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");

                mat.SetColor(
                    "_EmissionColor",
                    main * 1.4f
                );
            }

            if (type == CollectibleType.Magnet)
            {
                CreatePowerPart(
                    PrimitiveType.Cylinder,
                    visual.transform,
                    new Vector3(-0.22f, 0f, 0f),
                    new Vector3(0.14f, 0.24f, 0.14f),
                    mat,
                    Vector3.zero
                );

                CreatePowerPart(
                    PrimitiveType.Cylinder,
                    visual.transform,
                    new Vector3(0.22f, 0f, 0f),
                    new Vector3(0.14f, 0.24f, 0.14f),
                    mat,
                    Vector3.zero
                );

                CreatePowerPart(
                    PrimitiveType.Cube,
                    visual.transform,
                    new Vector3(0f, -0.19f, 0f),
                    new Vector3(0.48f, 0.14f, 0.14f),
                    mat,
                    Vector3.zero
                );

                CreatePowerPart(
                    PrimitiveType.Cube,
                    visual.transform,
                    new Vector3(-0.22f, 0.27f, 0f),
                    new Vector3(0.18f, 0.12f, 0.18f),
                    MakePowerMaterial(Color.red),
                    Vector3.zero
                );

                CreatePowerPart(
                    PrimitiveType.Cube,
                    visual.transform,
                    new Vector3(0.22f, 0.27f, 0f),
                    new Vector3(0.18f, 0.12f, 0.18f),
                    MakePowerMaterial(Color.blue),
                    Vector3.zero
                );
            }
            else if (type == CollectibleType.DoubleCoins)
            {
                CreatePowerPart(
                    PrimitiveType.Cube,
                    visual.transform,
                    Vector3.zero,
                    new Vector3(0.62f, 0.62f, 0.16f),
                    mat,
                    Vector3.zero
                );

                GameObject label = new GameObject("X2");

                label.transform.SetParent(
                    visual.transform, false
                );

                label.transform.localPosition =
                    new Vector3(0f, 0f, -0.095f);

                TextMesh text = label.AddComponent<TextMesh>();

                text.text = "2X\nCOINS";
                text.fontSize = 72;
                text.characterSize = 0.012f;
                text.anchor = TextAnchor.MiddleCenter;
                text.alignment = TextAlignment.Center;

                label.transform.localEulerAngles =
                    new Vector3(0f, 180f, 0f);

                text.color = new Color(
                    0.18f, 0.10f, 0.01f
                );
            }
            else
            {
                CreatePowerPart(
                    PrimitiveType.Sphere,
                    visual.transform,
                    Vector3.zero,
                    new Vector3(0.48f, 0.58f, 0.18f),
                    mat,
                    Vector3.zero
                );

                CreatePowerPart(
                    PrimitiveType.Cube,
                    visual.transform,
                    new Vector3(0f, 0f, -0.105f),
                    new Vector3(0.30f, 0.055f, 0.035f),
                    MakePowerMaterial(Color.white),
                    Vector3.zero
                );

                CreatePowerPart(
                    PrimitiveType.Cube,
                    visual.transform,
                    new Vector3(0f, 0f, -0.105f),
                    new Vector3(0.055f, 0.34f, 0.035f),
                    MakePowerMaterial(Color.white),
                    Vector3.zero
                );
            }

            string displayName =
                type == CollectibleType.Magnet
                    ? "MAGNET"
                    : type == CollectibleType.DoubleCoins
                        ? "2X COINS"
                        : "SHIELD";

            CreatePowerLabel(
                visual.transform,
                displayName
            );

            visualType = type;
            visualInitialized = true;
        }

        private void CreatePowerLabel(
            Transform parent,
            string labelText)
        {
            GameObject label = new GameObject(
                "PowerupNameLabel"
            );

            label.transform.SetParent(parent, false);

            label.transform.localPosition =
                new Vector3(0f, 0.48f, -0.08f);

            label.transform.localRotation =
                Quaternion.Euler(0f, 180f, 0f);

            TextMesh text = label.AddComponent<TextMesh>();

            text.text = labelText;
            text.fontSize = 72;
            text.characterSize = 0.0105f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = Color.white;
            text.fontStyle = FontStyle.Bold;

            MeshRenderer renderer =
                label.GetComponent<MeshRenderer>();

            if (renderer != null)
            {
                renderer.shadowCastingMode =
                    UnityEngine.Rendering.ShadowCastingMode.Off;

                renderer.receiveShadows = false;

                renderer.material.color = Color.white;
            }
        }

        private Material MakePowerMaterial(Color color)
        {
            if (powerMaterials.TryGetValue(
                color, out Material cached) &&
                cached != null)
            {
                return cached;
            }

            Shader shader =
                Shader.Find("Universal Render Pipeline/Lit");

            if (shader == null)
                shader = Shader.Find("Standard");

            Material material = new Material(shader);

            material.color = color;

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            powerMaterials[color] = material;

            return material;
        }

        private void CreatePowerPart(
            PrimitiveType shape,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Material material,
            Vector3 rotation)
        {
            GameObject part =
                GameObject.CreatePrimitive(shape);

            part.name = "PowerupPart";

            part.transform.SetParent(parent, false);

            part.transform.localPosition = position;
            part.transform.localScale = scale;

            part.transform.localEulerAngles = rotation;

            Renderer renderer =
                part.GetComponent<Renderer>();

            if (renderer != null)
                renderer.sharedMaterial = material;

            Collider partCollider =
                part.GetComponent<Collider>();

            if (partCollider != null)
            {
                partCollider.enabled = false;

                if (Application.isPlaying)
                    Destroy(partCollider);
                else
                    DestroyImmediate(partCollider);
            }
        }

        // ---------------- MOVEMENT ----------------

        private void Update()
        {
            if (isCollected)
                return;

            transform.Rotate(
                Vector3.up * rotateSpeed * Time.deltaTime,
                Space.World
            );

            HandleMagnetMovement();
        }

        private void HandleMagnetMovement()
        {
            if (!PlayerController.IsMagnetActive)
                return;

            if (type != CollectibleType.Coin &&
                type != CollectibleType.Gem)
                return;

            if (playerTarget == null &&
                GameManager.Instance != null &&
                GameManager.Instance.Player != null)
            {
                playerTarget =
                    GameManager.Instance.Player.transform;
            }

            if (playerTarget == null)
                return;

            float distance = Vector3.Distance(
                transform.position,
                playerTarget.position
            );

            if (distance < 18f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    playerTarget.position +
                    Vector3.up * 0.8f,
                    24f * Time.deltaTime
                );
            }
        }

        // ---------------- COLLECTION ----------------

        private void OnTriggerEnter(Collider other)
        {
            if (isCollected)
                return;

            if (other.CompareTag("Player"))
            {
                StartCoroutine(
                    ShrinkAndCollectRoutine(other.gameObject)
                );
            }
        }

        private IEnumerator ShrinkAndCollectRoutine(
            GameObject playerObj)
        {
            if (isCollected)
                yield break;

            isCollected = true;

            if (col != null)
                col.enabled = false;

            PlayerController player =
                playerObj.GetComponent<PlayerController>();

            switch (type)
            {
                case CollectibleType.Coin:

                    AudioManager.Instance?.PlaySFX(
                        "CoinCollect"
                    );

                    ScoreManager.Instance?.AddCoin(1);

                    break;

                case CollectibleType.Gem:

                    AudioManager.Instance?.PlaySFX(
                        "GemCollect"
                    );

                    ScoreManager.Instance?.AddGem(1);

                    break;

                case CollectibleType.Magnet:

                    AudioManager.Instance?.PlaySFX(
                        "Powerup"
                    );

                    player?.ActivateMagnet(10f);

                    break;

                case CollectibleType.DoubleCoins:

                    AudioManager.Instance?.PlaySFX(
                        "Powerup"
                    );

                    player?.ActivateDoubleCoins(10f);

                    break;

                case CollectibleType.Shield:

                    AudioManager.Instance?.PlaySFX(
                        "Powerup"
                    );

                    player?.ActivateShield(10f);

                    break;
            }

            float elapsed = 0f;

            float duration = 0.12f;

            Vector3 startScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                transform.localScale = Vector3.Lerp(
                    startScale,
                    Vector3.zero,
                    Mathf.Clamp01(elapsed / duration)
                );

                yield return null;
            }

            if (ObjectPooler.Instance != null)
            {
                ObjectPooler.Instance.ReturnToPool(
                    gameObject
                );
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
