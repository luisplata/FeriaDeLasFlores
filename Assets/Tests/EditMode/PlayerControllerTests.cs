using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// EditMode tests for PlayerController lane movement logic.
/// RED phase: tests reference code structures that must be implemented.
/// Does NOT depend on Time.deltaTime — tests state transitions and edge cases.
/// </summary>
public class PlayerControllerTests
{
    private GameObject playerObject;
    private PlayerController playerController;
    private PlayerConfigSO playerConfig;
    private Transform leftPos;
    private Transform centerPos;
    private Transform rightPos;

    // Lane positions matching typical 3-lane layout
    private const float LeftX = -5f;
    private const float CenterX = 0f;
    private const float RightX = 5f;

    [SetUp]
    public void SetUp()
    {
        ConfigurationUtils.Initialize();

        // Create a PlayerConfigSO with explicit test values
        playerConfig = ScriptableObject.CreateInstance<PlayerConfigSO>();
        playerConfig.laneSwitchSpeed = 5f;
        playerConfig.movementTolerance = 0.05f;
        playerConfig.movementSpeed = 15f;
        playerConfig.jumpHeight = 3f;
        playerConfig.jumpDuration = 0.5f;

        playerObject = new GameObject("TestPlayer");
        playerController = playerObject.AddComponent<PlayerController>();
        playerController.playerConfig = playerConfig;
        playerObject.AddComponent<Rigidbody>();

        // Create and position lane transforms
        leftPos = new GameObject("Left").transform;
        leftPos.position = new Vector3(LeftX, 0, 0);
        centerPos = new GameObject("Center").transform;
        centerPos.position = new Vector3(CenterX, 0, 0);
        rightPos = new GameObject("Right").transform;
        rightPos.position = new Vector3(RightX, 0, 0);

        // Assign lane transforms to the controller (internal fields accessible in same assembly)
        playerController.leftPosition = leftPos;
        playerController.centerPosition = centerPos;
        playerController.rightPosition = rightPos;

        // Initialize lane system (the lane-specific subset of Start)
        playerController.InitializeLaneSystem();

        // Mirror the jump config loading that Start() would do
        playerController.jumpHeight = playerConfig.jumpHeight;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(rightPos.gameObject);
        Object.DestroyImmediate(centerPos.gameObject);
        Object.DestroyImmediate(leftPos.gameObject);
        Object.DestroyImmediate(playerObject);
    }

    // ──────────────────────────────────────────────
    //  Initial State (Task 2.2 — RED / Task 2.3 — GREEN)
    // ──────────────────────────────────────────────

    /// <summary>
    /// Spec: "The player MUST start at lane index 1 (center)."
    /// </summary>
    [Test]
    public void InitialLaneIsCenter()
    {
        Assert.That(playerController.currentLane, Is.EqualTo(1),
            "Player must start at lane index 1 (center)");
    }

    /// <summary>
    /// Spec: "The player MUST start with no active lane transition."
    /// </summary>
    [Test]
    public void InitialIsNotSwitchingLane()
    {
        Assert.That(playerController.isSwitchingLane, Is.False,
            "Player must not be mid-transition at start");
    }

    /// <summary>
    /// Verifies laneSwitchSpeed is set from config during initialization.
    /// </summary>
    [Test]
    public void LaneSwitchSpeedSetFromConfig()
    {
        Assert.That(playerController.laneSwitchSpeed, Is.EqualTo(ConfigurationUtils.PlayerLaneSwitchSpeed),
            "laneSwitchSpeed should be initialized from ConfigurationUtils.PlayerLaneSwitchSpeed");
    }

    /// <summary>
    /// Verifies targetPosition is set to center lane position after init.
    /// </summary>
    [Test]
    public void InitialTargetPositionIsCenter()
    {
        Assert.That(playerController.targetPosition.x, Is.EqualTo(CenterX).Within(0.001f),
            "Target position X should match center lane on init");
    }

    // ──────────────────────────────────────────────
    //  Move / ProcessLaneInput (Task 2.4 — RED / Task 2.5 — GREEN)
    // ──────────────────────────────────────────────

    /// <summary>
    /// Spec: "Left input from center → begin transitioning toward lane 0."
    /// </summary>
    [Test]
    public void LeftInputFromCenterBeginsTransitionToLaneZero()
    {
        playerController.ProcessLaneInput(-0.8f);

        Assert.That(playerController.isSwitchingLane, Is.True,
            "Left input from center should start a lane transition");
        Assert.That(playerController.targetPosition.x, Is.EqualTo(LeftX).Within(0.001f),
            "Target position should be the left lane position");
    }

    /// <summary>
    /// Spec: "Right input from center → begin transitioning toward lane 2."
    /// </summary>
    [Test]
    public void RightInputFromCenterBeginsTransitionToLaneTwo()
    {
        playerController.ProcessLaneInput(0.8f);

        Assert.That(playerController.isSwitchingLane, Is.True,
            "Right input from center should start a lane transition");
        Assert.That(playerController.targetPosition.x, Is.EqualTo(RightX).Within(0.001f),
            "Target position should be the right lane position");
    }

    /// <summary>
    /// Spec: "Left input at lane 0 → player MUST remain at lane index 0."
    /// Also verifies isSwitchingLane stays false (no transition started).
    /// </summary>
    [Test]
    public void LeftInputAtLeftmostBoundaryStaysAtLaneZero()
    {
        playerController.currentLane = 0;
        playerController.targetPosition = leftPos.position;

        playerController.ProcessLaneInput(-0.8f);

        Assert.That(playerController.currentLane, Is.EqualTo(0),
            "Player must stay at lane 0 when left input at boundary");
        Assert.That(playerController.isSwitchingLane, Is.False,
            "No transition should start when at boundary");
    }

