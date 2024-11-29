using System.Drawing;

namespace SimpleDrawLib
{
    public class Ellipse : Shape
    {
        public override void Draw(Graphics g)
        {
            int finalX = Math.Abs(X2 - X1);
            int finalY = Math.Abs(Y2 - Y1);
            using (Pen pen = new Pen(LineColor))
            {
                g.DrawEllipse(pen, Math.Min(X1, X2), Math.Min(Y1, Y2), finalX, finalY);
            }
        }
    }
}
