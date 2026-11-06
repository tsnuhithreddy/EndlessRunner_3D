using UnityEngine;

namespace JungleRun
{
    public class HumanoidBuilder : MonoBehaviour
    {
        [Header("Character Colours")]
        public Color skinColor = new Color(0.85f, 0.62f, 0.40f);
        public Color shirtColor = new Color(0.05f, 0.48f, 0.42f);
        public Color pantsColor = new Color(0.12f, 0.19f, 0.28f);
        public Color shoeColor = new Color(0.12f, 0.10f, 0.09f);
        public Color hairColor = new Color(0.12f, 0.07f, 0.04f);

        [ContextMenu("Build Human Runner")]
        public void BuildHumanRunner()
        {
            Transform oldBody = transform.Find("HumanBody");

            if (oldBody != null)
                DestroyImmediate(oldBody.gameObject);

            GameObject body = new GameObject("HumanBody");
            body.transform.SetParent(transform, false);

            Material skin = CreateMaterial(skinColor);
            Material shirt = CreateMaterial(shirtColor);
            Material pants = CreateMaterial(pantsColor);
            Material shoes = CreateMaterial(shoeColor);
            Material hair = CreateMaterial(hairColor);

            BuildTorso(body.transform, shirt, pants);
            BuildHead(body.transform, skin, hair);
            BuildArms(body.transform, skin, shirt);
            BuildLegs(body.transform, pants, shoes);

            Debug.Log("Human Runner created successfully.");
        }

        private Material CreateMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");

            if (shader == null)
                shader = Shader.Find("Standard");

            Material mat = new Material(shader);
            mat.color = color;

            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);

            return mat;
        }

        private GameObject CreatePart(
            string partName,
            Transform parent,
            PrimitiveType type,
            Vector3 position,
            Vector3 scale,
            Material material)
        {
            GameObject part = GameObject.CreatePrimitive(type);

            part.name = partName;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localRotation = Quaternion.identity;
            part.transform.localScale = scale;

            Collider col = part.GetComponent<Collider>();

            if (col != null)
                DestroyImmediate(col);

            part.GetComponent<Renderer>().sharedMaterial = material;

            return part;
        }

        private Transform CreatePivot(
            string name,
            Transform parent,
            Vector3 position)
        {
            GameObject pivot = new GameObject(name);

            pivot.transform.SetParent(parent, false);
            pivot.transform.localPosition = position;
            pivot.transform.localRotation = Quaternion.identity;

            return pivot.transform;
        }

        private void BuildTorso(
            Transform body,
            Material shirt,
            Material pants)
        {
            CreatePart(
                "Pelvis",
                body,
                PrimitiveType.Cube,
                new Vector3(0, 0.82f, 0),
                new Vector3(0.34f, 0.20f, 0.22f),
                pants
            );

            Transform torso = CreatePivot(
                "Torso",
                body,
                new Vector3(0, 0.91f, 0)
            );

            CreatePart(
                "Chest",
                torso,
                PrimitiveType.Capsule,
                new Vector3(0, 0.20f, 0),
                new Vector3(0.38f, 0.34f, 0.23f),
                shirt
            );
        }

        private void BuildHead(
            Transform body,
            Material skin,
            Material hair)
        {
            Transform head = CreatePivot(
                "HeadPivot",
                body,
                new Vector3(0, 1.43f, 0)
            );

            CreatePart(
                "Head",
                head,
                PrimitiveType.Sphere,
                new Vector3(0, 0.13f, 0),
                new Vector3(0.27f, 0.33f, 0.25f),
                skin
            );

            CreatePart(
                "Hair",
                head,
                PrimitiveType.Sphere,
                new Vector3(0, 0.24f, -0.015f),
                new Vector3(0.28f, 0.16f, 0.27f),
                hair
            );

            CreatePart(
                "HairTop",
                head,
                PrimitiveType.Sphere,
                new Vector3(0, 0.30f, 0),
                new Vector3(0.23f, 0.08f, 0.22f),
                hair
            );
        }

        private void BuildArms(
            Transform body,
            Material skin,
            Material shirt)
        {
            BuildArm(body, "UArmL", -1, skin, shirt);
            BuildArm(body, "UArmR", 1, skin, shirt);
        }

        private void BuildArm(
            Transform body,
            string name,
            int side,
            Material skin,
            Material shirt)
        {
            float x = side * 0.25f;

            Transform arm = CreatePivot(
                name,
                body,
                new Vector3(x, 1.27f, 0)
            );

            CreatePart(
                "Shoulder",
                arm,
                PrimitiveType.Sphere,
                Vector3.zero,
                new Vector3(0.15f, 0.16f, 0.15f),
                shirt
            );

            CreatePart(
                "UpperArm",
                arm,
                PrimitiveType.Capsule,
                new Vector3(0, -0.16f, 0),
                new Vector3(0.11f, 0.18f, 0.11f),
                shirt
            );

            Transform forearm = CreatePivot(
                "ForearmPivot",
                arm,
                new Vector3(0, -0.31f, 0)
            );

            CreatePart(
                "Forearm",
                forearm,
                PrimitiveType.Capsule,
                new Vector3(0, -0.13f, 0),
                new Vector3(0.09f, 0.15f, 0.09f),
                skin
            );

            CreatePart(
                "Hand",
                forearm,
                PrimitiveType.Sphere,
                new Vector3(0, -0.26f, 0),
                new Vector3(0.09f, 0.11f, 0.09f),
                skin
            );
        }

        private void BuildLegs(
            Transform body,
            Material pants,
            Material shoes)
        {
            BuildLeg(body, "Thigh_L", -1, pants, shoes);
            BuildLeg(body, "Thigh_R", 1, pants, shoes);
        }

        private void BuildLeg(
            Transform body,
            string name,
            int side,
            Material pants,
            Material shoes)
        {
            Transform thigh = CreatePivot(
                name,
                body,
                new Vector3(side * 0.105f, 0.78f, 0)
            );

            CreatePart(
                "UpperLeg",
                thigh,
                PrimitiveType.Capsule,
                new Vector3(0, -0.18f, 0),
                new Vector3(0.15f, 0.22f, 0.14f),
                pants
            );

            Transform knee = CreatePivot(
                "KneePivot",
                thigh,
                new Vector3(0, -0.36f, 0)
            );

            CreatePart(
                "LowerLeg",
                knee,
                PrimitiveType.Capsule,
                new Vector3(0, -0.17f, 0),
                new Vector3(0.12f, 0.21f, 0.12f),
                pants
            );

            CreatePart(
                "Shoe",
                knee,
                PrimitiveType.Cube,
                new Vector3(0, -0.35f, 0.055f),
                new Vector3(0.16f, 0.12f, 0.27f),
                shoes
            );
        }
    }
}