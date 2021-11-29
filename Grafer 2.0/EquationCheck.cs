using System.Linq;

namespace Grafer2
{
    public static class EquationCheck
    {
        public static (int SelectionStart, int SelectionLength, int MessageID) InvalidSection { get; private set; } = (0, 0, -1);

        public static bool IsInputCorrect(Relation relation)
        {
            InvalidSection = new(0, 0, -1);

            return (
                             AreEdgesValid(relation) &&
                    !AreTwoOperationsInRow(relation) &&
                             CheckBrackets(relation) &&
                             !CheckMissing(relation)
                    );
        }

        private static readonly string[] mathOperations = new string[5] { "+", "-", "*", "/", "^" };

        private static bool AreEdgesValid(Relation relation)
        {
            bool areEdgesValid = true;
            if (!double.TryParse(relation[0], out _) && relation[0] != "-" && relation[0] != "x" && relation[0] != "(")
            {
                areEdgesValid = false;
                InvalidSection = new(0, 1, 1);
            }

            if (!double.TryParse(relation[^1], out _) && relation[^1] != "x" && relation[^1] != ")")
            {
                areEdgesValid = false;
                InvalidSection = new(relation.Count - 1, 1, 2);
            }

            return areEdgesValid;
        }

        private static bool AreTwoOperationsInRow(Relation relation)
        {
            bool areTwoOperationsInRow = false;

            for (int i = 0; i < relation.Count - 1; i++)
            {
                if (mathOperations.Contains(relation[i]) && mathOperations.Contains(relation[i + 1]))
                {
                    areTwoOperationsInRow = true;
                    InvalidSection = new(i, 2, 5);
                    break;
                }
            }

            return areTwoOperationsInRow;
        }

        private static bool CheckMissing(Relation relation)
        {
            for (int i = 0; i < relation.Count - 1; i++)
            {
                if (IsMissingSomething(i, relation[i], relation[i + 1]))
                {
                    break;
                }
            }

            return InvalidSection.MessageID != -1;
        }

        private static bool IsMissingSomething(int index, string left, string right)
        {
            switch (left)
            {
                case "(":
                    {
                        if(mathOperations.Contains(right) && right != "-")
                        {
                            InvalidSection = new(index, 2, 10);
                        }
                        break;
                    }

            }

            if(mathOperations.Contains(left))
            {
                switch(left)
                {
                    case "^":
                        {
                            if(right != "(")
                            {
                                InvalidSection = new(index, 2, 6);
                            }
                            break;
                        }
                    default:
                        {
                            if(right == ")")
                            {
                                InvalidSection = new(index, 2, 11);
                            }
                            break;
                        }
                }
            }

            return InvalidSection.MessageID != -1;
        }

        private static bool CheckBrackets(Relation relation)
        {
            return AreBracketsCorrect(relation) && !AreBracketsEmpty(relation);
        }

        private static bool AreBracketsCorrect(Relation relation)
        {
            int countOfBrackets = 0;
            int openingBracketIndex = 0;

            for (int i = 0; i < relation.Count; i++)
            {
                countOfBrackets = relation[i] == "(" ? countOfBrackets + 1 : relation[i] == ")" ? countOfBrackets - 1 : countOfBrackets;

                openingBracketIndex = relation[i] == "(" ? i : openingBracketIndex;

                if (countOfBrackets == -1)
                {
                    InvalidSection = new(i, 1, 7);
                    break;
                }
            }

            if (countOfBrackets > 0)
            {
                InvalidSection = new(openingBracketIndex, 1, 8);
            }

            return countOfBrackets == 0;
        }

        private static bool AreBracketsEmpty(Relation relation)
        {
            bool containsEmptyBrackets = false;

            for (int i = 0; i < relation.Count - 1; i++)
            {
                if (relation[i] == "(" && relation[i + 1] == ")")
                {
                    containsEmptyBrackets = true;
                    InvalidSection = new(i, 2, 9);
                    break;
                }
            }

            return containsEmptyBrackets;
        }
    }
}
