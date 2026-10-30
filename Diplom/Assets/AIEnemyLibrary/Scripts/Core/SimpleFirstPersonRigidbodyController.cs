using UnityEngine;
using AIEnemyLibrary.Sensors;

[RequireComponent(typeof(Rigidbody))]
public class SimpleFirstPersonRigidbodyController : MonoBehaviour
{
    [Header("Движение")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 7f;

    [Header("Мышь")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float verticalLookLimit = 80f;

    [Header("Прыжок")]
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float groundCheckDistance = 0.35f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Transform groundCheckPoint;

    [Header("Камера")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Camera playerCamera;

    [Header("Шумы")]
    [SerializeField] private ActionNoiseEmitter actionNoiseEmitter;

    [Header("Отладка")]
    [SerializeField] private bool debugGroundCheck = false;

    private Rigidbody rb;
    private bool isGrounded;
    private float cameraPitch = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (cameraPivot == null && playerCamera != null)
            cameraPivot = playerCamera.transform.parent;

        if (actionNoiseEmitter == null)
            actionNoiseEmitter = GetComponent<ActionNoiseEmitter>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        CheckGround();
        HandleJump();
        HandleMouseLook();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.forward * vertical + transform.right * horizontal;
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        Vector3 horizontalVelocity = moveDirection * currentSpeed;
        rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            if (actionNoiseEmitter != null)
                actionNoiseEmitter.EmitJumpNoise();

            if (debugGroundCheck)
                Debug.Log("JUMP");
        }
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -verticalLookLimit, verticalLookLimit);

        if (cameraPivot != null)
            cameraPivot.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void CheckGround()
    {
        Vector3 origin = groundCheckPoint != null
            ? groundCheckPoint.position
            : transform.position + Vector3.up * 0.1f;

        isGrounded = Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundMask);

        if (debugGroundCheck)
            Debug.Log($"Grounded: {isGrounded}");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Vector3 origin = groundCheckPoint != null
            ? groundCheckPoint.position
            : transform.position + Vector3.up * 0.1f;

        Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
    }
}