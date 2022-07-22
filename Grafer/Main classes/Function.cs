using Grafer.CustomControls;
using Grafer.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Grafer
{
    public class Function
    {
        public string Name { get; } = string.Empty;
        public Brush Brush { get; }
        public string InputRelation { get; }
        public double MinimumX { get; }
        public double MaximumX { get; }
        public bool Inverse { get; }
        public FunctionType Type { get; }
        public int ErrorMessageID { get; private set; } = -1;
        public int Number { get; } = 1;
        public enum FunctionType // Typy funkce na základě vhodné míry pro konkrétní osu.
        {
            Basic,
            TrigonometricFunction,
            InverseTrigonometricFunction
        }

        private protected Relation relation;

        private readonly CalculationOrder calculationOrder;

        private protected readonly List<Polyline> curves;

        private protected readonly CoordinateSystem coordinateSystem;

        private List<Point> points;
        private double y;
        private string[] relationBackup = Array.Empty<string>(); // Záloha předpisu.
        private int[] calculationOrderIndexesBackup = Array.Empty<int>(); // záloha výpočetního postupu.
        private protected double calculationMinimumX, calculationMaximumX; // Výpočetní minimum a maximum.
        private List<double> curveEdgesX = new List<double>();

        private protected double step = 0.01;

        private enum Side
        {
            Left,
            Right
        }

        public Function(string name, Brush color, string relation, double minimumX, double maximumX, CoordinateSystem coordinateSystem, bool inverse)
        {
            Name = name;
            this.relation = new Relation(relation);
            MinimumX = minimumX;
            MaximumX = maximumX;
            calculationOrder = new CalculationOrder();
            curves = new List<Polyline>();
            points = new List<Point>();
            this.coordinateSystem = coordinateSystem;
            Brush = color;
            Inverse = inverse;
            Type = GetFunctionType();
            Number = int.Parse(Name.Split(new char[] { 'f', ':' })[1]);
            InputRelation = relation;
        }

        //Zjištení typu funkce.
        private FunctionType GetFunctionType()
        {
            FunctionType type = relation.Contains("⁻¹") ? FunctionType.InverseTrigonometricFunction : FunctionType.Basic;

            if (type != FunctionType.InverseTrigonometricFunction)
            {
                for (int i = 0; i < relation.Count; i++)
                {
                    if (relation[i].IsTrigonometricFunction())
                    {
                        type = Inverse ? FunctionType.InverseTrigonometricFunction : FunctionType.TrigonometricFunction;
                        break;
                    }
                }
            }
            else
            {
                type = Inverse ? FunctionType.TrigonometricFunction : FunctionType.InverseTrigonometricFunction;
            }

            return type;
        }

        //Jestli je možné křivku udělat inverzní.
        private bool IsInvertible()
        {
            bool isInvertible = true;

            bool? isGrowing = curves[0].Points[0].Y > curves[0].Points[1].Y ? true : curves[0].Points[0].Y < curves[0].Points[1].Y ? false : (bool?)null;

            if (isGrowing != null)
            {
                for (int i = 0; i < curves.Count && isInvertible; i++)
                {
                    for (int j = 1; j < curves[i].Points.Count && isInvertible; j++)
                    {
                        if (isGrowing == true)
                        {
                            isInvertible = curves[i].Points[j - 1].Y > curves[i].Points[j].Y;
                        }

                        if (isGrowing == false)
                        {
                            isInvertible = curves[i].Points[j - 1].Y < curves[i].Points[j].Y;
                        }
                    }
                }
            }
            else
            {
                isInvertible = false;
            }

            return isInvertible;
        }

        //Výpočítání všech bodů.
        public void CalculatePoints()
        {
            CalculateCurvesPoints();

            if (curves.Count > 0)
            {
                if (curves.Count > 1 || ConvertToCalculatedX(curves[0].Points[0].X) != calculationMinimumX || ConvertToCalculatedX(curves[^1].Points[^1].X) != calculationMaximumX)
                {
                    AddPrecisePoints();
                }
            }

            points = new List<Point>();

            GetBackup();
        }

        //Výpočet křivek.
        private void CalculateCurvesPoints()
        {
            curves.Clear();

            PrepareForCalculation();

            DoCalculation();

            if (points.Count > 1)
            {
                SaveCurve(points);
            }
        }

        //Přidání precizních bodů.
        private void AddPrecisePoints()
        {
            for (int i = 0; i < curves.Count; i++)
            {
                CalculateEdgePoints(curves[i], Side.Left);
                CalculateEdgePoints(curves[i], Side.Right);
            }
        }

        //Výpočet precizních bodů jedné strany.
        private void CalculateEdgePoints(Polyline curve, Side side)
        {
            double increment = side == Side.Left ? 0.01 : -0.01;

            int validIndex = side == Side.Left ? 0 : curve.Points.Count - 1;

            double firstInvalidX = ConvertToCalculatedX(curve.Points[validIndex].X) - increment;

            List<Point> precisePoints = CalculatePrecisePoints(increment, firstInvalidX);

            if (precisePoints.Count == 3)
            {
                AddPrecisePointsToCurve(curve, precisePoints, side, firstInvalidX);
            }
        }

        //Přidání precizních bodů křivce.
        private void AddPrecisePointsToCurve(Polyline curve, List<Point> precisePoints, Side side, double firstInvalidX)
        {
            if (IsYDifferent(precisePoints))
            {
                if (precisePoints[0].Y < precisePoints[1].Y)
                {
                    y = 10000;
                }
                else
                {
                    y = -10000;
                }

                precisePoints.Add(new Point(ConvertToCoordinateX(firstInvalidX), y));

                InsertPrecisePoints(curve, precisePoints, side);
            }
        }

        //Zda u bodů viditelný rozdíl mezi y souřadnicí.
        private bool IsYDifferent(List<Point> points)
        {
            bool differentY = true;

            for (int i = 0; i < points.Count - 1; i++)
            {
                differentY = Math.Abs(points[i].Y - points[i + 1].Y) > 20;
            }

            return differentY;
        }

        //Výpočet precizních bodů.
        private List<Point> CalculatePrecisePoints(double increment, double firstInvalidX)
        {
            List<Point> precisePoints = new List<Point>(4);

            for (int i = 0; i < 3; i++)
            {
                double smallerNumber = increment * Math.Pow(10, -(i + 1));

                double x = Math.Round(firstInvalidX + smallerNumber, smallerNumber.ToString().Length);

                Point point = CalculatePoint(x);

                if (double.IsNormal(point.Y))
                {
                    point = ConvertToCoordinatePoint(point);

                    precisePoints.Add(point);
                }
            }

            return precisePoints;
        }

        //Vloží vypočítáné krajní body.
        private void InsertPrecisePoints(Polyline curve, List<Point> precisePoints, Side side)
        {
            for (int i = 0; i < precisePoints.Count; i++)
            {
                if (side == Side.Left)
                {
                    curve.Points.Insert(0, precisePoints[i]);

                }
                else
                {
                    curve.Points.Add(precisePoints[i]);
                }
            }
        }

        //Výpoočet bodů.
        private void DoCalculation()
        {
            for (double x = calculationMinimumX; x <= calculationMaximumX; x += step)
            {
                Point point = CalculatePoint(Math.Round(x, 2));

                SavePoint(point);
            }
        }

        //Výpočet bodu.
        private Point CalculatePoint(double x)
        {
            GetBackup();

            SubstituteX(x);

            ComputeY();

            return new Point(x, y);
        }

        //Vypočet y.
        private void ComputeY()
        {
            y = 0;

            y = (relation.Count > 1) ? CalculateYForX() : double.Parse(relation[0]);
        }

        //Příprava pro výpoočet.
        protected virtual void PrepareForCalculation()
        {
            calculationOrder.Create(relation);
            SetBackup();
            SetCalculationXRange();
        }

        //Omezení y pro případ velkých čísel.
        private double LimitY()
        {
            if (!double.IsInfinity(y))
            {
                if (y > 1000000)
                {
                    y = 1000000;
                }

                if (y < -1000000)
                {
                    y = -1000000;
                }
            }

            return y;
        }

        //Jestli lze funkce vykreslit.
        public bool IsDrawable()
        {
            if (!IsEmpty())
            {
                if (Inverse && (curves[0].Points.Count > 2 && !IsInvertible()))
                {
                    ErrorMessageID = 26;
                }
            }

            return ErrorMessageID == -1;
        }

        //Jestli není funkce prádzná.
        private bool IsEmpty()
        {
            if (curves.Count == 0)
            {
                ErrorMessageID = 27;
            }

            return ErrorMessageID != -1;
        }

        //Vykreslení funkce do plátna.
        public virtual void Plot(bool inverse, double opacity, Space freeShift = new Space())
        {
            for (int i = 0; i < curves.Count; i++)
            {
                Polyline curve = GetDeepCurveCopy(curves[i]);

                if (freeShift.OnX != 0 || freeShift.OnY != 0)
                {
                    curve.Points = AddFreeShift(curve.Points, freeShift);
                }

                curve.Opacity = opacity;
                curve.Uid = Name;

                if (inverse)
                {
                    coordinateSystem.Children.Add(InvertCurve(curve));
                }
                else
                {
                    coordinateSystem.Children.Add(curve);
                }
            }
        }

        //Vytvoří novou křivku v paměti nikoliv referenci.
        private Polyline GetDeepCurveCopy(Polyline inputCurve)
        {
            Polyline curve = NewCurve();

            for (int i = 0; i < inputCurve.Points.Count; i++)
            {
                curve.Points.Add(inputCurve.Points[i]);
            }

            return curve;
        }

        //Vytvoří základní křivku.
        private Polyline NewCurve()
        {
            return new Polyline()
            {
                Stroke = Brush,
                StrokeThickness = 2
            };
        }

        //Invertuje body křivky.
        private Polyline InvertCurve(Polyline polyline)
        {
            for (int i = 0; i < polyline.Points.Count; i++)
            {
                polyline.Points[i] = InvertPoint(polyline.Points[i]);
            }

            return polyline;
        }

        //Invertuje bod.
        private Point InvertPoint(Point point)
        {
            return new Point()
            {
                X = coordinateSystem.Width / 2 + coordinateSystem.Height / 2 - point.Y + coordinateSystem.AbsoluteShift.OnY + coordinateSystem.AbsoluteShift.OnX,
                Y = coordinateSystem.Height / 2 + coordinateSystem.Width / 2 - point.X + coordinateSystem.AbsoluteShift.OnY + coordinateSystem.AbsoluteShift.OnX
            };
        }

        private PointCollection AddFreeShift(PointCollection points, Space freeShift)
        {
            for (int i = 0; i < points.Count; i++)
            {
                Point point = points[i];

                points[i] = new Point()
                {
                    X = point.X + freeShift.OnX,
                    Y = point.Y + freeShift.OnY
                };
            }

            return points;
        }

        //Nastavení výpočetního rozsahu. Pokud by byl rozsah větší než plátno, omezí to jen na viditelnou plochu interně.
        private void SetCalculationXRange()
        {
            //V základu je rozsah od viditelného minima do maxima soustavy souřadnic
            calculationMinimumX = Math.Round((-coordinateSystem.Width / 200 - coordinateSystem.AbsoluteShift.OnX / 100) / coordinateSystem.Zoom, 2);
            calculationMaximumX = Math.Round((coordinateSystem.Width / 200 - coordinateSystem.AbsoluteShift.OnX / 100) / coordinateSystem.Zoom, 2);

            if (!double.IsNegativeInfinity(MinimumX))
            {
                calculationMinimumX = ((-Math.Abs(MinimumX) > coordinateSystem.NumberRange + (coordinateSystem.AbsoluteShift.OnX / 100 / coordinateSystem.Zoom)) ? -coordinateSystem.NumberRange - coordinateSystem.AbsoluteShift.OnX / 100 / coordinateSystem.Zoom : MinimumX);
            }

            if (!double.IsPositiveInfinity(MaximumX))
            {
                calculationMaximumX = ((MaximumX > coordinateSystem.NumberRange - (coordinateSystem.AbsoluteShift.OnX / 100 / coordinateSystem.Zoom)) ? coordinateSystem.NumberRange - coordinateSystem.AbsoluteShift.OnX / 100 / coordinateSystem.Zoom : MaximumX);
            }
        }

        //Dosazení za x.
        private void SubstituteX(double x)
        {
            for (int i = 0; i < relation.Count; i++)
            {
                relation[i] = relation[i] == "x" ? x.ToString() : relation[i];
                relation[i] = relation[i] == "-x" ? (-x).ToString() : relation[i];
            }
        }

        //Výpočet y pro x.
        private double CalculateYForX()
        {
            int orderProgression = 0;

            while (relation.Count > 1 && !double.IsNaN(y))
            {
                y = CalculateYValue(orderProgression);
                orderProgression++;
            }

            y = Math.Abs(y) > 1000000 ? LimitY() : y;

            return y;
        }

        //Výpočet hodnoty y.
        private double CalculateYValue(int orderProgression)
        {
            int index = calculationOrder.Indexes[orderProgression];

            y = Operation(index);

            index = relation[index].IsTrigonometricFunctionOrLogarithm() ? index + 1 : index;

            relation[index] = y.ToString();

            relation.RemoveNeighbors(index);

            y = double.Parse(relation[index - relation.RemovedElementsCount / 2]);

            calculationOrder.ShiftPosition(relation.RemovedElementsCount, orderProgression);

            relation.RemovedElementsCount = 0;

            return y;
        }

        //Uložení bodu.
        private void SavePoint(Point point)
        {
            point = ConvertToCoordinatePoint(point);

            if (!double.IsNaN(y) && !double.IsInfinity(y) && Math.Abs(point.Y) < 5000) // Pokud bod není definovaný, vzniká mezera.
            {
                points.Add(point);
            }
            else if (points.Count > 0)
            {
                if (points.Count > 1) // Křivka minimálně ze 2 bodů.
                {
                    SaveCurve(points);
                }

                points = new List<Point>();
            }
        }

        //Uložení křivky.
        private void SaveCurve(List<Point> points)
        {
            Polyline polyline = NewCurve();

            polyline.SnapsToDevicePixels = true;

            for (int i = 0; i < points.Count; i++)
            {
                polyline.Points.Add(points[i]);
            }

            curves.Add(polyline);
        }

        //Převedení bodu na bod do soustavy.
        private Point ConvertToCoordinatePoint(Point point)
        {
            point = new Point()
            {
                X = ConvertToCoordinateX(point.X),
                Y = ConvertToCoordinateY(point.Y)
            };
            return point;
        }

        private double ConvertToCalculatedX(double x)
        {
            return Math.Round((x - coordinateSystem.Width / 2 - coordinateSystem.AbsoluteShift.OnX) / coordinateSystem.Zoom / 100, 2);
        }

        private double ConvertToCoordinateX(double x)
        {
            return coordinateSystem.Width / 2 + (x * coordinateSystem.Zoom * 100) + coordinateSystem.AbsoluteShift.OnX;
        }

        private double ConvertToCoordinateY(double y)
        {
            return coordinateSystem.Height / 2 + (-y * coordinateSystem.Zoom * 100) + coordinateSystem.AbsoluteShift.OnY;
        }

        //Operace mezi 2 členy v předpisu.
        private double Operation(int index)
        {
            switch (relation[index])
            {
                case "+":
                    {
                        y = double.Parse(relation[index - 1]) + double.Parse(relation[index + 1]);
                        break;
                    }
                case "-":
                    {
                        y = double.Parse(relation[index - 1]) - double.Parse(relation[index + 1]);
                        break;
                    }
                case "*":
                    {
                        y = double.Parse(relation[index - 1]) * double.Parse(relation[index + 1]);
                        break;
                    }
                case "/":
                    {
                        y = double.Parse(relation[index - 1]) / double.Parse(relation[index + 1]);
                        break;
                    }
                case "^":
                    {
                        y = Math.Pow(double.Parse(relation[index - 1]), double.Parse(relation[index + 1]));
                        break;
                    }
                case "√":
                    {
                        y = Root(double.Parse(relation[index + 1]), double.Parse(relation[index - 1]));
                        break;
                    }
                case "sin":
                    {
                        y = TrigFunc(double.Parse(relation[index + 2]), relation[index + 1], Math.Sin, Math.Asin);
                        break;
                    }
                case "cos":
                    {
                        y = TrigFunc(double.Parse(relation[index + 2]), relation[index + 1], Math.Cos, Math.Acos);
                        break;
                    }
                case "tg":
                    {
                        y = TrigFunc(double.Parse(relation[index + 2]), relation[index + 1], Math.Tan, Math.Atan);
                        break;
                    }
                case "cotg":
                    {
                        y = TrigFunc(double.Parse(relation[index + 2]), relation[index + 1], Cotangens, ArcusCotangens);
                        break;
                    }
                case "log":
                    {
                        y = Math.Log(double.Parse(relation[index + 2]), double.Parse(relation[index + 1]));
                        break;
                    }
            }

            return y;
        }

        //Odmocnění
        private double Root(double number, double index)
        {
            double result = double.NaN;

            if (index > 0 && !double.IsInfinity(index))
            {
                if (index % 2 == 0)
                {
                    result = Math.Pow(number, 1 / index);
                }
                else
                {
                    if (number < 0)
                    {
                        result = -Math.Pow(-number, 1 / index);
                    }
                    else
                    {
                        result = Math.Pow(number, 1 / index);
                    }
                }
            }

            return result;
        }

        private double TrigFunc(double number, string argument, Func<double, double> func, Func<double, double> arcusFunc)
        {
            return argument == "⁻¹" ? arcusFunc(number) : func(number);
        }

        private double Cotangens(double number)
        {
            return Math.Cos(number) / Math.Sin(number);
        }

        private double ArcusCotangens(double number)
        {
            return Math.Atan(number) * -1;
        }

        //Vytvoření zálohy pro výpočet.
        private void SetBackup()
        {
            relationBackup = relation.ToArray();
            calculationOrderIndexesBackup = new int[calculationOrder.Indexes.Length];
            calculationOrder.Indexes.CopyTo(calculationOrderIndexesBackup, 0);
        }

        //Načtení zálohy.
        private void GetBackup()
        {
            relation = new Relation();
            relation.AddRange(relationBackup);
            calculationOrderIndexesBackup.CopyTo(calculationOrder.Indexes, 0);
        }

        //Převod hodnot vlastností do pole.
        public string[] PropertiesValueToArray()
        {
            var properties = typeof(Function).GetProperties();

            string[] propertiesValue = new string[properties.Length];

            for (int i = 0; i < properties.Length; i++)
            {
                propertiesValue[i] = (properties[i].GetValue(this)!.ToString()!);
            }

            return propertiesValue;
        }

        public string GetIndetificator()
        {
            return Name.Split(':')[0];
        }
    }
}
