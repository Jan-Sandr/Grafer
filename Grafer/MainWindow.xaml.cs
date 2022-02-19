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
using System.Windows.Media.Imaging;
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
            coordinateSystem.FunctionShiftChanged += CoordinateSystemFunctionShiftChanged;
        }

        private void CoordinateSystemFunctionShiftChanged(object? sender, EventArgs e)
        {
            if (gFunction != null)
            {
                coordinateSystem.RemoveItem(gFunction.Name);

                if (coordinateSystem.FunctionShift.OnX != 0 || coordinateSystem.FunctionShift.OnY != 0)
                {
                    checkBoxFreeFunction.IsChecked = true;
                }
                DrawFunction(gFunction, coordinateSystem.FunctionShift);
            }
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

        readonly List<Function> functions = new List<Function>();

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
        private bool IsMainMenuVisible = true;
        private int hiddenRelationIndex = -1;

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
            checkBoxFreeFunction.IsChecked = false;
            Start();
        }

        #region Tvorba nové funkce

        // 1. Začátek procesu.
        private void Start()
        {
            Reset(); // 2. Vynulování.

            if (equationInput.Text.Trim() != "")
            {
                DoProcess(); // 3. Získávání dat.
            }

            Draw(); // 4. Vykreslení funkce.

            // 5. Pokud je více funkcí najednou překreslí se všechny zaškrtlé.
            if (checkBoxMultipleFunctions.IsChecked == true)
            {
                Recalculation();
            }
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

                    if (!addingNewFunction && listBoxFunctions.SelectedIndex != -1)
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
            gFunction = new Function(name, rectangleColor.Fill, equationInput.Text, limitX.IsChecked == true, gMinimumX, gMaximumX, coordinateSystem, checkBoxInverse.IsChecked == true);
            gFunction.CalculatePoints();
        }

        //Vynulování pro výpočet
        private void Reset()
        {
            gFunction = null;
            isXRangeValid = true;
            SetStatus("OK", defaultStatusColor);
        }

        #endregion

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
                gMinimumX = double.NegativeInfinity;
                gMaximumX = double.PositiveInfinity;
            }
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

        #region Kreslení

        //Kreslení
        private void Draw()
        {
            coordinateSystem.RemoveFunctions();

            MarkLines();

            if (gFunction != null)
            {
                UpdateToMeasure(gFunction);

                if (gFunction.IsDrawable())
                {
                    DrawFunction(gFunction, coordinateSystem.FunctionShift);
                }
                else
                {
                    NotifyError(GetMessageFromID(gFunction.ErrorMessageID));
                }
            }
        }

        //Vykreslení funkce.
        private void DrawFunction(Function function, Space functionShift = new Space())
        {
            function.Plot(function.Inverse, 1, functionShift);

            if (checkBoxKeepOrigin.IsChecked == true) // Při inverzní, jestli má vykreslit původní křivku s nižší viditelností.
            {
                function.Plot(false, 0.3);
            }

            DrawFunctionsLabels();
        }

        //Vykreslení popisků funkcí.
        private void DrawFunctionsLabels()
        {
            int bottomMargin = 35 + (scrollButtonSection.Visibility == Visibility.Visible ? Convert.ToInt16(buttonSection.Height + 15) : 0);
            coordinateSystem.RemoveLabels();
            string[] labelsContent = new string[0];
            Brush[] labelsBrush = new Brush[0];
            GetLabelsContentAndBrush(ref labelsContent, ref labelsBrush);

            if (labelsContent.Length > 0)
            {
                coordinateSystem.DrawFunctionsLabels(labelsContent, labelsBrush, bottomMargin);
            }
        }

        //Naplní informace potřebné k popiskům tedy jejich obsah a barvičku. 
        private void GetLabelsContentAndBrush(ref string[] labelsContent, ref Brush[] labelsBrush)
        {
            List<string> contents = new List<string>(listBoxFunctions.Items.Count + 1);
            List<Brush> brushes = new List<Brush>(listBoxFunctions.Items.Count + 1);

            //Funkce z listu.
            for (int i = 0; i < listBoxFunctions.Items.Count; i++)
            {
                if ((listBoxFunctions.Items[i] as CheckBox)!.IsChecked == true)
                {
                    contents.Add((listBoxFunctions.Items[i] as CheckBox)!.Content.ToString()!);
                    brushes.Add(functions[i].Brush);
                }
            }

            //Funkce z předpisu.
            if (gFunction != null && !contents.Any(s => s.Contains(gFunction!.Name.Split(':')[0])))
            {
                if (checkBoxFreeFunction.IsChecked == true)
                {
                    contents.Add(gFunction.GetIndetificator() + (language == Language.English ? ": Unknown" : ": Neznámý"));
                }
                else
                {
                    contents.Add(gFunction.Name);
                }

                brushes.Add(gFunction.Brush);
            }

            labelsContent = contents.ToArray();
            labelsBrush = brushes.ToArray();
        }


        #endregion

        #region Změna míry

        //Aktualizace komponentů na aktuální míru (pokud se změnila).
        private void UpdateToMeasure(Function function)
        {
            if (previousFunctionType != function.Type)
            {
                UpdateCoordinateSystem(function);

                UpdateRangeMeasure(function.Type);

                SetDegreeLabelsVisibility(function.Type);
            }
        }

        //Pošle informace souřadnicové systém o tom jakou mírou má použit.
        private void UpdateCoordinateSystem(Function function)
        {
            Measure horizontalMeasure = function.Type == Function.FunctionType.TrigonometricFunction ? CoordinateSystem.Measure.Degree : CoordinateSystem.Measure.Numerical;
            Measure verticalMeasure = function.Type == Function.FunctionType.InverseTrigonometricFunction ? CoordinateSystem.Measure.Degree : CoordinateSystem.Measure.Numerical;

            coordinateSystem.Refresh(horizontalMeasure, verticalMeasure);
            previousFunctionType = function.Type;
        }

        //Aktualizuje míru v políčkách pro rozsah.
        private void UpdateRangeMeasure(Function.FunctionType functionType)
        {
            if (functionType == Function.FunctionType.TrigonometricFunction)
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
        private void SetDegreeLabelsVisibility(Function.FunctionType functionType)
        {
            if (functionType == Function.FunctionType.TrigonometricFunction)
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

        #endregion

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
                checkBoxFreeFunction.IsChecked = false;
                Start();
            }

            if (e.Key == Key.Escape)
            {
                equationInput.Text = "";
                equationInput.Focus();
                checkBoxFreeFunction.IsChecked = false;
                Start();
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

            Start();
        }

        #endregion

        #region Responzivita

        //Přizpůsobení prostředí velikosti aplikace.
        private void AdjustComponentsToApplicationSize()
        {
            AdjustCoordinateSystemSize();
            AdjustButtonSectionLayout();
            AdjustHideShowMainMenuButtonMargin();
            coordinateSystem.Refresh();
            Start();
        }

        //Přizpůsobení plátna velikosti aplikace.
        private void AdjustCoordinateSystemSize()
        {
            coordinateSystem.Width = IsMainMenuVisible ? ActualWidth - 400 : ActualWidth;
            coordinateSystem.Height = ActualHeight - 39;

            if (IsMainMenuVisible)
            {
                coordinateSystem.Margin = new Thickness(384, 0, 0, 700 - coordinateSystem.Height);
            }
            else
            {
                coordinateSystem.Margin = new Thickness(0, 0, 400, 700 - coordinateSystem.Height);
            }
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

        //Upravuje pozici tlačítka pro schování hlavního menu
        private void AdjustHideShowMainMenuButtonMargin()
        {
            if (IsMainMenuVisible)
            {
                buttonHideShowMainMenu.Margin = new Thickness(406, 20, 0, 0);
            }
            else
            {
                buttonHideShowMainMenu.Margin = new Thickness(26, 20, 0, 0);
            }
        }

        //Změna velikosti aplikace.
        private void ApplicationResize(object sender, SizeChangedEventArgs e)
        {
            AdjustComponentsToApplicationSize();
        }

        #endregion

        //Povolení tlačítka vykreslit.
        private void EquationInputTextChanged(object sender, TextChangedEventArgs e)
        {
            buttonDraw.IsEnabled = equationInput.Text.Trim() != "";

            if (!buttonDraw.IsEnabled && gFunction != null)
            {
                coordinateSystem.RemoveItem(gFunction.Name);
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
            DrawFunctionsLabels();
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

        enum ListBoxOperations
        {
            Add,
            Remove,
            Deselect,
            Load,
            Save
        }

        //Vyvovále se při kliknutí na jedno s tlačítek spojené s listboxem pro funkce.
        private void ListBoxFunctionOperationButtonsClick(object sender, RoutedEventArgs e)
        {
            Button operationButton = (Button)sender;

            //Získání názvu operace buttoAddFunction -> Add.
            ListBoxOperations operation = (ListBoxOperations)Enum.Parse(typeof(ListBoxOperations), operationButton.Name[6..].Split('F')[0]);

            ListBoxFunctionOperations(operation);
        }

        //Switch s jednotlivými operacemi listboxu.
        private void ListBoxFunctionOperations(ListBoxOperations operation)
        {
            //Vyvolá operaci listboxu na základě typu.
            switch (operation)
            {
                case ListBoxOperations.Add:
                    {
                        AddFunction();
                        break;
                    }
                case ListBoxOperations.Remove:
                    {
                        RemoveFunction();
                        break;
                    }
                case ListBoxOperations.Deselect:
                    {
                        listBoxFunctions.SelectedIndex = -1;
                        break;
                    }
                case ListBoxOperations.Save:
                    {
                        SaveToFile("csv", WriteFunctionsToFile);
                        break;
                    }
                case ListBoxOperations.Load:
                    {
                        LoadFunctions();
                        break;
                    }
            }

            buttonSaveFunctions.IsEnabled = functions.Count > 0;
            checkBoxMultipleFunctions.IsEnabled = functions.Count > 0;
        }

        #region Přidání funkce do listů

        //Přidání funkce do listů pokud je validní.
        private void AddFunction()
        {
            checkBoxFreeFunction.IsChecked = false;
            addingNewFunction = true;
            Start();

            if (gFunction != null)
            {
                AddFunctionToList(gFunction);
            }

            addingNewFunction = false;
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
                IsEnabled = checkBoxMultipleFunctions.IsChecked == true
            };

            checkBox.Checked += CheckBoxFunctionCheckedChanged;
            checkBox.Unchecked += CheckBoxFunctionCheckedChanged;

            listBoxFunctions.Items.Add(checkBox);
        }

        #endregion

        #region Odebrání funkce z listu

        //Odebrání funkce z listů.
        private void RemoveFunction()
        {
            if (listBoxFunctions.SelectedIndex != -1)
            {
                functions.RemoveAt(listBoxFunctions.SelectedIndex);
                listBoxFunctions.Items.RemoveAt(listBoxFunctions.SelectedIndex);
            }
        }

        #endregion

        #region Uložení funkcí do souboru

        //Uložení funkcí do souboru.
        private void SaveToFile(string fileType, Action<string> WritingMethod)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog()
            {
                Filter = $"{fileType.ToUpper()} files | *.{fileType}"
            };

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                WritingMethod(saveFileDialog.FileName);
            }
        }

        //Zápis funkcí do souboru.
        private void WriteFunctionsToFile(string path)
        {
            string lines = "Name;Color;Relation;Is limited;Minimum;Maximum;Is inverse;Type;\r\n";

            for (int i = 0; i < functions.Count; i++)
            {
                lines += $"{functions[i].Name};{functions[i].Brush};{functions[i].InputRelation};{functions[i].IsLimited};{functions[i].MinimumX};{functions[i].MaximumX};{functions[i].Inverse};{functions[i].Type};\r\n";
            }

            File.WriteAllText(path, lines);
        }

        #endregion

        #region Čtení funkcí ze souboru

        //Dialog pro souboru na načtení.
        private void LoadFunctions()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog()
            {
                Filter = "CSV files | *.csv"
            };

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadFunctionsFromFile(openFileDialog.FileName);
            }

            listBoxFunctions.SelectedIndex = listBoxFunctions.Items.Count - 1;
        }

        //Načte funkce ze souboru.
        private void LoadFunctionsFromFile(string path)
        {
            FileCompiler file = new FileCompiler(path, true);

            if (file.IsOK)
            {
                file.Read();

                if (file.IsDataOK)
                {
                    ReadFunctionsFromFile(file.Data);
                }
                else
                {
                    NotifyError(file.ErrorMessage);
                }
            }
            else
            {
                NotifyError(file.ErrorMessage);
            }
        }

        //Přečte načtené funkce ze souboru.;
        private void ReadFunctionsFromFile(List<string> functionsFromFile)
        {
            for (int i = 0; i < functionsFromFile.Count; i++)
            {
                LoadUserInterfaceValues(functionsFromFile[i].Split(';'));
                AddFunction();
            }

            if (functions.Count != functionsFromFile.Count)
            {
                MessageBox.Show(messages[32].Split(';')[(int)language]);
            }
        }

        #endregion

        //Přepsání vybrané funkce v listboxu.
        private void UpdateFunctionInList(Function function)
        {
            functions[listBoxFunctions.SelectedIndex] = function;

            if (checkBoxFreeFunction.IsChecked == false)
            {
                (listBoxFunctions.Items[listBoxFunctions.SelectedIndex] as CheckBox)!.Content = function.Name;
            }
        }

        //Změna vybrané funkce v listboxu.
        private void ListBoxFunctionsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBoxFunctions.SelectedIndex != -1)
            {
                gFunction = functions[listBoxFunctions.SelectedIndex];
                LoadUserInterfaceValues(gFunction.PropertiesValueToArray());
                checkBoxFreeFunction.IsChecked = false;
                Start();
            }
        }

        //Načtení vlastností funkce do uživatelských prvků.
        private void LoadUserInterfaceValues(string[] data)
        {
            rectangleColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(data[1])); // Barva

            equationInput.Text = data[2]; // Předpis

            limitX.IsChecked = bool.Parse(data[3]); // Omezená

            Function.FunctionType functionType = (Function.FunctionType)Enum.Parse(typeof(Function.FunctionType), data[7]);

            if (limitX.IsChecked == true)
            {
                UpdateRangeMeasure(functionType);

                if (functionType == Function.FunctionType.TrigonometricFunction)
                {
                    data[4] = data[4].ToDegrees();
                    data[5] = data[5].ToDegrees();
                }

                minimumXInput.Text = data[4]; // Minimum
                maximumXInput.Text = data[5]; // Maximum
            }

            checkBoxInverse.IsChecked = bool.Parse(data[6]); // Inverzní
        }

        #endregion

        #region Metody pro více funkcí najednou

        //Událost při změny hodnoty zaškrtnutí více funkcí najednou.
        private void CheckBoxMultipleFunctionsCheckedChanged(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < functions.Count; i++)
            {
                (listBoxFunctions.Items[i] as CheckBox)!.IsEnabled = checkBoxMultipleFunctions.IsChecked == true;

                if (checkBoxMultipleFunctions.IsChecked == false)
                {
                    (listBoxFunctions.Items[i] as CheckBox)!.IsChecked = false;
                }
            }

            coordinateSystem.RemoveFunctions();

            Start();
        }

        //Když se změní hodnota zaškrtnutí nějakého checkboxu v listu.
        private void CheckBoxFunctionCheckedChanged(object sender, RoutedEventArgs e)
        {
            CheckBox checkBoxFunction = (sender as CheckBox)!;

            int index = GetCheckBoxIndex(checkBoxFunction.Content.ToString()!);

            if (checkBoxFunction.IsChecked == false)
            {
                coordinateSystem.RemoveItem(functions[index].Name);
            }

            UpdateToMeasure(functions[index]);

            Start();
        }

        //Získá index právě zaškrtlého checkboxu.
        private int GetCheckBoxIndex(string content)
        {
            int index = 0;

            for (int i = 0; i < listBoxFunctions.Items.Count; i++)
            {
                if ((listBoxFunctions.Items[i] as CheckBox)!.Content.ToString() == content)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        //Přepočítá zaškrnuté funkce k aktuálním hodnotám.
        private void Recalculation()
        {
            for (int i = 0; i < functions.Count; i++)
            {
                if ((listBoxFunctions.Items[i] as CheckBox)!.IsChecked == true && !coordinateSystem.ContainsFunction(functions[i].Name))
                {
                    functions[i].CalculatePoints();
                    DrawFunction(functions[i]);
                }
            }
        }

        #endregion

        //Událost při kliku na tlačítko, které ovládá viditelnost hlavního menu.
        private void ButtonHideShowMainMenuClick(object sender, RoutedEventArgs e)
        {
            IsMainMenuVisible = !IsMainMenuVisible;

            buttonHideShowMainMenu.Content = IsMainMenuVisible ? "🢦" : "🢧";

            if (!IsMainMenuVisible)
            {
                scrollButtonSection.Visibility = Visibility.Collapsed;
                markLineSection.Visibility = Visibility.Collapsed;
            }

            AdjustComponentsToApplicationSize();
        }

        //Změna zaškrnutní u políčka jestli je funkce uvolněná.
        private void CheckBoxFreeFunctionChanged(object sender, RoutedEventArgs e)
        {
            if (checkBoxFreeFunction.IsChecked == true)
            {
                equationInput.Foreground = equationInput.Background;

                if (listBoxFunctions.SelectedIndex != -1)
                {
                    hiddenRelationIndex = listBoxFunctions.SelectedIndex;
                    (listBoxFunctions.Items[hiddenRelationIndex] as CheckBox)!.Content = functions[hiddenRelationIndex].Name.Split(':')[0] + (language == Language.English ? ": Unknown" : ": Neznámý");
                }
                else
                {
                    hiddenRelationIndex = -1;
                }
            }
            else
            {
                equationInput.Foreground = Brushes.Black;
                coordinateSystem.FunctionShift = new Space();

                if (hiddenRelationIndex != -1)
                {
                    (listBoxFunctions.Items[hiddenRelationIndex] as CheckBox)!.Content = functions[hiddenRelationIndex].Name;
                }
            }
        }

        //Událost, která se vyvolá při změně textu v políčkách pro označení posunutí.
        private void InputMarkLineTextChanged(object sender, TextChangedEventArgs e)
        {
            MarkLines();

            if (inputMarkLineY.Text == "")
            {
                coordinateSystem.RemoveItem("markLineY");
            }

            if (inputMarkLineX.Text == "")
            {
                coordinateSystem.RemoveItem("markLineX");
            }
        }

        //Vyznačení posunutí.
        private void MarkLines()
        {
            if (inputMarkLineY.IsValid)
            {
                double y = (-inputMarkLineY.Value * coordinateSystem.Zoom * 100) + coordinateSystem.Height / 2 + coordinateSystem.AbsoluteShift.OnY;
                coordinateSystem.RemoveItem("markLineY");
                coordinateSystem.MarkLine(0, y, coordinateSystem.Width, y, "markLineY");
            }

            if (inputMarkLineX.IsValid)
            {
                double x = coordinateSystem.Width / 2 + (inputMarkLineX.Value * coordinateSystem.Zoom * 100) + coordinateSystem.AbsoluteShift.OnX;
                coordinateSystem.RemoveItem("markLineX");
                coordinateSystem.MarkLine(x, 0, x, coordinateSystem.Height, "markLineX");
            }
        }

        //Ovládání viditelnosti boxu pro ovládání vyznačení posunutí.
        private void ButtonShowHideMarkSettingsClick(object sender, RoutedEventArgs e)
        {
            markLineSection.Visibility = markLineSection.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        //Kliknutí na tlačítko pro uložení aktuálního stavu plátna.
        private void ButtonExportCoordinateSystemClick(object sender, RoutedEventArgs e)
        {
            SaveToFile("png", ExportCoordinateSystem);
        }

        private void ExportCoordinateSystem(string path)
        {
            CroppedBitmap bitmap = CoordinateSystemToBitmap();

            ExportBitmap(path, bitmap);
        }

        //Vytvoří bitmapu canvasu.
        private CroppedBitmap CoordinateSystemToBitmap()
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)Width, (int)Height, 96, 96, PixelFormats.Pbgra32);

            bitmap.Render(coordinateSystem);

            return new CroppedBitmap(bitmap, new Int32Rect((int)coordinateSystem.Margin.Left, 0, (int)coordinateSystem.Width, (int)coordinateSystem.Height));
        }

        //Uloží canvas do souboru png.
        private void ExportBitmap(string path, CroppedBitmap bitmap)
        {
            BitmapEncoder pngEncoder = new PngBitmapEncoder();

            pngEncoder.Frames.Add(BitmapFrame.Create(bitmap));

            pngEncoder.Save(File.OpenWrite(path));
        }
    }
}
