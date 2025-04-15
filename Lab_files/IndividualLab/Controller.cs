using ClassLibraryTools;
using IndividualLab.View.WIndows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace IndividualLab
{
    /// <summary>
    /// Класс для распределения задач.
    /// </summary>
    public static class Controller
    {
        /// <summary>
        /// Экземпляр окна для вывода симплекс-метода.
        /// </summary>
        public static SimplexMethodWindow win { get; set; }
        /// <summary>
        /// Экземпляр главного окна.
        /// </summary>
        public static MainWindow mainWindow { get; set; }
        /// <summary>
        /// Данные о задаче, полученные в главном окне.
        /// </summary>
        public static InputData data { get; set; }
        /// <summary>
        /// Функция, которая запускает выбранный метод решения.
        /// </summary>
        /// <param name="data1">Входные данные о задаче.</param>
        /// <param name="mainWindow1">Экземпляр главного окна.</param>
        /// <returns>
        /// Строка с описанием ошибки, если произошла ошибка. 
        /// Если ошибок нет, возвращает <see langword="null"/>.
        /// </returns>
        public static string DoTask(InputData data1, MainWindow mainWindow1)
        {
            mainWindow = mainWindow1;
            data = data1;
            if (data.solutionMethod == SolutionMethod.Simplex)
            {

                SimplexMethodWindow simplexMethodWindow = new SimplexMethodWindow(mainWindow);
                win = simplexMethodWindow;
                win.Show();
                string error = null;
                data.targetFunction[data.targetFunction.Count - 1] = data.targetFunction.Last() * new Fraction(-1, 1);
                if (!Utils.minimize) data.targetFunction = data.targetFunction.Select(x => x * new Fraction(-1, 1)).ToList();
                if (data.basis != null) { error = choosenBasis(); return error; }
                else { error = artificialBasis(); return error; }
            }
            if (data.solutionMethod == SolutionMethod.Graphical)
            {
                if (data.numberOfVariables == 2 || 
                    data.numberOfVariables - data.numberOfRestrictions == 2 ||
                    data.numberOfVariables - data.numberOfRestrictions == 1 ||
                    data.numberOfVariables - data.numberOfRestrictions == 0)
                {
                    try
                    {
                        string error = GraphicalMethod();
                        return error;
                    }
                    catch(Exception)
                    {
                        return "Неизвестная ошибка.";
                    }

                }
                return "Невозможно выполнить графический метод, доступны только двумерное и одномерные пространства.";
            }
            return null;
        }

        #region Graphical
        /// <summary>
        /// Функция, которая запускает графический метод.
        /// <para>
        /// Разбираются случаи: (n - кол-во переменных; m - кол-во ограничений). <br></br>
        /// 1) n - m = 2 ; <br></br>
        /// 2) n - m = 1 ; <br></br>
        /// 3) n = 2, 1 &lt;= m &lt;= 16 ; <br></br>
        /// 4) n - m = 0 ; <br></br>
        /// </para>
        /// <para>
        /// Если n - m = 2; n - m = 1, и базис автоматический, то строится 
        /// искусственная симплекс-таблица и методом искусственного базиса находится 
        /// начальный базис. Если базис выбранный или он нашелся после выполнения метода 
        /// искусственного базиса, то выполняется симплекс-метод и проверяется, существует ли 
        /// решение для данной задачи.
        /// </para>
        /// </summary>
        /// <returns>
        /// Строка с описанием ошибки, если произошла ошибка.
        /// Если ошибок нет, возвращает <see langword="null"/>.
        /// </returns>
        private static string GraphicalMethod()
        {
            Utils.Equations = false;
            Utils.NxNCase = false;
            data.targetFunction[data.targetFunction.Count - 1] = data.targetFunction.Last() * new Fraction(-1, 1);
            List<int> columns = Utils.FindZerosColumns(data.restrictions, data.targetFunction);
            if (columns != null) return "Требование удалить нулевые столбцы!";

            if (data.numberOfVariables == 2)
            {
                Utils.Equations = true;
                data.basis = new List<int>();
                for (int i = 0; i < data.numberOfVariables; i++)
                {
                    data.basis.Add(i);
                }
                return StartGraphic(data.targetFunction, data.restrictions, "OK");
            }

            if (data.numberOfRestrictions - data.numberOfVariables == 0)
            {
                Utils.Equations = true;
                List<int> basis;
                if (data.basis == null)
                { 
                    basis = Utils.findBasis(data.restrictions, data.numberOfVariables, data.numberOfRestrictions);
                    if (basis == null) return "Невозможно привести данную матрицу к ступенчатому виду";
                    data.basis = basis;
                }
                Fraction[,] matrix;
                try
                {
                    matrix = Gauss.GMethod(data.basis, data.restrictions, data.numberOfRestrictions, data.numberOfVariables + 1);
                }
                catch
                {
                    return "Невозможно привести данную матрицу к ступенчатому виду";
                }
                if (matrix == null) return "Невозможно привести данную матрицу к ступенчатому виду";
                var targetFunctionForNxN = Utils.CreateTargetFunForNxN(matrix, data.targetFunction, data.numberOfVariables);
                var restrictions = Utils.CreateMatrixOfRestrictionsForNxN(matrix, data.numberOfVariables);
                if (data.numberOfVariables == 1)
                    data.basis = new List<int>() { 0 };
                else
                    data.basis = new List<int>() { 0, 1 };
                if (data.numberOfVariables > 2)
                {
                    List<Fraction> point = Utils.CreatePointFullPointForNxNCase(matrix);
                    Utils.NxNCase = true;
                    return StartGraphic(targetFunctionForNxN, restrictions, "OK", point);
                }
                return StartGraphic(targetFunctionForNxN, restrictions, "OK");
            }

            if (data.basis == null)
            {
                if (!Utils.minimize)
                {
                    data.targetFunction = data.targetFunction.Select(x => x * new Fraction(-1, 1)).ToList();
                }
                return AutoBasisGraphicalMethod();
            }
            else
            {
                if (!Utils.minimize)
                {
                    data.targetFunction = data.targetFunction.Select(x => x * new Fraction(-1, 1)).ToList();
                }
                return ChoosenBasisGraphicalMethod();
            }
        }
        /// <summary>
        /// Создаётся экземпляр класса <see cref="Graphic"/>.
        /// Создаётся экземпляр окна для вывода графического метода <see cref="GraphicalMethodWindow"/>.
        /// Выводятся все данные для графического метода в окно для вывода данных графического метода.
        /// </summary>
        /// <param name="targetFunctionForGraph">Преобразованная для графического метода целевая функция.</param>
        /// <param name="restrictionsForGraph">Преобразованная матрица ограничений для графического метода.</param>
        /// <param name="solution">Пустая строка, если случай одномерный. Если двумерный, то передаётся существует ли ответ, либо задач неразрешима.</param>
        /// <param name="point">В случае n - m = 0. 
        /// Передаётся точка из всех ограничений. Так как в этом случае у нас ступенчатая матрица. 
        /// Иначе <see langword="null"/>
        /// </param>
        /// <returns>
        /// Строка с описанием ошибки, если произошла ошибка.
        /// Если ошибок нет, возвращает <see langword="null"/>.
        /// </returns>
        private static string StartGraphic(List<Fraction> targetFunctionForGraph, Fraction[,] restrictionsForGraph, string solution, List<Fraction> point = null)
        {
            Graphic graph;
            data.basis.Sort();
            try
            {
                if (Utils.Equations)
                {
                    graph = new Graphic(restrictionsForGraph, targetFunctionForGraph, data.basis, data.basis, point, solution);

                }
                else
                {
                    graph = new Graphic(restrictionsForGraph, targetFunctionForGraph, Utils.findFreeX(data.basis, data.numberOfVariables), data.basis, point, solution);
                }
            }
            catch (Exception ex)
            {
                Utils.ClearAllFields();
                return ex.Message;
            }
            GraphicalMethodWindow graphicalMethodWindow = new GraphicalMethodWindow(mainWindow, graph);
            graphicalMethodWindow.graphGrid.Children.Add(graph.renderedGraph);
            Canvas canvas = graph.CreateLogAboutTask();
            graphicalMethodWindow.taskStackPanel.Children.Add(canvas);
            graphicalMethodWindow.answerTextBLock.Text = graph.solution;
            graphicalMethodWindow.Show();
            mainWindow.Hide();
            Utils.ClearAllFields();
            return null;
        }
        /// <summary>
        /// Для выбранного базиса с помощью симплекс-метода выполняется проверка существования ответа.
        /// </summary>
        /// <returns>
        /// Строка с описанием ошибки, если произошла ошибка.
        /// Если ошибок нет, возвращает <see langword="null"/>.
        /// </returns>
        private static string ChoosenBasisGraphicalMethod()
        {
            string solution = "";
            List<int> actualBasis = data.basis.ToList();
            Utils.artificialBasisMethod = true;
            var simplexTables = StartArtificialBasis();
            var simplexTable = simplexTables.Value.Item2;
            if (simplexTable == null) return "Невозможно запустить графический метод.";
            Utils.artificialBasisMethod = false;
            if (simplexTable.resultIteration.Contains("("))
            {
                var matrix = CreateMatrixOfRestrictionsForGraphMethod(simplexTable);
                var simplexTable2 = StartSimplexMethod();
                if (simplexTable2 == null) return "Невозможно запустить графический метод.";
                if (simplexTable2.resultIteration.Contains("З")) solution = "Задача неразрешима";
                else solution = "OK";
            }
            else
            {
                solution = "Задача неразрешима";
            }
            data.basis = actualBasis;
            var simplexTable1 = StartSimplexMethod(true);
            if (simplexTable1 == null) return "Невозможно привести к ступенчатому виду данную матрицу ограничений с данным базисом.";
            var restrictionsForGraph = Utils.createRestrictionsForGraphMethod(simplexTable1.matrix, simplexTable1.basis);
            var targetFunctionForGraph = Utils.createTargetFunForGraphMethod(simplexTable1.matrix);
        

            return StartGraphic(targetFunctionForGraph, restrictionsForGraph, solution);
        }
        /// <summary>
        /// Для автоматического базиса для графического метода запускается метод искусственного базиса.
        /// Если решение смогло найтись, то запускается симплекс метод, и проверяется есть ли решение.
        /// Если базис не нашёлся, то помечается, что решения не существует.
        /// </summary>
        /// <returns>
        /// Строка с описанием ошибки, если произошла ошибка.
        /// Если ошибок нет, возвращает <see langword="null"/>.
        /// </returns>
        private static string AutoBasisGraphicalMethod()
        {
            List<Fraction> targetFunctionForGraph;
            Fraction[,] restrictionsForGraph;
            string solution = "";
            Utils.artificialBasisMethod = true;
            var simplexTables = StartArtificialBasis();
            if (simplexTables == null) return "Невозможно запустить графический метод.";
            var firstTable = simplexTables.Value.Item1;
            var simplexTable = simplexTables.Value.Item2;
            Utils.artificialBasisMethod = false;
            if (!simplexTable.resultIteration.Contains("("))
            {
                solution = "Задача неразрешима";
                var basis = Utils.findBasis(data.restrictions, data.numberOfVariables, data.numberOfRestrictions);
                if (basis == null)
                {
                    return "Невозможно привести данную матрицу к ступенчатому виду";
                }
                data.basis = basis;
                var matrix = Utils.Copy(data.restrictions);
                targetFunctionForGraph = Utils.deleteBasisXFromTargetFun(basis, Utils.createTargetFun(basis, data.targetFunction, matrix));
                restrictionsForGraph = Utils.deleteBasisColumns(matrix, basis);
                return StartGraphic(targetFunctionForGraph, restrictionsForGraph, solution);
            }
            else
            {
                var matrix = CreateMatrixOfRestrictionsForGraphMethod(simplexTable);
                var restrictions = Utils.createRestrictionsForGraphMethod(matrix, simplexTable.basis);
                restrictionsForGraph = restrictions;
                targetFunctionForGraph = Utils.createTargetFunForGraphMethod(matrix);
                var simplexTable1 = StartSimplexMethod();
                if (simplexTable1 == null) return "Невозможно запустить графический метод.";
                if (simplexTable1.resultIteration.Contains("З")) solution = "Задача неразрешима";
                else solution = "OK";
                return StartGraphic(targetFunctionForGraph, restrictionsForGraph, solution);
            }
        }
        /// <summary>
        /// Функция для создания матрицы ограничений для графического метода после метода искусственного базиса.
        /// </summary>
        /// <param name="simplexTable">
        /// Первая построенная итерация искусственной симплекс-таблицы.
        /// </param>
        /// <returns>
        /// Строка с описанием ошибки, если произошла ошибка.
        /// Если ошибок нет, возвращает <see langword="null"/>.
        /// </returns>
        private static Fraction[,] CreateMatrixOfRestrictionsForGraphMethod(SimplexTable simplexTable)
        {
            List<int> free = simplexTable.free.Where(x => !Utils.artBasis.Contains(x)).ToList();
            List<int> basis = simplexTable.basis.ToList();
            data.basis = simplexTable.basis;
            var matrix = Utils.createMatrixForSimplexAfterArtficial(
                        Utils.artBasis,
                        simplexTable.matrix,
                        Utils.targetFun,
                        simplexTable.free,
                        simplexTable.basis,
                        data.restrictions,
                        free);
            if (free.Count > 1)
            {
                if (free[0] > free[1])
                {
                    matrix = Utils.SwapColumns(matrix);
                }
            }
            return matrix;
        }
        /// <summary>
        /// Запуск метода искусственного базиса.
        /// </summary>
        /// <returns>
        /// Если произошла ошибка возвращает <see cref="null"/>.
        /// Если ошибок нет, то возвращает кортеж из первой искусственной <see cref="SimplexTable"/> и
        /// из конечной искуственной <see cref="SimplexTable"/>.
        /// </returns>
        private static (SimplexTable, SimplexTable)? StartArtificialBasis()
        {
            List<int> basis = Enumerable.Range(data.numberOfVariables + 1, data.numberOfRestrictions).Select(x => x - 1).ToList(); //4 , 5
            Utils.artBasis = basis;
            Utils.targetFun = data.targetFunction.ToList();
            Utils.numberOfRestriction = data.numberOfRestrictions;
            Utils.numberOfVariables = data.numberOfVariables;
            SimplexTable simplexTable;
            try
            {
                simplexTable =
                    new SimplexTable(
                        basis,
                        null,
                        Utils.createMatrixForArtificialBasis(
                            Utils.Copy(data.restrictions),
                            data.numberOfRestrictions,
                            data.numberOfVariables
                            ),
                        data.numberOfVariables + basis.Count,
                        data.numberOfRestrictions
                        );
            }
            catch (Exception)
            {
                Utils.ClearAllFields();
                return null;
            }
            SimplexTable firstTable = simplexTable;
            SimplexTable tempTable = firstTable;
            var table = Utils.DoIteration(simplexTable);
            while (table != null)
            {
                tempTable = table;
                table = Utils.DoIteration(tempTable);
            }
            return (firstTable, tempTable);
        }
        /// <summary>
        /// Если <see cref="ChoosenBasis"/> = <see cref="true"/>, то возвращает сформированную первую симплекс-таблицу.
        /// Если <see cref="ChoosenBasis"/> = <see cref="false"/>, то возвращает конечную симплекс-таблицу, чтобы проверить существование решения.
        /// </summary>
        /// <param name="ChoosenBasis">
        /// Флаг, нужный для того, чтобы определить возвращать первую сформированную симплекс таблицу.
        /// Или пройти все итерации, чтобы проверить существование решения.
        /// </param>
        /// <returns>
        /// Если есть ошибка, возвращает <see langword="null"/>.
        /// Иначе возвращает <see cref="SimplexTable"/>.
        /// </returns>
        private static SimplexTable StartSimplexMethod(bool ChoosenBasis = false)
        {
            SimplexTable simplexTable;
            try
            {
                simplexTable =
                    new SimplexTable(
                        data.basis,
                        null,
                        Utils.createMatrixForSimplex(data.basis,
                                                     Utils.Copy(data.restrictions),
                                                     data.numberOfRestrictions,
                                                     data.numberOfVariables,
                                                     data.targetFunction),
                        data.numberOfVariables,
                        data.numberOfRestrictions
                        );
            }
            catch (Exception)
            {
                Utils.tables.Clear();
                mainWindow.Show();
                return null;
            }

            SimplexTable tempTable = simplexTable;
            if (ChoosenBasis) return tempTable;
            var table = Utils.DoIteration(simplexTable);
            while (table != null)
            {
                tempTable = table;
                table = Utils.DoIteration(table);
            }
            return tempTable;
        }
        #endregion

        #region Simplex
        /// <summary>
        /// Функция для создания симплекс-таблицы для метода искусственного базиса.
        /// </summary>
        /// <returns>
        /// Если ошибок при создании симплекс-таблицы ошибок нет возвращает <c>null</c>.
        /// Если ошибка есть возвращает строку с описанием ошибки.
        /// </returns>
        private static string artificialBasis()
        {
            List<int> basis = Enumerable.Range(data.numberOfVariables + 1, data.numberOfRestrictions).Select(x => x - 1).ToList(); //4 , 5
            Utils.artBasis = basis;
            Utils.targetFun = data.targetFunction;
            Utils.numberOfRestriction = data.numberOfRestrictions;
            Utils.numberOfVariables = data.numberOfVariables;
            SimplexTable simplexTable;
            try
            {
                simplexTable =
                    new SimplexTable(
                        basis,
                        null,
                        Utils.createMatrixForArtificialBasis(
                            data.restrictions,
                            data.numberOfRestrictions,
                            data.numberOfVariables
                            ),
                        data.numberOfVariables + basis.Count,
                        data.numberOfRestrictions
                        );
                if (!Utils.stepByStepMethod) autoSimplexMethod(simplexTable);
                else stepByStepArtificialBasisMethod(simplexTable);
            }
            catch (Exception ex)
            {
                win.Close();
                Utils.tables.Clear();
                mainWindow.Show();
                return ex.Message;
            }
            mainWindow.Hide();
            win.Show();
            return null;
        }
        /// <summary>
        /// Функция для создания симплекс-таблицы с выбранным базисом.
        /// </summary>
        /// <returns>
        /// Если ошибок при создании симплекс-таблицы ошибок нет возвращает <c>null</c>.
        /// Если ошибка есть возвращает строку с описанием ошибки.
        /// </returns>
        private static string choosenBasis()
        {
            SimplexTable simplexTable;
            try
            {
                simplexTable =
                    new SimplexTable(
                        data.basis,
                        null,
                        Utils.createMatrixForSimplex(data.basis,
                                                     data.restrictions,
                                                     data.numberOfRestrictions,
                                                     data.numberOfVariables,
                                                     data.targetFunction),
                        data.numberOfVariables,
                        data.numberOfRestrictions
                        );
                if (!Utils.stepByStepMethod) autoSimplexMethod(simplexTable);
                else stepByStepSimplexMethod(simplexTable);
            }
            catch (Exception ex)
            {
                win.Close();
                Utils.tables.Clear();
                mainWindow.Show();
                return ex.Message;
            }
            mainWindow.Hide();
            win.Show();
            return null;
        }
        /// <summary>
        /// Функция для запуска автоматического симплекс-метода и вывода ответа. 
        /// Если выбран метод искуственного базиса, то функция запускается сначала для икусственной таблицы,
        /// затем для результирующей таблицы, и в конце выводится ответ.
        /// </summary>
        /// <param name="simplexTable">Начальная симплекс таблица.</param>
        private static void autoSimplexMethod(SimplexTable simplexTable)
        {
            if (simplexTable.resultIteration != "Next")
            {
                win.outputStackPanel.Children.Add(Utils.tables[0].renderedTable);
                outputAnswerSimplexMethod(simplexTable);
                return;
            }
            SimplexTable table = Utils.DoIteration(simplexTable);
            while (table != null)
            {
                table = Utils.DoIteration(table);
            }
            SimplexTable lastIterTable = null;
            foreach (var simTable in Utils.tables)
            {
                win.outputStackPanel.Children.Add(simTable.renderedTable);
                if (simTable.renderedTablesLog != null) win.outputStackPanel.Children.Add(simTable.renderedTablesLog);
                lastIterTable = simTable;
            }

            if (lastIterTable.resultIteration.Contains("("))
            {
                if (Utils.artificialBasisMethod)
                {
                    win.outputStackPanel.Children.Add(renderLogAfterArtificialBasis(lastIterTable));
                    SimplexTable simplTable = createSimplexMethodAfterArtificialBasisMethod(lastIterTable);
                    autoSimplexMethod(simplTable);
                    return;
                }
                outputAnswerSimplexMethod(lastIterTable);
            }
            else
            {
                win.answerTextBlock.Text = lastIterTable.resultIteration;
            }

        }
        /// <summary>
        /// Функция, которая выводит начальную симплекс таблицу, если существуют следующие итерации, 
        /// то существует возможность нажать на опорный элемент в выведенной симплекс-таблице.
        /// </summary>
        /// <param name="simplexTable">Начальная симплекс-таблица.</param>
        private static void stepByStepSimplexMethod(SimplexTable simplexTable)
        {
            win.outputStackPanel.Children.Add(simplexTable.renderedTable);
            if (simplexTable.resultIteration != "Next")
            {
                outputAnswerSimplexMethod(simplexTable);
                return;
            }
            simplexTable.OnLabelClicked += HandleLabelClickedSimplexMethod;

        }
        /// <summary>
        /// Функция, которая выводит начальную искусственную таблицу, если существуют следующие итерации,
        /// то существует возможность нажать на опорный элемент в искусственной симплекс-таблице.
        /// </summary>
        /// <param name="simplexTable">Начальная искусственная симплекс-таблица.</param>
        private static void stepByStepArtificialBasisMethod(SimplexTable simplexTable)
        {
            win.outputStackPanel.Children.Add(simplexTable.renderedTable);
            if (simplexTable.resultIteration != "Next")
            {
                outputAnswerSimplexMethod(simplexTable);
                return;
            }
            simplexTable.OnLabelClicked += HandleLabelClickedArtificialBasisMethod;
        }
        /// <summary>
        /// Функция, которая удаляет все таблицы в выводе до таблицы, из которой выбран опорный элемент.
        /// И выводит следующую таблицу. Если исскуственный базис не может быть найден - выводится ответ,
        /// Если искусственный базис найден, то запускается пошаговый симплекс-метод.
        /// </summary>
        /// <param name="oldTable">
        /// Старая искусственная симплекс-таблица, в которой выбран опорный элемент.
        /// </param>
        /// <param name="newTable">
        /// Новая искусственная симплекс-таблица, 
        /// которая построена на основе выбранного опорного элемента в старой исскуственной симплекс-таблице.
        /// </param>
        private static void HandleLabelClickedArtificialBasisMethod(SimplexTable oldTable, SimplexTable newTable)
        {
            for (int i = win.outputStackPanel.Children.Count - 1; i >= 0; i--)
            {
                Canvas canvas = (Canvas)win.outputStackPanel.Children[i];
                if (canvas.Tag != null)
                {
                    string tag = (string)canvas.Tag;
                    if (tag == "A" + oldTable.iteration) { break; }
                }
                win.outputStackPanel.Children.RemoveAt(i);
            }
            win.outputStackPanel.Children.Add(oldTable.renderedTablesLog);
            win.outputStackPanel.Children.Add(newTable.renderedTable);
            win.answerTextBlock.Text = "";
            if (newTable.resultIteration != "Next")
            {
                if (newTable.resultIteration.Contains("("))
                {
                    win.outputStackPanel.Children.Add(renderLogAfterArtificialBasis(newTable));
                    SimplexTable simTable = createSimplexMethodAfterArtificialBasisMethod(newTable);
                    stepByStepSimplexMethod(simTable);
                    return;
                }
                else { win.answerTextBlock.Text = newTable.resultIteration; return; }
            }
            else { win.answerTextBlock.Text = ""; }
            newTable.OnLabelClicked += HandleLabelClickedArtificialBasisMethod;
        }
        /// <summary>
        /// Функция, которая удаляет все таблицы в выводе до таблицы, из которой выбран опорный элемент.
        /// И выводит следующую таблицу. Если ответ найден или не может быть найден, то выводится ответ.
        /// </summary>
        /// <param name="oldTable">
        /// Старая симплекс-таблица, в которой выбран опорный элемент.
        /// </param>
        /// <param name="newTable">
        /// Новая симплекс-таблица, которая построена на основе выбранного опорного элемента в старой симплекс-таблице.
        /// </param>
        private static void HandleLabelClickedSimplexMethod(SimplexTable oldTable, SimplexTable newTable)
        {
            Utils.artificialBasisMethod = false;
            for (int i = win.outputStackPanel.Children.Count - 1; i >= 0; i--)
            {
                Canvas canvas = (Canvas)win.outputStackPanel.Children[i];
                if (canvas.Tag != null)
                {
                    string tag = (string)canvas.Tag;
                    if (tag == oldTable.iteration.ToString()) { break; }
                }
                win.outputStackPanel.Children.RemoveAt(i);

            }
            win.outputStackPanel.Children.Add(oldTable.renderedTablesLog);
            win.outputStackPanel.Children.Add(newTable.renderedTable);
            if (newTable.resultIteration != "Next")
            {
                outputAnswerSimplexMethod(newTable);
                return;
            }
            else { win.answerTextBlock.Text = ""; }
            newTable.OnLabelClicked += HandleLabelClickedSimplexMethod;
        }
        /// <summary>
        /// Функция, которая строит новую симплекс-таблицу после успешного завершения работы метода искусственного базиса.
        /// </summary>
        /// <param name="lastIterTable">
        /// Последняя итерация искусственной таблицы.
        /// </param>
        /// <returns></returns>
        private static SimplexTable createSimplexMethodAfterArtificialBasisMethod(SimplexTable lastIterTable)
        {
            Utils.artificialBasisMethod = false;
            List<int> free = lastIterTable.free.Where(x => !Utils.artBasis.Contains(x)).ToList();
            Utils.tables.Clear();
            List<int> basis = lastIterTable.basis.ToList();
            SimplexTable simplTable;
            try
            {
                simplTable =
                    new SimplexTable(
                        basis,
                        free,
                        Utils.createMatrixForSimplexAfterArtficial(
                            Utils.artBasis,
                            lastIterTable.matrix,
                            Utils.targetFun,
                            lastIterTable.free,
                            lastIterTable.basis,
                            data.restrictions,
                            free
                            ),
                        lastIterTable.numberOfX - lastIterTable.numberOfRestrictions + 1,
                        lastIterTable.numberOfRestrictions - 1,
                        0,
                        false
                    );
            }
            catch (Exception ex)
            {
                win.Close(); 
                Utils.ClearAllFields();
                mainWindow.Show();
                MessageBox.Show(ex.Message);
                return null;
            }
            return simplTable;
        }
        /// <summary>
        /// Функция, которая выводит базис, который нашёлся после завершения метода искусственного базиса.
        /// </summary>
        /// <param name="simplexTable">
        /// Конечная искусственная симплекс-таблица.
        /// </param>
        /// <returns></returns>
        private static Canvas renderLogAfterArtificialBasis(SimplexTable simplexTable)
        {
            Canvas canvas = new Canvas();
            Label label = new Label();
            label.FontSize = 20;
            label.Height = 70;
            label.VerticalContentAlignment = VerticalAlignment.Center;
            label.HorizontalContentAlignment = HorizontalAlignment.Center;
            label.Content = "Результат метода искуственного базиса: " + simplexTable.resultIteration;
            canvas.Width = label.Width;
            canvas.Height = label.Height;
            canvas.Children.Add(label);
            return canvas;
        }
        /// <summary>
        /// Выводит ответ, который получился в конце симплекс-метода.
        /// </summary>
        /// <param name="newTable">
        /// Конечная симплекс-таблица.
        /// </param>
        private static void outputAnswerSimplexMethod(SimplexTable newTable)
        {
            if (newTable.resultIteration.Contains("("))
            {
                Fraction functionValue = newTable.functionValue;
                if (Utils.minimize)
                {
                    functionValue = functionValue * new Fraction(-1, 1);
                }
                win.answerTextBlock.Text = "Ответ: F" + newTable.resultIteration + " = ";
                if (Utils.doubleFlag)
                    win.answerTextBlock.Text += (double)functionValue;
                else
                    win.answerTextBlock.Text += functionValue;
            }   
            else
            {
                win.answerTextBlock.Text = newTable.resultIteration;
            }
            return;
        }
        #endregion


    }

}
