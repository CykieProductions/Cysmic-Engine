using CysmicEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CysmicSnake
{
    class SnakeController : Component
    {
        public class SnakeSegment
        {
            public Transform transform;
            public Vector2Int curCell;
            public Vector2Int direction;


            public SnakeSegment(Transform _transform, Vector2Int _curCell)
            {
                transform = _transform;
                curCell = _curCell;
            }
        }

        public bool playerControlled = true;
        public BotPlayer bot;
        Grid grid;
        Shape2D headRenderer;
        public Color headColor = Color.Blue;
        public Color[] bodyColors = new Color[2] { Color.BlueViolet, Color.DarkViolet };
        public Vector2Int startCell = (4, 4);

        public float moveDelay = 0.08f;
        /*public float moveDelay { get => _moveDelay; set
            {
                _moveDelay = value + grid.random.Next(1, 3) / 1000f;
            }
        }*/
        float _moveTimer = 0;
        public float moveTimer { get => _moveTimer; private set => _moveTimer = value; }

        public Vector2Int curCell;
        DateTime startTime;

        public Vector2Int inputDir = (1, 0);
        Vector2Int _direction = (1, 0);
        public Vector2Int direction { get => _direction; private set => _direction = value; }
        public List<SnakeSegment> segments = new List<SnakeSegment>();

        public bool _hasDied = false;
        public bool moveOutOfTurn = false;

        public bool hasDied { get => _hasDied; protected set => _hasDied = value; }


        public SnakeController(Vector2Int sCell)
        {
            startCell = sCell;
        }

        protected override void Start()
        {
            base.Start();
            if (gameObject.TryGetComponent(out headRenderer)) headRenderer.color = headColor;
            startTime = DateTime.Now;

            grid = SnakeGame.grid;

            grid.MoveToCell(startCell, transform);
            curCell = startCell;

            //playerControlled = false;
            bot = new BotPlayer(this);

            bot.botMoveDelay += grid.random.Next(1, 4) / (float)grid.random.Next(500, 1000);
            moveDelay += grid.random.Next(1, 4) / (float)grid.random.Next(500, 1000);
        }

        protected override void Update()
        {
            base.Update();

            if (hasDied)
                return;

            headRenderer.color = headColor;
            if (moveTimer >= moveDelay)
            {
                direction = inputDir;
                var prevCell = curCell;
                curCell += direction;
                grid.MoveToCell(curCell, transform);

                for (int i = 0; i < segments.Count; i++)
                {
                    var targetCell = prevCell;
                    var targetDir = direction;
                    if (i != 0)//not segment behind head
                    {
                        targetDir = segments[i - 1].direction;
                        targetCell = segments[i - 1].curCell - targetDir;
                    }

                    grid.MoveToCell(targetCell, segments[i].transform);
                    segments[i].curCell = targetCell;
                }
                for (int i = segments.Count - 1; i >= 0; i--)
                {
                    var targetDir = direction;
                    if (i != 0)//not segment behind head
                        targetDir = segments[i - 1].direction;
                    segments[i].direction = targetDir;
                }

                //moveTimer = 0;//set later
            }

            if (moveOutOfTurn)
                moveTimer += Time.deltaTime;

            if (playerControlled)
            {
                if (Input.GetAxis(AxisName.HORIZONTAL) > 0 && direction != new Vector2Int(-1, 0))//right
                    inputDir = (1, 0);
                else if (Input.GetAxis(AxisName.HORIZONTAL) < 0 && direction != new Vector2Int(1, 0))//left
                    inputDir = (-1, 0);

                if (Input.GetAxis(AxisName.VERTICAL) > 0 && direction != new Vector2Int(0, 1))//up
                    inputDir = (0, -1);
                else if (Input.GetAxis(AxisName.VERTICAL) < 0 && direction != new Vector2Int(0, -1))//down
                    inputDir = (0, 1);
            }
            else if (moveTimer >= moveDelay)
                bot.Act();
            
            if (moveTimer >= moveDelay)
                moveTimer = 0;
        }

        void Grow()
        {
            var newSeg = new GameObject($"Snake Segment {segments.Count + 1}", trnfrm: new Transform
                            (Vector2.zero, scl: (grid.cellSize, grid.cellSize)),
                            components: new List<Component>()
                            {
                            new Shape2D(Color.BlueViolet, (1, 1), Vector2.zero, fill: true, srtOdr: -10),
                            new Collider2D((0, 0), (1, 1), isTrig: true),
                            new Rigidbody2D()
                            }
                            );
            if(segments.Count > 0)//first one shouldn't kill you (sometimes it glitched into you or something)
                newSeg.layer = "Border";
            else
                newSeg.layer = "Neck";
            var targetCell = curCell;
            if (segments.Count == 0)//this is the first one
                targetCell -= direction;
            else
                targetCell = segments[segments.Count - 1].curCell - segments[segments.Count - 1].direction;//spawn behind last segment
            grid.MoveToCell(targetCell, newSeg.transform);

            if (segments.Count % 2 == 0)
            {
                if (newSeg.TryGetComponent(out Shape2D shape2D)) shape2D.color = bodyColors[0];
            }
            else
            {
                if (newSeg.TryGetComponent(out Shape2D shape2D)) shape2D.color = bodyColors[1];
            }

            segments.Add(new SnakeSegment(newSeg.transform, curCell));
        }

        /// <summary></summary>
        /// <param name="timer">Set to negative to add deltaTime * Abs(timer)</param>
        public void SetMoveTimer(float timer = 0)
        {
            if (timer < 0)
                moveTimer += Time.deltaTime * Math.Abs(timer);
            else
                moveTimer = timer;
        }

        public override void OnCollisionEnter(Rigidbody2D.Collision collision)
        {
            base.OnCollisionEnter(collision);

            var self = collision.self;
            var other = collision.other;

            print("PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP");
            Die();
        }

        public override void OnTriggerEnter(Collider2D other)
        {
            if (hasDied)
                return;
            base.OnTriggerEnter(other);
            //print("adasdfasdvsadf");

            if (other.gameObject.layer == "Apple")
            {
                CysmicGame.Destroy(other.gameObject);
                grid.SpawnAppleRandomly();
                Grow();
            }
            else if (other.gameObject.layer == "Border")
            {
                Die();
            }
        }
        void Die()
        {
            hasDied = true;
            gameObject.GetComponent<Shape2D>().color = Color.DarkMagenta;
            //print("Death by: " + other.gameObject.name);
            Console.ForegroundColor = ConsoleColor.Cyan;
            print(gameObject.name + " was eliminated!");
            print("Final Score: " + segments.Count);
            print($"Round Time: {(int)(DateTime.Now - startTime).TotalMinutes}:{((int)(DateTime.Now - startTime).TotalSeconds - (60 * (int)(DateTime.Now - startTime).TotalMinutes)).ToString("00")}");
            Console.ForegroundColor = ConsoleColor.White;
            //CysmicGame.Destroy(gameObject);
        }
    }

}
