using UnityEngine;

namespace JungleRun
{
    /// <summary>
    /// Sets jungle atmosphere on Start:
    ///   - Skybox color/tint
    ///   - Fog (linear distance + color)
    ///   - Ambient lighting & Sun directional light
    /// Works with URP and Built-in pipelines.
    /// </summary>
    [ExecuteAlways]
    public class JungleSkybox : MonoBehaviour
    {
        [Header("Sky Colors")]
        [SerializeField] private Color skyColor       = new Color(0.12f, 0.32f, 0.18f, 1f);   // Deep jungle green
        [SerializeField] private Color groundColor    = new Color(0.10f, 0.18f, 0.05f, 1f);   // Dark forest floor

        [Header("Fog Settings")]
        [SerializeField] private bool  enableFog      = true;
        [SerializeField] private Color fogColor       = new Color(0.24f, 0.45f, 0.22f, 1f);   // Lush misty green
        [SerializeField] private float fogStart       = 25f;
        [SerializeField] private float fogEnd         = 140f;

        [Header("Ambient Lighting")]
        [SerializeField] private Color ambientSky     = new Color(0.35f, 0.55f, 0.25f, 1f);
        [SerializeField] private Color ambientEquator = new Color(0.45f, 0.50f, 0.20f, 1f);
        [SerializeField] private Color ambientGround  = new Color(0.12f, 0.22f, 0.08f, 1f);

        [Header("Directional Sunlight")]
        [SerializeField] private Light sunLight;
        [SerializeField] private Color sunColor       = new Color(1.0f, 0.95f, 0.75f, 1f);   // Warm tropical sunlight
        [SerializeField] private float sunIntensity   = 1.15f;

        private void OnEnable()  => ApplyJungleAtmosphere();
        private void Start()     => ApplyJungleAtmosphere();

        public void ApplyJungleAtmosphere()
        {
            // ── Camera Clear Flags & Background Color ──
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                // In URP, setting SolidColor clear flag or background gives clean jungle canopy backdrop
                mainCam.backgroundColor = fogColor;
            }

            // ── Procedural Skybox Tint ──
            Shader proceduralShader = Shader.Find("Skybox/Procedural");
            if (proceduralShader != null)
            {
                Material skyMat = new Material(proceduralShader);
                skyMat.SetColor("_SkyTint", skyColor);
                skyMat.SetColor("_GroundColor", groundColor);
                skyMat.SetFloat("_AtmosphereThickness", 1.2f);
                skyMat.SetFloat("_SunDisk", 0f);
                RenderSettings.skybox = skyMat;
            }

            // ── Fog ──
            RenderSettings.fog              = enableFog;
            RenderSettings.fogMode          = FogMode.Linear;
            RenderSettings.fogColor         = fogColor;
            RenderSettings.fogStartDistance = fogStart;
            RenderSettings.fogEndDistance   = fogEnd;

            // ── Ambient Lighting ──
            RenderSettings.ambientMode         = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor     = ambientSky;
            RenderSettings.ambientEquatorColor = ambientEquator;
            RenderSettings.ambientGroundColor  = ambientGround;

            // ── Sun Light ──
            if (sunLight == null)
            {
                sunLight = Object.FindAnyObjectByType<Light>();
            }

            if (sunLight != null)
            {
                sunLight.color     = sunColor;
                sunLight.intensity = sunIntensity;
                sunLight.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            }

            DynamicGI.UpdateEnvironment();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ApplyJungleAtmosphere();
        }
#endif
    }
}
