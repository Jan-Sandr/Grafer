using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grafer2
{
    class CalculationOrder : List<List<int>>
    {
        readonly string[] mathOperations = new string[] { "+-", "*/" };

        public CalculationOrder GetOrder(Relation relation, CalculationOrder calculationOrder)
        {
            
            for(int i = 0; i < relation.Count; i++)
            {
                int index = Array.FindIndex(mathOperations, s => s.Contains(relation[i]));

                if (index != -1)
                {
                    calculationOrder[0].Add(i);
                    calculationOrder[1].Add(index);
                }
            }
            calculationOrder = SortOrder(calculationOrder);
            return calculationOrder;
        }

        private CalculationOrder SortOrder(CalculationOrder calculationOrder)
        {
            int[] indexs = calculationOrder[0].ToArray();
            int[] priorities = calculationOrder[1].ToArray();

            Array.Sort(priorities,indexs);
            Array.Reverse(indexs);
            Array.Reverse(priorities);

            calculationOrder[0] = indexs.ToList();
            calculationOrder[1] = priorities.ToList();
            return calculationOrder;
        }

        public void ShiftPosition(CalculationOrder calculationOrder, int removeCount, int index)
        {
            for(int i =0; i < calculationOrder[0].Count; i++)
            {
                if(calculationOrder[0][index] < calculationOrder[0][i])
                {
                    calculationOrder[0][i] -= removeCount;
                }
            }
        }
    }
}
