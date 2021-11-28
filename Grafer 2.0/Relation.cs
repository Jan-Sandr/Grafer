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

            if (relation.Count > 1)
            {
                relation = InsertMultiplication(relation);

                relation = ConnectNumbers(relation);

                relation = InsertZero(relation);
            }

            IsRelationValid = EquationCheck.BasicCheck(relation);

            relation = RemoveUnnecessaryBrackets(relation);

            return relation;
        }

        public void FillInvalidSection(int selectionStart, int selectionLength, string message)
        {
            InvalidSection = new(selectionStart, selectionLength, message);
        }

        private Relation RemoveUnnecessaryBrackets(Relation relation)
        {
            for (int i = 1; i < relation.Count - 1; i++)
            {
                CheckNeighbors(relation, i);
            }

            return relation;
        }

        private static Relation InsertZero(Relation relation)
        {
            if (relation[0] == "-")
            {
                relation.Insert(0, "0");
            }

            for (int i = 1; i < relation.Count; i++)
            {
                if (relation[i] == "-" && relation[i - 1] == "(")
                {
                    relation.Insert(i, "0");
                    i++;
                }
            }

            return relation;
        }

        private static Relation InsertMultiplication(Relation relation)
        {
            for (int i = 1; i < relation.Count; i++)
            {
                if (CanInsertMultiplication(relation[i - 1], relation[i]))
                {
                    relation.Insert(i, "*");
                }
            }

            return relation;
        }

        private static bool CanInsertMultiplication(string left, string right)
        {
            return (
                        (char.IsLetter(char.Parse(left)) && char.IsLetter(char.Parse(right))) ||
                        (char.IsDigit(char.Parse(left)) && char.IsLetter(char.Parse(right))) ||
                        (char.IsLetter(char.Parse(left)) && char.IsDigit(char.Parse(right))) ||
                        (char.IsLetterOrDigit(char.Parse(left)) && right == "(") ||
                        (left == ")" && char.IsLetterOrDigit(char.Parse(right))) ||
                        (left == ")" && right == "(")
                   );
        }

        private static Relation ConnectNumbers(Relation relation)
        {
            for (int i = 1; i < relation.Count; i++)
            {
                if (char.IsDigit(char.Parse(relation[i])) && char.IsDigit(char.Parse(relation[i - 1])))
                {
                    relation[i - 1] += relation[i];
                    relation.RemoveAt(i);
                    i--;
                }
            }

            return relation;
        }

        public void RemoveNeighbors(Relation relation, int index)
        {
            relation.RemoveAt(index + 1);
            relation.RemoveAt(index - 1);

            index--;

            RemovedElementsCount += 2;

            CheckNeighbors(relation, index);
        }

        private void CheckNeighbors(Relation relation, int index)
        {
            if (index != 0 && index < relation.Count - 1)
            {
                if (relation[index - 1] == "(" && relation[index + 1] == ")")
                {
                    RemoveNeighbors(relation, index);
                }
            }
        }
    }
}
