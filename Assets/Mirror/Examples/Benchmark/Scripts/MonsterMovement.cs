using UnityEngine;

namespace Mirror.Examples.Benchmark
{
    public class MonsterMovement : NetworkBehaviour
    {
        private Vector3 destination;
        public float movementDistance = 20;
        public float movementProbability = 0.5f;

        private bool moving;
        public float speed = 1;
        private Vector3 start;

        public override void OnStartServer()
        {
            start = transform.position;
        }

        [ServerCallback]
        private void Update()
        {
            if (moving)
            {
                if (Vector3.Distance(transform.position, destination) <= 0.01f)
                    moving = false;
                else
                    transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
            }
            else
            {
                var r = Random.value;
                if (r < movementProbability * Time.deltaTime)
                {
                    var circlePos = Random.insideUnitCircle;
                    var dir = new Vector3(circlePos.x, 0, circlePos.y);
                    var dest = transform.position + dir * movementDistance;

                    // within move dist around start?
                    // (don't want to wander off)
                    if (Vector3.Distance(start, dest) <= movementDistance)
                    {
                        destination = dest;
                        moving = true;
                    }
                }
            }
        }
    }
}