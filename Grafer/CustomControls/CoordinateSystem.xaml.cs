using System;
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

        private double space = 100; // Mezera mezi mřížními přímkami.

        private enum Direction // Směr pro vykreslování.
        {
            X,
            Y
        }

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

        // Překreslení soustavy.
        public void Refresh()
        {
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
            space = 100 * Zoom;
            NumberRange = (Width / 200) / Zoom;
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
            DrawGridNumbers(Direction.X, Width);
            DrawGridNumbers(Direction.Y, Height);
        }

        //Vykreslení čísel pro určitý směr.
        private void DrawGridNumbers(Direction direction, double size)
        {
            int startNumber = 0;
            int increment = 1;

            Point startPoint = GetStartPoint(direction);

            for (double i = space; i < size / 2; i += space)
            {
                switch (direction)
                {
                    case Direction.X:
                        {
                            Children.Add(NewNumber(startNumber - increment, startPoint.X - i - 4, startPoint.Y));
                            Children.Add(NewNumber(startNumber + increment, startPoint.X + i, startPoint.Y));
                            break;
                        }

                    case Direction.Y:
                        {
                            Children.Add(NewNumber(startNumber + increment, startPoint.X + 4, startPoint.Y - i));
                            Children.Add(NewNumber(startNumber - increment, startPoint.X, startPoint.Y + i - ((ZoomLevel == -4) ? 1 : 0)));
                            break;
                        }
                }

                increment++;
            }

            if (direction == Direction.X)
            {
                Children.Add(NewNumber(0, Width / 2 + 15, Height / 2 + 10));
            }
        }

        private Point GetStartPoint(Direction direction)
        {
            Point startPoint = GetDefaultStartPoint(direction);

            if (ZoomLevel == -4)
            {
                startPoint = new Point()
                {
                    X = (direction == Direction.X) ? startPoint.X - 1 : startPoint.X,
                    Y = (direction == Direction.X) ? startPoint.Y : startPoint.Y
                };
            }

            return startPoint;
        }

        private Point GetDefaultStartPoint(Direction direction)
        {
            return new Point()
            {
                X = (direction == Direction.X) ? Width / 2 - 3 : Width / 2 + 10,
                Y = (direction == Direction.X) ? Height / 2 + 10 : Height / 2 - 7
            };
        }

        //Vytvoření čísla.
        private static TextBlock NewNumber(double value, double x, double y)
        {
            TextBlock number = DefaultTextBlock(value.ToString());

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
}
