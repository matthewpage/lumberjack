using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Matthew.Meters
{
    public class MultiPPM : UserControl
    {
        private int _channels = 64;

        private bool _showPPM = false;
        private bool _showNumbers = false;
        private float[] _level;
        private float[] _currentLevel;

        private float[] _peakHold;
        private float[] _currentPeakHold;
        protected Timer _peakTimer;

        protected Timer _timer;

        private int[] ppmPixel = new int[7];

        private Bitmap displayBitmap;

        private SolidBrush backgroundBrush;

        private SolidBrush section1Brush;
        private SolidBrush section2Brush;
        private SolidBrush section3Brush;

        private Font drawFont;
        private SolidBrush drawBrush;
        private Pen notchPen;
        private Pen peakPen;

        private float div7 = 1 / 7.0f;

        private Rectangle _lastRedrawArea;


        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // DigitalPPM
            // 
            this.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Name = "DigitalPPM";
            this.Resize += new System.EventHandler(this.DigitalPPM_Resize);
            this.ResumeLayout(false);

        }

        #endregion


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                displayBitmap.Dispose();
                backgroundBrush.Dispose();
                section1Brush.Dispose();
                section2Brush.Dispose();
                section3Brush.Dispose();
                drawFont.Dispose();
                drawBrush.Dispose();
                notchPen.Dispose();
                peakPen.Dispose();
            }
            base.Dispose(disposing);
        }

        public MultiPPM()
        {
            displayBitmap = new Bitmap(this.Width, this.Height);
            InitializeComponent();

            _level = new float[_channels];
            _currentLevel = new float[_channels]; 
            _peakHold = new float[_channels];
            _currentPeakHold = new float[_channels];

            _peakTimer = new Timer();
            _peakTimer.Interval = 2000;
            _peakTimer.Enabled = true;
            _peakTimer.Tick += new EventHandler(peakTimer_Tick);

            backgroundBrush = new SolidBrush(System.Drawing.SystemColors.Control);
                //new SolidBrush(Color.Black);
            section1Brush = new SolidBrush(Color.LimeGreen);
            section2Brush = new SolidBrush(Color.Yellow);
            section3Brush = new SolidBrush(Color.Red);
           
            drawFont = new Font("Arial", 12);
            drawBrush = new SolidBrush(Color.White);

            notchPen = new Pen(Color.White);
            peakPen = new Pen(Color.Red);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Graphics offScreenDC;
            offScreenDC = Graphics.FromImage(displayBitmap);
            RectangleF visableBounds = offScreenDC.VisibleClipBounds;

            try
            {
                // paint the background colour
                offScreenDC.FillRectangle(backgroundBrush, visableBounds);

                int widthOfSingleMeter = (ClientRectangle.Width - (2*(_channels-1))) / _channels;

                for (int i = 0; i < _channels; i++)
                {
                    int meterLeft = (2 * i) + (i * widthOfSingleMeter);
                    /* THIS PAINTS A VERTICAL PPM */

                    // work out the overall height of the meter display
                    int _clearHeight = (int)(visableBounds.Height * (_level[i] * div7));

                    // draw section 1 (up to ppm4)
                    Rectangle r = new Rectangle(meterLeft, (int)(visableBounds.Height - _clearHeight), widthOfSingleMeter, _clearHeight);
                    offScreenDC.FillRectangle(section1Brush, r);

                    if (_level[i] > 4.0f)
                    {
                        int _section2 = _clearHeight - ppmPixel[4];
                        r = new Rectangle(meterLeft, (int)(visableBounds.Height - ppmPixel[4] - _section2), widthOfSingleMeter, _section2);
                        offScreenDC.FillRectangle(section2Brush, r);
                    }

                    if (_level[i] > 6.0f)
                    {
                        int _section3 = _clearHeight - ppmPixel[6];
                        r = new Rectangle(meterLeft, (int)(visableBounds.Height - ppmPixel[6] - _section3), widthOfSingleMeter, _section3);
                        offScreenDC.FillRectangle(section3Brush, r);
                    }

                    if (_peakHold[i] > 0)
                    {
                        float yp = (int)(visableBounds.Height - (visableBounds.Height * (_peakHold[i] * div7)));
                        offScreenDC.DrawLine(peakPen, meterLeft, yp, meterLeft + widthOfSingleMeter, yp);
                    }
                }

                if (_showPPM == true)
                {
                    for (int notch = 1; notch < 7; notch++)
                    {
                        float x = 10;
                        float y = visableBounds.Height - ppmPixel[notch];
                        offScreenDC.DrawLine(notchPen, 0, ppmPixel[notch], 10, ppmPixel[notch]);
                        if (_showNumbers == true)
                        {
                            offScreenDC.DrawString(notch.ToString(), drawFont, drawBrush, x, y - 9);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("PPM (" + this.Name + ") OnPaint 1: " + ex.Message);
            }

            try
            {
                g.DrawImage(displayBitmap, e.ClipRectangle, e.ClipRectangle,GraphicsUnit.Pixel);
            }
            catch (Exception ex)
            {
                Console.WriteLine("PPM (" + this.Name + ") OnPaint 2: " + ex.Message);
            }

            try
            {
                offScreenDC.Dispose();
                g.Dispose();
                offScreenDC = null;
                g = null;
                GC.Collect();
            }
            catch (Exception ex)
            {
                Console.WriteLine("PPM (" + this.Name + ") OnPaint 3: " + ex.Message);
            }
        }

        private void peakTimer_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < _channels; i++)
            {
                _peakHold[i] = 0;
            }
        }

        public void SetLevel(int channel, float value)
        {
            _level[channel] = value;

            if (float.IsNaN(_level[channel])) _level[channel] = 0.0f;
            if (_level[channel] > _peakHold[channel]) _peakHold[channel] = _level[channel];
        }


        private void DigitalPPM_Resize(object sender, EventArgs e)
        {
            for (int i = 1; i < 7; i++)
            {
                ppmPixel[i] = (int)(this.ClientRectangle.Height * (i / 7.0f));
            }

            displayBitmap = new Bitmap(ClientRectangle.Width, ClientRectangle.Height);
            _lastRedrawArea = ClientRectangle;
        }

    }
}
