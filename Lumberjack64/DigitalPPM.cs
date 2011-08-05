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
    public class DigitalPPM : UserControl
    {
        private bool _horizontal = false;
        private bool _showPPM = true;
        private bool _showNumbers = false;
        private float _level;
        private float _currentLevel;

        private float _peakHold;
        private float _currentPeakHold;
        protected Timer _peakTimer;

        protected Timer _timer;

        private int[] ppmPixel = new int[7];

        private Bitmap displayBitmap;

        private SolidBrush backgroundBrush;

        private SolidBrush section1Brush;
        private SolidBrush section2Brush;
        private SolidBrush section3Brush;

        /*
        private LinearGradientBrush section1Brush;
        private LinearGradientBrush section2Brush;
        private LinearGradientBrush section3Brush;
        */
      

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
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Name = "DigitalPPM";
            this.Size = new System.Drawing.Size(24, 160);
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

        public DigitalPPM()
        {
            displayBitmap = new Bitmap(this.Width, this.Height);
            InitializeComponent();        

            _peakTimer = new Timer();
            _peakTimer.Interval = 2000;
            _peakTimer.Enabled = true;
            _peakTimer.Tick += new EventHandler(peakTimer_Tick);

            backgroundBrush = new SolidBrush(Color.Black);
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

                if (_horizontal == true)
                {
                    /* THIS PAINTS A HORIZONTAL PPM */

                    // work out the overall width of the meter display
                    int _clearWidth = (int)(visableBounds.Width * (_level * div7));

                    // draw section 1 (up to ppm4)              
                    Rectangle r = new Rectangle(0, 0, (int)_clearWidth, (int)visableBounds.Height);
                    offScreenDC.FillRectangle(section1Brush, r);

                    if (_level > 4.0f)
                    {
                        r = new Rectangle(ppmPixel[4], 0, (int)(_clearWidth - ppmPixel[4]), (int)visableBounds.Height);
                        offScreenDC.FillRectangle(section2Brush, r);
                    }

                    if (_level > 6.0f)
                    {
                        r = new Rectangle(ppmPixel[6], 0, (int)(_clearWidth - ppmPixel[6]), (int)visableBounds.Height);
                        offScreenDC.FillRectangle(section3Brush, r);
                    }

                    if (_showPPM == true)
                    {
                        for (int notch = 1; notch < 7; notch++)
                        {
                            float x = ppmPixel[notch];
                            float y = 5;
                            offScreenDC.DrawLine(notchPen, ppmPixel[notch], 0, ppmPixel[notch], 5);
                            if (_showNumbers == true)
                            {
                                offScreenDC.DrawString(notch.ToString(), drawFont, drawBrush, x - 7, y);
                            }
                        }
                    }

                    if (_peakHold > 0)
                    {
                        float xp = (int)(visableBounds.Width * (_peakHold * div7));
                        offScreenDC.DrawLine(peakPen, xp, 0, xp, visableBounds.Height);
                    }
                }
                else
                {
                    /* THIS PAINTS A VERTICAL PPM */

                    // work out the overall height of the meter display
                    int _clearHeight = (int)(visableBounds.Height * (_level * div7));

                    // draw section 1 (up to ppm4)
                    Rectangle r = new Rectangle(0, (int)(visableBounds.Height - _clearHeight), (int)(visableBounds.Width), _clearHeight);
                    offScreenDC.FillRectangle(section1Brush, r);

                    if (_level > 4.0f)
                    {
                        int _section2 = _clearHeight - ppmPixel[4];
                        r = new Rectangle(0, (int)(visableBounds.Height - ppmPixel[4] - _section2), (int)(visableBounds.Width), _section2);
                        offScreenDC.FillRectangle(section2Brush, r);
                    }

                    if (_level > 6.0f)
                    {
                        int _section3 = _clearHeight - ppmPixel[6];
                        r = new Rectangle(0, (int)(visableBounds.Height - ppmPixel[6] - _section3), (int)(visableBounds.Width), _section3);
                        offScreenDC.FillRectangle(section3Brush, r);
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

                    if (_peakHold > 0)
                    {
                        float yp = (int)(visableBounds.Height - (visableBounds.Height * (_peakHold * div7)));
                        offScreenDC.DrawLine(peakPen, 0, yp, visableBounds.Width, yp);
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
            _peakHold = 0;
        }

        private void doInvalidate()
        {
            if (_level != _currentLevel)
            {
                _currentLevel = _level;
                this.Invalidate();
            }

            if (_peakHold != _currentPeakHold)
            {
                _currentPeakHold = _peakHold;
                this.Invalidate();
            }
        }

        public float level
        {
            set
            {
                _level = value;

                if (float.IsNaN(_level)) _level = 0.0f;
                if (_level > _peakHold) _peakHold = _level;
                this.doInvalidate();
            }
        }

        public bool Horizontal 
        {
            set
            {
                if (_horizontal != value)
                {
                    _horizontal = value;
                }
            }
            get
            {
                return _horizontal;
            }
        }

        private void DigitalPPM_Resize(object sender, EventArgs e)
        {
            for (int i=1; i < 7; i++) {
                if (_horizontal == true)
                {
                    ppmPixel[i] = (int)(this.ClientRectangle.Width * (i / 7.0f));
                }
                else
                {
                    ppmPixel[i] = (int)(this.ClientRectangle.Height * (i / 7.0f));
                }
            }

            displayBitmap = new Bitmap(ClientRectangle.Width, ClientRectangle.Height);
            _lastRedrawArea = ClientRectangle;
        }

    }
}
