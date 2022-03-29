using CSGRaycastingViewer.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGRaycastingViewer
{
    class CameraParameters
    {
        float[] cameraParams = new float[] { 0, 0, 0, 0, 0, 1, 0, 1, 0 };
        float screenDistance, screenHeight;
        
        public Point3D Observer
        {
            get => new Point3D(cameraParams[0], cameraParams[1], cameraParams[2]);
            set
            {
                cameraParams[0] = value.X;
                cameraParams[1] = value.Y;
                cameraParams[2] = value.Z;
            }
        }
        public Point3D Direction
        {
            get => new Point3D(cameraParams[3], cameraParams[4], cameraParams[5]);
            set
            {
                cameraParams[3] = value.X;
                cameraParams[4] = value.Y;
                cameraParams[5] = value.Z;
            }
        }
        public Point3D HorizontalVector
        {
            get =>new Point3D(cameraParams[6], cameraParams[7], cameraParams[8]);
            set 
            {
                cameraParams[6] = value.X;
                cameraParams[7] = value.Y;
                cameraParams[8] = value.Z;
            }
        }
        public float[] RawData => cameraParams;
        public float ScreenDistance => screenDistance;
        public float ScreenHeight => screenHeight;

        public CameraParameters()
        {
            screenDistance = 0.5f; 
            screenHeight = 0.5f;
        }

        public static CameraParameters LookAtCube(BoundingCube cube)
        {
            return LookAtCube(cube, new Point3D(0, 0, 1), new Point3D(0, 1, 0));
        }

        public static CameraParameters LookAtCube(BoundingCube cube, Point3D direction, Point3D horizontalVector)
        {
            // sprawdzić czy wektory w argumentach są prostopadłe!
            CameraParameters parameters = new CameraParameters();
            float cubeDiameter = (float)Math.Sqrt(cube.Width * cube.Width + cube.Height * cube.Height + cube.Depth * cube.Depth);
            float distanceFromObserverToCubeCenter = cubeDiameter * (float)Math.Sqrt(parameters.screenDistance * parameters.screenDistance + parameters.screenHeight * parameters.screenHeight / 4) / parameters.screenHeight;
            Point3D observer = new Point3D(
                cube.LeftBottomFrontCorner.X + cube.Width / 2 - distanceFromObserverToCubeCenter * direction.X,
                cube.LeftBottomFrontCorner.Y + cube.Height / 2 - distanceFromObserverToCubeCenter * direction.Y,
                cube.LeftBottomFrontCorner.Z + cube.Depth / 2 - distanceFromObserverToCubeCenter * direction.Z
                );
            parameters.Observer = observer;
            parameters.Direction = direction;
            parameters.HorizontalVector = horizontalVector;
            return parameters;
        }

        //public void MoveCameraForward(float velocity)
        //{
        //    cameraParams[0] += cameraParams[3] * velocity;
        //    cameraParams[1] += cameraParams[4] * velocity;
        //    cameraParams[2] += cameraParams[5] * velocity;
        //}
        //public void MoveCameraLeft(float velocity)
        //{
        //    cameraParams[0] += cameraParams[6] * velocity;
        //    cameraParams[1] += cameraParams[7] * velocity;
        //    cameraParams[2] += cameraParams[8] * velocity;
        //}
        //public void MoveCameraUp(float velocity)
        //{
        //    cameraParams[0] += (cameraParams[4] * cameraParams[8] - cameraParams[5] * cameraParams[7]) * velocity;
        //    cameraParams[0] += (cameraParams[5] * cameraParams[6] - cameraParams[3] * cameraParams[8]) * velocity;
        //    cameraParams[0] += (cameraParams[3] * cameraParams[7] - cameraParams[4] * cameraParams[6]) * velocity;
        //}

        //public void RotateCameraLeft(float angle)
        //{
        //    float sin = (float)Math.Sin(angle), cos = (float)Math.Cos(angle);
        //    float ux = cameraParams[4] * cameraParams[8] - cameraParams[5] * cameraParams[7],
        //        uy = cameraParams[5] * cameraParams[6] - cameraParams[3] * cameraParams[8],
        //        uz = cameraParams[3] * cameraParams[7] - cameraParams[4] * cameraParams[6];
        //    for (int i = 3; i < 7; i += 3)
        //    {
        //        cameraParams[i] = cameraParams[i] * (cos + ux * ux * (1 - cos)) + cameraParams[i + 1] * (ux * uy * (1 - cos) - uz * sin) + cameraParams[i + 2] * (ux * uz * (1 - cos) + uy * sin);
        //        cameraParams[i + 1] = cameraParams[i] * (uy * ux * (1 - cos) + uz * sin) + cameraParams[i + 1] * (cos + uy * uy * (1 - cos)) + cameraParams[i + 2] * (uy * uz * (1 - cos) - ux * sin);
        //        cameraParams[i + 2] = cameraParams[i] * (uz * ux * (1 - cos) - uy * sin) + cameraParams[i + 1] * (uz * uy * (1 - cos) + ux * sin) + cameraParams[i + 2] * (cos + uz * uz * (1 - cos));
        //        float d = (float)Math.Sqrt(cameraParams[i] * cameraParams[i] + cameraParams[i + 1] * cameraParams[i + 1] + cameraParams[i + 2] * cameraParams[i + 2]);
        //        cameraParams[i] /= d;
        //        cameraParams[i + 1] /= d;
        //        cameraParams[i + 2] /= d;
        //    }
        //}
        //public void RotateCameraUp(float angle)
        //{
        //    float sin = (float)Math.Sin(angle), cos = (float)Math.Cos(angle);
        //    float ux = cameraParams[6],
        //        uy = cameraParams[7],
        //        uz = cameraParams[8];

        //    cameraParams[3] = cameraParams[3] * (cos + ux * ux * (1 - cos)) + cameraParams[4] * (ux * uy * (1 - cos) - uz * sin) + cameraParams[5] * (ux * uz * (1 - cos) + uy * sin);
        //    cameraParams[4] = cameraParams[3] * (uy * ux * (1 - cos) + uz * sin) + cameraParams[4] * (cos + uy * uy * (1 - cos)) + cameraParams[5] * (uy * uz * (1 - cos) - ux * sin);
        //    cameraParams[5] = cameraParams[3] * (uz * ux * (1 - cos) - uy * sin) + cameraParams[4] * (uz * uy * (1 - cos) + ux * sin) + cameraParams[5] * (cos + uz * uz * (1 - cos));
        //    float d = (float)Math.Sqrt(cameraParams[3] * cameraParams[3] + cameraParams[4] * cameraParams[4] + cameraParams[5] * cameraParams[5]);
        //    cameraParams[3] /= d;
        //    cameraParams[4] /= d;
        //    cameraParams[5] /= d;

        //}

    }
}