    /// <summary>
    /// Spec: "Right input at lane 2 → player MUST remain at lane index 2."
    /// </summary>
    [Test]
    public void RightInputAtRightmostBoundaryStaysAtLaneTwo()
    {
        playerController.currentLane = 2;
        playerController.targetPosition = rightPos.position;

        playerController.ProcessLaneInput(0.8f);

        Assert.That(playerController.currentLane, Is.EqualTo(2),
            "Player must stay at lane 2 when right input at boundary");
        Assert.That(playerController.isSwitchingLane, Is.False,
            "No transition should start when at boundary");
    }

    /// <summary>
    /// Spec: "Zero or near-zero input → no-op."
    /// </summary>
    [Test]
    public void ZeroInputDoesNothing()
    {
        playerController.ProcessLaneInput(0f);

        Assert.That(playerController.currentLane, Is.EqualTo(1),
            "Current lane must remain unchanged on zero input");
        Assert.That(playerController.isSwitchingLane, Is.False,
            "No transition should start on zero input");
    }

    /// <summary>
    /// Triangulation: small positive and negative inputs that should still register.
    /// </summary>
    [Test]
    public void SmallPositiveInputMovesRight()
    {
        playerController.ProcessLaneInput(0.1f);
        Assert.That(playerController.isSwitchingLane, Is.True,
            "Even a small positive X should trigger rightward transition");
    }

    /// <summary>
    /// Triangulation: ProcessLaneInput should handle values from both ends of the axis.
    /// </summary>
    [Test]
    public void SmallNegativeInputMovesLeft()
    {
        playerController.ProcessLaneInput(-0.1f);
        Assert.That(playerController.isSwitchingLane, Is.True,
            "Even a small negative X should trigger leftward transition");
    }

    // ──────────────────────────────────────────────
    //  Update / Transition Completion (Task 2.6 — RED / Task 2.7 — GREEN)
    // ──────────────────────────────────────────────

    /// <summary>
    /// Spec: "When distance to target ≤ 0.05, snap to exact target position."
    /// Tests with position just inside the tolerance threshold.
    /// </summary>
    [Test]
    public void SnapCompletesWhenWithinTolerance()
    {
        // Start a transition to center
        playerController.StartLaneTransition(1);

        // Place the transform extremely close to target (within 0.05 tolerance)
        Vector3 nearTarget = new Vector3(0.03f, 1.5f, 10f);
        playerObject.transform.position = nearTarget;

        playerController.Update();

        Assert.That(playerObject.transform.position.x, Is.EqualTo(CenterX).Within(0.001f),
            "Player must snap to exact target X when within tolerance");
        Assert.That(playerController.isSwitchingLane, Is.False,
            "isSwitchingLane must be false after snap completes");
    }

    /// <summary>
    /// Triangulation: position just outside tolerance should NOT snap.
    /// Uses UpdateLaneMovement with explicit deltaTime to ensure consistent behavior.
    /// </summary>
    [Test]
    public void DoesNotSnapWhenOutsideTolerance()
    {
        playerController.StartLaneTransition(2); // target is (RightX, 0, 0)

        // Place the transform far from target (beyond 0.05 tolerance)
        Vector3 farFromTarget = new Vector3(0f, 1f, 10f);
        playerObject.transform.position = farFromTarget;

        // Use explicit deltaTime=0 to guarantee no MoveTowards movement
        playerController.UpdateLaneMovement(0f);

        float tolerance = playerConfig.movementTolerance;
        float xDist = Mathf.Abs(playerObject.transform.position.x - playerController.targetPosition.x);

        // Verify no snap happened
        Assert.That(playerController.isSwitchingLane, Is.True,
            "Player must still be mid-transition when outside tolerance (xDist={0}, tolerance={1})",
            xDist, tolerance);
    }

    /// <summary>
    /// Spec: "The player's Y and Z position MUST NOT be altered by lane-switch logic."
    /// Tests that Y/Z are preserved even when the snap fires.
    /// </summary>
    [Test]
    public void LaneTransitionPreservesYAndZ()
    {
        Vector3 startPos = new Vector3(2f, 1.5f, 8f);
        playerObject.transform.position = startPos;

        playerController.StartLaneTransition(1); // target is center (0, 0, 0)

        // Place within snap range
        Vector3 nearTarget = new Vector3(0.03f, 1.5f, 8f);
        playerObject.transform.position = nearTarget;

        playerController.Update();

        Assert.That(playerObject.transform.position.y, Is.EqualTo(1.5f).Within(0.001f),
            "Y position must be preserved during lane transition and snap");
        Assert.That(playerObject.transform.position.z, Is.EqualTo(8f).Within(0.001f),
            "Z position must be preserved during lane transition and snap");
    }

    // ──────────────────────────────────────────────
    //  Buffered Input (Task 2.8 — RED / Task 2.9 — GREEN)
    // ──────────────────────────────────────────────

