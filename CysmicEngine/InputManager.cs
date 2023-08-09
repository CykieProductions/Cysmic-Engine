using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CysmicEngine
{
    public enum AxisName
    {
        HORIZONTAL, VERTICAL
    }

    /*public static partial class AxisName
    {
        public static readonly Axis HORIZONTAL = new Axis("HORIZONTAL");
        public static readonly Axis VERTICAL = new Axis("VERTICAL");

        public class Axis
        {
            string name;

            public Axis(string varName)
            {
                name = varName.ToUpper();
            }

            public override string ToString()
            {
                return name;
            }
        }
    }*/

    public static class Input
    {
        public static bool pressedOnThisFrame = true;
        public static bool releasedOnThisFrame = true;
        static HashSet<Keys> curPressedKeys = new HashSet<Keys>();
        static HashSet<Keys> justPressedKeys = new HashSet<Keys>();
        static HashSet<Keys> justReleasedKeys = new HashSet<Keys>();

        public static bool clickedMouseOnThisFrame = true;
        public static bool releasedMouseOnThisFrame = true;
        public static bool scrolledMouseOnThisFrame = true;
        static Vector2 mouseWorldPos;
        static Vector2 mouseScreenPos;
        static int mouseScrollDelta = 0;
        static HashSet<MouseButtons> singleClickSet = new HashSet<MouseButtons>();
        static HashSet<MouseButtons> doubleClickSet = new HashSet<MouseButtons>();
        static HashSet<MouseButtons> mouseReleaseSet = new HashSet<MouseButtons>();

        private static bool _showCursor = true;
        public static bool showCursor
        {
            get => _showCursor;
            set
            {
                if (value == _showCursor)
                {
                    return;
                }

                if (value)
                {
                    Cursor.Show();
                }
                else
                {
                    Cursor.Hide();
                }

                _showCursor = value;
            }
        }

        public static Dictionary<string, (Keys, Keys)> axes = new Dictionary<string, (Keys, Keys)>()
        {
            ["Horizontal"] = (Keys.A, Keys.D),//(-1, 1)
            ["horizontal"] = (Keys.Left, Keys.Right),
            ["Vertical"] = (Keys.S, Keys.W),
            ["vertical"] = (Keys.Down, Keys.Up),
        };

        #region Mouse Function
        /*internal static void OffsetMousePosition(float offsetX, float offsetY)
        {
            mousePos += (offsetX, offsetY);
        }*/
        internal static void Window_MouseMove(object sender, MouseEventArgs e)
        {
            //mousePos = (e.X, e.Y);
        }
        internal static void p_SetMousePostionFromWindow(Canvas window)
        {
            var camDeltaPos = Cam.originalPos - Cam.position;
            var camDeltaZoom = 1;//Cam.originalZoom - Cam.zoom;
            mouseScreenPos = (Cursor.Position.X, Cursor.Position.Y);

            var mousePoint = new Point((int)(mouseScreenPos.x + (camDeltaPos.x * camDeltaZoom)), (int)(mouseScreenPos.y + (camDeltaPos.y * camDeltaZoom)));
            mouseWorldPos = (Vector2)window.PointToClient(mousePoint);
        }
        internal static void Window_MouseWheel(object sender, MouseEventArgs e)
        {
            mouseScrollDelta = e.Delta;
            scrolledMouseOnThisFrame = false;
        }
        internal static void Window_MouseClick(object sender, MouseEventArgs e)
        {
            singleClickSet.Add(e.Button);
            clickedMouseOnThisFrame = false;
        }
        internal static void Window_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            doubleClickSet.Add(e.Button);
            clickedMouseOnThisFrame = false;
        }
        internal static void Window_MouseUp(object sender, MouseEventArgs e)
        {
            mouseReleaseSet.Add(e.Button);
            releasedMouseOnThisFrame = false;
        }

        public static Vector2 GetMousePosition(bool UseWorldSpace)
        {
            if (UseWorldSpace)
                return mouseWorldPos;
            else
                return mouseScreenPos;
        }
        public static int GetMouseScroll()
        {
            scrolledMouseOnThisFrame = true;
            return mouseScrollDelta;
        }
        public static bool ClickedMouse(MouseButtons mouseButton)
        {
            clickedMouseOnThisFrame = true;
            return singleClickSet.Contains(mouseButton);
        }
        public static bool DoubleClickedMouse(MouseButtons mouseButton)
        {
            clickedMouseOnThisFrame = true;
            return doubleClickSet.Contains(mouseButton);
        }
        public static bool ReleasedMouseButton(MouseButtons mouseButton)
        {
            releasedMouseOnThisFrame = true;
            return doubleClickSet.Contains(mouseButton);
        }

        internal static void ResetMouseScroll()
        {
            mouseScrollDelta = 0;
        }
        internal static void ClearMouseClicks()
        {
            singleClickSet.Clear();
            doubleClickSet.Clear();
        }
        internal static void ClearMouseReleases()
        {
            mouseReleaseSet.Clear();
        }
        #endregion

        #region Keyboard Functions
        internal static void Win_KD_Event(object sender, KeyEventArgs e)
        {
            /*if(e.KeyCode == Keys.Space)
                print("Jump Key Down Event!");*/
            if (!curPressedKeys.Contains(e.KeyCode))//ensure it fires only once per press
                justPressedKeys.Add(e.KeyCode);
            curPressedKeys.Add(e.KeyCode);
            pressedOnThisFrame = false;
            //releasedOnThisFrame = false;
        }

        internal static void Win_KU_Event(object sender, KeyEventArgs e)
        {
            /*if (e.KeyCode == Keys.Space)
                print("Jump Key Up Event!");*/

            curPressedKeys.Remove(e.KeyCode);
            justReleasedKeys.Add(e.KeyCode);
            //pressedOnThisFrame = false;
            releasedOnThisFrame = false;
        }
        /// <summary>Clear the list of one time presses at the end of every frame</summary>
        internal static void ClearPressedKeys()
        {
            /*if (justPressedKeys.Contains(Keys.Space))
                print("CLEAR PRESSED");*/

            justPressedKeys.Clear();
            //justReleasedKeys.Clear();
        }
        /// <summary>Clear the list of one time presses at the end of every frame</summary>
        internal static void ClearReleasedKeys()
        {
            /*if (justReleasedKeys.Contains(Keys.Space))
                print("CLEAR RELEASED");*/

            //justPressedKeys.Clear();
            justReleasedKeys.Clear();
        }

        public static bool PressedKey(Keys key)
        {
            /*if (key == Keys.Space && justPressedKeys.Contains(key))
                print("Pressed the Jump Key!");*/
            pressedOnThisFrame = true;
            return justPressedKeys.Contains(key);
        }
        /// <summary>Is this key being held down?</summary>
        /// <param name="includeFirstFrame">If false, this won't return true on the first frame of the press</param>
        public static bool HoldingKey(Keys key, bool includeFirstFrame = true)
        {
            if (includeFirstFrame)
                return curPressedKeys.Contains(key);
            else
                return !justPressedKeys.Contains(key) && curPressedKeys.Contains(key);
        }
        public static bool ReleasedKey(Keys key)//Note: it's possible to trigger this without triggering KeyDown first; maybe not anymore?
        {
            /*if (key == Keys.Space && justReleasedKeys.Contains(key))
                print("Released the Jump Key!");*/
            releasedOnThisFrame = true;
            return justReleasedKeys.Contains(key);
        }

        public static int GetAxis(AxisName input)
        {
            List<string> altNames = new List<string>();
            for (int i = 0; i < axes.Keys.Count; i++)
            {
                if (input.ToString().ToUpper() == axes.Keys.ElementAt(i).ToUpper())//If the names (forced to uppercase) match
                {
                    altNames.Add(axes.Keys.ElementAt(i));//add the unalter name as an alt name for the current input
                }
            }
            if (altNames.Count == 0)//name didn't match with any alternate capitalizing
                throw new Exception("The provided Axis name wasn't valid");

            for (int i = 0; i < altNames.Count; i++)
            {
                if (!axes.TryGetValue(altNames[i], out (Keys, Keys) axis))//Make sure this alt name is the actual one in the dictionary
                    continue;

                if (curPressedKeys.Contains(axis.Item1))
                    return -1;
                else if (curPressedKeys.Contains(axis.Item2))
                    return 1;
            }
            return 0;//name was valid, but nothing was pressed
        }

        public static int GetAxis(string input)
        {
            List<string> altNames = new List<string>();
            for (int i = 0; i < axes.Keys.Count; i++)
            {
                if (input.ToString().ToUpper() == axes.Keys.ElementAt(i).ToUpper())//If the names (forced to uppercase) match
                {
                    altNames.Add(axes.Keys.ElementAt(i));//add the unalter name as an alt name for the current input
                }
            }
            if (altNames.Count == 0)//name didn't match with any alternate capitalizing
                throw new Exception("The provided Axis name wasn't valid");

            for (int i = 0; i < altNames.Count; i++)
            {
                if (!axes.TryGetValue(altNames[i], out (Keys, Keys) axis))//Make sure this alt name is the actual one in the dictionary
                    continue;

                if (curPressedKeys.Contains(axis.Item1))
                    return -1;
                else if (curPressedKeys.Contains(axis.Item2))
                    return 1;
            }
            return 0;//name was valid, but nothing was pressed
        }
        #endregion
    }
}
