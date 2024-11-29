using System.Drawing;

namespace SimpleDrawLib
{
    [Serializable]
    public abstract class Shape
    {
        public enum ShapeType { None, Line, Rectangle, Ellipse }
        public ShapeType Type { get; set; }
        public int Id { get; set; }

        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int X2 { get; set; }
        public int Y2 { get; set; }

        public Color LineColor { get; set; } = Color.Black;

        public abstract void Draw(Graphics g);

        public bool IsHit(int x, int y)
        {
            return x >= Math.Min(X1, X2) && x <= Math.Max(X1, X2) && y >= Math.Min(Y1, Y2) && y <= Math.Max(Y1, Y2);
        }
    }
}
