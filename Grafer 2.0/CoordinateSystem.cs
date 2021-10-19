using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;

namespace Grafer2
{
    public class CoordinateSystem : Canvas
    {

        public enum Direction
        {
            X,
            Y
        }

        public CoordinateSystem(double width,double height)
        {
            Width = width;
            Height = height;
        }

        public void Create()
        {
            DrawAxes();
            DrawGrid();
        }

        private void DrawGrid()
        {
            DrawGridLines(Direction.X, Width);
            DrawGridLines(Direction.Y, Height);
        }

        private void DrawGridLines(Direction direction, double size)
        {
            for (int i = 100; i < size; i += 100)
            {
                Line gridLine = new();
                
                switch (direction)
                {
                    case Direction.X:
                        {
                            gridLine = NewLine(direction, lineY: i);
                            break;
                        }

                    case Direction.Y:
                        {
                            gridLine = NewLine(direction, lineX: i);
                            break;
                        }
                }

                Children.Add(gridLine);            
            }
        }

        private void DrawAxes()
        {
            Line axisX = NewLine(Direction.X, lineY: Height / 2, strokeThickness: 1.5);

            Line axisY = NewLine(Direction.Y, lineX: Width / 2, strokeThickness: 1.5);

            Children.Add(axisX);
            Children.Add(axisY);
        }

        private Line NewLine(Direction direction, double lineX = 0, double lineY = 0, SolidColorBrush? brushes = null, double strokeThickness = 1)
        {
            Line line = new()
            {
                X1 = direction == Direction.X ? 0 : lineX,
                Y1 = direction == Direction.X ? lineY : 0,
                X2 = direction == Direction.X ? Width : lineX,
                Y2 = direction == Direction.X ? lineY : Height,
                Stroke = brushes ?? Brushes.Black,
                StrokeThickness = strokeThickness
            };

            line.SnapsToDevicePixels = true;

            return line;
        }
    }
}
