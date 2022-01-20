using UnityEngine;

public class CsController : MonoBehaviour {
    private float accel;
    private float accelspeed;
    private float addspeed;
    public float airAcceleration = 2.0f;
    public float airControl = 0.3f;
    public float airDeacceleration = 2.0f;
    private float control;
    public CharacterController controller;
    private float currentspeed;
    private float dot;
    private float drop;
    public float friction = 6f;
    private readonly float gravity = -20f;
    public Transform GroundCheck;

    public float GroundDistance = 0.4f;
    public LayerMask GroundMask;

    public bool IsGrounded;

    public bool JumpQueue;
    public float jumpSpeed = 8.0f;
    private float k;


    private Vector3 lastPos;
    public float ModulasSpeed;

    private Vector3 moved;


    public Vector3 moveDirection;
    public Vector3 moveDirectionNorm;
    public float moveSpeed = 7.0f;
    private float newspeed;

    public Transform player;
    public float playerFriction;
    private float playerTopVelocity;
    public Vector3 PlayerVel;
    private Vector3 playerVelocity;

    public Transform playerView;
    public float runAcceleration = 14f;
    public float runDeacceleration = 10f;

    public float
        sideStrafeAcceleration = 50f;

    public float sideStrafeSpeed = 1f;
    private float speed;
    private Vector3 udp;
    private Vector3 vec;
    private Vector3 wishdir;
    public bool wishJump;
    private float wishspeed;

    private float wishspeed2;

    public float x;
    public float XVelocity;
    public float z;
    private float zspeed;
    public float ZVelocity;


    private void Start() {
        lastPos = player.position;
    }


    private void Update() {
        #region

        moved = player.position - lastPos;
        lastPos = player.position;
        PlayerVel = moved / Time.fixedDeltaTime;

        ZVelocity = Mathf.Abs(PlayerVel.z);
        XVelocity = Mathf.Abs(PlayerVel.x);


        ModulasSpeed = Mathf.Sqrt(PlayerVel.z * PlayerVel.z + PlayerVel.x * PlayerVel.x);

        #endregion

        IsGrounded = Physics.CheckSphere(GroundCheck.position, GroundDistance, GroundMask);

        QueueJump();

        /* Movement, here's the important part */
        if (controller.isGrounded)
            GroundMove();
        else if (!controller.isGrounded)
            AirMove();


        controller.Move(playerVelocity * Time.deltaTime);


        udp = playerVelocity;
        udp.y = 0;
        if (udp.magnitude > playerTopVelocity)
            playerTopVelocity = udp.magnitude;
    }

    public void SetMovementDir() {
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");
    }


    private void QueueJump() {
        if (Input.GetButtonDown("Jump") && IsGrounded) wishJump = true;

        if (!IsGrounded && Input.GetButtonDown("Jump")) JumpQueue = true;
        if (IsGrounded && JumpQueue) {
            wishJump = true;
            JumpQueue = false;
        }
    }


    public void Accelerate(Vector3 wishdir, float wishspeed, float accel) {
        currentspeed = Vector3.Dot(playerVelocity, wishdir);
        addspeed = wishspeed - currentspeed;
        if (addspeed <= 0)
            return;
        accelspeed = accel * Time.deltaTime * wishspeed;
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        playerVelocity.x += accelspeed * wishdir.x;
        playerVelocity.z += accelspeed * wishdir.z;
    }


    public void AirMove() {
        SetMovementDir();

        wishdir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        wishdir = transform.TransformDirection(wishdir);

        wishspeed = wishdir.magnitude;

        wishspeed *= 7f;

        wishdir.Normalize();
        moveDirectionNorm = wishdir;


        wishspeed2 = wishspeed;
        if (Vector3.Dot(playerVelocity, wishdir) < 0)
            accel = airDeacceleration;
        else
            accel = airAcceleration;


        if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") != 0) {
            if (wishspeed > sideStrafeSpeed)
                wishspeed = sideStrafeSpeed;
            accel = sideStrafeAcceleration;
        }

        Accelerate(wishdir, wishspeed, accel);

        AirControl(wishdir, wishspeed2);


        playerVelocity.y += gravity * Time.deltaTime;

        /**
            * Air control occurs when the player is in the air, it allows
            * players to move side to side much faster rather than being
            * 'sluggish' when it comes to cornering.
            */

        void AirControl(Vector3 wishdir, float wishspeed) {
            if (Input.GetAxis("Horizontal") == 0 || wishspeed == 0)
                return;

            zspeed = playerVelocity.y;
            playerVelocity.y = 0;
            /* Next two lines are equivalent to idTech's VectorNormalize() */
            speed = playerVelocity.magnitude;
            playerVelocity.Normalize();

            dot = Vector3.Dot(playerVelocity, wishdir);
            k = 32;
            k *= airControl * dot * dot * Time.deltaTime;


            if (dot > 0) {
                playerVelocity.x = playerVelocity.x * speed + wishdir.x * k;
                playerVelocity.y = playerVelocity.y * speed + wishdir.y * k;
                playerVelocity.z = playerVelocity.z * speed + wishdir.z * k;

                playerVelocity.Normalize();
                moveDirectionNorm = playerVelocity;
            }

            playerVelocity.x *= speed;
            playerVelocity.y = zspeed;
            playerVelocity.z *= speed;
        }
    }

    /**
		* Called every frame when the engine detects that the player is on the ground
		*/
    public void GroundMove() {
        if (!wishJump)
            ApplyFriction(1.0f);
        else
            ApplyFriction(0);

        SetMovementDir();

        wishdir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        wishdir = transform.TransformDirection(wishdir);
        wishdir.Normalize();
        moveDirectionNorm = wishdir;

        wishspeed = wishdir.magnitude;
        wishspeed *= moveSpeed;

        Accelerate(wishdir, wishspeed, runAcceleration);


        playerVelocity.y = 0;

        if (wishJump) {
            playerVelocity.y = jumpSpeed;
            wishJump = false;
        }

        /**
            * Applies friction to the player, called in both the air and on the ground
            */
        void ApplyFriction(float t) {
            vec = playerVelocity;
            vec.y = 0f;
            speed = vec.magnitude;
            drop = 0f;

            /* Only if the player is on the ground then apply friction */
            if (controller.isGrounded) {
                control = speed < runDeacceleration ? runDeacceleration : speed;
                drop = control * friction * Time.deltaTime * t;
            }

            newspeed = speed - drop;
            playerFriction = newspeed;
            if (newspeed < 0)
                newspeed = 0;
            if (speed > 0)
                newspeed /= speed;

            playerVelocity.x *= newspeed;

            playerVelocity.z *= newspeed;
        }
    }
}