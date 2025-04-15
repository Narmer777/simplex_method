using IndividualLab;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClassLibraryTools
{
    public class Graphic
    {
        /// <summary>
        /// Матрица коэффициентов ограничений задачи.
        /// <para>
        /// Используется для хранения системы ограничений линейной задачи.<br></br>
        /// Если флаг <see cref="Utils.Equations"/> = <see cref="true"/>.
        /// То ограничения имеют виду <c>ax + by <= c</c>
        /// </para>
        /// <para>
        /// Если флаг <see cref="Utils.Equations"/> = <see cref="false"/>. <br></br>
        /// То ограничения имеют вид <c>ax + by = c</c> или <c>ax = c</c>
        /// </para>
        /// </summary>
        private Fraction[,] restrictions { get; set; }
        /// <summary>
        /// Целевая функция, представлена в виде списка коэффициентов.
        /// Последний элемент списка представляет свободный член.
        /// </summary>
        private List<Fraction> targetFunction { get; set; }
        /// <summary>
        /// Холст <see cref="Canvas"/>, на котором отрисованы все ограничения, целевая функция, точки входящие в решение, полигон(прямая) допустимых решений.
        /// </summary>
        public Canvas renderedGraph { get; set; }
        /// <summary>
        /// Центр координат в координатах холста.
        /// </summary>
        private List<double> centerOfCoords { get; set; }
        /// <summary>
        /// Вектор нормали или антинормали, представленый в виде списка коэффицентов.
        /// </summary>
        private List<Fraction> normalVector { get; set; }
        /// <summary>
        /// Список всех точек решения, при которых выполняются ограничения, в обычных координатах.
        /// </summary>
        private List<FractionPoint> pointsOfSolution { get; set; }
        /// <summary>
        /// Список точек полигона(прямой) допустимых решений в координатах холста.
        /// </summary>
        private List<Point> polygon { get; set; }
        /// <summary>
        /// Коэффицент приближения\отдаления графика.
        /// </summary>
        public double scale { get; set; }
        /// <summary>
        /// Если случай n - m = 0 (где n - кол-во переменных; m - кол-во ограничений)
        /// Содержит точку решения. Иначе является <see cref="null"/>.
        /// </summary>
        public List<Fraction> fullPointNxNCase { get; set; }
        /// <summary>
        /// Край изначального полигона решений в обычных координатах.
        /// </summary>
        private const int BOUNDS_OF_SOLUTION_POLYGON = 18;
        /// <summary>
        /// Массив разных цветов, для отрисовки прямых.
        /// </summary>
        private static readonly Brush[] functionColors = new Brush[]
        {
        Brushes.DarkOrchid, Brushes.Green, Brushes.Blue, Brushes.Yellow,
        Brushes.Orange, Brushes.Purple, Brushes.Pink, Brushes.Cyan,
        Brushes.Brown, Brushes.Gray, Brushes.Magenta, Brushes.Teal,
        Brushes.Lime, Brushes.Aqua, Brushes.Fuchsia, Brushes.Maroon
        };
        /// <summary>
        /// 
        /// </summary>
        public string solution { get; set; }
        /// <summary>
        /// Массив индексов свободных переменных.
        /// </summary>
        private List<int> free { get; set; }
        /// <summary>
        /// Массив индексов базисных переменных.
        /// </summary>
        private List<int> basis { get; set; }
        /// <summary>
        /// Одномерный случай <see cref="dimensions"/> = <see langword="true"/>.
        /// Двумерный случай <see cref="dimensions"/> = <see langword="false"/>.
        /// </summary>
        private bool dimensions { get; set; }
        /// <summary>
        /// Размер одной клеточки сетки графика в пикселях на холсте.
        /// </summary>
        private double cellSize { get; set; } = 40; //22 width X 17 height cells 
        public Graphic(Fraction[,] restrictions, List<Fraction> targetFunction, List<int> free, List<int> basis, List<Fraction> point, string solution = "")
        {
            this.restrictions = restrictions;
            this.free = free;
            this.basis = basis;
            this.scale = 1;
            this.solution = solution;
            targetFunction[targetFunction.Count - 1] = targetFunction[targetFunction.Count - 1] * new Fraction(-1, 1);
            this.targetFunction = targetFunction;
            if (point != null)
            {
                fullPointNxNCase = new List<Fraction>();
                fullPointNxNCase = point;
            }
            centerOfCoords = new List<double>() { 4 * cellSize, 13 * cellSize };
            normalVector = new List<Fraction>();
            pointsOfSolution = new List<FractionPoint>();
            polygon = new List<Point>() {
                new Point(new Fraction(0, 1), new Fraction (0, 1)),
                new Point(new Fraction(BOUNDS_OF_SOLUTION_POLYGON, 1), new Fraction(0, 1)),
                new Point(new Fraction(BOUNDS_OF_SOLUTION_POLYGON, 1), new Fraction(BOUNDS_OF_SOLUTION_POLYGON, 1)),
                new Point(new Fraction(0, 1), new Fraction(BOUNDS_OF_SOLUTION_POLYGON, 1))};
            if (restrictions.GetLength(1) == 2)
            {
                dimensions = true;
                if (targetFunction.Count < 2) throw new ArgumentException("Некорректная задача.");
                normalVector.Add(targetFunction[0] * new Fraction(-1, 1));
            }
            else if (restrictions.GetLength(1) == 3)
            {
                dimensions = false;
                normalVector.Add(targetFunction[0] * new Fraction(-1, 1));
                normalVector.Add(targetFunction[1] * new Fraction(-1, 1));
            }
            else throw new ArgumentException("1D or 2D allowed!");
            createGrid();
        }

        #region RenderCanvas
        /// <summary>
        /// Создаёт холст, на котором отображаются все элементы графика.
        /// </summary>
        /// <param name="firstIteration">
        /// Если первый раз происходит отрисовка графика, то находится точка\прямая решения.
        /// Если меняется <see cref="scale"/>, то решение заново не находится.
        /// </param>
        private void createGrid(bool firstIteration = true)
        {
            Canvas canvas = new Canvas
            {
                Width = 880,
                Height = 680,
                Background = Brushes.White
            };
            DrawGrid(canvas, Brushes.LightGray);
            DrawAxis(canvas, Brushes.LightGray);
            renderedGraph = canvas;
            AddFunctionAndIntersection();
            FindPolygonOfValidSolutions(canvas);
            renderedGraph.ClipToBounds = true;
            if (firstIteration)
            {
                FindSolution();
                CreateStringSolution();
            }
        }
        /// <summary>
        /// Отрисовывает сетку графика.
        /// </summary>
        /// <param name="canvas">
        /// Холст <see cref="Canvas"/>, на котором нужно отрисовать.
        /// </param>
        /// <param name="gridColor">
        /// Цвет сетки.
        /// </param>
        private void DrawGrid(Canvas canvas, Brush gridColor)
        {
            for (double x = 0; x <= canvas.Width; x += cellSize)
            {
                Line verticalLine = new Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = canvas.Height,
                    Stroke = gridColor,
                    StrokeThickness = 0.5
                };
                canvas.Children.Add(verticalLine);
            }

            for (double y = 0; y <= canvas.Height; y += cellSize)
            {
                Line horizontalLine = new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = canvas.Width,
                    Y2 = y,
                    Stroke = gridColor,
                    StrokeThickness = 0.5
                };
                canvas.Children.Add(horizontalLine);
            }
        }
        /// <summary>
        /// Отрисовывает риски для значений на вертикальной оси.
        /// </summary>
        /// <param name="canvas">
        /// Холст <see cref="Canvas"/> для отрисовки рисок.
        /// </param>
        private void DrawTicksY(Canvas canvas)
        {
            int counterY = 1;
            for (int y = -3; y < 13; y++)
            {
                if (y == 0) { counterY++; continue; }
                double canvasY = cellSize * 17 - cellSize * counterY;
                AddAxisLabel(canvas, (y / scale).ToString(), centerOfCoords[0] - 30, canvasY - 7, 12);
                Line tick = new Line
                {
                    Y1 = canvasY,
                    X1 = centerOfCoords[0] - 5,
                    Y2 = canvasY,
                    X2 = centerOfCoords[0] + 5,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                canvas.Children.Add(tick);
                counterY++;

            }
        }
        /// <summary>
        /// Отрисовывает риски для значений на горизонтальной оси.
        /// </summary>
        /// <param name="canvas">
        /// Холст <see cref="Canvas"/> для отрисовки рисок.
        /// </param>
        private void DrawTicksX(Canvas canvas)
        {
            int counter = 1;
            for (int x = -3; x < 18; x++)
            {
                if (x == 0) { counter++; continue; }
                double canvasX = cellSize * counter;
                AddAxisLabel(canvas, (x / scale).ToString(), canvasX - 5, centerOfCoords[1] + 5, 12);
                Line tick = new Line
                {
                    X1 = canvasX,
                    Y1 = centerOfCoords[1] - 5,
                    X2 = canvasX,
                    Y2 = centerOfCoords[1] + 5,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                canvas.Children.Add(tick);
                counter++;
            }
        }
        /// <summary>
        /// Отрисовывает горизонтальную ось, если одномерный случай.
        /// Отрисовывает вертикальную и горизонтальную ось, если двумерный случай.
        /// </summary>
        /// <param name="canvas">
        /// Холст <see cref="Canvas"/> для отрисовки.
        /// </param>
        /// <param name="gridColor">
        /// Цвет осей.
        /// </param>
        private void DrawAxis(Canvas canvas, Brush gridColor)
        {
            if (!dimensions)
            {
                Line yAxis = new Line
                {
                    X2 = cellSize * centerOfCoords[0] / (cellSize),
                    Y2 = 0,
                    X1 = cellSize * centerOfCoords[0] / (cellSize),
                    Y1 = canvas.Height,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };
                canvas.Children.Add(yAxis);
                createArrow(yAxis, canvas);
                DrawTicksY(canvas);

            }
            AddAxisLabel(canvas, "X" + Utils.makeLowerIndex(free[0] + 1), canvas.Width - 20, centerOfCoords[1] - 30, 16);
            if (free.Count > 1) AddAxisLabel(canvas,
                "X" + Utils.makeLowerIndex(free[1] + 1),
                centerOfCoords[0] + 10,
                10, 16);
            AddAxisLabel(canvas, "0", centerOfCoords[0] - 10, centerOfCoords[1] + 5, 16);

            Line xAxis = new Line
            {
                X1 = 0,
                Y1 = cellSize * centerOfCoords[1] / (cellSize),
                X2 = canvas.Width,
                Y2 = cellSize * centerOfCoords[1] / (cellSize),
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            canvas.Children.Add(xAxis);
            createArrow(xAxis, canvas);
            DrawTicksX(canvas);
        }
        /// <summary>
        /// Отрисовывает многоугольник допустимых решений.
        /// </summary>
        private void DrawPolygonOfValidSolutions()
        {
            if (polygon.Count == 0) return;
            polygon = polygon.ToHashSet().ToList();
            if (polygon.Count == 1) return;
            if (!dimensions)
            {
                var points = polygon.Select(x => TransformPoint(x)).ToList();
                Polygon polygonOfValidSolutions = new Polygon
                {
                    Points = new PointCollection(points),
                    Fill = new SolidColorBrush(Color.FromArgb(128, 0, 255, 255)),
                };
                renderedGraph.Children.Insert(0, polygonOfValidSolutions);
                return;
            }
            var point1 = TransformPoint(polygon[0]);
            var point2 = TransformPoint(polygon[1]);
            Line line = new Line
            {
                X1 = point1.X,
                X2 = point2.X,
                Y1 = point1.Y,
                Y2 = point2.Y,
                Stroke = new SolidColorBrush(Color.FromArgb(128, 0, 255, 255)),
                StrokeThickness = 12,
            };
            renderedGraph.Children.Insert(0, line);
        }
        /// <summary>
        /// Отрисовывает на холсте все данные задачи: вектор нормали(антинормали), ограничения, функцию цели.
        /// </summary>
        /// <returns>
        /// Возвращает холст <see cref="Canvas"/> с данными о задаче.
        /// </returns>
        public Canvas CreateLogAboutTask()
        {
            Canvas canvas = new Canvas();
            int rows = restrictions.GetLength(0);
            int cols = restrictions.GetLength(1);
            double fontSize = 16;
            double height = 40;
            double x = 5;
            double y = 5;
            List<List<string>> restrictionsStr = new List<List<string>>();
            string targetStr = createStringForTargetFunction();
            restrictionsStr.Add(new List<string>() { targetStr });
            for (int i = 0; i < rows; i++)
            {
                string restriction = CreateRestrictionText(i, cols);
                restrictionsStr.Add(new List<string>() { restriction });
            }
            double maxWidth = Math.Max(Utils.GetMaxLabelWidth(restrictionsStr, 16), 200);
            AddTargetFunAndRestrictionsAndNormalVectorToLog(fontSize, height, maxWidth, restrictionsStr, x, y, canvas);
            canvas.Width = maxWidth + x + 20;
            canvas.HorizontalAlignment = HorizontalAlignment.Left;
            return canvas;
        }
        /// <summary>
        /// Строит строку с ответом, для отображения в окне для графической задачи.
        /// </summary>
        private void CreateStringSolution()
        {
            if (solution != "OK") { return; }
            string answer = "";
            foreach (var point in pointsOfSolution)
            {
                if (!Utils.doubleFlag)
                {
                    Fraction[] taskPoint;
                    if (Utils.NxNCase)
                    {
                        answer += "F(";
                        for (int i = 0; i < fullPointNxNCase.Count; i++)
                        {
                            if (fullPointNxNCase[i] < 0) { solution = "Задача неразрешима"; return; }
                            answer += fullPointNxNCase[i];
                            if (i != fullPointNxNCase.Count - 1) answer += "; ";
                        }
                        answer += ") = ";
                        answer += targetFunction[0] * point.X + targetFunction[1] * point.Y + targetFunction[2];
                        solution = answer;
                        return;
                    }
                    if (Utils.Equations)
                    {
                        taskPoint = new Fraction[1];
                        if (dimensions)
                        {
                            answer += "F(" + point.X + ") = ";
                            answer += targetFunction[0] * point.X + targetFunction[1];
                        }
                        else
                        {
                            answer += "F(" + point.X + ", " + point.Y + ") = ";
                            answer += targetFunction[0] * point.X + targetFunction[1] * point.Y + targetFunction[2];
                        }
                        solution = answer;
                        return;
                    }
                    else
                    {
                        taskPoint = new Fraction[free.Count + basis.Count];
                    }

                    taskPoint[free[0]] = point.X;
                    if (!dimensions) taskPoint[free[1]] = point.Y;
                    for (int i = 0; i < basis.Count; i++)
                    {
                        if (!dimensions)
                        {
                            taskPoint[basis[i]] = point.X * (restrictions[i, 0] * new Fraction(-1, 1)) +
                                point.Y * (restrictions[i, 1] * new Fraction(-1, 1)) +
                                restrictions[i, 2];
                            continue;
                        }
                        taskPoint[basis[i]] =
                            point.X * (restrictions[i, 0] * new Fraction(-1, 1)) + restrictions[i, 1];
                    }
                    answer += "F(";
                    for (int i = 0; i < taskPoint.Length; i++)
                    {
                        if (taskPoint[i] == null) { continue; }
                        answer += taskPoint[i];
                        if (i != taskPoint.Length - 1) answer += "; ";
                    }
                    answer += ") = ";
                    if (!dimensions)
                    {
                        answer += targetFunction[0] * point.X + targetFunction[1] * point.Y + targetFunction[2];
                        answer += "   ";
                        continue;
                    }
                    answer += targetFunction[0] * point.X + targetFunction[1];
                    answer += "   ";
                }
                else
                {
                    double[] taskPoint;
                    if (Utils.NxNCase)
                    {
                        answer += "F(";
                        for (int i = 0; i < fullPointNxNCase.Count; i++)
                        {
                            if (fullPointNxNCase[i] < 0) { solution = "Задача неразрешима"; return; }
                            answer += (double)fullPointNxNCase[i];
                            if (i != fullPointNxNCase.Count - 1) answer += "; ";
                        }
                        answer += ") = ";
                        answer += (double)(targetFunction[0] * point.X + targetFunction[1] * point.Y + targetFunction[2]);
                        solution = answer;
                        return;
                    }
                    if (Utils.Equations)
                    {
                        taskPoint = new double[1];
                        if (dimensions)
                        {
                            answer += "F(" + (double)point.X + ") = ";
                            answer += (double)(targetFunction[0] * point.X + targetFunction[1]);
                        }
                        else
                        {
                            answer += "F(" + (double)point.X + ", " + (double)point.Y + ") = ";
                            answer += (double)(targetFunction[0] * point.X + targetFunction[1] * point.Y + targetFunction[2]);
                        }
                        solution = answer;
                        return;
                    }
                    else
                    {
                        taskPoint = new double[free.Count + basis.Count];
                    }

                    taskPoint[free[0]] = point.X;
                    if (!dimensions) taskPoint[free[1]] = point.Y;
                    for (int i = 0; i < basis.Count; i++)
                    {
                        if (!dimensions)
                        {
                            taskPoint[basis[i]] = point.X * (restrictions[i, 0] * new Fraction(-1, 1)) +
                                point.Y * (restrictions[i, 1] * new Fraction(-1, 1)) +
                                restrictions[i, 2];
                            continue;
                        }
                        taskPoint[basis[i]] =
                            point.X * (restrictions[i, 0] * new Fraction(-1, 1)) + restrictions[i, 1];
                    }
                    answer += "F(";
                    for (int i = 0; i < taskPoint.Length; i++)
                    {
                        if (taskPoint[i] == null) { continue; }
                        answer += taskPoint[i];
                        if (i != taskPoint.Length - 1) answer += "; ";
                    }
                    answer += ") = ";
                    if (!dimensions)
                    {
                        answer += (double)(targetFunction[0] * point.X + targetFunction[1] * point.Y + targetFunction[2]);
                        answer += "   ";
                        continue;
                    }
                    answer += (double)(targetFunction[0] * point.X + targetFunction[1]);
                    answer += "   ";
                }

            }
            solution = answer;
            return;

        }
        /// <summary>
        /// Строит строку для отображения целевой функции задачи.
        /// </summary>
        /// <returns>
        /// Возвращает целевую функцию, записанную в строку.
        /// </returns>
        private string createStringForTargetFunction()
        {
            string targetFunStr = "F=";
            if (!Utils.doubleFlag)
            {
                for (int i = 0; i < targetFunction.Count - 1; i++)
                {
                    if (targetFunction[i] == 0) continue;
                    if (targetFunction[i] < 0)
                    {
                        if (targetFunction[i] == -1)
                            targetFunStr += "-";
                        else
                            targetFunStr += targetFunction[i].ToString();
                    }
                    else
                    {
                        if (targetFunction[i] == 1)
                        {
                            if (i != 0)
                                targetFunStr += "+";
                        }
                        else
                        {
                            if (i != 0)
                            {
                                if (targetFunction[i - 1].Equals(new Fraction(0, 1)))
                                    targetFunStr += targetFunction[i].ToString();
                                else
                                    targetFunStr += "+" + targetFunction[i].ToString();

                            }
                            else
                                targetFunStr += targetFunction[i].ToString();
                        }
                    }
                    if (i < free.Count) targetFunStr += "x" + Utils.makeLowerIndex(free[i] + 1);
                }
                int last = targetFunction.Count - 1;
                if (targetFunction[last] != 0)
                {
                    if (targetFunction[last] > 0)
                        targetFunStr += "+" + targetFunction[last];
                    else
                        targetFunStr += targetFunction[last];
                }
            }
            else
            {
                for (int i = 0; i < targetFunction.Count - 1; i++)
                {
                    if (targetFunction[i] == 0) continue;
                    if (targetFunction[i] < 0)
                    {
                        if (targetFunction[i] == -1)
                            targetFunStr += "-";
                        else
                            targetFunStr += ((double)targetFunction[i]).ToString();
                    }
                    else
                    {
                        if (targetFunction[i] == 1)
                        {
                            if (i != 0)
                                targetFunStr += "+";
                        }
                        else
                        {
                            if (i != 0)
                            {
                                if (targetFunction[i - 1].Equals(new Fraction(0, 1)))
                                    targetFunStr += ((double)targetFunction[i]).ToString();
                                else
                                    targetFunStr += "+" + ((double)targetFunction[i]).ToString();

                            }
                            else
                                targetFunStr += ((double)targetFunction[i]).ToString();
                        }
                    }
                    if (i < free.Count) targetFunStr += "x" + Utils.makeLowerIndex(free[i] + 1);
                }
                int last = targetFunction.Count - 1;
                if (targetFunction[last] != 0)
                {
                    if (targetFunction[last] > 0)
                        targetFunStr += "+" + (double)targetFunction[last];
                    else
                        targetFunStr += (double)targetFunction[last];
                }
            }
            if (Utils.minimize) targetFunStr += "-->min";
            else targetFunStr += " --> max";
            return targetFunStr;
        }
        /// <summary>
        /// Строит строку для отображения вектора(нормали\антинормали) задачи.
        /// </summary>
        /// <returns>
        /// Возвращает вектор нормали(антинормали), записанный в строку.
        /// </returns>
        private string CreateNormalVectorString()
        {
            string normalVectorStr = "";
            if (!Utils.doubleFlag)
            {
                if (Utils.minimize)
                {
                    if (!dimensions)
                    {
                        normalVectorStr += "-n = (" + normalVector[0] + ", " + normalVector[1] + ")";
                        return normalVectorStr;
                    }
                    normalVectorStr += "-n = (" + normalVector[0] + ")";
                    return normalVectorStr;
                }
                if (!dimensions)
                {
                    normalVectorStr += "n = (" + (normalVector[0] * new Fraction(-1, 1)) + ", " + (normalVector[1] * new Fraction(-1, 1)) + ")";
                    return normalVectorStr;
                }
                normalVectorStr += "n = (" + (normalVector[0] * new Fraction(-1, 1)) + ")";
            }
            else
            {
                if (Utils.minimize)
                {
                    if (!dimensions)
                    {
                        normalVectorStr += "-n = (" + (double)normalVector[0] + ", " + (double)normalVector[1] + ")";
                        return normalVectorStr;
                    }
                    normalVectorStr += "-n = (" + (double)normalVector[0] + ")";
                    return normalVectorStr;
                }
                if (!dimensions)
                {
                    normalVectorStr += "n = (" + (double)(normalVector[0] * new Fraction(-1, 1)) + ", " + (double)(normalVector[1] * new Fraction(-1, 1)) + ")";
                    return normalVectorStr;
                }
                normalVectorStr += "n = (" + (double)(normalVector[0] * new Fraction(-1, 1)) + ")";
            }
            return normalVectorStr;
        }
        /// <summary>
        /// Добавляет строки целевой функции, вектор нормали(антинормали), ограничений на холст <see cref="Canvas"/>.
        /// </summary>
        /// <param name="fontSize">
        /// Размер шрифта.
        /// </param>
        /// <param name="height">
        /// Высота каждой строки.
        /// </param>
        /// <param name="maxWidth">
        /// Максимальная длина строки.
        /// </param>
        /// <param name="restrictionsStr">
        /// Все строки целевой функции, вектор нормали(антинормали).
        /// </param>
        /// <param name="x">
        /// Отклонения от левого края холста <see cref="Canvas"/>.
        /// </param>
        /// <param name="y">
        /// Отклонение от верхнего края холста <see cref="Canvas"/>.
        /// </param>
        /// <param name="canvas">
        /// Холст <see cref="Canvas"/>.
        /// </param>
        private void AddTargetFunAndRestrictionsAndNormalVectorToLog(double fontSize, double height, double maxWidth, List<List<string>> restrictionsStr, double x, double y, Canvas canvas)
        {
            Label targetFunDescriptionLabel = new Label
            {
                FontSize = fontSize + 2,
                FontWeight = FontWeights.Bold,
                Height = height,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Content = "Целевая функция :",
            };
            Canvas.SetTop(targetFunDescriptionLabel, y);
            Canvas.SetLeft(targetFunDescriptionLabel, x);
            canvas.Children.Add(targetFunDescriptionLabel);
            y += height;
            Label targetFunLabel = new Label
            {
                //FontFamily = Utils.fontFamily,
                FontSize = fontSize,
                Height = height,
                Width = maxWidth,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Content = restrictionsStr[0][0],

            };
            Canvas.SetLeft(targetFunLabel, x);
            Canvas.SetTop(targetFunLabel, y);
            canvas.Children.Add(targetFunLabel);
            Rectangle targetFunRectangle = new Rectangle
            {
                Width = 20,
                Height = 20,
                Fill = Brushes.Red,
            };
            Canvas.SetLeft(targetFunRectangle, x + maxWidth);
            Canvas.SetTop(targetFunRectangle, y + 5);
            canvas.Children.Add(targetFunRectangle);
            y += height;
            Label restrDescriptionLabel = new Label
            {
                FontSize = fontSize + 2,
                FontWeight = FontWeights.Bold,
                Height = height,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Content = "Ограничения :",
            };
            Canvas.SetTop(restrDescriptionLabel, y);
            Canvas.SetLeft(restrDescriptionLabel, x);
            canvas.Children.Add(restrDescriptionLabel);
            y += height;
            for (int i = 1; i < restrictionsStr.Count; i++)
            {
                string restriction = restrictionsStr[i][0];
                Label restr = new Label
                {
                    Height = height,
                    FontSize = fontSize,
                    Width = maxWidth,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Content = restriction,
                };
                Canvas.SetLeft(restr, x);
                Canvas.SetTop(restr, y);
                canvas.Children.Add(restr);
                Rectangle restrRectangle = new Rectangle
                {
                    Width = 20,
                    Height = 20,
                    Fill = GetColorForFunction(i - 1),
                };
                Canvas.SetLeft(restrRectangle, x + maxWidth);
                Canvas.SetTop(restrRectangle, y + 5);
                canvas.Children.Add(restrRectangle);
                y += height;
            }
            string str = "";
            if (Utils.minimize) str += "Вектор антинормали:";
            else str += "Вектор нормали:";
            Label normalVectorDescriptionLabel = new Label
            {
                FontSize = fontSize + 2,
                FontWeight = FontWeights.Bold,
                Height = height,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Content = str,
            };
            Canvas.SetTop(normalVectorDescriptionLabel, y);
            Canvas.SetLeft(normalVectorDescriptionLabel, x);
            canvas.Children.Add(normalVectorDescriptionLabel);
            y += height;
            string normalVectorStr = CreateNormalVectorString();
            Label normalVectorLabel = new Label
            {
                Height = height,
                FontSize = fontSize,
                Width = maxWidth,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Content = normalVectorStr,
            };
            Canvas.SetTop(normalVectorLabel, y);
            Canvas.SetLeft(normalVectorLabel, x);
            canvas.Children.Add(normalVectorLabel);
            y += height;
            canvas.Height = y;
        }
        /// <summary>
        /// Добавляет надпись на холст <see cref="Canvas"/>.
        /// </summary>
        /// <param name="canvas">
        /// Холст <see cref="Canvas"/>.
        /// </param>
        /// <param name="text">
        /// Текст.
        /// </param>
        /// <param name="x">
        /// Отклонения от левого края холста <see cref="Canvas"/>.
        /// </param>
        /// <param name="y">
        /// Отклонение от верхнего края холста <see cref="Canvas"/>.
        /// </param>
        /// <param name="fontSize">
        /// Размер шрифта.
        /// </param>
        private void AddAxisLabel(Canvas canvas, string text, double x, double y, int fontSize)
        {
            TextBlock label = new TextBlock
            {
                Text = text,
                FontSize = fontSize,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
            };

            Canvas.SetLeft(label, x);
            Canvas.SetTop(label, y);

            canvas.Children.Add(label);
        }
        #endregion

        #region DrawFunction
        /// <summary>
        /// Запускает функции отрисовки функции, в зависимости, от того, двумерная ли это прямая или одномерная.
        /// </summary>
        /// <param name="function">
        /// Функция прямой, представленная в виде массива коэффицентов.
        /// </param>
        /// <param name="color">
        /// Цвет отрисовки функции.
        /// </param>
        private void DrawFunction(List<double> function, Brush color)
        {
            if (function.Count == 2) { DrawFunctionWithOneDimension(function, color); return; }
            double a = function[0]; double b = function[1]; double c = function[2];
            if (a == 0 && b == 0) { return; }
            if (a == 0) { DrawFunctionWithOneDimension(new List<double>() { b, c }, color, "horizontal"); return; }
            if (b == 0) { DrawFunctionWithOneDimension(new List<double>() { a, c }, color); return; }
            DrawFunctionWithTwoDimensions(function, color);
        }
        /// <summary>
        /// Отрисовывает вектор нормали(антинормали).
        /// </summary>
        private void DrawVector()
        {
            Point point;
            Point canvasPoint;
            List<double> normalVector = this.normalVector.Select(x => (double)x).ToList();
            if (!Utils.minimize) normalVector = normalVector.Select(x => x * -1).ToList();
            // Определяем координаты вектора в зависимости от размерности
            if (dimensions)
            {
                point = new Point(normalVector[0], 0);
                canvasPoint = TransformPoint(point);
            }
            else
            {
                point = new Point(normalVector[0], normalVector[1]);
                canvasPoint = TransformPoint(point);
            }

            double length = Math.Sqrt(Math.Pow(canvasPoint.X - centerOfCoords[0], 2) + Math.Pow(canvasPoint.Y - centerOfCoords[1], 2));
            double maxLength = 100;
            if (length != maxLength)
            {
                if (length == 0) return;
                double vectorScale = maxLength / length;
                canvasPoint.X = centerOfCoords[0] + (canvasPoint.X - centerOfCoords[0]) * vectorScale;
                canvasPoint.Y = centerOfCoords[1] + (canvasPoint.Y - centerOfCoords[1]) * vectorScale;
            }
            Line vector = new Line
            {
                X1 = centerOfCoords[0],
                Y1 = centerOfCoords[1],
                X2 = canvasPoint.X,
                Y2 = canvasPoint.Y,
                Stroke = Brushes.Red,
                StrokeThickness = 2
            };
            renderedGraph.Children.Add(vector);
            createArrow(vector, renderedGraph);
        }
        /// <summary>
        /// Отрисовывает двумерную прямую.
        /// </summary>
        /// <param name="function">
        /// Функция прямой, представленная в виде массива коэффицентов.
        /// </param>
        /// <param name="color">
        /// Цвет отрисовки функции.
        /// </param>
        private void DrawFunctionWithTwoDimensions(List<double> function, Brush color)
        {
            double a = function[0];
            double b = function[1];
            double c = function[2];
            List<Point> canvasIntersectionsOnDefaultCoords = new List<Point>();
            double xMin = (0 - centerOfCoords[0]) / (cellSize * scale);
            double xMax = (renderedGraph.Width - centerOfCoords[0]) / (cellSize * scale);
            double yMin = (centerOfCoords[1] - renderedGraph.Height) / (cellSize * scale);
            double yMax = centerOfCoords[1] / (cellSize * scale);
            foreach (double x in new[] { xMax, xMin })
            {
                double y = (c - a * x) / b;
                if (y >= yMin && y <= yMax)
                {
                    canvasIntersectionsOnDefaultCoords.Add(new Point(x, y));
                }
            }

            foreach (double y in new[] { yMax, yMin })
            {
                double x = (c - b * y) / a;
                if (x >= xMin && x <= xMax)
                {
                    canvasIntersectionsOnDefaultCoords.Add((new Point(x, y)));
                }
            }
            if (canvasIntersectionsOnDefaultCoords.Count < 2) return;
            canvasIntersectionsOnDefaultCoords = canvasIntersectionsOnDefaultCoords.ToHashSet().ToList();
            Point point1;
            Point point2;
            if (canvasIntersectionsOnDefaultCoords.Count > 2)
            {
               point1 = TransformPoint(canvasIntersectionsOnDefaultCoords[0]);
               point2 = TransformPoint(canvasIntersectionsOnDefaultCoords[2]);
            }
            else
            {
                point1 = TransformPoint(canvasIntersectionsOnDefaultCoords[0]);
                point2 = TransformPoint(canvasIntersectionsOnDefaultCoords[1]);
            }
            Line line = new Line
            {
                X1 = point1.X,
                X2 = point2.X,
                Y1 = point1.Y,
                Y2 = point2.Y,
                Stroke = color,
                StrokeThickness = 2
            };
            renderedGraph.Children.Add(line);
        }
        /// <summary>
        /// Отрисовывает одномерную прямую.
        /// </summary>
        /// <param name="function">
        /// Функция прямой, представленная в виде массива коэффицентов.
        /// </param>
        /// <param name="color">
        /// Цвет отрисовки функции.
        /// </param>
        /// <param name="alligment">
        /// Является ли эта прямая вертикальной или горизонтальной.
        /// </param>
        private void DrawFunctionWithOneDimension(List<double> function, Brush color, string alligment = "vertical")
        {
            Point canvasPoint;
            if (function[0] == 0) return;
            if (alligment == "vertical")
            {
                var point = new Point(function[1] / function[0], 0);
                canvasPoint = TransformPoint(point);
                Line verticalLine = new Line
                {
                    X1 = canvasPoint.X,
                    Y1 = 0,
                    X2 = canvasPoint.X,
                    Y2 = renderedGraph.Height,
                    Stroke = color,
                    StrokeThickness = 2
                };
                renderedGraph.Children.Add(verticalLine);
                return;
            }
            var point1 = new Point(0, function[1] / function[0]);
            canvasPoint = TransformPoint(point1);
            Line horizontalLine = new Line
            {
                X1 = 0,
                Y1 = canvasPoint.Y,
                X2 = renderedGraph.Width,
                Y2 = canvasPoint.Y,
                Stroke = color,
                StrokeThickness = 2
            };
            renderedGraph.Children.Add(horizontalLine);


        }
        #endregion

        #region DrawIntersectPoint
        /// <summary>
        /// Отрисовывают точку пересечения двух двумерных ограничений, если она подходит под все ограничения.
        /// </summary>
        /// <param name="intersectPointFraction">
        /// Точка пересечения.
        /// </param>
        /// <param name="color">
        /// Цвет точки.
        /// </param>
        private void DrawIntersectPointTwoDimensional(FractionPoint intersectPointFraction, Brush color)
        {
            if (!checkRestrictions(intersectPointFraction)) return;

            Ellipse point = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = color,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            Point intersectPoint = intersectPointFraction.ToPoint();
            Point canvasPoint = TransformPoint(intersectPoint);
            Canvas.SetLeft(point, canvasPoint.X - point.Width / 2);
            Canvas.SetTop(point, canvasPoint.Y - point.Height / 2);
            renderedGraph.Children.Add(point);
            pointsOfSolution.Add(intersectPointFraction);
        }
        /// <summary>
        /// Отрисовывают точку пересечения одномерного ограничения, если она подходит под все ограничения.
        /// </summary>
        /// <param name="intersectPointFraction">
        /// Точка пересечения.
        /// </param>
        /// <param name="color">
        /// Цвет точки.
        /// </param>
        private void DrawIntersectPointOneDimensional(FractionPoint intersectPointFraction, Brush color)
        {
            if (!checkRestrictions(intersectPointFraction)) return;

            Ellipse point = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = color,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            Point intersectPoint = intersectPointFraction.ToPoint();
            Point canvasPoint = TransformPoint(intersectPoint);
            Canvas.SetLeft(point, canvasPoint.X - point.Width / 2);
            Canvas.SetTop(point, canvasPoint.Y - point.Height / 2);
            renderedGraph.Children.Add(point);
            pointsOfSolution.Add(intersectPointFraction);
        }
        #endregion

        #region CalculationFunctions
        /// <summary>
        /// Поиск решения, если оно существует.
        /// </summary>
        private void FindSolution()
        {
            if (solution == "Задача неразрешима") return;
            //if (polygon.Count == 0)
            //{
            //    solution = "Задача неразрешима.";
            //    return;
            //}
            FindSolutionInConstraintedPolygon();
        }
        /// <summary>
        /// Поиск точки\прямой решения из массива допустимых точек решения <see cref="pointsOfSolution"/>.
        /// </summary>
        private void FindSolutionInConstraintedPolygon()
        {
            int rows = restrictions.GetLength(0);
            List<FractionPoint> solution = new List<FractionPoint>();
            Fraction newFunctionValue;
            Fraction functionValue = new Fraction(0, 1);
            var pointsOfSol = Utils.RemoveDuplicates(pointsOfSolution);
            pointsOfSolution.Clear();
            if (!dimensions)
            {
                for (int i = 0; i < pointsOfSol.Count; i++)
                {
                    var point = pointsOfSol[i];
                    newFunctionValue = targetFunction[0] * point.X + targetFunction[1] * point.Y + targetFunction[2];
                    if (Utils.minimize)
                    {
                        if (i == 0)
                        {
                            functionValue = newFunctionValue;
                            solution.Add(point);
                            continue;
                        }
                        if (newFunctionValue < functionValue)
                        {
                            functionValue = newFunctionValue;
                            solution.Clear();
                            solution.Add(point);
                            continue;
                        }
                        if (newFunctionValue.Equals(functionValue))
                        {
                            solution.Add(point);
                            continue;
                        }
                        continue;
                    }
                    if (i == 0)
                    {
                        functionValue = newFunctionValue;
                        solution.Add(point);
                        continue;
                    }
                    if (newFunctionValue > functionValue)
                    {
                        functionValue = newFunctionValue;
                        solution.Clear();
                        solution.Add(point);
                        continue;
                    }
                    if (newFunctionValue.Equals(functionValue))
                    {
                        solution.Add(point);
                        continue;
                    }
                }
                pointsOfSolution = Utils.RemoveDuplicates(solution);
                if (pointsOfSolution.Count == 0) { this.solution = "Задача неразрешима"; return; }
                this.solution = "OK";
                return;
            }
            for (int i = 0; i < pointsOfSol.Count; i++)
            {
                var point = pointsOfSol[i];
                newFunctionValue = targetFunction[0] * point.X + targetFunction[1];
                if (Utils.minimize)
                {
                    if (i == 0)
                    {
                        functionValue = newFunctionValue;
                        solution.Add(point);
                        continue;
                    }
                    if (newFunctionValue < functionValue)
                    {
                        solution.Clear();
                        solution.Add(point);
                        continue;
                    }
                    continue;
                }

                if (i == 0)
                {
                    functionValue = newFunctionValue;
                    solution.Add(point);
                    continue;
                }
                if (newFunctionValue > functionValue)
                {
                    solution.Clear();
                    solution.Add(point);
                    continue;
                }
            }
            pointsOfSolution = Utils.RemoveDuplicates(solution);
            if (pointsOfSolution.Count == 0) { this.solution = "Задача неразрешима"; return; }
            this.solution = "OK";
        }
        /// <summary>
        /// Преобразование точки в координаты холста <see cref="Canvas"/>.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>
        /// Возвращает точку на холсте.
        /// </returns>
        private Point TransformPoint(Point point)
        {
            double y = centerOfCoords[1] - (point.Y * cellSize) * scale;
            double x = centerOfCoords[0] + (point.X * cellSize) * scale;
            return new Point(x, y);
        }
        /// <summary>
        /// Находит точки многоугольника решений на холсте и отрисовывает его.
        /// Нахождение точек многоугольника решений происходит с помощью "отсечений" от начальной области(первой четверти графика) частей с помощью ограничений.
        /// </summary>
        /// <param name="canvas">
        /// 
        /// </param>
        private void FindPolygonOfValidSolutions(Canvas canvas)
        {
            int rows = restrictions.GetLength(0);
            int cols = restrictions.GetLength(1);
            if (Utils.Equations)
            {
                polygon = Utils.RemoveDuplicates(pointsOfSolution).Select(x => (Point)x).ToList();
                return;
            }
            List<Point> newPolygonPoints = new List<Point>();
            if (!dimensions)
            {
                for (int i = 0; i < rows; i++)
                {
                    Fraction a = restrictions[i, 0];
                    Fraction b = restrictions[i, 1];
                    Fraction c = restrictions[i, 2];
                    for (int k = 0; k < polygon.Count; k++)
                    {
                        Point p1 = polygon[k];
                        Point p2 = polygon[(k + 1) % polygon.Count];

                        double x1 = p1.X, y1 = p1.Y;
                        double x2 = p2.X, y2 = p2.Y;
                        double denominator = a * (x2 - x1) + b * (y2 - y1);
                        if (Math.Abs(denominator) < 1e-9) continue;
                        double t = (c - a * x1 - b * y1) / denominator;
                        if (t >= 0 && t <= 1)
                        {
                            double x = x1 + t * (x2 - x1);
                            double y = y1 + t * (y2 - y1);
                            Point newPolygonPoint = new Point(x, y);
                            newPolygonPoints.Add(newPolygonPoint);
                        }
                    }
                    double lineEquation(double x, double y) => a * x + b * y - c;
                    for (int l = 0; l < polygon.Count; l++)
                    {
                        Point p1 = polygon[l];
                        if (lineEquation(p1.X, p1.Y) <= 0)
                        {
                            newPolygonPoints.Add(p1);
                        }
                    }
                    newPolygonPoints = SortPointsClockwise(newPolygonPoints);
                    polygon = newPolygonPoints.ToList();
                    newPolygonPoints.Clear();
                }
                DrawPolygonOfValidSolutions();
                return;
            }

            polygon = polygon.Where(x => x.Y == 0).ToList();
            for (int i = 0; i < rows; i++)
            {
                Fraction a = restrictions[i, 0];
                Fraction c = restrictions[i, 1];
                if (a == 0) continue;
                double xLine = c / a;
                for (int k = 0; k < polygon.Count - 1; k++)
                {
                    Point p1 = polygon[k];
                    Point p2 = polygon[k + 1];

                    double x1 = p1.X;
                    double x2 = p2.X;
                    if ((xLine >= x1 && xLine <= x2) || (xLine <= x1 && xLine >= x2))
                    {
                        newPolygonPoints.Add(new Point(xLine, 0));
                    }
                }
                newPolygonPoints = newPolygonPoints.Distinct().ToList();
                double lineEquations(double x) => a * x - c;
                for (int l = 0; l < polygon.Count; l++)
                {
                    Point p1 = polygon[l];
                    if (lineEquations(p1.X) <= 0)
                    {
                        newPolygonPoints.Add(p1);
                    }
                }
                newPolygonPoints = SortPointsClockwise(newPolygonPoints);
                polygon = newPolygonPoints.ToList();
                newPolygonPoints.Clear();
            }
            DrawPolygonOfValidSolutions();

        }
        /// <summary>
        /// Запускает для каждого ограничения функцию отрисовки прямых.
        /// И для каждой точки пересечения ограничений запускает функцию отрисовки точки.
        /// </summary>
        private void AddFunctionAndIntersection()
        {
            List<double> targetFun = targetFunction.Select(x => (double)x).ToList();
            targetFun[targetFun.Count - 1] = 0;
            DrawFunction(targetFun, Brushes.Red);
            if (!(Utils.Equations && dimensions))
                DrawIntersectPointOneDimensional(new FractionPoint(new Fraction(0, 1), new Fraction(0, 1)), Brushes.Green); 
            DrawVector();
            List<List<Fraction>> funcs = new List<List<Fraction>>();
            for (int i = 0; i < restrictions.GetLength(0); i++)
            {
                List<Fraction> func = new List<Fraction>();
                for (int j = 0; j < restrictions.GetLength(1); j++)
                {
                    func.Add(restrictions[i, j]);
                }
                var function = func.Select(x => (double)x).ToList();
                DrawFunction(function, GetColorForFunction(i));
                funcs.Add(func);
            }

            if (dimensions)
            {
                foreach (var func in funcs)
                {
                    if (func[0] == 0) continue;
                    DrawIntersectPointOneDimensional(new FractionPoint(func[1] / func[0], new Fraction(0, 1)), Brushes.Green);
                }
                return;
            }

            foreach (var func in funcs)
            {
                if (func[0] == 0 && func[1] == 0) continue;
                if (func[0] == 0)
                {
                    DrawIntersectPointOneDimensional(new FractionPoint(new Fraction(0, 1), func[2] / func[1]), Brushes.Green);
                }
                else if (func[1] == 0)
                {
                    DrawIntersectPointOneDimensional(new FractionPoint(func[2] / func[0], new Fraction(0, 1)), Brushes.Green);
                }
                else
                {
                    var points = findIntersectionPointWihtAxises(func);
                    DrawIntersectPointOneDimensional(points.Item1, Brushes.Green);
                    DrawIntersectPointOneDimensional(points.Item2, Brushes.Green);
                }


                foreach (var func1 in funcs)
                {
                    if (func == func1) continue;
                    if (func1[0] == 0 && func1[1] == 0) continue;
                    FractionPoint intersectionPoint = findIntersectionPoint(func, func1);
                    if (intersectionPoint == null) continue;
                    DrawIntersectPointTwoDimensional(intersectionPoint, Brushes.Green);
                }
            }
        }
        /// <summary>
        /// Нахождение точек пересечения между двумя ограничениями.
        /// </summary>
        /// <param name="func">
        /// Первое ограничение, представленное в виде массива коэффицентов.
        /// </param>
        /// <param name="func1">
        /// Второе ограничение, представленное в виде массива коэффицентов
        /// </param>
        /// <returns>
        /// Точку пересечения двух ограничений.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Если система ограничений несовместна.
        /// </exception>
        private FractionPoint findIntersectionPoint(List<Fraction> func, List<Fraction> func1)
        {

            Fraction a1 = func[0]; Fraction b1 = func[1]; Fraction c1 = func[2];
            Fraction a2 = func1[0]; Fraction b2 = func1[1]; Fraction c2 = func1[2];
            Fraction y;
            Fraction x;
            if (a1 == 0)
            {
                if (b1 == 0) throw new ArgumentException("Система ограничений несовместна!");
                if (a2 == 0) return null;
                y = c1 / b1;
                x = (c2 - b2 * (c1 / b1)) / a2;
                return new FractionPoint(x, y);
            }

            if (a2 == 0)
            {
                if (b2 == 0) throw new ArgumentException("Система ограничений несовместна!");
                if (a1 == 0) return null;
                y = c2 / b2;
                x = (c1 - b1 * (c2 / b2)) / a1;
                return new FractionPoint(x, y);
            }

            if (b1 == 0)
            {
                if (a1 == 0) throw new ArgumentException("Система ограничений несовместна!");
                if (b2 == 0) return null;
                x = c1 / a1;
                y = (c2 - (a2 * (c1 / a1))) / b2;
                return new FractionPoint(x, y);
            }

            if (b2 == 0)
            {
                if (a2 == 0) throw new ArgumentException("Система ограничений несовместна!");
                if (b1 == 0) return null;
                x = c2 / a2;
                y = (c1 - a1 * (c2 / a2)) / b1;
                return new FractionPoint(x, y);
            }


            Fraction delta = a1 * b2 - a2 * b1;
            if (Math.Abs(delta) < 1e-9)
            {
                return null;
            }
            Fraction deltaX = c1 * b2 - c2 * b1;
            Fraction deltaY = a1 * c2 - a2 * c1;

            x = deltaX / delta;
            y = deltaY / delta;
            return new FractionPoint(x, y);
        }
        /// <summary>
        /// Находит две точки пересечения ограничения с вертикальной и горизонтальной прямой.
        /// </summary>
        /// <param name="func">
        /// Ограничение.
        /// </param>
        /// <returns>
        /// Возвращает кортеж из точки пересечения с горизонтальной осью и из точки пересечения с вертикальной осью.
        /// </returns>
        private (FractionPoint, FractionPoint) findIntersectionPointWihtAxises(List<Fraction> func)
        {
            Fraction a = func[0]; Fraction b = func[1]; Fraction c = func[2];
            FractionPoint intersectPointWithX = new FractionPoint(c / a, new Fraction(0, 1));
            FractionPoint intersectPointWithY = new FractionPoint(new Fraction(0, 1), c / b);
            return (intersectPointWithX, intersectPointWithY);
        }

        #endregion

        #region Utils
        /// <summary>
        /// Сортирует точки по часовой стрелке.
        /// </summary>
        /// <param name="points">
        /// Массив содержащий точки.
        /// </param>
        /// <returns></returns>
        private List<Point> SortPointsClockwise(List<Point> points)
        {
            if (points.Count == 0) return points;
            double centerX = points.Average(p => p.X);
            double centerY = points.Average(p => p.Y);

            // Сортируем по углу относительно центра
            return points.OrderBy(p => Math.Atan2(p.Y - centerY, p.X - centerX)).ToList();
        }
        /// <summary>
        /// Обновление <see cref="scale"/>, отрисовка графика с новый скалированием.
        /// </summary>
        /// <param name="newScale">
        /// Новый <see cref="scale"/>
        /// </param>
        public void UpdateScale(double newScale)
        {
            this.scale = newScale;
            if (!dimensions)
            {
                polygon = new List<Point>()
                {
                    new Point(0, 0),
                    new Point(BOUNDS_OF_SOLUTION_POLYGON / scale, 0),
                    new Point(BOUNDS_OF_SOLUTION_POLYGON / scale, BOUNDS_OF_SOLUTION_POLYGON / scale),
                    new Point(0, 13 / scale)
                };
            }
            else
            {
                polygon = new List<Point>()
                {
                    new Point(0, 0),
                    new Point(BOUNDS_OF_SOLUTION_POLYGON / scale, 0),
                };
            }
            createGrid(false);
        }
        /// <summary>
        /// Получает по индексу цвет из массива цветов.
        /// </summary>
        /// <param name="index">
        /// Индекс.
        /// </param>
        /// <returns>
        /// Возвращает цвет.
        /// </returns>
        private Brush GetColorForFunction(int index)
        {
            return functionColors[index];
        }
        /// <summary>
        /// Проверяет точку.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private bool checkRestrictions(FractionPoint point)
        {
            if (point.X < 0 || point.Y < 0)
                return false;
            if (Utils.Equations)
            {
                for (int i = 0; i < restrictions.GetLength(0); i++)
                {
                    Fraction sum = new Fraction(0, 1);
                    if (dimensions) { sum += restrictions[i, 0] * point.X; if (sum > restrictions[i, 1]) return false; }
                    else { sum += restrictions[i, 0] * point.X + restrictions[i, 1] * point.Y; if (!sum.Equals(restrictions[i, 2])) return false; }
                }
                return true;
            }
            else
            {
                for (int i = 0; i < restrictions.GetLength(0); i++)
                {
                    Fraction sum = new Fraction(0, 1);
                    if (dimensions) { sum += restrictions[i, 0] * point.X; if (sum > restrictions[i, 1]) return false; }
                    else { sum += restrictions[i, 0] * point.X + restrictions[i, 1] * point.Y; if (sum > restrictions[i, 2]) return false; }
                }
                return true;
            }

        }
        /// <summary>
        /// Строит строку из ограничения.
        /// </summary>
        /// <param name="row">
        /// Индекс строки из двумерного массива ограничений.
        /// </param>
        /// <param name="cols">
        /// Кол-во колонок в двумерном массиве ограничений <see cref="restrictions"/>.
        /// </param>
        /// <returns>
        /// Возвращает ограничение, преобразованное в строку.
        /// </returns>
        private string CreateRestrictionText(int row, int cols)
        {
            bool isZero = true;
            var restrictionBuilder = new StringBuilder();
            for (int col = 0; col < cols; col++)
            {
                Fraction fractionValue = restrictions[row, col];
                if (Utils.doubleFlag)
                {
                    double value = ((double)fractionValue);
                    if (col == free.Count)
                    {
                        if (Utils.Equations)
                        {
                            if (isZero)
                                restrictionBuilder.Append("0");
                            restrictionBuilder.Append("=").Append(value.ToString());
                        }
                        else
                        {
                            if (isZero)
                                restrictionBuilder.Append("0");
                            restrictionBuilder.Append("≤").Append(value.ToString());
                        }
                        break;
                    }

                    if (value != 0)
                    {
                        isZero = false;
                        if (restrictionBuilder.Length > 0 && value > 0) restrictionBuilder.Append("+");

                        if (value == -1)
                        {
                            restrictionBuilder.Append("-");
                        }
                        else if (value != 1)
                        {
                            restrictionBuilder.Append(value.ToString());
                        }

                        if (col < free.Count)
                        {
                            restrictionBuilder.Append("x").Append(Utils.makeLowerIndex(free[col] + 1));
                        }
                    }

                }
                else
                {
                   Fraction value = fractionValue;
                    if (col == free.Count)
                    {
                        if (Utils.Equations)
                        {
                            if (isZero)
                                restrictionBuilder.Append("0");
                            restrictionBuilder.Append("=").Append(value.ToString());
                        }
                        else
                        {
                            if (isZero)
                                restrictionBuilder.Append("0");
                            restrictionBuilder.Append("≤").Append(value.ToString());
                        }
                        break;
                    }

                    if (value != 0)
                    {
                        isZero = false;
                        if (restrictionBuilder.Length > 0 && value > 0) restrictionBuilder.Append("+");

                        if (value == -1)
                        {
                            restrictionBuilder.Append("-");
                        }
                        else if (value != 1)
                        {
                            restrictionBuilder.Append(value.ToString());
                        }

                        if (col < free.Count)
                        {
                            restrictionBuilder.Append("x").Append(Utils.makeLowerIndex(free[col] + 1));
                        }
                    }
                }
            }
            return restrictionBuilder.ToString();
        }
        /// <summary>
        /// Создаёт стрелочку на конце прямой <see cref="Line"/>.
        /// </summary>
        /// <param name="xAxis">
        /// Прямая <see cref="Line"/>.
        /// </param>
        /// <param name="canvas">
        /// Холст <see cref="Canvas"/>.
        /// </param>
        private void createArrow(Line xAxis, Canvas canvas)
        {
            double angle = Math.Atan2(xAxis.Y2 - xAxis.Y1, xAxis.X2 - xAxis.X1);
            double arrowSize = 10;
            double angleOffset = Math.PI / 6;
            Point arrowTip = new Point(xAxis.X2, xAxis.Y2);
            Point arrowLeft = new Point(
                arrowTip.X - arrowSize * Math.Cos(angle - angleOffset),
                arrowTip.Y - arrowSize * Math.Sin(angle - angleOffset)
            );
            Point arrowRight = new Point(
                arrowTip.X - arrowSize * Math.Cos(angle + angleOffset),
                arrowTip.Y - arrowSize * Math.Sin(angle + angleOffset)
            );
            Polyline arrow = new Polyline
            {
                //Stroke = Brushes.Black,
                StrokeThickness = 1,
                Fill = Brushes.Black,
                Points = new PointCollection { arrowLeft, arrowTip, arrowRight }
            };
            canvas.Children.Add(arrow);
        }
        #endregion

    }

}
