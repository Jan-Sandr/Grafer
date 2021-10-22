using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grafer2
{
    public static class EquationCheck
    {
        public static bool BasicCheck(Relation relation)
        {
            bool isRelationValid = AreEdgesValid(relation);
            return isRelationValid;
        }

        private static bool AreEdgesValid(Relation relation)
        {
            bool areEdgesValid = true;
            if (!char.IsDigit(char.Parse(relation[0])) && relation[0] != "x")
            {
                areEdgesValid = false;
            }

            if (!char.IsDigit(char.Parse(relation[relation.Count-1])) && relation[relation.Count-1] != "x")
            {
                areEdgesValid = false;
            }

            return areEdgesValid;
        }
    }
}
