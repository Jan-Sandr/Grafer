using System;
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

        private string[] messages = Array.Empty<string>(); // Pole pro chybové hlášky.

        private string[] localizationData = Array.Empty<string>(); // Pole pro lokalizaci prostředí.

        private readonly Brush defaultSelectionBrush = new SolidColorBrush(Color.FromRgb(0, 120, 215)); // Výchozí označovací barva.

        //Spuštení aplikce - jako load ve formsech.
        protected override void OnActivated(EventArgs e)
        {
            LoadDataFromFiles();
            LoadData();
            LocalizeUserInterface();
        }

        //Načte data aplikace, ne ze souborů.
        private void LoadData()
        {
            language = Language.English;
            languageSelect.Items.Add(Language.English);
            languageSelect.Items.Add(Language.Czech);
        }

        //Načte data ze souborů.
        private void LoadDataFromFiles()
        {
            messages = ReadFile("Messages.csv", false);
            localizationData = ReadFile("UILocalization.csv", true);
        }

        //Kliknutí na tlačítko vykreslit.
        private void ButtonDrawClick(object sender, RoutedEventArgs e)
        {
            onlyFunctionPlot = true;
            Start();
        }

        // 1. Začátek procesu.
        private void Start()
        {
            Reset(); // 2. Vynulování.

            if (equationInput.Text.Trim() != "")
            {
                DoProcess(); // 3. Získávání dat.
            }

            Draw();

            onlyFunctionPlot = false;
        }

        private void DoProcess()
        {
            if (equationInput.IsEquationValid)
            {
                GetXRange(); // 4. Získání rozsahu x.

                if (isXRangeValid)
                {
                    CreateFunction(); // 5. vytvoření funkce.
                }
            }
            else
            {
                NotifyInvalidInput(equationInput, equationInput.InvalidSection.SelectionStart, equationInput.InvalidSection.SelectionLength, equationInput.InvalidSection.MessageID);
            }
        }

        //Vytvoření instance funkce.
        private void CreateFunction()
        {
            gFunction = new Function(equationInput.Text, gMinimumX, gMaximumX, coordinateSystem);
            gFunction.CalculatePoints();
        }

        //Vynulování pro výpočet
        private void Reset()
        {
            gFunction = null;
            isXRangeValid = true;
        }

        //Získání rozsahu x.
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

        //Pokud není zaškrtnuto omezit, tak se rozsah získá ze šířky plátna.
        private void GetXRangeFromCanvasWidth()
        {
            gMinimumX = -coordinateSystem.Width / 200;
            gMaximumX = coordinateSystem.Width / 200;
        }

        //Získání rozsahu x z inputů od uživatele.
        private void GetXRangeFromInputs()
        {
            if (IsXRangeInputValid()) //Pouze pokud je validní.
            {
                gMinimumX = minimumXInput.Value;
                gMaximumX = maximumXInput.Value;
                isXRangeValid = IsXRangeValid();
            }
        }
        
        //Zjištení zda je rozsah v pořádku.
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

        //Porovnání rozsahů.
        private bool IsXRangeValid()
        {
            return !IsMinimumHigher() && !IsRangeWidthZero() && !IsXRangeOut();
        }

        //Jestli je minimum větší než maximum.
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

        //Jestli je minimum a maximum stejné.
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

        //Jestli je zadaný rozsah mimo vyditelnou plochu.
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

        //Kreslení
        private void Draw()
        {
            RefreshCoordinateSystem();

            if (gFunction != null)
            {
                gFunction.Plot();
            }
        }

        //Překreslení plátna
        private void RefreshCoordinateSystem()
        {
            //Pokud by se změnila čístě funkce, tak se promažou.
            if (!onlyFunctionPlot)
            {
                coordinateSystem.Create();
            }
            else
            {
                coordinateSystem.RemoveFunctions();
            }
        }

        //Oznámení chybného vstupu s označením.
        private void NotifyInvalidInput(TextBox textBox, int selectionStart, int selectionLength, int messageID)
        {
            SelectInvalidSection(textBox, selectionStart, selectionLength);
            NotifyError(textBox.Uid + " " + GetMessageFromID(messageID));
        }

        //Získání zprávy z jejího indexu v poli.
        private string GetMessageFromID(int ID)
        {
            string message = (language == Language.English) ? messages[ID].Split(';')[0] : messages[ID].Split(';')[1];
            return message;
        }

        //Označení nevalidní sekce.
        private static void SelectInvalidSection(TextBox textBox, int selectionStart, int selectionLength)
        {
            textBox.Focus();
            textBox.Select(selectionStart, selectionLength);
            textBox.SelectionBrush = Brushes.Red;
        }

        //Pokud byla chyba a uživatel se nějak dotkne zmení se barva zpátky na standardní.
        private void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.SelectionBrush == Brushes.Red)
            {
                textBox.SelectionBrush = defaultSelectionBrush;
            }
        }

        //Oznámení chyby.
        private static void NotifyError(string message)
        {
            MessageBox.Show(message);
        }

        //Ovládácí zkratky.
        private void ShortcutsPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                onlyFunctionPlot = true;
                Start();
            }

            if (e.Key == Key.Escape)
            {
                equationInput.Text = "";
                equationInput.Focus();
            }
        }

        //Posunutí tlačítka vykreslit.
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

        //První vykreslení plátna po načtení aplikace.
        private void CoordinateSystemLoaded(object sender, RoutedEventArgs e)
        {
            coordinateSystem.Create();
        }

        //Tlačítka pro speciální znaky.
        private void InsertionButtonClick(object sender, RoutedEventArgs e)
        {
            int inputCursorIndex = equationInput.SelectionStart;

            Button button = (Button)sender;

            if (button.Name == "buttonExponent")
            {
                equationInput.Text = equationInput.Text.Insert(inputCursorIndex, "^()");
            }

            equationInput.Focus();
            equationInput.SelectionStart = inputCursorIndex + 2;
        }

        //Čtení souboru.
        private static string[] ReadFile(string filePath, bool haveHead)
        {
            FileCompiler fileCompiler = new(filePath, haveHead);

            fileCompiler.Read();

            return fileCompiler.Data.ToArray();
        }

        //Localizace prostředí.
        private void LocalizeUserInterface()
        {
            int index = Convert.ToInt16(language);

            labelLanguage.Content = localizationData[0].Split(';')[index];
            limitX.Content = localizationData[1].Split(';')[index];
            equationInput.Uid = localizationData[2].Split(';')[index];
            buttonDraw.Content = localizationData[3].Split(';')[index];
        }

        //Změna jazyka.
        private void LanguageSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            language = (Language)languageSelect.SelectedItem;
            LocalizeUserInterface();
        }

        //Přizpůsobení prostředí velikosti aplikace.
        private void AdjustComponentsToApplicationSize()
        {
            AdjustCoordinateSystemSize();
        }

        //Přizpůsobení plátna velikosti aplikace.
        private void AdjustCoordinateSystemSize()
        {
            coordinateSystem.Width = ActualWidth - 400;
            coordinateSystem.Height = ActualHeight - 39;
            coordinateSystem.Margin = new Thickness(384, 0, 0, 700 - coordinateSystem.Height);
        }

        //Změna velikosti aplikace.
        private void ApplicationResize(object sender, SizeChangedEventArgs e)
        {
            AdjustComponentsToApplicationSize();
            Start();
        }

        //Povolení tlačítka vykreslit.
        private void EquationInputTextChanged(object sender, TextChangedEventArgs e)
        {
            buttonDraw.IsEnabled = equationInput.Text.Trim() != "";

            if (!buttonDraw.IsEnabled)
            {
                coordinateSystem.RemoveFunctions();
            }
        }
    }
}
