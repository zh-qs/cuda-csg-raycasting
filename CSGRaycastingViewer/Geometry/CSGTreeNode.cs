using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGRaycastingViewer.Geometry
{
    public class CSGTreeNode
    {
        CSGTreeNode left, right;
        Sphere sphere;
        Operation operation;
        BoundingCube boundingCube;

        public Operation Operation { get => operation; }
        public CSGTreeNode Left { get => left; }
        public CSGTreeNode Right { get => right; }
        public Sphere Shape { get => sphere; }
        public BoundingCube BoundingCube { get => boundingCube; }

        public CSGTreeNode(CSGTreeNode left, CSGTreeNode right, Operation operation)
        {
            if (operation == Operation.Sphere) throw new ArgumentException();
            this.left = left;
            this.right = right;
            this.operation = operation;
        }
        public CSGTreeNode(Sphere shape)
        {
            this.sphere = shape;
            this.operation = Operation.Sphere;
        }

        public void Normalize()
        {
            //webserver2.tecgraf.puc-rio.br/...
            if (IsPrimitive()) return;
            do
            {
                while (FirstMatchingNormalizationRuleApplied()) ;
                left.Normalize();
            } while (!(operation == Operation.Union || (right.IsPrimitive() && left.operation != Operation.Union)));
            right.Normalize();
        }

        public bool IsPrimitive() => operation == Operation.Sphere;

        bool FirstMatchingNormalizationRuleApplied()
        {
            // X-(Y+Z) = (X-Y)-Z
            if (operation == Operation.Difference && right.operation==Operation.Union)
            {
                var newLeft = new CSGTreeNode(left, right.left, Operation.Difference);
                right = right.right;
                left = newLeft;
                return true;
            }
            // X*(Y+Z) = (X*Y)+(X*Z)
            if (operation == Operation.Intersection && right.operation==Operation.Union)
            {
                var newLeft = new CSGTreeNode(left, right.left, Operation.Intersection);
                var newRight = new CSGTreeNode(left, right.right, Operation.Intersection);
                left = newLeft;
                right = newRight;
                operation = Operation.Union;
                return true;
            }
            // X-(Y*Z) = (X-Y)+(X-Z)
            if (operation == Operation.Difference && right.operation == Operation.Intersection)
            {
                var newLeft = new CSGTreeNode(left, right.left, Operation.Difference);
                var newRight = new CSGTreeNode(left, right.right, Operation.Difference);
                left = newLeft;
                right = newRight;
                operation = Operation.Union;
                return true;
            }
            // X*(Y*Z) = (X*Y)*Z
            if (operation == Operation.Intersection && right.operation == Operation.Intersection)
            {
                var newLeft = new CSGTreeNode(left, right.left, Operation.Intersection);
                right = right.right;
                left = newLeft;
                return true;
            }
            // X-(Y-Z) = (X-Y)+(X*Z)
            if (operation == Operation.Difference && right.operation == Operation.Difference)
            {
                var newLeft = new CSGTreeNode(left, right.left, Operation.Difference);
                var newRight = new CSGTreeNode(left, right.right, Operation.Intersection);
                left = newLeft;
                right = newRight;
                operation = Operation.Union;
                return true;
            }
            // X*(Y-Z) = (X*Y)-Z
            if (operation == Operation.Intersection && right.operation == Operation.Difference)
            {
                var newLeft = new CSGTreeNode(left, right.left, Operation.Intersection);
                right = right.right;
                left = newLeft;
                operation = Operation.Difference;
                return true;
            }
            // (X-Y)*Z = (X*Z)-Y
            if (operation == Operation.Intersection && left.operation == Operation.Difference)
            {
                var newLeft = new CSGTreeNode(left.left, right, Operation.Intersection);
                right = left.right;
                left = newLeft;
                operation = Operation.Difference;
                return true;
            }
            // (X+Y)-Z = (X-Z)+(Y-Z)
            if (operation == Operation.Difference && left.operation == Operation.Union)
            {
                var newLeft = new CSGTreeNode(left.left, right, Operation.Difference);
                var newRight = new CSGTreeNode(left.right, right, Operation.Difference);
                left = newLeft;
                right = newRight;
                operation = Operation.Union;
                return true;
            }
            // (X+Y)*Z = (X*Z)+(Y*Z)
            if (operation == Operation.Intersection && left.operation == Operation.Union)
            {
                var newLeft = new CSGTreeNode(left.left, right, Operation.Intersection);
                var newRight = new CSGTreeNode(left.right, right, Operation.Intersection);
                left = newLeft;
                right = newRight;
                operation = Operation.Union;
                return true;
            }
            return false;
        }

        public void CalculateBoundings()
        {
            if (IsPrimitive())
            {
                boundingCube = sphere.GetBoundingCube();
                return;
            }
            left.CalculateBoundings();
            right.CalculateBoundings();
            switch (operation)
            {
                case Operation.Union:
                    boundingCube = left.boundingCube.UnionWith(right.boundingCube);
                    break;
                case Operation.Intersection:
                    boundingCube = left.boundingCube.IntersectionWith(right.boundingCube);
                    break;
                case Operation.Difference:
                    boundingCube = left.boundingCube;
                    break;
            }
        }

        public void PruneEmptyIntersections()
        {
            if (IsPrimitive()) return;
            if (operation == Operation.Union)
            {
                if (left.boundingCube.IsEmpty())
                {
                    operation = right.operation;
                    left = right.left;
                    right = right.right;
                }
                if (right.boundingCube.IsEmpty())
                {
                    operation = left.operation;
                    left = left.left;
                    right = left.right;
                }
            }
            if (operation == Operation.Difference)
            {
                if (left.boundingCube.IntersectionWith(right.boundingCube).IsEmpty())
                {
                    operation = left.operation;
                    left = left.left;
                    right = left.right;
                }
            }
            left.PruneEmptyIntersections();
            right.PruneEmptyIntersections();
        }

        public static CSGTreeNode operator +(CSGTreeNode left, CSGTreeNode right) => new CSGTreeNode(left, right, Operation.Union);
        public static CSGTreeNode operator *(CSGTreeNode left, CSGTreeNode right) => new CSGTreeNode(left, right, Operation.Intersection);
        public static CSGTreeNode operator -(CSGTreeNode left, CSGTreeNode right) => new CSGTreeNode(left, right, Operation.Difference);

    }
}
