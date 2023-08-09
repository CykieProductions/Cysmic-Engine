using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CysmicEngine
{
    public class Collider2D : Component
    {
        string layer = "";
        public Vector2 offset = Vector2.zero;
        public Vector2 size = (1, 1);

        public HashSet<Collider2D> collidersInContact = new HashSet<Collider2D>();
        //public Rigidbody2D rb;

        /*float prevPosTimer = 0;
        Vector2 prevPos = Vector2.Zero;
        Vector2 pos = Vector2.Zero;
        Vector2 scale = Vector2.Zero;*/

        //Rigidbody2D rb;
        //private bool isOneWay;

        //public bool isTouchingAnother;
        public bool isTrigger;
        bool scaleToTransformOnStart = false;

        float _topEdge;
        public float topEdge { get => _topEdge; private set => _topEdge = value; }
        float _bottomEdge;
        public float bottomEdge { get => _bottomEdge; private set => _bottomEdge = value; }

        float _rightEdge;
        public float rightEdge { get => _rightEdge; private set => _rightEdge = value; }
        float _leftEdge;
        public float leftEdge { get => _leftEdge; private set => _leftEdge = value; }

        public Collider2D(Vector2 os, Vector2 sz, bool isTrig = false, string lyr = "[default]", bool _scaleToTransform = false)
        {
            scaleToTransformOnStart = _scaleToTransform;

            offset = os;
            size = sz;
            isTrigger = isTrig;
            layer = lyr;
        }

        protected override void Start()
        {
            base.Start();
            if (scaleToTransformOnStart)
            {
                offset = offset * transform.scale / transform.scale;//I need to multiply, then divide by the same vaule for this to work for some reason
                size = size / transform.scale;
            }

            if (layer == "[default]")
                layer = gameObject.layer;
            var gizColor = Color.ForestGreen;
            if (isTrigger)
                gizColor = Color.LawnGreen;

            new GizmoObj(transform, new Shape2D(/*Color.FromArgb(200, gizColor)*/gizColor, size: size, offset: offset, fill: false, srtOdr: int.MaxValue));
            /*prevPos = transform.position;
            pos = transform.position + offset;
            scale = transform.scale * size;*/
        }

        protected override void Update()
        {
            base.Update();
            /*if (rb == null)//Should be optimized later
                TryGetComponent(out rb);*
            /*pos = transform.position + offset;
            scale = transform.scale * size;*/

            topEdge = transform.position.y + offset.y;
            bottomEdge = (transform.position.y + offset.y) + (transform.scale.y * size.y);
            rightEdge = (transform.position.x + offset.x) + (transform.scale.x * size.x);
            leftEdge = transform.position.x + offset.x;
        }

        /*protected override void FixedUpdate()
        {
            base.FixedUpdate();
            //CheckForCollisions();
        }*/


        public bool IsTouching(Collider2D other)
        {
            return collidersInContact.Contains(other);
        }
        public bool IsTouching(string layer)
        {
            return collidersInContact.Any(x => x != null && x.layer == layer);
        }

        /*void CheckForCollisions(/*float predictedXOffset = 0, float predictedYOffset = 0*)
        {
            if (transform.isStatic)
                return;

            var myRightEdge = pos.x + scale.x;
            var myLeftEdge = pos.x;
            var myBottomEdge = pos.y + scale.y;
            var myTopEdge = pos.y;

            var allColliders = CysmicGame.allColliders.ToList();
            for (int i = 0; i < allColliders.Count; i++)
            {
                if (allColliders[i].gameObject == gameObject)
                    continue;

                var otherRightEdge = allColliders[i].pos.x + allColliders[i].scale.x;
                var otherLeftEdge = allColliders[i].pos.x;
                var otherBottomEdge = allColliders[i].pos.y + allColliders[i].scale.y;
                var otherTopEdge = allColliders[i].pos.y;

                if (myRightEdge >= otherLeftEdge &&//right edge is farther right than left edge of other
                    myLeftEdge <= otherRightEdge &&//left edge is farther left than right edge of other
                    myBottomEdge > otherTopEdge &&//bottom edge is lower than top edge of other
                    myTopEdge < otherBottomEdge)//top edge is higher than bottom edge of other
                {
                    //isTouchingAnother = true;

                    
                    /*if (myRightEdge >= otherLeftEdge + 5 || myLeftEdge <= otherRightEdge - 5)//farther in left wall or right wall
                    {
                        if (myBottomEdge > otherTopEdge + 1 && pos.y + (scale.y / 2) < otherTopEdge)//Hit Floor
                        {
                            /*if (rb != null && rb.velocity.y < 0)// && prevPos.y <= transform.position.y)//NOTE: down and up are flipped for some reason
                                rb.velocity = (rb.velocity.x, 0);
                            if (rb == null || (rb != null && rb.velocity.y <= 0))
                            {
                                if (pos.y + scale.y > allColliders[i].pos.y + 1.5)//lower than by more than 1.5
                                    transform.Translate(0, 2);
                                else
                                    transform.SetPosition(new Vector2(transform.position.x, prevPos.y));
                            }*
                        }

                        if (!allColliders[i].isOneWay && myTopEdge < otherBottomEdge && myBottomEdge > otherBottomEdge)//Hit Ceiling
                        {
                            /*if (rb != null && rb.velocity.y > 0)
                                rb.velocity = (rb.velocity.x, 0);
                            if (rb == null || (rb != null && rb.velocity.y >= 0))
                            {
                                if (myTopEdge < otherBottomEdge - 1.5)//Higher than by more than 1.5
                                    transform.Translate(0, -2);
                                else
                                    transform.SetPosition(new Vector2(transform.position.x, prevPos.y));
                            }*
                        }
                    }
                    
                    if (myBottomEdge > otherTopEdge + 5)//farther in floor
                    {
                        if (myRightEdge >= otherLeftEdge + 1 && myLeftEdge < otherLeftEdge)//Hit Left Wall only
                        {
                            /*if (rb != null && rb.velocity.x < 0)//going left
                                rb.velocity = (0, rb.velocity.y);
                            if (rb == null || (rb != null && rb.velocity.x <= 0))
                            {
                                if (myBottomEdge > otherTopEdge + 1.5)//greater than actually means is under
                                    transform.Translate(-5, 0);
                                else
                                    transform.SetPosition(new Vector2(prevPos.x, transform.position.y));
                            }*
                        }
                        if (myLeftEdge <= otherRightEdge - 1 && myRightEdge > otherRightEdge)//Hit Right Wall only
                        {
                            /*if (rb != null && rb.velocity.x > 0)//going right
                                rb.velocity = (0, rb.velocity.y);
                            if (rb == null || (rb != null && rb.velocity.x >= 0))
                            {
                                if (myBottomEdge > otherTopEdge + 1.5)
                                    transform.Translate(5, 0);
                                else
                                    transform.SetPosition(new Vector2(prevPos.x, transform.position.y));
                            }*
                        }
                    }*
                    
                    
                    prevPos = transform.position;
                }//Is touching this
            }//Check every collider
        }*/


    }
}
