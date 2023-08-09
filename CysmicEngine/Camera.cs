namespace CysmicEngine
{
    public static class Cam
    {
        static bool hasAlteredPosition = false;
        static bool hasAlteredZoom = false;

        //private static Vector2 _lastFramePos = Vector2.Zero;
        static Vector2 _originalPos = Vector2.zero;
        public static Vector2 originalPos { get => _originalPos; private set => _originalPos = value; }

        private static Vector2 _position = Vector2.zero;
        public static Vector2 position
        {
            get => _position; set
            {
                //lastFramePos = _position;
                if (!hasAlteredPosition)
                {
                    originalPos = value;
                    hasAlteredPosition = true;
                }
                _position = value;
                //Basic.print(lastFramePos + " | " + value);
            }
        }

        private static float _originalZoom = 1;
        public static float originalZoom { get => _originalZoom; private set => _originalZoom = value; }
        static float _zoom = 1;
        public static float zoom
        {
            get => _zoom; set
            {
                //lastFramePos = _position;
                if (!hasAlteredZoom)
                {
                    originalZoom = _zoom;
                    hasAlteredZoom = true;
                }
                _zoom = value;
                //Basic.print(lastFramePos + " | " + value);
            }
        }

        static Vector2 _camCenter = Vector2.zero;
        static public Vector2 camCenter { get { return _camCenter; } }

        public static float speed = 120;

        /*static RectangleF _VisibleClipBounds = new RectangleF();
public static RectangleF VisibleClipBounds { get { return _VisibleClipBounds; } }*/

        internal static void p_SetCenter(Canvas window)
        {
            //_camCenter = (position.x + graphics.DpiX / 2, position.y + graphics.DpiY / 2);
            _camCenter = (position.x + window.Width / 2, position.y + window.Height / 2);
        }

        public static void Follow(Vector2 targetPos, float smoothing = 0.12f)
        {
            position = Vector2.Lerp(position, (-targetPos * zoom) + new Vector2(CysmicGame.game.window.Width / 2, CysmicGame.game.window.Height / 2), smoothing);
        }
    }
}
