using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGRaycastingViewer.Geometry
{
    public class BoundingCube
    {
        public static BoundingCube Empty = new BoundingCube(new Point3D(0, 0, 0), 0, 0, 0);
        Point3D leftBottomFrontCorner;
        float width, height, depth; // X,Y,Z

        public Point3D LeftBottomFrontCorner => leftBottomFrontCorner;
        public float Width => width;
        public float Height => height;
        public float Depth => depth;

        public BoundingCube(Point3D corner, float width, float height, float depth)
        {
            leftBottomFrontCorner = corner;
            this.width = width;
            this.height = height;
            this.depth = depth;
        }

        public BoundingCube IntersectionWith(BoundingCube cube)
        {
            //if (cube.leftBottomFrontCorner.X + cube.width < leftBottomFrontCorner.X || leftBottomFrontCorner.X + width < cube.leftBottomFrontCorner.X
            //    || cube.leftBottomFrontCorner.Y + cube.height < leftBottomFrontCorner.Y || leftBottomFrontCorner.Y + height < cube.leftBottomFrontCorner.Y
            //    || cube.leftBottomFrontCorner.Z + cube.depth < leftBottomFrontCorner.Z || leftBottomFrontCorner.Z + depth < cube.leftBottomFrontCorner.Z)
            //    return Empty;
            float x, y, z, w, h, d;
            if (cube.leftBottomFrontCorner.X < leftBottomFrontCorner.X)
            {
                x = leftBottomFrontCorner.X;
                w = cube.leftBottomFrontCorner.X + cube.width - leftBottomFrontCorner.X > 0 ? cube.leftBottomFrontCorner.X + cube.width - leftBottomFrontCorner.X : 0;
            }
            else
            {
                x = cube.leftBottomFrontCorner.X;
                w = leftBottomFrontCorner.X + width - cube.leftBottomFrontCorner.X > 0 ? leftBottomFrontCorner.X + width - cube.leftBottomFrontCorner.X : 0;
            }
            if (cube.leftBottomFrontCorner.Y < leftBottomFrontCorner.Y)
            {
                y = leftBottomFrontCorner.Y;
                h = cube.leftBottomFrontCorner.Y + cube.height - leftBottomFrontCorner.Y > 0 ? cube.leftBottomFrontCorner.Y + cube.height - leftBottomFrontCorner.Y : 0;
            }
            else
            {
                y = cube.leftBottomFrontCorner.Y;
                h = leftBottomFrontCorner.Y + height - cube.leftBottomFrontCorner.Y > 0 ? leftBottomFrontCorner.Y + height - cube.leftBottomFrontCorner.Y : 0;
            }
            if (cube.leftBottomFrontCorner.Z < leftBottomFrontCorner.Z)
            {
                z = leftBottomFrontCorner.Z;
                d = cube.leftBottomFrontCorner.Z + cube.depth - leftBottomFrontCorner.Z > 0 ? cube.leftBottomFrontCorner.Z + cube.depth - leftBottomFrontCorner.Z : 0;
            }
            else
            {
                z = cube.leftBottomFrontCorner.Z;
                d = leftBottomFrontCorner.Z + depth - cube.leftBottomFrontCorner.Z > 0 ? leftBottomFrontCorner.Z + depth - cube.leftBottomFrontCorner.Z : 0;
            }
            return new BoundingCube(new Point3D(x, y, z), w, h, d);
        }
        public BoundingCube UnionWith(BoundingCube cube)
        {
            if (IsEmpty() && cube.IsEmpty()) return Empty;
            float x, y, z, w, h, d;
            if (cube.leftBottomFrontCorner.X < leftBottomFrontCorner.X)
            {
                x = cube.leftBottomFrontCorner.X;
                w = Math.Max(leftBottomFrontCorner.X - cube.leftBottomFrontCorner.X + width, cube.width);
            }
            else
            {
                x = leftBottomFrontCorner.X;
                w = Math.Max(cube.leftBottomFrontCorner.X - leftBottomFrontCorner.X + cube.width, width);
            }
            if (cube.leftBottomFrontCorner.Y < leftBottomFrontCorner.Y)
            {
                y = cube.leftBottomFrontCorner.Y;
                h = Math.Max(leftBottomFrontCorner.Y - cube.leftBottomFrontCorner.Y + height, cube.height);
            }
            else
            {
                y = leftBottomFrontCorner.Y;
                h = Math.Max(cube.leftBottomFrontCorner.Y - leftBottomFrontCorner.Y + cube.height, height);
            }
            if (cube.leftBottomFrontCorner.Z < leftBottomFrontCorner.Z)
            {
                z = cube.leftBottomFrontCorner.Z;
                d = Math.Max(leftBottomFrontCorner.Z - cube.leftBottomFrontCorner.Z + depth, cube.depth);
            }
            else
            {
                z = leftBottomFrontCorner.Z;
                d = Math.Max(cube.leftBottomFrontCorner.Z - leftBottomFrontCorner.Z + cube.depth, depth);
            }
            return new BoundingCube(new Point3D(x, y, z), w, h, d);
        }
        public BoundingCube DifferenceWith(BoundingCube cube) => this;

        public bool IsEmpty() => width <= 0 || height <= 0 || depth <= 0;
    }
}
