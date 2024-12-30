using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq; // Import LINQ for FirstOrDefault
using System.Windows;
using System.Windows.Controls;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using NCalc;

namespace Graphorama
{
    public partial class MainWindow : Window
    {
        private PlotModel plotModel;
        private List<string> functionList = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            InitializeGraph();
            SetupPlaceholderBehavior();
        }

        private void InitializeGraph()
        {
            plotModel = new PlotModel { Title = "Graphorama" };
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "X-Axis" });
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Y-Axis" });
            PlotView.Model = plotModel;
        }

        private void PlotFunction(string equation)
        {
            try
            {
                var series = new LineSeries { Title = equation };
                for (double x = -10; x <= 10; x += 0.1)
                {
                    double y = EvaluateEquation(equation, x);
                    if (!double.IsNaN(y) && !double.IsInfinity(y))
                    {
                        series.Points.Add(new DataPoint(x, y));
                    }
                }

                plotModel.Series.Add(series);
                plotModel.InvalidatePlot(true);
                functionList.Add(equation);
                UpdateFunctionList();
            }
            catch
            {
                MessageBox.Show("Invalid equation. Please check your input.");
            }
        }

        private double EvaluateEquation(string equation, double x)
        {
            try
            {
                equation = equation.Replace("x", x.ToString(CultureInfo.InvariantCulture));
                var expression = new NCalc.Expression(equation);
                var result = expression.Evaluate();
                return Convert.ToDouble(result);
            }
            catch
            {
                return double.NaN;
            }
        }

        private void UpdateFunctionList()
        {
            FunctionList.Items.Clear();
            foreach (var function in functionList)
            {
                FunctionList.Items.Add(function);
            }
        }

        private void OnAddExpressionClick(object sender, RoutedEventArgs e)
        {
            string equation = EquationInput.Text.Trim();
            if (!string.IsNullOrWhiteSpace(equation))
            {
                PlotFunction(equation);
                EquationInput.Clear();
            }
        }

        private void OnClearGraphClick(object sender, RoutedEventArgs e)
        {
            plotModel.Series.Clear();
            plotModel.InvalidatePlot(true);
            functionList.Clear();
            UpdateFunctionList();
        }

        private void OnResetZoomClick(object sender, RoutedEventArgs e)
        {
            foreach (var axis in plotModel.Axes)
            {
                axis.Reset();
            }
            plotModel.InvalidatePlot(true);
        }

        private void OnFunctionListKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Back)
            {
                if (FunctionList.SelectedItem is string selectedFunction)
                {
                    functionList.Remove(selectedFunction);

                    // Find the series with the matching title
                    var seriesToRemove = plotModel.Series.FirstOrDefault(s => s.Title == selectedFunction);
                    if (seriesToRemove != null)
                    {
                        plotModel.Series.Remove(seriesToRemove);
                        plotModel.InvalidatePlot(true);
                    }

                    UpdateFunctionList();
                }
            }
        }

        private void SetupPlaceholderBehavior()
        {
            EquationInput.Text = "Enter function, e.g., 2x + 3";
            EquationInput.Foreground = System.Windows.Media.Brushes.Gray;

            EquationInput.GotFocus += (s, e) =>
            {
                if (EquationInput.Text == "Enter function, e.g., 2x + 3")
                {
                    EquationInput.Text = "";
                    EquationInput.Foreground = System.Windows.Media.Brushes.Black;
                }
            };

            EquationInput.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(EquationInput.Text))
                {
                    EquationInput.Text = "Enter function, e.g., 2x + 3";
                    EquationInput.Foreground = System.Windows.Media.Brushes.Gray;
                }
            };
        }
    }
}
