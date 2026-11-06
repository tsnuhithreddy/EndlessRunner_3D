using UnityEngine;

namespace JungleRun
{
    public enum ObstacleType { Low, High, Block, Moving }

    [ExecuteInEditMode]
    public class Obstacle : MonoBehaviour
    {
        public ObstacleType type;
        [SerializeField] private float moveSpeed = 3.0f;
        [SerializeField] private bool movingLeftToRight = false;
        private Vector3 startPos;
        private static Material sharedSlideMaterial;

        private void Awake()
        {
            AdjustObstacleDimensions();
        }

        private void OnEnable()
        {
            startPos = transform.position;
            AdjustObstacleDimensions();
        }

        private void Update()
        {
            if (type == ObstacleType.Moving)
            {
                float offset = Mathf.PingPong(Time.time * moveSpeed, 4.0f) - 2.0f;
                transform.position = new Vector3(startPos.x + (movingLeftToRight ? offset : 0), startPos.y, startPos.z);
            }
        }

        // Fixes oversized obstacle height so the runner can clearly see the track and upcoming coins
        private void AdjustObstacleDimensions()
        {
            // Remove any old complex sub-visual hierarchy if it existed
            Transform oldVis = transform.Find("JungleVisual");
            if (oldVis != null)
            {
                if (Application.isPlaying) Destroy(oldVis.gameObject);
                else DestroyImmediate(oldVis.gameObject);
            }

            string objName = gameObject.name;
            MeshRenderer rend = GetComponent<MeshRenderer>();
            if (rend != null) rend.enabled = true;

            // Load URP jungle materials
            Material barkMat = Resources.Load<Material>("Materials/Mat_JungleBark");
            Material stoneMat = Resources.Load<Material>("Materials/Mat_TempleStone");
            Material foliageMat = Resources.Load<Material>("Materials/Mat_JungleFoliage");

            // 1. High obstacles (Arch / Vine) - must leave headroom to slide under
            if (objName.Contains("Arch") || objName.Contains("Vine") || type == ObstacleType.High)
            {
                transform.localScale = new Vector3(2.4f, 0.45f, 0.6f);
                transform.localPosition = new Vector3(transform.localPosition.x, 2.0f, transform.localPosition.z);
                if (rend != null)
                {
                    Material slideMat = rend.material;
                    if (slideMat == null) slideMat = CreateObstacleMaterial(new Color(0.28f, 0.78f, 0.12f));
                    slideMat.color = new Color(0.28f, 0.78f, 0.12f);
                    if (slideMat.HasProperty("_BaseColor")) slideMat.SetColor("_BaseColor", new Color(0.28f, 0.78f, 0.12f));
                    if (slideMat.HasProperty("_EmissionColor"))
                    {
                        slideMat.EnableKeyword("_EMISSION");
                        slideMat.SetColor("_EmissionColor", new Color(0.08f, 0.28f, 0.025f));
                    }
                    rend.material = slideMat;
                    rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    rend.receiveShadows = false;
                }
                BuildSlideWarningVisual();
            }
            // 2. Low obstacles (Log / Root) - jump over
            else if (objName.Contains("Log") || objName.Contains("Root") || type == ObstacleType.Low)
            {
                transform.localScale = new Vector3(2.4f, 0.55f, 0.6f);
                transform.localPosition = new Vector3(transform.localPosition.x, 0.35f, transform.localPosition.z);
                if (rend != null && barkMat != null) rend.sharedMaterial = barkMat;
            }
            // 3. Block obstacles (Rock / Statue) - MUST NOT block horizon view! Cap height to 1.3m (lower than player eye line)
            else
            {
                transform.localScale = new Vector3(1.8f, 1.25f, 1.0f);
                transform.localPosition = new Vector3(transform.localPosition.x, 0.65f, transform.localPosition.z);
                if (rend != null && stoneMat != null) rend.sharedMaterial = stoneMat;
            }
        }

        private Material CreateObstacleMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material material = new Material(shader);
            material.color = color;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            return material;
        }

        private void BuildSlideWarningVisual()
        {
            Transform old = transform.Find("SlideWarningVisual");
            if (old != null)
            {
                if (Application.isPlaying) Destroy(old.gameObject);
                else DestroyImmediate(old.gameObject);
            }

            GameObject root = new GameObject("SlideWarningVisual");
            root.transform.SetParent(transform, false);
            root.transform.localPosition = Vector3.zero;
            Material vine = Resources.Load<Material>("Materials/Mat_JungleFoliage");
            if (vine == null) vine = CreateObstacleMaterial(new Color(0.12f, 0.55f, 0.12f));

            // Clear instruction visible from the runner's approach direction.
            GameObject sign = new GameObject("SlideInstruction");
            sign.transform.SetParent(root.transform, false);
            sign.transform.localPosition = new Vector3(0f, -0.06f, -0.34f);
            sign.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
            TextMesh instruction = sign.AddComponent<TextMesh>();
            instruction.text = "SLIDE";
            instruction.fontSize = 64;
            instruction.characterSize = 0.009f;
            instruction.anchor = TextAnchor.MiddleCenter;
            instruction.alignment = TextAlignment.Center;
            instruction.color = new Color(1f, 0.92f, 0.18f);

            // Hanging strands make the low clearance unmistakable from the approach.
            for (int i = 0; i < 7; i++)
            {
                float x = -0.95f + i * 0.315f;
                GameObject strand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                strand.name = "HangingVine";
                strand.transform.SetParent(root.transform, false);
                strand.transform.localPosition = new Vector3(x, -0.43f, 0.02f);
                strand.transform.localScale = new Vector3(0.055f, 0.43f, 0.055f);
                Renderer renderer = strand.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = vine;
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                }
                Collider collider = strand.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.enabled = false;
                    if (Application.isPlaying) Destroy(collider);
                    else DestroyImmediate(collider);
                }
            }
        }
    }
}
