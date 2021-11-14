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
            gFunction = new Function(equationInput.Text, gMinimumX, gMaximumX, drawingCanvas);
            gFunction.PrepareForCalculation();

            if (gFunction.Relation.IsRelationValid)
            {
                gFunction.CalculatePoints();
            }
            else
            {
                equationInput.Text = string.Join("", gFunction.Relation);
                NotifyInvalidInput(gFunction.Relation.InvalidSection.SelectionStart, gFunction.Relation.InvalidSection.SelectionLength, gFunction.Relation.InvalidSection.Message);
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
            if (isXRangeValid == IsXRangeInputValid())
            {
                gMinimumX = double.Parse(minimumXIpnut.Text);
                gMaximumX = double.Parse(maximumXInput.Text);
                isXRangeValid = IsXRangeValid();
            }
        }

        private bool IsXRangeValid()
        {
            return !IsMinimumHigher() && !IsRangeWidthZero() && !IsXRangeOut();
        }

        private bool AreXRangeEdgesValid()
        {
            if(!AreEdgesValid(minimumXIpnut.Text) || !AreEdgesValid(maximumXInput.Text))
            {
                isXRangeValid = false;
                NotifyError("Edge of range can't contains minus or comma.");
            }

            return isXRangeValid;
        }

        private static bool AreEdgesValid(string input)
        {
            bool areEdgesValid = true;

            if(input[^1] == '-' || input[0] == ',' || input[^1] == ',')
            {
                areEdgesValid = false;
            }

            return areEdgesValid;
        }

        private void GetXRangeFromCanvasWidth()
        {
            gMinimumX = -drawingCanvas.Width / 200;
            gMaximumX = drawingCanvas.Width / 200;
        }

        private bool IsRangeEmpty()
        {
            bool isRangeEmpty = false;

            if(minimumXIpnut.Text == "" || maximumXInput.Text == "")
            {
                NotifyError("Range is empty.");
                isRangeEmpty = true;
            }

            return isRangeEmpty;
        }

        private bool ContainsMultipleChars()
        {
            bool containsMultipleChars = false;

            if (GetCountOfChars(minimumXIpnut.Text, '-') > 1 || GetCountOfChars(minimumXIpnut.Text, ',') > 1)
            {
                containsMultipleChars = true;
            }

            if (GetCountOfChars(maximumXInput.Text, '-') > 1 || GetCountOfChars(maximumXInput.Text, ',') > 1)
            {
                containsMultipleChars = true;            
            }

            if(containsMultipleChars == true)
            {
                NotifyError("Range can't contains more than one minus or comma.");
            }

            return containsMultipleChars;
        }

        private bool ContainsTwoInvalidcharsInRow()
        {
            bool containsTwoInvalidcharsInRow = false;

            if (minimumXIpnut.Text.Contains("-,") || minimumXIpnut.Text.Contains(",-"))
            {
                containsTwoInvalidcharsInRow = true;
            }

            if (maximumXInput.Text.Contains("-,") || maximumXInput.Text.Contains(",-"))
            {
                containsTwoInvalidcharsInRow = true;
            }

            if(containsTwoInvalidcharsInRow)
            {
                NotifyError("Range can't contains abreast minus and comma");
            }

            return containsTwoInvalidcharsInRow;
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

        private bool IsXRangeInputValid()
        {
            return (
                                    !IsRangeEmpty() &&
                              AreXRangeEdgesValid() &&
                           !ContainsMultipleChars() &&
                    !ContainsTwoInvalidcharsInRow()
                   );
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

            if (gMaximumX < -drawingCanvas.Width / 200 || gMinimumX > drawingCanvas.Width / 200)
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
            drawingCanvas.Children.Clear();

            DrawCoordinateSystem();

            if (gFunction != null)
            {
                gFunction.Plot();
            }
        }

        private void DrawCoordinateSystem()
        {
            CoordinateSystem coordinateSystem = new(drawingCanvas.Width, drawingCanvas.Height);
            coordinateSystem.Create();
            drawingCanvas.Children.Add(coordinateSystem);
        }

        private void DrawingCanvasLoaded(object sender, RoutedEventArgs e)
        {
            Draw();
        }

        private void NotifyInvalidInput(int selectionStart, int selectionLength, string message)
        {
            SelectInvalidSection(selectionStart, selectionLength);
            NotifyError(message);
        }

        private void SelectInvalidSection(int selectionStart, int selectionLength)
        {
            defaultSelectionBrush = equationInput.SelectionBrush;
            equationInput.Focus();
            equationInput.SelectionBrush = Brushes.Red;
            equationInput.Select(selectionStart, selectionLength);
        }

        private void EquationInputTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            buttonDraw.IsEnabled = equationInput.Text.Trim() != "";
        }

        private void RelationInputCheck(object sender, TextCompositionEventArgs e)
        {
            if(!Regex.IsMatch(e.Text,"[0-9 x + * /]") && e.Text != "-")
            {
                e.Handled = true;
            }

            if (equationInput.SelectionBrush == Brushes.Red)
            {
                equationInput.SelectionBrush = defaultSelectionBrush;
            }
        }

        private void XRangeInputCheck(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, "[0-9 ,]") && e.Text != "-")
            {
                e.Handled = true;
            }
        }

        private void XRangeSpaceRestriction(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private static void NotifyError(string message)
        {
            MessageBox.Show(message);
        }

        private void ShortcutsPress(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter && equationInput.Text.Trim() != "")
            {
                DoProcess();
            }

            if(e.Key == Key.Escape)
            {
                equationInput.Text = "";
            }
        }
    }
}
