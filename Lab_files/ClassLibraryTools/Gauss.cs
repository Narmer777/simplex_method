using IndividualLab;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace ClassLibraryTools
{
    /// <summary>
    /// Класс, имеющий функции выполняющие метод Гаусса.
    /// </summary>
    public static class Gauss
    {
        //public static Fraction[,] GMethod1(List<int> Minor, Fraction[,] Matrix, int number_of_rows, int number_of_cols)
        //{
        //    int half_matrix_size = (number_of_rows * (number_of_rows - 1)) / 2;
        //    int oper_row = 1;
        //    int coef_row = 0;
        //    int counter = 2;
        //    Minor.Sort();
        //    int k = 0;
        //    Fraction coefficient = new Fraction(0, 1);


        //    for (int i = 0; i < half_matrix_size; i++)
        //    {
        //        if (Matrix[coef_row, Minor[k]] == 0)
        //        {
        //            for (int l = 1; l < number_of_rows; l++)
        //            {
        //                Matrix = SwapRows(Matrix, number_of_cols, coef_row, l);
        //                if (Matrix[coef_row, Minor[k]] != 0)
        //                {
        //                    break;
        //                }
        //            }
        //        }

        //        if (Matrix[coef_row, Minor[k]] == 0)
        //            return null;

        //        coefficient = Matrix[oper_row, Minor[k]] / Matrix[coef_row, Minor[k]];

        //        if (coefficient != 0)
        //        {
        //            for (int j = 0; j < number_of_cols; j++)
        //            {
        //                Matrix[oper_row, j] = Matrix[oper_row, j] - (Matrix[coef_row, j] * coefficient);
        //            }
        //        }

        //        oper_row += 1;
        //        if (oper_row > number_of_rows - 1)
        //        {
        //            oper_row = counter;
        //            coef_row++;
        //            counter++;
        //            k++;
        //        }
        //    }
        //    oper_row = number_of_rows - 2;
        //    coef_row = number_of_rows - 1;
        //    Minor.Reverse();
        //    k = 0;
        //    coefficient = new Fraction(0, 1);
        //    counter = number_of_rows - 3;

        //    for (int i = 0; i < half_matrix_size; i++)
        //    {
        //        if (Matrix[coef_row, Minor[k]] == 0)
        //        {
        //            return null;
        //        }

        //        coefficient = Matrix[oper_row, Minor[k]] / Matrix[coef_row, Minor[k]];

        //        if (coefficient != 0)
        //        {
        //            for (int j = 0; j < number_of_cols; j++)
        //            {
        //                Matrix[oper_row, j] = Matrix[oper_row, j] - (Matrix[coef_row, j] * coefficient);
        //            }
        //        }

        //        oper_row -= 1;

        //        if (oper_row < 0)
        //        {
        //            oper_row = counter;
        //            coef_row--;
        //            counter--;
        //            k++;
        //        }

        //    }
        //    Minor.Sort();
        //    int minor = 0;
        //    Fraction basis_elem = new Fraction(0, 1);
        //    for (int row = 0; row < number_of_rows; row++)
        //    {
        //        basis_elem = Matrix[row, Minor[minor]]; //TODO add IF
        //        //if (!basis_elem.Equals(new Fraction(0, 1)))
        //        //{
        //            for (int col = 0; col < number_of_cols; col++)
        //            {
        //                Matrix[row, col] = Matrix[row, col] / basis_elem;
        //            }
        //        //}
        //        minor++; 
        //    }
        //    return Matrix;
        //}

        /// <summary>
        /// Выполняет метод Гаусса для приведения матрицы ограничений к ступенчатому виду.
        /// </summary>
        /// <param name="Minor">Список индексов базисных элементов (минор).</param>
        /// <param name="Matrix">Двумерный массив с коэффициентами системы ограничений.</param>
        /// <param name="number_of_rows">Количество строк в матрице.</param>
        /// <param name="number_of_cols">Количество столбцов в матрице.</param>
        /// <returns>Матрица в ступенчатом виде с нормализованными диагональными элементами.</returns>
        /// <exception cref="Exception">
        /// Выбрасывается, если невозможно привести матрицу к ступенчатому виду, 
        /// что происходит, когда ведущий элемент равен нулю в процессе нормализации.
        /// </exception>
        public static Fraction[,] GMethod(List<int> Minor, Fraction[,] Matrix, int number_of_rows, int number_of_cols)
        {
            int half_matrix_size = (number_of_rows * (number_of_rows - 1)) / 2;
            List<int> Size = new List<int>();
            Size.Add(number_of_rows);
            Size.Add(number_of_cols);
            int coef_row = 0; 
            int k = 0;

            // Прямой ход
            for (int i = 0; i < half_matrix_size; i++)
            {
                if (k >= Minor.Count) 
                {
                    break;
                }

                if (Matrix[coef_row, Minor[k]] == 0)
                {
                    bool swapped = false;
                    for (int l = coef_row + 1; l < Size[0]; l++)
                    {
                        Matrix = SwapRows(Matrix, number_of_cols, coef_row, l);
                        if (Matrix[coef_row, Minor[k]] != 0)
                        {
                            swapped = true;
                            break;
                        }
                    }
                    if (!swapped)
                    {
                        k++;
                        continue;
                    }
                }

                for (int row = coef_row + 1; row < Size[0]; row++)
                {
                    if (k >= Minor.Count) break; 
                    Fraction coefficient = Matrix[row, Minor[k]] / Matrix[coef_row, Minor[k]];
                    for (int col = 0; col < Size[1]; col++)
                    {
                        Matrix[row, col] -= Matrix[coef_row, col] * coefficient;
                    }
                }

                coef_row++;
                k++;
            }

            // Обратный ход
            coef_row = Size[0] - 1;
            k = Minor.Count - 1;

            for (int i = 0; i < half_matrix_size; i++)
            {
                if (k < 0) 
                {
                    break;
                }

                for (int row = coef_row - 1; row >= 0; row--)
                {
                    if (k < 0) break; 
                    Fraction coefficient = Matrix[row, Minor[k]] / Matrix[coef_row, Minor[k]];
                    for (int col = 0; col < Size[1]; col++)
                    {
                        Matrix[row, col] -= Matrix[coef_row, col] * coefficient;
                    }
                }

                coef_row--;
                k--;
            }

            // Нормализация диагональных элементов
            for (int row = 0; row < Size[0]; row++)
            {
                if (row >= Minor.Count) 
                {
                    break;
                }

                Fraction basis_elem = Matrix[row, Minor[row]];
                if (basis_elem == 0)
                {
                    throw new Exception("Невозможно привести матрицу ограничений к ступенчатому виду.");
                    //return null;
                }
                for (int col = 0; col < Size[1]; col++)
                {
                    Matrix[row, col] /= basis_elem;
                }
            }

            return Matrix;
        }
        /// <summary>
        /// Меняет строки местами.
        /// </summary>
        /// <param name="Matrix">Матрица коэффицентов.</param>
        /// <param name="number_of_cols">Количество столбцов в матрице.</param>
        /// <param name="row1">Индекс первой строки для смены местами со второй строкой.</param>
        /// <param name="row2">Индекс второй строки для смены местами с первой строкой.</param>
        /// <returns>
        /// Возвращает матрицу коэффицентов с поменяными строками.
        /// </returns>
        private static Fraction[,] SwapRows(Fraction[,] Matrix, int number_of_cols, int row1, int row2)
        {
            for (int col = 0; col < number_of_cols; col++)
            {
                Fraction temp = Matrix[row1, col];
                Matrix[row1, col] = Matrix[row2, col];
                Matrix[row2, col] = temp;
            }
            return Matrix;
        }

    }
}
