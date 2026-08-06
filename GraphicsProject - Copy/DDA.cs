using System;

namespace GraphicsProject
{
    public class DDA
    {
        public float Xst, Yst;
        public float Xend, Yend;
        float dy, dx, m;
        public float cx, cy;
        int speed = 10;
        public bool travel;

        public void calc()
        {
            dy = Yend - this.Yst;
            dx = Xend - Xst;
            m = dy / dx;
            cx = Xst;
            cy = Yst;
            travel = true;
        }

        public void Rotate(float xRef, float yRef, float angle)
        {
            Xst -= xRef; Yst -= yRef;
            Xend -= xRef; Yend -= yRef;

            float newXst = (float)(Xst * Math.Cos(angle) - Yst * Math.Sin(angle));
            float newYst = (float)(Xst * Math.Sin(angle) + Yst * Math.Cos(angle));

            float newXend = (float)(Xend * Math.Cos(angle) - Yend * Math.Sin(angle));
            float newYend = (float)(Xend * Math.Sin(angle) + Yend * Math.Cos(angle));

            Xst = newXst; Yst = newYst;
            Xend = newXend; Yend = newYend;

            Xst += xRef; Yst += yRef;
            Xend += xRef; Yend += yRef;

            calc(); 
        }

        public void CalcNextPoint()
        {
            if (travel)
            {
                if (Math.Abs(dx) > Math.Abs(dy))
                {
                    if (Xst < Xend)
                    {
                        cx += speed;
                        cy += m * speed;
                        if (cx >= Xend) travel = false;
                    }
                    else
                    {
                        cx -= speed;
                        cy -= m * speed;
                        if (cx <= Xend) travel = false;
                    }
                }
                else
                {
                    if (Yst < Yend)
                    {
                        cy += speed;
                        cx += 1 / m * speed;
                        if (cy >= Yend) travel = false;
                    }
                    else
                    {
                        cy -= speed;
                        cx -= 1 / m * speed;
                        if (cy <= Yend) travel = false;
                    }
                }
            }
        }
    }
}