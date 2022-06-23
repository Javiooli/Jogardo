using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public bool canMove { get; private set; } = true;
    private bool isSprinting => (holdToSprint && canSprint && Input.GetKey(sprintKey) && !duringCrouchAnimation) || (sprintToggled && canSprint);
    private bool shouldJump => Input.GetKeyDown(jumpKey) && characterController.isGrounded;
    private bool shouldCrouch => (timeBetweenCrouches == timeToCrouch) && (holdToCrouch ? Input.GetKey(crouchKey) && !isCrouching && characterController.isGrounded :
                                   Input.GetKeyDown(crouchKey) && !duringCrouchAnimation && characterController.isGrounded && !(holdToSprint && isSprinting));

    //[SerializeField] private Animator animator;

    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool holdToSprint = true;
    [SerializeField] private bool holdToCrouch = true;

    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 5.0f;
    [SerializeField] private float sprintSpeed = 8.0f;
    [SerializeField] private float crouchSpeed = 2.5f;
    private bool sprintToggled = false;

    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;

    [Header("Jumping Parameters")]
    [SerializeField] private float jumpForce = 10.0f;
    [SerializeField] private float gravity = 15.0f;

    [Header("Crouch Parameters")]
    [SerializeField] private float crouchingHeight = 1f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float timeToCrouch = 1f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);
    private bool isCrouching;
    private bool duringCrouchAnimation;
    private float timeBetweenCrouches;

    private Camera playerCamera;
    private CharacterController characterController;

    private Vector3 moveDirection;
    private Vector2 currentInput;

    private float rotationX = 0;

    // Start is called before the first frame update
    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        //animator = GetComponentInChildren<Animator>();
        //animator.speed = 4.15f / timeToCrouch;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
        {
            HandleMovementInput();
            HandleMouseLook();

            if (canJump)
                HandleJump();

            if (canCrouch)
                timeBetweenCrouches = Mathf.Min(Time.deltaTime + timeBetweenCrouches, timeToCrouch);
                HandleCrouch();

            ApplyFinalMovement();
        }
    }

    private void HandleMovementInput()
    {
        if (canSprint && !holdToSprint && Input.GetKeyDown(sprintKey))
        {
            sprintToggled = !sprintToggled;
        }
        currentInput = new Vector2((isCrouching ? crouchSpeed : isSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical"),
                                (isCrouching ? crouchSpeed : isSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal"));

        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y);
        if (!isCrouching && duringCrouchAnimation && moveDirection.x == 0) moveDirection += transform.TransformDirection(Vector3.forward) * 0.01f;
        moveDirection.y = moveDirectionY;
        if (!holdToSprint && moveDirection.x + moveDirection.z == 0) sprintToggled = false;
    }

    private void HandleMouseLook()
    {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
    }

    private void HandleJump()
    {
        if (shouldJump)
            moveDirection.y = jumpForce;
    }

    private void HandleCrouch()
    {
        if (shouldCrouch)
        {
            StartCoroutine(CrouchStand());
            sprintToggled = false;
        }
        else if (isCrouching && ((holdToCrouch && !Input.GetKey(crouchKey)) && !duringCrouchAnimation || !holdToCrouch && isSprinting))
        {
            StartCoroutine(CrouchStand());
        }

    }

    private void ApplyFinalMovement()
    {
        if (!characterController.isGrounded && !duringCrouchAnimation)
            moveDirection.y -= gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private IEnumerator CrouchStand()
    {
        timeBetweenCrouches = 0;
        duringCrouchAnimation = true;
        isCrouching = !isCrouching;
        //animator.SetTrigger("CtrlPressed");

        float timeElapsed = 0;
        float targetHeight = isCrouching ? crouchingHeight : standingHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? crouchingCenter : standingCenter;
        Vector3 currentCenter = characterController.center;

        while (timeElapsed < timeToCrouch)
        {
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        characterController.height = targetHeight;
        characterController.center = targetCenter;

        duringCrouchAnimation = false;
    }
}
