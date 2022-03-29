using CSGRaycastingViewer.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CSGRaycastingViewer
{
    public class GPURenderer
    {
        [DllImport(@"C:\Users\szymo\Desktop\pw\sem5\gpu\CSGRaycasting\x64\Release\CSGRaycastingCUDALib.dll", CharSet = CharSet.Ansi, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        private static extern void GPURender(
            int[] h_out, 
            int width, 
            int height, 
            float[/*9*/] cameraParams, 
            float planeDistance, 
            float planeHeight,
            /*int[] tree_shapeIds,*/ 
            float[] lightParams,
            int[] tree_flags, 
            int[] color_data, 
            float[] raw_spheres, 
            int treeSize);

        CSGTree tree;
        RawCSGData data;
        CameraParameters camera;
        LightParameters light;

        public GPURenderer(CSGTree tree)
        {
            this.tree = tree;
            tree.Normalize();
            List<int> shapeIdList = new List<int>();
            List<Sphere> shapeList = new List<Sphere>();
            List<int> operationList = new List<int>();
            List<int> colorList = new List<int>();
            List<int> intersectionCounts = new List<int>();
            int shapes = 0;
            ParseTreeInOrder(tree.Root, shapeIdList, shapeList, operationList, colorList, intersectionCounts, ref shapes);
            operationList.Insert(0, (int)Operation.Intersection); // dodajemy "przecięcie z przestrzenią"
            data = new RawCSGData();
            data.flags = operationList.ToArray();
            data.shapeIds = shapeIdList.ToArray();
            data.raw_spheres = ToRawShapeData(shapeList);
            data.color_data = colorList.ToArray();
            data.SetFlags();
            camera = CameraParameters.LookAtCube(tree.Root.BoundingCube);
            light = LightParameters.DefaultParameters();
        }

        void ParseTreeInOrder(CSGTreeNode node, List<int> shapeIdList, List<Sphere> shapeList, List<int> operationList, List<int> colorList, List<int> intersectionCounts, ref int shapes)
        {
            if (node.IsPrimitive())
            {
                shapeIdList.Add(shapes++);
                shapeList.Add(node.Shape);
                colorList.Add(node.Shape.Color.ToArgb());
                return;
            }
            ParseTreeInOrder(node.Left, shapeIdList, shapeList, operationList, colorList, intersectionCounts, ref shapes);
            operationList.Add((int)node.Operation);
            ParseTreeInOrder(node.Right, shapeIdList, shapeList, operationList, colorList, intersectionCounts, ref shapes);
        }

        float[] ToRawShapeData(List<Sphere> shapes)
        {
            float[] raw = new float[shapes.Count * Sphere.DataCount];
            int i = 0;
            foreach (var shape in shapes)
            {
                raw[i] = shape.Center.X;
                raw[i + shapes.Count] = shape.Center.Y;
                raw[i + 2 * shapes.Count] = shape.Center.Z;
                raw[i + 3 * shapes.Count] = shape.Radius;
                ++i;
            }
            return raw;
        }

        public void SetCameraRotation(Rotation rotation)
        {
            camera = CameraParameters.LookAtCube(tree.Root.BoundingCube, rotation.GetRotatedZAxisUnitVector(), rotation.GetRotatedYAxisUnitVector());
        }

        public void SetLightDirection(float phi, float psi)
        {
            light.Phi = phi;
            light.Psi = psi;
        }

        public void SetLightParameters(float ka, float kd, float ks, float m)
        {
            light.Ka = ka;
            light.Kd = kd;
            light.Ks = ks;
            light.M = m;
        }

        public void RenderTo(DirectBitmap bitmap)
        {
            GPURender(bitmap.Bits, bitmap.Width, bitmap.Height, camera.RawData, camera.ScreenDistance, camera.ScreenHeight, light.RawData,/*data.shapeIds,*/ data.flags, data.color_data, data.raw_spheres, data.shapeIds.Length);
        }
        
        public class RawCSGData
        {
            public int[] shapeIds;
            public int[] flags;
            public int[] color_data;
            public float[] raw_spheres;

            public void SetFlags()
            {
                List<int> intCounts = new List<int>();
                int count = 1;
                for (int i=1;i<flags.Length;i++)
                {
                    if (flags[i] == (int)Operation.Union)
                    {
                        intCounts.Add(count);
                        count = 1;
                    }
                    else if (flags[i] == (int)Operation.Intersection)
                    {
                        count++;
                    }
                }
                intCounts.Add(count);
                int countsIndex = 0;
                int segIndex = 0;
                flags[0] = MakeFlags(countsIndex, intCounts[countsIndex], 1, flags[0]);
                for (int i=1;i<flags.Length;i++)
                {
                    if (flags[i] == (int)Operation.Union)
                    {
                        flags[i] = MakeFlags(++countsIndex, intCounts[countsIndex], 1, (int)Operation.Intersection);
                    }
                    else
                    {
                        flags[i] = MakeFlags(countsIndex, intCounts[countsIndex], 0, flags[i]);
                    }
                }
            }

            int MakeFlags(int intId, int intCount, int newSeg, int op) => ((intId << 16) | (intCount << 3) | (newSeg << 2) | op);
        }
    }
}
