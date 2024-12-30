using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using NCalc; // Ensure NCalc is used for expressions

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

            // Add placeholder logic for the input box
            EquationInput.GotFocus += (s, e) =>
            {
                if (EquationInput.Text == "Enter equation, e.g., x^2 + 3")
                {
                    EquationInput.Text = "";
                    EquationInput.Foreground = System.Windows.Media.Brushes.Black;
                }
            };

            EquationInput.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(EquationInput.Text))
                {
                    EquationInput.Text = "Enter equation, e.g., x^2 + 3";
                    EquationInput.Foreground = System.Windows.Media.Brushes.Gray;
                }
            };

            // Initialize with placeholder text
            EquationInput.Text = "Enter equation, e.g., x^2 + 3";
            EquationInput.Foreground = System.Windows.Media.Brushes.Gray;
        }

        // Initialize the graph with axes and title
        private void InitializeGraph()
        {
            plotModel = new PlotModel { Title = "Graphorama" };
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "X-Axis" });
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Y-Axis" });
            PlotView.Model = plotModel;
        }

        // Function to plot the mathematical equation
        private void PlotFunction(string equation)
        {
            var series = new LineSeries { Title = equation };
            try
            {
                for (double x = -10; x <= 10; x += 0.01) // Smaller step size for smoother curves
                {
                    double y = EvaluateEquation(equation, x);
                    if (!double.IsNaN(y))
                    {
                        series.Points.Add(new DataPoint(x, y));
                    }
                }
                plotModel.Series.Add(series);
                plotModel.InvalidatePlot(true);

                // Add to function list
                functionList.Add(equation);
                UpdateFunctionList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error plotting function: {ex.Message}");
            }
        }

        // Evaluate the mathematical equation for a specific x value
        private double EvaluateEquation(string equation, double x)
        {
            try
            {
                // Handle implicit 'y =' notation (e.g., y = 2x becomes 2*x)
                if (equation.Contains("="))
                {
                    var parts = equation.Split('=');
                    if (parts.Length == 2)
                    {
                        equation = parts[1].Trim(); // Take the part after '='
                    }
                    else
                    {
                        throw new ArgumentException("Invalid equation format.");
                    }
                }

                // Replace 'x' in the equation with the actual value of x
                equation = equation.Replace("x", x.ToString(CultureInfo.InvariantCulture));

                // Handle inverse trigonometric functions (e.g., sin$ becomes asin)
                equation = equation.Replace("sin$", "asin");
                equation = equation.Replace("cos$", "acos");
                equation = equation.Replace("tan$", "atan");

                // Use NCalc to evaluate the equation
                var expression = new NCalc.Expression(equation);  // Explicitly use NCalc.Expression
                var result = expression.Evaluate();
                return Convert.ToDouble(result, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error evaluating equation: {ex.Message}");
                return double.NaN;
            }
        }

        // Update the list of functions displayed in the UI
        private void UpdateFunctionList()
        {
            FunctionListBox.Items.Clear();
            foreach (var function in functionList)
            {
                FunctionListBox.Items.Add(function);
            }
        }

        // Event handler for the "Plot" button click
        private void OnPlotButtonClick(object sender, RoutedEventArgs e)
        {
            var equation = EquationInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(equation) || equation == "Enter equation, e.g., x^2 + 3")
            {
                MessageBox.Show("Please enter a valid equation.");
                return;
            }
            PlotFunction(equation);
        }

        // Event handler for the "Clear" button click
        private void OnClearButtonClick(object sender, RoutedEventArgs e)
        {
            plotModel.Series.Clear();
            functionList.Clear();
            FunctionListBox.Items.Clear();
            plotModel.InvalidatePlot(true);
        }

        // Event handler for the "Reset Zoom" button click
        private void OnZoomResetClick(object sender, RoutedEventArgs e)
        {
            foreach (var axis in plotModel.Axes)
            {
                axis.Reset();
            }

            plotModel.InvalidatePlot(true);
        }
    }
}
