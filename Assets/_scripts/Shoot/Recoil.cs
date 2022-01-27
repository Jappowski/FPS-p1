using UnityEngine;

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

    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;

    [SerializeField] private FPSController fpsController;

    void Update() {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void RecoilFire() {
        if (fpsController.isMoving) {
            targetRotation += new Vector3(recoilX * isMovingMultiplyX, Random.Range(-recoilY * isMovingMultiplyY, recoilY * isMovingMultiplyY), Random.Range(-recoilZ * isMovingMultiplyZ, recoilZ * isMovingMultiplyZ));
        }
        else {
            targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
        }
    }
    
    public void RecoilFireZoom() {
        if (fpsController.isMoving) {
            targetRotation += new Vector3(aimRecoilX * isMovingMultiplyX, Random.Range(-aimRecoilY * isMovingMultiplyY, aimRecoilY * isMovingMultiplyY), Random.Range(-aimRecoilZ * isMovingMultiplyZ, aimRecoilZ * isMovingMultiplyZ));
        }
        else {
            targetRotation += new Vector3(aimRecoilX, Random.Range(-aimRecoilY, aimRecoilY), Random.Range(-aimRecoilZ, aimRecoilZ));
        }
    }
}