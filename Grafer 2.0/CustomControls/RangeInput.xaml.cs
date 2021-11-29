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

        public (int SelectionStart, int SelectionLength, int MessageID) InvalidSection { get; private set; } = (0, 0, -1);

        public RangeInput()
        {
            InitializeComponent();
        }

        private bool IsRangeEmpty()
        {
            InvalidSection = Text == "" ? new(0, 0, 0) : new(0, 0, -1);

            return InvalidSection.MessageID != -1;
        }

        private bool AreEdgesValid()
        {
            if (Text[0] == ',')
            {
                InvalidSection = new(0, 1, 1);
            }

            if (Text[^1] == '-' || Text[^1] == ',')
            {
                InvalidSection = new(Text.Length - 1, 1, 2);
            }

            return InvalidSection.MessageID == -1;
        }

        private bool ContainsMultipleChars()
        {
            if (GetCountOfChars(Text, '-') > 1 || GetCountOfChars(Text, ',') > 1)
            {
                InvalidSection = new(0, 0, 3);
            }

            return InvalidSection.MessageID != -1;
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
                InvalidSection = new(0, 0, 4);
            }
            return InvalidSection.MessageID != -1;
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
