using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace GraphicsProject
{
    class CimgActor
    {
        public int X, Y;
        public List<Bitmap> Limgs = new List<Bitmap>();
        public int IF;
        public int dx;
        public int dy;
    }
    public partial class Form1 : Form
    {

        List<CimgActor> snow = new List<CimgActor>();
        List<CimgActor> raven = new List<CimgActor>();
        List<DDA> Lines = new List<DDA>();
        List<Circle> Circles = new List<Circle>();
        List<int> segmentOrder = new List<int>();
        List<int> segmentIndices = new List<int>();
        List<Curve> Curves = new List<Curve>();

        Bitmap off;
        Bitmap bag1;
        Bitmap bag2;

        Transformation trans = new Transformation();
        Timer tt = new Timer();

        int camx = 0;
        int timer = 0;
        float Xcurrentpos = 100;
        float Ycurrentpos = 690;
        int type = 0; //1 =line , 2=circle , 3=curve
        bool movelines = false;
        float currentAngle = 0f;
        float speed = 0.08f;
        int cntlines = -1;
        int animstate = 0;
        int currentLineIndex = 0;
        float circleAngle = 0;
        bool movesnow = false;
        int jonType = 0;
        int cntcircle = -1;
        int currentSegment = 0;
        bool gameStarted = false;
        int cntcurve = -1;
        float curveT = 0f;
        float curveRotationAngle = 0f;
        bool curveGoingUp = true;
        float downAngle = 0f;

        [DllImport("winmm.dll")]
        static extern int mciSendString(string command, string returnValue, int returnLength, IntPtr callback);

        bool musicOn = false;
        public Form1()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            this.Load += Form1_Load1;
            this.Paint += Form1_Paint;
            this.KeyDown += Form1_KeyDown;
            tt.Tick += Tt_Tick;
            tt.Start();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.P && !gameStarted)
            {
                gameStarted = true;
                return;
            }

            if (!gameStarted) return; 

            if (e.KeyCode == Keys.A)
            {
                if (type == 2)
                {
                    AddLine(Xcurrentpos - 70, Ycurrentpos, Xcurrentpos + 300, Ycurrentpos);
                }
                else
                {
                    type = 1;
                    AddLine(Xcurrentpos, Ycurrentpos, Xcurrentpos + 400, Ycurrentpos);
                }
                cntlines++;
            }

            if (e.KeyCode == Keys.Space)
            {
                if (type == 1)
                {
                    movelines = true;
                    if (animstate == 0)
                        animstate = 1;
                    else if (animstate == 2)
                        animstate = 3;
                }

                if (type == 2)
                {
                    Circles[cntcircle].Rad += 20;
                }

                if (type == 3)
                {
                    Point p = Curves[cntcurve].ControlPoints[1];
                    p.Y -= 20;
                    Curves[cntcurve].ControlPoints[1] = p;

                    Point p2 = Curves[cntcurve].ControlPoints[2];
                    p2.Y -= 20;
                    Curves[cntcurve].ControlPoints[2] = p2;
                }
            }

            if (e.KeyCode == Keys.Z )
            {
                if (type == 1)
                {
                    movelines = true;
                    if (animstate == 2)
                        animstate = 3;
                }
                if (type == 2)
                {
                    Circles[cntcircle].Rad -= 20;

                }
                if (type == 3)
                {
                    Point p = Curves[cntcurve].ControlPoints[1];
                    int minY = Curves[cntcurve].ControlPoints[0].Y - 300; // original height limit
                    if (p.Y < minY - 20) // only go down if not at original position
                    {
                        p.Y += 20;
                        Curves[cntcurve].ControlPoints[1] = p;

                        Point p2 = Curves[cntcurve].ControlPoints[2];
                        p2.Y += 20;
                        Curves[cntcurve].ControlPoints[2] = p2;
                    }
                }
            }

            if (e.KeyCode == Keys.Enter && cntlines >= 0)
            {
                movesnow = true;
                jonType = 1;
                currentSegment = 0;
                circleAngle = 90;
                curveT = 0f;
            }

            if (e.KeyCode == Keys.B)
            {
                cntcircle++;
                type = 2;
                AddCircle(Xcurrentpos + 30, Ycurrentpos - 150, 150);
            }

            if (e.KeyCode == Keys.C)
            {
                AddCurve(Xcurrentpos, Ycurrentpos);
            }

            if (e.KeyCode == Keys.R && type == 3)
            {
                float pivotX = Curves[cntcurve].ControlPoints[0].X;
                float pivotY = Curves[cntcurve].ControlPoints[0].Y;
                float step = 15f;

                if (curveGoingUp && curveRotationAngle >= 90f)
                {
                    curveGoingUp = false;
                }
                if (!curveGoingUp && curveRotationAngle <= 0f)
                {
                    curveGoingUp = true;
                }

                float actualStep = curveGoingUp ? -step : +step;

                float angle = (float)(actualStep * Math.PI / 180);

                for (int i = 1; i < Curves[cntcurve].ControlPoints.Count; i++)
                {
                    Point p = Curves[cntcurve].ControlPoints[i];
                    float dx = p.X - pivotX;
                    float dy = p.Y - pivotY;
                    float newX = (float)(dx * Math.Cos(angle) - dy * Math.Sin(angle)) + pivotX;
                    float newY = (float)(dx * Math.Sin(angle) + dy * Math.Cos(angle)) + pivotY;
                    Curves[cntcurve].ControlPoints[i] = new Point((int)newX, (int)newY);
                }

                curveRotationAngle += curveGoingUp ? step : -step;

                Xcurrentpos = Curves[cntcurve].ControlPoints[3].X;
                Ycurrentpos = Curves[cntcurve].ControlPoints[3].Y;
            }

            if (e.KeyCode == Keys.Down && type == 1)
            {
                float pivotX = Lines[cntlines].Xst;
                float pivotY = Lines[cntlines].Yst;
                float angle = (float)(15 * Math.PI / 180);

                Lines[cntlines].Rotate(pivotX, pivotY, angle);

                Xcurrentpos = Lines[cntlines].Xend;
                Ycurrentpos = Lines[cntlines].Yend;
                //movelines = true;
                //animstate = 4;
            }

            if (e.KeyCode == Keys.M)
            {
                if (e.KeyCode == Keys.M)
                {
                    if (musicOn)
                    {
                        mciSendString("stop song", null, 0, IntPtr.Zero);
                        mciSendString("close song", null, 0, IntPtr.Zero);
                        musicOn = false;
                    }
                    else
                    {
                        mciSendString("open \"song2.mp3\" type mpegvideo alias song", null, 0, IntPtr.Zero);
                        mciSendString("play song repeat", null, 0, IntPtr.Zero);
                        musicOn = true;
                    }
                }
            } //music

            DrawDubb(this.CreateGraphics());
        }

        private void Tt_Tick(object sender, EventArgs e)
        {
            if (movelines)
            {
                moveDDA(cntlines);
            }

            if (movesnow)
            {
                moveJon();
            }

            UpdateCamera();
            DrawDubb(this.CreateGraphics());
        }


        void moveJon()
        {
            if (currentSegment >= segmentOrder.Count) return;

            int seg = segmentOrder[currentSegment];
            int idx = segmentIndices[currentSegment];

            if (seg == 1) // line
            {
                Lines[idx].CalcNextPoint();
                snow[0].X = (int)Lines[idx].cx - 100;
                snow[0].Y = (int)Lines[idx].cy - 170;

                if (!Lines[idx].travel)
                {
                    currentSegment++;
                    if (currentSegment < segmentOrder.Count && segmentOrder[currentSegment] == 2)
                    {
                        circleAngle = 90;
                    }
                }
            }
            else if (seg == 2) // circle
            {
                float rad1 = (float)(circleAngle * Math.PI / 180);
                snow[0].X = (int)(Circles[idx].XC + Circles[idx].Rad * Math.Cos(rad1)) - 100;
                snow[0].Y = (int)(Circles[idx].YC + Circles[idx].Rad * Math.Sin(rad1)) - 170;

                circleAngle -= 5;
                if (circleAngle <= -270)
                {
                    circleAngle = 90;
                    currentSegment++;
                }
            }
            else if (seg == 3) // curve
            {
                PointF pt = Curves[idx].CalcCurvePointAtTime(curveT);
                snow[0].X = (int)pt.X - 100;
                snow[0].Y = (int)pt.Y - 170;

                curveT += 0.01f; // speed
                if (curveT >= 1.0f)
                {
                    curveT = 0f;
                    currentSegment++;
                }
            }
        }

        void moveDDA(int i)
        {
            if (!movelines) return;

            float pivotx = Lines[i].Xst;
            float pivoty = Lines[i].Yst;

            if (animstate == 1)
            {
                Lines[i].Rotate(pivotx, pivoty, -speed);
                currentAngle += speed;
                if (currentAngle >= 0.785f)
                {
                    movelines = false;
                    animstate = 2;
                }
            }
            else if (animstate == 3)
            {
                Lines[i].Rotate(pivotx, pivoty, +speed);
                currentAngle -= speed;
                if (currentAngle <= 0)
                {
                    currentAngle = 0;
                    movelines = false;
                    animstate = 0;
                }
            }

            //if (animstate == 4)
            //{
            //    float pivotX = Lines[cntlines].Xst;
            //    float pivotY = Lines[cntlines].Yst;
            //
            //    Lines[cntlines].Rotate(pivotX, pivotY, speed);

            //    downAngle += speed;

            //    Xcurrentpos = Lines[cntlines].Xend;
            //    Ycurrentpos = Lines[cntlines].Yend;

            //    if (downAngle >= 0.785f)
            //    {
            //        downAngle = 0f;
            //        movelines = false;
            //        animstate = 0;
            //    }
            //}
        }



        void AddCircle(float xc, float yc, int radius)
        {
            Circle newCircle = new Circle();
            newCircle.XC = (int)xc;
            newCircle.YC = (int)yc;
            newCircle.Rad = radius;
            newCircle.st = 0;
            newCircle.end = 360;
            Circles.Add(newCircle);

            segmentOrder.Add(2);
            segmentIndices.Add(Circles.Count - 1);

            Xcurrentpos = xc + radius;
        }

        void AddLine(float xst, float yst, float xend, float yend)
        {
            DDA newLine = new DDA();
            newLine.Xst = xst-20;
            newLine.Yst = yst-15;
            newLine.Xend = xend-20;
            newLine.Yend = yend-15;
            newLine.calc();
            Lines.Add(newLine);

            segmentOrder.Add(1);
            segmentIndices.Add(Lines.Count - 1);

            type = 1; 

            Xcurrentpos = xend;
            Ycurrentpos = yend;
        }

        void AddCurve(float xst, float yst)
        {
            Curve newCurve = new Curve();

            if (type == 2) 
            {
                xst += 30;
            }

            newCurve.SetControlPoint(new Point((int)xst, (int)yst));
            newCurve.SetControlPoint(new Point((int)xst, (int)yst - 300));
            newCurve.SetControlPoint(new Point((int)xst + 300, (int)yst - 300));
            newCurve.SetControlPoint(new Point((int)xst + 300, (int)yst));

            Curves.Add(newCurve);

            segmentOrder.Add(3);
            segmentIndices.Add(Curves.Count - 1);

            type = 3;
            cntcurve++;

            Xcurrentpos = xst + 300;
            Ycurrentpos = yst;
        }

        private void Form1_Load1(object sender, EventArgs e)
        {
            off = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
            bag1 = new Bitmap("wall.png");
            bag2 = new Bitmap("wall2.png");

            createJon();
            createRaven();


        }

        void createJon()
        {
            CimgActor pnn = new CimgActor();
            pnn.Limgs = new List<Bitmap>();
            Bitmap pImg = new Bitmap("jon.png");
            pImg.MakeTransparent(pImg.GetPixel(0, 0));
            pnn.Limgs.Add(pImg);

            pnn.X = 0;
            pnn.Y = this.ClientSize.Height / 2 + 120;

            snow.Add(pnn);
        }

        void createRaven()
        {
            CimgActor pnn = new CimgActor();
            pnn.Limgs = new List<Bitmap>();
            Bitmap pImg = new Bitmap("raven.png");
            pImg.MakeTransparent(pImg.GetPixel(0, 0));
            pnn.Limgs.Add(pImg);

            pnn.X = this.ClientSize.Width-200;
            pnn.Y = 0;

            raven.Add(pnn);
        }

        private void DrawScene(Graphics g)
        {
            if (!gameStarted)
            {
                g.DrawImage(new Bitmap("start.png"), 0, 0, this.ClientSize.Width, this.ClientSize.Height);
                return;
            }
            g.DrawImage(bag1, -camx, 0, this.ClientSize.Width, this.ClientSize.Height);
            g.DrawImage(bag2, this.ClientSize.Width - camx, 0, this.ClientSize.Width, this.ClientSize.Height);
            g.DrawImage(new Bitmap("ned.png"), 10, 40, 150, 200);

            g.DrawString("Winter is Coming..", new Font("Palatino Linotype", 20), Brushes.Black, 10, 10);


            for (int i = 0; i < snow.Count; i++)
            {
                g.DrawImage(snow[i].Limgs[snow[i].IF], snow[i].X - camx, snow[i].Y);
            }

            g.DrawImage(new Bitmap("raven.png"), this.ClientSize.Width - 200, 20, 150, 200);

            Font f = new Font("Palatino Linotype", 10, FontStyle.Bold);
            int tx = this.ClientSize.Width - 200;
            int ty = 40;

            g.DrawString("A = Add Line", f, Brushes.Black, tx+15, ty + 10);
            g.DrawString("B = Add Circle", f, Brushes.Black, tx+15, ty + 30);
            g.DrawString("C = Add Curve", f, Brushes.Black, tx + 15, ty + 50);
            g.DrawString("Enter = Move", f, Brushes.Black, tx + 15, ty + 70);
            g.DrawString("Z = Edit(DEC)", f, Brushes.Black, tx + 15, ty + 90);
            g.DrawString("Space = Edit(INC)", f, Brushes.Black, tx + 15, ty + 110);
            g.DrawString("M = Music", f, Brushes.Black, tx + 15, ty + 130);



            // line
            for (int i = 0; i < Lines.Count; i++)
            {
                int offset = 25;
                float xst = Lines[i].Xst - camx;
                float yst = Lines[i].Yst;
                float xend = Lines[i].Xend - camx;
                float yend = Lines[i].Yend;

                g.DrawLine(new Pen(Color.Black, 10), xst, yst - offset, xend, yend - offset);
                g.DrawLine(new Pen(Color.Black, 4), xst, yst + offset, xend, yend + offset);

                float dx = xend - xst;
                float dy = yend - yst;
                float length = (float)Math.Sqrt(dx * dx + dy * dy);
                int numTies = (int)(length / 15);

                for (int j = 0; j <= numTies; j++)
                {
                    float t = (float)j / numTies;
                    float mx = xst + t * dx;
                    float my = yst + t * dy;
                    g.DrawLine(Pens.Black, mx, my - offset, mx, my + offset);
                }
            }

            // circle
            for (int i = 0; i < Circles.Count; i++)
            {
                int offset = 25;
                int xc = Circles[i].XC - camx;
                int yc = Circles[i].YC;
                int rad = Circles[i].Rad;

                g.DrawEllipse(new Pen(Color.Black, 4), xc - (rad + offset), yc - (rad + offset), (rad + offset) * 2, (rad + offset) * 2);
                g.DrawEllipse(new Pen(Color.Black, 10), xc - (rad - offset), yc - (rad - offset), (rad - offset) * 2, (rad - offset) * 2);

                for (float angle = Circles[i].st; angle <= Circles[i].end; angle += 10)
                {
                    float rad1 = (float)(angle * Math.PI / 180);
                    float innerX = xc + (rad - offset) * (float)Math.Cos(rad1);
                    float innerY = yc + (rad - offset) * (float)Math.Sin(rad1);
                    float outerX = xc + (rad + offset) * (float)Math.Cos(rad1);
                    float outerY = yc + (rad + offset) * (float)Math.Sin(rad1);
                    g.DrawLine(new Pen(Color.Black, 2), innerX, innerY, outerX, outerY);
                }
            }

            //curve
            for (int i = 0; i < Curves.Count; i++)
            {
                int offset = 25;
                for (float t = 0.0f; t <= 1.0f; t += 0.001f)
                {
                    PointF pt = Curves[i].CalcCurvePointAtTime(t);

                    PointF pt2 = Curves[i].CalcCurvePointAtTime(t + 0.001f);

                    float dx = pt2.X - pt.X;
                    float dy = pt2.Y - pt.Y;
                    float len = (float)Math.Sqrt(dx * dx + dy * dy);

                    float px = -dy / len;
                    float py = dx / len;

                    float r1x = pt.X + px * offset - camx;
                    float r1y = pt.Y + py * offset;

                    float r2x = pt.X - px * offset - camx;
                    float r2y = pt.Y - py * offset;

                    g.FillEllipse(new Pen(Color.Black, 4).Brush, r1x, r1y, 4, 4);
                    g.FillEllipse(new Pen(Color.Black, 4).Brush, r2x, r2y, 4, 4);
                }

                for (float t = 0.0f; t <= 1.0f; t += 0.02f)
                {
                    PointF pt = Curves[i].CalcCurvePointAtTime(t);
                    PointF pt2 = Curves[i].CalcCurvePointAtTime(t + 0.001f);

                    float dx = pt2.X - pt.X;
                    float dy = pt2.Y - pt.Y;
                    float len = (float)Math.Sqrt(dx * dx + dy * dy);

                    float px = -dy / len;
                    float py = dx / len;

                    float r1x = pt.X + px * offset - camx;
                    float r1y = pt.Y + py * offset;
                    float r2x = pt.X - px * offset - camx;
                    float r2y = pt.Y - py * offset;

                    g.DrawLine(new Pen(Color.Black, 2), r1x, r1y, r2x, r2y);
                }
            }
        }

        void UpdateCamera()
        {
            int center = this.ClientSize.Width / 2;

            if (snow[0].X > center)
            {
                camx = snow[0].X - center;
            }
            else
            {
                camx = 0;
            }
        }

        void DrawDubb(Graphics g)
        {
            Graphics g2 = Graphics.FromImage(off);
            DrawScene(g2);
            g.DrawImage(off, 0, 0);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            DrawDubb(e.Graphics);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}