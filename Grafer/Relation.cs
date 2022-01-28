using Grafer.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grafer
{
    public class Relation : List<string>
    {
        public int RemovedElementsCount { get; set; }

        //Prázdný konstruktor.
        public Relation()
        {

        }

        //Naplnění předpisu.
        public Relation(string input)
        {
            AddRange(input.Select(s => s.ToString()));
            Adjust();
        }

        //Úprava předpisu pro výpočet.
        private void Adjust()
        {
            RemoveAll(s => s == " ");

            SubstitutePi();

            if (Count > 1)
            {
                Union();

                AdjustAbsoluteValue();

                ReplaceNaturalLogarithm();

                EncapsulateLogarithmBase();

                Insertions();

                ConvertDegrees();

                RemoveUnnecessaryBrackets();

                InsertIntermediary();
            }
        }

        //Spojování do schlívečků.
        private void Union()
        {
            UniteLetters();

            UniteUpperIndex();

            UniteNumbers();
        }

        //Spojí písmena do jednoho políčka krom x: s i n -> sin.
        private void UniteLetters()
        {
            for (int i = 0; i < Count - 1; i++)
            {
                if (this[i].Any(char.IsLetter) && this[i] != "x" && this[i] != "π" && char.IsLetter(char.Parse(this[i + 1])) && this[i + 1] != "x" && this[i + 1] != "π")
                {
                    this[i] += this[i + 1];
                    RemoveAt(i + 1);
                    i--;
                }
            }
        }

        //Spojí ⁻ a ¹ do jednoho chlívečku.
        private void UniteUpperIndex()
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i] == "⁻" && this[i + 1] == "¹")
                {
                    this[i] += this[i + 1];
                    RemoveAt(i + 1);
                    i--;
                }
            }
        }

        //Dosazení za Pi
        private void SubstitutePi()
        {
            for (int i = 0; i < Count; i++)
            {
                this[i] = (this[i] == "π") ? Math.PI.ToString() : this[i];
            }
        }

        //Vkládání znaků dle potřeby pro standard.
        private void Insertions()
        {
            InsertAtBeginning();

            InsertMultiplication();

            InsertRootIndex();

            InsertZero();

            InsertLogarithmBase();
        }

        //Vloží dekadický základ logaritmu log(x) -> log[10](x)
        private void InsertLogarithmBase()
        {
            for (int i = 0; i < Count - 1; i++)
            {
                if (this[i] == "log" && this[i + 1] == "(")
                {
                    Insert(i + 1, "10");
                    i++;
                }
            }
        }

        //Odebrání zbytečných závorek x + (2).
        private void RemoveUnnecessaryBrackets()
        {
            for (int i = 1; i < Count - 1; i++)
            {
                CheckNeighbors(i);
            }

            for (int i = 0; i < Count; i++)
            {
                if (this[i] == "[" || this[i] == "]")
                {
                    RemoveAt(i);
                    i--;
                }
            }
        }

        //Případy pro pomocné vložení na začátek předpisu.
        private void InsertAtBeginning()
        {
            if (this[0] == "-")
            {
                Insert(0, "0");
            }

            if (this[0] == "√")
            {
                Insert(0, "2");
            }
        }

        //Vložení nulu. Když předpis začíná minusem a nebo (-.
        private void InsertZero()
        {
            for (int i = 1; i < Count - 1; i++)
            {
                if (this[i] == "-" && this[i - 1] == "(")
                {
                    Insert(i, "0");
                    i++;
                }

                if (this[i - 1] == "|" && this[i] != ")")
                {
                    // |x| -> |0+x|
                    if (!this[i].IsMathOperation())
                    {
                        Insert(i, "0");
                        Insert(i + 1, "+");
                        i += 2;
                    }

                    // |-2+x| -> |0-2+x|, ale zároveň, aby se nestalo |x+2|-2 -> |x+2|0-2
                    if (this[i] == "-")
                    {
                        Insert(i, "0");
                    }
                }
            }
        }

        //Vložení znaku  pro násobení.
        private void InsertMultiplication()
        {
            for (int i = 1; i < Count; i++)
            {
                if (!this[i - 1].IsTrigonometricFunctionOrLogarithm() && CanInsertMultiplication(this[i - 1], this[i]))
                {
                    Insert(i, "*");
                }
            }
        }

        //Tabulka pro vložení znaku pro násobení.
        private static bool CanInsertMultiplication(string left, string right)
        {
            return (
                        (left.IsOnly(char.IsLetter) && right.IsOnly(char.IsLetter)) || // x     x
                        (left.IsOnly(char.IsDigit) && right.IsOnly(char.IsLetter)) || // 2     x
                        (left.IsOnly(char.IsLetter) && right.IsOnly(char.IsDigit)) || // x     2
                        (left.IsOnly(char.IsLetterOrDigit) && right == "(") || // [x,2] (
                        (left == ")" && right.IsOnly(char.IsLetterOrDigit)) || // )     [x,2]      
                        (left == ")" && right == "(")                                  // )     (
                   );
        }

        //Spojení čísel do políčka třeba 10 je v základu jako 1 a 0. 
        private void UniteNumbers()
        {
            for (int i = 1; i < Count; i++)
            {
                if ((double.TryParse(this[i], out _) || this[i] == ",") && double.TryParse(this[i - 1], out _))
                {
                    this[i - 1] += this[i];
                    RemoveAt(i);
                    i--;
                }
            }
        }

        //Převede stupně na číselnou hodnotu.
        private void ConvertDegrees()
        {
            for (int i = 1; i < Count; i++)
            {
                if (this[i] == "°")
                {
                    this[i - 1] = double.Parse(this[i - 1]).ToNumerical().ToString();
                    RemoveAt(i);
                    i--;
                }
            }
        }

        //Pokud uživatel napsal jenom odmocninu převede to na 2. odmocninu, protože ta se dá napsat bez indexu.
        private void InsertRootIndex()
        {
            for (int i = 1; i < Count; i++)
            {
                if (this[i] == "√" && (this[i - 1].IsMathOperation() || this[i - 1] == "(" || this[i - 1] == "|"))
                {
                    Insert(i, "2");
                    i++;
                }
            }
        }

        //Obalí absolutní hodnot závorkami, aby bylo možné rozlišít počáteční a koncovou.
        private void AdjustAbsoluteValue()
        {
            int openingsCount = 0;

            for (int i = 0; i < Count; i++)
            {
                //obaluje absolutní hodnotu závorkami
                if (this[i] == "|")
                {
                    WrapAbsoluteValue(i, ref openingsCount);
                    i++;
                }
            }
        }

        //Přidá závorku pokud je nutná.
        private void WrapAbsoluteValue(int index, ref int openingsCount)
        {
            bool isWrapped = false;

            // (|
            if (index > 0 && this[index - 1] == "(")
            {
                openingsCount++;
                isWrapped = true;
            }

            // |)
            if (index < Count - 1 && this[index + 1] == ")")
            {
                openingsCount--;
                isWrapped = true;
            }

            if (!isWrapped)
            {
                if (openingsCount == 0)
                {
                    Insert(index, "(");
                    openingsCount++;
                }
                else
                {
                    Insert(index + 1, ")");
                    openingsCount--;
                }
            }
        }


        //Vloží mezi sin,4 -> sin,p,4. Aby se jednolo o 3 členy a šlo tak odebírat sousedy. Jako tomu je u opreací 1 + 2, také jsou tři.
        private void InsertIntermediary()
        {
            for (int i = 0; i < Count - 1; i++)
            {
                if (this[i].IsTrigonometricFunction() && this[i + 1] != "⁻¹")
                {
                    Insert(i + 1, "p");
                }
            }
        }

        //Odebrání sousedů elementu.
        public void RemoveNeighbors(int index)
        {
            RemoveAt(index + 1);
            RemoveAt(index - 1);

            index--;

            RemovedElementsCount += 2;

            CheckNeighbors(index);
        }

        //Převede obsolutní hodnota na závorky a číslo uvnitř převede na kladné.
        private void AbsoluteValueToBrackets(int index)
        {
            if (this[index - 1] == "|" && this[index + 1] == "|")
            {
                if (double.TryParse(this[index], out _))
                {
                    this[index] = Math.Abs(double.Parse(this[index])).ToString();
                    this[index - 1] = "(";
                    this[index + 1] = ")";
                }
            }
        }

        //Zda jsou sousedé závorky.
        private void CheckNeighbors(int index)
        {
            if (index != 0 && index < Count - 1)
            {
                AbsoluteValueToBrackets(index);

                if (this[index - 1] == "(" && this[index + 1] == ")")
                {
                    RemoveNeighbors(index);
                }
            }
        }

        //Převede ln(x) -> log[ℇ](x)
        private void ReplaceNaturalLogarithm()
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i] == "ln")
                {
                    this[i] = "log";
                    Insert(i + 1, "]");
                    Insert(i + 1, Math.E.ToString());
                    Insert(i + 1, "[");
                    i += 3;
                }
            }
        }

        //Obaluje obsah logaritmu log[x+2-1/x](x) -> log[(x+2-1/x)](x)
        private void EncapsulateLogarithmBase()
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i] == "[")
                {
                    Insert(i + 1, "(");
                    i++;
                }

                if (this[i] == "]")
                {
                    Insert(i, ")");
                    i++;
                }
            }
        }
    }
}
