﻿using Mirror;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class FPSController : NetworkBehaviour {
    [HideInInspector] public bool canMove = true;

    private CharacterController characterController;
    public float gravity = 20.0f;
    public float jumpSpeed = 8.0f;
    private float currentLookSpeed = 2f;
    public float lookXLimit = 90.0f;
    private Vector3 moveDirection = Vector3.zero;
    public Camera playerCamera;
    private float rotationX;
    public float slowWalkSpeed = 7f;
    public float walkingSpeed = 11.5f;
    [SerializeField] private Animator animator;
    [SerializeField] private Animator fpAnimator;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] jumpClips;

    private void Start() {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private void Update() {
        
        if (GameManager.instance.gameState == GameManager.GameState.InGame && isLocalPlayer) {
            var forward = transform.TransformDirection(Vector3.forward);
            var right = transform.TransformDirection(Vector3.right);

            var isWalkingSlow = Input.GetKey(KeyCode.LeftShift);
            var curSpeedX = canMove ? (isWalkingSlow ? slowWalkSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
            var curSpeedY = canMove ? (isWalkingSlow ? slowWalkSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
            var movementDirectionY = moveDirection.y;
            moveDirection = forward * curSpeedX + right * curSpeedY;

            if (Input.GetKeyDown(KeyCode.LeftShift)) {
                animator.SetBool("SlowWalk", true);
            }
            
            if (Input.GetKeyUp(KeyCode.LeftShift)) {
                animator.SetBool("SlowWalk", false);
            }
            
            animator.SetFloat("Vertical", Input.GetAxis("Vertical"));
            animator.SetFloat("Horizontal", Input.GetAxis("Horizontal"));
            fpAnimator.SetFloat("Vertical", curSpeedX);
            fpAnimator.SetFloat("Horizontal", curSpeedY);
            if (curSpeedX != 0 || curSpeedY != 0) {
                fpAnimator.SetFloat("Walk", 1);
            }
            else 
                fpAnimator.SetFloat("Walk", 0);
            
            if (Input.GetButton("Jump") && canMove && characterController.isGrounded) {
                moveDirection.y = jumpSpeed;
                var index = Random.Range(0, jumpClips.Length);
                audioSource.clip = jumpClips[index];
                audioSource.Play();
            }
            else {
                moveDirection.y = movementDirectionY;
            }

            if (Input.GetButton("Jump") && canMove && characterController.isGrounded) {
                animator.SetTrigger("Jump");
            }

            if (!characterController.isGrounded) moveDirection.y -= gravity * Time.deltaTime;

            characterController.Move(moveDirection * Time.deltaTime);

            if (canMove) {
                rotationX += -Input.GetAxis("Mouse Y") * currentLookSpeed;
                rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
                playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
                transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * currentLookSpeed, 0);
            }
        }

        if (isLocalPlayer) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                GameEvents.BroadcastOnEscClick();
            }
        }
    }
}