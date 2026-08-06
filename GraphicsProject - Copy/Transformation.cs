using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsProject
{
    public class Transformation
    {
        public class LineSegment
        {
            public PointF ptS, ptE;

            public void DrawYourSelf(Graphics g)
            {
                g.DrawLine(Pens.Black, ptS.X, ptS.Y, ptE.X, ptE.Y);
                g.FillEllipse(Brushes.Red, ptS.X - 5, ptS.Y - 5, 10, 10);
                g.FillEllipse(Brushes.Red, ptE.X - 5, ptE.Y - 5, 10, 10);
            }
        }

        public LineSegment Rotate(LineSegment L, float xRef, float yRef, float angle)
        {
            ///////////////////
            //// translate
            //////////////////
            L.ptS.X -= xRef;
            L.ptS.Y -= yRef;
            L.ptE.X -= xRef;
            L.ptE.Y -= yRef;

            ///////////////////
            //// Rotate around origin
            //////////////////
            double xn = L.ptS.X * Math.Cos(angle) - L.ptS.Y * Math.Sin(angle);
            double Yn = L.ptS.X * Math.Sin(angle) + L.ptS.Y * Math.Cos(angle);

            L.ptS.X = (float)xn;
            L.ptS.Y = (float)Yn;

            xn = L.ptE.X * Math.Cos(angle) - L.ptE.Y * Math.Sin(angle);
            Yn = L.ptE.X * Math.Sin(angle) + L.ptE.Y * Math.Cos(angle);

            L.ptE.X = (float)xn;
            L.ptE.Y = (float)Yn;

            ///////////////////
            //// undo the translation
            //////////////////
            L.ptS.X += xRef;
            L.ptS.Y += yRef;
            L.ptE.X += xRef;
            L.ptE.Y += yRef;

            return L;
        }
    }
}
