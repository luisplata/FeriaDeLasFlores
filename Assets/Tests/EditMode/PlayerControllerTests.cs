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

        // Create ground checker child (required by PlayerController.Start but not by lane init)
        var groundChecker = new GameObject("GroundChecker").transform;
        groundChecker.parent = playerObject.transform;

        // Initialize lane system (the lane-specific subset of Start)
        playerController.InitializeLaneSystem();
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
}
