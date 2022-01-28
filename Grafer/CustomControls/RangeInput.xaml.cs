using Grafer.ExtensionMethods;
using System;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace Grafer.CustomControls
{
    /// <summary>
    /// Interaction logic for RangeInput.xaml
    /// </summary>
    public partial class RangeInput : TextBox
    {
        //Získání číslo rovnou.
        public double Value
        {
            get
            {
                return double.Parse(Text);
            }
        }

        //Pokud je hodnota ve stupních, tak aby stále byla dostupná číselná - pro výpočty atd.
        public double NumericalValue
        {
            get
            {
                return ValueType == DisplayValueType.Degree ? Value.ToNumerical() : Value;
            }
        }

        public enum DisplayValueType
        {
            Numerical,
            Degree
        }

        public DisplayValueType ValueType { get; set; } = DisplayValueType.Numerical;

        //Nastavý zobrazovací mírou a při změne přepočítá.
        public void SetValueType(DisplayValueType valueType)
        {
            if (ValueType == DisplayValueType.Numerical && valueType == DisplayValueType.Degree)
            {
                Text = Text != "" ? Math.Round(Value.ToDegrees(), 0).ToString() : "";
                ValueType = DisplayValueType.Degree;
            }

            if (ValueType == DisplayValueType.Degree && valueType == DisplayValueType.Numerical)
            {
                Text = Text != "" ? Math.Round(Value.ToNumerical(), 2).ToString() : "";
                ValueType = DisplayValueType.Numerical;
            }
        }

        //Jestli je obsah v pořádku.
        public bool IsValid
        {
            get
            {
                return (
                                        !IsRangeEmpty() &&
                                        AreEdgesValid() &&
                               !ContainsMultipleChars() &&
                        !ContainsTwoInvalidcharsInRow()
                       );
            }
        }

        //Invalidní sekce
        public (int SelectionStart, int SelectionLength, int MessageID) InvalidSection { get; private set; } = (0, 0, -1);

        public RangeInput()
        {
            InitializeComponent();
        }

        //Jestli je prázdný.
        private bool IsRangeEmpty()
        {
            InvalidSection = Text == "" ? (0, 0, 3) : (0, 0, -1);

            return InvalidSection.MessageID != -1;
        }

        //Zda  začíná a končí validním znakem.
        private bool AreEdgesValid()
        {
            if (Text[0] == ',')
            {
                InvalidSection = (0, 1, 4);
            }

            if (Text[^1] == '-' || Text[^1] == ',')
            {
                InvalidSection = (Text.Length - 1, 1, 5);
            }

            return InvalidSection.MessageID == -1;
        }

        //Jestli obsahuje třeba více minusů nebo čárek.
        private bool ContainsMultipleChars()
        {
            if (GetCountOfChars(Text, '-') > 1 || GetCountOfChars(Text, ',') > 1)
            {
                InvalidSection = (0, 0, 6);
            }

            return InvalidSection.MessageID != -1;
        }

        //Získání počtu konkrétních znaků ve stringu.
        private static int GetCountOfChars(string input, char character)
        {
            int countOfChars = 0;

            foreach (char c in input)
            {
                countOfChars = (c == character) ? countOfChars + 1 : countOfChars;
            }

            return countOfChars;
        }

        //Jestli obsahuje dva znaky za sebou, které nejsou možné.
        private bool ContainsTwoInvalidcharsInRow()
        {
            if (Text.Contains("-,") || Text.Contains(",-"))
            {
                InvalidSection = (0, 0, 7);
            }
            return InvalidSection.MessageID != -1;
        }

        //Omezení znaků, které lze napsat.
        private void InputCheck(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, "[0-9 ,]") && e.Text != "-")
            {
                e.Handled = true;
            }
        }

        //Zakázení použití mezerníku.
        private void SpaceRestriction(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
    }
}
