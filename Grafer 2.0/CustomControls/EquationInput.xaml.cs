using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace Grafer.CustomControls
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

        //Invalidní sekce.
        public (int SelectionStart, int SelectionLength, int MessageID) InvalidSection { get; private set; } = (0, 0, -1);

        //Jestli je rovnice v pořádku.
        public bool IsEquationValid
        {
            get
            {
                bool isValid = EquationCheck.IsEquationValid(Text);

                if (!isValid)
                {
                    InvalidSection = EquationCheck.InvalidSection;
                }

                return isValid;
            }
        }

        //Při vkládání textu z klávesnice. 
        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            RelationInputCheck(e);
            CloseBracket(e.Text);
        }

        //Omezení možných znaků.
        private static void RelationInputCheck(TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, "[0-9 x + * / ( ) ^ √]") && e.Text != "-")
            {
                e.Handled = true;
            }
        }

        //Dokončení závorky.
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
