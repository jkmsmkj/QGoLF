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
        SKPaint blackLines = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = 5,
            StrokeCap = SKStrokeCap.Square
        };

        SKPaint blueFillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Blue.WithAlpha(0x80),
        };

        SKPaint redDots = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Red,
            StrokeWidth = 6,
            StrokeCap = SKStrokeCap.Round
        };

        SKPaint grayFillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Gray.WithAlpha(0x80),
        };

        readonly Random rndNum = new Random();
        List<int[]> Ccells = new List<int[]>(); // Covered cells in this call
        List<int[]> Fcells = new List<int[]>(); // Free cells for the next call
        int iters = 0;
        bool autoStop = false;
        float scale = 1;
        int[] axLim = new int[2];
        int[] ayLim = new int[2];
        private static readonly BindableProperty HnessProperty = BindableProperty.Create("Hness", typeof(float), typeof(MainPage), null);
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
            int px = (int)Math.Floor(eLoc.X / 50.0) * 50;
            int py = (int)Math.Floor(eLoc.Y / 50.0) * 50;
            int ind = Ccells.FindIndex(item => item[0] == px && item[1] == py);
            if (ind >= 0)
            {
                Ccells.RemoveAt(ind);
                //Take care of left cell
                if (Ccells.Any(p => p.SequenceEqual(new int[] { px - 50, py + 0 })))
                {
                    Fcells.Add(new int[] { px + 25, py + 25 });
                }
                ind = Fcells.FindIndex(item => item[0] == px - 25 && item[1] == py + 25);
                if (ind >= 0) Fcells.RemoveAt(ind);
                //Take care of bottom cell
                if (Ccells.Any(p => p.SequenceEqual(new int[] { px - 0, py + 50 })))
                {
                    Fcells.Add(new int[] { px + 25, py + 25 });
                }
                ind = Fcells.FindIndex(item => item[0] == px + 25 && item[1] == py + 75);
                if (ind >= 0) Fcells.RemoveAt(ind);
                //Take care of left cell
                if (Ccells.Any(p => p.SequenceEqual(new int[] { px + 50, py - 0 })))
                {
                    Fcells.Add(new int[] { px + 25, py + 25 });
                }
                ind = Fcells.FindIndex(item => item[0] == px + 75 && item[1] == py + 25);
                if (ind >= 0) Fcells.RemoveAt(ind);
                //Take care of top cell
                if (Ccells.Any(p => p.SequenceEqual(new int[] { px + 0, py - 50 })))
                {
                    Fcells.Add(new int[] { px + 25, py + 25 });
                }
                ind = Fcells.FindIndex(item => item[0] == px + 25 && item[1] == py - 25);
                if (ind >= 0) Fcells.RemoveAt(ind);
            }
            else
            {
                Ccells.Add(new int[] { px, py });
                Fcells.RemoveAll(item => item[0] == px + 25 && item[1] == py + 25);
                Fcells.Add(new int[] { px - 25, py + 25 });
                Fcells.Add(new int[] { px + 25, py + 75 });
                Fcells.Add(new int[] { px + 75, py + 25 });
                Fcells.Add(new int[] { px + 25, py - 25 });
                List<int> mtd = new List<int>();
                for (int m = 4; m > 0; m--)
                {
                    int[] Fxy = { Fcells[Fcells.Count - m][0], Fcells[Fcells.Count - m][1] };
                    bool cov = Ccells.Any(p => p.SequenceEqual(new int[] { Fxy[0] - 25, Fxy[1] - 25 }));
                    if (cov)
                    {
                        mtd.Add(m);
                    }
                }
                foreach (int m in mtd)
                {
                    int[] Fxy = { Fcells[Fcells.Count - m][0], Fcells[Fcells.Count - m][1] };
                    Fcells.RemoveAll(item => item[0] == Fxy[0] && item[1] == Fxy[1]);
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
                    Ccells.Add(new int[] { px, py });
                    Fcells.Clear();
                    Fcells.Add(new int[] { px - 25, py + 25 });
                    Fcells.Add(new int[] { px + 25, py + 75 });
                    Fcells.Add(new int[] { px + 75, py + 25 });
                    Fcells.Add(new int[] { px + 25, py - 25 });
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
            
            if (Fcells.Exists(item => item[0] < axLim[0] || item[0] > axLim[1]))
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
            if(Fcells.Exists(item => item[1] < ayLim[0] || item[1] > ayLim[1]))
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

            foreach (int[] pxy in Ccells)
            {
                canvas.DrawRect(pxy[0]+2, pxy[1]+2, 48, 48, blueFillPaint);
            }
            foreach (int[] pxy in Fcells)
            {
                canvas.DrawPoints(SKPointMode.Points, new SKPoint[] { new SKPoint(pxy[0], pxy[1]) }, redDots);
            }
            //    Dcells = setdiff(1:app.Iters, unique(app.Fcells(:, 3)));
            //    if ~isempty(Dcells)
            //        app.Ccells(logical(sum(app.Ccells(:, 3) == Dcells, 2)),:) =[];
            //        ListPcs = app.QGaxes.Children(strcmpi(get(app.QGaxes.Children, 'Type'), 'patch'));
            //        set(ListPcs(cellfun(@(x)logical(sum(x == Dcells)), get(ListPcs, 'UserData'))), 'FaceColor', 'black');
            //    end
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
                        //autoStop = true; 
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
