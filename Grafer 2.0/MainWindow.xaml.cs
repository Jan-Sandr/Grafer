using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Grafer2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private Function? gFunction;
        private double gMinimumX;
        private double gMaximumX;
        private bool isXRangeValid;

        private Brush? defaultSelectionBrush;

        private void ButtonDrawClick(object sender, RoutedEventArgs e)
        {
            DoProcess();
        }

        private void DoProcess()
        {
            Reset();
            GetXRange();

            if (isXRangeValid)
            {
                CreateFunction();
            }

            Draw();
        }

        private void CreateFunction()
        {
            gFunction = new Function(equationInput.Text, gMinimumX, gMaximumX, coordinateSystem);

            if (gFunction.Relation.IsRelationValid)
            {
                gFunction.CalculatePoints();
            }
            else
            {
                equationInput.Text = string.Join("", gFunction.Relation);
                NotifyInvalidInput(equationInput, gFunction.Relation.InvalidSection.SelectionStart, gFunction.Relation.InvalidSection.SelectionLength, gFunction.Relation.InvalidSection.Message);
            }
        }

        private void Reset()
        {
            gFunction = null;
            isXRangeValid = true;
        }

        private void GetXRange()
        {
            if (limitX.IsChecked == true)
            {
                GetXRangeFromInputs();
            }
            else
            {
                GetXRangeFromCanvasWidth();
            }

        }

        private void GetXRangeFromInputs()
        {
            if (IsXRangeInputValid())
            {
                gMinimumX = minimumXInput.Value;
                gMaximumX = maximumXInput.Value;
                isXRangeValid = IsXRangeValid();
            }
        }

        private bool IsXRangeInputValid()
        {
            if (!minimumXInput.IsValid)
            {
                isXRangeValid = false;
                NotifyInvalidInput(minimumXInput, minimumXInput.InvalidSection.SelectionStart, minimumXInput.InvalidSection.SelectionLength, minimumXInput.InvalidSection.Message);
            }
            else if (!maximumXInput.IsValid)
            {
                isXRangeValid = false;
                NotifyInvalidInput(maximumXInput, maximumXInput.InvalidSection.SelectionStart, maximumXInput.InvalidSection.SelectionLength, maximumXInput.InvalidSection.Message);
            }

            return isXRangeValid;
        }

        private bool IsXRangeValid()
        {
            return !IsMinimumHigher() && !IsRangeWidthZero() && !IsXRangeOut();
        }

        private static bool AreEdgesValid(string input)
        {
            bool areEdgesValid = true;

            if (input[^1] == '-' || input[0] == ',' || input[^1] == ',')
            {
                areEdgesValid = false;
            }

            return areEdgesValid;
        }

        private void GetXRangeFromCanvasWidth()
        {
            gMinimumX = -coordinateSystem.Width / 200;
            gMaximumX = coordinateSystem.Width / 200;
        }

        private bool IsMinimumHigher()
        {
            bool isMinimumHigher = false;

            if (gMinimumX > gMaximumX)
            {
                NotifyError("Minimum can't be higher than maximum");
                isMinimumHigher = true;
            }

            return isMinimumHigher;
        }

        private bool IsXRangeOut()
        {
            bool isXRangeOut = false;

            if (gMaximumX < -coordinateSystem.Width / 200 || gMinimumX > coordinateSystem.Width / 200)
            {
                NotifyError("Function won't be plotted because x's range is outside of drawing canvas.");
                isXRangeOut = true;
            }

            return isXRangeOut;
        }

        private bool IsRangeWidthZero()
        {
            bool isRangeWidthZero = false;

            if (gMinimumX == gMaximumX)
            {
                NotifyError("Minimum and maximum x can't be same.");
                isRangeWidthZero = true;
            }

            return isRangeWidthZero;
        }

        private void Draw()
        {
            coordinateSystem.RemoveFunctions();

            if (gFunction != null)
            {
                gFunction.Plot();
            }
        }

        private void DrawingCanvasLoaded(object sender, RoutedEventArgs e)
        {
            Draw();
        }

        private void NotifyInvalidInput(TextBox textBox, int selectionStart, int selectionLength, string message)
        {
            SelectInvalidSection(textBox, selectionStart, selectionLength);
            NotifyError(message);
        }

        private void SelectInvalidSection(TextBox textBox, int selectionStart, int selectionLength)
        {
            defaultSelectionBrush = textBox.SelectionBrush;
            textBox.Focus();
            textBox.SelectionBrush = Brushes.Red;
            textBox.Select(selectionStart, selectionLength);
        }

        private void EquationInputTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            buttonDraw.IsEnabled = equationInput.Text.Trim() != "";

            if (equationInput.Text.Trim() != "" && equationInput.Text[^1] == '(')
            {
                CloseBracket();
            }

            if(!buttonDraw.IsEnabled)
            {
                coordinateSystem.RemoveFunctions();
            }
        }

        private void CloseBracket()
        {
            equationInput.Text += ')';
            equationInput.SelectionStart = equationInput.Text.Length - 1;
        }

        private void RelationInputCheck(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, "[0-9 x + * / ( )]") && e.Text != "-")
            {
                e.Handled = true;
            }

            if (equationInput.SelectionBrush == Brushes.Red)
            {
                equationInput.SelectionBrush = defaultSelectionBrush;
            }
        }

        private static void NotifyError(string message)
        {
            MessageBox.Show(message);
        }

        private void ShortcutsPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && equationInput.Text.Trim() != "")
            {
                DoProcess();
            }

            if (e.Key == Key.Escape)
            {
                equationInput.Text = "";
                equationInput.Focus();
            }
        }

        private void SetButtonDrawMargin(object sender, KeyEventArgs e)
        {
            int marginTopMultiply;

            if (e.Key == Key.Back)
            {
                marginTopMultiply = Convert.ToInt16(Math.Floor((double)((equationInput.Text.Length - 2) / 14)));
            }
            else
            {
                marginTopMultiply = Convert.ToInt16(Math.Floor((double)((equationInput.Text.Length) / 14)));
            }

            buttonDraw.Margin = new Thickness(64, 206 + (26 * marginTopMultiply), 0, 0);
        }

        private void CoordinateSystemLoaded(object sender, RoutedEventArgs e)
        {
            coordinateSystem.Create();
        }
    }
}
