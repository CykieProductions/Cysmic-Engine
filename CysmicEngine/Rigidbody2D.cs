using System;
using System.Collections.Generic;
using System.Linq;
using CyTools;

namespace CysmicEngine
{
    /// <summary>
    /// A component that adds Physics to a GameObject. Only one can be on a GameObject.
    /// </summary>
    public class Rigidbody2D : Component
    {
        HashSet<Collider2D> collidersTouchedLastFrame = new HashSet<Collider2D>();
        public Action<Collider2D> TriggerEnterAction;
        public Action<Collider2D> TriggerStayAction;
        public Action<Collider2D> TriggerExitAction;

        public Action<Collision> CollisionEnterAction;
        public Action<Collider2D, Collider2D> CollisionStayAction;
        public Action<Collider2D, Collider2D> CollisionExitAction;

        public bool isStatic = true;
        public bool isKinematic = false;
        public float gravScale = 6;
        const float _constantG = 196f;
        public float mass = 1;
        //public bool isPushable = false;
        public const bool allowPushing = false;

        Vector2 _velocity;
        public Vector2 velocity { get { return _velocity; } set { _velocity = value; waitToZeroYVel = true; } }
        public bool waitToZeroYVel = false;
        //float _realXVel = 0;

        List<Collider2D> myColliders = new List<Collider2D>();
        //float prevPosTimer = 0;//prevPos is set whenever you're not touching anything
        Vector2 prevPos = Vector2.zero;
        private float frictionDamp = 0f;
        private float prevYVel;

        public override bool OnlyOnePerGO()
        {
            return true;
        }

        public Rigidbody2D() { }
        public Rigidbody2D(bool _isStatic = true, float gravity = 6f)
        {
            isStatic = _isStatic;
            gravScale = gravity;
        }

        protected override void Start()
        {
            base.Start();
            if(!isStatic)
                transform.isStatic = isStatic;

            //onlyOnePerGO = true;//override the OnlyOnePerGO funtion instead
            myColliders = gameObject.allComponents.OfType<Collider2D>().ToList();

            prevPos = transform.position;
            //Collision actions could be assigned to whenever the rigidbody is added, but I'd have to make them public
            for (int i = 0; i < gameObject.allComponents.Count; i++)
            {
                TriggerEnterAction += gameObject.allComponents[i].OnTriggerEnter;
                TriggerStayAction += gameObject.allComponents[i].OnTriggerStay;
                TriggerExitAction += gameObject.allComponents[i].OnTriggerExit;

                CollisionEnterAction += gameObject.allComponents[i].OnCollisionEnter;
                CollisionStayAction += gameObject.allComponents[i].OnCollisionStay;
                CollisionExitAction += gameObject.allComponents[i].OnCollisionExit;
            }
        }

        protected override void Update()
        {
            base.Update();

            /*var giz = new GameObject(trnfrm: new Transform(pos: (transform.position.x, transform.position.y)
                        , scl: (10, 10), rot: 0)
                        , components: new List<Component>()//Top Left
                    {
                        new Shape2D(Color.FromArgb(100, Color.LightGreen), sz: (1f, 1f), Shape2D.ShapeType.Rectangle, 99, isGiz: true)
                    }
                    ).transform;
            giz.SetPosition((transform.position.x + velocity.x, transform.position.y - velocity.y));
            giz.SetScale((transform.scale.x * myColliders[0].size.x, transform.scale.y * myColliders[0].size.y));
            giz.lifespan = 0.1f;*/
        }

