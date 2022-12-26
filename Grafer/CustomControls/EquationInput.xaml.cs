using Grafer.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
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
            shortcuts = new Dictionary<string, string>();
            LoadShortcuts();
        }

        private Dictionary<string, string> shortcuts;
        //Invalidní sekce.
        public (int SelectionStart, int SelectionLength, int MessageID) InvalidSection { get; private set; } = (0, 0, -1);

        public string LeftSide { get; set; } = string.Empty;

        public string RightSide { get; set; } = string.Empty;

        public enum InputType
        {
            None,
            Function,
            Sequence
        }

        public InputType EquationType
        {
            get
            {

                return IsEquationValid ? IsSequence() ? InputType.Sequence : InputType.Function : InputType.None;
            }
        }
        //Jestli je rovnice v pořádku.
        public bool IsEquationValid
        {
            get
            {
                InvalidSection = (0, 0, -1);
                if (Text.Contains("="))
                {
                    if (Text.IndexOf('=') != Text.LastIndexOf('='))
                    {
                        InvalidSection = (Text.LastIndexOf('='), 1, 36);
                        return false;
                    }

                    LeftSide = Text.Split('=')[0].Trim();
                    RightSide = Text.Split('=')[1].Trim();
                }
                else
                {
                    RightSide = Text;
                    CompleteEquation();
                }


                bool isValid = !AreSidesEmpty() && ContainsCorrectVariablesPair() && AreSidesValid(); // EquationCheck.IsEquationValid(Text) 

                if (isValid)
                {
                    if (LeftSide != "aₙ" && !EquationCheck.IsEquationValid(LeftSide))
                    {
                        InvalidSection = EquationCheck.InvalidSection;
                    }

                    if (RightSide != "aₙ" && !EquationCheck.IsEquationValid(RightSide))
                    {
                        int firstIndexAfterEqual = GetSpaceBetweenEqualAndNextChar();


                        InvalidSection = (EquationCheck.InvalidSection.SelectionStart + firstIndexAfterEqual, EquationCheck.InvalidSection.SelectionLength, EquationCheck.InvalidSection.MessageID);
                    }
                }

                return isValid && InvalidSection.MessageID == -1; // isValid;
            }
        }

        //Získá index prvního znaku po rovná se.
        private int GetSpaceBetweenEqualAndNextChar()
        {
            int space = 0;

            for (int i = Text.IndexOf("=") + 1; i < Text.Length; i++)
            {
                if (Text[i] != ' ')
                {
                    space = i;
                    break;
                }

            }

            return space;
        }

        //Kontrola zda se nekombinují proměnné pro funkci a posloupnost.
        private bool ContainsCorrectVariablesPair()
        {
            bool containsN = false;

            for (int i = 1; i < Text.Length; i++)
            {
                if (Text[i] == 'n' && Text[i - 1] != 'i' && Text[i - 1] != 'l')
                {
                    containsN = true;
                }
            }

            if ((Text.Contains("aₙ") || containsN) && (Text.Contains("y") || Text.Contains("x")))
            {
                InvalidSection = (0, 0, 39);
            }

            return InvalidSection.MessageID == -1;
        }

        //Kontrola zda nejsou strany rovnice prázdné.
        private bool AreSidesEmpty()
        {
            if (LeftSide == "")
            {
                InvalidSection = (0, 0, 37);
            }

            if (RightSide == "")
            {
                InvalidSection = (Text.Length, 0, 38);
            }

            if (LeftSide == RightSide)
            {
                InvalidSection = (0, 0, 42);
            }

            return InvalidSection.MessageID != -1;
        }

        //Jestli jsou vzájemně strany rovnice v pořádku.
        private bool AreSidesValid()
        {
            if (Text.Contains("aₙ") && (LeftSide != "aₙ" && RightSide != "aₙ"))
            {
                InvalidSection = (Text.Length, 0, 40);
            }

            if (Text.Contains("y") && (LeftSide != "y" && RightSide != "y"))
            {
                InvalidSection = (Text.Length, 0, 41);
            }

            if ((LeftSide.Contains('y') && RightSide.Contains('y')) || (LeftSide.Contains('x') && RightSide.Contains('x')))
            {
                InvalidSection = (0, 0, 43);
            }

            return InvalidSection.MessageID == -1;
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
            string[] special = new string[3] { "-", "[", "]" }; // tyto znaky nejdou zadávat do regexu.

            if (!Regex.IsMatch(e.Text, "[0-9 x + * / ( ) ^ √ , s i n c o s t a g ° π | l e = y]") && !special.Contains(e.Text))
            {
                e.Handled = true;
            }
        }

        //Dokončení závorky.
        private void CloseBracket(string input)
        {
            if (SelectionStart == Text.Length)
            {
                if (input == "(")
                {
                    Text += ')';
                    SelectionStart = Text.Length - 1;
                }

                if (input == "|")
                {
                    Text += "|";
                    SelectionStart = Text.Length - 1;
                }
            }
        }

        //Vyvolávní zkratky z klávesnice PreviewKeyDown event.
        private void ShortcutInput(object sender, KeyEventArgs e)
        {
            string modifier = Keyboard.Modifiers.ToString();
            string shortcut = modifier + e.Key.ToString();

            if (shortcuts.ContainsKey(shortcut))
            {
                InsertShortcut(shortcuts[modifier + e.Key.ToString()]);
            }
        }

        //Vložení zkratky do textového pole.
        public void InsertShortcut(string shortcut)
        {
            string addition = (shortcut != "π" && shortcut != "°" && shortcut != "|" && shortcut != "aₙ") ? "()" : "";

            int selectionStart = SelectionStart;

            Text = Text.Insert(SelectionStart, shortcut + addition);

            SelectionStart = selectionStart + shortcut.Length + addition.Length + ((shortcut != "π" && shortcut != "°" && shortcut != "|" && shortcut != "aₙ") ? -1 : 0);
        }

        private void LoadShortcuts()
        {
            List<string> inputShortcuts = Properties.Resources.Shortcuts.Split("\r\n").Skip(1).ToList();

            shortcuts = inputShortcuts.ToArray().ToDictionary();
        }

        private void CompleteEquation()
        {
            if (IsSequence())
            {
                LeftSide = "aₙ";
            }
            else
            {
                LeftSide = "y";
            }

            Text = Text.Insert(0, LeftSide + " = ");
            SelectionStart = Text.Length;
        }

        public bool IsSequence()
        {
            bool isSequence = Text[0] == 'n';

            for (int i = 0; i < Text.Length - 1 && !isSequence; i++)
            {
                if ((Text[i + 1] == 'n' && Text[i] != 'i' && Text[i] != 'l') || (Text[i] == 'a' && Text[i + 1] == 'ₙ'))
                {
                    isSequence = true;
                }
            }

            return isSequence;
        }
    }
}
