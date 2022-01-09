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
            Width = 700;
            Height = 700;
            Create();
        }

        public event EventHandler? AbsoluteShiftChanged; // Event který nastane při změne posunu.

        public int ZoomLevel { get; set; } = 0; // Slouží pro výpočet zoomu a zároveň celočíselný vyjádření zoomu.

        public double Zoom { get; private set; } = 1; // Zoom pro násobek mezer na základě přiblížení.

        public double NumberRange { get; private set; } = 3.5;

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

        private Space previousAbsoluteShift = new Space(0, 0); // Absolutní posunít před započetím pohybu v soustavě.

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
            DrawGrid();
            DrawNumbers();

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

        //Metoda pro zachycení skrolování.
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            absoluteShift = AdjustAbsoluteShiftToZoom(e.Delta);
            ZoomLevel = GetZoomLevel(e.Delta);
            SetValues();
            Create();
        }

        //Zmáčknutí levého tlačíka na myši.
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            isMouseDown = true;
            mouseDownPosition = e.GetPosition(this);
            previousAbsoluteShift = AbsoluteShift;
        }

        //Resetování posunutí.
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                AbsoluteShift = new Space(0, 0);
            }
        }

        //Pohyb myši.
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (isMouseDown)
            {
                Point mousePosition = e.GetPosition(this);

                AbsoluteShift = new Space(previousAbsoluteShift.OnX - (mouseDownPosition.X - mousePosition.X), (previousAbsoluteShift.OnY - (mouseDownPosition.Y - mousePosition.Y)));
            }
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

            return new Space(xSpace, ySpace);
        }

        //Úprava hodnoty pusuní při změne zoomu.
        private Space AdjustAbsoluteShiftToZoom(int delta)
        {
            double absoluteShiftX = absoluteShift.OnX;
            double absoluteShiftY = absoluteShift.OnY;

            if (delta == 120 && ZoomLevel < 4)
            {
                absoluteShiftX *= 1.25;
                absoluteShiftY *= 1.25;
            }

            if (delta == -120 && ZoomLevel > -4)
            {
                absoluteShiftX /= 1.25;
                absoluteShiftY /= 1.25;
            }
            return new Space(absoluteShiftX, absoluteShiftY);
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
            return currentValue + (currentMeasure == Measure.Degree ? 60 : 1);
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
            Line axisX = NewLine(Direction.X, lineX: Width / 2 + AbsoluteShift.OnX, strokeThickness: 1.5);

            Line axisY = NewLine(Direction.Y, lineY: Height / 2 + AbsoluteShift.OnY, strokeThickness: 1.5);

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
