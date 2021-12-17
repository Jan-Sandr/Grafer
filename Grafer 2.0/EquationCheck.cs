using System.Linq;

namespace Grafer2
{
    public static class EquationCheck
    {
        public static (int SelectionStart, int SelectionLength, int MessageID) InvalidSection { get; private set; } = (0, 0, -1);

        private static int[] elementsIndex = System.Array.Empty<int>();

        //Kontrola rovnice
        public static bool IsEquationValid(string equation)
        {
            InvalidSection = (0, 0, -1);

            elementsIndex = FillElementsIndex(equation);

            bool isEquationValid = (
                                             AreEdgesValid(equation) &&
                                    !AreTwoOperationsInRow(equation) &&
                                             CheckBrackets(equation) &&
                                             !CheckMissing(equation)
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

        private static readonly char[] mathOperations = new char[5] { '+', '-', '*', '/', '^' };

        //Zda předpis začíná a končí validním znakem.
        private static bool AreEdgesValid(string equation)
        {
            bool areEdgesValid = true;

            if (!char.IsDigit(equation[elementsIndex[0]]) && equation[elementsIndex[0]] != '-' && equation[elementsIndex[0]] != 'x' && equation[elementsIndex[0]] != '(')
            {
                areEdgesValid = false;
                InvalidSection = (elementsIndex[0], 1, 4);
            }

            if (!char.IsDigit(equation[elementsIndex[^1]]) && equation[elementsIndex[^1]] != 'x' && equation[elementsIndex[^1]] != ')')
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
                if (mathOperations.Contains(equation[elementsIndex[i]]) && mathOperations.Contains(equation[elementsIndex[i + 1]]))
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
                        if (mathOperations.Contains(right) && right != '-')
                        {
                            InvalidSection = (leftIndex, compareLength, 13);
                        }
                        break;
                    }

            }

            if (mathOperations.Contains(left))
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
    }
}

