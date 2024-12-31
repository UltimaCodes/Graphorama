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
using System.Windows.Input;

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
                plotModel.Series.Add(series);

                List<DataPoint> buffer = new List<DataPoint>();
                const int batchSize = 10;
                double step = 0.1;

                double xMin = double.Parse(XMinInput.Text);
                double xMax = double.Parse(XMaxInput.Text);

                for (double x = xMin; x <= xMax; x += step)
                {
                    double y = EvaluateEquation(equation, x);
                    if (!double.IsNaN(y) && !double.IsInfinity(y))
                    {
                        buffer.Add(new DataPoint(x, y));
                    }

                    if (buffer.Count >= batchSize || x >= xMax)
                    {
                        series.Points.AddRange(buffer);
                        buffer.Clear();
                        ThrottledInvalidatePlot();
                        await Task.Delay(1000 / realTimeSpeed);
                    }
                }

                plotModel.InvalidatePlot(true);
                functionList.Add(equation);
                UpdateFunctionList();
            }
            catch
            {
                ShowErrorMessage("Invalid equation for real-time graphing.");
            }
        }

        private DateTime lastRedrawTime = DateTime.MinValue;
        private readonly TimeSpan redrawInterval = TimeSpan.FromMilliseconds(16);

        private void ThrottledInvalidatePlot()
        {
            if (DateTime.Now - lastRedrawTime > redrawInterval)
            {
                plotModel.InvalidatePlot(false);
                lastRedrawTime = DateTime.Now;
            }
        }

        private void PlotView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                ThrottledInvalidatePlot();
            }
        }

        private void PlotView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            plotModel.InvalidatePlot(true);
        }

        private string PreprocessEquation(string equation)
        {
            equation = System.Text.RegularExpressions.Regex.Replace(equation, @"(\d)([a-zA-Z])", "$1*$2");

            equation = System.Text.RegularExpressions.Regex.Replace(equation, @"\b(sin|cos|tan|log|sqrt|exp)\b", match =>
            {
                return char.ToUpper(match.Value[0]) + match.Value.Substring(1);
            });

            equation = System.Text.RegularExpressions.Regex.Replace(equation, @"\b(asin|acos|atan)\b", match =>
            {
                return match.Value.Substring(0, 1).ToUpper() + match.Value.Substring(1).Replace("asin", "ArcSin").Replace("acos", "ArcCos").Replace("atan", "ArcTan");
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error evaluating equation '{equation}': {ex.Message}");
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

        private bool IsValidExpression(string equation)
        {
            try
            {
                equation = PreprocessEquation(equation);

                equation = equation.Replace("x", "0");

                var expression = new NCalc.Expression(equation);
                var result = expression.Evaluate();
                return !double.IsNaN(Convert.ToDouble(result));
            }
            catch
            {
                return false;
            }
        }

        private void OnAddExpressionClick(object sender, RoutedEventArgs e)
        {
            string equation = EquationInput.Text.Trim();

            if (!string.IsNullOrWhiteSpace(equation))
            {
                if (IsValidExpression(equation))
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
                else
                {
                    ShowErrorMessage("Invalid expression. Please check your input.");
                }
            }
        }

        private void PlotFunction(string equation)
        {
            try
            {
                var series = new LineSeries { Title = equation };

                // Get axis limits from user input
                double xMin = double.Parse(XMinInput.Text);
                double xMax = double.Parse(XMaxInput.Text);

                for (double x = xMin; x <= xMax; x += 0.1)
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

        private void OnFunctionListKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                var selectedItem = FunctionList.SelectedItem as string;

                if (selectedItem != null)
                {
                    functionList.Remove(selectedItem);

                    FunctionList.Items.Remove(selectedItem);

                    RemoveFunctionFromGraph(selectedItem);
                }
            }
        }

        private void RemoveFunctionFromGraph(string equation)
        {
            var seriesToRemove = plotModel.Series.FirstOrDefault(s => s.Title == equation);
            if (seriesToRemove != null)
            {
                plotModel.Series.Remove(seriesToRemove);
                plotModel.InvalidatePlot(true);
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

                if (xMin >= xMax)
                {
                    ShowErrorMessage("Invalid X-axis limits. Ensure Min value is less than Max value.");
                    return;
                }

                foreach (var axis in plotModel.Axes)
                {
                    if (axis.Position == AxisPosition.Bottom)
                    {
                        axis.Minimum = xMin;
                        axis.Maximum = xMax;
                    }
                }

                plotModel.InvalidatePlot(true);
            }
            catch
            {
                ShowErrorMessage("Invalid input. Please enter numeric values for X-axis limits.");
            }
        }

    }
}