using UnityEngine;

namespace Mirror.Examples.RigidbodyPhysics
{
    public class AddForce : NetworkBehaviour
    {
        public float force = 500f;
        public Rigidbody rigidbody3d;

        private void Start()
        {
            rigidbody3d.isKinematic = !isServer;
        }

        private void Update()
        {
            if (isServer && Input.GetKeyDown(KeyCode.Space)) rigidbody3d.AddForce(Vector3.up * force);
        }
    }
}