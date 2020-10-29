using UnityEngine;
/// <summary>
/// Moves the player left and right by input
/// </summary>
public class PlayerMovement : MonoBehaviour
{
	private float movementTolerance;
	private float movementSpeed;

	private Vector3[] linesPositions;
	private int currentLineIndex;
	private int targetLineIndex = -1;

	private CharacterController characterController;

	void Start()
	{
		movementTolerance = ConfigurationUtils.PlayerMovementTolerance;
		movementSpeed = ConfigurationUtils.PlayerMovementSpeed;
		linesPositions = ConfigurationUtils.LinesPositions;
		currentLineIndex = linesPositions.Length / 2;
		transform.position = new Vector3(linesPositions[currentLineIndex].x, transform.position.y, transform.position.z);

		characterController = gameObject.GetComponent<CharacterController>();
	}

	void Update()
	{
        if (targetLineIndex < 0)
        {
			CheckInput();
		}
        else
        {
			MovePlayer();
		}
	}

	private void CheckInput()
	{
		float horizontalAxis = Input.GetAxis(GameInitializer.HorizontalAxis);
        if (horizontalAxis != 0)
        {
			targetLineIndex = currentLineIndex + (int)Mathf.Sign(horizontalAxis);
			if(targetLineIndex < 0 || targetLineIndex >= linesPositions.Length)
            {
				targetLineIndex = -1;
				return;
			}
		}
	}

	private void MovePlayer()
    {
		Vector3 currentPosition = transform.position;
		Vector3 moveDirection = new Vector3(linesPositions[targetLineIndex].x - currentPosition.x, 0, 0);

		if(moveDirection.magnitude > movementTolerance)
        {
			characterController.Move(moveDirection.normalized * movementSpeed * Time.deltaTime);
		}
        else{
			transform.position = new Vector3(linesPositions[targetLineIndex].x, currentPosition.y, currentPosition.z);
			currentLineIndex = targetLineIndex;
			targetLineIndex = -1;
		}
	}
}