    /// <summary>
    /// Spec: "Same-direction input during transition is buffered."
    /// Opposite-direction cancels instead.
    /// </summary>
    [Test]
    public void InputDuringTransitionIsBuffered()
    {
        // Start a transition to the right
        playerController.StartLaneTransition(2);

        // Same-direction input (rightward again) → buffer next lane
        playerController.ProcessLaneInput(0.8f);

        Assert.That(playerController.bufferedLane, Is.EqualTo(2),
            "Same-direction rightward input during rightward transition should buffer lane 2");
    }

    /// <summary>
    /// Spec: "Same-direction repeated inputs overwrite buffer."
    /// Opposite-direction input mid-transition cancels instead of buffering.
    /// </summary>
    [Test]
    public void SameDirectionRepeatedInputOverwritesBuffer()
    {
        // Start moving left (from center to left)
        playerController.StartLaneTransition(0);

        // Same direction again → buffer another left
        playerController.ProcessLaneInput(-0.8f);
        Assert.That(playerController.bufferedLane, Is.EqualTo(0),
            "Mid-transition, same-direction input should buffer");

        // Same direction again → overwrite buffer (still 0)
        playerController.ProcessLaneInput(-0.5f);
        Assert.That(playerController.bufferedLane, Is.EqualTo(0),
            "Repeated same-direction input should keep buffer at 0");
    }

    /// <summary>
    /// Opposite-direction input mid-transition cancels the transition.
    /// </summary>
    [Test]
    public void OppositeDirectionCancelsTransition()
    {
        playerController.StartLaneTransition(2); // moving right

        // Opposite direction (left) → should cancel, not buffer
        playerController.ProcessLaneInput(-0.8f);
        Assert.That(playerController.bufferedLane.HasValue, Is.False,
            "Opposite direction should cancel transition, not buffer");
        Assert.That(playerController.targetPosition.x, Is.EqualTo(CenterX).Within(0.001f),
            "Target should be back at center (currentLane) after cancel");
    }

    /// <summary>
    /// Triangulation: buffer is null when no input during transition.
    /// </summary>
    [Test]
    public void BufferIsNullWhenNoInputDuringTransition()
    {
        playerController.StartLaneTransition(2);
        Assert.That(playerController.bufferedLane.HasValue, Is.False,
            "Buffer must be null when no input is received during transition");
    }

    /// <summary>
    /// Spec: "Buffered input fires on completion."
    /// Same-direction input mid-transition is buffered and fires after snap.
    /// </summary>
    [Test]
    public void BufferedInputFiresAfterSnap()
    {
        // Start a transition heading left (from center to left)
        playerController.StartLaneTransition(0);
        Assert.That(playerController.isSwitchingLane, Is.True);

        // While mid-transition, press left AGAIN → buffer another left move.
        // currentLane is still 1 (center), so pressing left sets bufferedLane = 0.
        playerController.ProcessLaneInput(-1f);
        Assert.That(playerController.bufferedLane, Is.EqualTo(0),
            "Same-direction input mid-transition must buffer the next lane (should be 0)");

        // Simulate frames. The first transition goes from center→left (lane 0).
        // On snap, the buffer fires (lane 0 again), but since we're at left boundary,
        // Mathf.Clamp(0 + (-1), 0, 2) = 0 = currentLane → no-op (already at boundary).
        float dt = 0.02f;
        int maxFrames = 200;
        int frame = 0;
        while (playerController.isSwitchingLane && frame < maxFrames)
        {
            playerController.UpdateLaneMovement(dt);
            frame++;
        }

        Assert.That(frame, Is.LessThan(maxFrames),
            "Transition should complete within frame budget");
        Assert.That(playerController.isSwitchingLane, Is.False,
            "Transition should have completed");
        Assert.That(playerController.bufferedLane.HasValue, Is.False,
            "Buffer must be cleared after firing or no-op");
        Assert.That(playerObject.transform.position.x, Is.EqualTo(LeftX).Within(playerConfig.movementTolerance),
            "Final position should be at left lane");
    }

    /// <summary>
    /// Spec: "Opposite-direction input mid-transition cancels and goes back."
    /// </summary>
    [Test]
    public void OppositeInputCancelsTransition()
    {
        // Start moving left from center
        playerController.StartLaneTransition(0);
        Assert.That(playerController.isSwitchingLane, Is.True);

        // Simulate a few frames so we're partway there
        for (int i = 0; i < 10; i++)
            playerController.UpdateLaneMovement(0.02f);

        float posBeforeCancel = playerObject.transform.position.x;
        Assert.That(posBeforeCancel, Is.LessThan(CenterX),
            "Player should have moved left from center");

        // Press right → should cancel the transition back to currentLane (center)
        playerController.ProcessLaneInput(1f);

        // Now we should be moving right (back toward center)
        Assert.That(playerController.isSwitchingLane, Is.True,
            "Should still be mid-transition (now going back to center)");
        Assert.That(playerController.targetPosition.x, Is.EqualTo(CenterX).Within(0.001f),
            "Target should be back to center lane");

        // Simulate frames to complete the return journey
        int frames = 0;
        while (playerController.isSwitchingLane && frames < 200)
        {
            playerController.UpdateLaneMovement(0.02f);
            frames++;
        }

        Assert.That(playerController.isSwitchingLane, Is.False,
            "Return transition should complete");
        Assert.That(playerObject.transform.position.x, Is.EqualTo(CenterX).Within(playerConfig.movementTolerance),
            "Player should be back at center lane");
        Assert.That(playerController.currentLane, Is.EqualTo(1),
            "currentLane should be 1 (center) after returning");
    }

    // ──────────────────────────────────────────────
    //  Helper Methods (Task 2.10 — REFACTOR verification)
    // ──────────────────────────────────────────────

