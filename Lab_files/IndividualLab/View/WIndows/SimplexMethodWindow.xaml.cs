using ClassLibraryTools;
using System.Windows;
using System.Windows.Input;

namespace IndividualLab.View.WIndows
{
    /// <summary>
    /// Interaction logic for SimplexMethodWindow.xaml
    /// </summary>
    public partial class SimplexMethodWindow : Window
    {
        private MainWindow win { get; set; }

        private bool isFullscreen = false;
        private double prevWidth; 
        private double prevHeight; 
        private double prevTop; 
        private double prevLeft; 
        public SimplexMethodWindow(MainWindow win)
        {
            this.win = win;
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
            this.Activate();
        }
        private void outputStackPanel_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            inputScroll.ScrollToEnd();
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape && isFullscreen)
            {
                btnMaximize_Click(null, null);
            }
            base.OnKeyDown(e);
        }
    }
}
