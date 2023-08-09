using CyTools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace CysmicEngine
{
    public class Canvas : Form
    {
        public Canvas()
        {
            DoubleBuffered = true;
        }
    }

    public class CE_Common : Basic
    {
        public static float Lerp(float start, float end, float percentage)
        {
            return start + percentage * (end - start);
        }
    }

    public static class Time
    {
        public static DateTime prevFrameTime = DateTime.Today;
        public const float fixedDeltaTime = 0.02f;
        public static float targetFPS = 60;
        static float _deltaTime = 0;
        public static float deltaTime { get { return _deltaTime; } }

        /// <summary>Only for use in the main GameLoop function</summary>
        public static void private_SetDT(float newDT)
        {
            _deltaTime = newDT;
        }
    }

    /// <summary>
    /// The basis for your game. Create a child class and call Play() at the start of the program.
    /// </summary>
    public abstract class CysmicGame : Basic
    {
        public static bool displayGizmos = false;

        Vector2 _defaultResolution = new Vector2(512, 512);
        public Vector2 defaultResolution { get => _defaultResolution; protected set => _defaultResolution = value; }
        string title = "New Game";
        public Canvas window = null;
        protected Thread GameLoopThread;
        protected Thread PhysicsThread;
        InterpolationMode drawMode;

        public static List<GameObject> allGameObjects = new List<GameObject>();
        public static HashSet<GizmoObj> allGizmos = new HashSet<GizmoObj>();
        public static HashSet<Renderer> allRenderers = new HashSet<Renderer>();
        public static HashSet<Collider2D> allColliders = new HashSet<Collider2D>();

        public static CysmicGame game;
        public Color backgroundColor = Color.Beige;


        public CysmicGame(Vector2 winSize, string _title, InterpolationMode interpolationMode = InterpolationMode.NearestNeighbor, Color? bColor = null)
        {
            defaultResolution = winSize;
            title = _title;
            drawMode = interpolationMode;

            if (bColor == null)
                backgroundColor = Color.Beige;
            else
                backgroundColor = bColor.Value;
        }

        /// <summary>
        /// Starts the game
        /// </summary>
        public void Play()
        {
            Time.prevFrameTime = DateTime.MinValue;
            //window.Paint += InitGraphics;//Render Function will be added later to paint

            InitWindow();

            GameLoopThread = new Thread(GameLoop);
            game = this;

            GameLoopThread.Start();
            //Start fixed loop after first frame of game loop

            Application.Run(window);//Must come last
        }

        protected void InitWindow()
        {
            window = null;
            window = new Canvas();
            window.Text = title;
            window.Size = new Size((int)defaultResolution.x, (int)defaultResolution.y);

            window.Paint += Render;
            window.KeyDown += Input.Win_KD_Event;
            window.KeyUp += Input.Win_KU_Event;

            window.MouseWheel += Input.Window_MouseWheel;
            window.MouseMove += Input.Window_MouseMove;
            window.MouseClick += Input.Window_MouseClick;
            window.MouseDoubleClick += Input.Window_MouseDoubleClick;
            window.MouseUp += Input.Window_MouseUp;

            window.MouseEnter += Window_MouseEnter;
            window.MouseLeave += Window_MouseLeave;

            window.FormClosing += Window_FormClosing;
            window.FormClosed += Window_FormClosed;
        }

        private void Window_MouseLeave(object sender, EventArgs e)
        {
            Cursor.Show();
        }

        private void Window_MouseEnter(object sender, EventArgs e)
        {
            Cursor.Hide();
        }

        private void Window_FormClosing(object sender, FormClosingEventArgs e)
        {
            allRenderers.Clear();
            for (int i = 0; i < allGameObjects.Count; i++)
            {
                allGameObjects[i] = null;
            }
            allGameObjects.Clear();
        }
        private void Window_FormClosed(object sender, FormClosedEventArgs e)
        {
            GameLoopThread?.Abort();
            PhysicsThread?.Abort();
            Application.Exit();
        }

        protected void FixedLoop()
        {
            while (PhysicsThread.IsAlive)
            {
                FixedUpdate();

                Thread.Sleep(TimeSpan.FromSeconds(Time.fixedDeltaTime));
            }
        }

        protected virtual void GameLoop()
        {
            //This didn't work
            /*window = new Canvas();
            window.Text = title;
            window.Size = new Size((int)defaultResolution.x, (int)defaultResolution.y);*/
            //InitWindow();
            //Application.Run(window);


            while (window.IsDisposed) ;
            OnStart();
            float desiredFrameTime = 1f / Time.targetFPS;
            while (GameLoopThread.IsAlive)
            {
                try
                {
                    if (Time.prevFrameTime == DateTime.MinValue)//update every "deltaTime" seconds
                    {
                        Time.prevFrameTime = DateTime.UtcNow;
                        if (TimeSpan.FromSeconds(desiredFrameTime - Time.deltaTime).TotalSeconds >= 0)
                        {
                            float frameOffset = (float)TimeSpan.FromSeconds(desiredFrameTime - Time.deltaTime).TotalSeconds;
                            //print($"deltaTime ({_deltaTime}) + frameOffset ({frameOffset})" +
                            //  $" = {_deltaTime + frameOffset}.");
                            Thread.Sleep(TimeSpan.FromSeconds(frameOffset));
                        }
                        else
                            Thread.Sleep(1);//some delay is needed to avoid potential crashes

                        Update();

                        var asyncDraw = window.BeginInvoke((MethodInvoker)delegate { window.Refresh(); });//Draw Frame

                        while (!asyncDraw.IsCompleted) ;

                        LateUpdate();

                        //Calculate deltaTime
                        var newFrameTime = DateTime.UtcNow;
                        Time.private_SetDT((float)newFrameTime.Subtract(Time.prevFrameTime).TotalSeconds);
                        Time.prevFrameTime = DateTime.MinValue;
                    }
                    else
                    {
                        Time.prevFrameTime = DateTime.MinValue;
                    }
                }
                catch
                {
                    print("Game is loading...");
                }

                //Input.ClearReleasedKeys();//moved to late update
                if (PhysicsThread == null)
                {
                    PhysicsThread = new Thread(FixedLoop);
                    PhysicsThread.Start();
                }
            }
            //if the game loop ever stops then close the game
            Application.Exit();
        }

        private void Render(object sender, PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            //graphics.Clear(backgroundColor);
            /*if (!isFullySetUp)
                return;*/
            if (Cam.zoom <= 0.1f)//Max zoom out value
                Cam.zoom = 0.1f;

            //print(window.Width / 2 + " | " + Cam.camCenter);
            //print(graphics.Transform.OffsetX + " | " + Cam.position.x);\
            graphics.TranslateTransform(Cam.position.x /* (defaultResolution.x / window.Width)*/, Cam.position.y);
            graphics.ScaleTransform(Cam.zoom * (window.Width / defaultResolution.x), Cam.zoom * (window.Height / defaultResolution.y));
            Cam.p_SetCenter(window);
            Input.p_SetMousePostionFromWindow(window);

            graphics.InterpolationMode = drawMode;
            graphics.SmoothingMode = SmoothingMode.HighSpeed;

            //allRenderers.RemoveWhere(x => x == null || x.gameObject == null || x.gameObject.wasDestroyed || x.transform.wasDestroyed);
            var orderedRenderers = allRenderers.OrderBy(x => x?.sortOrder).ToHashSet();
            allRenderers = orderedRenderers;

            try
            {
                graphics.Clear(backgroundColor);//moved
                foreach (var renderer in orderedRenderers)
                {
                    renderer.Draw(graphics);
                }

                if (displayGizmos)
                {
                    allGizmos.RemoveWhere(gizmo => gizmo.transform.gameObject.wasDestroyed);
                    foreach (var gizmo in allGizmos)
                    {
                        gizmo.renderer.Draw(graphics);
                    }
                }
            }
            catch
            {
                //print("FRAME COULDN'T BE DRAWN");
                //graphics.Clear(Color.Beige);
            }
        }

        public GameObject Duplicate(GameObject gameObject)
        {
            return new GameObject(gameObject);
        }
        public static void Destroy(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            try
            {
                for (int i = 0; i < gameObject.allComponents.Count; i++)
                {
                    game.OnUpdate -= gameObject.allComponents[i].OnUpdate;
                    game.OnLateUpdate -= gameObject.allComponents[i].OnLateUpdate;
                    game.OnFixedUpdate -= gameObject.allComponents[i].OnFixedUpdate;
                    if(gameObject.allComponents[i].gameObject != null) gameObject.allComponents[i].gameObject._wasDestroyed = true;
                    gameObject.allComponents[i] = null;
                }

                if(allGameObjects.IndexOf(gameObject) >= 0)
                    allGameObjects[allGameObjects.IndexOf(gameObject)] = null;
                gameObject = null;
                ClearNullGameObjects();
            }
            catch
            {

            }
        }

        /// <summary>
        /// Called once, right before the game starts
        /// </summary>
        public abstract void OnStart();

        public static void ClearNullGameObjects()
        {
            allGameObjects.RemoveAll(x => x == null || x.wasDestroyed == true);
            allColliders.RemoveWhere(x => x == null || x.gameObject.wasDestroyed == true);
            allRenderers.RemoveWhere(x => x == null || x.gameObject.wasDestroyed == true);
        }

        public Action OnUpdate;
        /// <summary>
        /// Called before the next frame is drawn. If overrided, the base function must be called.
        /// </summary>
        public virtual void Update()
        {
            ClearNullGameObjects();
            OnUpdate.Invoke();
        }

        public Action OnLateUpdate;
        /// <summary>
        /// Called after the next frame is drawn. If overrided, the base function must be called.
        /// </summary>
        public virtual void LateUpdate()
        {
            ClearNullGameObjects();
            OnLateUpdate.Invoke();

            if (Input.pressedOnThisFrame)
                Input.ClearPressedKeys();
            if (Input.releasedOnThisFrame)
                Input.ClearReleasedKeys();

            if (Input.clickedMouseOnThisFrame)
                Input.ClearMouseClicks();
            if (Input.releasedMouseOnThisFrame)
                Input.ClearMouseReleases();
            if (Input.scrolledMouseOnThisFrame)
                Input.ResetMouseScroll();

            Input.pressedOnThisFrame = true;
            Input.releasedOnThisFrame = true;
            Input.clickedMouseOnThisFrame = true;
            Input.releasedMouseOnThisFrame = true;
            Input.scrolledMouseOnThisFrame = true;
        }

        public Action OnFixedUpdate;
        /// <summary>
        /// Called at a fixed rate; Use for physics calculation
        /// </summary>
        public virtual void FixedUpdate()
        {
            ClearNullGameObjects();
            OnFixedUpdate?.Invoke();
        }
    }
}