    /// <summary>
    /// Verifies GetLaneTransform maps indices correctly.
    /// </summary>
    [Test]
    public void GetLaneTransformReturnsCorrectTransform()
    {
        Assert.That(playerController.GetLaneTransform(0), Is.SameAs(leftPos),
            "Index 0 must return leftPosition");
        Assert.That(playerController.GetLaneTransform(1), Is.SameAs(centerPos),
            "Index 1 must return centerPosition");
        Assert.That(playerController.GetLaneTransform(2), Is.SameAs(rightPos),
            "Index 2 must return rightPosition");
    }

    /// <summary>
    /// Verifies GetLaneTransform falls back to center for invalid index.
    /// </summary>
    [Test]
    public void GetLaneTransformInvalidIndexReturnsCenter()
    {
        Assert.That(playerController.GetLaneTransform(-1), Is.SameAs(centerPos),
            "Invalid negative index must return centerPosition");
        Assert.That(playerController.GetLaneTransform(99), Is.SameAs(centerPos),
            "Invalid large index must return centerPosition");
    }

    /// <summary>
    /// Verifies GetLaneIndexForPosition identifies the nearest lane.
    /// </summary>
    [Test]
    public void GetLaneIndexForPositionReturnsClosestLane()
    {
        Assert.That(playerController.GetLaneIndexForPosition(leftPos.position), Is.EqualTo(0),
            "Position at left lane should map to index 0");
        Assert.That(playerController.GetLaneIndexForPosition(centerPos.position), Is.EqualTo(1),
            "Position at center lane should map to index 1");
        Assert.That(playerController.GetLaneIndexForPosition(rightPos.position), Is.EqualTo(2),
            "Position at right lane should map to index 2");
    }

    /// <summary>
    /// Triangulation: GetLaneIndexForPosition with midpoint positions.
    /// </summary>
    [Test]
    public void GetLaneIndexForPositionMidpoint()
    {
        // Midpoint between left (-5) and center (0) → should return 0 (left is slightly nearer? 
        // Actually -2.5 is closer to center: distance to center = 2.5, to left = 2.5. Equal!
        // At exactly -2.5, d0 = 2.5, d1 = 2.5. The function checks d0 < d1 (false), so falls to 
        // d1 < d2: 2.5 < 7.5 (true) → returns 1 (center).
        // That's correct — at exactly midpoint it prefers center.
        float midX = (LeftX + CenterX) / 2f;
        Vector3 midLeftRight = new Vector3(midX, 0, 0);
        int index = playerController.GetLaneIndexForPosition(midLeftRight);
        Assert.That(index, Is.EqualTo(0).Or.EqualTo(1),
            "Midpoint between left and center should resolve to 0 or 1");

        // Point closer to right lane
        Vector3 nearRight = new Vector3(RightX - 0.1f, 0, 0);
        Assert.That(playerController.GetLaneIndexForPosition(nearRight), Is.EqualTo(2),
            "Position near right lane should map to index 2");
    }

    // ── Integration tests (simulated frame progression) ─────────────

    /// <summary>
    /// Simulates N frames of UpdateLaneMovement with a fixed deltaTime.
    /// Returns the number of frames simulated OR until transition completes.
    /// </summary>
    private int SimulateFrames(float deltaTime, int maxFrames)
    {
        int frames = 0;
        while (playerController.isSwitchingLane && frames < maxFrames)
        {
            playerController.UpdateLaneMovement(deltaTime);
            frames++;
        }
        return frames;
    }

    /// <summary>
    /// Spec: "Full transition + snap completes to correct lane position."
    /// Uses simulated frame steps instead of real-time yielding.
    /// </summary>
    [Test]
    public void FullTransitionCompletesToCorrectLane()
    {
        Vector3 startPos = new Vector3(CenterX, 1f, 10f);
        playerObject.transform.position = startPos;

        float switchSpeed = playerConfig.laneSwitchSpeed;
        float tolerance = playerConfig.movementTolerance;
        float distance = Mathf.Abs(LeftX - CenterX); // 5 units
        float dt = 0.02f; // ~50fps
        int expectedFrames = Mathf.CeilToInt(distance / (switchSpeed * dt)) + 5; // +5 buffer

        playerController.StartLaneTransition(0); // target is (-5, 0, 0)

        int frames = SimulateFrames(dt, expectedFrames + 50);

        Assert.That(frames, Is.LessThan(expectedFrames + 50),
            "Transition should complete within expected frame budget");
        Assert.That(playerController.isSwitchingLane, Is.False,
            "Transition should complete after simulation");
        Assert.That(playerObject.transform.position.x, Is.EqualTo(LeftX).Within(tolerance),
            "Player X must reach left lane position after transition");
        Assert.That(playerController.currentLane, Is.EqualTo(0),
            "currentLane must be 0 after completing left transition");
    }

    /// <summary>
    /// Triangulation: full transition from center to right.
    /// </summary>
    [Test]
    public void FullTransitionCenterToRight()
    {
        Vector3 startPos = new Vector3(CenterX, 1f, 10f);
        playerObject.transform.position = startPos;

        playerController.StartLaneTransition(2); // target is (5, 0, 0)

        int frames = SimulateFrames(0.02f, 500);

        Assert.That(playerController.isSwitchingLane, Is.False,
            "Transition should complete after simulation");
        Assert.That(playerObject.transform.position.x, Is.EqualTo(RightX).Within(playerConfig.movementTolerance),
            "Player X must reach right lane position after transition");
        Assert.That(playerController.currentLane, Is.EqualTo(2),
            "currentLane must be 2 after completing right transition");
    }

