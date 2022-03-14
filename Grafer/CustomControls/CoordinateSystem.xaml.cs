using Grafer.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Grafer.CustomControls
{
    public partial class CoordinateSystem : Canvas
    {
        //Výška a šířka nejsou ještě v  této metodě čísla, proto jim zde přiřazuji výchozí hodnotu.
        public CoordinateSystem()
        {
            InitializeComponent();
            Width = 780;
            Height = 770;
            Create();
        }

        public event EventHandler? AbsoluteShiftChanged; // Event který nastane při změne posunu soustavy.

        public event EventHandler? FunctionShiftChanged; // Event který nastane při změně posunu funkce.

        public int ZoomLevel // Slouží pro výpočet zoomu a zároveň celočíselný vyjádření zoomu.
        {
            get
            {
                return zoomLevel;
            }
            set
            {
                int delta = value - zoomLevel;

                absoluteShift = AdjustShiftToZoom(delta, absoluteShift);
                functionShift = AdjustShiftToZoom(delta, functionShift);
                zoomLevel = value;
                SetValues();
                Create();
            }
        }

        private int zoomLevel = 0;

        public double Zoom { get; private set; } = 1; // Zoom pro násobek mezer na základě přiblížení.

        public double NumberRange { get; private set; } = 3.5;

        public bool AreGridLabelsVisible { get; set; } = true;

        public bool AreGridLinesVisible { get; set; } = true;

        private Space absoluteShift = new Space(0, 0);

        public Space AbsoluteShift
        {
            get
            {
                return absoluteShift;
            }
            private set
            {
                absoluteShift = value;
                Refresh();
                AbsoluteShiftChanged?.Invoke(this, EventArgs.Empty); // Aby bylo vyvolání eventu dostupné i mimo třídu.
            }
        }

        private Space functionShift = new Space(0, 0);

        public Space FunctionShift
        {
            get
            {
                return functionShift;
            }
            set
            {
                functionShift = value;
                FunctionShiftChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool ShowPointer
        {
            get
            {
                return showPointer;
            }
            set
            {
                showPointer = value;

                pointer.Visibility = showPointer ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private bool showPointer = false;

        private TextBlock pointer = new TextBlock();

        public bool IsFunctionShiftEnabled { get; set; } = false;

        private Space previousAbsoluteShift = new Space(0, 0); // Absolutní posunutí před započetím pohybu v soustavě.

        private Space previousFunctionShift = new Space(0, 0); // Minulé posunutí funkce před započetím pohybu v soustavě

        private Space spaceBetween = new Space(100, 100); // Mezera mezi mřížními přímkami.

        private Point mouseDownPosition = new Point(0, 0); // Pozice kliknutí myši.

        bool isMouseDown = false; // Jestli je myší tlačíko dole.

        private enum Direction // Směr pro vykreslování.
        {
            X,
            Y
        }

        public new enum Measure
        {
            Numerical,
            Degree
        }

        private readonly Dictionary<Measure, double> defaultMeasureSpace = new Dictionary<Measure, double>()
        {
            {  Measure.Numerical, 100 },
            {  Measure.Degree, (Math.PI / 3) * 100 }
        };

        private Measure xMeasure = Measure.Numerical;
        private Measure yMeasure = Measure.Numerical;

        private int defaultElementsCount; // Počet vnitřně přidáných dětí - mřížka a popisky.

        //Vytvoření soustavy.
        private void Create()
        {
            Children.Clear();

            DrawAxes();

            if (AreGridLinesVisible)
            {
                DrawGrid();
            }

            if (AreGridLabelsVisible)
            {
                DrawNumbers();
            }

            pointer = new TextBlock()
            {
                Text = "[0;0]",
                FontFamily = new FontFamily("Cambria"),
                FontSize = 20,
                Foreground = Brushes.Navy,
                RenderTransform = previousPointerPosition,
                Visibility = showPointer == true ? Visibility.Visible : Visibility.Collapsed,
            };

            Children.Add(pointer);

            defaultElementsCount = Children.Count;
        }

        //Překreslení soustavy.
        public void Refresh()
        {
            SetValues();
            Create();
        }

        //Překreslení soustavy se změnami v míře.
        public void Refresh(Measure horizontalMeasure, Measure verticalMeasure)
        {
            xMeasure = horizontalMeasure;
            yMeasure = verticalMeasure;

            SetValues();
            Create();
        }

        //Odebrání funkcí.
        public void RemoveFunctions()
        {
            Children.RemoveRange(defaultElementsCount, Children.Count - defaultElementsCount);
        }

        //Odebere všechny popisky funkcí.
        public void RemoveLabels()
        {
            for (int i = defaultElementsCount; i < Children.Count; i++)
            {
                UIElement uIElement = Children[i];

                if (uIElement!.Uid.Contains("Label"))
                {
                    Children.Remove(uIElement);
                    i--;
                }
            }
        }

        //Odebere konkrétné funkci na základě Uid.
        public void RemoveItem(string name)
        {
            for (int i = defaultElementsCount; i < Children.Count; i++)
            {
                UIElement uIElement = Children[i];

                if (uIElement!.Uid == name)
                {
                    Children.Remove(uIElement);
                    i--;
                }
            }
        }

        //Jestli už funkci systém obsahuje na základě Uid.
        public bool ContainsFunction(string name)
        {
            bool result = false;

            for (int i = defaultElementsCount; i < Children.Count; i++)
            {
                UIElement uIElement = Children[i];

                if (uIElement!.Uid == name)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        //Vykreslení popisků funkcí.
        public void DrawFunctionsLabels(string[] contents, Brush[] brushes, int baseBottomMargin)
        {
            int bottomMargin = baseBottomMargin;

            for (int i = 0; i < contents.Length; i++)
            {
                TextBlock functionLabel = FunctionLabel(contents[i], brushes[i], bottomMargin);
                Children.Add(functionLabel);
                bottomMargin += 35;
            }
        }

        //Vyznačení posunutí na osách.
        public void MarkLine(double x1, double y1, double x2, double y2, string name)
        {
            Line markLine = new Line()
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = Brushes.DarkGoldenrod,
                StrokeThickness = 3,
                StrokeDashArray = new DoubleCollection() { 3, 3 },
                Uid = name
            };

            Children.Add(markLine);
        }

        //Šablona pro popisek funkce.
        private TextBlock FunctionLabel(string content, Brush brush, int bottomMargin)
        {
            return new TextBlock()
            {
                Text = content,
                FontFamily = new FontFamily("Cambria"),
                FontSize = 25,
                Foreground = brush,
                RenderTransform = new TranslateTransform()
                {
                    X = 20,
                    Y = Height - bottomMargin
                },
                Uid = content + " Label"
            };
        }

        //Metoda pro zachycení skrolování.
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            ZoomLevel = GetZoomLevel(e.Delta);
        }

        //Resetování posunutí.
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            isMouseDown = true;
            mouseDownPosition = e.GetPosition(this);

            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                AbsoluteShift = new Space(0, 0);
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                previousAbsoluteShift = AbsoluteShift;
            }

            if (e.RightButton == MouseButtonState.Pressed && IsFunctionShiftEnabled)
            {
                previousFunctionShift = FunctionShift;
            }
        }

        //Pohyb myši.
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (isMouseDown)
            {
                Point mousePosition = e.GetPosition(this);

                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    AbsoluteShift = new Space(previousAbsoluteShift.OnX - (mouseDownPosition.X - mousePosition.X), previousAbsoluteShift.OnY - (mouseDownPosition.Y - mousePosition.Y));
                }

                if (e.RightButton == MouseButtonState.Pressed && IsFunctionShiftEnabled)
                {
                    FunctionShift = new Space(previousFunctionShift.OnX - (mouseDownPosition.X - mousePosition.X), previousFunctionShift.OnY - (mouseDownPosition.Y - mousePosition.Y));
                }
            }

            if (ShowPointer)
            {
                SetPointer(e);
            }
        }

        private Transform previousPointerPosition = new TranslateTransform(0, 0);

        //Natavení pozice ukazovátka
        private void SetPointer(MouseEventArgs e)
        {
            double cursorX = e.GetPosition(this).X;
            double cursorY = e.GetPosition(this).Y;

            if (Width - cursorX < 120) // Pokud je kurzor hodně vpravo, tak se zobrazí na druhé straně kurzoru.
            {
                cursorX -= 100;
            }

            if (cursorY < 30) // POkud je kurzor hodně nahoře, tak se zobrazí pod kurzorem.
            {
                cursorY += 50;
            }

            pointer.RenderTransform = new TranslateTransform() // Posunutí, aby to nepřekrýval kurzor.
            {
                X = cursorX + 10,
                Y = cursorY - 25
            };

            previousPointerPosition = new TranslateTransform()
            {
                X = e.GetPosition(this).X,
                Y = e.GetPosition(this).Y
            };

            pointer.Text = GetCursorCoordinates();
        }

        //Získání souřadnici kurozru pro zobrazení.
        private string GetCursorCoordinates()
        {
            string cursorX = Math.Round((previousPointerPosition.Value.OffsetX - Width / 2 - AbsoluteShift.OnX) / Zoom / 100, 2).ToString();
            string cursorY = Math.Round(-(previousPointerPosition.Value.OffsetY - Height / 2 - AbsoluteShift.OnY) / Zoom / 100, 2).ToString();

            if (xMeasure == Measure.Degree)
            {
                cursorX = Math.Round(double.Parse(cursorX.ToDegrees()), 0) + "°";
            }

            if (yMeasure == Measure.Degree)
            {
                cursorY = Math.Round(double.Parse(cursorY.ToDegrees()), 0) + "°";
            }

            return $"[{cursorX};{cursorY}]";
        }

        //Úvolnění levého tlačítka myši.
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            isMouseDown = false;
        }

        //Když myš opustí soustavu.
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            isMouseDown = false;

            pointer.Visibility = Visibility.Collapsed;
        }

        //Když myš vstupí do soustavy.
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (showPointer)
            {
                pointer.Visibility = Visibility.Visible;
            }
        }

        //Nastavení zoomLevel maximální je absolutní 4.
        private int GetZoomLevel(int delta)
        {
            int nextLevel = delta > 0 ? ZoomLevel + 1 : ZoomLevel - 1;

            return Math.Abs(nextLevel) > 4 ? ZoomLevel : nextLevel;
        }

        //Nastavení hodnot pro výpočet.
        private void SetValues()
        {
            Zoom = Math.Pow(1.25, ZoomLevel);

            spaceBetween = GetSpaceBetween();

            NumberRange = (Width / 200) / Zoom;
        }

        //Získá mezery mezi hodnotami na základě míry.
        private Space GetSpaceBetween()
        {
            double xSpace = defaultMeasureSpace[xMeasure] * Zoom;
            double ySpace = defaultMeasureSpace[yMeasure] * Zoom;

            if (ZoomLevel < -2)
            {
                xSpace *= 2;
                ySpace *= 2;
            }

            if (ZoomLevel > 2)
            {
                xSpace /= 2;
                ySpace /= 2;
            }

            return new Space(xSpace, ySpace);
        }

        //Úprava hodnoty pusunutí při změne zoomu.
        private Space AdjustShiftToZoom(int delta, Space inputShift)
        {
            double newShiftX = inputShift.OnX;
            double newShiftY = inputShift.OnY;

            if (delta > 0 && ZoomLevel < 4)
            {
                newShiftX *= 1.25;
                newShiftY *= 1.25;
            }

            if (delta < 0 && ZoomLevel > -4)
            {
                newShiftX /= 1.25;
                newShiftY /= 1.25;
            }

            return new Space(newShiftX, newShiftY);
        }

        //Vykreslení mřížky.
        private void DrawGrid()
        {
            DrawGridLines(Direction.X, Width);
            DrawGridLines(Direction.Y, Height);
        }

        //Vykreslení čísel.
        private void DrawNumbers()
        {
            DrawGridNumbers(Direction.X, Width, xMeasure);
            DrawGridNumbers(Direction.Y, Height, yMeasure);
        }

        //Vykreslení čísel pro určitý směr.
        private void DrawGridNumbers(Direction direction, double size, Measure measure)
        {
            double increment = GetGridNumberIncrement(measure, 0); // Popiskové rozdíly.

            Point startPoint = GetStartPoint(direction); // Nulový bod

            double space = direction == Direction.X ? spaceBetween.OnX : spaceBetween.OnY;

            double sideWidth = size / 2 + Math.Abs(direction == Direction.X ? AbsoluteShift.OnX : AbsoluteShift.OnY);

            for (double i = space; i < sideWidth; i += space)
            {
                double xNumberShift = (measure == Measure.Degree ? -5 : 0) - increment.ToString().Length;

                switch (direction)
                {
                    case Direction.X:
                        {
                            if (Width / 2 + AbsoluteShift.OnX - i > 0 && startPoint.X - i - 4 + xNumberShift + AbsoluteShift.OnX < Width)
                            {
                                Children.Add(NewNumber(-increment, startPoint.X - i - 4 + xNumberShift + AbsoluteShift.OnX, startPoint.Y + AbsoluteShift.OnY, measure));
                            }

                            if (Width / 2 + AbsoluteShift.OnX + i < Width && startPoint.X + i + xNumberShift + AbsoluteShift.OnX > 0)
                            {
                                Children.Add(NewNumber(increment, startPoint.X + i + xNumberShift + AbsoluteShift.OnX, startPoint.Y + AbsoluteShift.OnY, measure));
                            }

                            break;
                        }

                    case Direction.Y:
                        {
                            if (Height / 2 + AbsoluteShift.OnY - i > 0)
                            {
                                Children.Add(NewNumber(increment, startPoint.X + 4 + AbsoluteShift.OnX, startPoint.Y - i + AbsoluteShift.OnY, measure));
                            }

                            if (Height / 2 + AbsoluteShift.OnY + i < Height)
                            {
                                Children.Add(NewNumber(-increment, startPoint.X + AbsoluteShift.OnX, startPoint.Y + i - ((ZoomLevel == -4) ? 1 : 0) + AbsoluteShift.OnY, measure));
                            }

                            break;
                        }
                }

                increment = GetGridNumberIncrement(measure, increment);
            }

            if (direction == Direction.X)
            {
                Children.Add(NewNumber(0, Width / 2 + 15 + AbsoluteShift.OnX, Height / 2 + 10 + AbsoluteShift.OnY, measure));
            }
        }

        //Přičítá popisnou hodnotu na základě míry.
        private double GetGridNumberIncrement(Measure currentMeasure, double currentValue)
        {
            double increment = currentMeasure == Measure.Degree ? 60 : 1;

            if (ZoomLevel > 2)
            {
                increment /= 2;
            }

            if (ZoomLevel < -2)
            {
                increment *= 2;
            }

            return currentValue + increment;
        }

        //Získání vychozí pozice - střed vykreslování.
        private Point GetStartPoint(Direction direction)
        {
            Point startPoint = GetDefaultStartPoint(direction);

            if (ZoomLevel == -4) // Drobné posunutí, když je přiblížení na maximu.
            {
                startPoint = new Point()
                {
                    X = (direction == Direction.X) ? startPoint.X - 1 : startPoint.X,
                    Y = (direction == Direction.X) ? startPoint.Y : startPoint.Y
                };
            }

            return startPoint;
        }

        //Výchozí popisková pozice.
        private Point GetDefaultStartPoint(Direction direction)
        {
            return new Point()
            {
                X = (direction == Direction.X) ? Width / 2 - 3 : Width / 2 + 10,
                Y = (direction == Direction.X) ? Height / 2 + 10 : Height / 2 - 7
            };
        }

        //Vytvoření čísla.
        private static TextBlock NewNumber(double value, double x, double y, Measure measure)
        {
            string addition = measure == Measure.Degree ? "°" : "";

            TextBlock number = DefaultTextBlock(value.ToString() + addition);

            number.RenderTransform = new TranslateTransform()
            {
                X = x,
                Y = y
            };

            return number;
        }

        //Defaultní textblok.
        private static TextBlock DefaultTextBlock(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontFamily = new FontFamily("Arial"),
                FontSize = 13
            };
        }

        //Vykreslení mřížky pro určitý směr.
        private void DrawGridLines(Direction direction, double size)
        {
            SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(75, 0, 0, 0));

            double space = direction == Direction.X ? spaceBetween.OnX : spaceBetween.OnY;

            Children.Add(NewLine(direction, lineX: (size / 2) + AbsoluteShift.OnX % space, brushes: brush));
            Children.Add(NewLine(direction, lineY: (size / 2) + AbsoluteShift.OnY % space, brushes: brush));

            double sideWidth = size / 2 + Math.Abs((direction == Direction.X ? AbsoluteShift.OnX % space : AbsoluteShift.OnY % space));

            for (double i = space; i < sideWidth; i += space)
            {
                switch (direction)
                {
                    case Direction.X:
                        {
                            Children.Add(NewLine(direction, lineX: (size / 2) - i + AbsoluteShift.OnX % space, brushes: brush));
                            Children.Add(NewLine(direction, lineX: (size / 2) + i + AbsoluteShift.OnX % space, brushes: brush));
                            break;
                        }

                    case Direction.Y:
                        {
                            Children.Add(NewLine(direction, lineY: (size / 2) - i + AbsoluteShift.OnY % space, brushes: brush));
                            Children.Add(NewLine(direction, lineY: (size / 2) + i + AbsoluteShift.OnY % space, brushes: brush));
                            break;
                        }
                }
            }
        }

        //Vykreslení os.
        private void DrawAxes()
        {
            SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(175, 0, 0, 0));

            Line axisX = NewLine(Direction.X, lineX: Width / 2 + AbsoluteShift.OnX, brushes: brush, strokeThickness: 2);
            Line axisY = NewLine(Direction.Y, lineY: Height / 2 + AbsoluteShift.OnY, brushes: brush, strokeThickness: 2);

            Children.Add(axisX);
            Children.Add(axisY);
        }

        //Vytvoření nové úsečky.
        private Line NewLine(Direction direction, double lineX = 0, double lineY = 0, SolidColorBrush? brushes = null, double strokeThickness = 1)
        {
            Line line = new Line()
            {
                X1 = (direction == Direction.X) ? lineX : 0,
                Y1 = (direction == Direction.X) ? 0 : lineY,
                X2 = (direction == Direction.X) ? lineX : Width,
                Y2 = (direction == Direction.X) ? Height : lineY,
                Stroke = brushes ?? Brushes.Black,
                StrokeThickness = strokeThickness,
            };

            line.SnapsToDevicePixels = true;

            return line;
        }
    }

    //Struktura mezera - stejné jako Point akorát to je logické, protože bod není mezera.
    public struct Space
    {
        public readonly double OnX;
        public readonly double OnY;

        public Space(double onX, double onY)
        {
            OnX = onX;
            OnY = onY;
        }
    }
}
