using System.Linq;

namespace Grafer.ExtensionMethods
{
    public static class CharExtensionMethods
    {
        private static readonly char[] mathOperations = new char[6] { '+', '-', '*', '/', '^', '√' };

        //Jestli se jedná o matematickou operaci.
        public static bool IsMathOperation(this char input)
        {
            return mathOperations.Contains(input);
        }

        //Jestli se jedná o matematickou operaci s rozashem pro porovnání.
        public static bool IsMathOperation(this char input, int startIndex, int length)
        {
            bool isMathOperation = false;

            for (int i = startIndex; i < length; i++)
            {
                if (mathOperations[i] == input)
                {
                    isMathOperation = true;
                }
            }

            return isMathOperation;
        }
    }
}
