using System.Drawing;

namespace SimpleDrawLib
{
    public class Line : Shape
    {
        public override void Draw(Graphics g)
        {
            using (Pen pen = new Pen(LineColor))
            {
                g.DrawLine(pen, X1, Y1, X2, Y2);
            }
        }
    }
}
