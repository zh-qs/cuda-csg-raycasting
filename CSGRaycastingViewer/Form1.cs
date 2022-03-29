using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using CSGRaycastingViewer.Geometry;

namespace CSGRaycastingViewer
{
    public partial class Form1 : Form
    {
        //[DllImport(@"C:\Users\szymo\Desktop\pw\sem5\gpu\CSGRaycasting\x64\Debug\CSGRaycastingCUDALib.dll", CharSet = CharSet.Ansi, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        //public static extern void GPURender(int[] h_out, int width, int height, int[] raw_tree, int treeSize, float[] raw_spheres, int spheresSize);
        const float pi = 3.14159265358979323946f;
        Graphics g;
        //int[] raw_tree = new int[] { 1, -1, 3, -1, -1, 2, -1, 4, -1, -1, 0, 3, 0, 3, 3, -1, 0, -1, 1, 2 };
        //float[] raw_spheres = new float[] { 0, 0, 0.3f, 0, 0.5f, 0.25f, 1.5f, 1.5f, 1.5f, 0.5f, 0.5f, 0.5f };
        CSGTree tree;/* = new CSGTree()*/
        //{
          //  Root = GenerateRandomTree()
                //(((CSGTreeNode)new Sphere(new Point3D(0, 0, 1.5f), 0.5f, Color.Green)
                //+ new Sphere(new Point3D(0.5f, 0, 1.5f), 0.5f, Color.Red))
                //-  new Sphere(new Point3D(0.25f, 0.3f, 1.5f), 0.5f, Color.Blue))
        //};
        GPURenderer renderer;
        DirectBitmap bitmap;
        public Form1()
        {
            InitializeComponent();
            tree = new CSGTree
            {
                Root =
                  (new Sphere(new Point3D(0.5f, 0, 1.5f), 0.5f, Color.Red)
                 + (CSGTreeNode)new Sphere(new Point3D(0, 0, 1.5f), 0.5f, Color.Green))

               - new Sphere(new Point3D(0.25f, 0.3f, 1.5f), 0.5f, Color.Blue)
            };
            // {Root= GenerateRandomTree(1024) };
            renderer = new GPURenderer(tree);
            bitmap = new DirectBitmap(splitContainer1.Panel1.Width, splitContainer1.Panel1.Height); 
        }

        CSGTreeNode GenerateRandomTree(int n)
        {
            Random random = new Random();
            float rnd() => (float)random.NextDouble();
            CSGTreeNode node = new CSGTreeNode(new Sphere(new Point3D(rnd(),rnd(),rnd()), rnd(), Color.White));
            for (int i=1;i<n;i++)
            {
                node += new Sphere(new Point3D(rnd(), rnd(), rnd()), rnd(), Color.White);
            }
            return node;            
        }

        void Render()
        {
            g = splitContainer1.Panel1.CreateGraphics();
            //GPURender(bitmap.Bits, Width, Height, raw_tree, 5, raw_spheres, 3);
            renderer.RenderTo(bitmap);
            g.DrawImage(bitmap.Bitmap, new Point(0, 0));
            
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {
            Render();
        }

        private void splitContainer1_Panel1_SizeChanged(object sender, EventArgs e)
        {
            if (bitmap != null) bitmap.Dispose();
            bitmap = new DirectBitmap(splitContainer1.Panel1.Width, splitContainer1.Panel1.Height);
            splitContainer1.Panel1.Invalidate();
        }

        private void trackBar_Scroll(object sender, EventArgs e)
        {
            renderer.SetCameraRotation(new Rotation(
                rotationXTrackBar.Value * pi / 180.0f,
                rotationYTrackBar.Value * pi / 180.0f,
                rotationZTrackBar.Value * pi / 180.0f));
            rotationXLabel.Text = rotationXTrackBar.Value.ToString();
            rotationYLabel.Text = rotationYTrackBar.Value.ToString();
            rotationZLabel.Text = rotationZTrackBar.Value.ToString();
            Render();
        }

        private void lightDirectionTrackBar_Scroll(object sender, EventArgs e)
        {
            renderer.SetLightDirection(
                phiLightDirectionTrackBar.Value * pi / 180.0f,
                psiLightDirectionTrackBar.Value * pi / 180.0f
                );
            phiLabel.Text = phiLightDirectionTrackBar.Value.ToString();
            psiLabel.Text = psiLightDirectionTrackBar.Value.ToString();
            Render();
        }

     

        private void lightParamtersTrackBar_Scroll(object sender, EventArgs e)
        {
            renderer.SetLightParameters(kaTrackBar.Value / 100.0f, kdTrackBar.Value / 100.0f, ksTrackBar.Value / 100.0f, mTrackBar.Value);
            kaLabel.Text = (kaTrackBar.Value / 100.0f).ToString();
            kdLabel.Text = (kdTrackBar.Value / 100.0f).ToString();
            ksLabel.Text = (ksTrackBar.Value / 100.0f).ToString();
            mLabel.Text = mTrackBar.Value.ToString();
            Render();
        }
    }
}