        public void AddForce(float forceX, float forceY, bool zeroOutY = false, bool zeroOutX = false)
        {
            if (zeroOutX)
                velocity = (0, velocity.y);
            if (zeroOutY)
                velocity = (velocity.x, 0);

            velocity += (forceX, forceY);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (gameObject.wasDestroyed)
                return;

            //Perform any velocity calculations before checking for walls
            _velocity.y -= gravScale * _constantG * (float)Math.Pow(Time.fixedDeltaTime, 2);

            if (collidersTouchedLastFrame.ToList().Exists(x => !x.isTrigger))
                _velocity.x = Lerp(_velocity.x, 0, frictionDamp);
            if (_velocity.x > -0.01f && _velocity.x < 0.01f)
                _velocity.x = 0;

            //Check ahead for colliders
            //Vector2 preCheckVel = velocity;
            Vector2 newPos = (float.Epsilon, float.Epsilon);
            for (int i = 0; i < myColliders.Count; i++)
            {
                var result = CheckAhead(myColliders[i], _velocity.x, _velocity.y);
                if (!myColliders[i].isTrigger)
                    newPos = result;
            }

            newPos -= myColliders[0].offset;//Offset is factored in below

            if (newPos.y == float.Epsilon - myColliders[0].offset.y)//won't hit anything vertically
            {
                if (_velocity.y > 0.1f || _velocity.y < -0.1f)
                    transform.Translate(0, _velocity.y);

                prevYVel = _velocity.y;
            }
            else
            {
                /*if (Math.Abs(_velocity.y) > 0.0001)//wait a frame
                {
                    _velocity.y = _velocity.y.Clamp(-0.0001f, 0.0001f);
                    //_velocity.y = 8888888888888;
                }
                else if (Math.Abs(_velocity.y) <= 0.0001)
                {
                    _velocity.y = 0;
                }*/

                if (_velocity.y != 0)
                    prevYVel = _velocity.y;//Collsions can't read the y vel in time (?)
                else
                    prevYVel = 0;

                //if ((prevYVel > 0 && _velocity.y > 0) || (prevYVel < 0 && _velocity.y < 0))//don't trigger if Y vel was reversed
                if (!waitToZeroYVel)
                {
                    _velocity.y = 0;
                }
                else
                    waitToZeroYVel = false;

                transform.SetPosition((transform.position.x, newPos.y));
            }

            if (newPos.x == float.Epsilon - myColliders[0].offset.x)//won't hit anything horizontally
            {
                transform.Translate(_velocity.x, 0);
            }
            else
            {
                //if (Math.Abs(preCheckVel.x) >= Math.Abs(velocity.x))
                //velocity.x = 0;
                transform.SetPosition((newPos.x, transform.position.y));
            }
        }

