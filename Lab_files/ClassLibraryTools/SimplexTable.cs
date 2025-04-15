using IndividualLab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClassLibraryTools
{
    /// <summary>
    /// Класс, который является симплекс-таблицей.
    /// </summary>
    public class SimplexTable
    {
        /// <summary>
        /// Событие, вызываемое при нажатии на опорный элемент в симплекс-таблице при пошаговом методе решения.
        /// </summary>
        /// <param name="oldTable">Предыдущая версия симплекс-таблицы.</param>
        /// <param name="newTable">Обновленная версия симплекс-таблицы.</param>
        public event Action<SimplexTable, SimplexTable> OnLabelClicked;
        /// <summary>
        /// Базисные переменные, представленные в виде массива индексов.
        /// </summary>
        public List<int> basis { get; set; }
        /// <summary>
        /// Свободные переменные, представленные в виде массива индексов.
        /// </summary>
        public List<int> free { get; set; }
        /// <summary>
        /// Матрица ограничений и целевой функции, представленная в виде двумерного массива коэффицентов.
        /// </summary>
        public Fraction[,] matrix { get; set; }
        /// <summary>
        /// Холст <see cref="Canvas"/>, на котором отображена таблица.
        /// </summary>
        public Canvas renderedTable { get; set; }
        /// <summary>
        /// Холст <see cref="Canvas"/>, на котором изображён выбранные опорный элемент у данной таблицы <see cref="SimplexTable"/>.
        /// </summary>
        public Canvas renderedTablesLog { get; set; }
        /// <summary>
        /// Количество переменных.
        /// </summary>
        public int numberOfX { get; set; }
        /// <summary>
        /// Флаг <see cref="artificialTable"/> = <see cref="true"/> - таблица является искусственной. <br></br>
        /// Флаг <see cref="artificialTable"/> = <see cref="false"/> - таблица не является искусственной.
        /// </summary>
        private bool artificialTable { get; set; }
        /// <summary>
        /// Флаг <see cref="idleStep"/> = <see cref="true"/> - надо сделать холостой шаг, либо удалить искуственные переменные, независящие от начальный переменных.
        /// Иначе <see cref="idleStep"/> = <see cref="false"/>.
        /// </summary>
        private bool idleStep { get; set; }
        /// <summary>
        /// Количество ограничений.
        /// </summary>
        public int numberOfRestrictions { get; set; }
        /// <summary>
        /// Номер итерации.
        /// </summary>
        public int iteration { get; set; }
        /// <summary>
        /// Строка, в которой описан конец итерации.
        /// </summary>
        public string resultIteration { get; set; }
        /// <summary>
        /// Опорный элемент, представленный в виде индекса строки и столбца в матрице <see cref="matrix"/>.
        /// </summary>
        public List<int> selectedRefElement { get; set; }
        /// <summary>
        /// Все возможные опорные элементы, представленные в виде словаря - column1 : {row1, row2} ит.д
        /// </summary>
        internal Dictionary<int, List<int>> permissionPairs { get; set; }
        /// <summary>
        /// Множество для хранения опорных строк.
        /// </summary>
        private HashSet<int> permissionColumns { get; set; }
        /// <summary>
        /// Значение целевой функции в данной <see cref="SimplexTable"/>.
        /// </summary>
        public Fraction functionValue { get; set; }
        public SimplexTable(List<int> basis, List<int> free, Fraction[,] matrix, int numberOfX, int numberOfRestrictions, int iteration = 0, bool free_x_finding = true)
        {
            this.basis = basis;
            this.idleStep = false;
            this.numberOfX = numberOfX;
            this.matrix = matrix;
            this.functionValue = matrix[matrix.GetLength(0) - 1, matrix.GetLength(1) - 1];
            this.numberOfRestrictions = numberOfRestrictions;
            permissionPairs = new Dictionary<int, List<int>>();
            permissionColumns = new HashSet<int>();
            this.iteration = iteration;
            if (Utils.artificialBasisMethod) artificialTable = true;
            if (iteration == 0 && free_x_finding) { this.free = new List<int>(); findFreeX(); }
            else this.free = free;
            var result = FindPermRowAndColumns();
            if (artificialTable && resultIteration.Contains("("))
            {
                if (functionValue != 0)
                {
                    resultIteration = "Система ограничений несовместна, искуственный базис найти невозможно.";
                }
                else
                {
                    if (basis.Count(x => Utils.artBasis.Contains(x)) > 0)
                    {
                        findPermissionElementsForIdleStep();
                        idleStep = true;
                    }
                }
            }
            if (!Utils.graphicalMethod) renderTable();
            if (result == "Next" && !Utils.stepByStepMethod) renderTablesLog();
            Utils.tables.Add(this);
        }
        /// <summary>
        /// Находит свободные переменные.
        /// </summary>
        public void findFreeX()
        {
            for (int x = 0; x < numberOfX; x++)
            {
                if (!basis.Contains(x)) free.Add(x);
            }
        }
        /// <summary>
        /// Шаг симплекс таблицы.
        /// </summary>
        /// <param name="permission_row">
        /// Индекс опорной строки.
        /// </param>
        /// <param name="permission_column">
        /// Индекс опорного столбца.
        /// </param>
        /// <returns>
        /// Возвращает новую матрицу коэффицентов.
        /// </returns>
        public Fraction[,] simplexStep(int permission_row, int permission_column)
        {
            Fraction ref_element = matrix[permission_row, permission_column];
            Fraction[,] matrix_for_next_iteration = new Fraction[matrix.GetLength(0), matrix.GetLength(1)];

            Fraction new_ref_element = Fraction.swap(ref_element);
            matrix_for_next_iteration[permission_row, permission_column] = new_ref_element;

            for (int col = 0; col < matrix.GetLength(1); col++)
            {
                if (col != permission_column)
                {
                    matrix_for_next_iteration[permission_row, col] = matrix[permission_row, col] / ref_element;
                }
            }
            for (int row = 0; row < matrix.GetLength(0); row++)
            {
                if (row != permission_row)
                {
                    matrix_for_next_iteration[row, permission_column] =
                        (matrix[row, permission_column] / ref_element) * new Fraction(-1, 1);
                }
            }

            for (int row = 0; row < matrix.GetLength(0); row++)
            {
                for (int col = 0; col < matrix.GetLength(1); col++)
                {
                    if (row != permission_row && col != permission_column)
                    {
                        matrix_for_next_iteration[row, col] = matrix[row, col] -
                            matrix[row, permission_column] * matrix_for_next_iteration[permission_row, col];
                    }
                }
            }
            return matrix_for_next_iteration;
        }
        /// <summary>
        /// Отрисовывает симплекс-таблицу <see cref="SimplexTable"/> на холсте <see cref="Canvas"/>.
        /// </summary>
        public void renderTable()
        {
            List<List<string>> rows = new List<List<string>>();
            for (int row = -1; row < matrix.GetLength(0); row++)
            {
                List<string> temp = new List<string>();
                for (int col = -1; col < matrix.GetLength(1); col++)
                {
                    if (row == -1 && col == -1)
                        temp.Add("X" + Utils.makeUpperIndex(iteration));
                    else if (row == -1 && col != -1 && col != matrix.GetLength(1) - 1)
                    {
                        temp.Add("X" + Utils.makeLowerIndex(free[col] + 1));
                    }
                    else if (row == -1 && col == matrix.GetLength(1) - 1)
                    {
                        temp.Add("b");
                    }
                    else if (row != -1 && col == -1 && row != matrix.GetLength(0) - 1)
                    {
                        temp.Add("X" + Utils.makeLowerIndex(basis[row] + 1));
                    }
                    else if (row != -1 && col != -1)
                    {
                        if (Utils.doubleFlag)
                            temp.Add(((double)matrix[row, col]).ToString());
                        else
                            temp.Add(matrix[row, col]);
                    }
                    else if (row == matrix.GetLength(0) - 1 && col == -1)
                        temp.Add("F");
                }
                rows.Add(temp);
            }

            double maxLabelWidth = Utils.GetMaxLabelWidth(rows, 16);
            maxLabelWidth = Math.Max(maxLabelWidth, 70);
            Canvas canvas = new Canvas();
            double height = 40;
            double currentX = 0;
            double currentY = 0;
            for (int row = -1; row < rows.Count - 1; row++)
            {
                for (int col = -1; col < matrix.GetLength(1); col++)
                {
                    Label label = new Label();
                    label.Content = rows[row + 1][col + 1];
                    label.FontSize = 16;
                    if (permissionPairs.ContainsKey(col))
                    {
                        if (permissionPairs[col].Contains(row))
                        {
                            if (Utils.stepByStepMethod)
                            {
                                label.MouseDown += Label_Click;
                                label.Tag = Tuple.Create(row, col);
                            }

                            if (selectedRefElement[0] == row && selectedRefElement[1] == col)
                            {
                                label.BorderBrush = new SolidColorBrush(Color.FromArgb(100, 255, 0, 0));
                                label.BorderThickness = new Thickness(4);
                            }
                            else
                            {
                                label.BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 255, 0));
                                label.BorderThickness = new Thickness(3);
                            }
                        }
                        else
                        {
                            label.BorderBrush = Brushes.Black;
                            label.BorderThickness = new Thickness(2);
                        }
                    }
                    else
                    {
                        label.BorderBrush = Brushes.Black;
                        label.BorderThickness = new Thickness(2);
                    }
                    label.Height = height;
                    label.Width = maxLabelWidth;
                    label.VerticalContentAlignment = VerticalAlignment.Center;
                    label.HorizontalContentAlignment = HorizontalAlignment.Center;

                    Canvas.SetLeft(label, currentX);
                    Canvas.SetTop(label, currentY);
                    canvas.Children.Add(label);
                    currentX += label.Width;
                }

                currentY += height;
                currentX = 0;
            }

            canvas.Height = 0;
            foreach (var child in canvas.Children)
            {
                if (child is FrameworkElement elem)
                {
                    canvas.Height += elem.Height;
                }
            }
            canvas.Margin = new Thickness(5);
            canvas.Height = (canvas.Height / rows[0].Count) + 10;
            canvas.Width = (maxLabelWidth * rows[0].Count) + 10;
            if (artificialTable) canvas.Tag = "A" + iteration;
            else canvas.Tag = iteration.ToString();
            canvas.HorizontalAlignment = HorizontalAlignment.Left;
            renderedTable = canvas;
        }
        /// <summary>
        /// Отрисовывает выбранный опорный элемент у данной таблицы на холст <see cref="Canvas"/>.
        /// </summary>
        public void renderTablesLog()
        {
            Canvas canvas = new Canvas();
            Label label = new Label();
            label.FontSize = 20;
            label.Height = 40;
            label.VerticalContentAlignment = VerticalAlignment.Center;
            label.HorizontalContentAlignment = HorizontalAlignment.Center;
            string str;
            if (!Utils.doubleFlag)
                str = matrix[selectedRefElement[0], selectedRefElement[1]];
            else
                str = ((double)matrix[selectedRefElement[0], selectedRefElement[1]]).ToString();
            label.Content = "Выбран опорный элемент а" + " " +
                Utils.makeLowerIndex(basis[selectedRefElement[0]] + 1) + " " +
                Utils.makeLowerIndex(free[selectedRefElement[1]] + 1) +
                " = " + str;
            canvas.Width = label.Width;
            canvas.Height = label.Height;
            canvas.Children.Add(label);
            renderedTablesLog = canvas;

        }
        /// <summary>
        /// Поиск опорного(-ых) элемента(-ов).
        /// </summary>
        /// <returns>
        /// Возвращает результат симплекс таблицы, можно ли сделать следующую итерацию или нет.
        /// </returns>
        public string FindPermRowAndColumns()
        {
            List<int> permission_columns;
            int permission_column = -1;
            int permission_row = -1;
            List<int> bad_columns = new List<int>();
            permissionColumns.Clear();
            permissionPairs.Clear();
            permission_columns = findMinPermissionColumns(bad_columns);

            if (permission_columns.Count == 0)
            {
                bool check = checkForMatrixThatFreeXArePositiveValue();
                if (!check) this.resultIteration = "Выбран неверный базис.";
                else this.resultIteration = findPlanToOutput();
                return null;
            }

            foreach (int col in permission_columns)
            {
                permission_row = findPermissionRow(col);
                if (permission_row != -1)
                {
                    permission_column = col;
                    break;
                }
                else
                {
                    bad_columns.Add(col);
                }
            }


            while (permission_row == -1)
            {
                if (bad_columns.Count >= matrix.GetLength(1) - 1)
                {
                    this.resultIteration = "Задача неразрешима.";
                    return null;
                }
                permission_columns = findMinPermissionColumns(bad_columns);
                foreach (int col in permission_columns)
                {
                    permission_row = findPermissionRow(col);
                    if (permission_row != -1)
                    {
                        permission_column = col;
                        break;
                    }
                    else
                    {
                        bad_columns.Add(col);
                    }
                }
            }

            foreach (int col in permissionColumns)
            {
                List<int> permission_rows = findPermissionRows(col);
                if (permission_rows.Count == 0)
                    continue;
                else
                {
                    permissionPairs.Add(col, permission_rows);
                }
            }
            selectedRefElement = new List<int>() { permission_row, permission_column };
            this.resultIteration = "Next";
            return "Next";
        }
        /// <summary>
        /// Находит минимальный опорный столбец.
        /// </summary>
        /// <param name="bad_columns">
        /// Столбцы, которые не подходят для выбора в них опорной строки.
        /// </param>
        /// <returns>
        /// Список индексов опорных столбцов.
        /// </returns>
        private List<int> findMinPermissionColumns(List<int> bad_columns)
        {
            List<int> permission_columns = new List<int>();
            Fraction minValue = new Fraction(0, 1);

            for (int i = 0; i < matrix.GetLength(1) - 1; i++)
            {
                Fraction t1 = matrix[matrix.GetLength(0) - 1, i];
                if (t1 < 0)
                    permissionColumns.Add(i);
                else if (t1 < 0 && bad_columns.Contains(i))
                    permissionColumns.Remove(i);
                else if (t1 >= 0)
                    bad_columns.Add(i);

                if (t1 < minValue && !bad_columns.Contains(i))
                {
                    minValue = matrix[matrix.GetLength(0) - 1, i];
                    permission_columns.Clear();
                    permission_columns.Add(i);
                }
                else if (t1.Equals(minValue) && !bad_columns.Contains(i))
                {
                    permission_columns.Add(i);
                }
            }

            return permission_columns;
        }
        /// <summary>
        /// Находит индекс опорной строки.
        /// </summary>
        /// <param name="permission_column">
        /// Возвращает индекс опорной строки.
        /// </param>
        /// <returns></returns>
        private int findPermissionRow(int permission_column)
        {
            int permission_row = -1;
            Fraction minRatio = new Fraction(int.MaxValue, 1);
            for (int i = 0; i < matrix.GetLength(0) - 1; i++)
            {
                if (matrix[i, permission_column] > 0 && matrix[i, matrix.GetLength(1) - 1] >= 0)
                {
                    Fraction ratio = matrix[i, matrix.GetLength(1) - 1] / matrix[i, permission_column];
                    if (ratio < minRatio)
                    {
                        minRatio = ratio;
                        permission_row = i;
                    }
                }
            }
            return permission_row;
        }
        /// <summary>
        /// Возвращает список индексов опорных строк.
        /// </summary>
        /// <param name="permission_column">
        /// Индекс опорного столбца.
        /// </param>
        /// <returns></returns>
        private List<int> findPermissionRows(int permission_column)
        {

            List<int> permission_rows = new List<int>();
            Fraction minRatio = new Fraction(int.MaxValue, 1);
            int permission_row = -1;
            for (int i = 0; i < matrix.GetLength(0) - 1; i++)
            {
                if (matrix[i, permission_column] > 0 && matrix[i, matrix.GetLength(1) - 1] >= 0)
                {
                    Fraction ratio = matrix[i, matrix.GetLength(1) - 1] / matrix[i, permission_column];
                    if (ratio < minRatio)
                    {
                        minRatio = ratio;
                        permission_row = i;
                        if (i > 0) permission_rows.Clear();
                    }
                    else if (ratio.Equals(minRatio))
                    {
                        permission_rows.Add(i);
                    }
                }
            }
            if (permission_row != -1)
                permission_rows.Add(permission_row);
            permission_rows.Sort();
            return permission_rows;
        }
        /// <summary>
        /// Проверка являются ли переменные отрицательными.
        /// </summary>
        /// <returns>
        /// <see langword="true""/> если базисные переменные не являются отрицательными.
        /// <see langword="false"/> если базисные переменные являются отрицательными.
        /// </returns>
        private bool checkForMatrixThatFreeXArePositiveValue()
        {
            for (int row = 0; row < matrix.GetLength(0) - 1; row++)
            {
                if (matrix[row, matrix.GetLength(1) - 1] < 0)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Преобразовывает план в строку.
        /// </summary>
        /// <returns>
        /// Возвращает план, преобразованный в строку.
        /// </returns>
        public string findPlanToOutput()
        {
            string str = "(";
            string[] plan = new string[numberOfX];
            for (int x = 0; x < numberOfX; x++)
            {
                if (basis.Contains(x))
                {
                    int index = basis.IndexOf(x);
                    if (!Utils.doubleFlag)
                        plan[x] = matrix[index, matrix.GetLength(1) - 1];
                    else
                        plan[x] = ((double)matrix[index, matrix.GetLength(1) - 1]).ToString();
                }
                else
                {
                    plan[x] = "0";
                }

            }
            for (int i = 0; i < plan.Length; i++)
            {
                str += plan[i].ToString();
                if (i != plan.Length - 1)
                    str += ", ";
            }
            str += ")";
            return str;
        }
        /// <summary>
        /// Событие, которое вызывается при нажатии на опорный элемент в таблице.
        /// </summary>
        /// <param name="sender">
        /// <see cref="Label"/>, который вызвал событие.
        /// </param>
        /// <param name="e">
        /// Данные события.
        /// </param>
        public void Label_Click(object sender, EventArgs e)
        {
            foreach (var elem in renderedTable.Children)
            {
                Label label1 = elem as Label;
                label1.IsEnabled = true;
                label1.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 255));
            }
            var label = sender as Label;
            if (label == null) return;
            label.IsEnabled = false;
            // Получаем индекс строки и столбца из Tag
            if (label.Tag is Tuple<int, int> coordinates)
            {
                int rowIndex = coordinates.Item1;
                int colIndex = coordinates.Item2;
                List<int> ref_elem = new List<int>() { colIndex, rowIndex };

                label.Background = new SolidColorBrush(Color.FromArgb(100, 0, 0, 255));
                selectedRefElement.Clear();
                selectedRefElement.Add(ref_elem[1]);
                selectedRefElement.Add(ref_elem[0]);
            }
            if (artificialTable) Utils.artificialBasisMethod = true;
            else Utils.artificialBasisMethod = false;
            renderTablesLog();
            SimplexTable table = Utils.DoIteration(this, idleStep);
            OnLabelClicked?.Invoke(this, table);
        }
        /// <summary>
        /// Нахождение опорных элементов при холостом шаге.
        /// </summary>
        /// <returns>
        /// Возвращает результат симплекс таблицы, можно ли сделать следующую итерацию или нет.
        /// </returns>
        public string findPermissionElementsForIdleStep()
        {
            List<int> rowsForDeleting = new List<int>();
            permissionPairs.Clear();
            int counter = 0;
            int numberOfXInCols = free.Count(x => !Utils.artBasis.Contains(x));
            for (int row = 0; row < basis.Count; row++)
            {
                if (!Utils.artBasis.Contains(basis[row]) || matrix[row, matrix.GetLength(1) - 1] != 0) continue;
                for (int col = 0; col < free.Count; col++)
                {
                    if (!Utils.artBasis.Contains(free[col]))
                    {
                        if (matrix[row, col] != 0) 
                        { 
                            if(!permissionPairs.ContainsKey(col))
                                permissionPairs.Add(col, new List<int>() { row });
                            else
                            {
                                List<int> list = permissionPairs[col];
                                list.Add(row);
                                permissionPairs[col] = list;
                            }
                        }
                        //элемент с этим же ключом был бы добалвен TODO
                        else { counter++; }
                    }
                }
                if (counter == numberOfXInCols) rowsForDeleting.Add(row);
                counter = 0;
            }

            rowsForDeleting.Sort();
            rowsForDeleting.Reverse();
            if (rowsForDeleting.Count > 0)
            {
                foreach (int row in rowsForDeleting)
                {
                    deleteRow(row);
                    basis.RemoveAt(row);
                }
                if (basis.Count(x => Utils.artBasis.Contains(x)) == 0)
                {
                    //renderTable();
                    return this.resultIteration = findPlanToOutput();
                }
            }

            if (permissionPairs.Count > 0)
            {
                int column = permissionPairs.Keys.First();
                selectedRefElement = new List<int>() { permissionPairs[column][0], column };
                if (!Utils.stepByStepMethod)
                {
                    renderTable();
                    renderTablesLog();
                }
                this.resultIteration = "Next";
                return "Next"; //findPlanToOutput(); TODO FIX THIS
            }

            if (basis.Count(x => Utils.artBasis.Contains(x)) > 0) { return resultIteration = "Искуственный базис найти невозможно!"; }
            this.resultIteration = findPlanToOutput();
            return null;

        }
        /// <summary>
        /// Удаление строки из матрицы коэффицентов <see cref="matrix"/>.
        /// </summary>
        /// <param name="row">
        /// Индекс строки.
        /// </param>
        private void deleteRow(int row)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            Fraction[,] newMatrix = new Fraction[rows - 1, cols];

            int newRow = 0;

            for (int i = 0; i < rows; i++)
            {
                if (i == row)
                {
                    continue;
                }
                for (int j = 0; j < cols; j++)
                {
                    newMatrix[newRow, j] = matrix[i, j];
                }
                newRow++;
            }
            matrix = newMatrix;
        }
    }
}
