using System;
using Microsoft.Xna.Framework;

namespace QuakeConsole
{
    // Modified conversion to Rectangle from explicit to implicit.
    // Ref: https://github.com/SiliconStudio/paradox/blob/master/sources/common/core/SiliconStudio.Core.Mathematics/RectangleF.cs
    internal struct RectangleF
    {
        public static readonly RectangleF Empty = new RectangleF();

        public RectangleF(float x, float y, float width, float height)
        {
            Left = x;
            Top = y;
            Width = width;
            Height = height;
        }

        public float Left { get; set; }
        public float Top { get; set; }
        public float Right => Left + Width;
        public float Bottom => Top + Height;

        public float X
        {
            get { return Left; }
            set { Left = value; }
        }

        public float Y
        {
            get { return Top; }
            set { Top = value; }
        }

        public float Width { get; set; }
        public float Height { get; set; }

        public Vector2 Location
        {
            get { return new Vector2(X, Y); }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public Vector2 Center => new Vector2(X + (Width / 2), Y + (Height / 2));

        public bool IsEmpty => (Width == 0.0f) && (Height == 0.0f) && (X == 0.0f) && (Y == 0.0f);

        public Vector2 TopLeft => new Vector2(Left, Top);
        public Vector2 TopRight => new Vector2(Right, Top);
        public Vector2 BottomLeft => new Vector2(Left, Bottom);
        public Vector2 BottomRight => new Vector2(Right, Bottom);

        public void Offset(Point amount) => Offset(amount.X, amount.Y);
        public void Offset(Vector2 amount) => Offset(amount.X, amount.Y);
        public void Offset(float offsetX, float offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }

        public void Inflate(float horizontalAmount, float verticalAmount)
        {
            X -= horizontalAmount;
            Y -= verticalAmount;
            Width += horizontalAmount * 2;
            Height += verticalAmount * 2;
        }

        public void Contains(ref Vector2 value, out bool result) =>       
            result = (X <= value.X) && (value.X < Right) && (Y <= value.Y) && (value.Y < Bottom);        
        public bool Contains(Rectangle value) => 
            (X <= value.X) && (value.Right <= Right) && (Y <= value.Y) && (value.Bottom <= Bottom);
        public void Contains(ref RectangleF value, out bool result) => 
            result = (X <= value.X) && (value.Right <= Right) && (Y <= value.Y) && (value.Bottom <= Bottom);        

        public bool Contains(float x, float y) => (x >= Left && x <= Right && y >= Top && y <= Bottom);        
        public bool Contains(Vector2 vector2D) => Contains(vector2D.X, vector2D.Y);        
        public bool Contains(Point point) => Contains(point.X, point.Y);

        public bool Intersects(RectangleF value)
        {
            bool result;
            Intersects(ref value, out result);
            return result;
        }

        public void Intersects(ref RectangleF value, out bool result) => 
            result = (value.X < Right) && (X < value.Right) && (value.Y < Bottom) && (Y < value.Bottom);

        public static RectangleF Intersect(RectangleF value1, RectangleF value2)
        {
            RectangleF result;
            Intersect(ref value1, ref value2, out result);
            return result;
        }

        public static void Intersect(ref RectangleF value1, ref RectangleF value2, out RectangleF result)
        {
            float newLeft = (value1.X > value2.X) ? value1.X : value2.X;
            float newTop = (value1.Y > value2.Y) ? value1.Y : value2.Y;
            float newRight = (value1.Right < value2.Right) ? value1.Right : value2.Right;
            float newBottom = (value1.Bottom < value2.Bottom) ? value1.Bottom : value2.Bottom;
            result = (newRight > newLeft) && (newBottom > newTop)
                ? new RectangleF(newLeft, newTop, newRight - newLeft, newBottom - newTop)
                : Empty;
        }

        public static RectangleF Union(RectangleF value1, RectangleF value2)
        {
            RectangleF result;
            Union(ref value1, ref value2, out result);
            return result;
        }

        public static void Union(ref RectangleF value1, ref RectangleF value2, out RectangleF result)
        {
            float left = Math.Min(value1.Left, value2.Left);
            float right = Math.Max(value1.Right, value2.Right);
            float top = Math.Min(value1.Top, value2.Top);
            float bottom = Math.Max(value1.Bottom, value2.Bottom);
            result = new RectangleF(left, top, right - left, bottom - top);
        }

        public override bool Equals(object obj)
        {
            while (true)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (obj.GetType() != typeof (RectangleF)) return false;
                obj = (RectangleF) obj;
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = Left.GetHashCode();
                result = (result * 397) ^ Top.GetHashCode();
                result = (result * 397) ^ Width.GetHashCode();
                result = (result * 397) ^ Height.GetHashCode();
                return result;
            }
        }

        public static bool operator ==(RectangleF left, RectangleF right) => left.Equals(right);
        public static bool operator !=(RectangleF left, RectangleF right) => !(left == right);

        public static implicit operator Rectangle(RectangleF value) => 
            new Rectangle((int) value.X, (int) value.Y, (int) value.Width, (int) value.Height);

        public override string ToString() => $"{nameof(X)}:{X} {nameof(Y)}:{Y} {nameof(Width)}:{2} {nameof(Height)}:{3}";
    }
}