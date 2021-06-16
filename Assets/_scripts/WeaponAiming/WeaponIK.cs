using Mirror;
using UnityEngine;

public class WeaponIK : NetworkBehaviour
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform aimTransform;
    [SerializeField] private Transform bone;

    private void Update()
    {
        if (isLocalPlayer)
        {
            var targetPosition = targetTransform.position;
            AimAtTarget(bone, targetPosition);
        }
    }

    private void AimAtTarget(Transform bone, Vector3 targetPosition)
    {
        var aimDirection = aimTransform.forward;
        var targetDirection = targetPosition - aimTransform.position;
        Quaternion aimTowards = Quaternion.FromToRotation(aimDirection, targetDirection);
        bone.rotation = aimTowards * bone.rotation;
    }
}