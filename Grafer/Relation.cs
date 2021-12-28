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

            if (Count > 1)
            {
                Union();

                Insertions();

                ConnectNumbers();

                ConvertDegrees();

                SubstitutePi();

                RemoveUnnecessaryBrackets();

                InsertIntermediary();
            }
        }

        private void Union()
        {
            UniteLetters();

            UniteUpperIndex();
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
        }

        //Odebrání zbytečných závorek x + (2).
        private void RemoveUnnecessaryBrackets()
        {
            for (int i = 1; i < Count - 1; i++)
            {
                CheckNeighbors(i);
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
            for (int i = 1; i < Count; i++)
            {
                if (this[i] == "-" && this[i - 1] == "(")
                {
                    Insert(i, "0");
                    i++;
                }
            }
        }

        //Vložení znaku  pro násobení .
        private void InsertMultiplication()
        {
            for (int i = 1; i < Count; i++)
            {
                if (!this[i - 1].IsTrigonometricFunction() && CanInsertMultiplication(this[i - 1], this[i]))
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
        private void ConnectNumbers()
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

        private void ConvertDegrees()
        {
            for (int i = 1; i < Count; i++)
            {
                if (this[i] == "°")
                {
                    this[i - 1] = (Math.PI * double.Parse(this[i - 1]) / 180).ToString();
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
                if (this[i] == "√" && (this[i - 1].IsMathOperation() || this[i - 1] == "("))
                {
                    Insert(i, "2");
                    i++;
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

        //Zda jsou sousedé závorky.
        private void CheckNeighbors(int index)
        {
            if (index != 0 && index < Count - 1)
            {
                if (this[index - 1] == "(" && this[index + 1] == ")")
                {
                    RemoveNeighbors(index);
                }
            }
        }
    }
}
