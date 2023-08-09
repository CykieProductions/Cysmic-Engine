using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CysmicEngine;

namespace CysmicSnake
{
    class BotPlayer
    {
        SnakeController snake;
        Grid grid;

        public float botMoveDelay = 0.08f;

        public BotPlayer(SnakeController snakeController)
        {
            snake = snakeController;
            grid = SnakeGame.grid;
        }

        void GoRight()
        {
            if (snake.direction != new Vector2Int(-1, 0))
                snake.inputDir = (1, 0);
        }
        void GoLeft()
        {
            if (snake.direction != new Vector2Int(1, 0))
                snake.inputDir = (-1, 0);
        }
        void GoUp()
        {
            if (snake.direction != new Vector2Int(0, 1))
                snake.inputDir = (0, -1);
        }
        void GoDown()
        {
            if (snake.direction != new Vector2Int(0, -1))
                snake.inputDir = (0, 1);
        }

        public void Act()
        {
            //snake.moveDelay = botMoveDelay;//Revert freeze
            var appleCell = (Vector2Int)((grid.apple.position - grid.startPos) / grid.cellSize);
            //var appleCell = grid.appleCell;//same thing ^

            void LastCorrection()
            {
                if (!grid.IsOverlappingCell(snake.transform, snake.curCell + snake.inputDir))
                    return;

                //snake.moveDelay = 0.5f;//Freeze effect
                var prevDir = snake.inputDir;
                snake.inputDir = (prevDir.y, -prevDir.x);//-x since y values are flipped?
                if (grid.IsOverlappingCell(snake.transform, snake.curCell + snake.inputDir))//sometimes triggers when it shouldn't
                    snake.inputDir = -(Vector2)snake.inputDir;
                if (grid.IsOverlappingCell(snake.transform, snake.curCell + snake.inputDir))
                    snake.inputDir = prevDir;
            }
            void AvoidBorder()
            {
                if (!grid.IsOverlappingCell(snake.transform, snake.curCell + snake.inputDir))
                    return;

                if (snake.inputDir.y != 0)//travelling vertically
                {
                    if (appleCell.x - snake.curCell.x > 0)//apple is to the right
                        GoRight();
                    else if (appleCell.x - snake.curCell.x < 0)//apple is to the left
                        GoLeft();
                    
                    LastCorrection();
                }
                else if (snake.inputDir.x != 0)//travelling horizontally
                {
                    if (appleCell.y - snake.curCell.y < 0)//apple is above
                        GoUp();
                    else if (appleCell.y - snake.curCell.y > 0)//apple is below
                        GoDown();
                    
                    LastCorrection();
                }
            }

            bool spreadOut = grid.random.Next(0, 10) >= 8  && snake.segments.Count > 3 && Vector2.Distance(appleCell, snake.curCell) < grid.random.Next(5, 9);//leads to some indirect pathing which can prevent wrapping

            if (grid.IsOverlappingCell(snake.transform, snake.curCell + snake.inputDir))
            {
                AvoidBorder();
            }
            else if (snake.inputDir.y != 0 && !spreadOut || (spreadOut && snake.curCell.y == appleCell.y))//travelling vertically and lined up vertically
            {
                if (appleCell.x - snake.curCell.x > 0)//apple is to the right
                    GoRight();
                else if (appleCell.x - snake.curCell.x < 0)//apple is to the left
                    GoLeft();
                AvoidBorder();
                LastCorrection();
            }
            else if(snake.inputDir.x != 0 && !spreadOut || (spreadOut && snake.curCell.x == appleCell.x))//travelling horizontally and lined up horizontally
            {
                if (appleCell.y - snake.curCell.y < 0)//apple is above
                    GoUp();
                else if (appleCell.y - snake.curCell.y > 0)//apple is below
                    GoDown();
                AvoidBorder();
                LastCorrection();
            }


        }//END OF ACT()
    }
}
