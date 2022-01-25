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

    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;

    void Update() {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void RecoilFire() {
        targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
    }
    
    public void RecoilFireZoom() {
        targetRotation += new Vector3(aimRecoilX, Random.Range(-aimRecoilY, aimRecoilY), Random.Range(-aimRecoilZ, aimRecoilZ));
    }
}