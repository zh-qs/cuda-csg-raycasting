using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGRaycastingViewer
{
    public class LightParameters
    {
        float[] lightParams = new float[6];

        public float Phi { get => lightParams[0]; set => lightParams[0] = value; }
        public float Psi { get => lightParams[1]; set => lightParams[1] = value; }
        public float Ka { get => lightParams[2]; set => lightParams[2] = value; }
        public float Kd { get => lightParams[3]; set => lightParams[3] = value; }
        public float Ks { get => lightParams[4]; set => lightParams[4] = value; }
        public float M { get => lightParams[5]; set => lightParams[5] = value; }
        public float[] RawData => lightParams;

        public static LightParameters DefaultParameters()
        {
            LightParameters l = new LightParameters();
            l.lightParams = new float[] { 0, 0, 0.1f, 0.45f, 0.45f, 30 };
            return l;
        }
    }
}
