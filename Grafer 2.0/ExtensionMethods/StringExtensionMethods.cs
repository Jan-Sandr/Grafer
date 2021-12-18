namespace Grafer.ExtensionMethods
{
    public static class StringExtensionMethods
    {
        public static bool IsMathOperation(this string input)
        {
            bool isMathOperation = false;

            if (input.Length == 1)
            {
                isMathOperation = char.Parse(input).IsMathOperation();
            }

            return isMathOperation;
        }
    }
}
