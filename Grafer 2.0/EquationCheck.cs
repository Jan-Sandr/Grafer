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
                             !CheckMissing(relation)
                    );
        }

        private static readonly string[] mathOperations = new string[4] { "+", "-", "*", "/" };

        private static bool AreEdgesValid(Relation relation)
        {
            bool areEdgesValid = true;
            if (!double.TryParse(relation[0], out _) && relation[0] != "x")
            {
                areEdgesValid = false;
                relation.FillInvalidSection(0, 1, "Relation begins with invalid character.");
            }

            if (!double.TryParse(relation[^1], out _) && relation[^1] != "x")
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
                if ((double.TryParse(relation[i], out _) || relation[i] == "x") && !mathOperations.Contains(relation[i + 1]))
                {
                    isMissingSomething = true;
                    relation.FillInvalidSection(i, 2, "Relation is missing operation between two characters.");
                    break;
                }
            }

            return isMissingSomething;
        }
    }
}
