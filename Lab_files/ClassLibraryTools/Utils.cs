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
    /// Класс, в котором находятся функции для различных подсчётов.
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// Список симплекс-таблиц <see cref="SimplexTable"/> всех итераций.
        /// </summary>
        public static List<SimplexTable> tables { get; set; } = new List<SimplexTable>();
        /// <summary>
        /// <see cref="stepByStepMethod"/> = <see langword="true"/> - пошаговый метод решения. <br></br>
        /// <see cref="stepByStepMethod"/> = <see langword="false"/> - автоматический метод решения. <br></br>
        /// </summary>
        public static bool stepByStepMethod { get; set; }
        /// <summary>
        /// <see cref="artificialBasisMethod"/> = <see langword="true"/> - метод искусственного базиса. <br></br>
        /// <see cref="artificialBasisMethod"/> = <see langword="false"/> - симплекс метод. <br></br>
        /// </summary>
        public static bool artificialBasisMethod { get; set; }
        /// <summary>
        /// <see cref="doubleFlag"/> = <see langword="true"/> - десятичные дроби. <br></br>
        /// <see cref="doubleFlag"/> = <see langword="false"/> - обыкновенные дроби. <br></br>
        /// </summary>
        public static bool doubleFlag { get; set; } = false;
        /// <summary>
        /// <see cref="minimize"/> = <see langword="true"/> - задача минимизации. <br></br>
        /// <see cref="minimize"/> = <see langword="false"/> - задача максимизации. <br></br> 
        /// </summary>
        public static bool minimize { get; set; }
        /// <summary>
        /// Количество переменных.
        /// </summary>
        public static int numberOfVariables { get; set; }
        /// <summary>
        /// Количество ограничений.
        /// </summary>
        public static int numberOfRestriction { get; set; }
        /// <summary>
        /// Искусственные переменные, представленные в виде списка индексов.
        /// </summary>
        public static List<int> artBasis { get; set; }
        /// <summary>
        /// Целевая функция, представленная в виде списка коэффицентов.
        /// </summary>
        public static List<Fraction> targetFun { get; set; }
        /// <summary>
        /// <see cref="NxNCase"/> = <see langword="true"/> - графический метод: случай - (n - m = 0), 
        /// где n - количество переменных, m - количество ограничений. <br></br>
        /// <see cref="NxNCase"/> = <see langword="false"/> - иной случай. <br></br>  
        /// </summary>
        public static bool NxNCase { get; set; }
        /// <summary>
        /// <see cref="graphicalMethod"/> = <see langword="true"/> - графический метод. <br></br>
        /// <see cref="graphicalMethod"/> = <see langword="false"/> - иной метод. <br></br>  
        /// </summary>
        public static bool graphicalMethod { get; set; }
        /// <summary>
        /// <see cref="Equations"/> = <see langword="true"/> - в графическом методе ограничения вида <br></br> 1) ax + by = c <br></br> 2) ax = c <br></br>
        /// <see cref="Equations"/> = <see langword="false"/> - иначе. <br></br> 
        /// </summary>
        public static bool Equations { get; set; }
        /// <summary>
        /// Преобразует число в нижний индекс.
        /// </summary>
        /// <param name="number">
        /// Число.
        /// </param>
        /// <returns>
        /// Возвращает строку с нижним индексом.
        /// </returns>
        public static string makeLowerIndex(int number)
        {
            string[] subscriptNumbers =
             {
               "\u2080", "\u2081", "\u2082", "\u2083", "\u2084",
               "\u2085", "\u2086", "\u2087", "\u2088", "\u2089"
            };
            string subscript = "";
            if (number < 10)
            {
                subscript = subscriptNumbers[number];
            }
            else
            {
                foreach (char digit in number.ToString())
                {
                    int digitValue = digit - '0';
                    subscript += subscriptNumbers[digitValue];
                }
            }
            return subscript;
        }
        /// <summary>
        /// Преобразует число в верхний индекс.
        /// </summary>
        /// <param name="number">
        /// Число.
        /// </param>
        /// <returns>
        /// Возвращает строку с верхним индексом.
        /// </returns>
        public static string makeUpperIndex(int number)
        {
            string[] superscriptNumbers = {
        "\u2070", "\u00B9", "\u00B2", "\u00B3", "\u2074",
        "\u2075", "\u2076", "\u2077", "\u2078", "\u2079"
        };
            string subscript = "";
            if (number < 10)
            {
                subscript = superscriptNumbers[number];
            }
            else
            {
                foreach (char digit in number.ToString())
                {
                    int digitValue = digit - '0';
                    subscript += superscriptNumbers[digitValue];
                }
            }
            return subscript;
        }
        /// <summary>
        /// Вычисляет ширину строки в <see cref="double"/>.
        /// </summary>
        /// <param name="rows">
        /// Двумерный массив строк.
        /// </param>
        /// <param name="fontSize">
        /// Размер шрифта.
        /// </param>
        /// <returns>
        /// Возвращает ширину строки.
        /// </returns>
        public static double GetMaxLabelWidth(List<List<string>> rows, double fontSize)
        {
            double maxWidth = 0;

            foreach (var row in rows)
            {
                foreach (var content in row)
                {
                    TextBlock tempTextBlock = new TextBlock();
                    tempTextBlock.Text = content;
                    tempTextBlock.FontSize = fontSize;
                    tempTextBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                    if (tempTextBlock.DesiredSize.Width > maxWidth)
                    {
                        maxWidth = tempTextBlock.DesiredSize.Width;
                    }
                }
            }

            return maxWidth + 20;
        }
        /// <summary>
        /// Вычисление целевой функции для симплекс-таблицы <see cref="SimplexTable"/>.
        /// </summary>
        /// <param name="basis">
        /// Базисные переменные, представленные списком индексов базисных переменных.
        /// </param>
        /// <param name="targetFun">
        /// Целевая функция.
        /// </param>
        /// <param name="newMatrix">
        /// Матрица коэффицентов ограничений.
        /// </param>
        /// <returns>
        /// Возвращает целевую функцию, представленную в виде списка коэффицентов.
        /// </returns>
        public static List<Fraction> createTargetFun(List<int> basis, List<Fraction> targetFun, Fraction[,] newMatrix)
        {
            for (int i = 0; i != basis.Count; i++)
            {
                if (targetFun[basis[i]] != 0)
                {
                    Fraction coeff = targetFun[basis[i]];
                    for (int j = 0; j != targetFun.Count; j++)
                    {
                        targetFun[j] -= newMatrix[i, j] * coeff;
                    }
                }
            }
            return targetFun;
        }
        /// <summary>
        /// Выполнение одной итерации симплекс метода.
        /// </summary>
        /// <param name="simplexTable">
        /// Начальная симплекс-таблица <see cref="SimplexTable"/>.
        /// </param>
        /// <param name="idleStep">
        /// Флаг, является ли итерация холостым шагом.
        /// </param>
        /// <returns>
        /// Возвращает новую симплекс-таблицу <see cref="SimplexTable"/>.
        /// </returns>
        public static SimplexTable DoIteration(SimplexTable simplexTable, bool idleStep = false) 
        {
            if (simplexTable.resultIteration != "Next") return null;
            Fraction[,] matrix = simplexTable.simplexStep(simplexTable.selectedRefElement[0], simplexTable.selectedRefElement[1]);
            var basis = simplexTable.basis.ToList();
            var free = simplexTable.free.ToList();
            var basisAndFree = swapBasisAndFreeX(simplexTable.selectedRefElement[0], simplexTable.selectedRefElement[1], basis, free);
            var result = simplexTable.FindPermRowAndColumns();
            if (idleStep) result = simplexTable.findPermissionElementsForIdleStep();
            var iteration = simplexTable.iteration + 1;
            if (result == "Next" && !stepByStepMethod) simplexTable.renderTablesLog();
            SimplexTable newSimplexTable = new SimplexTable(basisAndFree[0], basisAndFree[1], matrix, basisAndFree[0].Count + basisAndFree[1].Count, matrix.GetLength(0), iteration);
            return newSimplexTable;
        }
        /// <summary>
        /// Создание матрицы коэффицентов ограничений и целевой функции для начальной симплекс-таблицы <see cref="SimplexTable"/>. 
        /// </summary>
        /// <param name="basis">
        /// Базисные переменные, представленные списком индексов.
        /// </param>
        /// <param name="restrictions">
        /// Ограничения, представленные в виде двумерного массива коэффицентов.
        /// </param>
        /// <param name="numberOfRestrictions">
        /// Количество ограничений.
        /// </param>
        /// <param name="numberOfX">
        /// Количество переменных.
        /// </param>
        /// <param name="targetFun">
        /// Целевая функция задачи, представленная в виде списка коэффицентов.
        /// </param>
        /// <returns>
        /// Матрицу ограничений и целевой функции <see cref="SimplexTable.matrix"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Если с данным базисом невозможно сформировать ступенчатую матрицу коэффицентов.
        /// </exception>
        public static Fraction[,] createMatrixForSimplex(List<int> basis, Fraction[,] restrictions, int numberOfRestrictions, int numberOfX, List<Fraction> targetFun)
        {
             Fraction[,] newMatrix;
            try
            {
                newMatrix = Gauss.GMethod(basis, restrictions, numberOfRestrictions, numberOfX + 1);
            }
            catch (Exception)
            {
                throw new ArgumentException("Невозможно привести данную матрицу с данным базисом к ступенчатому виду!");
            }
            if (newMatrix == null) { throw new ArgumentException("Невозможно привести данную матрицу с данным базисом к ступенчатому виду!"); }
            targetFun = createTargetFun(basis, targetFun, newMatrix);
            Fraction[,] matrix_for_simplex = new Fraction[newMatrix.GetLength(0) + 1, newMatrix.GetLength(1)];
            for (int row = 0; row <= newMatrix.GetLength(0); row++)
            {
                if (row != newMatrix.GetLength(0))
                {
                    for (int col = 0; col < newMatrix.GetLength(1); col++)
                    {
                        matrix_for_simplex[row, col] = newMatrix[row, col];
                    }
                }
                else
                {
                    for (int col = 0; col < newMatrix.GetLength(1); col++)
                    {
                        matrix_for_simplex[row, col] = targetFun[col];
                    }
                }
            }
            matrix_for_simplex = deleteBasisColumns(matrix_for_simplex, basis);

            return matrix_for_simplex;
        }
        /// <summary>
        /// Удаление базисных столбцов из матрицы.
        /// </summary>
        /// <param name="matrix">
        /// Матрица.
        /// </param>
        /// <param name="basis">
        /// Базисные переменные, представленные списком индексов.
        /// </param>
        /// <returns>
        /// Новая матрица без базисных столбцов.
        /// </returns>
        public static Fraction[,] deleteBasisColumns(Fraction[,] matrix, List<int> basis)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int newColCount = cols - basis.Count;

            Fraction[,] newMatrix = new Fraction[rows, newColCount];
            int newColIndex = 0;

            for (int j = 0; j < cols; j++)
            {
                if (!basis.Contains(j))
                {
                    for (int i = 0; i < rows; i++)
                    {
                        newMatrix[i, newColIndex] = matrix[i, j];
                    }
                    newColIndex++;
                }
            }

            return newMatrix;
        }
        /// <summary>
        /// Меняет базисную и свободную переменную местами.
        /// </summary>
        /// <param name="perm_row">
        /// Индекс опорной строки.
        /// </param>
        /// <param name="perm_col">
        /// Индекс опорного столбца.
        /// </param>
        /// <param name="basis">
        /// Базисные переменные, представленные списком индексов.
        /// </param>
        /// <param name="free">
        /// Свободные переменные, представленные списком индексов.
        /// </param>
        /// <returns></returns>
        public static List<List<int>> swapBasisAndFreeX(int perm_row, int perm_col, List<int> basis, List<int> free) //не путать строки со столбцами!!!
        {
            int temp = free[perm_col];
            free[perm_col] = basis[perm_row];
            basis[perm_row] = temp;
            return new List<List<int>>() { basis, free };
        }
        /// <summary>
        /// Создание матрицы коэффицентов ограничений и целевой функции для начальной искусственной симплекс-таблицы <see cref="SimplexTable"/>.
        /// </summary>
        /// <param name="restrictions">
        /// Ограничения, представленные в виде матрицы коэффицентов.
        /// </param>
        /// <param name="numberOfRestrictions">
        /// Количество ограничений.
        /// </param>
        /// <param name="numberOfX">
        /// Количество переменных.
        /// </param>
        /// <returns>
        /// Матрицу ограничений и целевой функции <see cref="SimplexTable.matrix"/>.
        /// </returns>
        public static Fraction[,] createMatrixForArtificialBasis(Fraction[,] restrictions, int numberOfRestrictions, int numberOfX)
        {
            Fraction[,] matrix_for_artificial_basis = new Fraction[numberOfRestrictions + 1, numberOfX + 1];
            int rows = restrictions.GetLength(0);
            int cols = restrictions.GetLength(1);
            for (int row = 0; row < rows; row++)
            {
                if (restrictions[row, cols - 1] < 0)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        restrictions[row, col] = new Fraction(-1, 1) * restrictions[row, col];
                    }
                }
            }
            for (int col = 0; col <= numberOfX; col++)
            {
                Fraction sum = new Fraction(0, 1);
                for (int row = 0; row < numberOfRestrictions; row++)
                {
                    sum -= restrictions[row, col];
                    matrix_for_artificial_basis[row, col] = restrictions[row, col];
                }
                matrix_for_artificial_basis[numberOfRestrictions, col] = sum;
            }
            return matrix_for_artificial_basis;
        }
        /// <summary>
        /// Создание матрицы ограничений и целевой функции для симплекс-метода после метоа искусственного базиса.
        /// </summary>
        /// <param name="artBasis">Искуственные переменные, представленные списком индексов.</param>
        /// <param name="matrix">Конечная матрица <see cref="SimplexTable.matrix"/> ограничений и целевой функции метода искусственного базиса.</param>
        /// <param name="targetFun">Целевая функция.</param>
        /// <param name="free">Свободные переменные метода искусственного, представленные списком индексов.</param>
        /// <param name="basis">Базисные переменные, представленные списком индексов.</param>
        /// <param name="restrictions">Ограничения, представленные в виде матрицы коэффицентов.</param>
        /// <param name="newFree">Свободные переменные без искусственных переменных, представленные списком индексов.</param>
        /// <returns>
        /// Матрицу ограничений и целевой функции <see cref="SimplexTable.matrix"/>.
        /// </returns>
        public static Fraction[,] createMatrixForSimplexAfterArtficial(
            List<int> artBasis,
            Fraction[,] matrix,
            List<Fraction> targetFun,
            List<int> free,
            List<int> basis,
            Fraction[,] restrictions,
            List<int> newFree)
        {
            int cols = free.Count(x => !artBasis.Contains(x)) + 1;
            int rows = matrix.GetLength(0) - 1;
            Fraction[,] matrix_for_simplex = new Fraction[rows, cols];
            int column = 0;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < matrix.GetLength(1); col++)
                {
                    if (col != matrix.GetLength(1) - 1)
                    {
                        if (!artBasis.Contains(free[col]))
                        {
                            matrix_for_simplex[row, column] = matrix[row, col];
                            column++;
                        }
                    }
                    else
                    {
                        matrix_for_simplex[row, column] = matrix[row, col];
                    }
                }
                column = 0;
            }
            int counter = 0;
            List<Fraction> newTargetFun = new List<Fraction>();
            for (int col = 0; col < cols; col++)
            {
                Fraction sum = new Fraction(0, 1);
                for (int j = 0; j < rows; j++)
                {
                    Fraction coeff = targetFun[basis[j]];
                    sum -= coeff * matrix_for_simplex[j, col];
                }
                if (col != cols - 1) newTargetFun.Add(sum + targetFun[newFree[counter]]);
                else newTargetFun.Add(sum + targetFun[targetFun.Count - 1]);
                counter++;
            }

            Fraction[,] final_matrix = new Fraction[rows + 1, cols];
            for (int i = 0; i < rows + 1; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (i != rows) final_matrix[i, j] = matrix_for_simplex[i, j];
                    else final_matrix[i, j] = newTargetFun[j];
                }
            }
            return final_matrix;
        }
        /// <summary>
        /// Поиск базиса, при котором возможно привести матрицу к ступенчатому виду.
        /// </summary>
        /// <param name="restrictions">Матрица.</param>
        /// <param name="numberOfVariables">Количество переменных.</param>
        /// <param name="numberOfRestrictions">Количество строк.</param>
        /// <returns>
        /// Базисные переменные, представленные списком индексов.
        /// </returns>
        public static List<int> findBasis(Fraction[,] restrictions, int numberOfVariables, int numberOfRestrictions)
        {
            List<int> columns = Enumerable.Range(0, numberOfVariables).ToList();
            var combinations = GetCombinations(columns, numberOfRestrictions);
            foreach (var minor in combinations)
            {
                Fraction[,] newMatrix;
                try
                {
                    newMatrix = Gauss.GMethod(minor, restrictions, numberOfRestrictions, numberOfVariables + 1);
                }
                catch
                {
                    continue;
                }
                if( newMatrix == null) continue;
                return minor;
            }

            return null;
        }
        /// <summary>
        /// Рекурсивный метод для генерации всех возможных комбинаций указанного размера из заданного списка.
        /// </summary>
        /// <param name="array">Список элементов, из которых создаются комбинации.</param>
        /// <param name="combinationSize">Размер каждой комбинации.</param>
        /// <param name="index">Текущий индекс в исходном списке для обработки.</param>
        /// <param name="Result">Массив, используемый для хранения текущей комбинации.</param>
        /// <param name="depth">Текущая глубина рекурсии, отражающая количество уже добавленных элементов.</param>
        /// <returns>
        /// Перечисление, содержащее все возможные комбинации указанного размера из исходного списка.
        /// </returns>
        private static IEnumerable<List<int>> GetCombinationsRecursive(List<int> array, int combinationSize, int index, int[] Result, int depth)
        {
            if (depth == combinationSize)
            {
                yield return Result.ToList();
                yield break;
            }

            for (int i = index; i <= array.Count - combinationSize + depth; i++)
            {
                Result[depth] = array[i];
                foreach (var combination in GetCombinationsRecursive(array, combinationSize, i + 1, Result, depth + 1))
                {
                    yield return combination;
                }
            }
        }
        /// <summary>
        /// Метод для получения всех возможных комбинаций заданного размера из списка.
        /// </summary>
        /// <param name="array">Список элементов, из которых создаются комбинации.</param>
        /// <param name="combinationSize">Размер каждой комбинации.</param>
        /// <returns>
        /// Перечисление, содержащее все возможные комбинации указанного размера из исходного списка.
        /// </returns>
        private static IEnumerable<List<int>> GetCombinations(List<int> array, int combinationSize)
        {
            int[] Result = new int[combinationSize];
            return GetCombinationsRecursive(array, combinationSize, 0, Result, 0);
        }
        /// <summary>
        /// Создание матрицы ограничений, представленной в виде матрицы коэффицентов для графического метода.
        /// </summary>
        /// <param name="restrictions">Ограничения, представленные в виде матрицы ограничений.</param>
        /// <param name="basisX">Базисные переменные, представленные списком индексов.</param>
        /// <returns>
        /// Ограничения, представленные в виде матрицы коэффицентов.
        /// </returns>
        public static Fraction[,] createRestrictionsForGraphMethod(Fraction[,] restrictions, List<int> basisX)
        {
            int rows = restrictions.GetLength(0) - 1;
            int cols = restrictions.GetLength(1);
            List<int> basis = basisX.ToList();
            List<int> usedRows = new List<int>();
            Fraction[,] newRestrictions = new Fraction[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                int row = basis
                    .Select((value, index) => new { Value = value, Index = index })
                    .Where(x => !usedRows.Contains(x.Index))
                    .OrderBy(x => x.Value)
                    .Select(x => x.Index)
                    .FirstOrDefault();
                for (int j = 0; j < cols; j++)
                {
                    newRestrictions[i, j] = restrictions[row, j];
                }
                usedRows.Add(row);
            }
            return newRestrictions;
        }
        /// <summary>
        /// Создание целевой функции для графического метода после формирования матрицы целевой функции и ограничений для симплекс-таблицы.
        /// </summary>
        /// <param name="restrctions">
        /// Матрицы целевой функции и ограничений для симплекс-таблицы <see cref="SimplexTable.matrix"/>.
        /// </param>
        /// <returns>
        /// Целевую функцию, представленную в виде списка коэффицентов.
        /// </returns>
        public static List<Fraction> createTargetFunForGraphMethod(Fraction[,] restrctions)
        {
            List<Fraction> targetFunction = new List<Fraction>();
            int cols = restrctions.GetLength(1);
            int rows = restrctions.GetLength(0);
            for (int col = 0; col < cols; col++)
            {
                if (!minimize)
                {
                    targetFunction.Add(restrctions[rows - 1, col] * new Fraction(-1, 1));
                }
                else targetFunction.Add(restrctions[rows - 1, col]);
            }
            return targetFunction;
        }
        /// <summary>
        /// Нахождение свободных переменных.
        /// </summary>
        /// <param name="basis">Базисные переменные, представленные списком индексов.</param>
        /// <param name="numberOfX">Количество переменных.</param>
        /// <returns>
        /// Свободные переменные, представленные списком индексов.
        /// </returns>
        public static List<int> findFreeX(List<int> basis, int numberOfX)
        {
            List<int> free = new List<int>();
            for (int x = 0; x < numberOfX; x++)
            {
                if (!basis.Contains(x)) free.Add(x);
            }
            return free;
        }
        /// <summary>
        /// Удаление базисных переменных из функции цели.
        /// </summary>
        /// <param name="basis">Базисные переменные, представленные списком индексов.</param>
        /// <param name="targetFun">Функция цели.</param>
        /// <returns>
        /// Целевую функцию, представленную в виде списка коэффицентов.
        /// </returns>
        public static List<Fraction> deleteBasisXFromTargetFun(List<int> basis, List<Fraction> targetFun)
        {
            for (int i = targetFun.Count; i >= 0; i--)
            {
                if (basis.Contains(i)) targetFun.RemoveAt(i);
            }
            if (!minimize)
            {
                targetFun = targetFun.Select(x => x * new Fraction(-1, 1)).ToList();
            }

            return targetFun;
        }
        /// <summary>
        /// Копирование двумерного массива.
        /// </summary>
        /// <param name="source">Исходный двумерный массив.</param>
        /// <returns>
        /// Копия.
        /// </returns>
        public static Fraction[,] Copy(Fraction[,] source)
        {
            int rows = source.GetLength(0); // Количество строк
            int cols = source.GetLength(1); // Количество столбцов

            Fraction[,] restrinctionsForGraph = new Fraction[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    restrinctionsForGraph[i, j] = source[i, j];
                }
            }
            return restrinctionsForGraph;
        }
        /// <summary>
        /// Нахождение нулевых столбцов в задаче оптимизации.
        /// </summary>
        /// <param name="source">Ограничения, представленные матрицей коэффицентов.</param>
        /// <param name="targetFun">Функция цели, представленная списком коэффицентов.</param>
        /// <returns>
        /// Список индексов нулевых столбцов.
        /// </returns>
        public static List<int> FindZerosColumns(Fraction[,] source, List<Fraction> targetFun) 
        {
            List<int> columns = new List<int>();
            int rows = source.GetLength(0);
            int cols = source.GetLength(1);
            for (int col = 0; col < cols - 1; col++)
            {
                Fraction sum = new Fraction(0,1);
                for (int row = 0 ; row < rows ; row++)
                {
                    sum += source[row, col];
                }
                if (sum == 0 && targetFun[col] == 0) columns.Add(col);
            }
            if (columns.Count == 0) return null;
            return columns;
           

        }
        /// <summary>
        /// Создание матрицы ограничений для графического метода, случая n - m = 0. (где n - количество переменных, m - количество ограничений).
        /// </summary>
        /// <param name="source">Исходная матрица ограничений.</param>
        /// <param name="n">Количество переменных.</param>
        /// <returns>
        /// Матрицу ограничений для графического метода.
        /// </returns>
        public static Fraction[,] CreateMatrixOfRestrictionsForNxN(Fraction[,] source, int n)
        {
            Fraction[,] newMatrix;
            if (n == 1)
            {
                newMatrix = new Fraction[1, 2];
                newMatrix[0, 0] = source[0, 0];
                newMatrix[0, 1] = source[0, 1];
                return newMatrix;
            }
            newMatrix = new Fraction[2, 3];
            int cols = source.GetLength(1);
            newMatrix[0, 0] = source[0, 0];
            newMatrix[0, 1] = new Fraction(0, 1);
            newMatrix[0, 2] = source[0, cols - 1];
            newMatrix[1, 0] = new Fraction(0, 1);
            newMatrix[1, 1] = source[1, 1];
            newMatrix[1, 2] = source[1, cols - 1];
            return newMatrix;
        }
        /// <summary>
        /// Создание целевой функции для графического метода, случая n - m = 0. (где n - количество переменных, m - количество ограничений).
        /// </summary>
        /// <param name="source">Исходная матрица ограничений.</param>
        /// <param name="targetFun">Исходная целевая функция.</param>
        /// <param name="n">Количество переменных.</param>
        /// <returns>
        /// Целевую функцию для графического метода.
        /// </returns>
        public static List<Fraction> CreateTargetFunForNxN(Fraction[,] source, List<Fraction> targetFun, int n)
        {
            List<Fraction> newTargetFun = new List<Fraction>();
            newTargetFun.Add(targetFun[0]);
            newTargetFun.Add(targetFun[1]);
            if (n == 1) return newTargetFun;
            int cols = source.GetLength(1);
            Fraction sum = new Fraction(0, 1);
            for (int i = 2; i < targetFun.Count - 1; i++)
            {
                sum += targetFun[i] * source[i, cols - 1];
            }
            sum += targetFun[targetFun.Count - 1];
            newTargetFun.Add(sum);
            return newTargetFun;
        }
        /// <summary>
        /// Меняет местами первые два столбца в матрице коэффициентов.
        /// </summary>
        /// <param name="source">
        /// Исходная матрица коэффициентов, в которой требуется поменять местами столбцы.
        /// </param>
        /// <returns>
        /// Новая матрица, в которой первые два столбца поменяны местами, а остальные остаются без изменений.
        /// </returns>
        public static Fraction[,] SwapColumns(Fraction[,] source)
        {
            int rows = source.GetLength(0);
            int cols = source.GetLength(1);
            Fraction[,] newRestrictions = new Fraction[rows, cols];
            
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (col == 0)
                    {
                        newRestrictions[row, col + 1] = source[row, col];
                        continue;
                    }
                    if (col == 1)
                    {
                        newRestrictions[row, col - 1] = source[row, col];
                        continue;
                    }
                    newRestrictions[row, col] = source[row, col];
                }
            }
            return newRestrictions;
        }
        /// <summary>
        /// Удаление одинаковых точек из списка точек <see cref="FractionPoint"/>.
        /// </summary>
        /// <param name="points">Список точек.</param>
        /// <returns>
        /// Список точек без дубликатов.
        /// </returns>
        public static List<FractionPoint> RemoveDuplicates(List<FractionPoint> points)
        {
            return points.Distinct().ToList();
        }
        /// <summary>
        /// Создание точки ответа для графического метода, случая n - m = 0. (где n - количество переменных, m - количество ограничений).
        /// </summary>
        /// <param name="matrix">Матрица ограничений.</param>
        /// <returns>
        /// Точка ответа представлена в виде списка.
        /// </returns>
        public static List<Fraction> CreatePointFullPointForNxNCase(Fraction[,] matrix)
        {
            List<Fraction> point = new List<Fraction>();
            int cols = matrix.GetLength(1);
            int rows = matrix.GetLength(0);
            for (int i = 0; i < rows; i++)
            {
                point.Add(matrix[i, cols - 1]);
            }
            return point;
        }
        /// <summary>
        /// Очищает поля данного класса <see cref="Utils"/>.
        /// </summary>
        public static void ClearAllFields()
        {
            numberOfVariables = -1;
            numberOfRestriction = -1;
            if (tables != null) { tables.Clear(); }
        }

    }

}

