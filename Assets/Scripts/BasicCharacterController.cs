
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class BasicCharacterController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float runSpeed = 6.5f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;

    [Header("Camera")]
    [SerializeField] private float cameraDistance = 4f;
    [SerializeField] private float cameraHeight = 1.5f;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float verticalLookLimit = 80f;

    private CharacterController characterController;
    private Vector3 verticalVelocity;

    private Transform cameraPivot;
    private Camera playerCamera;
    private float cameraPitch;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        SetupCamera();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        HandleMouseLook();
        HandleMovement();
        HandleCursor();
    }

    private void SetupCamera()
    {
        playerCamera = Camera.main;

        if (playerCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            playerCamera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<AudioListener>();
        }

        GameObject pivotObject = new GameObject("CameraPivot");
        cameraPivot = pivotObject.transform;

        cameraPivot.SetParent(transform);
        cameraPivot.localPosition = Vector3.up * cameraHeight;
        cameraPivot.localRotation = Quaternion.identity;

        playerCamera.transform.SetParent(cameraPivot);
        playerCamera.transform.localPosition = new Vector3(0f, 0f, -cameraDistance);
        playerCamera.transform.localRotation = Quaternion.identity;
    }

    private void HandleMovement()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current.wKey.isPressed)
            input.y += 1f;

        if (Keyboard.current.sKey.isPressed)
            input.y -= 1f;

        if (Keyboard.current.dKey.isPressed)
            input.x += 1f;

        if (Keyboard.current.aKey.isPressed)
            input.x -= 1f;

        input = Vector2.ClampMagnitude(input, 1f);

        bool isRunning = Keyboard.current.leftShiftKey.isPressed;
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 movement = transform.right * input.x + transform.forward * input.y;
        movement *= currentSpeed;

        if (characterController.isGrounded && verticalVelocity.y < 0f)
            verticalVelocity.y = -2f;

        if (characterController.isGrounded && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("Jump");
        }

        verticalVelocity.y += gravity * Time.deltaTime;

        Vector3 velocity = movement + verticalVelocity;
        characterController.Move(velocity * Time.deltaTime);
        animator.SetFloat("Blend", movement.magnitude);
    }

    private void HandleMouseLook()
    {
        if (Mouse.current == null || Cursor.lockState != CursorLockMode.Locked)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseDelta.x);

        cameraPitch -= mouseDelta.y;
        cameraPitch = Mathf.Clamp(cameraPitch, -verticalLookLimit, verticalLookLimit);

        cameraPivot.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void HandleCursor()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            Cursor.lockState = CursorLockMode.None;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            Cursor.lockState = CursorLockMode.Locked;
    }
}
