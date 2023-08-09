using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CysmicEngine;

namespace CysmicPong
{
    class PongGame : CysmicGame
    {
        public PongGame(Vector2 winSize, string t, InterpolationMode im = InterpolationMode.NearestNeighbor, Color? bColor = null) : base(winSize, t, im, bColor)
        {
            if (bColor == null)
                backgroundColor = Color.Black;
            else
                backgroundColor = bColor.Value;
        }

        List<Team> teams;
        Vector2 screenCenter;
        private GameObject ball;
        bool gameOver = false;

        private int maxHp = 3;

        //UI
        Action<Control> AddControl;
        Action<Control> RemoveControl;
        Action<Control, string> ChangeText = new Action<Control, string>((control, text) => control.Text = text);
        Action<Control, Point> MoveControl = new Action<Control, Point>((control, point) => control.Location = point);
        Action<Label, ContentAlignment> ChangeTextAlignment = new Action<Label, ContentAlignment>((label, align) => label.TextAlign = align);
        Action<Control, Font> ChangeFont = new Action<Control, Font>((control, font) => control.Font = font);
        Action<Label, bool> ToggleAutoSize = new Action<Label, bool>((control, auto) => control.AutoSize = auto);
        Label scoreLable = new Label();

        public Team GetTeamByGoal(GameObject goal)
        {
            for (int i = 0; i < teams.Count; i++)
            {
                if (teams[i].goal == goal)
                    return teams[i];
            }

            throw new Exception("Goal, " + goal.name + ", was not associated with a team.");
        }

        public void Score(GameObject goal)
        {
            var team = GetTeamByGoal(goal);

            team.hp--;
            if(team.hp <= 0)
            {
                gameOver = true;
                SetWinnerText();
            }
        }

        void SetWinnerText()
        {
            int teamsAlive = 0;
            Team winner = null;
            for (int i = 0; i < teams.Count; i++)
            {
                if (teams[i].hp > 0)
                {
                    winner = teams[i];
                    teamsAlive++;
                }
            }
            if (teamsAlive > 1)
                return;

            //window.Invoke(AddControl, winnerLable);
            scoreLable.Invoke(ChangeText, scoreLable, winner.name + " Wins!");//.Text = team.name + " lost the game!";
            scoreLable.Invoke(ChangeFont, scoreLable, new Font(FontFamily.GenericMonospace, 24, FontStyle.Bold));
            scoreLable.Invoke(ChangeTextAlignment, scoreLable, ContentAlignment.MiddleCenter);
            //winnerLable.Size = new Size(400, 80);
            scoreLable.Invoke(MoveControl, scoreLable, new Point((int)(defaultResolution.x / 2) - scoreLable.Size.Width / 2, 80));
            scoreLable.BackColor = Color.Crimson;
            scoreLable.ForeColor = Color.Black;
            //window.Controls.Add(loseLabel);//Can't be cross-thread
        }

        /*protected override void GameLoop()
        {
            //This didn't work
            /*window = new Canvas();
            window.Text = title;
            window.Size = new Size((int)defaultResolution.x, (int)defaultResolution.y);*
        //InitWindow();
        //Application.Run(window);

        Thread.Sleep(1000);
            if (window.IsDisposed)
            {
                //GameLoopThread.Abort();
                Play();
                return;
            }
            Thread.Sleep(1000);

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
        }*/

