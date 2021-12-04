using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace Grafer2.CustomControls
{
    /// <summary>
    /// Interaction logic for EquationInput.xaml
    /// </summary>
    public partial class EquationInput : TextBox
    {
        public EquationInput()
        {
            InitializeComponent();
        }

        public (int SelectionStart, int SelectionLength, int MessageID) InvalidSection { get; private set; } = (0, 0, -1);

        public bool IsEquationValid
        {
            get
            {
                Text = Text.Replace(" ", "");
                bool isValid = EquationCheck.IsEquationValid(Text);

                if (!isValid)
                {
                    InvalidSection = EquationCheck.InvalidSection;
                }

                return isValid;
            }
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            RelationInputCheck(e);
            CloseBracket(e.Text);
        }

        private static void RelationInputCheck(TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, "[0-9 x + * / ( ) ^]") && e.Text != "-")
            {
                e.Handled = true;
            }
        }

        private void CloseBracket(string input)
        {
            if (input == "(" && SelectionStart == Text.Length)
            {
                Text += ')';
                SelectionStart = Text.Length - 1;
            }
        }

    }
}
