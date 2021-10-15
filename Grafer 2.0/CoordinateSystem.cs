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
        }

        private void DrawAxes()
        {
            Line axisX = NewLine(Direction.X, lineY: Height/2);

            Line axisY = NewLine(Direction.Y, lineX: Width/2);

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
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            return line;
        }
    }
}