    /// <summary>
    /// Spec: "Buffered input fires after transition completes."
    /// Starts a transition, buffers a same-direction input, simulates frames,
    /// and verifies the buffered input executes automatically.
    /// </summary>
    [Test]
    public void BufferedInputFiresAfterTransition()
    {
        Vector3 startPos = new Vector3(CenterX, 1f, 10f);
        playerObject.transform.position = startPos;

        // Start moving left (center → left). currentLane=1.
        playerController.StartLaneTransition(0);

        // Same-direction input while mid-transition → buffer another left.
        // currentLane is still 1, direction -1 → targetLane = clamp(1-1,0,2) = 0.
        playerController.ProcessLaneInput(-0.8f);
        Assert.That(playerController.bufferedLane, Is.EqualTo(0),
            "Buffer should contain leftward lane index (0)");

        // Simulate frames. The first transition goes center→left (lane 0).
        // On snap, buffer fires: currentLane=0, direction -1 → target clamp(0-1,0,2)=0.
        // Since target==currentLane at boundary, buffered input is a no-op.
        // So the player arrives at lane 0 and stays there.
        int frames = SimulateFrames(0.02f, 500);

        Assert.That(frames, Is.LessThan(500),
            "Transition should complete within frame budget");
        Assert.That(playerController.isSwitchingLane, Is.False,
            "Transition should have completed");
        Assert.That(playerController.currentLane, Is.EqualTo(0),
            "Final lane should be 0 (left)");
        Assert.That(playerController.bufferedLane.HasValue, Is.False,
            "Buffer should be cleared after firing");
    }

    /// <summary>
    /// Spec: "Only X-axis is modified during lane switch."
    /// Verifies Y and Z remain unchanged through simulated frames.
    /// </summary>
    [Test]
    public void YAndZPositionPreservedDuringTransition()
    {
        float startY = 1.5f;
        float startZ = 8f;
        Vector3 startPos = new Vector3(CenterX, startY, startZ);
        playerObject.transform.position = startPos;

        playerController.StartLaneTransition(2); // moving toward (RightX, 0, 0)

        // Simulate several frames and verify Y/Z unchanged
        for (int i = 0; i < 20; i++)
        {
            playerController.UpdateLaneMovement(0.02f);

            Assert.That(playerObject.transform.position.y, Is.EqualTo(startY).Within(0.001f),
                $"Y position must be preserved at frame {i}");
            Assert.That(playerObject.transform.position.z, Is.EqualTo(startZ).Within(0.001f),
                $"Z position must be preserved at frame {i}");
        }

        // Finish the transition
        int remaining = 0;
        while (playerController.isSwitchingLane && remaining < 500)
        {
            playerController.UpdateLaneMovement(0.02f);
            remaining++;
        }

        Assert.That(playerObject.transform.position.y, Is.EqualTo(startY).Within(0.001f),
            "Y position must be preserved after transition completes");
        Assert.That(playerObject.transform.position.z, Is.EqualTo(startZ).Within(0.001f),
            "Z position must be preserved after transition completes");
    }

    /// <summary>
    /// Verifies the player cannot move past lane boundaries even with rapid input.
    /// </summary>
    [Test]
    public void RapidInputAtBoundaryDoesNotLeaveBounds()
    {
        // Teleport to leftmost lane
        playerObject.transform.position = new Vector3(LeftX, 0, 0);
        playerController.currentLane = 0;
        playerController.targetPosition = leftPos.position;

        // Send rapid left inputs (no frame simulation needed)
        for (int i = 0; i < 5; i++)
        {
            playerController.ProcessLaneInput(-0.8f);
        }

        Assert.That(playerController.currentLane, Is.EqualTo(0),
            "Rapid left input at boundary must keep lane at 0");
        Assert.That(playerController.isSwitchingLane, Is.False,
            "No transition should start from boundary");
    }

    // ──────────────────────────────────────────────
    //  Jump Gating (Task 3.2 — RED / Task 2.5 — GREEN)
    // ──────────────────────────────────────────────

    /// <summary>
    /// Spec: "Jump MUST fire when not already jumping (performed action)."
    /// Verifies TryPerformJump returns true and sets isJumping.
    /// </summary>
    [Test]
    public void JumpFiresOnPerformedWhenNotJumping()
    {
        bool jumped = playerController.TryPerformJump();

        Assert.That(jumped, Is.True,
            "TryPerformJump should return true when not jumping");
        Assert.That(playerController.isJumping, Is.True,
            "isJumping should be set to true after successful jump");
    }

    /// <summary>
    /// Spec: "Jump MUST be ignored when isJumping is already true (double-tap guard)."
    /// </summary>
    [Test]
    public void JumpIgnoredWhenAlreadyJumping()
    {
        // First jump succeeds
        playerController.TryPerformJump();
        Assert.That(playerController.isJumping, Is.True, "First jump should succeed");

        // Capture jumpStartY after first jump
        float startY = playerObject.transform.position.y;

        // Second attempt should be ignored
        bool jumpedAgain = playerController.TryPerformJump();

        Assert.That(jumpedAgain, Is.False,
            "TryPerformJump should return false when already jumping");
        Assert.That(playerController.isJumping, Is.True,
            "isJumping should remain true after rejected jump");
        // Y should not have been reset (jumpStartY unchanged means jump continues)
        Assert.That(playerObject.transform.position.y, Is.EqualTo(startY).Within(0.001f),
            "Position Y should not change when jump is rejected");
    }

