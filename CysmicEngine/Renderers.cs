using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CysmicEngine
{
    public abstract class Renderer : Component
    {
        public Vector2 offset;
        public Vector2 size;
        public int sortOrder = 0;

        public bool flipX = false;
        public bool flipY = false;
        //protected bool _isGizmo = false;
        //public bool isGizmo { get { return _isGizmo; } }

        public abstract void Draw(Graphics graphics);
    }

    public class Shape2D : Renderer
    {
        /*Vector2 pos;
        Vector2 scale;*/
        //public Vector2 offset;
        public Color color;
        bool isFilled = true;

        public enum ShapeType
        {
            Rectangle, Circle
        }
        public ShapeType shape = ShapeType.Rectangle;

        void _MasterConstructor(Color clr, Vector2? _size, Vector2? _offset, ShapeType shapeType = ShapeType.Rectangle, bool fill = true, int srtOdr = 0)
        {
            if (_size == null)
                _size = (1, 1);
            if (_offset == null)
                _offset = (0, 0);

            sortOrder = srtOdr;
            size = _size.Value;
            color = clr;
            shape = shapeType;
            isFilled = fill;
            offset = _offset.Value;
        }
        /*public Shape2D(Color clr, ShapeType shapeType = ShapeType.Rectangle, bool fill = true, int srtOdr = 0)
        {
            _MasterConstructor(clr, (1, 1), Vector2.Zero, shapeType, fill, srtOdr);
        }*/
        public Shape2D(Color clr, Vector2? size = null, Vector2? offset = null, ShapeType shapeType = ShapeType.Rectangle, bool fill = true, int srtOdr = 0)
        {
            _MasterConstructor(clr, size, offset, shapeType, fill, srtOdr);
        }

        /*protected override void Update()
        {
        }*/

        public override void Draw(Graphics graphics)
        {
            Vector2 pos;
            Vector2 scale;
            scale = transform.scale * size;
            if (flipX)
                scale.x *= -1;
            if (flipY)
                scale.y *= -1;

            pos = transform.position + offset;//(transform.position.x + (scale.x / 2), transform.position.y/* + (scale.y / 2)*/);
            if (scale.x < 0)
                pos.x -= scale.x;
            if (scale.y < 0)
                pos.y -= scale.y;

            SolidBrush brush = new SolidBrush(color);

            switch (shape)
            {
                case ShapeType.Rectangle:
                    //graphics.FillRectangle(Brushes.Blue, pos.x - (size.x / 2), pos.y + (size.y / 2), size.x, size.y);
                    if (isFilled)
                        graphics.FillRectangle(brush, pos.x, pos.y, scale.x, scale.y);
                    else
                        graphics.DrawRectangle(new Pen(brush, 2.5f), pos.x, pos.y, scale.x, scale.y);
                    break;

                case ShapeType.Circle:
                    graphics.FillEllipse(brush, pos.x, pos.y, scale.x, scale.y);
                    break;

                default:
                    break;
            }
        }
    }

    public class SpriteRenderer : Renderer
    {
        string spritePath;
        string fullPath;
        protected Sprite _sprite = null;
        public Sprite sprite { get { return _sprite; } protected set { _sprite = value; } }

        public SpriteRenderer(string path, int srtOdr = 0)
        {
            spritePath = path.Replace("/", @"\");
            fullPath = ($"Assets/Sprites/{spritePath}.png").Replace("/", @"\");
            sprite = new Bitmap(Image.FromFile(fullPath));
            sortOrder = srtOdr;
            size = (1, 1);
            offset = Vector2.zero;
        }
        public SpriteRenderer(string path, Vector2 _size, Vector2 _offset, int srtOdr = 0)
        {
            spritePath = path.Replace("/", @"\");
            fullPath = ($"Assets/Sprites/{spritePath}.png").Replace("/", @"\");
            sprite = new Bitmap(Image.FromFile(fullPath));
            sortOrder = srtOdr;
            size = _size;
            offset = _offset;
        }

        public void SetSprite(Sprite image, string name = "")
        {
            sprite = image;

            var sheet = image as SpriteSheet;

            if (sheet != null && name != "")
                sheet.curName = name;
        }

        public override void Draw(Graphics graphics)
        {
            if (size == Vector2.zero)
            {
                size = (sprite.Width, sprite.Height);
            }

            Vector2 pos;
            Vector2 scale;
            scale = transform.scale * size;
            if (flipX)
                scale.x *= -1;
            if (flipY)
                scale.y *= -1;

            pos = transform.position + offset;
            if (scale.x < 0)
                pos.x -= scale.x;
            if (scale.y < 0)
                pos.y -= scale.y;

            try
            {
                if (sprite.isSpriteSheet)
                {
                    SpriteSheet sheet = sprite as SpriteSheet;
                    sheet.slices.TryGetValue(sheet.curName, out Vector2Int slice);

                    graphics.DrawImage(sprite, new RectangleF(pos.x, pos.y, scale.x, scale.y)

                        , new RectangleF(sheet.curColumn * slice.x, sheet.curRow * slice.y, width: slice.x/* * size.x * transform.scale.x*/, height: slice.y/* * size.y * transform.scale.y*/), GraphicsUnit.Pixel);
                }
                else
                    graphics.DrawImage(sprite, pos.x, pos.y, scale.x, scale.y);
            }
            catch
            {

            }
        }
    }

    public class Sprite
    {
        public bool isSpriteSheet = false;
        //public bool isSpriteSheet { get { return _isSpriteSheet; } protected set { _isSpriteSheet = value; } }
        protected Image image;

        public float Width { get { return image.Width; } }
        public float Height { get { return image.Height; } }

        public Sprite(Image img)
        {
            image = img;
        }
        public Sprite(string path)
        {
            image = new Bitmap(path);
        }

        public static implicit operator Image(Sprite sprite)
        {
            return sprite.image;
        }
        public static implicit operator Sprite(Image bitmap)
        {

            return new Sprite(bitmap);
        }
    }
    public class SpriteSheet : Sprite
    {
        int rows = 0;
        int columns = 0;

        public int curRow = 0;
        public int curColumn = 0;
        public string curName = "";
        public Dictionary<string, Vector2Int> slices = new Dictionary<string, Vector2Int>();

        void _MasterConstructor(Image img, Vector2Int sliceSize, string[] sliceNames)
        {
            columns = img.Width / sliceSize.x;
            rows = img.Height / sliceSize.y;

            int totalSlices = (image.Width / sliceSize.x) * (image.Height / sliceSize.y);

            for (int i = 0; i < totalSlices; i++)
            {
                if (sliceNames == null || i >= sliceNames.Length || sliceNames[i] == "" || slices.ContainsKey(sliceNames[i]))//No blanks or dupes
                    slices.Add(/*"Slice " + */i.ToString(), sliceSize);
                else
                    slices.Add(sliceNames[i], sliceSize);
            }

            isSpriteSheet = true;
        }

        public SpriteSheet(Image img, Vector2Int sliceSize, string[] sliceNames = null) : base(img)
        {
            _MasterConstructor(img, sliceSize, sliceNames);
        }
        public SpriteSheet(string path, Vector2Int sliceSize, string[] sliceNames = null) : base(path)
        {
            _MasterConstructor(new Bitmap(path), sliceSize, sliceNames);
        }

        public SpriteSheet GetSliceByName(string name)
        {
            curName = name;
            isSpriteSheet = true;

            for (int i = 0; i < slices.Count; i++)
            {
                if (slices.ElementAt(i).Key == name)
                    break;

                curColumn++;
                if (curColumn > columns)
                {
                    curColumn = 0;
                    rows++;
                }
            }

            var result = new SpriteSheet(this, slices[name], slices.Keys.ToArray());
            result.curName = curName;
            result.slices = slices;
            result.curColumn = curColumn;
            result.curRow = curRow;

            curColumn = 0;
            curRow = 0;
            return result;
        }

    }
}
