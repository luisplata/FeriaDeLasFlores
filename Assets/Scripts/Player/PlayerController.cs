using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{   
    [SerializeField]
    private float GroundDistance = 0.7f;
    [SerializeField]
    private LayerMask Ground;

    private float speed = 5f;
    private float jumpHeight = 2f;
    
    private Rigidbody rigidBody;
    private Vector3 inputs = Vector3.zero;
    public bool isGrounded = true;
    private Transform groundChecker;

    void Start()
    {
        speed = ConfigurationUtils.PlayerMovementSpeed;
        jumpHeight = ConfigurationUtils.PlayerJumpHeight;

        rigidBody = GetComponent<Rigidbody>();
        groundChecker = transform.GetChild(0);
    }

    private void Update()
    {
        CheckGround();
        CheckInputs();
    }

    private void FixedUpdate()
    {
        rigidBody.MovePosition(rigidBody.position + inputs * speed * Time.fixedDeltaTime);
    }

    private void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundChecker.position, GroundDistance, Ground, QueryTriggerInteraction.Ignore);
    }

    private void CheckInputs()
    {
        inputs = Vector3.zero;
        inputs.x = Input.GetAxis("Horizontal");

        if (Input.GetButtonDown("Vertical") && isGrounded)
        {
            rigidBody.AddForce(Vector3.up * Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y), ForceMode.VelocityChange);
            AudioManager.Play(AudioClipName.Jump);
        }
        else if (Input.GetButtonDown("Vertical") && !isGrounded)
        {
            rigidBody.AddForce(Vector3.down * Mathf.Sqrt(1.5f * jumpHeight * -2f * Physics.gravity.y), ForceMode.VelocityChange);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Obstacle")
        {
            rigidBody.velocity = Vector3.zero;
            transform.SetParent(collision.gameObject.transform.parent);
        }
    }
}