    /// <summary>
    /// Triangulation: After landing (isJumping cleared), a new jump is allowed.
    /// </summary>
    [Test]
    public void JumpAllowedAfterLanding()
    {
        // First jump
        Assert.That(playerController.TryPerformJump(), Is.True, "First jump");
        // Complete the jump
        playerController.UpdateJump(0.5f);
        Assert.That(playerController.isJumping, Is.False, "isJumping cleared after landing");

        // Second jump should be allowed
        bool jumpedAgain = playerController.TryPerformJump();
        Assert.That(jumpedAgain, Is.True,
            "TryPerformJump should return true after landing (isJumping cleared)");
        Assert.That(playerController.isJumping, Is.True,
            "isJumping should be set again for second jump");
    }

    // ──────────────────────────────────────────────
    //  Jump Arc Math (Task 3.3 — RED / Task 2.6 — GREEN)
    // ──────────────────────────────────────────────

    /// <summary>
    /// Spec: "The jump arc MUST peak at exactly jumpHeight above jumpStartY at the midpoint."
    /// Parabola: 4*t*(1-t) peaks at t=0.5 with value 1.0 → yOffset = jumpHeight.
    /// </summary>
    [Test]
    public void JumpArcPeaksAtMidpoint()
    {
        float startY = 1.5f;
        playerObject.transform.position = new Vector3(0, startY, 0);
        playerController.TryPerformJump();

        // Advance to midpoint (t = 0.25 / 0.5 = 0.5)
        playerController.UpdateJump(0.25f);

        float expectedY = startY + playerConfig.jumpHeight; // 4 * 0.5 * 0.5 = 1.0 multiplier
        float actualY = playerObject.transform.position.y;

        Assert.That(actualY, Is.EqualTo(expectedY).Within(0.001f),
            $"At midpoint, Y should be startY + jumpHeight ({expectedY}), but was {actualY}");
    }

    /// <summary>
    /// Spec: "The jump arc MUST land at jumpStartY when t >= 1.0."
    /// </summary>
    [Test]
    public void JumpLandsAtStartY()
    {
        float startY = 2.0f;
        playerObject.transform.position = new Vector3(0, startY, 0);
        playerController.TryPerformJump();

        // Advance past full duration
        playerController.UpdateJump(0.5f);

        Assert.That(playerObject.transform.position.y, Is.EqualTo(startY).Within(0.001f),
            "Y should snap back to startY after full duration");
        Assert.That(playerController.isJumping, Is.False,
            "isJumping should be false after landing");
    }

    /// <summary>
    /// Spec: "t MUST be clamped to 1.0 when deltaTime > jumpDuration."
    /// </summary>
    [Test]
    public void JumpArcClampsTGreaterThanOne()
    {
        float startY = 1.0f;
        playerObject.transform.position = new Vector3(0, startY, 0);
        playerController.TryPerformJump();

        // Pass deltaTime >> jumpDuration
        playerController.UpdateJump(1.0f);

        Assert.That(playerObject.transform.position.y, Is.EqualTo(startY).Within(0.001f),
            "Y should snap to startY even with deltaTime > jumpDuration");
        Assert.That(playerController.isJumping, Is.False,
            "isJumping should be cleared after massive deltaTime");
    }

    /// <summary>
    /// Triangulation: Very small deltaTime produces a very small Y offset (near start).
    /// </summary>
    [Test]
    public void JumpArcSmallDeltaTimeProducesSmallOffset()
    {
        float startY = 1.0f;
        playerObject.transform.position = new Vector3(0, startY, 0);
        playerController.TryPerformJump();

        // Advance a tiny amount (t ≈ 0.02 / 0.5 = 0.04)
        playerController.UpdateJump(0.02f);

        float actualY = playerObject.transform.position.y;
        Assert.That(actualY, Is.GreaterThan(startY),
            "Y should be slightly above startY after a small deltaTime");
        Assert.That(actualY, Is.LessThan(startY + playerConfig.jumpHeight),
            "Y should be well below peak after a very small deltaTime");
        Assert.That(playerController.isJumping, Is.True,
            "Should still be jumping after small deltaTime");
    }

    // ──────────────────────────────────────────────
    //  Jump + Lane Integration (Task 3.4 — RED / Phase 2 — GREEN)
    // ──────────────────────────────────────────────

    /// <summary>
    /// Spec: "Jumping mid-lane-switch MUST NOT affect lane X movement."
    /// Jump mid-transition: Y arcs while X continues toward target.
    /// </summary>
    [Test]
    public void JumpDuringLaneTransitionPreservesY()
    {
        float startY = 1.0f;
        Vector3 startPos = new Vector3(CenterX, startY, 10f);
        playerObject.transform.position = startPos;

        // Start lane switch to right
        playerController.StartLaneTransition(2);
        Assert.That(playerController.isSwitchingLane, Is.True, "Should be mid-transition");

        // Jump mid-transition
        playerController.TryPerformJump();
        Assert.That(playerController.isJumping, Is.True, "Should be jumping");

        // Simulate frames — Y should arc while X moves toward target
        bool yWentUp = false;
        bool xMovedRight = false;
        for (int i = 0; i < 30; i++)
        {
            playerController.UpdateLaneMovement(0.02f);
            playerController.UpdateJump(0.02f);

            float currentY = playerObject.transform.position.y;

            // Early frames: Y should be above startY
            if (i < 15 && playerController.isJumping)
            {
                Assert.That(currentY, Is.GreaterThanOrEqualTo(startY),
                    $"Frame {i}: Y should be >= startY during jump arc");
                if (currentY > startY + 0.001f)
                    yWentUp = true;
            }

            if (playerObject.transform.position.x > CenterX + 0.001f)
                xMovedRight = true;
        }

        Assert.That(yWentUp, Is.True, "Y should have gone up during jump arc");
        Assert.That(xMovedRight, Is.True, "X should have moved right during lane transition");
    }

