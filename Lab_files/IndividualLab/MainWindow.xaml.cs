using ClassLibraryTools;
using IndividualLab.View.WIndows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace IndividualLab
{
    /// <summary>
    /// Главное окно программы.
    /// </summary>
    public partial class MainWindow : Window
    {

        #region DynamicCreatingUIElements
        /// <summary>
        /// Поле для решения, нужно ли отрисовывать элементы интерфейса для симплекс-метода.
        /// </summary>
        private bool simplexMethodIsChoosen { get; set; } = false;
        /// <summary>
        /// Поле для решения, нужно ли открывать диалоговое окно выбора базиса.
        /// </summary>
        private bool simplexMethodBasisIsArtificial { get; set; } = false;
        /// <summary>
        /// Поле для решения, нужно ли отрисовывать элементы интерфейса для графического метода.
        /// </summary>
        private bool graphicalMethodSelected { get; set; } = false;
        #endregion

        #region Utils
        /// <summary>
        /// Поле для вывода таблицы для ввода целевой функции.
        /// </summary>
        internal List<String> matrixHeadersForTargetFunction { get; set; }
        /// <summary>
        /// Поле для вывода таблицы для ввода ограничений.
        /// </summary>
        internal List<String> matrixHeadersForRestrictions { get; set; }
        #endregion

        /// <summary>
        /// Поле для хранения базиса, если базис выбранный.
        /// </summary>
        public static List<int> selectedBasis { get; set; }
        public MainWindow()
        {
            matrixHeadersForTargetFunction = new List<string>
            {
                "c"+Utils.makeLowerIndex(1),
                "c"+Utils.makeLowerIndex(2),
                "c"+Utils.makeLowerIndex(3),
                "c"+Utils.makeLowerIndex(4),
                "c"+Utils.makeLowerIndex(5),
                "c"+Utils.makeLowerIndex(6),
                "c"+Utils.makeLowerIndex(7),
                "c"+Utils.makeLowerIndex(8),
                "c"+Utils.makeLowerIndex(9),
                "c"+Utils.makeLowerIndex(10),
                "c"+Utils.makeLowerIndex(11),
                "c"+Utils.makeLowerIndex(12),
                "c"+Utils.makeLowerIndex(13),
                "c"+Utils.makeLowerIndex(14),
                "c"+Utils.makeLowerIndex(15),
                "c"+Utils.makeLowerIndex(16),
                "c",
            };
            matrixHeadersForRestrictions = new List<string>
            {
                "a"+Utils.makeLowerIndex(1),
                "a"+Utils.makeLowerIndex(2),
                "a"+Utils.makeLowerIndex(3),
                "a"+Utils.makeLowerIndex(4),
                "a"+Utils.makeLowerIndex(5),
                "a"+Utils.makeLowerIndex(6),
                "a"+Utils.makeLowerIndex(7),
                "a"+Utils.makeLowerIndex(8),
                "a"+Utils.makeLowerIndex(9),
                "a"+Utils.makeLowerIndex(10),
                "a"+Utils.makeLowerIndex(11),
                "a"+Utils.makeLowerIndex(12),
                "a"+Utils.makeLowerIndex(13),
                "a"+Utils.makeLowerIndex(14),
                "a"+Utils.makeLowerIndex(15),
                "a"+Utils.makeLowerIndex(16),
                "b",
            };
            selectedBasis = new List<int>();
            InitializeComponent();
        }

        #region WindowFunctions
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
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else WindowState = WindowState.Maximized;

        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            bool flag = true;
            int rows = matrixGrid.RowDefinitions.Count;
            int columns = matrixGrid.ColumnDefinitions.Count;
            if (rows == 0 || columns == 0) { MessageBox.Show("Неверно введённые данные!", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            int numberOfX = -1;
            int numberOfRestrictions = -1;
            SolutionMethod solutionMethod = SolutionMethod.Graphical;
            List<string> dataAboutTask = new List<string>();
            Fraction[,] restrictions = new Fraction[rows - 3, columns - 1];
            List<Fraction> targetFun = new List<Fraction>();
            List<string> bufferMatrix = new List<string>();

            foreach (var child in inputStackPanel.Children)
            {
                if (child is TextBox textBox)
                {
                    if (flag)
                    {
                        try
                        {
                            numberOfX = int.Parse(textBox.Text);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Количество переменных должно быть от 1 до 16", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        flag = false;
                        continue;
                    }
                    try
                    {
                        numberOfRestrictions = int.Parse(textBox.Text);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Количество ограничений должно быть от 1 до 16", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                if (child is ComboBox comboBox)
                {
                    var selectedValue = comboBox.SelectedItem;

                    if (selectedValue == null)
                    {
                        MessageBox.Show("Все поля для данных должны быть заполнены", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (selectedValue is ComboBoxItem comboBoxItem)
                    {
                        dataAboutTask.Add(comboBoxItem.Content.ToString());
                    }
                    else
                    {
                        dataAboutTask.Add(selectedValue.ToString());
                    }
                }
            }
            if (dataAboutTask[1] == "Десятичные") { Utils.doubleFlag = true; }
            else Utils.doubleFlag = false;
            foreach (var child in matrixGrid.Children)
            {
                if (child is TextBox textBox)
                {
                    string temp = textBox.Text;
                    bufferMatrix.Add(temp);
                }
            }
            int counter = 0;
            try
            {
                for (int i = 0; i < rows - 2; i++)
                {
                    for (int j = 0; j < columns - 1; j++)
                    {
                        if (i == 0) targetFun.Add(Fraction.Parse(bufferMatrix[counter]));
                        else restrictions[i - 1, j] = Fraction.Parse(bufferMatrix[counter]);
                        counter++;
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Некорректный ввод данных в матрицу ограничений и целевой функции!", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (dataAboutTask[2] == "Минимизировать") Utils.minimize = true; else Utils.minimize = false;

            if (dataAboutTask[0] == "Симплекс")
            {
                Utils.graphicalMethod = false;
                solutionMethod = SolutionMethod.Simplex;
                if (dataAboutTask[3] == "Пошаговый") Utils.stepByStepMethod = true; else Utils.stepByStepMethod = false;
                if (dataAboutTask[4] == "Выбранный базис")
                {
                    if (numberOfRestrictions > numberOfX) { MessageBox.Show("Неверное кол-во ограничений!", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                    DialogWindowBasisSelection basisSelectionWIndow = new DialogWindowBasisSelection(numberOfX, numberOfRestrictions);
                    var dialog = basisSelectionWIndow.ShowDialog();
                    if (dialog == false) { selectedBasis = new List<int>(); selectedBasis.Clear(); return; }
                    if (selectedBasis.Count != numberOfRestrictions) { selectedBasis = new List<int>(); selectedBasis.Clear(); MessageBox.Show("Неверное кол-во базисных переменных!", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                    Utils.artificialBasisMethod = false;
                }
                else { selectedBasis = null; Utils.artificialBasisMethod = true; }
            }
            else
            {
                Utils.graphicalMethod = true;
                if (dataAboutTask[3] == "Выбранный базис")
                {
                    DialogWindowBasisSelection basisSelectionWIndow = new DialogWindowBasisSelection(numberOfX, numberOfRestrictions);
                    var dialog = basisSelectionWIndow.ShowDialog();
                    if (dialog == false) { selectedBasis = new List<int>(); selectedBasis.Clear(); return; }
                    if (numberOfX != 2)
                    {
                        if (selectedBasis.Count != numberOfRestrictions) { selectedBasis = new List<int>(); selectedBasis.Clear(); MessageBox.Show("Неверное кол-во базисных переменных!", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                    }
                    Utils.artificialBasisMethod = false;
                }
                else { selectedBasis = null; }
            }


            InputData data = new InputData(numberOfX, numberOfRestrictions, solutionMethod, targetFun, restrictions, dataAboutTask[2], selectedBasis);
            var error = Controller.DoTask(data, this);
            if (error != null)
            {
                MessageBox.Show(error.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                if (selectedBasis != null) selectedBasis.Clear();
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try 
            { 
                Random rnd = new Random();
                foreach (var child in matrixGrid.Children)
                {
                    if (child is TextBox textBox)
                    {
                        textBox.Text = rnd.Next(0, 5).ToString();
                    }
                }
            }
            catch(Exception) 
            {
                MessageBox.Show("Произошла неизвестная ошибка.","Ошибка",MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private void openItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    Title = "Выберите JSON файл"
                };

                if (!(openFileDialog.ShowDialog() == true))
                {
                    return;
                }

                List<List<string>> jsonMatrix = new List<List<string>>();

                try
                {
                    string jsonFilePath = openFileDialog.FileName;
                    string jsonString = File.ReadAllText(jsonFilePath);
                    jsonMatrix = JsonSerializer.Deserialize<List<List<string>>>(jsonString);
                }
                catch (Exception)
                {
                    MessageBox.Show("Файл пуст или содержит неверные данные.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                fileNameLabel.Content = openFileDialog.SafeFileName;
                int amount_of_x = jsonMatrix[0].Count - 1;
                amountOfRestrictionsTextBox.Text = (jsonMatrix.Count - 1).ToString();
                amountOfXTextBox.Text = (amount_of_x).ToString();

                int cols = 0;
                int rows = 0;
                foreach (var child in matrixGrid.Children)
                {
                    if (!(child is TextBox textBox))
                        continue;
                    if (cols == amount_of_x + 1)
                    {
                        cols = 0;
                        rows++;
                    }
                    textBox.Text = jsonMatrix[rows][cols];
                    cols++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Данные в файле некорректны!" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        private void saveItem_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(amountOfRestrictionsTextBox.Text) || string.IsNullOrEmpty(amountOfXTextBox.Text))
            {
                MessageBox.Show("Поля для кол-ва ограничений и переменных должны быть заполнены!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(typeOfNumbersComboBox.Text))
            {
                MessageBox.Show("Тип дробей должен быть выбран!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                int amount_of_x = int.Parse(amountOfXTextBox.Text);
                int amount_of_restrictions = int.Parse(amountOfRestrictionsTextBox.Text);
                Fraction[,] matrix = new Fraction[amount_of_restrictions + 1, amount_of_x + 1];
                int rows = 0;
                int cols = 0;
                List<List<string>> jsonMatrix = new List<List<string>>();
                List<string> rowValues = new List<string>();
                foreach (var child in matrixGrid.Children)
                {
                    if (!(child is TextBox textBox))
                    {
                        continue;
                    }
                    if (cols == amount_of_x + 1)
                    {
                        cols = 0;
                        rows++;
                        jsonMatrix.Add(new List<string>(rowValues));
                        rowValues.Clear();
                    }
                    rowValues.Add(textBox.Text);
                    cols++;
                }
                jsonMatrix.Add(rowValues);
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "JSON Files (*.json)|*.json",
                    Title = "Сохранить матрицу как JSON"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        string json = JsonSerializer.Serialize(jsonMatrix, new JsonSerializerOptions { WriteIndented = true });
                        File.WriteAllText(saveFileDialog.FileName, json);
                        MessageBox.Show("Файл успешно сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при сохранении файла: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Произошла неизвестная ошибка:" + ex.Message,"Ошибка",MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        private void mainWindowGrid_Loaded(object sender, RoutedEventArgs e)
        {
            methodComboBox.IsEnabled = false;
        }
        #endregion

        #region FunctionsForValidationsAmountOfXTextBoxAndAmountOfRestrictionsTextBox
        private void amountOfXTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            DataObject.AddPastingHandler(amountOfXTextBox, new DataObjectPastingEventHandler(amountOfXTextBox_OnPaste));
        }
        private void amountOfRestrictionsTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            DataObject.AddPastingHandler(amountOfRestrictionsTextBox, new DataObjectPastingEventHandler(amountOfRestrictionsTextBox_OnPaste));
        }
        private void amountOfRestrictionsTextBox_OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string pasteText = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextAllowed((sender as TextBox).Text + pasteText))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
        private void amountOfXTextBox_OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string pasteText = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextAllowed((sender as TextBox).Text + pasteText))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
        private bool IsTextAllowed(string text)
        {
            // Проверка, является ли введенный текст числом и находится ли в диапазоне от 0 до 16
            if (int.TryParse(text, out int result))
            {
                return result >= 0 && result <= 16;
            }
            return false;
        }
        private void amountOfXTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!IsTextAllowed(e.Text))
            {
                e.Handled = true;
            }
        }
        private void amountOfRestrictionsTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!IsTextAllowed(e.Text))
            {
                e.Handled = true;
            }
        }
        #endregion

        #region DynamicCreatingTableForInput_TargetFunction_And_Restrictions
        private void amountOfRestrictionsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (!String.IsNullOrEmpty(amountOfRestrictionsTextBox.Text))
            {
                if (int.Parse(amountOfRestrictionsTextBox.Text) <= 16 && int.Parse(amountOfRestrictionsTextBox.Text) > 0)
                {
                    if (!(String.IsNullOrEmpty(amountOfRestrictionsTextBox.Text) || String.IsNullOrEmpty(amountOfXTextBox.Text)))
                    {
                        if (int.Parse(amountOfRestrictionsTextBox.Text) <= 16 && int.Parse(amountOfXTextBox.Text) > 0)
                        {
                            ClearGrid(matrixGrid);
                            createTable();
                            methodComboBox.IsEnabled = true;
                        }
                        else
                        {
                            MessageBox.Show("Число ограничений должно быть в диапазоне 1 до 16", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                            amountOfRestrictionsTextBox.Text = "";
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Число ограничений должно быть в диапазоне 1 до 16", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    amountOfRestrictionsTextBox.Text = "";
                }
            }
            else
            {
                ClearGrid(matrixGrid);
                methodComboBox.IsEnabled = false;
            }

        }
        private void amountOfXTextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            if (!String.IsNullOrEmpty(amountOfXTextBox.Text))
            {
                if (int.Parse(amountOfXTextBox.Text) <= 16 && int.Parse(amountOfXTextBox.Text) > 0)
                {
                    if (!(String.IsNullOrEmpty(amountOfRestrictionsTextBox.Text) || String.IsNullOrEmpty(amountOfXTextBox.Text)))
                    {
                        if (int.Parse(amountOfXTextBox.Text) <= 16 && int.Parse(amountOfXTextBox.Text) > 0)
                        {
                            ClearGrid(matrixGrid);
                            createTable();
                            methodComboBox.IsEnabled = true;
                        }
                        else
                        {
                            MessageBox.Show("Число переменных должно быть в диапазоне 1 до 16", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                            amountOfXTextBox.Text = "";
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Число переменных должно быть в диапазоне 1 до 16", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    amountOfXTextBox.Text = "";
                }
            }
            else
            {
                ClearGrid(matrixGrid);
                methodComboBox.IsEnabled = false;
            }
        }
        private void createTable()
        {

            int amountOfX = 0;
            int amountOfRestrictions = 0;
            try
            {
                amountOfX = int.Parse(amountOfXTextBox.Text);
                amountOfRestrictions = int.Parse(amountOfRestrictionsTextBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            matrixGrid.Background = Brushes.LightGray;

            matrixGrid.Children.Clear();
            matrixGrid.RowDefinitions.Clear();
            matrixGrid.ColumnDefinitions.Clear();


            mainGrid.ColumnDefinitions[1].Width = new GridLength(amountOfX * 50 + 80 + 33, GridUnitType.Pixel);
            matrixAndButtonAcceptGrid.RowDefinitions[0].Height = new GridLength(amountOfRestrictions * 30 + 60 + 33, GridUnitType.Pixel);

            for (int j = 0; j <= amountOfX + 1; j++)
            {
                matrixGrid.ColumnDefinitions.Add(new ColumnDefinition()); // Последний столбец для 'b'
            }

            for (int i = 0; i <= amountOfRestrictions + 2; i++)
            {
                matrixGrid.RowDefinitions.Add(new RowDefinition());
            }
            int width = 50;
            Thickness margin = new Thickness(0);
            Thickness padding = new Thickness(0);
            for (int row = 0; row <= amountOfRestrictions + 2; row++) //amountOfRest = 3
            {
                for (int col = 0; col <= amountOfX + 1; col++)  //amountOfX = 5
                {
                    if (row == 0 && col == 0) // Пустая ячейка в верхнем левом углу
                    {
                        Label emptyLabel = createLabelForInputTable("", width, margin, padding);
                        Grid.SetRow(emptyLabel, row);
                        Grid.SetColumn(emptyLabel, col);
                        matrixGrid.Children.Add(emptyLabel);
                    }
                    else if (row == 0 && col != 0)
                    {
                        if (col < amountOfX + 1)
                        {

                            Label headerColumnLabel = createLabelForInputTable(matrixHeadersForTargetFunction[col - 1], width, margin, padding);
                            Grid.SetRow(headerColumnLabel, row);
                            Grid.SetColumn(headerColumnLabel, col);
                            matrixGrid.Children.Add(headerColumnLabel);
                        }
                        else
                        {
                            Label headerColumnLabel = createLabelForInputTable("c", width, margin, padding);
                            Grid.SetRow(headerColumnLabel, row);
                            Grid.SetColumn(headerColumnLabel, col);
                            matrixGrid.Children.Add(headerColumnLabel);
                        }
                    }
                    else if (row == 1 && col == 0)
                    {
                        Label label = createLabelForInputTable("f(x)", width, margin, padding);
                        Grid.SetRow(label, row);
                        Grid.SetColumn(label, col);
                        matrixGrid.Children.Add(label);
                    }
                    else if (row == 1 && col != 0)
                    {
                        TextBox textBox = createTextBoxForInputTable(width, margin, padding);
                        Grid.SetRow(textBox, row);
                        Grid.SetColumn(textBox, col);
                        matrixGrid.Children.Add(textBox);
                    }
                    else if (row == 2 && col == 0)
                    {
                        Label emptyLabel = createLabelForInputTable("", width, margin, padding);
                        Grid.SetRow(emptyLabel, row);
                        Grid.SetColumn(emptyLabel, col);
                        matrixGrid.Children.Add(emptyLabel);
                    }
                    else if (row == 2 && col != 0)
                    {
                        if (col < amountOfX + 1)
                        {
                            Label headerColumnLabel = createLabelForInputTable(matrixHeadersForRestrictions[col - 1], width, margin, padding);
                            Grid.SetRow(headerColumnLabel, row);
                            Grid.SetColumn(headerColumnLabel, col);
                            matrixGrid.Children.Add(headerColumnLabel);
                        }
                        else
                        {
                            Label headerColumnLabel = createLabelForInputTable("b", width, margin, padding);
                            Grid.SetRow(headerColumnLabel, row);
                            Grid.SetColumn(headerColumnLabel, col);
                            matrixGrid.Children.Add(headerColumnLabel);
                        }
                    }
                    else if (col == 0)
                    {
                        Label emptyLabel = createLabelForInputTable("f(x" + Utils.makeLowerIndex(row - 2) + ")", width, margin, padding);
                        Grid.SetRow(emptyLabel, row);
                        Grid.SetColumn(emptyLabel, col);
                        matrixGrid.Children.Add(emptyLabel);
                    }
                    else
                    {
                        TextBox textBox = createTextBoxForInputTable(width, margin, padding);
                        Grid.SetRow(textBox, row);
                        Grid.SetColumn(textBox, col);
                        matrixGrid.Children.Add(textBox);
                    }

                }
            }



        }
        private Label createLabelForInputTable(string content, int width, Thickness margin, Thickness padding)
        {
            Label emptyLabel = new Label
            {
                HorizontalContentAlignment = HorizontalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = margin,
                Padding = padding,
                Width = width,
                Height = 30,
                Content = content,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                FontSize = 18,
            };

            return emptyLabel;
        }
        private TextBox createTextBoxForInputTable(int width, Thickness margin, Thickness padding)
        {
            TextBox textBox = new TextBox
            {
                HorizontalContentAlignment = HorizontalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = margin,
                Padding = padding,
                Width = width,
                Height = 30,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                FontSize = 18,

            };
            textBox.KeyDown += TextBox_KeyDown;
            return textBox;
        }
        private void ClearGrid(Grid grid)
        {
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
        }
        #endregion

        #region DynamicCreatingUIElementsInputStackPanel
        private void methodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)methodComboBox.SelectedItem;


            if (simplexMethodIsChoosen)
            {
                inputStackPanel.Children.RemoveAt(inputStackPanel.Children.Count - 1);
                inputStackPanel.Children.RemoveAt(inputStackPanel.Children.Count - 1);
                inputStackPanel.Children.RemoveAt(inputStackPanel.Children.Count - 1);
                inputStackPanel.Children.RemoveAt(inputStackPanel.Children.Count - 1);
                simplexMethodBasisIsArtificial = false;
                simplexMethodIsChoosen = false;

            }
            if (graphicalMethodSelected)
            {
                inputStackPanel.Children.RemoveAt(inputStackPanel.Children.Count - 1);
                inputStackPanel.Children.RemoveAt(inputStackPanel.Children.Count - 1);
                graphicalMethodSelected = false;
            }

            if (selectedItem.Content.ToString() == "Симплекс" && !simplexMethodIsChoosen)
            {
                Label solutionRegime = new Label
                {
                    Content = "Режим решения",
                    Width = 200,
                    Height = 30,
                    FontSize = 14,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                };

                ComboBox solutionRegimeComboBox = new ComboBox
                {
                    Width = 180,
                    Height = 30,
                    FontSize = 16,
                };
                solutionRegimeComboBox.Items.Add("Автоматический");
                solutionRegimeComboBox.Items.Add("Пошаговый");

                Label basisLabel = new Label
                {
                    Content = "Базис",
                    Width = 200,
                    Height = 30,
                    FontSize = 14,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                };

                ComboBox basisComboBox = new ComboBox
                {
                    Width = 180,
                    Height = 30,
                    FontSize = 16,
                };

                basisComboBox.Items.Add("Искуственный базис");
                basisComboBox.Items.Add("Выбранный базис");
                inputStackPanel.Children.Add(solutionRegime);
                inputStackPanel.Children.Add(solutionRegimeComboBox);
                inputStackPanel.Children.Add(basisLabel);
                inputStackPanel.Children.Add(basisComboBox);
                simplexMethodIsChoosen = true;

            }
            else if (selectedItem.Content.ToString() == "Графический" && !graphicalMethodSelected)
            {
                Label basisLabel = new Label
                {
                    Content = "Базис",
                    Width = 200,
                    Height = 30,
                    FontSize = 14,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                };

                ComboBox basisComboBox = new ComboBox
                {
                    Width = 180,
                    Height = 30,
                    FontSize = 16,
                };

                basisComboBox.Items.Add("Автоматический базис");
                basisComboBox.Items.Add("Выбранный базис");
                inputStackPanel.Children.Add(basisLabel);
                inputStackPanel.Children.Add(basisComboBox);
                graphicalMethodSelected = true;
            }


        }

        #endregion

        #region Utils
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox currentTextBox = sender as TextBox;

                if (currentTextBox == null)
                    return;

                UIElement nextElement = currentTextBox.PredictFocus(FocusNavigationDirection.Right) as UIElement;
                if (nextElement == null)
                {
                    nextElement = currentTextBox.PredictFocus(FocusNavigationDirection.Down) as UIElement;
                }
                if (nextElement != null && nextElement is TextBox)
                {
                    nextElement.Focus();
                    e.Handled = true; 
                }
            }
        }

        #endregion

    }
}
