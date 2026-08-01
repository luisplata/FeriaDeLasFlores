using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : IntEventInvoker
{
    private float speed = 5f;
    [SerializeField] public float jumpHeight = 2f;

    private Rigidbody rigidBody;
    public bool isGrounded => !isJumping;

    [SerializeField] private int lifes = 1;
    [SerializeField] private GameObject silleta;
    private bool canReceiveDamage = true;
    private GameOverEvent gameOverEvent = new GameOverEvent();

    private EnvironmentName currentEnvironment;
    private float flowerCompletionPercentage;
    private EnvironmentChangedEvent environmentChangedEvent = new EnvironmentChangedEvent();
    public int FlorEnPorcenajeParaEscribir => (int)(flowerCompletionPercentage * 100);
    public float FlorEnPorcentajeParaUi => flowerCompletionPercentage;

    [SerializeField] public Transform leftPosition;
    [SerializeField] public Transform centerPosition;
    [SerializeField] public Transform rightPosition;

    [Header("Config")] [SerializeField] public PlayerConfigSO playerConfig;

    private Animator animator;

    // ── Lane movement fields ──────────────────────────────────────
    [SerializeField] public int currentLane = 1; // 0=left, 1=center, 2=right
    [SerializeField] public Vector3 targetPosition;
    [SerializeField] public bool isSwitchingLane;
    public int? bufferedLane; // null when no buffer
    [SerializeField] public float laneSwitchSpeed;
    private float lastInputX; // for filtering held-key repeats (only when not switching)
    // ──────────────────────────────────────────────────────────────

    // ── Jump fields ───────────────────────────────────────────────
    [SerializeField] public bool isJumping;
    public float jumpStartY;
    private float jumpTimer;
    private float jumpDuration = 0.5f;
    [SerializeField] public bool isFastFalling;
    private float fastFallMultiplier = 2f;
    // ─────────────────────────────────────────────────────────────

    void Start()
    {
        InitializeLaneSystem();

        if (playerConfig != null)
        {
            speed = playerConfig.movementSpeed;
            jumpHeight = playerConfig.jumpHeight;
            jumpDuration = Mathf.Max(playerConfig.jumpDuration, 0.016f);
            fastFallMultiplier = playerConfig.fastFallMultiplier;
        }
        else
        {
            speed = ConfigurationUtils.PlayerMovementSpeed;
            jumpHeight = ConfigurationUtils.PlayerJumpHeight;
            jumpDuration = Mathf.Max(ConfigurationUtils.PlayerJumpDuration, 0.016f);
            fastFallMultiplier = ConfigurationUtils.PlayerFastFallMultiplier;
        }

        rigidBody = GetComponent<Rigidbody>();

        unityEvents.Add(EventName.GameOverEvent, gameOverEvent);
        // EventManager.AddInvoker(EventName.GameOverEvent, this);

        currentEnvironment = EnvironmentName.Forest;
        unityEvents.Add(EventName.EnvironmentChangedEvent, environmentChangedEvent);
        // EventManager.AddInvoker(EventName.EnvironmentChangedEvent, this);

        animator = GetComponent<Animator>();
        animator.enabled = false;
    }

    public void Update()
    {
        UpdateLaneMovement(Time.deltaTime);
        if (isJumping) UpdateJump(Time.deltaTime);
    }

    public void UpdateLaneMovement(float deltaTime)
    {
        if (!isSwitchingLane) return;

        Vector3 pos = transform.position;
        float newX = Mathf.MoveTowards(pos.x, targetPosition.x,
            laneSwitchSpeed * deltaTime);
        transform.position = new Vector3(newX, pos.y, pos.z);

        // Check only X distance — Y/Z may differ if player was moved after StartLaneTransition
        float tolerance = playerConfig != null
            ? playerConfig.movementTolerance
            : ConfigurationUtils.PlayerMovementTolerance;
        if (Mathf.Abs(transform.position.x - targetPosition.x) <= tolerance)
        {
            // Snap: use target X, preserve current Y and Z
            transform.position = new Vector3(targetPosition.x, pos.y, pos.z);
            isSwitchingLane = false;
            currentLane = GetLaneIndexForPosition(targetPosition);
            // Reset lastInputX to allow new input in same direction
            lastInputX = 0;

            if (bufferedLane.HasValue)
            {
                StartLaneTransition(bufferedLane.Value);
                bufferedLane = null;
            }
        }
    }

    // ── Jump arc logic ────────────────────────────────────────────

    /// <summary>
    /// Advances the jump arc each frame. Parabolic curve: 0 → peak → 0.
    /// When t >= 1.0, snaps Y to jumpStartY and clears isJumping.
    /// </summary>
    public void UpdateJump(float deltaTime)
    {
        float rawT = jumpTimer / jumpDuration;
        bool inDescent = rawT > 0.5f;
        float effectiveDt = (isFastFalling && inDescent)
            ? deltaTime * fastFallMultiplier
            : deltaTime;

        jumpTimer += effectiveDt;
        float t = Mathf.Clamp01(jumpTimer / jumpDuration);

        // Parabola: 4 * t * (1 - t) gives 0 at t=0, 1 at t=0.5, 0 at t=1
        float yOffset = jumpHeight * 4f * t * (1f - t);
        Vector3 pos = transform.position;
        transform.position = new Vector3(pos.x, jumpStartY + yOffset, pos.z);

        if (t >= 1f)
        {
            // Snap to exact start Y on landing
            transform.position = new Vector3(transform.position.x, jumpStartY, transform.position.z);
            isJumping = false;
        }
    }

    // ── Lane system initialisation ────────────────────────────────

    public void InitializeLaneSystem()
    {
        currentLane = 1;
        laneSwitchSpeed = playerConfig != null
            ? Mathf.Max(playerConfig.laneSwitchSpeed, 0.01f)
            : Mathf.Max(ConfigurationUtils.PlayerLaneSwitchSpeed, 0.01f);
        isSwitchingLane = false;
        bufferedLane = null;
        targetPosition = GetLaneTransform(currentLane).position;
        lastInputX = 0;
    }

    // ── Input handler ─────────────────────────────────────────────

    public void Move(InputAction.CallbackContext context)
    {
        Vector2 value = context.ReadValue<Vector2>();
        float x = value.x;
        isFastFalling = value.y < -0.5f;
        // Debug.Log($"Move input: x={x}, down={isFastFalling}");
        ProcessLaneInput(x);
    }

    public void Falling()
    {
        isFastFalling = true;
    }

    /// <summary>
    /// Input handler for jumping. Same SendMessage-compatible signature as Move.
    /// Gates on context.performed (press-only) and !isJumping.
    /// User binds this to their preferred input action.
    /// </summary>
    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
            TryPerformJump();
    }

    public void JumpWithSwipe()
    {
        TryPerformJump();
    }

    /// <summary>
    /// public jump gating logic. Returns true if the jump was initiated,
    /// false if already jumping. Testable without constructing InputAction.CallbackContext.
    /// </summary>
    public bool TryPerformJump()
    {
        if (isJumping) return false;

        // Debug.Log("Jump performed");
        jumpStartY = transform.position.y;
        jumpTimer = 0f;
        isJumping = true;
        return true;
    }

    public void ProcessLaneInput(float x)
    {
        // Ignore neutral
        if (Mathf.Approximately(x, 0))
        {
            lastInputX = 0;
            return;
        }

        int direction = x > 0 ? 1 : -1;

        if (!isSwitchingLane)
        {
            // Ignore held-key repeats only when not switching
            if (Mathf.Approximately(x, lastInputX)) return;
            lastInputX = x;

            int targetLane = Mathf.Clamp(currentLane + direction, 0, 2);
            if (targetLane != currentLane)
                StartLaneTransition(targetLane);
        }
        else
        {
            // Allow input even if same as last (so we can buffer)
            // But we don't update lastInputX here, it remains for next time we are not switching
            // (we could update it, but it doesn't matter because we ignore it in else)

            // Determine which way we're currently moving
            float moveDir = targetPosition.x - transform.position.x;
            bool movingRight = moveDir > 0f;

            if ((direction == 1 && !movingRight) || (direction == -1 && movingRight))
            {
                // Opposite direction: cancel current transition, go back
                // Cancel smoothly by stopping current transition and snapping to current position
                // Then move in the new direction
                isSwitchingLane = false; // stop transition
                // Snap to current position (but we want to stay at current X)
                // Actually, we need to reset to current lane's position? No, keep current position.
                // But currentLane hasn't changed yet, so we can just start a new transition from where we are.
                // However, we should update currentLane to the lane we are actually closest to,
                // to avoid weird snapping. Let's recalculate current lane based on current position.
                int actualLane = GetLaneIndexForPosition(transform.position);
                currentLane = actualLane;
                // Now move in the new direction
                int targetLane = Mathf.Clamp(currentLane + direction, 0, 2);
                if (targetLane != currentLane)
                    StartLaneTransition(targetLane);
                else
                {
                    // If we are at the edge and trying to go further, do nothing
                    // Reset lastInputX? Not needed because we are not switching
                }
            }
            else
            {
                // Same direction: buffer next lane (if there is one)
                // Use targetLane (the one we are moving towards) as base
                int nextLane = Mathf.Clamp(GetLaneIndexForPosition(targetPosition) + direction, 0, 2);
                // But if we are already at the edge, nextLane might equal current target, so no change.
                if (nextLane != GetLaneIndexForPosition(targetPosition))
                {
                    bufferedLane = nextLane;
                }
                // else, we are at edge and cannot buffer further, so ignore
            }
        }
    }

    // ── Helpers ───────────────────────────────────────────────────

    public void StartLaneTransition(int laneIndex)
    {
        Vector3 lane = GetLaneTransform(laneIndex).position;
        targetPosition = new Vector3(lane.x, transform.position.y, transform.position.z);
        isSwitchingLane = true;
    }

    public Transform GetLaneTransform(int index) => index switch
    {
        0 => leftPosition,
        1 => centerPosition,
        2 => rightPosition,
        _ => centerPosition
    };

    public int GetLaneIndexForPosition(Vector3 pos)
    {
        float d0 = Vector3.Distance(pos, leftPosition.position);
        float d1 = Vector3.Distance(pos, centerPosition.position);
        float d2 = Vector3.Distance(pos, rightPosition.position);
        return d0 < d1 ? (d0 < d2 ? 0 : 2) : (d1 < d2 ? 1 : 2);
    }

    // ── Collision & damage ────────────────────────────────────────

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Obstacle")
        {
            TakeDamage(collision.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Obstacle")
        {
            TakeDamage(other.gameObject);
            other.gameObject.transform.parent.gameObject.SetActive(false);
        }
    }

    private void TakeDamage(GameObject aggressorObject)
    {
        aggressorObject.SetActive(false);

        if (!canReceiveDamage)
        {
            return;
        }

        AudioManager.Play(AudioClipName.Crash);

        if (lifes <= 0)
        {
            //derrota
            gameOverEvent.Invoke(0);
            animator.enabled = true;
            animator.SetTrigger("gameOver");
            return;
        }

        lifes -= 1;
        StartCoroutine(DamageAnimation());
    }

    private IEnumerator DamageAnimation()
    {
        canReceiveDamage = false;
        for (int i = 0; i < 10; i++)
        {
            silleta.SetActive(false);
            yield return new WaitForSeconds(0.05f);
            silleta.SetActive(true);
            yield return new WaitForSeconds(0.05f);
        }

        canReceiveDamage = true;
    }

    public void SetFlowerCompletionPercentage(float percentage)
    {
        flowerCompletionPercentage = percentage;
        int flowerCompletionPercentageInt = (int)(flowerCompletionPercentage * 100);
        if (currentEnvironment != EnvironmentName.Street && flowerCompletionPercentageInt >= 33 &&
            flowerCompletionPercentageInt < 66)
        {
            currentEnvironment = EnvironmentName.Street;
            environmentChangedEvent.Invoke((int)EnvironmentName.Street);
        }
        else if (currentEnvironment != EnvironmentName.Tram && flowerCompletionPercentageInt >= 66 &&
                 flowerCompletionPercentageInt < 100)
        {
            currentEnvironment = EnvironmentName.Tram;
            environmentChangedEvent.Invoke((int)EnvironmentName.Tram);
        }
        else if (flowerCompletionPercentageInt == 100)
        {
            gameOverEvent.Invoke(0);
            animator.enabled = true;
            rigidBody.useGravity = false;
            animator.SetTrigger("win");
        }
    }

    public void ChangeToGameOverScene()
    {
        SceneManager.LoadScene(2);
    }
}