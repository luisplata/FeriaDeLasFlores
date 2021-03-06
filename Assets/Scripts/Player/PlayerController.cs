﻿using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [SerializeField]
    private int lifes = 1;
    [SerializeField]
    private GameObject silleta;
    private bool canReceiveDamage = true;
    private GameOverEvent gameOverEvent = new GameOverEvent();

    private EnvironmentName currentEnvironment;
    private float flowerCompletionPercentage;
    private EnvironmentChangedEvent environmentChangedEvent = new EnvironmentChangedEvent();
    public int FlorEnPorcenajeParaEscribir => (int)(flowerCompletionPercentage * 100);
    public float FlorEnPorcentajeParaUi => flowerCompletionPercentage;

    private Animator animator;

    void Start()
    {
        speed = ConfigurationUtils.PlayerMovementSpeed;
        jumpHeight = ConfigurationUtils.PlayerJumpHeight;
        rigidBody = GetComponent<Rigidbody>();
        groundChecker = transform.GetChild(0);

        unityEvents.Add(EventName.GameOverEvent, gameOverEvent);
        EventManager.AddInvoker(EventName.GameOverEvent, this);

        currentEnvironment = EnvironmentName.Forest;
        unityEvents.Add(EventName.EnvironmentChangedEvent, environmentChangedEvent);
        EventManager.AddInvoker(EventName.EnvironmentChangedEvent, this);

        animator = GetComponent<Animator>();
        animator.enabled = false;
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
        else if (Input.GetButtonDown("Fall"))
        {
            rigidBody.AddForce(Vector3.down * Mathf.Sqrt(1.5f * jumpHeight * -2f * Physics.gravity.y), ForceMode.VelocityChange);
        }
    }

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
        for(int i = 0; i < 10; i++)
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
        if (currentEnvironment != EnvironmentName.Street && flowerCompletionPercentageInt >= 33 && flowerCompletionPercentageInt < 66)
        {
            currentEnvironment = EnvironmentName.Street;
            environmentChangedEvent.Invoke((int)EnvironmentName.Street);
        }
        else if(currentEnvironment != EnvironmentName.Tram && flowerCompletionPercentageInt >= 66 && flowerCompletionPercentageInt < 100)
        {
            currentEnvironment = EnvironmentName.Tram;
            environmentChangedEvent.Invoke((int)EnvironmentName.Tram);
        }
        else if(flowerCompletionPercentageInt == 100)
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