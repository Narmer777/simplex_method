using ClassLibraryTools;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace IndividualLab.View.WIndows
{
    /// <summary>
    /// Диалоговое окно для выбора базиса.
    /// </summary>
    public partial class DialogWindowBasisSelection : Window
    {
        private int numberOfRestrictions { get; set; }
        private int numberOfX { get; set; }
        private List<int> basis { get; set; }
        public DialogWindowBasisSelection(int numberOfX, int numberOfRestrictions)
        {
            this.numberOfRestrictions = numberOfRestrictions;
            this.numberOfX = numberOfX;
            basis = new List<int>();
            InitializeComponent();
        }
        private void dialogWindowForBasis_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        private void dialogWindowForBasis_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas selectBasisCanvas = new Canvas(); //8
            double currentX = 10;
            double currentY = 10;
            for (int i = 0; i < numberOfX; i++)
            {
                Label label = new Label();
                label.MouseDown += Label_Click;
                label.Tag = i;
                label.Height = 50;
                label.Width = 50;
                label.BorderBrush = new SolidColorBrush(Colors.Black);
                label.Background = new SolidColorBrush(Colors.White);
                label.BorderThickness = new Thickness(2);
                label.Content = "X" + Utils.makeLowerIndex(i + 1);
                label.FontWeight = FontWeights.Bold;
                label.FontSize = 20;
                label.HorizontalContentAlignment = HorizontalAlignment.Center;
                label.VerticalContentAlignment = VerticalAlignment.Center;
                label.FontFamily = new FontFamily("Times New Roman");

                if (i == 8) { currentY += 60; currentX = 10; }
                Canvas.SetLeft(label, currentX);
                Canvas.SetTop(label, currentY);
                selectBasisCanvas.Children.Add(label);
                currentX += label.Width + 3;
            }
            selectBasisGrid.Children.Add(selectBasisCanvas);
        }
        private void Label_Click(object sender, MouseButtonEventArgs e)
        {
            var label = sender as Label;
            if (label == null) return;
            int index = (int)label.Tag;

            if (basis.Contains(index))
            {
                label.Background = new SolidColorBrush(Colors.White);
                basis.Remove(index);
            }
            else
            {
                label.Background = new SolidColorBrush(Color.FromArgb(100, 0, 0, 255));
                basis.Add(index);
            }
        }
        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            dialogWindowForBasis.DialogResult = false;
            this.Close();
        }
        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            basis.Sort();
            MainWindow.selectedBasis = new List<int>();
            MainWindow.selectedBasis = basis;
            dialogWindowForBasis.DialogResult = true;
            this.Close();
        }


    }
}
