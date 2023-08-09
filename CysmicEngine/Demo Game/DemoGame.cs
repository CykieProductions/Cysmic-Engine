using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CysmicEngine.Demo_Game
{
    class DemoGame : CysmicGame
    {
        public Transform player;
        public Transform cursorObj;
        public Transform floor;
        public Transform box;
        //public Transform camGizmo;

        public DemoGame() : base(new Vector2(800, 450), "Cysmic Engine Demo")
        {

        }
        DateTime prevNow;
        public override void OnStart()
        {
            prevNow = DateTime.UtcNow;


            /*var b = new Bitmap("Main Characters/Virtual Guy/Jump (32x32).png");
            var c = new Animator.AnimationClip(_frames: new (Bitmap, float)[]
                    {
                        (new Bitmap("Main Characters/Virtual Guy/Jump (32x32).png"), 0),
                        (new Bitmap("Main Characters/Virtual Guy/Fall (32x32).png"), 0)
                    });*/
            {
                var idleCycle = new SpriteSheet(@"Assets/Sprites/Main Characters/Virtual Guy/Idle (32x32).png", new Vector2Int(32, 32));
                var runCycle = new SpriteSheet(@"Assets/Sprites/Main Characters/Virtual Guy/Run (32x32).png", new Vector2Int(32, 32));

                player = new GameObject("Player", trnfrm: new Transform
                    ((game.window.Size.Width / 2, (game.window.Size.Height / 2) - 50), scl: (32, 32), 0)
                    , components: new List<Component>()
                {
                new SpriteRenderer("Main Characters/Virtual Guy/Fall (32x32)", 50),
                new PlayerMotor(),
                new InputController(),
                new Rigidbody2D(),
                new Collider2D(os: (2, 2) , (28, 28), _scaleToTransform: true),
                new Animator(new Dictionary<string, Animator.AnimationClip>()
                {
                    ["Idle"] = new Animator.AnimationClip(_frames: new (Sprite, float)[]
                    {
                        (idleCycle.GetSliceByName("0"), 0),
                        (idleCycle.GetSliceByName("1"), 0),
                        (idleCycle.GetSliceByName("2"), 0),
                        (idleCycle.GetSliceByName("3"), 0),
                        (idleCycle.GetSliceByName("4"), 0),
                        (idleCycle.GetSliceByName("5"), 0),
                        (idleCycle.GetSliceByName("6"), 0),
                        (idleCycle.GetSliceByName("7"), 0),
                        (idleCycle.GetSliceByName("8"), 0),
                        (idleCycle.GetSliceByName("9"), 0),
                        (idleCycle.GetSliceByName("10"), 0),
                    }),
                    ["Run"] = new Animator.AnimationClip(_frames: new (Sprite, float)[]
                    {
                        (runCycle.GetSliceByName("0"), 0),
                        (runCycle.GetSliceByName("1"), 0),
                        (runCycle.GetSliceByName("2"), 0),
                        (runCycle.GetSliceByName("3"), 0),
                        (runCycle.GetSliceByName("4"), 0),
                        (runCycle.GetSliceByName("5"), 0),
                        (runCycle.GetSliceByName("6"), 0),
                        (runCycle.GetSliceByName("7"), 0),
                        (runCycle.GetSliceByName("8"), 0),
                        (runCycle.GetSliceByName("9"), 0),
                        (runCycle.GetSliceByName("10"), 0),
                        (runCycle.GetSliceByName("11"), 0),
                    }),
                    ["Jump"] = new Animator.AnimationClip(_frames: new (Sprite, float)[]
                    {
                        (new Sprite(@"Assets/Sprites/Main Characters/Virtual Guy/Jump (32x32).png"), 1)
                    }),
                    ["Fall"] = new Animator.AnimationClip(_frames: new (Sprite, float)[]
                    {
                        (new Sprite(@"Assets/Sprites/Main Characters/Virtual Guy/Fall (32x32).png"), 0)
                    })
                })
                }
                ).transform;
            }
            player.transform.isStatic = false;
            player.isStatic = false;
            if (player.gameObject.TryGetComponent(out Rigidbody2D someRB))
            {
                someRB.gravScale = 5;
                //someRB.isPushable = false;
            }

            //Input.showCursor = false;
            cursorObj = new GameObject("Cursor", trnfrm: new Transform
                ((game.window.Size.Width / 2, (game.window.Size.Height / 2) - 90), scl: (8, 8), 0)
                , components: new List<Component>()
            {
                //new SpriteRenderer("Main Characters/Virtual Guy/Fall (32x32)", 50),
                new Shape2D(Color.HotPink, size: (1,1), offset: Vector2.zero, srtOdr: int.MaxValue),
                new Shape2D(Color.DarkRed, size: (1.25f,1.25f), offset: (-1.75f, -1.75f), srtOdr: int.MaxValue -1),
            }).transform;

            box = new GameObject("Box", trnfrm: new Transform
                ((game.window.Size.Width / 2, (game.window.Size.Height / 2) - 90), scl: (32, 32), 0)
                , components: new List<Component>()
            {
                //new SpriteRenderer("Main Characters/Virtual Guy/Fall (32x32)", 50),
                new Shape2D(Color.Red, size: (1,1), offset: Vector2.zero, srtOdr: -5),
                //new PlayerMotor(),
                //new InputController(),
                new Rigidbody2D(),
                new Collider2D((0, 0), (1,1), isTrig: false),
                new AudioSource()
                //new Collider2D(Vector2.Zero, (10, 10)),
            }, lyr: "Ground"
            ).transform;
            box.isStatic = false;
            if (box.gameObject.TryGetComponent(out someRB))
            {
                someRB.gravScale = 3;
            }

            floor = new GameObject("Floor", trnfrm: new Transform
                (pos: ((game.window.Size.Width / 2) - 200, (game.window.Size.Height / 2) + 50), scl: (300, 30), rot: 0)
                , components: new List<Component>()
            {
                new Shape2D(Color.Blue, size: (1, 1), offset: Vector2.zero, Shape2D.ShapeType.Rectangle),
                new Collider2D(Vector2.zero,(1,1))
            }, lyr: "Ground"
            ).transform;

            floor = new GameObject("Floor", trnfrm: new Transform
                (pos: ((game.window.Size.Width / 2) - 60 - 200, (game.window.Size.Height / 2) + 130), scl: (420, 30), rot: 0)
                , components: new List<Component>()
            {
                new Shape2D(Color.Blue, size: (1, 1), offset: Vector2.zero, Shape2D.ShapeType.Rectangle),
                new Collider2D(Vector2.zero,(1,1))
            }, lyr: "Ground"
            ).transform;

            floor = new GameObject("Floor", trnfrm: new Transform
                (pos: ((game.window.Size.Width / 2) - 60, (game.window.Size.Height / 2) - 50), scl: (20, 300), rot: 0)
                , components: new List<Component>()
            {
                new Shape2D(Color.Blue, size: (1, 1), offset: Vector2.zero, Shape2D.ShapeType.Rectangle),
                new Collider2D(Vector2.zero,(1,1))
            }, lyr: "Ground"
            ).transform;
        }

        public override void Update()
        {
            base.Update();

            /*if (Input.PressedKey(Keys.Up))
                print("Player pressed: " + Keys.Up);
            if (Input.HoldingKey(Keys.Up))
                print("Player is holding down: " + Keys.Up);
            if (Input.ReleasedKey(Keys.Up))
                print("Player released: " + Keys.Up);*/

            //print(Input.GetAxis(AxisName.HORIZONTAL));

            /*if(Input.PressedKey(Keys.P))
            {
                if (box.gameObject.TryGetComponent(out AudioSource audioSource))
                {
                    audioSource.Play("Gloomstead");
                    print("play");
                }
            }
            if(Input.PressedKey(Keys.S))
            {
                if (box.gameObject.TryGetComponent(out AudioSource audioSource))
                {
                    audioSource.Stop();//("Gloomstead");
                    print("stop");
                }
            }*/

        }
        float timer = 0;
        int myFrames = 0;
        int trueFrames = 0;
        public override void LateUpdate()
        {
            base.LateUpdate();
            CountFPS();

            Cam.Follow(player.position + (player.scale.x / 2, player.scale.y / 2));
            Cam.zoom += Input.GetMouseScroll() * 0.08f * Time.deltaTime;

            cursorObj.position = Input.GetMousePosition(true);


            //print(Input.GetMousePosition(true) + " | " + Input.GetMousePosition(false) + " | " + player.position);
            //if (box.gameObject.TryGetComponent(out Rigidbody2D rb)) rb.gravScale = 0;

            if (Input.ClickedMouse(MouseButtons.Left))
            {
                var newGround = new GameObject("Floor", trnfrm: new Transform
                    (pos: ((game.window.Size.Width / 2) - 200, (game.window.Size.Height / 2) + 50), scl: (300, 30), rot: 0)
                    , components: new List<Component>()
                {
                new Shape2D(Color.Blue, size: (1, 1), offset: Vector2.zero, Shape2D.ShapeType.Rectangle),
                new Collider2D(Vector2.zero,(1,1))
                }, lyr: "Ground"
                ).transform;

                newGround.position = cursorObj.position;
            }
            else
            {
                var newGround = new GameObject("Floor", trnfrm: new Transform
                    (pos: ((game.window.Size.Width / 2) - 200, (game.window.Size.Height / 2) + 50), scl: (300, 30), rot: 0)
                    , components: new List<Component>()
                {
                new Shape2D(Color.FromArgb(50, Color.Blue), size: (1, 1), offset: Vector2.zero, Shape2D.ShapeType.Rectangle),
                }, lyr: "Ground"
                ).transform;

                newGround.position = cursorObj.position;
                newGround.lifespan = 0.001f;
            }

            /*//Vector2 camPos = (graphics.DpiX / 2, graphics.DpiY / 2);
            Vector2 camPos = Cam.position;
            Vector2 targetPos = ((window.Width / 2), (window.Height / 2));

            if (Vector2.Distance(Cam.position, targetPos) > 0.2f)
                print("Player Pos: " + targetPos + " | Cam Pos: " + camPos);

            if (Math.Abs(camPos.x - targetPos.x) > 0.2f)
            {
                if (camPos.x < targetPos.x)//too far left
                    Cam.position.x += deltaTime * Cam.speed;//so move right
                else
                    Cam.position.x += deltaTime * -Cam.speed;
            }

            if (Math.Abs(camPos.y - targetPos.y) > 0.2f)
            {
                if (camPos.y < targetPos.y)//too far down
                    Cam.position.y += deltaTime * Cam.speed;//so move up
                else
                    Cam.position.y += deltaTime * -Cam.speed;
            }*/
        }

        void CountFPS(bool showTrueFPS = true)
        {
            if (timer >= 1)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                //print("My Timer: " + timer);
                print("My FPS: " + myFrames);
                Console.ForegroundColor = ConsoleColor.White;
                timer = 0;
                myFrames = 0;
            }
            timer += Time.deltaTime;
            //print("My Timer: " + timer);
            myFrames++;

            if (showTrueFPS && DateTime.UtcNow.Subtract(prevNow).TotalSeconds >= 1)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                //print("True Timer: " + DateTime.UtcNow.Subtract(prevNow).TotalSeconds);
                print("True FPS: " + trueFrames);
                Console.ForegroundColor = ConsoleColor.White;
                trueFrames = 0;
                prevNow = DateTime.UtcNow;
            }
            trueFrames++;
        }

        float fixedTimer = 0;
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            ////*print("Fixed Timer: " + fixedTimer);
            if (fixedTimer * Time.fixedDeltaTime >= 1)
            {
                //print("Fixed Second");
                fixedTimer = 0;
            }
            fixedTimer++;//*/
        }
    }

}
