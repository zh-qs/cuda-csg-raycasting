using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGRaycastingViewer.Geometry
{
    public class Sphere
    {
        public const int DataCount = 4;
        public Point3D Center { get; }
        public float Radius { get; }
        public Color Color { get; }
       
        public Sphere(Point3D center, float radius, Color color)
        {
            Center = center;
            Radius = radius;
            Color = color;
        }
        public BoundingCube GetBoundingCube()
        {
            return new BoundingCube(new Point3D(Center.X - Radius, Center.Y - Radius, Center.Z - Radius), 2 * Radius, 2 * Radius, 2 * Radius);
        }

        public static implicit operator CSGTreeNode(Sphere sphere)
        {
            return new CSGTreeNode(sphere);
        }
    }
}
