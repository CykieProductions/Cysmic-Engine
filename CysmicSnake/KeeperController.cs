using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CysmicEngine;
using CyTools;

namespace CysmicPong
{
    public enum PlayerNumber
    {
        ONE, TWO//, THREE, FOUR
    }
    class KeeperController : Component
    {

        public float speed = 10f;
        public float moveDamp = 0.1f;
        public Rigidbody2D rb;
        public Shape2D gfx;

        PlayerNumber playerNumber;
        //bool useArrowKeys = false;

        bool willHardHit = false;
        float hardHitWindow = 0.16f;
        float hardHitTimer = 0;
        float hardHitAmplifier = 1.5f;

        public KeeperController(PlayerNumber _playerNumber)
        {
            playerNumber = _playerNumber;
            /*if (_playerNumber == PlayerNumber.ONE)
            {
                useArrowKeys = false;
            }
            else if (_playerNumber == PlayerNumber.TWO)
            {

            }*/
        }

        protected override void Start()
        {
            base.Start();
            transform.isStatic = false;
            gameObject.TryGetComponent(out rb);
            rb.gravScale = 0;
            gameObject.TryGetComponent(out gfx);
            //transform.scale = (1, 1);
        }
        protected override void Update()
        {
            base.Update();
            float moddedSpeed = speed;

            if(playerNumber == PlayerNumber.ONE)
                rb.velocity = (rb.velocity.x, Lerp(rb.velocity.y, Input.GetAxis(AxisName.VERTICAL) * moddedSpeed, moveDamp));
            else
                rb.velocity = (rb.velocity.x, Lerp(rb.velocity.y, Input.GetAxis("VerticalArrows") * moddedSpeed, moveDamp));

            if ((playerNumber == PlayerNumber.ONE && Input.PressedKey(Keys.Space)) 
                || (playerNumber == PlayerNumber.TWO && Input.PressedKey(Keys.Enter)))
            {
                willHardHit = true;
            }

            if(willHardHit)
            {
                hardHitTimer += Time.deltaTime;
                if (hardHitTimer >= hardHitWindow)
                {
                    willHardHit = false;
                    hardHitTimer = 0;
                }
            }
        }

        public override void OnCollisionEnter(Rigidbody2D.Collision collision)
        {
            base.OnCollisionEnter(collision);

            var self = collision.self;
            var other = collision.other;

            if (other.gameObject.TryGetComponent(out BallController ball))
            {
                ball.rb.velocity = (-ball.rb.velocity.x, ball.rb.velocity.y + rb.velocity.y * 0.5f);
                if (willHardHit)
                {
                    ball.rb.velocity = (ball.rb.velocity.x * hardHitAmplifier, ball.rb.velocity.y);
                    PulseEffect();
                }
                else
                    ball.rb.velocity = Vector2.Lerp(ball.rb.velocity, ball.rb.velocity.Normalize() * ball.baseSpeed, 0.35f);

                ball.rb.velocity = (ball.rb.velocity.x.Clamp(-ball.maxVelocity.x, ball.maxVelocity.x), ball.rb.velocity.y.Clamp(-ball.maxVelocity.y, ball.maxVelocity.y));
                //print("Ball Speed: " + Math.Abs(ball.rb.velocity.x));
            }
        }

        private void PulseEffect()
        {
            Vector2 newScl = (transform.scale.x * 3f, transform.scale.y * 1.5f);

            new GameObject("Pulse", new Transform(transform.position - (newScl.x / 3f, newScl.y / 6f), newScl, transform.rotation), new List<Component>()
            {
                new Shape2D(Color.FromArgb(50, gfx.color), srtOdr: gfx.sortOrder - 1)
            }).transform.lifespan = 0.3f;

            new GameObject("Pulse", new Transform(transform.position - (newScl.x / 3f, newScl.y / 6f), newScl, transform.rotation), new List<Component>()
            {
                new Shape2D(Color.FromArgb(50, gfx.color), srtOdr: gfx.sortOrder - 1)
            }).transform.lifespan = 0.2f;

            new GameObject("Pulse", new Transform(transform.position - (newScl.x / 3f, newScl.y / 6f), newScl, transform.rotation), new List<Component>()
            {
                new Shape2D(Color.FromArgb(50, gfx.color), srtOdr: gfx.sortOrder - 1)
            }).transform.lifespan = 0.1f;
        }
    }
}
