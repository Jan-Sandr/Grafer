using Grafer.ExtensionMethods;
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
                Insertions();

                ConnectNumbers();

                RemoveUnnecessaryBrackets();
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
                if (CanInsertMultiplication(this[i - 1], this[i]))
                {
                    Insert(i, "*");
                }
            }
        }

        //Tabulka pro vložení znaku pro násobení.
        private static bool CanInsertMultiplication(string left, string right)
        {
            return (
                        (char.IsLetter(char.Parse(left)) && char.IsLetter(char.Parse(right))) ||
                        (char.IsDigit(char.Parse(left)) && char.IsLetter(char.Parse(right))) ||
                        (char.IsLetter(char.Parse(left)) && char.IsDigit(char.Parse(right))) ||
                        (char.IsLetterOrDigit(char.Parse(left)) && right == "(") ||
                        (left == ")" && char.IsLetterOrDigit(char.Parse(right))) ||
                        (left == ")" && right == "(")
                   );
        }

        //Spojení čísel do políčka třeba 10 je v základu jako 1 a 0. 
        private void ConnectNumbers()
        {
            for (int i = 1; i < Count; i++)
            {
                if (double.TryParse(this[i], out _) && double.TryParse(this[i - 1], out _))
                {
                    this[i - 1] += this[i];
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
