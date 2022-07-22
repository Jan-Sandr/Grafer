using Grafer.CustomControls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Grafer
{
    public class Sequence : Function
    {
        public Sequence(string name, Brush color, string relation, double minimumX, double maximumX, CoordinateSystem coordinateSystem, bool inverse) : base(name, color, relation, minimumX, maximumX, coordinateSystem, inverse)
        {

        }

        public override void Plot(bool inverse, double opacity, Space freeShift = default)
        {
            for (int i = 0; i < curves[0].Points.Count; i++)
            {
                // Add an Ellipse Element
                Ellipse myEllipse = new Ellipse();
                myEllipse.Margin = new Thickness(curves[0].Points[i].X - 8 - coordinateSystem.ZoomLevel, curves[0].Points[i].Y - 8 - coordinateSystem.ZoomLevel, 0, 0);
                myEllipse.Stroke = Brushes.Black;
                myEllipse.Fill = Brush;
                myEllipse.Width = 16 + 2 * coordinateSystem.ZoomLevel;
                myEllipse.Height = 16 + 2 * coordinateSystem.ZoomLevel;
                coordinateSystem.Children.Add(myEllipse);
            }
        }

        protected override void PrepareForCalculation()
        {
            if (relation.Contains("n"))
            {
                for (int i = 0; i < relation.Count; i++)
                {
                    if (relation[i] == "n")
                    {
                        relation[i] = "x";
                    }
                }
                step = 1;
            }

            base.PrepareForCalculation();

                calculationMinimumX = 0;
        }
    }
}
