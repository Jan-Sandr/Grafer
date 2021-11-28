using System.Linq;

namespace Grafer2
{
    public static class EquationCheck
    {
        public static bool BasicCheck(Relation relation)
        {
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
            if (!double.TryParse(relation[0], out _) && relation[0] != "x" && relation[0] != "(")
            {
                areEdgesValid = false;
                relation.FillInvalidSection(0, 1, "Relation begins with invalid character.");
            }

            if (!double.TryParse(relation[^1], out _) && relation[^1] != "x" && relation[^1] != ")")
            {
                areEdgesValid = false;
                relation.FillInvalidSection(relation.Count - 1, 1, "Relation ends with invalid character.");
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
                    relation.FillInvalidSection(i, 2, "Relation cannot contains two operations in row.");
                    break;
                }
            }

            return areTwoOperationsInRow;
        }

        private static bool CheckMissing(Relation relation)
        {
            bool isMissingSomething = false;

            for (int i = 0; i < relation.Count - 1; i++)
            {
                if (IsMissingOperation(relation[i], relation[i + 1]))
                {
                    isMissingSomething = true;
                    relation.FillInvalidSection(i, 2, "Relation is missing operation between two characters.");
                    break;
                }

                if (relation[i] == "^" && relation[i + 1] != "(")
                {
                    isMissingSomething = true;
                    relation.FillInvalidSection(i, 2, "After power brackets are required.");
                    break;
                }
            }

            return isMissingSomething;
        }

        private static bool IsMissingOperation(string left, string right)
        {
            bool isMissingSomething = false;

            if ((double.TryParse(left, out _) || left == "x") && (!mathOperations.Contains(right) && right != ")" && right != "x"))
            {
                isMissingSomething = true;
            }

            if (left == "(" && (!double.TryParse(right, out _) && right != "x"))
            {
                isMissingSomething = true;
            }

            return isMissingSomething;
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
                    relation.FillInvalidSection(i, 1, "Relation can't have closing bracket before opening bracket.");
                    break;
                }
            }

            if (countOfBrackets > 0)
            {
                relation.FillInvalidSection(openingBracketIndex, 1, "Relation can't contain opening bracket without closing one.");
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
                    relation.FillInvalidSection(i, 2, "Ralation can't contain empty brackets.");
                    break;
                }
            }

            return containsEmptyBrackets;
        }
    }
}
