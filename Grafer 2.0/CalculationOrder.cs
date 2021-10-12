using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grafer2
{
    public class CalculationOrder : List<List<int>>
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
            int[] indexes = calculationOrder[0].ToArray();
            int[] priorities = calculationOrder[1].ToArray();

            Array.Sort(priorities,indexes);
            Array.Reverse(indexes);
            Array.Reverse(priorities);

            indexes = SortIndexes(indexes,priorities);

            calculationOrder[0] = indexes.ToList();
            calculationOrder[1] = priorities.ToList();
            return calculationOrder;
        }

        private int[] SortIndexes(int[] indexes, int[] priorities)
        {
            int sameElementsCount = 0;
            for(int i = 1; i < indexes.Length; i++)
            {
                sameElementsCount++;

                if(sameElementsCount > 1)
                {
                    if (priorities[i] != priorities[i - 1])
                    {
                        Array.Sort(indexes, i - sameElementsCount, sameElementsCount);
                        sameElementsCount = 0;
                    }

                    if (i == indexes.Length - 1)
                    {
                        Array.Sort(indexes, i - sameElementsCount, sameElementsCount + 1);
                    }
                }
            }

            return indexes;
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
