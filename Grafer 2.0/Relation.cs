using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grafer2
{
    public class Relation: List<string>
    {
        public int RemovedElementsCount { get; set; }
        public bool IsRelationValid { get; private set; }

        public Relation()
        {
            RemovedElementsCount = 0;
        }

        public Relation Adjust(Relation relation)
        {
            relation.RemoveAll(s => s == " ");

            IsRelationValid = EquationCheck.BasicCheck(relation);

            return relation;
        }

        public void RemoveNeighbors(Relation relation, int index)
        {
            relation.RemoveAt(index + 1);
            relation.RemoveAt(index - 1);
            RemovedElementsCount = 2;
        }

       
    }
}
