using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : IntEventInvoker
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

    private int lifes = 1;
    [SerializeField]
    private GameObject silleta;
    private bool canReceiveDamage = true;
    private GameOverEvent gameOverEvent = new GameOverEvent();

    void Start()
    {
        speed = ConfigurationUtils.PlayerMovementSpeed;
        jumpHeight = ConfigurationUtils.PlayerJumpHeight;

        rigidBody = GetComponent<Rigidbody>();
        groundChecker = transform.GetChild(0);

        unityEvents.Add(EventName.GameOverEvent, gameOverEvent);
        EventManager.AddInvoker(EventName.GameOverEvent, this);
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

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rigidBody.AddForce(Vector3.up * Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y), ForceMode.VelocityChange);
            AudioManager.Play(AudioClipName.Jump);
        }
        else if (Input.GetButtonDown("Fall") && !isGrounded)
        {
            rigidBody.AddForce(Vector3.down * Mathf.Sqrt(1.5f * jumpHeight * -2f * Physics.gravity.y), ForceMode.VelocityChange);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Obstacle")
        {
            collision.gameObject.SetActive(false);
            if (!canReceiveDamage)
            {
                return;
            }

            if(lifes <= 0)
            {
                gameOverEvent.Invoke(0);
                return;
            }

            lifes -= 1;
            StartCoroutine(DamageAnimation());
            AudioManager.Play(AudioClipName.Crash);
        }
    }

    private IEnumerator DamageAnimation()
    {
        canReceiveDamage = false;
        for(int i = 0; i < 10; i++)
        {
            silleta.SetActive(false);
            yield return new WaitForSeconds(0.05f);
            silleta.SetActive(true);
            yield return new WaitForSeconds(0.05f);
        }
        canReceiveDamage = true;
    }
}