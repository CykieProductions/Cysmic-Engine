using System;
using System.Drawing;

namespace CysmicEngine
{
    public struct Vector2Int
    {
        public int x, y;

        public Vector2Int(int xv = 0, int yv = 0)
        {
            x = xv;
            y = yv;
        }

        public static implicit operator string(Vector2Int vector2)
        {
            return "(" + vector2.x + "," + vector2.y + ")";
        }

        public static implicit operator Vector2Int(Vector2 vector2)
        {
            Vector2Int result = new Vector2Int();
            result.x = (int)vector2.x;
            result.y = (int)vector2.y;

            return result;
        }
        public static implicit operator Vector2Int((int, int) tuple)
        {
            Vector2Int result = new Vector2Int();
            result.x = tuple.Item1;
            result.y = tuple.Item2;

            return result;
        }
        public static explicit operator Vector2Int(Point point)
        {
            Vector2Int result = new Vector2Int();
            result.x = point.X;
            result.y = point.Y;

            return result;
        }

        public static Vector2Int operator +(Vector2Int v1, Vector2Int v2)
        {
            return new Vector2(v1.x + v2.x, v1.y + v2.y);
        }
        public static Vector2Int operator -(Vector2Int v1, Vector2Int v2)
        {
            return new Vector2(v1.x - v2.x, v1.y - v2.y);
        }
    }
    public struct Vector2
    {
        public float x, y;

        public Vector2(float xv = 0, float yv = 0)
        {
            x = xv;
            y = yv;
        }
        public static float Distance(Vector2 pnt1, Vector2 pnt2)
        {
            return (float)Math.Sqrt(Math.Pow(pnt2.x - pnt1.x, 2) + Math.Pow(pnt2.y - pnt1.y, 2));
        }

        public static Vector2 Lerp(Vector2 start, Vector2 end, float percentage)
        {
            return (start.x + percentage * (end.x - start.x), start.y + percentage * (end.y - start.y));
        }

        /// <summary>
        /// A Vector2 of (0, 0)
        /// </summary>
        public static readonly Vector2 zero = (0, 0);


        // Casting and Operators //

        public static implicit operator Vector2((float, float) tuple)
        {
            Vector2 result = new Vector2();
            result.x = tuple.Item1;
            result.y = tuple.Item2;

            return result;
        }
        public static implicit operator Vector2(Vector2Int vector2int)
        {
            Vector2 result = new Vector2();
            result.x = vector2int.x;
            result.y = vector2int.y;

            return result;
        }
        /*public static explicit operator Vector2(Vector2Int vector2int)
        {
            Vector2Int result = new Vector2Int();
            result.x = vector2int.x;
            result.y = vector2int.y;

            return result;
        }*/


        public static explicit operator Vector2(Point point)
        {
            Vector2 result = new Vector2();
            result.x = point.X;
            result.y = point.Y;

            return result;
        }

        public static implicit operator string(Vector2 vector2)
        {
            return "(" + vector2.x + "," + vector2.y + ")";
        }

        public static Vector2 operator +(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.x + v2.x, v1.y + v2.y);
        }
        public static Vector2 operator -(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.x - v2.x, v1.y - v2.y);
        }

        public static Vector2 operator -(Vector2 v1)
        {
            return new Vector2(-v1.x, -v1.y);
        }

        public static Vector2 operator /(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.x / v2.x, v1.y / v2.y);
        }
        public static Vector2 operator *(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.x * v2.x, v1.y * v2.y);
        }
        public static Vector2 operator *(Vector2 v1, float m)
        {
            return new Vector2(v1.x * m, v1.y * m);
        }
        public static Vector2 operator /(Vector2 v1, float d)
        {
            return new Vector2(v1.x / d, v1.y / d);
        }
        /*public static bool operator ==(Vector2 v1, Vector2 v2)//does a string comparision automatically
        {
            bool result = false;

            if (v1.x == v2.x && v1.y == v2.y)
                result = true;

            return result;
        }
        public static bool operator !=(Vector2 v1, Vector2 v2)
        {
            bool result = true;

            if (v1.x == v2.x && v1.y == v2.y)
                result = false;

            return result;
        }*/

        public override string ToString()
        {
            return "(" + x + "," + y + ")";
        }
    }
}
