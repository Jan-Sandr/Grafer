using System.Collections.Generic;

namespace Grafer2
{
    public class Relation : List<string>
    {
        public int RemovedElementsCount { get; set; }

        public bool IsRelationValid { get; private set; }

        public (int SelectionStart, int SelectionLength, string Message) InvalidSection { get; private set; }

        public Relation()
        {
            RemovedElementsCount = 0;
        }

        public Relation Adjust(Relation relation)
        {          
            relation.RemoveAll(s => s == " ");
            
            relation = InsertZero(relation);

            relation = InsertMultiplication(relation);

            relation = ConnectNumbers(relation);

            IsRelationValid = EquationCheck.BasicCheck(relation);

            return relation;
        }

        public void FillInvalidSection(int selectionStart,int selectionLength, string message)
        {
            InvalidSection = new(selectionStart, selectionLength, message);
        }

        private static Relation InsertZero(Relation relation)
        {
            if (relation[0] == "-")
            {
                relation.Insert(0, "0");
            }
            return relation;
        }

        private static Relation InsertMultiplication(Relation relation)
        {
            for (int i = 1; i < relation.Count; i++)
            {
                if(relation[i] == "x" && char.IsLetterOrDigit(char.Parse(relation[i-1])))
                {
                    relation.Insert(i, "*");
                }
            }

            return relation;
        }

        private static Relation ConnectNumbers(Relation relation)
        {
            for (int i = 1; i < relation.Count; i++)
            {
                if(char.IsDigit(char.Parse(relation[i])) && char.IsDigit(char.Parse(relation[i-1])))
                {
                    relation[i - 1] += relation[i];
                    relation.RemoveAt(i);
                }
            }

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
