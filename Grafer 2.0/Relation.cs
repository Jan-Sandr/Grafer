using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grafer2
{
    public class Relation: List<string>
    {
        public int RemovedElementsCount { get; private set; }

        public Relation()
        {
            RemovedElementsCount = 0;
        }

        public void RemoveNeighbors(Relation relation, int index)
        {
            relation.RemoveAt(index + 1);
            relation.RemoveAt(index - 1);
        }
    }
}
