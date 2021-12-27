using Grafer.ExtensionMethods;
using System.Linq;

namespace Grafer
{
    public static class EquationCheck
    {
        public static (int SelectionStart, int SelectionLength, int MessageID) InvalidSection { get; private set; } = (0, 0, -1);

        private static int[] elementsIndex = System.Array.Empty<int>(); // Pole s indexy, kde nejsou prázdná místa v rovnici.

        private readonly static char[] allowedBeginningChars = new char[7] { 'x', '-', '(', '√', 's', 'c', 't' }; // Povolené znaky na začátku předpisu.

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
                                          IsRootIndexValid(equation) &&
                                            AreCommasValid(equation) &&
                                     AreFunctionNamesValid(equation)
                                   );

            return isEquationValid;
        }

        //Náplnění indexů, které nejsou mezery.
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

        //Jestli není index odmocniny 0.
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

        //Jestli čárky v pořádku.
        private static bool AreCommasValid(string equation)
        {
            bool areCommasValid = AreNumbersAroundCommas(equation);
            string number = "";
            int startIndex = 0;
            int endIndex = 0;

            for (int i = 0; i < elementsIndex.Length && areCommasValid; i++)
            {
                //startIndex = elementsIndex[i];
                //number = Build(equation, i, "number");
                //endIndex = elementsIndex[i + number.Length - 1];
                bool stillNumber = false;

                if (char.IsDigit(equation[elementsIndex[i]]) || equation[elementsIndex[i]] == ',')
                {
                    if (number.Length == 0)
                    {
                        startIndex = elementsIndex[i];
                    }

                    number += equation[elementsIndex[i]];
                    endIndex = elementsIndex[i];
                    stillNumber = true;
                }

                if (number.Length > 0 && (!stillNumber || i + 1 == elementsIndex.Length))
                {
                    areCommasValid = AreMultipleCommasInNumber(number, startIndex, endIndex);
                    number = "";
                }
            }

            return areCommasValid;
        }

        //Jestli je více čárek v jednom čísle.
        private static bool AreMultipleCommasInNumber(string number, int startIndex, int endIndex)
        {
            bool isMultipleCommasValid = false;

            if (number.Count(x => x == ',') > 1)
            {
                isMultipleCommasValid = true;
                InvalidSection = (startIndex, endIndex - startIndex + 1, 18);
            }

            return !isMultipleCommasValid;
        }

        //Jestli je čírka obalená číslama.
        private static bool AreNumbersAroundCommas(string equation)
        {
            bool areNumbersAroundCommas = true;

            for (int i = 1; i < elementsIndex.Length - 1; i++)
            {
                if (equation[elementsIndex[i]] == ',')
                {
                    if (!AreNeighboardsNumbers(i, equation[elementsIndex[i - 1]], equation[elementsIndex[i + 1]]))
                    {
                        areNumbersAroundCommas = false;
                        break;
                    }
                }
            }

            return areNumbersAroundCommas;
        }

        //Kontrola zda jsou sousedé čísla.
        private static bool AreNeighboardsNumbers(int index, char left, char right)
        {
            bool areNeighboardsNumbers = true;

            if (!char.IsDigit(left))
            {
                areNeighboardsNumbers = false;
                InvalidSection = (elementsIndex[index - 1], elementsIndex[index] - elementsIndex[index - 1] + 1, 16);
            }
            else if (!char.IsDigit(right))
            {
                areNeighboardsNumbers = false;
                InvalidSection = (elementsIndex[index], elementsIndex[index + 1] - elementsIndex[index] + 1, 17);
            }

            return areNeighboardsNumbers;
        }

        //Jestli jsou názvy trigonometrických funkcí napsány správně.
        private static bool AreFunctionNamesValid(string equation)
        {
            bool areFunctionNamesValid = true;

            string functionName = "";

            int startIndex = 0;
            int endIndex = 0;

            for (int i = 0; i < elementsIndex.Length; i++)
            {
                bool stillBuilding = false;

                if (char.IsLetter(equation[elementsIndex[i]]) && equation[elementsIndex[i]] != 'x')
                {
                    if (functionName.Length == 0)
                    {
                        startIndex = elementsIndex[i];
                    }

                    functionName += equation[elementsIndex[i]];
                    endIndex = elementsIndex[i];
                    stillBuilding = true;
                }

                if (functionName.Length > 0 && (!stillBuilding || i + 1 == elementsIndex.Length))
                {
                    if (!functionName.IsTrigonometricFunction())
                    {
                        areFunctionNamesValid = false;
                        InvalidSection = (startIndex, endIndex - startIndex + 1, 19);
                        break;
                    }

                    areFunctionNamesValid = IsTrigonometricFunctionsSyntaxValid(equation, i - 1);

                    functionName = "";
                }
            }

            return areFunctionNamesValid;
        }

        //Jestli je syntax po trigonometrických funkcích správná
        private static bool IsTrigonometricFunctionsSyntaxValid(string equation, int index)
        {
            bool isTrigonometricFunctionsSyntaxValid = true;

            //Jestli je  po trigonometrické funkci závorka. Pokud je tam ale horní index, tak to se řeší níže.
            if (equation[elementsIndex[index + 1]] != '(' && equation[elementsIndex[index + 1]] != '⁻' && equation[elementsIndex[index + 1]] != '¹')
            {
                isTrigonometricFunctionsSyntaxValid = false;

                //Sin x oznáčí od mezery po x.
                InvalidSection = (elementsIndex[index] + 1, elementsIndex[index + 1] - elementsIndex[index] + 1, 20);
            }

            //Kontrola syntaxe arcusů.
            if (isTrigonometricFunctionsSyntaxValid)
            {
                int addtionSelection = 0;

                if (equation[elementsIndex[index + 1]] == '⁻')
                {
                    if (equation[elementsIndex[index + 2]] == '¹')
                    {
                        if (equation[elementsIndex[index + 3]] == '(')
                        {
                            isTrigonometricFunctionsSyntaxValid = true;
                        }
                        else
                        {
                            //Sin ⁻¹ x označí od mezery po sinu do x.
                            isTrigonometricFunctionsSyntaxValid = false;
                            addtionSelection = 2;
                        }
                    }
                    else
                    {
                        //Sin ⁻ x označí od mezery po sinu do x.
                        isTrigonometricFunctionsSyntaxValid = false;
                        addtionSelection = 1;
                    }
                }

                if (equation[elementsIndex[index + 1]] == '¹')
                {
                    //Sin ¹ x označí od mezery po sinu do x.
                    isTrigonometricFunctionsSyntaxValid = false;
                    addtionSelection = 1;
                }

                if (!isTrigonometricFunctionsSyntaxValid)
                {
                    //Označení na základě počtu chybějících prvků.
                    InvalidSection = (elementsIndex[index] + 1, elementsIndex[index + 1 + addtionSelection] - elementsIndex[index], 21);
                }
            }

            return isTrigonometricFunctionsSyntaxValid;
        }

        //private static string Build(string equation, int index, string target)
        //{
        //    string item = "";

        //    for (int i = index; i < elementsIndex.Length; i++)
        //    {
        //        bool stillBuilding = false;

        //        if(target == "number")
        //        {
        //            if (char.IsDigit(equation[elementsIndex[i]]) || equation[elementsIndex[i]] == ',')
        //            {
        //                item += equation[elementsIndex[i]];
        //                stillBuilding = true;
        //            }
        //        }

        //        if(!stillBuilding)
        //        {
        //            break;
        //        }

        //    }

        //    return item;
        //}
    }
}

