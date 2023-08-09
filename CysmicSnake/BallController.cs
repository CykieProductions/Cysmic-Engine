using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CysmicEngine;

namespace CysmicPong
{
    class BallController : Component
    {
        public Rigidbody2D rb;
        public Shape2D gfx;
        public float baseSpeed = 7f;
        public Vector2 maxVelocity = (15f, 15f);
        float startDelay = 1f;
        bool hasLaunched = false;
        float aliveTimer = 0f;
        float trailTimer = 0f;

        protected override void Start()
        {
            base.Start();
            gameObject.TryGetComponent(out rb);
            gameObject.TryGetComponent(out gfx);
        }

        protected override void Update()
        {
            base.Update();
            
            aliveTimer += Time.deltaTime;
            if(!hasLaunched && aliveTimer >= startDelay)
            {
                hasLaunched = true;
                Random random = new Random();
                Vector2Int randDir = (random.Next(-1, 2), random.Next(-1, 2));
                if (randDir.x == 0)
                    randDir = (1, randDir.y);
                rb.velocity = (Vector2)randDir * (baseSpeed * 0.8f);
            }
            else if (!hasLaunched)
            {
                return;
            }

            if (trailTimer >= 0.02f)
            {
                new GameObject("Blur", new Transform(transform.position, transform.scale, transform.rotation), new List<Component>()
                {
                new Shape2D(Color.FromArgb(50, gfx.color), shapeType: gfx.shape, srtOdr: gfx.sortOrder - 1)
                }).transform.lifespan = 0.05f;
                trailTimer = 0;
            }
            //trailTimer += Time.deltaTime;
        }

        public override void OnTriggerEnter(Collider2D other)
        {
            base.OnTriggerEnter(other);
            if (other.gameObject.layer == "Goal")
            {
                //Mabe use a GameManager gameObject for this
                (CysmicGame.game as PongGame).Score(other.gameObject);
                CysmicGame.Destroy(gameObject);
            }
        }

        public override void OnCollisionEnter(Rigidbody2D.Collision collision)
        {
            base.OnCollisionEnter(collision);

            var self = collision.self;
            var other = collision.other;

            //if(other.gameObject.layer != "Keeper")
            {
                /*Color gizColor = Color.Cyan;
                var giz = new GameObject("giz", new Transform(Vector2.zero, (16, 16)), new List<Component>()
                    {
                        new Shape2D(gizColor, (1, 1), (0, 0), fill: true, srtOdr: 999)
                    });
                //giz.transform.lifespan = 3f;
                giz.transform.position = (other.leftEdge, other.topEdge);

                gizColor = Color.Blue;
                giz = new GameObject("giz", new Transform(Vector2.zero, (16, 16)), new List<Component>()
                    {
                        new Shape2D(gizColor, (1, 1), (0, 0), fill: true, srtOdr: 999)
                    });
                //giz.transform.lifespan = 3f;
                giz.transform.position = (other.leftEdge, other.bottomEdge);

                gizColor = Color.Magenta;
                giz = new GameObject("giz", new Transform(Vector2.zero, (16, 16)), new List<Component>()
                    {
                        new Shape2D(gizColor, (1, 1), (0, 0), fill: true, srtOdr: 999)
                    });
                //giz.transform.lifespan = 3f;
                giz.transform.position = (other.rightEdge, other.bottomEdge) - (Vector2)(transform.scale.x / 2, 0);

                gizColor = Color.HotPink;
                giz = new GameObject("giz", new Transform(Vector2.zero, (16, 16)), new List<Component>()
                    {
                        new Shape2D(gizColor, (1, 1), (0, 0), fill: true, srtOdr: 999)
                    });
                //giz.transform.lifespan = 3f;
                giz.transform.position = (other.rightEdge, other.topEdge) + (Vector2)(transform.scale.x / 2, 0);*/


                if (other.leftEdge < self.rightEdge && other.rightEdge > self.leftEdge)//if between left and right, reflect vertically
                {
                    //rb.velocity = new Vector2(rb.velocity.x, -rb.velocity.y * 50);
                    rb.velocity = new Vector2(collision.prevVelocity.x, -collision.prevVelocity.y);
                }
            }
        }
    }
}
