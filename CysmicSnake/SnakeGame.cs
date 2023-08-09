using CysmicEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CysmicSnake
{

    class SnakeGame : CysmicGame
    {
        public SnakeGame(Vector2 winSize, string t, InterpolationMode im = InterpolationMode.NearestNeighbor, Color? bColor = null) : base(winSize, t, im, bColor)
        {
            //displayGizmos = true;
        }

        SnakeController mainSnake;
        List<SnakeController> snakesInPlay = new List<SnakeController>();
        public static Grid grid;
        bool isRestarting = false;

        public override void OnStart()
        {
            snakesInPlay = new List<SnakeController>();
            Vector2 screenCenter = ((game.window.Size.Width / 2), (game.window.Size.Height / 2));

            grid = new Grid(Vector2.zero + (200, 50), (18, 18));
            grid.DrawGrid();

            mainSnake = new GameObject("Mainey", trnfrm: new Transform
                (screenCenter - (0, 90), scl: (grid.cellSize, grid.cellSize), 0)
                , components: new List<Component>()
            {
                //new SpriteRenderer("Main Characters/Virtual Guy/Fall (32x32)", 50),
                new Shape2D(Color.Blue, size: (1,1), offset: Vector2.zero, srtOdr: 50),
                //new PlayerMotor(),
                //new InputController(),
                new Rigidbody2D(),
                new Collider2D((grid.cellSize / 4, grid.cellSize / 4), (0.5f, 0.5f), isTrig: false),
                new SnakeController((4, 4)),
                //new AudioSource()
                //new Collider2D(Vector2.Zero, (10, 10)),
            }, lyr: "Snake Head"
            ).GetComponent<SnakeController>();
            mainSnake.transform.isStatic = false;
            mainSnake.gameObject.GetComponent<Rigidbody2D>().gravScale = 0;
            mainSnake.playerControlled = false;

            /*var botSnake = new GameObject("Greenie", trnfrm: new Transform
                (screenCenter - (0, 90), scl: (grid.cellSize, grid.cellSize), 0)
                , components: new List<Component>()
            {
                //new SpriteRenderer("Main Characters/Virtual Guy/Fall (32x32)", 50),
                new Shape2D(Color.Blue, size: (1,1), offset: Vector2.zero, srtOdr: 50),
                //new PlayerMotor(),
                //new InputController(),
                new Rigidbody2D(),
                new Collider2D((grid.cellSize / 4, grid.cellSize / 4), (0.5f, 0.5f), isTrig: false),
                new SnakeController((1, 1)),
                //new AudioSource()
                //new Collider2D(Vector2.Zero, (10, 10)),
            }, lyr: "Snake Head"
            ).GetComponent<SnakeController>();
            botSnake.playerControlled = false;
            botSnake.transform.isStatic = false;
            botSnake.gameObject.GetComponent<Rigidbody2D>().gravScale = 0;
            //botSnake.bot.botMoveDelay = 0.1f;
            botSnake.headColor = Color.YellowGreen;
            botSnake.bodyColors = new Color[2] { Color.Green, Color.ForestGreen };*/


            snakesInPlay.Add(mainSnake);
            //snakesInPlay.Add(botSnake);

            grid.SpawnAppleRandomly();
            isRestarting = false;
        }

        float restartTimer = 0;
        float restartDelay = 3;
        //private int moveFrameStagger;

        public override void Update()
        {
            base.Update();
            if (Input.PressedKey(Keys.G))
                displayGizmos = !displayGizmos;

            if (Input.PressedKey(Keys.Enter))
            {
                mainSnake.playerControlled = !mainSnake.playerControlled;
                if (mainSnake.playerControlled)
                    mainSnake.moveDelay = 0.1f;
                else
                    mainSnake.moveDelay = mainSnake.bot.botMoveDelay;
            }

            if (Input.PressedKey(Keys.Escape) || (!isRestarting && snakesInPlay.Exists(x => x.hasDied) && (Input.PressedKey(Keys.R) || (restartDelay >= 0 && restartTimer >= restartDelay) )))
            {
                //_ = DestroyAll();
                isRestarting = true;
                restartTimer = 0;
            }
            if (!isRestarting && snakesInPlay.Exists(x => x.hasDied))
                restartTimer += Time.deltaTime;

            if(isRestarting)
            {
                for (int i = 0; i < allGameObjects.Count; i++)
                {
                    Destroy(allGameObjects[i]);
                }
                if (allGameObjects.Count == 0)
                    OnStart();
            }


            //Group Snake Control
            for (int i = 0; i < snakesInPlay.Count; i++)
            {
                /*if (moveFrameStagger != i)
                    continue;*/
                if (!snakesInPlay[i].moveOutOfTurn)
                    snakesInPlay[i].SetMoveTimer(-1);

                /*if (snakesInPlay[i].moveTimer >= snakesInPlay[i].moveDelay)
                {
                    moveFrameStagger++;
                    System.Threading.Thread.Sleep(200);
                    break;
                }*/
            }

            /*if (moveFrameStagger >= snakesInPlay.Count)
                moveFrameStagger = 0;*/
        }

        /*async Task DestroyAll()
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < allGameObjects.Count; i++)
                {
                    Destroy(allGameObjects[i]);
                }
            });
            OnStart();
        }*/
    }
}
