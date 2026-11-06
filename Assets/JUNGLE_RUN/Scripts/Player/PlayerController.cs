using UnityEngine;
using System.Collections;

namespace JungleRun
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Lane Settings")]
        [SerializeField] private float laneDistance = 3f;
        [SerializeField] private float laneChangeSpeed = 25f;

        [Header("Speed Settings")]
        [SerializeField] private float startSpeed = 12f;
        [SerializeField] private float maxSpeed = 28f;
        [SerializeField] private float speedIncreaseRate = 0.08f;

        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float gravity = 30f;

        [Header("Slide Settings")]
        [SerializeField] private float slideDuration = 0.85f;
        [SerializeField] private float slideHeightMultiplier = 0.55f;
        [SerializeField] private float slideTransitionTime = 0.15f;

        [Header("Running Animation")]
        [SerializeField] private float runAnimationSpeed = 8f;
        [SerializeField] private float armSwingAngle = 40f;
        [SerializeField] private float legSwingAngle = 35f;
        [SerializeField] private float bodyBounceAmount = 0.025f;

        private CharacterController controller;

        private Transform humanBody;
        private Transform torso;
        private Transform head;

        private Transform leftArm;
        private Transform rightArm;

        private Transform leftForearm;
        private Transform rightForearm;

        private Transform leftLeg;
        private Transform rightLeg;

        private Transform leftKnee;
        private Transform rightKnee;

        private Vector3 originalBodyScale;

        private Vector3 torsoPosition;
        private Vector3 headPosition;
        private Vector3 leftArmPosition;
        private Vector3 rightArmPosition;

        private float originalHeight;
        private Vector3 originalCenter;

        private float currentSpeed;
        private float verticalVelocity;
        private float animationTime;

        private int currentLane = 1;

        private bool isSliding;
        private bool isDead;

        private Vector2 touchStart;

        private const float MIN_SWIPE_DIST = 40f;

        private GameObject shieldVisual;

        public static bool IsMagnetActive { get; private set; }
        public static bool HasShield { get; private set; }
        public static bool Is2XCoinsActive { get; private set; }

        public static float MagnetRemaining { get; private set; }
        public static float DoubleCoinsRemaining { get; private set; }
        public static float ShieldRemaining { get; private set; }

        public float CurrentSpeed => currentSpeed;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            originalHeight = controller.height;
            originalCenter = controller.center;

            currentSpeed = startSpeed;

            IsMagnetActive = false;
            HasShield = false;
            Is2XCoinsActive = false;

            MagnetRemaining = 0f;
            DoubleCoinsRemaining = 0f;
            ShieldRemaining = 0f;
        }

        private void Start()
        {
            CacheBodyParts();
            CreateShieldVisual();
        }

        // ---------------- BODY REFERENCES ----------------

        private void CacheBodyParts()
        {
            humanBody = transform.Find("HumanBody");

            if (humanBody == null)
            {
                Debug.LogError("HumanBody not found. Build the character first.");
                return;
            }

            originalBodyScale = humanBody.localScale;

            torso = humanBody.Find("Torso");
            head = humanBody.Find("HeadPivot");

            leftArm = humanBody.Find("UArmL");
            rightArm = humanBody.Find("UArmR");

            leftLeg = humanBody.Find("Thigh_L");
            rightLeg = humanBody.Find("Thigh_R");

            if (leftArm != null)
                leftForearm = leftArm.Find("ForearmPivot");

            if (rightArm != null)
                rightForearm = rightArm.Find("ForearmPivot");

            if (leftLeg != null)
                leftKnee = leftLeg.Find("KneePivot");

            if (rightLeg != null)
                rightKnee = rightLeg.Find("KneePivot");

            if (torso != null)
                torsoPosition = torso.localPosition;

            if (head != null)
                headPosition = head.localPosition;

            if (leftArm != null)
                leftArmPosition = leftArm.localPosition;

            if (rightArm != null)
                rightArmPosition = rightArm.localPosition;
        }

        // ---------------- UPDATE ----------------

        private void Update()
        {
            if (isDead)
                return;

            if (GameManager.Instance != null &&
                !GameManager.Instance.IsGameActive)
                return;

            UpdatePowerUpTimers();

            HandleKeyboardInput();
            HandleTouchInput();

            IncreaseSpeed();
            MovePlayer();
        }

        private void LateUpdate()
        {
            if (isDead || humanBody == null)
                return;

            if (GameManager.Instance != null &&
                !GameManager.Instance.IsGameActive)
                return;

            if (isSliding)
                return;

            if (controller.isGrounded)
                AnimateRunning();
            else
                AnimateJumping();

            if (shieldVisual != null && HasShield)
            {
                shieldVisual.transform.Rotate(
                    0f, 50f * Time.deltaTime, 0f);
            }
        }

        // ---------------- RUNNING ANIMATION ----------------

        private void AnimateRunning()
        {
            float speedFactor = currentSpeed / startSpeed;

            animationTime += Time.deltaTime *
                runAnimationSpeed * speedFactor;

            float cycle = animationTime;

            float movement = Mathf.Sin(cycle);

            // Natural body bounce
            if (torso != null)
            {
                Vector3 p = torsoPosition;

                p.y += Mathf.Abs(Mathf.Sin(cycle * 2f))
                    * bodyBounceAmount;

                torso.localPosition = p;

                torso.localRotation =
                    Quaternion.Euler(2f, 0f, 0f);
            }

            // Arms swing in opposite directions
            float armAngle = movement * armSwingAngle;

            if (leftArm != null)
            {
                leftArm.localRotation =
                    Quaternion.Euler(armAngle, 0f, -4f);
            }

            if (rightArm != null)
            {
                rightArm.localRotation =
                    Quaternion.Euler(-armAngle, 0f, 4f);
            }

            // Elbows bend naturally
            float leftElbow =
                Mathf.Max(0f, Mathf.Sin(cycle + 0.8f)) * 25f;

            float rightElbow =
                Mathf.Max(0f, Mathf.Sin(cycle + Mathf.PI + 0.8f))
                * 25f;

            if (leftForearm != null)
            {
                leftForearm.localRotation =
                    Quaternion.Euler(-leftElbow, 0f, 0f);
            }

            if (rightForearm != null)
            {
                rightForearm.localRotation =
                    Quaternion.Euler(-rightElbow, 0f, 0f);
            }

            // Legs move opposite to arms
            float legAngle = movement * legSwingAngle;

            if (leftLeg != null)
            {
                leftLeg.localRotation =
                    Quaternion.Euler(-legAngle, 0f, 0f);
            }

            if (rightLeg != null)
            {
                rightLeg.localRotation =
                    Quaternion.Euler(legAngle, 0f, 0f);
            }

            // Knee movement
            float leftBend =
                Mathf.Max(0f, Mathf.Sin(cycle + Mathf.PI)) * 30f;

            float rightBend =
                Mathf.Max(0f, Mathf.Sin(cycle)) * 30f;

            if (leftKnee != null)
                leftKnee.localRotation =
                    Quaternion.Euler(leftBend, 0f, 0f);

            if (rightKnee != null)
                rightKnee.localRotation =
                    Quaternion.Euler(rightBend, 0f, 0f);
        }

        // ---------------- JUMP ANIMATION ----------------

        private void AnimateJumping()
        {
            if (torso != null)
                torso.localRotation =
                    Quaternion.Euler(-8f, 0f, 0f);

            if (leftArm != null)
                leftArm.localRotation =
                    Quaternion.Euler(-40f, 0f, 0f);

            if (rightArm != null)
                rightArm.localRotation =
                    Quaternion.Euler(-40f, 0f, 0f);

            if (leftLeg != null)
                leftLeg.localRotation =
                    Quaternion.Euler(-20f, 0f, 0f);

            if (rightLeg != null)
                rightLeg.localRotation =
                    Quaternion.Euler(20f, 0f, 0f);
        }

        // ---------------- SLIDE ANIMATION ----------------

        private IEnumerator SlideRoutine()
        {
            if (isSliding || !controller.isGrounded)
                yield break;

            isSliding = true;

            float slideHeight =
                originalHeight * slideHeightMultiplier;

            // Keep the bottom of the collider on the ground
            controller.height = slideHeight;

            controller.center = new Vector3(
                originalCenter.x,
                slideHeight * 0.5f,
                originalCenter.z
            );

            // Smoothly enter the crouching slide pose
            float elapsed = 0f;

            while (elapsed < slideTransitionTime)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(
                    elapsed / slideTransitionTime);

                ApplySlidePose(t);

                yield return null;
            }

            ApplySlidePose(1f);

            // Maintain slide pose
            float remaining = slideDuration -
                slideTransitionTime * 2f;

            yield return new WaitForSeconds(
                Mathf.Max(0f, remaining));

            // Smoothly return to running pose
            elapsed = 0f;

            while (elapsed < slideTransitionTime)
            {
                elapsed += Time.deltaTime;

                float t = 1f -
                    Mathf.Clamp01(elapsed / slideTransitionTime);

                ApplySlidePose(t);

                yield return null;
            }

            // Restore original collider
            controller.height = originalHeight;
            controller.center = originalCenter;

            ResetBodyPose();

            isSliding = false;
        }

        private void ApplySlidePose(float amount)
        {
            amount = Mathf.Clamp01(amount);

            // Compress the body slightly without moving the feet
            if (humanBody != null)
            {
                Vector3 scale = originalBodyScale;
                scale.y *= Mathf.Lerp(1f, 0.85f, amount);

                humanBody.localScale = scale;
            }

            // Lower the upper body
            if (torso != null)
            {
                torso.localPosition = Vector3.Lerp(
                    torsoPosition,
                    torsoPosition + new Vector3(0f, -0.10f, 0f),
                    amount);

                torso.localRotation = Quaternion.Slerp(
                    Quaternion.identity,
                    Quaternion.Euler(45f, 0f, 0f),
                    amount);
            }

            // Keep the head facing forward
            if (head != null)
            {
                head.localPosition = Vector3.Lerp(
                    headPosition,
                    headPosition + new Vector3(0f, -0.18f, 0f),
                    amount);

                head.localRotation = Quaternion.Slerp(
                    Quaternion.identity,
                    Quaternion.Euler(-20f, 0f, 0f),
                    amount);
            }

            // Move arms backwards while sliding
            if (leftArm != null)
            {
                leftArm.localPosition = Vector3.Lerp(
                    leftArmPosition,
                    leftArmPosition + new Vector3(0f, -0.18f, 0f),
                    amount);

                leftArm.localRotation = Quaternion.Slerp(
                    Quaternion.identity,
                    Quaternion.Euler(65f, 0f, -10f),
                    amount);
            }

            if (rightArm != null)
            {
                rightArm.localPosition = Vector3.Lerp(
                    rightArmPosition,
                    rightArmPosition + new Vector3(0f, -0.18f, 0f),
                    amount);

                rightArm.localRotation = Quaternion.Slerp(
                    Quaternion.identity,
                    Quaternion.Euler(65f, 0f, 10f),
                    amount);
            }

            if (leftForearm != null)
                leftForearm.localRotation = Quaternion.Slerp(
                    Quaternion.identity,
                    Quaternion.Euler(-25f, 0f, 0f),
                    amount);

            if (rightForearm != null)
                rightForearm.localRotation = Quaternion.Slerp(
                    Quaternion.identity,
                    Quaternion.Euler(-25f, 0f, 0f),
                    amount);

            // Fold legs underneath the body
            if (leftLeg != null)
                leftLeg.localRotation = Quaternion.Slerp(
                    Quaternion.identity,
                    Quaternion.Euler(-20f, 0f, 0f),
                    amount);

            if (rightLeg != null)
                rightLeg.localRotation = Quaternion.Slerp(
                    Quaternion.identity,
                    Quaternion.Euler(-20f, 0f, 0f),
                    amount);

            if (leftKnee != null)
                leftKnee.localRotation = Quaternion.Slerp(
                    Quaternion.identity,
                    Quaternion.Euler(75f, 0f, 0f),
                    amount);

            if (rightKnee != null)
                rightKnee.localRotation = Quaternion.Slerp(
                    Quaternion.identity,
                    Quaternion.Euler(75f, 0f, 0f),
                    amount);
        }

        private void ResetBodyPose()
        {
            if (humanBody != null)
                humanBody.localScale = originalBodyScale;

            if (torso != null)
            {
                torso.localPosition = torsoPosition;
                torso.localRotation = Quaternion.identity;
            }

            if (head != null)
            {
                head.localPosition = headPosition;
                head.localRotation = Quaternion.identity;
            }

            if (leftArm != null)
            {
                leftArm.localPosition = leftArmPosition;
                leftArm.localRotation = Quaternion.identity;
            }

            if (rightArm != null)
            {
                rightArm.localPosition = rightArmPosition;
                rightArm.localRotation = Quaternion.identity;
            }

            if (leftForearm != null)
                leftForearm.localRotation = Quaternion.identity;

            if (rightForearm != null)
                rightForearm.localRotation = Quaternion.identity;

            if (leftLeg != null)
                leftLeg.localRotation = Quaternion.identity;

            if (rightLeg != null)
                rightLeg.localRotation = Quaternion.identity;

            if (leftKnee != null)
                leftKnee.localRotation = Quaternion.identity;

            if (rightKnee != null)
                rightKnee.localRotation = Quaternion.identity;
        }

        // ---------------- INPUT ----------------

        private void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.A) ||
                Input.GetKeyDown(KeyCode.LeftArrow))
                ChangeLane(-1);

            if (Input.GetKeyDown(KeyCode.D) ||
                Input.GetKeyDown(KeyCode.RightArrow))
                ChangeLane(1);

            if ((Input.GetKeyDown(KeyCode.W) ||
                 Input.GetKeyDown(KeyCode.Space) ||
                 Input.GetKeyDown(KeyCode.UpArrow)) &&
                 controller.isGrounded && !isSliding)
                Jump();

            if ((Input.GetKeyDown(KeyCode.S) ||
                 Input.GetKeyDown(KeyCode.DownArrow) ||
                 Input.GetKeyDown(KeyCode.LeftShift)) &&
                 !isSliding)
            {
                StartCoroutine(SlideRoutine());
            }
        }

        private void HandleTouchInput()
        {
            if (Input.touchCount == 0)
                return;

            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
                touchStart = touch.position;

            if (touch.phase == TouchPhase.Ended)
            {
                Vector2 delta = touch.position - touchStart;

                if (delta.magnitude < MIN_SWIPE_DIST)
                    return;

                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                {
                    ChangeLane(delta.x > 0 ? 1 : -1);
                }
                else if (delta.y > 0 &&
                         controller.isGrounded && !isSliding)
                {
                    Jump();
                }
                else if (delta.y < 0 && !isSliding)
                {
                    StartCoroutine(SlideRoutine());
                }
            }
        }

        // ---------------- MOVEMENT ----------------

        private void ChangeLane(int direction)
        {
            currentLane = Mathf.Clamp(
                currentLane + direction, 0, 2);
        }

        private void Jump()
        {
            if (!controller.isGrounded || isSliding)
                return;

            verticalVelocity = jumpForce;
        }

        private void IncreaseSpeed()
        {
            if (currentSpeed < maxSpeed)
            {
                currentSpeed +=
                    speedIncreaseRate * Time.deltaTime;

                currentSpeed = Mathf.Min(
                    currentSpeed, maxSpeed);
            }
        }

        private void MovePlayer()
        {
            float targetX =
                (currentLane - 1) * laneDistance;

            float newX = Mathf.MoveTowards(
                transform.position.x,
                targetX,
                laneChangeSpeed * Time.deltaTime);

            float deltaX = newX - transform.position.x;

            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;
            else
                verticalVelocity -= gravity * Time.deltaTime;

            Vector3 movement = new Vector3(
                deltaX,
                verticalVelocity * Time.deltaTime,
                currentSpeed * Time.deltaTime
            );

            controller.Move(movement);

            transform.rotation = Quaternion.identity;

            if (transform.position.y < -3f)
                TriggerDeath();
        }

        // ---------------- POWERUPS ----------------

        private void UpdatePowerUpTimers()
        {
            if (IsMagnetActive)
            {
                MagnetRemaining -= Time.deltaTime;

                if (MagnetRemaining <= 0f)
                {
                    MagnetRemaining = 0f;
                    IsMagnetActive = false;
                }
            }

            if (Is2XCoinsActive)
            {
                DoubleCoinsRemaining -= Time.deltaTime;

                if (DoubleCoinsRemaining <= 0f)
                {
                    DoubleCoinsRemaining = 0f;
                    Is2XCoinsActive = false;

                    if (ScoreManager.Instance != null)
                        ScoreManager.Instance.CoinMultiplier = 1;
                }
            }

            if (HasShield)
            {
                ShieldRemaining -= Time.deltaTime;

                if (ShieldRemaining <= 0f)
                {
                    ShieldRemaining = 0f;
                    HasShield = false;

                    if (shieldVisual != null)
                        shieldVisual.SetActive(false);
                }
            }
        }

        public void ActivateMagnet(float duration = 10f)
        {
            IsMagnetActive = true;
            MagnetRemaining = duration;
        }

        public void ActivateDoubleCoins(float duration = 10f)
        {
            Is2XCoinsActive = true;
            DoubleCoinsRemaining = duration;

            if (ScoreManager.Instance != null)
                ScoreManager.Instance.CoinMultiplier = 2;
        }

        public void ActivateShield(float duration = 10f)
        {
            HasShield = true;
            ShieldRemaining = duration;

            if (shieldVisual != null)
                shieldVisual.SetActive(true);
        }

        // ---------------- SHIELD VISUAL ----------------

        // Three energy rings replace the old solid blue ball.
        private void CreateShieldVisual()
        {
            if (shieldVisual != null)
                return;

            shieldVisual = new GameObject("ShieldAura");
            shieldVisual.transform.SetParent(transform, false);
            shieldVisual.transform.localPosition =
                new Vector3(0f, 0.9f, 0f);

            Material material = new Material(
                Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Standard"));

            material.color = new Color(0.05f, 0.85f, 1f);

            CreateShieldRing("HorizontalRing",
                shieldVisual.transform, material,
                new Vector3(0.8f, 0.8f, 0.8f),
                Vector3.zero);

            CreateShieldRing("VerticalRing1",
                shieldVisual.transform, material,
                new Vector3(0.8f, 1.2f, 0.8f),
                new Vector3(90f, 0f, 0f));

            CreateShieldRing("VerticalRing2",
                shieldVisual.transform, material,
                new Vector3(0.8f, 1.2f, 0.8f),
                new Vector3(90f, 60f, 0f));

            shieldVisual.SetActive(false);
        }

        private void CreateShieldRing(
            string ringName,
            Transform parent,
            Material material,
            Vector3 scale,
            Vector3 rotation)
        {
            GameObject ring = GameObject.CreatePrimitive(
                PrimitiveType.Cylinder);

            ring.name = ringName;
            ring.transform.SetParent(parent, false);
            ring.transform.localPosition = Vector3.zero;
            ring.transform.localRotation =
                Quaternion.Euler(rotation);
            ring.transform.localScale = new Vector3(
                scale.x, 0.025f, scale.z);

            Collider col = ring.GetComponent<Collider>();

            if (col != null)
                Destroy(col);

            ring.GetComponent<Renderer>().sharedMaterial =
                material;
        }

        // ---------------- COLLISION ----------------

        private void OnControllerColliderHit(
            ControllerColliderHit hit)
        {
            if (isDead)
                return;

            if (hit.gameObject.CompareTag("Obstacle"))
            {
                float dot = Vector3.Dot(
                    hit.normal, Vector3.back);

                if (dot > 0.45f)
                {
                    if (HasShield)
                    {
                        HasShield = false;
                        ShieldRemaining = 0f;

                        if (shieldVisual != null)
                            shieldVisual.SetActive(false);

                        AudioManager.Instance?.PlaySFX(
                            "PlayerCrash");

                        UIManager.Instance?.TriggerCollisionFeedback();

                        hit.gameObject.SetActive(false);
                        return;
                    }

                    TriggerDeath();
                }
            }
        }

        // ---------------- DEATH ----------------

        private void TriggerDeath()
        {
            if (isDead)
                return;

            isDead = true;

            if (GameManager.Instance != null)
                GameManager.Instance.GameOver();
        }

        // ---------------- RESET ----------------

        public void ResetPlayer()
        {
            StopAllCoroutines();

            isDead = false;
            isSliding = false;

            currentLane = 1;
            currentSpeed = startSpeed;
            verticalVelocity = 0f;
            animationTime = 0f;

            IsMagnetActive = false;
            HasShield = false;
            Is2XCoinsActive = false;

            MagnetRemaining = 0f;
            DoubleCoinsRemaining = 0f;
            ShieldRemaining = 0f;

            if (ScoreManager.Instance != null)
                ScoreManager.Instance.CoinMultiplier = 1;

            if (shieldVisual != null)
                shieldVisual.SetActive(false);

            controller.enabled = false;

            controller.height = originalHeight;
            controller.center = originalCenter;

            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;

            ResetBodyPose();

            controller.enabled = true;
        }
    }
}