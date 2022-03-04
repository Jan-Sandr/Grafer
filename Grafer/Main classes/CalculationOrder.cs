using System;

namespace Grafer
{
    public class CalculationOrder
    {
        private readonly string[] mathCharacters = new string[] { "+-", "*/", "^√", "sin cos tg cotg log", "()" };

        public int[] Indexes { get; private set; } = Array.Empty<int>();
        public int[] Priorities { get; private set; } = Array.Empty<int>();

        public CalculationOrder()
        {

        }

        //Získání výpočetního postupu na základě priorit operací a závorek.
        public void Create(Relation relation)
        {
            SetOrderLength(relation);

            int additionalPriority = 0;  // pro závorku
            int index = 0;

            for (int i = 0; i < relation.Count; i++)
            {
                int priority = Array.FindIndex(mathCharacters, s => s.Contains(relation[i]));

                if (priority == 4) // případ závorky
                {
                    additionalPriority = relation[i] == "(" ? additionalPriority + 4 : additionalPriority - 4;
                }
                else if (priority != -1)
                {
                    Indexes[index] = i;
                    Priorities[index] = priority + additionalPriority;
                    index++;
                }
            }

            SortOrder();
        }

        //Nastaví délku indexů a priority
        private void SetOrderLength(Relation relation)
        {
            int length = GetCountOfOperations(relation);

            Indexes = new int[length];
            Priorities = new int[length];
        }

        //Získá délká pro indexy a priority
        private int GetCountOfOperations(Relation relation)
        {
            int countOfOperations = 0;

            for (int i = 0; i < relation.Count; i++)
            {
                int index = Array.FindIndex(mathCharacters, 0, 4, s => s.Contains(relation[i]));

                if (index != -1)
                {
                    countOfOperations++;
                }
            }

            return countOfOperations;
        }

        //Seřazení priority na základě jejich výše se zachováním pozice v předpisu.
        private void SortOrder()
        {
            Array.Sort(Priorities, Indexes);
            Array.Reverse(Priorities);
            Array.Reverse(Indexes);

            Indexes = SortIndexes();
        }

        //Když je více operací se stejnou prioritou za sebou, tak aby postup výpočtu šel z leva.
        private int[] SortIndexes()
        {
            int sameElementsCount = 1;
            for (int i = 1; i < Indexes.Length; i++)
            {
                if (Priorities[i] == Priorities[i - 1])
                {
                    sameElementsCount++;
                }
                else
                {
                    if (sameElementsCount > 1)
                    {
                        if (Priorities[i - 1] != 2) // Výjimka pro mocninua a odmocninu u těch se jde nejdříve zprava. 
                        {
                            Array.Sort(Indexes, i - sameElementsCount, sameElementsCount);
                        }
                    }

                    sameElementsCount = 1;
                }

                if (i == Indexes.Length - 1)
                {
                    if (Priorities[i] != 2)
                    {
                        Array.Sort(Indexes, (i + 1) - sameElementsCount, sameElementsCount);
                    }
                }

            }

            return Indexes;
        }

        //Posunutí indexů priority na základě počtu odebraných elemetnů z předpisu.
        public void ShiftPosition(int removeCount, int index)
        {
            for (int i = 0; i < Indexes.Length; i++)
            {
                if (Indexes[index] < Indexes[i])
                {
                    Indexes[i] -= removeCount;
                }
            }
        }
    }
}