        Vector2 CheckAhead(Collider2D collider, float inputVelX, float inputVelY)
        {
            var startVel = _velocity;

            collider.collidersInContact.Clear();
            Vector2 scale = transform.scale * collider.size;
            Vector2 prevRealPos = prevPos + collider.offset;//transform.position + collider.offset;

            Vector2 phanPos = (transform.position.x + inputVelX, transform.position.y - inputVelY) + collider.offset;
            var myRightEdge = phanPos.x + scale.x;
            var myLeftEdge = phanPos.x;
            var myBottomEdge = phanPos.y + scale.y;
            var myTopEdge = phanPos.y;

            void CalculateEdgesFromNewPos(Vector2 newPos)
            {
                //phanPos = (transform.position.x + newInputVelX, transform.position.y - newInputVelY) + collider.offset;
                phanPos = newPos;
                myRightEdge = phanPos.x + scale.x;
                myLeftEdge = phanPos.x;
                myBottomEdge = phanPos.y + scale.y;
                myTopEdge = phanPos.y;
            }
            void CalculateEdgesFromVelocity(float newInputVelX, float newInputVelY)
            {
                phanPos = (transform.position.x + newInputVelX, transform.position.y - newInputVelY) + collider.offset;
                myRightEdge = phanPos.x + scale.x;
                myLeftEdge = phanPos.x;
                myBottomEdge = phanPos.y + scale.y;
                myTopEdge = phanPos.y;
            }

            bool hitX = false;
            bool hitY = false;

            var allColliders = CysmicGame.allColliders/*.OrderBy(x => !x.transform.isStatic)*/.ToList();
            var collidersTouchedThisFrame = new HashSet<Collider2D>();
            bool hitNothing = true;

            for (int i = 0; i < allColliders.Count; i++)
            {
                if (allColliders[i] == null || allColliders[i].gameObject == gameObject)//Don't collide with self
                    continue;


                Vector2 otherScale = ((allColliders[i].transform.scale.x * allColliders[i].size.x), (allColliders[i].transform.scale.y * allColliders[i].size.y));

                var otherRightEdge = (allColliders[i].transform.position.x + allColliders[i].offset.x) + otherScale.x;
                var otherLeftEdge = (allColliders[i].transform.position.x + allColliders[i].offset.x);
                var otherBottomEdge = (allColliders[i].transform.position.y + allColliders[i].offset.y) + otherScale.y;
                var otherTopEdge = (allColliders[i].transform.position.y + allColliders[i].offset.y);

                /*if (collider.gameObject.name == "Player" && collider.isTrigger)
                    print("player made it past self check");*/

                //check without prediction
                if (myRightEdge - inputVelX >= otherLeftEdge &&//right edge is farther right than left edge of other
                myLeftEdge - inputVelX <= otherRightEdge &&//left edge is farther left than right edge of other
                myBottomEdge + inputVelY >= otherTopEdge &&//bottom edge is lower than top edge of other
                myTopEdge + inputVelY <= otherBottomEdge)//top edge is higher than bottom edge of other
                {
                    collider.collidersInContact.Add(allColliders[i]);//Needed for collider.IsTouching
                    /*if (collider.gameObject.name == "Player" && collider.isTrigger)
                        print("player collided");*/
                }

                if (myRightEdge >= otherLeftEdge &&//right edge is farther right than left edge of other
                    myLeftEdge <= otherRightEdge &&//left edge is farther left than right edge of other
                    myBottomEdge >= otherTopEdge &&//bottom edge is lower than top edge of other
                    myTopEdge <= otherBottomEdge)//top edge is higher than bottom edge of other
                {
                    //hitNothing = false;//Moved into the while loop below
                    //Collision events are triggered after moving you out of the wall

                    if (collider.isTrigger || !allColliders[i].isTrigger)
                    {
                        //float push = 0.1f;

                        //if (myLeftEdge > otherLeftEdge + 2 && myRightEdge < otherRightEdge - 2)
                        {
                            if (prevRealPos.y + scale.y <= otherTopEdge)// && myBottomEdge < otherTopEdge + (otherScale.y / 2))
                            {//Should push up
                             //inputVelY += push;
                                myTopEdge = otherTopEdge - scale.y;
                                hitY = true;
                            }
                            else if (prevRealPos.y >= otherBottomEdge)//real top is lower than other bottom
                            {//Should push down
                             //inputVelY -= push;
                                myTopEdge = otherBottomEdge;
                                hitY = true;
                            }
                        }

                        //if (myBottomEdge > otherTopEdge + 2 && myTopEdge < otherBottomEdge - 2)
                        {
                            if (prevRealPos.x >= otherRightEdge)// && myLeftEdge > otherRightEdge - (otherScale.x / 2))//Should push to the right
                            {
                                //inputVelX += push;
                                myLeftEdge = otherRightEdge;
                                hitX = true;
                            }
                            else if (prevRealPos.x + scale.x <= otherLeftEdge)//Should push to the left
                            {
                                //inputVelX -= push;
                                myLeftEdge = otherLeftEdge - scale.x;
                                hitX = true;
                            }
                        }

                        CalculateEdgesFromNewPos((myLeftEdge, myTopEdge));

                        //Failsafe for if you still end up in a wall
                        /*while*/
                        if (myRightEdge > otherLeftEdge &&//right edge is farther right than left edge of other
               myLeftEdge < otherRightEdge &&//left edge is farther left than right edge of other
               myBottomEdge > otherTopEdge &&//bottom edge is lower than top edge of other
               myTopEdge < otherBottomEdge)//top edge is higher than bottom edge of other
                        {
                            hitNothing = false;
                            /*if (justStartedTouching)
                            {
                                //OnCollision
                                //OnTrigger
                                justStartedTouching = false;
                            }*/

                            float push = 0.1f;

                            //if (myLeftEdge > otherLeftEdge + 2 && myRightEdge < otherRightEdge - 2)
                            {
                                if (prevRealPos.y + scale.y <= otherTopEdge || prevRealPos.y < otherBottomEdge - (otherScale.y / 2))// && myBottomEdge < otherTopEdge + (otherScale.y / 5))
                                {//Should push up
                                    inputVelY += push;
                                    //hitY = true;
                                }
                                else if (prevRealPos.y >= otherBottomEdge || prevRealPos.y >= otherBottomEdge - (otherScale.y / 2))//real top is lower than other bottom
                                {//Should push down
                                    inputVelY -= push;
                                    //hitY = true;
                                }
                            }

                            //if (myBottomEdge > otherTopEdge + 2 && myTopEdge < otherBottomEdge - 2)
                            {
                                if (prevRealPos.x >= otherRightEdge || prevRealPos.x >= otherRightEdge - (otherScale.x / 2))// && myLeftEdge > otherRightEdge - (otherScale.x / 5))//Should push to the right
                                {
                                    inputVelX += push;
                                    //hitX = true;
                                }
                                else if (prevRealPos.x + scale.x <= otherLeftEdge || prevRealPos.x + scale.x >= otherRightEdge - (otherScale.x / 2))//Should push to the left
                                {
                                    inputVelX -= push;
                                    //hitX = true;
                                }
                            }

                            CalculateEdgesFromVelocity(inputVelX, inputVelY);
                        }
                    }//end of is trigger check

                    //Collsion Events
                    //check without prediction
                    if (myRightEdge - inputVelX >= otherLeftEdge &&//right edge is farther right than left edge of other
                    myLeftEdge - inputVelX <= otherRightEdge &&//left edge is farther left than right edge of other
                    myBottomEdge + inputVelY >= otherTopEdge &&//bottom edge is lower than top edge of other
                    myTopEdge + inputVelY <= otherBottomEdge)//top edge is higher than bottom edge of other
                    {
                        collidersTouchedThisFrame.Add(allColliders[i]);
                        bool firstFrame = true;
                        if (collidersTouchedLastFrame.Contains(allColliders[i]))
                        {
                            firstFrame = false;
                        }

                        if (allColliders[i].isTrigger)
                        {
                            if (firstFrame)
                            {
                                TriggerEnterAction.Invoke(allColliders[i]);
                            }
                            else
                                TriggerStayAction.Invoke(allColliders[i]);
                        }
                        else if (!collider.isTrigger)//Both must be physical colliders
                        {
                            if (firstFrame)
                            {
                                CollisionEnterAction.Invoke(new Collision(collider, allColliders[i], new Vector2(startVel.x, prevYVel) ));
                            }
                            else
                                CollisionStayAction.Invoke(collider, allColliders[i]);

                            if (allColliders[i].gameObject.TryGetComponent(out Rigidbody2D otherRb) && Rigidbody2D.allowPushing)//Temp pushing solution//Player falls into the ground if they stop push from the left side
                            {
                                if (myBottomEdge > otherTopEdge && myTopEdge < otherBottomEdge)
                                    otherRb._velocity.x = Lerp(otherRb._velocity.x, _velocity.x - otherRb._velocity.x - otherRb.mass, 0.2f);

                                if (myRightEdge > otherLeftEdge && myLeftEdge < otherRightEdge)
                                    otherRb._velocity.y = Lerp(otherRb._velocity.y, _velocity.y - otherRb._velocity.y - otherRb.mass, 0.2f);
                            }
                            else
                            {
                                if (myBottomEdge > otherTopEdge && myTopEdge < otherBottomEdge)
                                {
                                    //Some bounce value is needed
                                    //velocity.x -= velocity.x * 1.5f;
                                    //print("Bounce");
                                }
                            }
                        }
                    }

                }
            }

            //Collision Exit Events
            for (int i = 0; i < collidersTouchedLastFrame.Count; i++)
            {
                if (!collidersTouchedThisFrame.Contains(collidersTouchedLastFrame.ElementAt(i)))//if no longer touching a collider from last frame
                {
                    if (collidersTouchedLastFrame.ElementAt(i).isTrigger)
                    {
                        TriggerExitAction.Invoke(collidersTouchedLastFrame.ElementAt(i));
                    }
                    else if (!collider.isTrigger)//Both must be physical colliders
                    {
                        CollisionExitAction.Invoke(collider, collidersTouchedLastFrame.ElementAt(i));
                    }
                }
            }
            collidersTouchedLastFrame.Clear();
            collidersTouchedLastFrame = collidersTouchedThisFrame;

            if (collider.isTrigger)
                return (float.Epsilon, float.Epsilon);

            if (hitNothing)
                prevPos = transform.position;

            float xResult = float.Epsilon;
            float yResult = float.Epsilon;
            if (hitX)
                xResult = myLeftEdge;
            if (hitY)
                yResult = myTopEdge;

            return (xResult, yResult);
        }