    /// <summary>
    /// Spec: "Lane-switching mid-jump MUST NOT affect the Y jump arc."
    /// Jump first, then switch lanes mid-air. Y arcs unaffected by X movement.
    /// </summary>
    [Test]
    public void LaneSwitchDuringJumpPreservesY()
    {
        float startY = 1.0f;
        Vector3 startPos = new Vector3(CenterX, startY, 10f);
        playerObject.transform.position = startPos;

        // Jump first
        playerController.TryPerformJump();
        Assert.That(playerController.isJumping, Is.True, "Should be jumping");

        // Advance partway through jump (t ≈ 0.15/0.5 = 0.3)
        playerController.UpdateJump(0.15f);
        float yMidArc = playerObject.transform.position.y;
        Assert.That(yMidArc, Is.GreaterThan(startY),
            "Y should be above startY after partial jump");

        // Now start lane switch mid-air
        playerController.StartLaneTransition(2);
        Assert.That(playerController.isSwitchingLane, Is.True, "Should be mid-transition");

        // Simulate frames — Y should continue arc while X moves
        for (int i = 0; i < 20; i++)
        {
            playerController.UpdateLaneMovement(0.02f);
            playerController.UpdateJump(0.02f);

            // X should be moving right
            if (i > 0)
            {
                Assert.That(playerObject.transform.position.x,
                    Is.GreaterThan(CenterX).Or.EqualTo(CenterX),
                    $"Frame {i}: X should have started moving right");
            }
        }

        // Complete the jump
        while (playerController.isJumping)
            playerController.UpdateJump(0.02f);

        Assert.That(playerObject.transform.position.y, Is.EqualTo(startY).Within(0.001f),
            "Y should return to startY after jump completes");

        // Complete the lane switch if still transitioning
        while (playerController.isSwitchingLane)
            playerController.UpdateLaneMovement(0.02f);

        Assert.That(playerObject.transform.position.x, Is.EqualTo(RightX).Within(playerConfig.movementTolerance),
            "X should have moved to right lane after full transition");
    }

    // ──────────────────────────────────────────────
    //  Fast-Fall (Task 3.1–3.6 — RED / Phase 2 — GREEN)
    // ──────────────────────────────────────────────

    /// <summary>
    /// Spec: "Holding down mid-jump during descent (t > 0.5) MUST accelerate the fall."
    /// When isFastFalling is true and we're past the peak, effectiveDt is multiplied by fastFallMultiplier.
    /// </summary>
    [Test]
    public void FastFallAcceleratesDescent()
    {
        float startY = 1.0f;
        playerObject.transform.position = new Vector3(0, startY, 0);
        playerController.TryPerformJump();

        // Advance past the peak into descent (t ≈ 0.3 / 0.5 = 0.6)
        playerController.UpdateJump(0.3f);

        // Set fast-fall and advance with a known delta
        playerController.isFastFalling = true;
        playerController.UpdateJump(0.05f); // effectiveDt = 0.05 * 2 = 0.1

        // Expected: jumpTimer = 0.3 + 0.1 = 0.4, t = 0.4 / 0.5 = 0.8
        // yOffset = jumpHeight * 4 * 0.8 * (1 - 0.8) = jumpHeight * 0.64
        float expectedY = startY + playerConfig.jumpHeight * 0.64f;
        float actualY = playerObject.transform.position.y;

        Assert.That(actualY, Is.EqualTo(expectedY).Within(0.001f),
            $"With fast-fall active in descent, Y should be {expectedY} but was {actualY}");
    }

    /// <summary>
    /// Spec: "Releasing the down input mid-descent MUST restore normal descent speed."
    /// When isFastFalling transitions from true to false, subsequent UpdateJump calls
    /// should use regular deltaTime.
    /// </summary>
    [Test]
    public void FastFallClearedMidDescentRestoresNormalSpeed()
    {
        float startY = 1.0f;
        playerObject.transform.position = new Vector3(0, startY, 0);
        playerController.TryPerformJump();

        // Advance to descent (t = 0.3 / 0.5 = 0.6)
        playerController.UpdateJump(0.3f);

        // Fast-fall for one step
        playerController.isFastFalling = true;
        playerController.UpdateJump(0.05f); // effectiveDt = 0.1 → t = 0.8

        // Release fast-fall
        playerController.isFastFalling = false;
        playerController.UpdateJump(0.05f); // normal Dt = 0.05 → t = 0.85

        // Expected: jumpTimer = 0.3 + 0.1 + 0.05 = 0.45, t = 0.45 / 0.5 = 0.9
        // yOffset = 3 * 4 * 0.9 * 0.1 = 1.08
        float expectedY = startY + playerConfig.jumpHeight * 4f * 0.9f * 0.1f;
        float actualY = playerObject.transform.position.y;

        Assert.That(actualY, Is.EqualTo(expectedY).Within(0.001f),
            $"After releasing fast-fall mid-descent, Y should be {expectedY} but was {actualY}");
        Assert.That(playerController.isJumping, Is.True,
            "Should still be jumping after partial fast-fall");
    }

