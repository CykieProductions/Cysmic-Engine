using System;
using System.Windows.Forms;

namespace CysmicEngine.Demo_Game
{
    class InputController : Component
    {
        public Vector2 movement = Vector2.zero;
        public bool pressedJump;
        public float jumpBuffer = 0.2f;
        public float jumpTimer = 0f;

        protected override void Start()
        {
            base.Start();
        }
        protected override void Update()
        {
            base.Update();
            movement = (Input.GetAxis(AxisName.HORIZONTAL), Input.GetAxis(AxisName.VERTICAL));

            if (pressedJump && jumpTimer < jumpBuffer)
            {
                jumpTimer += Time.deltaTime;
            }
            else if (pressedJump)
            {
                jumpTimer = 0;
                pressedJump = false;
            }
            if (Input.PressedKey(Keys.Space))
            {
                pressedJump = true;
                jumpTimer = 0;
            }
        }
    }
    class PlayerMotor : Component
    {
        public InputController ic;
        public Rigidbody2D rb;
        public Animator anim;
        public SpriteRenderer gfx;
        public float speed = 8f;

        public float jumpForce = 6;
        public float jumpHoldLimit = 0.4f;
        float jumpHoldTimer = 0;
        private bool isGrounded = false;

        public Collider2D groundDetector;
        private bool isJumping;
        bool isSprinting;

        protected override void Start()
        {
            base.Start();
            gameObject.TryGetComponent(out ic);
            gameObject.TryGetComponent(out rb);
            gameObject.TryGetComponent(out anim);
            gameObject.TryGetComponent(out gfx);
            //transform.scale = (1, 1);
            groundDetector = gameObject.AddComponent(new Collider2D((3, 28), (26, 4), isTrig: true, _scaleToTransform: true));
        }
        protected override void Update()
        {
            base.Update();
            float moddedSpeed = speed;
            if (!Input.HoldingKey(Keys.ShiftKey) || (isGrounded && Math.Abs(rb.velocity.x) <= speed / 2))
                isSprinting = false;
            else if ((Input.HoldingKey(Keys.ShiftKey) && isGrounded && Math.Abs(rb.velocity.x) > speed / 2) || isSprinting)
            {
                moddedSpeed = speed * 1.8f;
                isSprinting = true;
            }

            isGrounded = groundDetector.IsTouching("Ground");

            rb.velocity = (Lerp(rb.velocity.x, ic.movement.x * moddedSpeed, 0.25f), rb.velocity.y);

            if (ic.movement.x > 0)
            {
                gfx.flipX = false;
            }
            else if (ic.movement.x < 0)
            {
                gfx.flipX = true;
            }

            if (isGrounded)
            {
                if (ic.movement.x != 0)
                    anim?.Play("Run", loop: true);
                else
                    anim?.Play("Idle", loop: true);
            }
            else
            {
                if (rb.velocity.y > 0)
                    anim?.Play("Jump", loop: true);
                else
                    anim?.Play("Fall", loop: true);
            }

            if (isJumping && (rb.velocity.y <= 0 || jumpHoldTimer >= jumpHoldLimit || !Input.HoldingKey(Keys.Space)))
            {
                jumpHoldTimer = 0;
                rb.velocity = (rb.velocity.x, rb.velocity.y / 2);
                isJumping = false;
            }
            if ((ic.pressedJump && isGrounded) || (Input.HoldingKey(Keys.Space) && isJumping))
            {
                rb.AddForce(0f, jumpForce, true);
                jumpHoldTimer += Time.deltaTime;
                isJumping = true;
            }
        }

        public override void OnTriggerEnter(Collider2D other)
        {
            //print($"[ENTER] {other.gameObject.name}", false);
        }
        public override void OnTriggerStay(Collider2D other)
        {
            //print($"[STAY] {other.gameObject.name}", false);
        }
        public override void OnTriggerExit(Collider2D other)
        {
            //print($"[EXIT] {other.gameObject.name}", false);
        }
    }
}
