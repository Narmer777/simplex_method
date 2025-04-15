using System.Collections.Generic;

namespace IndividualLab
{
    /// <summary>
    /// Класс для хранения входных данных.
    /// </summary>
    public class InputData
    {
        /// <summary>
        /// Поле для хранения кол-ва переменных.
        /// </summary>
        public int numberOfVariables { get; set; }
        /// <summary>
        /// Поле для хранения кол-ва ограничений.
        /// </summary>
        public int numberOfRestrictions { get; set; }
        /// <summary>
        /// Поле для хранения базиса, если он выбран.
        /// </summary>
        public List<int> basis { get; set; }
        /// <summary>
        /// Поле для хранения метода решения.
        /// </summary>
        public SolutionMethod solutionMethod { get; set; }
        /// <summary>
        /// Поле для хранения типа задачи <c>"минимизация"</c> или <c>"максимизация"</c>.
        /// </summary>
        public string typeOfTask { get; set; }
        /// <summary>
        /// Поле для хранения целевой функции.
        /// </summary>
        public List<Fraction> targetFunction { get; set; }
        /// <summary>
        /// Поле для хранения матрицы ограничений.
        /// </summary>
        public Fraction[,] restrictions { get; set; }

        public InputData(
            int numberOfVariables,
            int numberOfRestrictions,
            SolutionMethod solutionMethod,
            List<Fraction> targetFunction,
            Fraction[,] restrictions,
            string typeOfTask,
            List<int> basis)
        {
            this.numberOfVariables = numberOfVariables;
            this.numberOfRestrictions = numberOfRestrictions;
            this.solutionMethod = solutionMethod;
            this.targetFunction = targetFunction;
            this.restrictions = restrictions;
            this.typeOfTask = typeOfTask;
            this.basis = basis;
        }
    }

    /// <summary>
    /// Класс для различных методов решения.
    /// </summary>
    public enum SolutionMethod
    {
        Simplex,
        Graphical,
    }
}
