using Grafer.ExtensionMethods;
using System.Linq;

namespace Grafer
{
    public static class EquationCheck
    {
        public static (int SelectionStart, int SelectionLength, int MessageID) InvalidSection { get; private set; } = (0, 0, -1);

        private static int[] elementsIndex = System.Array.Empty<int>();

        private readonly static char[] allowedBeginningChars = new char[4] { 'x', '-', '(', '√' }; // Povolené znaky na začátku předpisu.

        private readonly static char[] allowedEndeningChars = new char[2] { 'x', ')' }; // Povolené znaky na konci předpisu.

        //Kontrola rovnice
        public static bool IsEquationValid(string equation)
        {
            InvalidSection = (0, 0, -1);

            elementsIndex = FillElementsIndex(equation);

            bool isEquationValid = (
                                             AreEdgesValid(equation) &&
                                    !AreTwoOperationsInRow(equation) &&
                                             CheckBrackets(equation) &&
                                             !CheckMissing(equation) &&
                                          IsRootIndexValid(equation)
                                    );

            return isEquationValid;
        }

        private static int[] FillElementsIndex(string equation)
        {
            int[] charactersIndex = new int[equation.Replace(" ", "").Length];

            int index = 0;

            for (int i = 0; i < equation.Length; i++)
            {
                if (equation[i] != 32)
                {
                    charactersIndex[index] = i;
                    index++;
                }
            }

            return charactersIndex;
        }

        //Zda předpis začíná a končí validním znakem.
        private static bool AreEdgesValid(string equation)
        {
            bool areEdgesValid = true;

            if (!char.IsDigit(equation[elementsIndex[0]]) && !allowedBeginningChars.Contains(equation[elementsIndex[0]]))
            {
                areEdgesValid = false;
                InvalidSection = (elementsIndex[0], 1, 4);
            }

            if (!char.IsDigit(equation[elementsIndex[^1]]) && !allowedEndeningChars.Contains(equation[elementsIndex[^1]]))
            {
                areEdgesValid = false;
                InvalidSection = (elementsIndex[^1], 1, 5);
            }

            return areEdgesValid;
        }

        //Jestli jsou 2 operace za sebou.
        private static bool AreTwoOperationsInRow(string equation)
        {
            bool areTwoOperationsInRow = false;

            for (int i = 0; i < elementsIndex.Length; i++)
            {
                if (equation[elementsIndex[i]].IsMathOperation(0, 5) && equation[elementsIndex[i + 1]].IsMathOperation(0, 5))
                {
                    areTwoOperationsInRow = true;
                    InvalidSection = (elementsIndex[i], elementsIndex[i + 1] - elementsIndex[i] + 1, 8);
                    break;
                }
            }

            return areTwoOperationsInRow;
        }

        //Jestli něco chybí mezi členy.
        private static bool CheckMissing(string equation)
        {
            for (int i = 0; i < elementsIndex.Length - 1; i++)
            {
                if (IsMissingSomething(i, equation[elementsIndex[i]], equation[elementsIndex[i + 1]]))
                {
                    break;
                }
            }

            return InvalidSection.MessageID != -1;
        }

        //Tabulka pro detekci chybějících elementů.
        private static bool IsMissingSomething(int index, char left, char right)
        {
            int leftIndex = elementsIndex[index]; // Pozice levého elementu.
            int compareLength = elementsIndex[index + 1] - elementsIndex[index] + 1; // Rozpětí poronávacích elementů.

            switch (left)
            {
                case '(':
                    {
                        if (right.IsMathOperation() && right != '-' && right != '√')
                        {
                            InvalidSection = (leftIndex, compareLength, 13);
                        }
                        break;
                    }

            }

            if (left.IsMathOperation())
            {
                switch (left)
                {
                    case '^':
                        {
                            if (right != '(')
                            {
                                InvalidSection = (leftIndex, compareLength, 9);
                            }
                            break;
                        }
                    case '√':
                        {
                            if (right != '(')
                            {
                                InvalidSection = (leftIndex, compareLength, 9);
                            }
                            break;
                        }
                    default:
                        {
                            if (right == ')')
                            {
                                InvalidSection = (leftIndex, compareLength, 14);
                            }
                            break;
                        }
                }
            }

            return InvalidSection.MessageID != -1;
        }

        //Kontrola závorek.
        private static bool CheckBrackets(string equation)
        {
            return AreBracketsCorrect(equation) && !AreBracketsEmpty(equation);
        }

        //Jestli nejsou případy třeba nejdříve koncová závorka, nebo jejich počet si není roven.
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
                    InvalidSection = (i, 1, 10);
                    break;
                }
            }

            if (countOfBrackets > 0)
            {
                InvalidSection = (openingBracketIndex, 1, 11);
            }

            return countOfBrackets == 0;
        }

        //Jestli se v předpisu náchází prázdné závorky.
        private static bool AreBracketsEmpty(string equation)
        {
            bool containsEmptyBrackets = false;

            for (int i = 0; i < elementsIndex.Length - 1; i++)
            {
                if (equation[elementsIndex[i]] == '(' && equation[elementsIndex[i + 1]] == ')')
                {
                    containsEmptyBrackets = true;
                    InvalidSection = (elementsIndex[i], elementsIndex[i + 1] - elementsIndex[i] + 1, 12);
                    break;
                }
            }

            return containsEmptyBrackets;
        }

        private static bool IsRootIndexValid(string equation)
        {
            bool isRootIndexValid = true;

            for (int i = 1; i < elementsIndex.Length; i++)
            {
                if (equation[elementsIndex[i]] == '√' && equation[elementsIndex[i - 1]] == '0')
                {
                    isRootIndexValid = false;
                    InvalidSection = (elementsIndex[i - 1], elementsIndex[i] - elementsIndex[i - 1], 15);
                    break;
                }
            }

            return isRootIndexValid;
        }
    }
}

