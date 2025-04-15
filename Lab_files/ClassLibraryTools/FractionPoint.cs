using IndividualLab;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ClassLibraryTools
{
    /// <summary>
    /// Класс, который является точкой, но координаты являются обыкновенными дробями.
    /// </summary>
    public class FractionPoint
    {
        public Fraction X { get; set; }
        public Fraction Y { get; set; }
        public FractionPoint(Fraction x, Fraction y)
        {
            X = x;
            Y = y;
        }

        public Point ToPoint()
        {
            return new Point((double)X, (double)Y);
        }

        public static implicit operator Point(FractionPoint fractionPoint)
        {
            return new Point((double)fractionPoint.X, (double)fractionPoint.Y);
        }
        public override bool Equals(object obj)
        {
            if (obj is FractionPoint other)
            {
                return X.Equals(other.X) && Y.Equals(other.Y);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }
        
    }
}
