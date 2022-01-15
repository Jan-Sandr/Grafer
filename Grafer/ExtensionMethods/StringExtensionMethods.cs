using System;
using System.Collections.Generic;
using System.Linq;

namespace Grafer.ExtensionMethods
{
    public static class StringExtensionMethods
    {
        private static readonly string[] trigonometricFunctions = new string[4] { "sin", "cos", "tg", "cotg" };

        //Jestli se jedná o math operaci.
        public static bool IsMathOperation(this string input)
        {
            bool isMathOperation = false;

            if (input.Length == 1)
            {
                isMathOperation = char.Parse(input).IsMathOperation();
            }

            return isMathOperation;
        }

        //Jestli se jedná o trigonometrickou funkci.
        public static bool IsTrigonometricFunction(this string input)
        {
            bool isTrigonometricFunction = false;

            if (input.Length > 1)
            {
                isTrigonometricFunction = trigonometricFunctions.Contains(input);
            }

            return isTrigonometricFunction;
        }

        //Jestli je string pouze něco.
        public static bool IsOnly(this string input, Func<char, bool> func)
        {
            //Pokud se ptám jestli to je jenom číslo nesmí tam být čárka.
            if (func == char.IsDigit || func == char.IsLetterOrDigit)
            {
                input = input.Replace(",", "");
            }

            return input.All(func);
        }

        //Převod stringobáho pole do slovníku. Přičemz se musí jedna o csv soubor.
        public static Dictionary<string, string> ToDictionary(this string[] input)
        {
            Dictionary<string, string> collection = new Dictionary<string, string>();

            for (int i = 0; i < input.Length; i++)
            {
                int firstSeparatorIndex = input[i].IndexOf(';');
                string key = input[i][..firstSeparatorIndex];
                string value = input[i].Substring(firstSeparatorIndex + 1, input[i].Length - firstSeparatorIndex - 2);
                collection.Add(key, value);
            }

            return collection;
        }
    }
}
