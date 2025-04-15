using ClassLibraryTools;
using System.Windows;
using System.Windows.Input;

namespace IndividualLab.View.WIndows
{
    /// <summary>
    /// Interaction logic for GraphicalMethodWindow.xaml
    /// </summary>
    public partial class GraphicalMethodWindow : Window
    {
        private MainWindow win { get; set; }
        private Graphic graphic { get; set; }
        private bool isFullscreen = false;
        private double prevWidth;
        private double prevHeight;
        private double prevTop;
        private double prevLeft;
        public GraphicalMethodWindow(MainWindow win, Graphic graph)
        {
            this.win = win;
            this.graphic = graph;
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (isFullscreen)
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                ResizeMode = ResizeMode.CanResize;
                WindowState = WindowState.Normal;
                Width = prevWidth;
                Height = prevHeight;
                Top = prevTop;
                Left = prevLeft;
                isFullscreen = false;
            }
            else
            {
                prevWidth = Width;
                prevHeight = Height;
                prevTop = Top;
                prevLeft = Left;
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                WindowState = WindowState.Normal;
                Width = SystemParameters.PrimaryScreenWidth;
                Height = SystemParameters.PrimaryScreenHeight;
                Top = 0;
                Left = 0;
                isFullscreen = true;
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Utils.ClearAllFields();
            win.Show();
            win.Activate();
        }

        private void increaseScale_Click(object sender, RoutedEventArgs e) // +
        {
            if (graphic.scale >= 4) return;
            double newScale = graphic.scale * 2;
            graphic.UpdateScale(newScale);
            graphGrid.Children.Clear();
            graphGrid.Children.Add(graphic.renderedGraph);
        }
        private void deacreseScale_Click(object sender, RoutedEventArgs e) // -
        {
            if (graphic.scale <= 0.01) return;
            double newScale = graphic.scale * 0.5;
            graphic.UpdateScale(newScale);
            graphGrid.Children.Clear();
            graphGrid.Children.Add(graphic.renderedGraph);
        }
    }
}
