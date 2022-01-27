using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Recoil : MonoBehaviour {

    private Vector3 currentRotation;
    private Vector3 targetRotation;

    [SerializeField] private float recoilX;
    [SerializeField] private float recoilY;
    [SerializeField] private float recoilZ;

    [SerializeField] private float aimRecoilX;
    [SerializeField] private float aimRecoilY;
    [SerializeField] private float aimRecoilZ;
    
    [SerializeField] private float isMovingMultiplyX = 2f;
    [SerializeField] private float isMovingMultiplyY = 2f;
    [SerializeField] private float isMovingMultiplyZ = 1.3f;

    [SerializeField] private float NotGroundedMultiplyX = 3f;
    [SerializeField] private float NotGroundedMultiplyY = 3f;
    [SerializeField] private float NotGroundedMultiplyZ = 1.6f;

    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;

    [SerializeField] private FPSController fpsController;
    [SerializeField] private CharacterController characterController;

    void Update() {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void RecoilFire() {
        if (fpsController.isMoving && characterController.isGrounded) {
            targetRotation += new Vector3(recoilX * isMovingMultiplyX, Random.Range(-recoilY * isMovingMultiplyY, recoilY * isMovingMultiplyY), Random.Range(-recoilZ * isMovingMultiplyZ, recoilZ * isMovingMultiplyZ));
        } else if (!characterController.isGrounded) {
            targetRotation += new Vector3(recoilX * NotGroundedMultiplyX, Random.Range(-recoilY * NotGroundedMultiplyY, recoilY * NotGroundedMultiplyY), Random.Range(-recoilZ * NotGroundedMultiplyZ, recoilZ * NotGroundedMultiplyZ));
        }
        else {
            targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
        }
    }
    
    public void RecoilFireZoom() {
        if (fpsController.isMoving && characterController.isGrounded) {
            targetRotation += new Vector3(aimRecoilX * isMovingMultiplyX, Random.Range(-aimRecoilY * isMovingMultiplyY, aimRecoilY * isMovingMultiplyY), Random.Range(-aimRecoilZ * isMovingMultiplyZ, aimRecoilZ * isMovingMultiplyZ));
        } else if (!characterController.isGrounded) {
            targetRotation += new Vector3(aimRecoilX * NotGroundedMultiplyX, Random.Range(-aimRecoilY * NotGroundedMultiplyY, aimRecoilY * NotGroundedMultiplyY), Random.Range(-aimRecoilZ * NotGroundedMultiplyZ, aimRecoilZ * NotGroundedMultiplyZ));
        }
        else {
            targetRotation += new Vector3(aimRecoilX, Random.Range(-aimRecoilY, aimRecoilY), Random.Range(-aimRecoilZ, aimRecoilZ));
        }
    }
}