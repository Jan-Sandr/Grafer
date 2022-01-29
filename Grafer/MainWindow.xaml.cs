using Grafer.CustomControls;
using Grafer.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static Grafer.CustomControls.CoordinateSystem;

namespace Grafer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            coordinateSystem.MouseWheel += CoordinateSystemMouseWheel;
            coordinateSystem.AbsoluteShiftChanged += CoordinateSystemAbsoluteShiftChanged;
        }

        //Vyvolání překreslení funkce při změny posunutí soustavy.
        private void CoordinateSystemAbsoluteShiftChanged(object? sender, EventArgs e)
        {
            Start();
        }

        //Událost pohybu kolečka myši
        private void CoordinateSystemMouseWheel(object sender, MouseWheelEventArgs e)
        {
            changedByScrool = true;
            sliderZoomLevel.Value = coordinateSystem.ZoomLevel;
            changedByScrool = false;
            Start();
        }

        private new enum Language
        {
            English = 0,
            Czech = 1
        }

        List<Function> functions = new List<Function>();

        private Language language;
        private Function? gFunction;
        private double gMinimumX;
        private double gMaximumX;
        private bool isXRangeValid;
        private bool changedByScrool;
        private Function.FunctionType previousFunctionType = Function.FunctionType.Basic; // Předešlí typ funkce

        private string[] messages = Array.Empty<string>(); // Pole pro chybové hlášky.

        private Dictionary<string, string> localizationData = new Dictionary<string, string>(); // Pole pro lokalizaci prostředí.

        private readonly Brush defaultSelectionBrush = new SolidColorBrush(Color.FromRgb(0, 120, 215)); // Výchozí označovací barva.
        private readonly Color defaultStatusColor = Color.FromRgb(125, 255, 99); // výchozí barva statusu.

        private bool addingNewFunction;

        #region Načítání dat

        //Spuštení aplikce - jako load ve formsech.
        private void ApplicationLoaded(object sender, RoutedEventArgs e)
        {
            string directory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName + "\\Data";
            LoadDataFromFiles(directory);
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
        private void LoadDataFromFiles(string directory)
        {
            messages = ReadFile(directory + "\\Messages.csv", true);
            localizationData = ReadFile(directory + "\\UILocalization.csv", true).ToDictionary();
        }

        #endregion

        //Kliknutí na tlačítko vykreslit.
        private void ButtonDrawClick(object sender, RoutedEventArgs e)
        {
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

            Draw(); // 4. Vykreslení funkce.
        }

        //Hlavní proces
        private void DoProcess()
        {
            if (equationInput.IsEquationValid)
            {
                GetXRange(); // 4. Získání rozsahu x.

                if (isXRangeValid)
                {
                    CreateFunction(); // 5. vytvoření funkce.

                    if(!addingNewFunction && listBoxFunctions.SelectedIndex != -1)
                    {
                        UpdateFunctionInList(gFunction!);
                    }
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
            string name = GenerateUniqueName();
            gFunction = new Function(name, equationInput.Text, gMinimumX, gMaximumX, limitX.IsChecked == true, coordinateSystem, rectangleColor.Fill, checkBoxInverse.IsChecked == true);
            gFunction.CalculatePoints();
        }

        //Vynulování pro výpočet
        private void Reset()
        {
            gFunction = null;
            isXRangeValid = true;
            SetStatus("OK", defaultStatusColor);
        }

        #region Získání rozsahu x

        //Získání rozsahu x.
        private void GetXRange()
        {
            if (limitX.IsChecked == true)
            {
                GetXRangeFromInputs();
            }
            else
            {
                GetXRangeFromCoordinateSystem();
            }
        }

        //Pokud není zaškrtnuto omezit, tak se rozsah získá ze šířky plátna.
        private void GetXRangeFromCoordinateSystem()
        {
            gMinimumX = (-coordinateSystem.Width / 200 - coordinateSystem.AbsoluteShift.OnX / 100) / coordinateSystem.Zoom;
            gMaximumX = (coordinateSystem.Width / 200 - coordinateSystem.AbsoluteShift.OnX / 100) / coordinateSystem.Zoom;
        }

        //Získání rozsahu x z inputů od uživatele.
        private void GetXRangeFromInputs()
        {
            if (IsXRangeInputValid()) //Pouze pokud je validní.
            {
                gMinimumX = minimumXInput.NumericalValue;
                gMaximumX = maximumXInput.NumericalValue;
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

        #endregion

        #region Kontrola rozsahu x

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
                string message = messages[0].Split(';')[(int)language];
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
                string message = messages[1].Split(';')[(int)language];
                NotifyError(message);
                isRangeWidthZero = true;
            }

            return isRangeWidthZero;
        }

        //Jestli je zadaný rozsah mimo vyditelnou plochu.
        private bool IsXRangeOut()
        {
            bool isXRangeOut = false;

            if (gMaximumX < -(coordinateSystem.Width / 200 + coordinateSystem.AbsoluteShift.OnX / 100) / coordinateSystem.Zoom || gMinimumX > (coordinateSystem.Width / 200 - coordinateSystem.AbsoluteShift.OnX / 100) / coordinateSystem.Zoom)
            {
                string message = messages[2].Split(';')[(int)language];
                NotifyError(message);
                isXRangeOut = true;
            }

            return isXRangeOut;
        }

        #endregion

        //Kreslení
        private void Draw()
        {
            coordinateSystem.RemoveFunctions();

            if (gFunction != null)
            {
                UpdateToMeasure();

                if (gFunction.IsDrawable())
                {
                    DrawFunction();
                }
                else
                {
                    NotifyError(GetMessageFromID(gFunction.ErrorMessageID));
                }
            }
        }

        //Vykreslení funkce.
        private void DrawFunction()
        {
            gFunction!.Plot(gFunction.Inverse, 1);

            if (checkBoxKeepOrigin.IsChecked == true) // Při inverzní, jestli má vykreslit původní křivku s nižší viditelností.
            {
                gFunction.Plot(false, 0.3);
            }
        }

        //Aktualizace komponentů na aktuální míru (pokud se změnila).
        private void UpdateToMeasure()
        {
            if (previousFunctionType != gFunction!.Type)
            {
                UpdateCoordinateSystem();

                UpdateRangeMeasure();

                SetDegreeLabelsVisibility();
            }
        }

        //Pošle informace souřadnicové systém o tom jakou mírou má použit.
        private void UpdateCoordinateSystem()
        {
            Measure horizontalMeasure = gFunction!.Type == Function.FunctionType.TrigonometricFunction ? CoordinateSystem.Measure.Degree : CoordinateSystem.Measure.Numerical;
            Measure verticalMeasure = gFunction.Type == Function.FunctionType.InverseTrigonometricFunction ? CoordinateSystem.Measure.Degree : CoordinateSystem.Measure.Numerical;

            coordinateSystem.Refresh(horizontalMeasure, verticalMeasure);
            previousFunctionType = gFunction!.Type;
        }

        //Aktualizuje míru v políčkách pro rozsah.
        private void UpdateRangeMeasure()
        {
            if (gFunction!.Type == Function.FunctionType.TrigonometricFunction)
            {
                minimumXInput.SetValueType(RangeInput.DisplayValueType.Degree);
                maximumXInput.SetValueType(RangeInput.DisplayValueType.Degree);
            }
            else
            {
                minimumXInput.SetValueType(RangeInput.DisplayValueType.Numerical);
                maximumXInput.SetValueType(RangeInput.DisplayValueType.Numerical);
            }
        }

        //Aktualizuje zda se zobrazí stupně za políčky pro rozsah.
        private void SetDegreeLabelsVisibility()
        {
            if (gFunction!.Type == Function.FunctionType.TrigonometricFunction)
            {
                labelDegreeMinimum.Visibility = Visibility.Visible;
                labelDegreeMaximum.Visibility = Visibility.Visible;
            }
            else
            {
                labelDegreeMinimum.Visibility = Visibility.Hidden;
                labelDegreeMaximum.Visibility = Visibility.Hidden;
            }
        }

        #region Oznámení chyb

        //Oznámení chybného vstupu s označením.
        private void NotifyInvalidInput(TextBox textBox, int selectionStart, int selectionLength, int messageID)
        {
            SelectInvalidSection(textBox, selectionStart, selectionLength);
            NotifyError(textBox.Uid + " " + GetMessageFromID(messageID));
        }

        //Získání zprávy z jejího indexu v poli.
        private string GetMessageFromID(int ID)
        {
            string message = messages[ID].Split(';')[(int)language];
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
        private void NotifyError(string message)
        {
            SetStatus(message, Colors.Red);
        }

        //Nastaví zprávu status a jeho barvu.
        private void SetStatus(string message, Color color)
        {
            textBlockStatus.Text = $"Status: {message}";
            textBlockStatus.Foreground = new SolidColorBrush(color);
        }

        #endregion

        //Ovládácí zkratky.
        private void ShortcutsPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Start();
            }

            if (e.Key == Key.Escape)
            {
                equationInput.Text = "";
                equationInput.Focus();
                Reset();
            }
        }

        //Tlačítka pro speciální znaky.
        private void InsertionButtonClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            equationInput.Focus();

            equationInput.InsertShortcut(button.Uid);
        }

        //Čtení souboru.
        private static string[] ReadFile(string filePath, bool haveHead)
        {
            FileCompiler fileCompiler = new FileCompiler(filePath, haveHead);

            fileCompiler.Read();

            if (!fileCompiler.IsOK || !fileCompiler.IsDataOK)
            {
                MessageBox.Show(fileCompiler.ErrorMessage);
            }

            return fileCompiler.Data.ToArray();
        }

        #region Lokalizace

        //Localizace prostředí.
        private void LocalizeUserInterface()
        {
            List<ContentControl> controls = gridMain.Children.OfType<ContentControl>().ToList();

            for (int i = 0; i < controls.Count; i++)
            {
                if (localizationData.ContainsKey(controls[i].Name))
                {
                    string[] localizationTexts = localizationData[controls[i].Name].Split(';');
                    controls[i].Content = localizationTexts[(int)language];
                }
            }

            equationInput.Uid = (language == Language.English) ? "Relation" : "Předpis";
        }

        //Změna jazyka.
        private void LanguageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            language = (Language)languageSelect.SelectedItem;

            LocalizeUserInterface();

            SetStatus("OK", defaultStatusColor);

            if (equationInput.Text.Trim() != "" && !equationInput.IsEquationValid)
            {
                NotifyInvalidInput(equationInput, equationInput.InvalidSection.SelectionStart, equationInput.InvalidSection.SelectionLength, equationInput.InvalidSection.MessageID);
            }
        }

        #endregion

        #region Responzivita

        //Přizpůsobení prostředí velikosti aplikace.
        private void AdjustComponentsToApplicationSize()
        {
            AdjustCoordinateSystemSize();
            AdjustButtonSectionLayout();
        }

        //Přizpůsobení plátna velikosti aplikace.
        private void AdjustCoordinateSystemSize()
        {
            coordinateSystem.Width = ActualWidth - 400;
            coordinateSystem.Height = ActualHeight - 39;
            coordinateSystem.Margin = new Thickness(384, 0, 0, 700 - coordinateSystem.Height);
        }

        //Přizpůsobení sekce s tlačítky velikosti aplikace
        private void AdjustButtonSectionLayout()
        {
            scrollButtonSection.Width = coordinateSystem.Width;

            scrollButtonSection.Margin = new Thickness(384, ActualHeight - scrollButtonSection.Height - 39, 0, 0);

            AdjustButtonSectionInnerMargin();
        }

        //Upravuje pozici tlačítek na základě toho, zda je viditelný horizontální posuvník.
        private void AdjustButtonSectionInnerMargin()
        {
            double buttonsBottomMargin = 20;

            if (coordinateSystem.Width < 700) // Scrollbar posunu nahoru - pro vyrovnání.
            {
                buttonsBottomMargin = 3;
            }

            buttonSection.Margin = new Thickness(0, 0, 0, buttonsBottomMargin);
        }

        //Změna velikosti aplikace.
        private void ApplicationResize(object sender, SizeChangedEventArgs e)
        {
            AdjustComponentsToApplicationSize();
            coordinateSystem.Refresh();
            Start();
        }

        #endregion

        //Povolení tlačítka vykreslit.
        private void EquationInputTextChanged(object sender, TextChangedEventArgs e)
        {
            buttonDraw.IsEnabled = equationInput.Text.Trim() != "";

            if (!buttonDraw.IsEnabled)
            {
                coordinateSystem.RemoveFunctions();
            }
        }

        //Posunutí tlačítka vykreslit.
        private void EquationInputSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double marginTopMultiply = equationInput.LineCount - 1;

            buttonDraw.Margin = new Thickness(64, 206 + (26 * marginTopMultiply), 0, 0);
        }

        //Událost při pohybu jezdítka pro nastavení přiblížení soustavy.
        private void SliderZoomLevelValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!changedByScrool)
            {
                coordinateSystem.ZoomLevel = Convert.ToInt16(sliderZoomLevel.Value);
                coordinateSystem.Refresh();
            }
        }

        //Detail aby se v tooltipu zobrazil level - důvod aby nebyl prádzný - nešel odstranit!
        private void SliderZoomLevelToolTipOpening(object sender, ToolTipEventArgs e)
        {
            sliderZoomLevel.ToolTip = sliderZoomLevel.Value;
        }

        //Ovládání zobrazení sekce s tlačítky.
        private void ButtonShowHideButtonsClick(object sender, RoutedEventArgs e)
        {
            scrollButtonSection.Visibility = scrollButtonSection.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        //Kliknutí na obdelník s barvou.
        private void RectangleColorMouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color selectedColor = Color.FromArgb(255, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                rectangleColor.Fill = new SolidColorBrush(selectedColor);
            }
        }

        #region Generování jména

        //Vytvoří unikátní jméno pro funkci.
        private string GenerateUniqueName()
        {
            int number = 1;

            if (functions.Count > 0)
            {
                if (addingNewFunction)
                {
                    number = GetNextFunctionNumber();
                }
                else
                {
                    number = listBoxFunctions.SelectedIndex == -1 ? GetNextFunctionNumber() : functions[listBoxFunctions.SelectedIndex].Number;
                }
            }
           
            return $"f{number}: {equationInput.Text}";
        }

        //Získá nejmenší volné číslé pro jméno funkce.
        private int GetNextFunctionNumber()
        {
            int[] numbers = GetFunctionNumbers();

            int lowestNumber = numbers[^1] + 1;

            for (int i = 0; i < numbers.Length; i++)
            {
                if (numbers[i] - i > 1)
                {
                    lowestNumber = i + 1;
                    break;
                }
            }

            return lowestNumber;
        }

        //Získá pole z čísel jmén funkcí v listu
        private int[] GetFunctionNumbers()
        {
            int[] numbers = new int[functions.Count];

            for (int i = 0; i < functions.Count; i++)
            {
                numbers[i] = functions[i].Number;
            }

            Array.Sort(numbers);

            return numbers;
        }

        #endregion

        //Událost, která nastavá při změne jestli je inverzní čekbox zaškrtnut.
        private void InverseCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (checkBoxInverse.IsChecked == true)
            {
                checkBoxKeepOrigin.IsEnabled = true;
            }
            else
            {
                checkBoxKeepOrigin.IsEnabled = false;
                checkBoxKeepOrigin.IsChecked = false;
            }
        }

        #region Ovládání listboxu

        //Kliknutí na tlačítko přidat funkci.
        private void ButtonAddFunctionClick(object sender, RoutedEventArgs e)
        {
            addingNewFunction = true;
            Start();

            if (gFunction != null)
            {
                AddFunctionToList(gFunction);
            }
           
            addingNewFunction = false;
        }

        //Kliknutí na tlačítko odebrat funkci.
        private void ButtonRemoveFunctionClick(object sender, RoutedEventArgs e)
        {
            if (listBoxFunctions.SelectedIndex != -1)
            {
                functions.RemoveAt(listBoxFunctions.SelectedIndex);
                listBoxFunctions.Items.RemoveAt(listBoxFunctions.SelectedIndex);
            }
        }

        //Přidání funkce do listu.
        private void AddFunctionToList(Function function)
        {
                functions.Add(function);
                AddFunctionToListBox();
        }

        //Přidání checkboxu, který reprezentuje funkci v listboxu.
        private void AddFunctionToListBox()
        {
            CheckBox checkBox = new CheckBox
            {
                Name = "checkBox" + gFunction!.Name.Split(':')[0],
                Content = gFunction.Name,
                IsEnabled = false

            };

            listBoxFunctions.Items.Add(checkBox);
        }

        //Přepsání vybrané funkce v listboxu.
        private void UpdateFunctionInList(Function function)
        {
                functions[listBoxFunctions.SelectedIndex] = function;
                (listBoxFunctions.Items[listBoxFunctions.SelectedIndex] as CheckBox)!.Content = function.Name;
        }

        //Změna vybrané funkce v listboxu.
        private void ListBoxFunctionsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBoxFunctions.SelectedIndex != -1)
            {
                gFunction = functions[listBoxFunctions.SelectedIndex];
                LoadUserInterfaceValuesFromFunction(gFunction);
                Start();    
            }
        }

        //Načtení vlastností funkce do uživatelských prvků.
        private void LoadUserInterfaceValuesFromFunction(Function function)
        {
            limitX.IsChecked = function.IsLimited;

            if(function.IsLimited)
            {
                minimumXInput.Text = function.MinimumX.ToString();
                maximumXInput.Text = function.MaximumX.ToString();
            }

            rectangleColor.Fill = function.Brush;
            checkBoxInverse.IsChecked = function.Inverse;

            equationInput.Text = function.InputRelation;
        }

        //Zrušení výběru v listboxu
        private void ButtonDeselectFunctionClick(object sender, RoutedEventArgs e)
        {
            listBoxFunctions.SelectedIndex = -1;
        }

        #endregion
    }
}
