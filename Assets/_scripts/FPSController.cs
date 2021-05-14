using Mirror;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : NetworkBehaviour
{
    [HideInInspector] public bool canMove = true;

    private CharacterController characterController;
    public float gravity = 20.0f;
    public float jumpSpeed = 8.0f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;
    private Vector3 moveDirection = Vector3.zero;
    public Camera playerCamera;
    private float rotationX;
    public float slowWalkSpeed = 7f;
    public float walkingSpeed = 11.5f;

    [SerializeField] private Animator animator;
    
    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // We are grounded, so recalculate move direction based on axes
        var forward = transform.TransformDirection(Vector3.forward);

        var right = transform.TransformDirection(Vector3.right);
        // Press Left Shift to run
        var isWalkingSlow = Input.GetKey(KeyCode.LeftShift);
        var curSpeedX = canMove ? (isWalkingSlow ? slowWalkSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        var curSpeedY = canMove ? (isWalkingSlow ? slowWalkSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        var movementDirectionY = moveDirection.y;
        moveDirection = forward * curSpeedX + right * curSpeedY;

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            animator.SetBool("SlowWalk", true);
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            animator.SetBool("SlowWalk", false);
        }
        
        
        
        animator.SetFloat("Vertical", Input.GetAxis("Vertical"));
        animator.SetFloat("Horizontal", Input.GetAxis("Horizontal"));

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            animator.SetTrigger("Jump");
        }
        
        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        if (!characterController.isGrounded) moveDirection.y -= gravity * Time.deltaTime;

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }
}