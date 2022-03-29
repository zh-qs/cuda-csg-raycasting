using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGRaycastingViewer.Geometry
{
    public class CSGTree
    {
        public CSGTreeNode Root { get; set; }

        public void Normalize()
        {
            Root.Normalize();
            Root.CalculateBoundings();
            Root.PruneEmptyIntersections();
        }

    }
}
