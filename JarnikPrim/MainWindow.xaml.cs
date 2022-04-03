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

namespace JarnikPrim
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Mode mode;
        private List<Ellipse> points;
        private List<Tuple<Ellipse, Ellipse, Line, double>> connections;
        private bool first;
        private bool resetRequire;
        private Ellipse actualPointEl;
        private Point actualPointPo;

        public MainWindow()
        {
            InitializeComponent();

            mode = Mode.none;
            points = new List<Ellipse>();
            connections = new List<Tuple<Ellipse, Ellipse, Line, double>>();
            first = true;
            resetRequire = false;

            ChangeLabelByMode();
        }

        private void ChangeLabelByMode()
        {
            switch(mode)
            {
                case Mode.placing:
                    label.Content = "Actual Mode: Placing Nodes";
                    break;
                case Mode.connecting1:
                    label.Content = "Actual Mode: Choose first node to connect";
                    break;
                case Mode.connecting2:
                    label.Content = "Actual Mode: Choose second node to connect";
                    break;
                default:
                    label.Content = "Actual Mode: None";
                    break;
            }
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            if(points.Count == 0)
            {
                label.Content = "You must place nodes!";
                return;
            }

            Ellipse actual = points[0];
            List<Ellipse> foundedPoints = new List<Ellipse>();
            foundedPoints.Add(points[0]);
            points.Remove(points[0]);

            while(points.Count > 0)
            {
                actual.Fill = Brushes.Red;

                List<Tuple<Ellipse, Ellipse, Line, double>> list = new List<Tuple<Ellipse, Ellipse, Line, double>>();
                foreach (var tuple in connections)
                {
                    foreach (var point in foundedPoints)
                    {
                        if (tuple.Item1 == point || tuple.Item2 == point)
                        {
                            list.Add(tuple);
                        }
                    }
                }

                Tuple<Ellipse, Ellipse, Line, double> shortest = null;
                foreach (var tuple in list)
                {
                    if(shortest == null)
                        shortest = tuple;

                    if(tuple.Item4 < shortest.Item4)
                        shortest = tuple;
                }

                shortest.Item3.Stroke = Brushes.Red;
                connections.Remove(shortest);
                actual = shortest.Item1 == actual ? shortest.Item2 : shortest.Item1;
                foundedPoints.Add(actual);
                points.Remove(actual);
            }

            actual.Fill = Brushes.Red;
            resetRequire = true;

            //zaruci, aby byly vrcholy nejvysse
            foreach (var item in foundedPoints)
            {
                canvas.Children.Remove(item);
                canvas.Children.Add(item);
            }
        }

        private void placeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (resetRequire)
            {
                resetBtn_Click(sender, e);
                resetRequire = false;
            }

            if (mode != Mode.connecting2)
            {
                mode = Mode.placing;
                ChangeLabelByMode();
            }
        }

        private void connectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (resetRequire)
            {
                resetBtn_Click(sender, e);
                resetRequire = false;
            }

            if (mode != Mode.connecting2)
            {
                mode = Mode.connecting1;
                ChangeLabelByMode();
            }
        }

        private void resetBtn_Click(object sender, RoutedEventArgs e)
        {
            List<UIElement> list = new List<UIElement>();
            foreach (var item in canvas.Children)
            {
                list.Add((UIElement)item);
            }
            foreach (var item in list)
            {
                canvas.Children.Remove(item);
            }

            mode = Mode.none;
            points = new List<Ellipse>();
            connections = new List<Tuple<Ellipse, Ellipse, Line, double>>();
            first = true;
            ChangeLabelByMode();
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (resetRequire)
            {
                resetBtn_Click(sender, e);
                resetRequire = false;
            }

            if (mode == Mode.placing)
            {
                Point point = e.GetPosition(canvas);

                Ellipse ellipse = new Ellipse();
                ellipse.Fill = Brushes.Black;
                ellipse.Height = 50;
                ellipse.Width = 50;
                ellipse.Margin = new Thickness(point.X - 25, point.Y - 25, 0, 0);
                ellipse.MouseDown += Ellipse_MouseDown;
                canvas.Children.Add(ellipse);
                points.Add(ellipse);

                actualPointEl = ellipse;
                actualPointPo = new Point(point.X, point.Y);

                if (!first)
                {
                    mode = Mode.connecting2;
                }

                ChangeLabelByMode();
                first = false;
            }
            
        }

        private void Ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (resetRequire)
            {
                resetBtn_Click(sender, e);
            }

            if (sender is Ellipse el)
            {
                if(mode == Mode.connecting2)
                {
                    Line line = new Line();
                    line.X1 = actualPointPo.X;
                    line.Y1 = actualPointPo.Y;
                    line.X2 = e.GetPosition(canvas).X;
                    line.Y2 = e.GetPosition(canvas).Y;
                    line.Stroke = Brushes.Black;
                    line.StrokeThickness = 2;
                    canvas.Children.Add(line);

                    double x = Math.Abs(line.X1 - line.X2);
                    double y = Math.Abs(line.Y1 - line.Y2);

                    double vzdalenost = Math.Sqrt(x * x + y * y);


                    Tuple<Ellipse, Ellipse, Line, double> tuple = Tuple.Create(actualPointEl, el, line, vzdalenost);
                    connections.Add(tuple);

                    mode = Mode.none;
                    ChangeLabelByMode();
                }
                else if(mode == Mode.connecting1)
                {
                    actualPointEl = sender as Ellipse;
                    actualPointPo = e.GetPosition(canvas);

                    mode = Mode.connecting2;
                    ChangeLabelByMode();
                }
            }
        }
    }

    public enum Mode
    {
        none,
        placing,
        connecting1,
        connecting2
    }
}
