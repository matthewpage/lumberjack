using System;
using System.Text;
using System.Windows.Forms;

namespace Matthew.Meters
{
    public class ppmObj
    {
        // decay time 24dB in 2.8 seconds
        float DROP_FACTOR = (float)(Math.Pow(10.0f, (-24.0f / 20.0f)));
        float DROP_TIME = 2.8f;

        float LOCK_ON_FACTOR = (1.0f - 0.6f);
        float LOCK_ON_TIME = 0.0025f;

        float CLIP_PPM = 8.5f;
        float SAMPLE_LIMIT = 32767;
        float DB_PER_PPM = 4.0f;        // dB between PPMs
        float DB_CONST = 20.0f;

        float MIN_INTERMEDIATE = 0.001f;

        int channels;
        float[] intermediateValue;
        float[] maxValue;
        float[] minValue;

        private float drop;         // fall time factor
        private float lockonfract;   // rise time rate
        private float sampleRate;

        public ppmObj(int ch, float sr)
        {
            this.channels = ch;
            this.sampleRate = sr;

            intermediateValue = new float[channels];
            maxValue = new float[channels];
            minValue = new float[channels];

            drop = (float)Math.Pow(DROP_FACTOR, (1.0 / (sampleRate * DROP_TIME)));
            lockonfract = (float)(1.0f - Math.Pow(LOCK_ON_FACTOR, (1.0f / (sampleRate * LOCK_ON_TIME))));
        }

        public void calculateIntermediate(int ch, int value)
        {
            if (ch < channels)
            {
                float tmpPPM = intermediateValue[ch];
                float absValue = Math.Abs((float)value);

                if (absValue > tmpPPM)
                {
                    //tmpPPM = tmpPPM + lockonfract * (absValue - tmpPPM);   // this is proper PPM
                    tmpPPM = absValue;   // this shows the peaks as they happen
                }
                else
                {
                    tmpPPM *= drop;
                }

                intermediateValue[ch] = tmpPPM;
                if (float.IsNaN(intermediateValue[ch])) intermediateValue[ch] = 0.0f;
            }        
        }

        public float getM3()
        {
            float mono;
            if (channels == 2)
            {
                // A+B -3dB
                mono = (intermediateValue[0] + intermediateValue[1]) * 0.7071f;
            }
            else
            {
                mono = intermediateValue[0];
            }

            return convertIntermediate(mono);
        }

        public float getM6()
        {
            float mono;
            if (channels == 2)
            {
                // A+B -6dB
                mono = (intermediateValue[0] + intermediateValue[1]) * 0.5f;
            }
            else
            {
                mono = intermediateValue[0];
            }

            return convertIntermediate(mono);
        }

        public float getValue(int ch)
        {
            if (ch >= channels)
            {
                // channel value out of range
                return 0.0f;
            }
            else
            {
                return convertIntermediate(intermediateValue[ch]);
            }
        }

        private float convertIntermediate(float value)
        {
            if (value < MIN_INTERMEDIATE)
            {
                return 0.0f;
            }
            else
            {
                float ppmVal;
                
                ppmVal = (float)(CLIP_PPM - (DB_CONST / DB_PER_PPM * Math.Log10(SAMPLE_LIMIT / value)));                  
                /*
                if (ppmVal < 1.0f)
                {
                    // scale to 24dB per PPM instead of 4dB per PPM //
                    ppmVal = 1.0f - 1.0f / (24.0f / DB_PER_PPM) + (1.0f / (24.0f / DB_PER_PPM)) * ppmVal;
                } 
                */

                if (float.IsNaN(ppmVal)) ppmVal = 0.0f;          
                return (ppmVal < 0.001f) ? 0.0f : ppmVal;
            }
        }
    }
}
