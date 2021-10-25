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

            if (!char.IsDigit(char.Parse(relation[^1])) && relation[^1] != "x")
            {
                areEdgesValid = false;
            }

            return areEdgesValid;
        }
    }
}
