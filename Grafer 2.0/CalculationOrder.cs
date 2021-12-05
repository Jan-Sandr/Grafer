using System;
using System.Collections.Generic;
using System.Linq;

namespace Grafer2
{
    public class CalculationOrder : List<List<int>>
    {
        private readonly string[] mathCharacters = new string[] { "+-", "*/", "^", "", "()" };

        //Získání výpočetního postupu na základě priorit operací a závorek.
        public CalculationOrder GetOrder(Relation relation, CalculationOrder calculationOrder)
        {
            int additionalPriority = 0;  // pro závorku

            for (int i = 0; i < relation.Count; i++)
            {
                int priority = Array.FindIndex(mathCharacters, s => s.Contains(relation[i]));

                if (priority == 4) // případ závorky
                {
                    additionalPriority = relation[i] == "(" ? additionalPriority + 4 : additionalPriority - 4;
                }
                else if (priority != -1)
                {
                    calculationOrder[0].Add(i);
                    calculationOrder[1].Add(priority + additionalPriority);
                }
            }
            calculationOrder = SortOrder(calculationOrder);
            return calculationOrder;
        }

        //Seřazení priority na základě jejich výše se zachováním pozice v předpisu.
        private static CalculationOrder SortOrder(CalculationOrder calculationOrder)
        {
            int[] indexes = calculationOrder[0].ToArray();
            int[] priorities = calculationOrder[1].ToArray();

            Array.Sort(priorities, indexes);
            Array.Reverse(indexes);
            Array.Reverse(priorities);

            indexes = SortIndexes(indexes, priorities);

            calculationOrder[0] = indexes.ToList();
            calculationOrder[1] = priorities.ToList();
            return calculationOrder;
        }

        //Když je více operací se stejnou prioritou za sebou, tak aby postup výpočtu šel z leva.
        private static int[] SortIndexes(int[] indexes, int[] priorities)
        {
            int sameElementsCount = 1;
            for (int i = 1; i < indexes.Length; i++)
            {
                if (priorities[i] == priorities[i - 1])
                {
                    sameElementsCount++;
                }
                else
                {
                    if (sameElementsCount > 1)
                    {
                        if (priorities[i - 1] != 2) // Výjimka pro mocninua a odmocninu u těch se jde nejdříve zprava. 
                        {
                            Array.Sort(indexes, i - sameElementsCount, sameElementsCount);
                        }
                    }

                    sameElementsCount = 1;
                }

                if (i == indexes.Length - 1)
                {
                    if (priorities[i] != 2)
                    {
                        Array.Sort(indexes, (i + 1) - sameElementsCount, sameElementsCount);
                    }
                }

            }

            return indexes;
        }

        //Posunutí indexů priority na základě počtu odebraných elemetnů z předpisu.
        public static void ShiftPosition(CalculationOrder calculationOrder, int removeCount, int index)
        {
            for (int i = 0; i < calculationOrder[0].Count; i++)
            {
                if (calculationOrder[0][index] < calculationOrder[0][i])
                {
                    calculationOrder[0][i] -= removeCount;
                }
            }
        }
    }
}
