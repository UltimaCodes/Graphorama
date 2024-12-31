using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
        private bool isDarkMode = false;
        private bool isRealTime = false;
        private int realTimeSpeed = 256;

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

            foreach (var axis in plotModel.Axes)
            {
                axis.MajorGridlineStyle = LineStyle.Solid;
                axis.MinorGridlineStyle = LineStyle.Dot;
                axis.MajorGridlineColor = OxyColors.Gray;
                axis.MinorGridlineColor = OxyColors.LightGray;
            }

            PlotView.Model = plotModel;
        }

        private async Task PlotFunctionRealTime(string equation)
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
                        plotModel.Series.Clear();
                        plotModel.Series.Add(series);
                        plotModel.InvalidatePlot(true);
                        await Task.Delay(1000 / realTimeSpeed);
                    }
                }
                functionList.Add(equation);
                UpdateFunctionList();
            }
            catch
            {
                ShowErrorMessage("Invalid equation for real-time graphing.");
            }
        }

        private string PreprocessEquation(string equation)
        {
            equation = System.Text.RegularExpressions.Regex.Replace(equation, @"(\d)([a-zA-Z])", "$1*$2");

            equation = System.Text.RegularExpressions.Regex.Replace(equation, @"\b(sin|cos|tan|log|sqrt|exp)\b", match =>
            {
                return char.ToUpper(match.Value[0]) + match.Value.Substring(1);
            });

            return equation;
        }

        private double EvaluateEquation(string equation, double x)
        {
            try
            {
                equation = PreprocessEquation(equation);

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
                if (isRealTime)
                {
                    _ = PlotFunctionRealTime(equation);
                }
                else
                {
                    PlotFunction(equation);
                }
                EquationInput.Clear();
            }
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
                ShowErrorMessage("Invalid equation. Please check your input.");
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

        private void OnToggleDarkModeClick(object sender, RoutedEventArgs e)
        {
            isDarkMode = !isDarkMode;
            var backgroundColor = isDarkMode ? OxyColors.Black : OxyColors.White;
            var textColor = isDarkMode ? OxyColors.LightGray : OxyColors.Black;

            plotModel.Background = backgroundColor;
            plotModel.TextColor = textColor;
            plotModel.InvalidatePlot(true);
        }

        private void OnRealTimeToggleClick(object sender, RoutedEventArgs e)
        {
            isRealTime = !isRealTime;
            RealTimeButton.Content = isRealTime ? "Real-Time: On" : "Real-Time: Off";
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private void OnSpeedChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(SpeedInput.Text, out int speed))
            {
                realTimeSpeed = Clamp(speed, 1, 256);
            }
            else
            {
                realTimeSpeed = 256;
            }
        }

        private void EquationInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (EquationInput.Text == "Enter function, e.g., 2x + 3")
            {
                EquationInput.Text = "";
                EquationInput.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void EquationInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EquationInput.Text))
            {
                EquationInput.Text = "Enter function, e.g., 2x + 3";
                EquationInput.Foreground = System.Windows.Media.Brushes.Gray;
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

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnChangeLimitsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                double xMin = double.Parse(XMinInput.Text);
                double xMax = double.Parse(XMaxInput.Text);
                double yMin = double.Parse(YMinInput.Text);
                double yMax = double.Parse(YMaxInput.Text);

                if (xMin >= xMax || yMin >= yMax)
                {
                    ShowErrorMessage("Invalid axis limits. Ensure Min values are less than Max values.");
                    return;
                }

                foreach (var axis in plotModel.Axes)
                {
                    if (axis.Position == AxisPosition.Bottom)
                    {
                        axis.Minimum = xMin;
                        axis.Maximum = xMax;
                    }
                    else if (axis.Position == AxisPosition.Left)
                    {
                        axis.Minimum = yMin;
                        axis.Maximum = yMax;
                    }
                }

                plotModel.InvalidatePlot(true);
            }
            catch
            {
                ShowErrorMessage("Invalid input. Please enter numeric values for axis limits.");
            }
        }
    }
}