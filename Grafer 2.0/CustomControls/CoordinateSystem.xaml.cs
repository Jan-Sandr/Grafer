using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Grafer2.CustomControls
{
    /// <summary>
    /// Interaction logic for CoordinateSystem.xaml
    /// </summary>
    public partial class CoordinateSystem : Canvas
    {
        public CoordinateSystem()
        {
            InitializeComponent();
        }

        private enum Direction
        {
            X,
            Y
        }

        private int defaultElementsCount;

        //Vytvoření soustavy.
        public void Create()
        {
            Children.Clear();
            DrawAxes();
            DrawGrid();
            DrawNumbers();

            defaultElementsCount = Children.Count;
        }

        //Odebrání funkcí.
        public void RemoveFunctions()
        {
            Children.RemoveRange(defaultElementsCount, Children.Count - defaultElementsCount);
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
            for (int i = 100; i < size / 2; i += 100)
            {
                switch (direction)
                {
                    case Direction.X:
                        {
                            Children.Add(NewNumber(-i / 100, (size / 2) - i - 7, Height / 2 + 10));
                            Children.Add(NewNumber(i / 100, (size / 2) + i - 3, Height / 2 + 10));
                            break;
                        }

                    case Direction.Y:
                        {
                            Children.Add(NewNumber(i / 100, Width / 2 + 16, (size / 2) - i - 7));
                            Children.Add(NewNumber(-i / 100, Width / 2 + 10, (size / 2) + i - 7));
                            break;
                        }
                }
            }

            if (direction == Direction.X)
            {
                Children.Add(NewNumber(0, Width / 2 + 16, Height / 2 + 10));
            }

        }

        //Vytvoření čísla.
        private static TextBlock NewNumber(int value, double x, double y)
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
            SolidColorBrush brush = new(Color.FromArgb(75, 0, 0, 0));
            for (int i = 100; i < size / 2; i += 100)
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
            Line line = new()
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
