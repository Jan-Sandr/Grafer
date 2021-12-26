using System;
using System.Linq;

namespace Grafer.ExtensionMethods
{
    public static class StringExtensionMethods
    {
        private static readonly string[] trigonometricFunctions = new string[4] { "sin", "cos", "tan", "cotg" };

        public static bool IsMathOperation(this string input)
        {
            bool isMathOperation = false;

            if (input.Length == 1)
            {
                isMathOperation = char.Parse(input).IsMathOperation();
            }

            return isMathOperation;
        }

        public static bool IsTrigonometricFunction(this string input)
        {
            bool isTrigonometricFunction = false;

            if (input.Length > 2)
            {
                isTrigonometricFunction = trigonometricFunctions.Contains(input);
            }

            return isTrigonometricFunction;
        }

        public static bool IsOnly(this string input, Func<char, bool> func)
        {
            return input.All(func);
        }
    }
}
