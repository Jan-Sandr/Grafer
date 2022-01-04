using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Grafer.CustomControls
{
    /// <summary>
    /// Interaction logic for CoordinateSystem.xaml
    /// </summary>
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

        public int ZoomLevel { get; set; } = 0; // Slouží pro výpočet zoomu a zároveň celočíselný vyjádření zoomu.

        public double Zoom { get; private set; } = 1; // Zoom pro násobek mezer na základě přiblížení.

        public double NumberRange { get; private set; } = 3.5;

        private Space spaceBetween = new Space(100, 100); // Mezera mezi mřížními přímkami.

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
            ZoomLevel = GetZoomLevel(e.Delta);
            SetValues();
            Create();
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
            int startNumber = 0;

            double increment = GetGridNumberIncrement(measure, 0); // Popiskové rozdíly.

            Point startPoint = GetStartPoint(direction); // Nulový bod

            double space = direction == Direction.X ? spaceBetween.OnX : spaceBetween.OnY;

            for (double i = space; i < size / 2; i += space)
            {
                double xNumberShift = (measure == Measure.Degree ? -5 : 0) - (startNumber + increment).ToString().Length;

                switch (direction)
                {
                    case Direction.X:
                        {
                            Children.Add(NewNumber(startNumber - increment, startPoint.X - i - 4 + xNumberShift, startPoint.Y, measure));
                            Children.Add(NewNumber(startNumber + increment, startPoint.X + i + xNumberShift, startPoint.Y, measure));
                            break;
                        }

                    case Direction.Y:
                        {
                            Children.Add(NewNumber(startNumber + increment, startPoint.X + 4, startPoint.Y - i, measure));
                            Children.Add(NewNumber(startNumber - increment, startPoint.X, startPoint.Y + i - ((ZoomLevel == -4) ? 1 : 0), measure));
                            break;
                        }
                }

                increment = GetGridNumberIncrement(measure, increment);
            }

            if (direction == Direction.X)
            {
                Children.Add(NewNumber(0, Width / 2 + 15, Height / 2 + 10, measure));
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

            for (double i = space; i < size / 2; i += space)
            {
                switch (direction)
                {
                    case Direction.X:
                        {
                            Children.Add(NewLine(direction, lineX: (size / 2) - i, brushes: brush));
                            Children.Add(NewLine(direction, lineX: (size / 2) + i, brushes: brush));
                            break;
                        }

                    case Direction.Y:
                        {
                            Children.Add(NewLine(direction, lineY: (size / 2) - i, brushes: brush));
                            Children.Add(NewLine(direction, lineY: (size / 2) + i, brushes: brush));
                            break;
                        }
                }
            }
        }

        //Vykreslení os.
        private void DrawAxes()
        {
            Line axisX = NewLine(Direction.X, lineX: Width / 2, strokeThickness: 1.5);

            Line axisY = NewLine(Direction.Y, lineY: Height / 2, strokeThickness: 1.5);

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
