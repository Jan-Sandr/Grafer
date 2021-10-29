using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Grafer2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private Function? gFunction;
        private double gMinimumX;
        private double gMaximumX;
        private bool IsRangeValid;

        private void ButtonDrawClick(object sender, RoutedEventArgs e)
        {
            Start();
        }

        private void Start()
        {
            gFunction = null;
            IsRangeValid = true;
            GetXRange();

            if(IsRangeValid)
            {
                gFunction = new Function(equationInput.Text, gMinimumX, gMaximumX, drawingCanvas);
                gFunction.PrepareForCalculation();

                if (gFunction.Relation.IsRelationValid)
                {
                    gFunction.CalculatePoints();
                }
            }

            Draw();
        }

        private void GetXRange()
        {
            if (limitX.IsChecked == true)
            {
                gMinimumX = double.Parse(minimumXIpnut.Text);
                gMaximumX = double.Parse(maximumXInput.Text);
                CheckXRange();
            }
            else
            {
                gMinimumX = -drawingCanvas.Width / 200;
                gMaximumX = drawingCanvas.Width / 200;
            }

        }

        private void CheckXRange()
        {
            if(gMinimumX > gMaximumX)
            {
                MessageBox.Show("Minimum can't be higher than maximum");
                IsRangeValid = false;
            }
        }

        private void Draw()
        {
            drawingCanvas.Children.Clear();
            CoordinateSystem coordinateSystem = new(drawingCanvas.Width, drawingCanvas.Height);
            coordinateSystem.Create();
            drawingCanvas.Children.Add(coordinateSystem);

            if (gFunction != null)
            {
                gFunction.Plot();
            }
        }

        private void RangeXInputCheck(object sender, KeyEventArgs e)
        {
            if (!Regex.IsMatch(e.Key.ToString(), "[0-9]") && e.Key.ToString() != "Subtract" && e.Key.ToString() != "Back" && e.Key.ToString() != "OemComma")
            {
                e.Handled = true;
            }
        }

        private void RelationInputCheck(object sender, KeyEventArgs e)
        {
            if (!Regex.IsMatch(e.Key.ToString(), "[0-9,X,Add,Subtract,Multiply,Divide]"))
            {
                e.Handled = true;
            }
        }

        private void DrawingCanvasLoaded(object sender, RoutedEventArgs e)
        {
            Draw();
        }
    }
}
