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

        private new enum Language
        {
            English = 1,
            Czech = 2
        }

        private Language language;
        private Function? gFunction;
        private double gMinimumX;
        private double gMaximumX;
        private bool isXRangeValid;
        private bool onlyFunctionPlot;

        private string[] messages = Array.Empty<string>();

        private string[] localizationData = Array.Empty<string>();

        private Brush? defaultSelectionBrush;

        protected override void OnActivated(EventArgs e)
        {
            LoadDataFromFiles();
            LoadData();
            LocalizeUserInterface();
        }

        private void LoadData()
        {
            language = Language.English;
            languageSelect.Items.Add(Language.English);
            languageSelect.Items.Add(Language.Czech);
        }

        private void LoadDataFromFiles()
        {
            messages = ReadFile("Messages.csv", false);
            localizationData = ReadFile("UILocalization.csv", true);
        }

        private void ButtonDrawClick(object sender, RoutedEventArgs e)
        {
            onlyFunctionPlot = true;
            DoProcess();
        }

        private void DoProcess()
        {
            Reset();

            if (equationInput.Text.Trim() != "")
            {
                GetXRange();

                if (isXRangeValid)
                {
                    CreateFunction();
                }
            }

            Draw();
            onlyFunctionPlot = false;
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
                NotifyInvalidInput(equationInput, gFunction.Relation.InvalidSection.SelectionStart, gFunction.Relation.InvalidSection.SelectionLength, gFunction.Relation.InvalidSection.MessageID);
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
                NotifyInvalidInput(minimumXInput, minimumXInput.InvalidSection.SelectionStart, minimumXInput.InvalidSection.SelectionLength, minimumXInput.InvalidSection.MessageID);
            }
            else if (!maximumXInput.IsValid)
            {
                isXRangeValid = false;
                NotifyInvalidInput(maximumXInput, maximumXInput.InvalidSection.SelectionStart, maximumXInput.InvalidSection.SelectionLength, maximumXInput.InvalidSection.MessageID);
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
                string message = (language == Language.English) ? messages[0].Split(';')[0] : messages[0].Split(';')[1];
                NotifyError(message);
                isMinimumHigher = true;
            }

            return isMinimumHigher;
        }

        private bool IsRangeWidthZero()
        {
            bool isRangeWidthZero = false;

            if (gMinimumX == gMaximumX)
            {
                string message = (language == Language.English) ? messages[1].Split(';')[0] : messages[1].Split(';')[1];
                NotifyError(message);
                isRangeWidthZero = true;
            }

            return isRangeWidthZero;
        }

        private bool IsXRangeOut()
        {
            bool isXRangeOut = false;

            if (gMaximumX < -coordinateSystem.Width / 200 || gMinimumX > coordinateSystem.Width / 200)
            {
                string message = (language == Language.English) ? messages[2].Split(';')[0] : messages[2].Split(';')[1];
                NotifyError(message);
                isXRangeOut = true;
            }

            return isXRangeOut;
        }

        private void Draw()
        {
            RefreshCoordinateSystem();
            
            if (gFunction != null)
            {
                gFunction.Plot();
            }
        }

        private void RefreshCoordinateSystem()
        {
            if (!onlyFunctionPlot)
            {
                coordinateSystem.Create();
            }
            else
            {
                coordinateSystem.RemoveFunctions();
            }
        }

        private void NotifyInvalidInput(TextBox textBox, int selectionStart, int selectionLength, int messageID)
        {
            SelectInvalidSection(textBox, selectionStart, selectionLength);
            NotifyError(textBox.Uid + " " + GetMessageFromID(messageID));
        }

        private string GetMessageFromID(int ID)
        {
            string message = (language == Language.English) ? messages[ID].Split(';')[0] : messages[ID].Split(';')[1];
            return message;
        }

        private void SelectInvalidSection(TextBox textBox, int selectionStart, int selectionLength)
        {
            defaultSelectionBrush = textBox.SelectionBrush;
            textBox.Focus();
            textBox.Select(selectionStart, selectionLength);
            textBox.SelectionBrush = Brushes.Red;
        }

        private void EquationInputTextChanged(object sender, TextChangedEventArgs e)
        {
            buttonDraw.IsEnabled = equationInput.Text.Trim() != "";

            if(!buttonDraw.IsEnabled)
            {
                coordinateSystem.RemoveFunctions();
            }
        }

        private void PreviewEquationInputTextChanged(object sender, TextCompositionEventArgs e)
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

        private void SetDefaultSelectionBrush()
        {
            if (equationInput.SelectionBrush == Brushes.Red)
            {
                equationInput.SelectionBrush = defaultSelectionBrush;
            }
        }

        private void CloseBracket(string input)
        {
            if (input == "(" && equationInput.SelectionStart == equationInput.Text.Length)
            {
                equationInput.Text += ')';
                equationInput.SelectionStart = equationInput.Text.Length - 1;
            }
        }

        private static void NotifyError(string message)
        {
            MessageBox.Show(message);
        }

        private void ShortcutsPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                onlyFunctionPlot = true;
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

        private void InsertionButtonClick(object sender, RoutedEventArgs e)
        {
            int inputCursorIndex = equationInput.SelectionStart;

            Button button = (Button)sender;

            if(button.Name == "buttonExponent")
            {
                equationInput.Text = equationInput.Text.Insert(inputCursorIndex, "^()");
            }

            equationInput.Focus();
            equationInput.SelectionStart = inputCursorIndex + 2;
        }

        private static string[] ReadFile(string filePath, bool haveHead)
        {
            FileCompiler fileCompiler = new(filePath, haveHead );

            fileCompiler.Read();

            return fileCompiler.Data.ToArray();
        } 

        private void LocalizeUserInterface()
        {
            int index = Convert.ToInt16(language);

            labelLanguage.Content = localizationData[0].Split(';')[index];
            limitX.Content = localizationData[1].Split(';')[index];
            equationInput.Uid = localizationData[2].Split(';')[index];
            buttonDraw.Content = localizationData[3].Split(';')[index];
        }

        private void LanguageSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            language = (Language)languageSelect.SelectedItem;
            LocalizeUserInterface();
        }

        private void AdjustComponentsToApplicationSize()
        {
            AdjustCoordinateSystemSize();
        }

        private void AdjustCoordinateSystemSize()
        {
            coordinateSystem.Width = ActualWidth - 400;
            coordinateSystem.Height = ActualHeight - 39;
            coordinateSystem.Margin = new Thickness(384, 0, 0, 700 - coordinateSystem.Height);
        }

        private void ApplicationResize(object sender, SizeChangedEventArgs e)
        {
            AdjustComponentsToApplicationSize();
            DoProcess();
        }

        private void EquationInputSelectionChanged(object sender, RoutedEventArgs e)
        {
            SetDefaultSelectionBrush();
        }
    }
}
