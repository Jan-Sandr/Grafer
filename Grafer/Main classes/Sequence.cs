using Grafer.CustomControls;
using System;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Grafer
{
    public class Sequence : Function
    {
        public Sequence(string name, Brush color, string relation, double minimumX, double maximumX, CoordinateSystem coordinateSystem, bool inverse) : base(name, color, relation, minimumX, maximumX, coordinateSystem, inverse)
        {

        }

        //Vykreslení křížků
        public override void Plot(bool inverse, double opacity, Space freeShift = default)
        {
            for (int i = 0; i < curves[0].Points.Count; i++)
            {
                //První část křížku
                Line LineUpDown = new Line
                {
                    X1 = curves[0].Points[i].X - 8,
                    Y1 = curves[0].Points[i].Y - 8,
                    X2 = curves[0].Points[i].X + 8,
                    Y2 = curves[0].Points[i].Y + 8,
                    Stroke = Brush,
                    StrokeThickness = 2,
                    Fill = Brush,
                };

                //Druhá část křížku
                Line LineDownUp = new Line
                {
                    X1 = curves[0].Points[i].X - 8,
                    Y1 = curves[0].Points[i].Y + 8,
                    X2 = curves[0].Points[i].X + 8,
                    Y2 = curves[0].Points[i].Y - 8,
                    Stroke = Brush,
                    StrokeThickness = 2,
                    Fill = Brush,
                };

                coordinateSystem.Children.Add(LineUpDown);
                coordinateSystem.Children.Add(LineDownUp);
            }
        }

        //Přepíše v předpisu znaky n na x, protože výpočet pracuje s x.
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

            calculationMinimumX = calculationMinimumX < 0 ? 0 : Math.Ceiling(calculationMinimumX);
        }
    }
}