    /// <summary>
    /// Spec: "Fast-fall MUST NOT affect ascent (t ≤ 0.5) — only descent."
    /// When isFastFalling is true but we're still rising, effectiveDt should be plain deltaTime.
    /// </summary>
    [Test]
    public void FastFallIgnoredDuringAscent()
    {
        float startY = 1.0f;
        playerObject.transform.position = new Vector3(0, startY, 0);
        playerController.TryPerformJump();

        // Advance a small amount while still in ascent (t = 0.1 / 0.5 = 0.2)
        playerController.isFastFalling = true; // should be ignored during ascent
        playerController.UpdateJump(0.1f);     // plain dt, NOT multiplied

        // Expected: jumpTimer = 0.1, t = 0.2
        // yOffset = 3 * 4 * 0.2 * 0.8 = 1.92
        float expectedY = startY + playerConfig.jumpHeight * 4f * 0.2f * 0.8f;
        float actualY = playerObject.transform.position.y;

        Assert.That(actualY, Is.EqualTo(expectedY).Within(0.001f),
            $"During ascent with fast-fall set, Y should be {expectedY} but was {actualY}");
        Assert.That(playerController.isJumping, Is.True,
            "Should still be jumping during ascent");
    }

    /// <summary>
    /// Spec: "When isFastFalling is true and t > 0.5, the timer MUST advance faster."
    /// Verifies the multiplier effect produces the exact expected jumpTimer and position.
    /// This is the core math verification for the fast-fall feature.
    /// </summary>
    [Test]
    public void FastFallMultiplierAdvancesTimerFaster()
    {
        float startY = 1.0f;
        playerObject.transform.position = new Vector3(0, startY, 0);
        playerController.TryPerformJump();

        // Advance to descent (t = 0.3 / 0.5 = 0.6)
        playerController.UpdateJump(0.3f);

        // Apply fast-fall and update with 0.05f → effectiveDt = 0.05 * 2 = 0.1
        playerController.isFastFalling = true;
        playerController.UpdateJump(0.05f);

        // Expected: jumpTimer = 0.3 + 0.1 = 0.4, t = 0.4 / 0.5 = 0.8
        // yOffset = jumpHeight * 4 * 0.8 * (1 - 0.8) = jumpHeight * 0.64
        float expectedY = playerController.jumpStartY + playerConfig.jumpHeight * 0.64f;
        float actualY = playerObject.transform.position.y;

        Assert.That(actualY, Is.EqualTo(expectedY).Within(0.001f),
            $"Fast-fall should advance timer by effectiveDt; expected Y {expectedY} but was {actualY}");
    }

    /// <summary>
    /// Spec: "Landing MUST still work correctly when fast-fall is held throughout the entire descent."
    /// When fast-fall is active from peak to landing, the jump should complete and snap to startY.
    /// </summary>
    [Test]
    public void LandingWorksWithFastFallHeldThroughout()
    {
        float startY = 1.0f;
        playerObject.transform.position = new Vector3(0, startY, 0);
        playerController.TryPerformJump();

        // Advance to just past peak
        playerController.UpdateJump(0.26f); // t ≈ 0.52 — just into descent

        // Fast-fall for the rest of the jump
        playerController.isFastFalling = true;
        // From t=0.52 to t=1.0 with multiplier 2x: need (1.0 - 0.52) / 2 = 0.24 real seconds
        // But the jump will land when t >= 1.0
        playerController.UpdateJump(0.24f);
        // With fast-fall, effectiveDt = 0.24 * 2 = 0.48, so jumpTimer = 0.26 + 0.48 = 0.74, t = 1.48 ≥ 1.0

        Assert.That(playerController.isJumping, Is.False,
            "Jump should have completed with fast-fall held throughout");
        Assert.That(playerObject.transform.position.y, Is.EqualTo(startY).Within(0.001f),
            "Player should snap back to startY after landing with fast-fall");
    }

    /// <summary>
    /// Spec: "Landing MUST still work when fast-fall is active then released mid-descent."
    /// Fast-fall speeds up part of the descent, then normal speed finishes the jump to landing.
    /// </summary>
    [Test]
    public void LandingWorksWithFastFallReleasedMidDescent()
    {
        float startY = 1.0f;
        playerObject.transform.position = new Vector3(0, startY, 0);
        playerController.TryPerformJump();

        // Advance to descent (t = 0.3 / 0.5 = 0.6)
        playerController.UpdateJump(0.3f);

        // Fast-fall for a step (effectiveDt = 0.05 * 2 = 0.1 → t = 0.8)
        playerController.isFastFalling = true;
        playerController.UpdateJump(0.05f);

        // Release fast-fall — subsequent steps use normal dt
        playerController.isFastFalling = false;
        playerController.UpdateJump(0.05f); // t = 0.85

        // Finish the jump with normal speed
        playerController.UpdateJump(0.15f); // t = 1.0 → LANDING

        Assert.That(playerController.isJumping, Is.False,
            "Jump should land after fast-fall released mid-descent");
        Assert.That(playerObject.transform.position.y, Is.EqualTo(startY).Within(0.001f),
            "Player must be at startY after landing (fast-fall released mid-descent)");
    }
}