        public override void OnStart()
        {
            //When trying to switch games, window Disposes right before this
            if (AddControl == null)
                AddControl += (control) => { window.Controls.Add(control); };

            if (RemoveControl == null)
                RemoveControl += (control) => { window.Controls.Remove(control); };

            //while (window.Disposing || window.IsDisposed) ;
            window.Invoke(AddControl, scoreLable);
            scoreLable.Invoke(ToggleAutoSize, scoreLable, true);


            teams = new List<Team>(2) { new Team(maxHp), new Team(maxHp) };
            screenCenter = ((game.window.Size.Width / 2), (game.window.Size.Height / 2));
            //addControl += Program.AddControl;
            float thickness = (defaultResolution.x / defaultResolution.y) * 16;
            float ratio = (defaultResolution.x / defaultResolution.y);


            new GameObject("Top Border", new Transform(screenCenter + (-defaultResolution.x / 2, -defaultResolution.y / 2), (defaultResolution.x, thickness), 0), new List<Component>()
            {
                new Shape2D(Color.Crimson, srtOdr: 500),
                new Collider2D((0f, 0f), (1, 1)),
                new Rigidbody2D(true, 0f),
            });

            teams[0].goal = new GameObject("Left Border", new Transform(screenCenter + (-defaultResolution.x / 2, -defaultResolution.y / 2), (thickness, defaultResolution.y), 0), new List<Component>()
            {
                new Shape2D(Color.Magenta, srtOdr: 400),
                //new Collider2D((0f, 0f), (1, 1)),
                new Collider2D((0f, 0f), (1f, 1f), true),
                new Rigidbody2D(true, 0f),
            });//layer "Goal" is assigned as a part of the goal set property

            teams[1].goal = new GameObject("Right Border", new Transform(screenCenter + ((defaultResolution.x / 2) - (thickness * ratio * 1.25f), -defaultResolution.y / 2), (thickness, defaultResolution.y), 0), new List<Component>()
            {
                new Shape2D(Color.Magenta, srtOdr: 400),
                //new Collider2D((0f, 0f), (1, 1)),
                new Collider2D((0f, 0f), (1f, 1f), true),
                new Rigidbody2D(true, 0f),
            });

            new GameObject("Bottom Border", new Transform(screenCenter + (-defaultResolution.x / 2, (defaultResolution.y / 2) - (thickness * ratio * 2f)), (defaultResolution.x, thickness), 0), new List<Component>()
            {
                new Shape2D(Color.Crimson, srtOdr: 500),
                new Collider2D((0f, 0f), (1, 1)),
                new Rigidbody2D(true, 0f),
            });

            SpawnBall();

            new GameObject("Left Blocker", new Transform(screenCenter + ((-defaultResolution.x / 2) + (thickness * ratio * 1.75f), 0), (thickness, defaultResolution.y / 6), 0), new List<Component>()
            {
                new Shape2D(Color.Crimson),
                new Collider2D((0f, 0f), (1, 1)),
                new Rigidbody2D(),
                new KeeperController(PlayerNumber.ONE)
            }).layer = "Keeper";
            new GameObject("Right Blocker", new Transform(screenCenter + ((defaultResolution.x / 2) - (thickness * ratio * 3f), 0), (thickness, defaultResolution.y / 6), 0), new List<Component>()
            {
                new Shape2D(Color.Crimson),
                new Collider2D((0f, 0f), (1, 1)),
                new Rigidbody2D(false, 0),
                new KeeperController(PlayerNumber.TWO)
            }).layer = "Keeper";
        }

        float timer = 0;
        int myFrames = 0;
        int trueFrames = 0;
        DateTime prevNow;

        public override void Update()
        {
            base.Update();
            if (Input.PressedKey(Keys.G))
            {
                displayGizmos = !displayGizmos;
                /*if(Time.targetFPS >= 60)
                    Time.targetFPS = 15;
                else
                    Time.targetFPS = 60;*/
            }    

            if (ball == null || ball.wasDestroyed)
            {
                ball = null;
                SpawnBall();
            }

            if (gameOver && Input.PressedKey(Keys.R))
            {
                //window.Invoke(RemoveControl, winnerLable);
                for (int i = 0; i < teams.Count; i++)
                {
                    teams[i].hp = maxHp;
                }
                gameOver = false;
            }

            //CountFPS();
            //print(Time.targetFPS);

            if (!gameOver)
            {
                scoreLable.Invoke(ChangeText, scoreLable, teams[0].hp + " - " + teams[1].hp);
                scoreLable.Invoke(ChangeTextAlignment, scoreLable, ContentAlignment.MiddleCenter);
                scoreLable.Invoke(ChangeFont, scoreLable, new Font(FontFamily.GenericMonospace, 18, FontStyle.Bold));
                //winnerLable.Size = new Size(400, 80);
                scoreLable.Invoke(MoveControl, scoreLable, new Point((int)(defaultResolution.x / 2) - scoreLable.Size.Width / 2, 80));
                scoreLable.BackColor = Color.FromArgb(0, Color.Black);
                scoreLable.ForeColor = Color.Magenta;
            }
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

        void SpawnBall()
        {
            if (ball != null || gameOver)
            {
                return;
            }

            ball = new GameObject("Ball", new Transform(screenCenter, (32, 32), 0), new List<Component>()
            {
                new Shape2D(Color.Crimson, shapeType: Shape2D.ShapeType.Circle),
                new Rigidbody2D(false, 0f),
                new Collider2D((0f, 0f), (1f, 1f)),
                //new Collider2D((0f, 0f), (1f, 1f), true),
                new BallController()
            });
            ball.layer = "Ball";
        }

        public class Team
        {
            public string name = "";
            public int hp = 10;
            public GameObject _goal;
            public GameObject goal{ get => _goal; 
                set
                {
                    _goal = value;
                    _goal.layer = "Goal";
                    if (name == "")
                        name = _goal.name.Split(' ')[0] + " Team";
                }
            }

            public Team(int _hp, string _name = "")
            {
                name = _name;
                hp = _hp;
            }

        }
    }
}
