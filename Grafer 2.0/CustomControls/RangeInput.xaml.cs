using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace Grafer2.CustomControls
{
    /// <summary>
    /// Interaction logic for RangeInput.xaml
    /// </summary>
    public partial class RangeInput : TextBox
    {
        public double Value
        {

            get
            {
                return double.Parse(Text);
            }
        }

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

        public (int SelectionStart, int SelectionLength, string Message) InvalidSection { get; private set; }

        public string DisplayName { get; set; } = string.Empty;

        public RangeInput()
        {
            InitializeComponent();
        }

        private bool IsRangeEmpty()
        {
            InvalidSection = Text == "" ? new(0, 0, DisplayName + " is empty.") : new(0, 0, "");

            return InvalidSection.Message.Length != 0;
        }

        private bool AreEdgesValid()
        {
            if (Text[0] == ',')
            {
                InvalidSection = new(0, 1, DisplayName + " begins with invalid character.");
            }

            if (Text[^1] == '-' || Text[^1] == ',')
            {
                InvalidSection = new(Text.Length - 1, 1, DisplayName + " ends with invalid character.");
            }

            return InvalidSection.Message.Length == 0;
        }

        private bool ContainsMultipleChars()
        {
            if (GetCountOfChars(Text, '-') > 1 || GetCountOfChars(Text, ',') > 1)
            {
                InvalidSection = new(0, 0, DisplayName + " can't contain more than one minus or comma.");
            }

            return InvalidSection.Message.Length != 0;
        }

        private static int GetCountOfChars(string input, char character)
        {
            int countOfChars = 0;

            foreach (char c in input)
            {
                countOfChars = (c == character) ? countOfChars + 1 : countOfChars;
            }

            return countOfChars;
        }


        private bool ContainsTwoInvalidcharsInRow()
        {
            if (Text.Contains("-,") || Text.Contains(",-"))
            {
                InvalidSection = new(0, 0, DisplayName + " can't contains abreast minus and comma");
            }
            return InvalidSection.Message.Length != 0;
        }

        private void InputCheck(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, "[0-9 ,]") && e.Text != "-")
            {
                e.Handled = true;
            }
        }

        private void SpaceRestriction(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
    }
}
