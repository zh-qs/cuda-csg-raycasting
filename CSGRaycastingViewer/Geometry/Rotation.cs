using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGRaycastingViewer.Geometry
{
    public class Rotation
    {
        float angleX;
        float angleY;
        float angleZ;

        public Rotation(float angleX, float angleY, float angleZ)
        {
            this.angleX = angleX;
            this.angleY = angleY;
            this.angleZ = angleZ;
        }

        public Point3D GetRotatedXAxisUnitVector() // R([1,0,0])
        {
            float cosy = (float)Math.Cos(angleY),
                cosz = (float)Math.Cos(angleZ), 
                siny = (float)Math.Sin(angleX), 
                sinz = (float)Math.Sin(angleZ);
            return new Point3D(cosz * cosy, sinz * cosy, -siny);
        }

        public Point3D GetRotatedYAxisUnitVector() // R([0,1,0])
        {
            float cosx = (float)Math.Cos(angleX),
                cosy = (float)Math.Cos(angleY),
                cosz = (float)Math.Cos(angleZ),
                sinx = (float)Math.Sin(angleX),
                siny = (float)Math.Sin(angleY),
                sinz = (float)Math.Sin(angleZ);
            return new Point3D(cosz * siny * sinx - sinz * cosx, sinz * siny * sinx + cosz * cosx, cosy * sinx);
        }

        public Point3D GetRotatedZAxisUnitVector() // R([0,0,1])
        {
            float cosx = (float)Math.Cos(angleX), 
                cosy = (float)Math.Cos(angleY),
                cosz = (float)Math.Cos(angleZ),
                sinx = (float)Math.Sin(angleX),
                siny = (float)Math.Sin(angleY),
                sinz = (float)Math.Sin(angleZ);
            return new Point3D(cosz * siny * cosx + sinz * sinx, sinz * siny * cosx - cosz * sinx, cosy * cosx);
        }
    }
}
