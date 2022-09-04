using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace QGoLF
{
    public partial class MainPage : ContentPage
    {
        readonly SKPaint blackLines = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = 5,
            StrokeCap = SKStrokeCap.Square
        };
        readonly SKPaint blueFillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Blue.WithAlpha(0x80)
        };
        readonly SKPaint redDots = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Red,
            StrokeWidth = 6,
            StrokeCap = SKStrokeCap.Round
        };
        readonly SKPaint grayFillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Gray.WithAlpha(0x80)
        };
        //readonly SKPaint textPaint = new SKPaint
        //{
        //    TextSize = 32.0f,
        //    IsAntialias = true,
        //    Color = SKColors.White,
        //    TextAlign = SKTextAlign.Center
        //};
        //This text style can be used with canvas.DrawText(⋯)
        
        readonly Random rndNum = new Random();
        List<int[]> Ccells = new List<int[]>(); // Covered cells in this call
        List<int[]> Fcells = new List<int[]>(); // Free cells for the next call
        List<int[]> Scells = new List<int[]>(); // Covered cells with no free cells around
        int iters = 0;
        bool autoStop = false;
        float scale = 1;
        int[] axLim = new int[2];
        int[] ayLim = new int[2];
        //private static readonly BindableProperty HnessProperty = BindableProperty.Create("Hness", typeof(float), typeof(MainPage), null);
        float Hness
        {
            get { return (float)Fcells.Count/Ccells.Count; }
        }

        public MainPage()
        {
            InitializeComponent();
            canvasView.InvalidateSurface();
        }

        private void InitFunc(SKPoint eLoc)
        {
            List<int> ind4Scells = new List<int>();
            int px = (int)Math.Floor(eLoc.X / 50.0) * 50;
            int py = (int)Math.Floor(eLoc.Y / 50.0) * 50;
            int ind = Ccells.FindIndex(item => item[0] == px && item[1] == py);
            if (ind >= 0)
            {
                Ccells.RemoveAt(ind);
                //Take care of left cell
                if (Ccells.Any(p => p[0]==px - 50 && p[1]==py + 0 )) //p.SequenceEqual(new int[] { px - 50, py + 0 })
                {
                    Fcells.Add(new int[] { px + 25, py + 25, iters });
                }
                ind = Fcells.FindIndex(item => item[0] == px - 25 && item[1] == py + 25);
                if (ind >= 0) Fcells.RemoveAt(ind);
                //Take care of bottom cell
                if (Ccells.Any(p => p[0]== px - 0 && p[1]==py + 50 ))
                {
                    Fcells.Add(new int[] { px + 25, py + 25, iters });
                }
                ind = Fcells.FindIndex(item => item[0] == px + 25 && item[1] == py + 75);
                if (ind >= 0) Fcells.RemoveAt(ind);
                //Take care of left cell
                if (Ccells.Any(p => p[0]== px + 50 && p[1]== py - 0 ))
                {
                    Fcells.Add(new int[] { px + 25, py + 25, iters });
                }
                ind = Fcells.FindIndex(item => item[0] == px + 75 && item[1] == py + 25);
                if (ind >= 0) Fcells.RemoveAt(ind);
                //Take care of top cell
                if (Ccells.Any(p => p[0]==px + 0 && p[1]==py - 50 ))
                {
                    Fcells.Add(new int[] { px + 25, py + 25, iters });
                }
                ind = Fcells.FindIndex(item => item[0] == px + 25 && item[1] == py - 25);
                if (ind >= 0) Fcells.RemoveAt(ind);
            }
            else
            {
                if (iters!=0 && Ccells[Ccells.Count-1][2] == iters) //Due to fast processing, multiple calls can be made at same 'iters'?!?!
                {
                    return;
                }
                Ccells.Add(new int[] { px, py, iters });
                foreach(int[] m in Fcells.FindAll(item => item[0] == px + 25 && item[1] == py + 25))
                {
                    if (m[2] != 0)
                    {
                        ind4Scells.Add(m[2]);
                    }
                }
                Fcells.RemoveAll(item => item[0] == px + 25 && item[1] == py + 25);
                Fcells.Add(new int[] { px - 25, py + 25, iters });
                Fcells.Add(new int[] { px + 25, py + 75, iters });
                Fcells.Add(new int[] { px + 75, py + 25, iters });
                Fcells.Add(new int[] { px + 25, py - 25, iters });
                List<int> mtd = new List<int>();
                for (int m = 4; m > 0; m--)
                {
                    int[] Fxy = { Fcells[Fcells.Count - m][0], Fcells[Fcells.Count - m][1] };
                    if (Ccells.Any(item => item[0] == Fxy[0] - 25 && item[1] == Fxy[1] - 25))
                    {
                        mtd.Add(m);
                    }
                }
                foreach (int m in mtd)
                {
                    int[] Fxy = { Fcells[Fcells.Count - m][0], Fcells[Fcells.Count - m][1] };
                    foreach (int[] mm in Fcells.FindAll(item => item[0] == Fxy[0] && item[1] == Fxy[1]))
                    {
                        if (mm[2] != 0)
                        {
                            ind4Scells.Add(mm[2]);
                        }
                    }
                    Fcells.RemoveAll(item => item[0] == Fxy[0] && item[1] == Fxy[1]);
                }
                foreach (int m in ind4Scells.Distinct())
                {
                    if (!Fcells.Any(item => item[2] == m))
                    {
                        Scells.AddRange(Ccells.FindAll(item => item[2] == m));
                        Ccells.RemoveAll(item => item[2] == m);
                    }
                }
            }
        }

        private void CanvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;
            canvas.Clear(SKColors.White);
            //canvas.Save();
            //canvas.Restore();

            int px=0;
            int py=0;
            int ind;
            
            if (iters == 0)
            {
                axLim[0] = 0;
                axLim[1] = (int)Math.Floor(canvas.LocalClipBounds.Right / 50.0) * 50;
                ayLim[0] = 0;
                ayLim[1] = (int)Math.Floor(canvas.LocalClipBounds.Bottom / 50.0) * 50;
                if (!initButton.Text.Equals("Done •") || Ccells.Count==0)
                {
                    if (Math.IEEERemainder((axLim[1] - axLim[0]) / 2, 50) == 0)
                    {
                        px = (axLim[1] - axLim[0]) / 2;
                    }
                    else
                    {
                        px = 25 + ((axLim[1] - axLim[0]) / 2); 
                    }
                    if (Math.IEEERemainder((ayLim[1] - ayLim[0]) / 2, 50) == 0)
                    {
                        py = (ayLim[1] - ayLim[0]) / 2;
                    }
                    else
                    {
                        py = 25 + ((ayLim[1] - ayLim[0]) / 2); 
                    }
                    Ccells.Clear();
                    Scells.Clear();
                    Ccells.Add(new int[] { px, py, iters });
                    Fcells.Clear();
                    Fcells.Add(new int[] { px - 25, py + 25, iters });
                    Fcells.Add(new int[] { px + 25, py + 75, iters });
                    Fcells.Add(new int[] { px + 75, py + 25, iters });
                    Fcells.Add(new int[] { px + 25, py - 25, iters });
                }
                scale = Math.Min((float)(canvas.LocalClipBounds.Width / axLim[1]), (float)(canvas.LocalClipBounds.Height / ayLim[1]));
                canvas.Scale(scale, scale, canvas.LocalClipBounds.MidX, canvas.LocalClipBounds.MidY);
                canvas.Translate(canvas.LocalClipBounds.Left - axLim[0], canvas.LocalClipBounds.Top - ayLim[0]);
            }
            else
            {
                canvas.Scale(scale, scale, canvas.LocalClipBounds.MidX, canvas.LocalClipBounds.MidY);
                canvas.Translate(canvas.LocalClipBounds.Left - axLim[0], canvas.LocalClipBounds.Top - ayLim[0]);
                ind = rndNum.Next(0, Fcells.Count);
                px = Fcells[ind][0] - 25;
                py = Fcells[ind][1] - 25;
                InitFunc(new SKPoint(px, py));
            }
            
            if (iters!=0 && (Fcells.Exists(item => item[0] < axLim[0] || item[0] > axLim[1])))
            {
                axLim[0] -= 250;
                axLim[1] += 250;
                float LocScale = (canvas.LocalClipBounds.Width / (axLim[1] - axLim[0]));
                canvas.Clear(SKColors.White);
                canvas.Scale(LocScale, LocScale, canvas.LocalClipBounds.MidX, canvas.LocalClipBounds.MidY);
                ayLim[0] = (int)Math.Floor(canvas.LocalClipBounds.Top / 50.0) * 50;
                ayLim[1] = (int)Math.Floor(canvas.LocalClipBounds.Bottom / 50.0) * 50;
                canvas.Translate(canvas.LocalClipBounds.Left - axLim[0], canvas.LocalClipBounds.Top - ayLim[0]);
                scale *= LocScale;
            }
            if(iters!=0 && (Fcells.Exists(item => item[1] < ayLim[0] || item[1] > ayLim[1])))
            {
                ayLim[0] -= 250;
                ayLim[1] += 250;
                float LocScale = (canvas.LocalClipBounds.Height / (ayLim[1] - ayLim[0]));
                canvas.Clear(SKColors.White);
                canvas.Scale(LocScale, LocScale, canvas.LocalClipBounds.MidX, canvas.LocalClipBounds.MidY);
                axLim[0] = (int)Math.Floor(canvas.LocalClipBounds.Left / 50.0) * 50;
                axLim[1] = (int)Math.Floor(canvas.LocalClipBounds.Right / 50.0) * 50;
                canvas.Translate(canvas.LocalClipBounds.Left - axLim[0], canvas.LocalClipBounds.Top - ayLim[0]);
                scale *= LocScale;
            }
            
            canvas.DrawRect(canvas.LocalClipBounds, blackLines);
            for (int i = axLim[0]; i <= canvas.LocalClipBounds.Right; i += 50)
            {
                canvas.DrawLine(i, canvas.LocalClipBounds.Top, i, canvas.LocalClipBounds.Bottom, blackLines); //ayLim[s]
            }
            for (int i = ayLim[0]; i <= canvas.LocalClipBounds.Bottom; i += 50)
            {
                canvas.DrawLine(canvas.LocalClipBounds.Left, i, canvas.LocalClipBounds.Right, i, blackLines); //axLim[s]
            }

            foreach (int[] pxy in Scells)
            {
                canvas.DrawRect(pxy[0] + 2, pxy[1] + 2, 48, 48, grayFillPaint);
            }
            foreach (int[] pxy in Ccells)
            {
                canvas.DrawRect(pxy[0]+2, pxy[1]+2, 48, 48, blueFillPaint);
            }
            foreach (int[] pxy in Fcells)
            {
                canvas.DrawPoints(SKPointMode.Points, new SKPoint[] { new SKPoint(pxy[0], pxy[1]) }, redDots);
            }
            titleLabel.Text = "H = " + Hness.ToString("F4") + " (Step " + iters + ")";
        }

        private void CanvasView_Touch(object sender, SKTouchEventArgs e)
        {
            switch (e.ActionType)
            {
                case SKTouchAction.Pressed:
                    // start of a stroke
                    InitFunc(e.Location);
                    break;
                case SKTouchAction.Moved:
                    // the stroke, while pressed
                    //if (e.InContact) InitFunc(e.Location);
                    break;
                case SKTouchAction.Released:
                    // end of a stroke
                    break;
            }
            e.Handled = true; // we have handled these events
            ((SKCanvasView)sender).InvalidateSurface();
        }

        private void InitButton_Clicked(object sender, EventArgs e)
        {
            if(initButton.Text.Equals("Initialize ⋯"))
            {
                iters = 0;
                Ccells.Clear(); // Used as a flag to "reset" the canvas upon initialization!
                canvasView.InvalidateSurface();
                stepButton.IsEnabled = false;
                astepButton.IsEnabled = false;
                resetButton.IsEnabled = false;
                canvasView.EnableTouchEvents = true;
                initButton.Text = "Done •";
            }
            else
            {
                canvasView.EnableTouchEvents = false;
                if (Ccells.Count != 0)
                {
                    stepButton.IsEnabled = true;
                    astepButton.IsEnabled = true;
                    resetButton.IsEnabled = true;
                }
                initButton.Text = "Initialize ⋯";
            }
        }

        private void StepButton_Clicked(object sender, EventArgs e)
        {
            iters += 1;
            canvasView.InvalidateSurface();
        }

        private void AStepButton_Clicked(object sender, EventArgs e)
        {
            if (astepButton.Text.Equals("AUTO STEP ≫"))
            {
                astepButton.Text = "STOP ◼";
                initButton.IsEnabled = false;
                stepButton.IsEnabled = false;
                resetButton.IsEnabled = false;
                autoStop = false;
                Device.StartTimer(TimeSpan.FromSeconds(1f / 30), () =>
                {
                    iters += 1;
                    canvasView.InvalidateSurface();
                    if (iters % 500 != 0 && !autoStop)
                    {
                        return true;
                    }
                    else
                    {
                        astepButton.Text = "AUTO STEP ≫";
                        initButton.IsEnabled = true;
                        stepButton.IsEnabled = true;
                        resetButton.IsEnabled = true;
                        return false;
                    }
                });
            }
            else
            {
                astepButton.Text = "AUTO STEP ≫";
                initButton.IsEnabled = true;
                stepButton.IsEnabled = true;
                resetButton.IsEnabled = true; 
                autoStop = true;
            }
        }

        private void ResetButton_Clicked(object sender, EventArgs e)
        {
            iters = 0;
            canvasView.InvalidateSurface();
            initButton.IsEnabled = true;
            stepButton.IsEnabled = true;
            resetButton.IsEnabled = true;
        }
    }
}
