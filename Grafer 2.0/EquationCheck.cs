using System.Linq;

namespace Grafer2
{
    public static class EquationCheck
    {
        public static (int SelectionStart, int SelectionLength, int MessageID) InvalidSection { get; private set; } = (0, 0, -1);

        public static bool IsEquationCorrect(string equation)
        {
            InvalidSection = new(0, 0, -1);

            return (
                             AreEdgesValid(equation) &&
                    !AreTwoOperationsInRow(equation) &&
                             CheckBrackets(equation) &&
                             !CheckMissing(equation)
                    );
        }

        private static readonly char[] mathOperations = new char[5] { '+', '-', '*', '/', '^' };

        private static bool AreEdgesValid(string equation)
        {
            bool areEdgesValid = true;
            if (!char.IsDigit(equation[0]) && equation[0] != '-' && equation[0] != 'x' && equation[0] != '(')
            {
                areEdgesValid = false;
                InvalidSection = new(0, 1, 4);
            }

            if (!char.IsDigit(equation[^1]) && equation[^1] != 'x' && equation[^1] != ')')
            {
                areEdgesValid = false;
                InvalidSection = new(equation.Length - 1, 1, 5);
            }

            return areEdgesValid;
        }

        private static bool AreTwoOperationsInRow(string equation)
        {
            bool areTwoOperationsInRow = false;

            for (int i = 0; i < equation.Length - 1; i++)
            {
                if (mathOperations.Contains(equation[i]) && mathOperations.Contains(equation[i + 1]))
                {
                    areTwoOperationsInRow = true;
                    InvalidSection = new(i, 2, 8);
                    break;
                }
            }

            return areTwoOperationsInRow;
        }

        private static bool CheckMissing(string equation)
        {
            for (int i = 0; i < equation.Length - 1; i++)
            {
                if (IsMissingSomething(i, equation[i], equation[i + 1]))
                {
                    break;
                }
            }

            return InvalidSection.MessageID != -1;
        }

        private static bool IsMissingSomething(int index, char left, char right)
        {
            switch (left)
            {
                case '(':
                    {
                        if(mathOperations.Contains(right) && right != '-')
                        {
                            InvalidSection = new(index, 2, 13);
                        }
                        break;
                    }

            }

            if(mathOperations.Contains(left))
            {
                switch(left)
                {
                    case '^':
                        {
                            if(right != '(')
                            {
                                InvalidSection = new(index, 2, 9);
                            }
                            break;
                        }
                    default:
                        {
                            if(right == ')')
                            {
                                InvalidSection = new(index, 2, 14);
                            }
                            break;
                        }
                }
            }

            return InvalidSection.MessageID != -1;
        }

        private static bool CheckBrackets(string equation)
        {
            return AreBracketsCorrect(equation) && !AreBracketsEmpty(equation);
        }

        private static bool AreBracketsCorrect(string equation)
        {
            int countOfBrackets = 0;
            int openingBracketIndex = 0;

            for (int i = 0; i < equation.Length; i++)
            {
                countOfBrackets = equation[i] == '(' ? countOfBrackets + 1 : equation[i] == ')' ? countOfBrackets - 1 : countOfBrackets;

                openingBracketIndex = equation[i] == '(' ? i : openingBracketIndex;

                if (countOfBrackets == -1)
                {
                    InvalidSection = new(i, 1, 10);
                    break;
                }
            }

            if (countOfBrackets > 0)
            {
                InvalidSection = new(openingBracketIndex, 1, 11);
            }

            return countOfBrackets == 0;
        }

        private static bool AreBracketsEmpty(string equation)
        {
            bool containsEmptyBrackets = false;

            for (int i = 0; i < equation.Length - 1; i++)
            {
                if (equation[i] == '(' && equation[i + 1] == ')')
                {
                    containsEmptyBrackets = true;
                    InvalidSection = new(i, 2, 12);
                    break;
                }
            }

            return containsEmptyBrackets;
        }
    }
}
