using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;

namespace Grafer2
{
    public class CoordinateSystem : Canvas
    {

        public CoordinateSystem(double width,double height)
        {
            Width = width;
            Height = height;
        }

        public void Draw()
        {
            DrawAxis();
        }

        private void DrawAxis()
        {
            Line lineX = new()
            {
                X1 = 0,
                Y1 = Height / 2,
                X2 = Width,
                Y2 = Height / 2,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            Line lineY = new()
            {
                X1 = Width/2,
                Y1 = 0,
                X2 = Width/2,
                Y2 = Height,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            Children.Add(lineX);
            Children.Add(lineY);
        }
    }
}
