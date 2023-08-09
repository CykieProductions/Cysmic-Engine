using CysmicEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CyTools.Basic;

namespace CysmicSnake
{
    class Grid
    {
        public Random random = new Random();
        public Vector2 startPos;
        public Vector2Int size;
        public int cellSize = 24;

        public Transform apple;
        public Vector2Int appleCell;

        //string[,] cellData;

        public Grid(Vector2 sPos, Vector2Int _size)
        {
            startPos = sPos;
            size = _size;
            //cellData = new string[size.x, size.y];
        }

        public void MoveToCell(Vector2Int cell, Transform transform)
        {
            transform.position = startPos + (cell.x * cellSize, cell.y * cellSize);
            //cellData[x, y] = transform.gameObject.layer;
        }

        public void SpawnAppleRandomly()
        {
            var x = random.Next(0, size.x);
            var y = random.Next(0, size.y);

            apple = new GameObject($"Apple ({x},{y})", trnfrm: new Transform
                            (startPos + (x * cellSize, y * cellSize), scl: (cellSize, cellSize)),
                            components: new List<Component>()
                            {
                                //new Shape2D(Color.Red, (1, 1), Vector2.Zero, fill: true),
                                new Collider2D((0, 0), (1, 1), isTrig: true),
                                new Rigidbody2D()
                            }
                            ).transform;
            apple.gameObject.layer = "Apple";
            MoveToCell((x, y), apple);
            bool validCell = false;

            do
            {
                validCell = !IsOverlappingPosition(apple);
                if(!validCell)
                {
                    //print($"Apple overlapped at {apple.transform.position}; Trying again...");
                    x = random.Next(0, size.x);
                    y = random.Next(0, size.y);
                    MoveToCell((x, y), apple.transform);
                }
            }
            while (!validCell);

            appleCell = (x, y);
            //print($"Apple placed at {apple.transform.position}");
            apple.gameObject.AddComponent(new Shape2D(Color.Red, (1, 1), Vector2.zero, fill: true));//this can be replace with enabling the GameObject or component
        }
        public bool IsOverlappingPosition(Transform self, Vector2? posToCheck = null)
        {
            if (posToCheck == null)
                posToCheck = self.position;

            for (int i = 0; i < CysmicGame.allColliders.Count; i++)
            {
                var collider = CysmicGame.allColliders.ElementAt(i);
                //print(collider.transform.position + "\t| " + apple.transform.position);
                if (collider.transform != self && Vector2.Distance(collider.transform.position, posToCheck.Value) < 0.2f)//overlapping cell
                {
                    return true;
                }
            }
            return false;
        }
        public bool IsOverlappingCell(Transform self, Vector2Int? cellToCheck = null)
        {
            if (cellToCheck == null)
                cellToCheck = (Vector2Int)((self.position - startPos) / cellSize);

            for (int i = 0; i < CysmicGame.allColliders.Count; i++)
            {
                var collider = CysmicGame.allColliders.ElementAt(i);
                Vector2Int otherCell = (collider.transform.position - startPos) / cellSize;

                //print(collider.transform.position + "\t| " + apple.transform.position);
                var snakeSelf = self.gameObject.GetComponent<SnakeController>();
                if ((snakeSelf == null || (snakeSelf != null && (snakeSelf.segments.Count < 2 || collider.transform != snakeSelf.segments[snakeSelf.segments.Count - 1].transform) )) && (collider.gameObject.layer == "Border" || collider.gameObject.layer == "Snake Head" || collider.gameObject.layer == "Neck") && collider.transform != self && cellToCheck == otherCell)//overlapping cell
                {
                    //print("About to hit " + collider.gameObject.name);
                    //var giz = new GizmoObj(/*new Transform(Vector2.Zero, (cellSize, cellSize))*/, new Shape2D(Color.HotPink, fill: false, srtOdr: 999));
                    Color gizColor = Color.HotPink;

                    if (collider.gameObject.layer == "Snake Head")
                        gizColor = Color.DarkCyan;

                    var giz = new GameObject("giz", new Transform(Vector2.zero, (cellSize, cellSize)), new List<Component>() 
                    {
                        new Shape2D(gizColor, (1, 1), (0, 0), fill: false, srtOdr: 999)
                    });
                    //giz.AddComponent(new Shape2D(Color.HotPink, fill: false, srtOdr: 999));
                    giz.transform.lifespan = 2f;
                    giz.transform.scale = (cellSize, cellSize);
                    MoveToCell(otherCell, giz.transform);
                    return true;
                }
            }
            return false;
        }


        public void DrawGrid()
        {
            //cellData = new string[size.x, size.y];
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    if ((x == 0 || x == size.x - 1) || (y == 0 || y == size.y - 1))
                    {
                        var box = new GameObject($"Border ({x},{y})", trnfrm: new Transform
                            (startPos + (x * cellSize, y * cellSize), scl: (cellSize, cellSize)),
                            components: new List<Component>()
                            {
                            new Shape2D(CysmicGame.game.backgroundColor, (1, 1), Vector2.zero, fill: true),
                            new Collider2D((0, 0), (1, 1), isTrig: true),
                            new Rigidbody2D()
                            }
                            );
                        box.layer = "Border";
                        //box.transform.isStatic = false;
                    }
                    else
                    {
                        new GameObject($"Background ({x},{y})", trnfrm: new Transform
                            (startPos + (x * cellSize, y * cellSize), scl: (cellSize, cellSize)),
                            components: new List<Component>()
                            {
                            new Shape2D(Color.Beige, (1, 1), Vector2.zero, fill: true, srtOdr: int.MinValue),
                            //new Collider2D((0, 0), (1, 1), isTrig: true),
                            //new Rigidbody2D()
                            }
                            );
                    }

                }
            }
        }

    }
}
