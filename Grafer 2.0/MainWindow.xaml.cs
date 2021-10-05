using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

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

        Function gFunction;
        double gMinimumX;
        double gMaximumX;

        private void ButtonDrawClick(object sender, RoutedEventArgs e)
        {
            Start();
        }

        private void Start()
        {
            GetXRange();

            gFunction = new Function(equationInput.Text,gMinimumX,gMaximumX, drawingCanvas);
            gFunction.PrepareForCalculation();
            gFunction.CalculatePoints();
            Draw();
        }

        private void GetXRange()
        {
            if(limitX.IsChecked == true)
            {
                gMinimumX = double.Parse(minimumXIpnut.Text);
                gMaximumX = double.Parse(maximumXInput.Text);
            }
            else
            {
                gMinimumX = -drawingCanvas.Width / 200;
                gMaximumX = drawingCanvas.Width / 200;
            }
            
        }

        private void Draw()
        {
            drawingCanvas.Children.Clear();
            gFunction.Plot();
        }

        private void RangeXInputCheck(object sender, KeyEventArgs e)
        {
            if (!Regex.IsMatch(e.Key.ToString(), "[0-9]") && e.Key.ToString() != "Subtract")
            {
                e.Handled = true;
            }
        }

        private void RelationInputCheck(object sender, KeyEventArgs e)
        {
            if(!Regex.IsMatch(e.Key.ToString(), "[0-9,X,Add,Subtract,Multiply,Divide]"))
            {
                e.Handled = true;
            }
        }

    }
}
