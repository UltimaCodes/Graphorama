using System;
using System.Windows;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using System.Windows.Controls;
using System.Data;
using OxyPlot.Wpf;

namespace Graphorama
{
    public partial class MainWindow : Window
    {
        private PlotModel plotModel;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGraph();
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
            var series = new LineSeries { Title = equation };
            try
            {
                for (double x = -10; x <= 10; x += 0.1)
                {
                    double y = EvaluateEquation(equation, x);
                    series.Points.Add(new DataPoint(x, y));
                }
                plotModel.Series.Add(series);
                plotModel.InvalidatePlot(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error plotting function: {ex.Message}");
            }
        }

        private double EvaluateEquation(string equation, double x)
        {
            // Basic parser for mathematical expressions
            // Extend this or use libraries like NCalc for more complex parsing
            equation = equation.Replace("x", x.ToString());
            DataTable table = new DataTable();
            return Convert.ToDouble(table.Compute(equation, ""));
        }

        private void OnPlotButtonClick(object sender, RoutedEventArgs e)
        {
            var equation = EquationInput.Text;
            PlotFunction(equation);
        }

        private void OnClearButtonClick(object sender, RoutedEventArgs e)
        {
            plotModel.Series.Clear();
            plotModel.InvalidatePlot(true);
        }

        private void OnZoomResetClick(object sender, RoutedEventArgs e)
        {
            plotModel.Axes[0].Reset();
            plotModel.Axes[1].Reset();
            plotModel.InvalidatePlot(true);
        }
    }
}
