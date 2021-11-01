using System.Linq;

namespace Grafer2
{
    public static class EquationCheck
    {
        public static bool BasicCheck(Relation relation)
        {
            bool isRelationValid = true;

            isRelationValid &= AreEdgesValid(relation);

            
            isRelationValid &= !AreTwoOperationsInRow(relation);
           

            isRelationValid &= !CheckMissing(relation);
            
            return isRelationValid;
        }

        private static readonly string[] mathOperations = new string[4] { "+", "-", "*", "/" };

        private static bool AreEdgesValid(Relation relation)
        {
            bool areEdgesValid = true;
            if (!char.IsDigit(char.Parse(relation[0])) && relation[0] != "x")
            {
                areEdgesValid = false;
            }

            if (!char.IsDigit(char.Parse(relation[^1])) && relation[^1] != "x")
            {
                areEdgesValid = false;
            }

            return areEdgesValid;
        }

        private static bool AreTwoOperationsInRow(Relation relation)
        {
            bool areTwoOperationsInRow = false;

            for (int i = 0; i < relation.Count-1; i++)
            {
                if(mathOperations.Contains(relation[i]) && mathOperations.Contains(relation[i+1]))
                {
                    areTwoOperationsInRow = true;
                    break;
                }
            }

            return areTwoOperationsInRow;
        }

        private static bool CheckMissing(Relation relation)
        {
            bool isMissingSomething = false;

            for(int i =0; i < relation.Count-1; i++)
            {
                if((char.IsDigit(char.Parse(relation[i])) || relation[i] =="x") && !mathOperations.Contains(relation[i+1]))
                {
                    isMissingSomething = true;
                    break;
                }
            }

            return isMissingSomething;
        }
    }
}
