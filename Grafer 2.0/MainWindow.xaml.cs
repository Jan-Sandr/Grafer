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

namespace Grafer_2._0
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

        private void ButtonDrawClick(object sender, RoutedEventArgs e)
        {
            Draw();
        }

        private void Draw()
        {
            Line line = new()
            {
                X1 = 0,
                Y1 = 0,
                X2 = 100,
                Y2 = 100,
                StrokeThickness = 1,
                Stroke = Brushes.Black
                
            };

            Polyline polyline = new()
            {
                StrokeThickness = 2,
                Stroke = Brushes.Blue

            };

            Point point = new Point()
            {
                X = 100,
                Y = 100
            };
            Point point2 = new Point()
            {
                X = 200,
                Y = 100
            };
            Point point3 = new Point()
            {
                X = 200,
                Y = 200
            };
            Point point4 = new Point()
            {
                X = 100,
                Y = 200
            };
            Point point5 = new Point()
            {
                X = 100,
                Y = 100
            };

            polyline.Points.Add(point);
            polyline.Points.Add(point2);
            polyline.Points.Add(point3);
            polyline.Points.Add(point4);
            polyline.Points.Add(point5);

            drawingCanvas.Children.Add(line);
            drawingCanvas.Children.Add(polyline);
        }
    }
}
