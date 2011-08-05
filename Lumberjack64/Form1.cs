using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Matthew.Meters;

namespace Lumberjack64
{
    public partial class Form1 : Form
    {
        DigitalPPM[] meters;
        ppmObj ppm;
        float[] levels;

        public Form1()
        {
            InitializeComponent();

            ppm = new ppmObj(64, 44100);
            meters = new DigitalPPM[64];
            levels = new float[64];
            /*
            for (int i = 0; i < 64; i++)
            {
                meters[i] = new DigitalPPM();
                meters[i].BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                meters[i].Horizontal = false;
                meters[i].Location = new System.Drawing.Point(12 + (i*18), 12);
                meters[i].Name = "PPM" + i.ToString();
                meters[i].Size = new System.Drawing.Size(12, 242);
                this.Controls.Add(meters[i]);
            }
            */
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Random rnd = new Random();

            for (int i = 0; i < 64; i++)
            {
                levels[i] = (int)(rnd.NextDouble() * 8);
                multiPPM1.SetLevel(i, levels[i]);
            }

            multiPPM1.Invalidate();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < 64; i++)
            {
                levels[i] = levels[i] * 0.95f;
                multiPPM1.SetLevel(i, levels[i]);
            }
            multiPPM1.Invalidate();
        }
    }
}
