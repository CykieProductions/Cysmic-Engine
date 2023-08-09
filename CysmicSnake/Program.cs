using CysmicEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using CysmicPong;
using CyTools;
using System.Threading;
using System.Drawing;

namespace CysmicSnake
{
    public class Program : Basic
    {
        static void Main(string[] args)
        {
            var snakeGame = new SnakeGame((800, 600), "Snake", bColor: Color.BurlyWood);
            var pongGame = new PongGame((800, 600), "Pong", InterpolationMode.High);


            bool isRunning = true;
            //while (isRunning)
            {
                print("Which game?");
                print("1. Snake | 2. Pong | Q. Quit");

                var input = Console.ReadLine();

                switch (input.ToUpper())
                {
                    case "Q":
                        return;
                    case "1":
                        snakeGame.Play();
                        break;
                    default:
                        Input.axes.Remove("vertical");
                        try
                        {
                            Input.axes.Add("VerticalArrows", (Keys.Down, Keys.Up));
                        }
                        catch (ArgumentException) { }


                        pongGame.Play();
                        break;
                }
                //Thread.Sleep(1000);
            }


            /*while (CysmicGame.game.window.Controls != null)
            {
                _ = ";";
            }*/
        }

        /*public static void AddControl(Control control)
        {
            var window = CysmicGame.game.window;
            if (window.InvokeRequired)
                window.Invoke(AddControl(control));
            else
                window.Controls.Add(control);
        }*/

    }
}
