using UnityEngine;
using UnityEngine.AI;

namespace Mirror.Examples.Tanks
{
    public class Tank : NetworkBehaviour
    {
        [Header("Components")] public NavMeshAgent agent;

        public Animator animator;
        public Transform projectileMount;
        public GameObject projectilePrefab;

        [Header("Movement")] public float rotationSpeed = 100;

        [Header("Firing")] public KeyCode shootKey = KeyCode.Space;

        private void Update()
        {
            // movement for local player
            if (!isLocalPlayer) return;

            // rotate
            var horizontal = Input.GetAxis("Horizontal");
            transform.Rotate(0, horizontal * rotationSpeed * Time.deltaTime, 0);

            // move
            var vertical = Input.GetAxis("Vertical");
            var forward = transform.TransformDirection(Vector3.forward);
            agent.velocity = forward * Mathf.Max(vertical, 0) * agent.speed;
            animator.SetBool("Moving", agent.velocity != Vector3.zero);

            // shoot
            if (Input.GetKeyDown(shootKey)) CmdFire();
        }

        // this is called on the server
        [Command]
        private void CmdFire()
        {
            var projectile = Instantiate(projectilePrefab, projectileMount.position, transform.rotation);
            NetworkServer.Spawn(projectile);
            RpcOnFire();
        }

        // this is called on the tank that fired for all observers
        [ClientRpc]
        private void RpcOnFire()
        {
            animator.SetTrigger("Shoot");
        }
    }
}