        /*public (bool, bool) CheckForCollisions(Collider2D collider, float xOffset, float yOffset, out float toFloorDist, out float newXPos/*toWallDir*)
        {
            toFloorDist = 0;
            //toWallDir = 0;
            newXPos = 0;
            if (transform.isStatic)
                return (true, true);

            var pos = transform.position + collider.offset;
            var scale = transform.scale * collider.size;

            var myRightEdge = pos.x + xOffset + scale.x;
            var myLeftEdge = pos.x + xOffset;
            var myBottomEdge = pos.y + yOffset + scale.y;
            var myTopEdge = pos.y + yOffset;

            var allColliders = CysmicGame.allColliders.ToList();
            for (int i = 0; i < allColliders.Count; i++)
            {
                if (allColliders[i].gameObject == gameObject)
                    continue;

                Vector2 otherScale = ((allColliders[i].transform.scale.x * allColliders[i].size.x), (allColliders[i].transform.scale.y * allColliders[i].size.y));

                var otherRightEdge = (allColliders[i].transform.position.x + allColliders[i].offset.x) + otherScale.x;
                var otherLeftEdge = (allColliders[i].transform.position.x + allColliders[i].offset.x);
                var otherBottomEdge = (allColliders[i].transform.position.y + allColliders[i].offset.y) + otherScale.y;
                var otherTopEdge = (allColliders[i].transform.position.y + allColliders[i].offset.y);

                if (myRightEdge > otherLeftEdge &&//right edge is farther right than left edge of other
                    myLeftEdge < otherRightEdge &&//left edge is farther left than right edge of other
                    myBottomEdge > otherTopEdge &&//bottom edge is lower than top edge of other
                    myTopEdge < otherBottomEdge)//top edge is higher than bottom edge of other
                {
                    bool hitWall = false;
                    bool hitFloor = false;
                    if (myBottomEdge > otherTopEdge + 1)// + (scale.y * 0.1f).Clamp(10, scale.y))
                    {
                        //toFloorDist = -(otherTopEdge - myBottomEdge + yOffset);
                        toFloorDist = otherBottomEdge + scale.y;

                        hitFloor = true;
                    }
                    //else if (myTopEdge > otherBottomEdge - (scale.y * 0.1f).Clamp(10, scale.y))
                    else if (myTopEdge < otherBottomEdge - 1)
                    {
                        //toFloorDist = -(otherBottomEdge - myTopEdge + yOffset);
                        toFloorDist = otherBottomEdge;
                        hitFloor = true;
                    }

                    if (myBottomEdge > otherTopEdge + yOffset + 1 && myTopEdge < otherBottomEdge - yOffset - 1)
                    {
                        if (myRightEdge > otherLeftEdge && myLeftEdge < otherLeftEdge)// && myRightEdge <= otherLeftEdge + (scale.y / 2))//Hit Left Wall from right
                        {
                            newXPos = (otherLeftEdge - scale.x);
                            //toWallDist = (otherLeftEdge - myRightEdge - xOffset);
                            //toWallDir = -1;
                            hitWall = true;
                        }
                        else if (myLeftEdge < otherRightEdge)// && myRightEdge > otherRightEdge)// && myLeftEdge > otherLeftEdge + (scale.y / 2))//Hit Right Wall from left
                        {
                            newXPos = (otherRightEdge);
                            //toWallDist = (myLeftEdge - xOffset - otherRightEdge);
                            //toWallDist = (myLeftEdge - xOffset - otherRightEdge);
                            //toWallDir = 1;
                            hitWall = true;
                        }
                    }

                    /*var giz = new GameObject(trnfrm: new Transform(pos: (transform.position.x, transform.position.y)
                        , scl: (10, 10), rot: 0)
                        , components: new List<Component>()//Top Left
                    {
                        new Shape2D(Color.Green, sz: (1, 1), Shape2D.ShapeType.Rectangle, 99)
                    }
                    ).transform;
                    giz.SetPosition((transform.position.x, transform.position.y));
                    giz.SetScale((scale.x, scale.y));
                    giz.lifespan = 01f;*

                    return (hitWall, hitFloor);
                }//Is touching this
            }//Check every collider
            return (false, false);
        }*/
        public struct Collision
        {
            public Collider2D self;
            public Collider2D other;
            public Vector2 prevVelocity;

            public Collision(Collider2D _self, Collider2D _other, Vector2 preVel)
            {
                self = _self;
                other = _other;
                prevVelocity = preVel;
            }
        }
    }

}
