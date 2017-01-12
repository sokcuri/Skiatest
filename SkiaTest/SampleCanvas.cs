using SkiaSharp.Views.Desktop;
using System.Windows.Forms;
using SkiaSharp;
using System;
using System.Threading;
using System.Collections.Generic;

namespace SkiaTest
{
    class FPSCounter
    {
        int fpsStep = 0;
        int lastFps = 0;
        DateTime lastDate = DateTime.Now;

        public int FPS
        {
            get
            {
                return lastFps;
            }
        }

        public void Update()
        {
            fpsStep++;

            var delta = DateTime.Now - lastDate;

            if (delta.TotalSeconds >= 1)
            {
                lastDate = DateTime.Now;

                lastFps = fpsStep;
                fpsStep = 0;
            }
        }
    }

    public class KCanvas : SKGLControl
    {
        class Ball
        {
            public SKPaint Paint { get; set; }

            public float X { get; set; }
            public float Y { get; set; }

            public float Radius { get; set; }

            public float SpeedX { get; set; } = 1f;
            public float SpeedY { get; set; } = 1f;

            public int DirectionX = 1;
            public int DirectionY = 1;

            public float Opacity { get; set; } = 1;
        }

        FPSCounter counter;

        float scale = 1;
        float angle = 0;
        float render = 0;

        Random r = new Random();
        List<Ball> balls = new List<Ball>();
        Queue<Ball> lazyAddBalls = new Queue<Ball>();
        Queue<Ball> lazyRemoveBalls = new Queue<Ball>();

        public KCanvas()
        {
            counter = new FPSCounter();

            var renderer = new Thread(() =>
            {
                while (true)
                {
                    this.Invalidate();
                }
            });

            renderer.IsBackground = true;
            renderer.Start();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);


        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            for (int i = 0; i < 1024; i++)
                lazyAddBalls.Enqueue(new Ball()
                {
                    X = e.X,
                    Y = e.Y,
                    SpeedX = r.Next(10) + 1,
                    SpeedY = r.Next(10) + 1,
                    Radius = r.Next(2, 10),
                    DirectionX = new[] { -1, 1 }[r.Next(2)],
                    DirectionY = new[] { -1, 1 }[r.Next(2)],
                    Paint = new SKPaint()
                    {
                        Color = System.Drawing.Color.FromArgb(
                            r.Next(256),
                            r.Next(256),
                            r.Next(256)
                            ).ToSKColor(),
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = 2,
                        IsAntialias = true
                    }
                });
        }

        protected override void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);

            var c = e.Surface.Canvas;

            c.Clear(SKColors.White);

            while (lazyAddBalls.Count > 0)
                balls.Add(lazyAddBalls.Dequeue());

            while (lazyRemoveBalls.Count > 0)
                balls.Remove(lazyRemoveBalls.Dequeue());

            foreach (Ball ball in balls)
            {
                ball.X += ball.SpeedX * ball.DirectionX;
                ball.Y += ball.SpeedY * ball.DirectionY;

                if (ball.X < ball.Radius || ball.X > Width - ball.Radius)
                    ball.DirectionX *= -1;

                if (ball.Y < ball.Radius || ball.Y > Height - ball.Radius)
                    ball.DirectionY *= -1;

                ball.X = Math.Min(Math.Max(ball.X, ball.Radius), Width - ball.Radius);
                ball.Y = Math.Min(Math.Max(ball.Y, ball.Radius), Height - ball.Radius);

                /*ball.Opacity -= 0.05f;
                ball.Opacity = Math.Max(ball.Opacity, 0);
                ball.Paint.Color = ball.Paint.Color.WithAlpha((byte)(255 * ball.Opacity));*/

                if (ball.Opacity < float.Epsilon)
                    lazyRemoveBalls.Enqueue(ball);

                c.DrawOval(ball.X, ball.Y, ball.Radius, ball.Radius, ball.Paint);
            }
            
            counter.Update();

            using (var paint = new SKPaint()
            {
                Color = SKColors.Red,
                IsAntialias = true,
                TextAlign = SKTextAlign.Right,
                TextSize = 20
            })
            {
                c.DrawText($"FPS: {counter.FPS}",
                    (int)(Width - 10),
                    (int)(Height - 10),
                    paint);

                c.DrawText($"Object: {balls.Count}",
                    (int)(Width - 10),
                    (int)(Height - 10 - paint.TextSize * 1.5),
                    paint);
            }
        }
    }
}