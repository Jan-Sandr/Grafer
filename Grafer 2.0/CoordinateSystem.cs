﻿using System.Windows.Controls;
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
            DrawNumbers();
        }

        private void DrawGrid()
        {
            DrawGridLines(Direction.X, Width);
            DrawGridLines(Direction.Y, Height);
        }

        private void DrawNumbers()
        {
            DrawGridNumbers(Direction.X, Width);
            DrawGridNumbers(Direction.Y, Height);
        }

        private void DrawGridNumbers(Direction direction, double size)
        {
            for (int i = 100; i < size / 2; i += 100)
            {
                switch (direction)
                {
                    case Direction.X:
                        {
                            Children.Add(NewNumber(-i / 100, size / 2 - i - 12, Height / 2 + 10));
                            Children.Add(NewNumber(i / 100, size / 2 + i - 5, Height / 2 + 10));
                            break;
                        }

                    case Direction.Y:
                        {
                            Children.Add(NewNumber(i / 100, Width / 2 + 16, size / 2 - i - 15));
                            Children.Add(NewNumber(-i / 100, Width / 2 + 10, size / 2 + i - 15));
                            break;
                        }
                }
            }

            Children.Add(NewNumber(0, Width / 2 + 16, Height / 2 + 10));
        }

        private TextBlock NewNumber(int value, double x, double y)
        {
            TextBlock number = new();
            number.Text = value.ToString();
            number.FontSize = 20;

            number.RenderTransform = new TranslateTransform()
            {
                X = x,
                Y = y
            };

            return number;
        }

        private void DrawGridLines(Direction direction, double size)
        {
            Color color = Color.FromArgb(100, 0, 0, 0);

            for (int i = 100; i < size; i += 100)
            {
                Line gridLine = new();
                SolidColorBrush brush = new SolidColorBrush(color);
                switch (direction)
                {
                    case Direction.X:
                        {
                            gridLine = NewLine(direction, lineY: i, brushes: brush);
                            break;
                        }

                    case Direction.Y:
                        {
                            gridLine = NewLine(direction, lineX: i, brushes: brush);
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
                StrokeThickness = strokeThickness,              
            };

            line.SnapsToDevicePixels = true;

            return line;
        }
    }
}
