using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    PlayerInput playerInput;
    CharacterController characterController;
    Animator animator;

    int isWalkingHash;
    int isRunningHash;

    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 currentRunMovement;
    bool isMovementPressed;
    bool isRunPressed;
    float rotationFactorPerFrame = 15.0f;
    float runMultiplier = 3.0f;

    void Awake()
    {
        playerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");

        playerInput.CharaterControls.Move.started += onMovementInput;
        playerInput.CharaterControls.Move.canceled += onMovementInput;
        playerInput.CharaterControls.Move.performed += onMovementInput;
        playerInput.CharaterControls.Run.started += onRun;
        playerInput.CharaterControls.Run.canceled += onRun;
    }

    void onRun(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
    }

    void handleRotation()
    {
        Vector3 positionToLookAt;

        positionToLookAt.x = currentMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = currentMovement.z;

        Quaternion currentRotation = transform.rotation;

        if (isMovementPressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);

        }
    }

    void onMovementInput (InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x;
        currentMovement.z = currentMovementInput.y;
        currentRunMovement.x = currentMovementInput.x * runMultiplier;
        currentRunMovement.y = currentMovementInput.y * runMultiplier;
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }

    void handleAnimation()
    {
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);

        // start walking if movement pressed is true and not already walking
        if (isMovementPressed && !isWalking)
        {
            animator.SetBool(isWalkingHash, true);
        }
        // stop walking if isMovementPressed is false and not already walking
        else if (!isMovementPressed && isWalking)
        {
            animator.SetBool(isWalkingHash, false);
        }

        if ((isMovementPressed && isRunPressed) && !isRunning)
        {
            animator.SetBool(isRunningHash, true);
        }

        else if ((!isMovementPressed || !isRunPressed) && isRunning)
        {
            animator.SetBool(isRunningHash, false);
        }
    }
    
    void handleGravity()
    {
        if (characterController.isGrounded)
        {
            float groundGravity = -.05f;
            currentMovement.y = groundGravity;
            currentRunMovement.y = groundGravity;
        }
        else
        {
            float gravity = -9.8f;
            currentMovement.y += gravity;
            currentRunMovement += gravity;
        }
    }

    // Update is called once per frame
    void Update()
    {
        handleRotation();
        handleAnimation();

        if (isRunPressed)
        {
            characterController.Move(currentRunMovement * Time.deltaTime);
        }
        else
        {
            characterController.Move(currentMovement * Time.deltaTime);
        }
    }

    void OnEnable()
    {
        // enable the charater controls action map
        playerInput.CharaterControls.Enable();
    }

    void OnDisable()
    {
        playerInput.CharaterControls.Disable();
    }
}
