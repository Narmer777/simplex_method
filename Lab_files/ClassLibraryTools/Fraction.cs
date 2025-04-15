using System;
using System.Numerics;

namespace IndividualLab
{
    /// <summary>
    /// Класс для работы с дробями.
    /// </summary>
    public class Fraction
    {
        public BigInteger numerator { get; set; }
        public BigInteger denominator { get; set; }
        public Fraction(BigInteger numerator, BigInteger denominator)
        {
            if (denominator < 0)
            {
                numerator = -numerator;
                denominator = -denominator;
            }
            this.numerator = numerator;
            this.denominator = denominator;
        }
        public static implicit operator string(Fraction fraction)
        {
            if (fraction.numerator % fraction.denominator == 0)
            {
                return (fraction.numerator / fraction.denominator).ToString();
            }
            return fraction.numerator.ToString() + "/" + fraction.denominator.ToString();
        }
        public static Fraction operator +(Fraction a, Fraction b)
        {
            BigInteger commonDenominator = a.denominator * b.denominator;
            BigInteger newNumerator = a.numerator * b.denominator + b.numerator * a.denominator;
            return Simplify(newNumerator, commonDenominator);
        }
        public static Fraction operator -(Fraction a, Fraction b)
        {
            BigInteger commonDenominator = a.denominator * b.denominator;      
            BigInteger newNumerator = a.numerator * b.denominator - b.numerator * a.denominator;
            return Simplify(newNumerator, commonDenominator);
        }
        public static Fraction operator *(Fraction a, Fraction b)
        {
            BigInteger newNumerator = a.numerator * b.numerator;
            BigInteger newDenominator = a.denominator * b.denominator;
            return Simplify(newNumerator, newDenominator);
        }
        public static Fraction operator /(Fraction a, Fraction b)
        {
            if (b.numerator == 0)
                throw new DivideByZeroException("Нельзя делить на дробь с числителем 0");
            BigInteger newNumerator = a.numerator * b.denominator;
            BigInteger newDenominator = a.denominator * b.numerator;
            return Simplify(newNumerator, newDenominator);
        }
        public static Fraction Simplify(BigInteger numerator, BigInteger denominator)
        {
            BigInteger gcd = GCD(numerator, denominator); 
            return new Fraction(numerator / gcd, denominator / gcd);
        }
        private static BigInteger GCD(BigInteger a, BigInteger b)
        {
            while (b != 0)
            {
                BigInteger temp = b;
                b = a % b;
                a = temp;
            }
            return BigInteger.Abs(a);
        }
        public static Fraction Parse(string s)
        {
            s = s.Trim();
            if (s.Contains("/"))
            {
                string[] parts = s.Split('/');
                if (parts.Length != 2)
                    throw new FormatException("Некорректный формат строки. Ожидался формат 'числитель/знаменатель'.");
                if (!BigInteger.TryParse(parts[0], out BigInteger numerator))
                    throw new FormatException("Числитель имеет неверный формат.");
                if (!BigInteger.TryParse(parts[1], out BigInteger denominator))
                    throw new FormatException("Знаменатель имеет неверный формат.");
                if (denominator == 0)
                    throw new DivideByZeroException("Знаменатель не может быть равен нулю.");
                return new Fraction(numerator, denominator);
            }//double.TryParse(s, out double decimalValue) //(double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double decimalValue)
            else if (double.TryParse(s, out double decimalValue))
            {
                return DecimalToFraction(decimalValue);
            }
            else
            {
                if (!BigInteger.TryParse(s, out BigInteger numerator))
                    throw new FormatException("Некорректный формат строки. Ожидалось целое число или дробь.");
                return new Fraction(numerator, 1);
            }
        }
        public static bool operator ==(Fraction a, int b)
        {
            return a.numerator == b * a.denominator;
        }
        public static bool operator !=(Fraction a, int b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            if (obj is Fraction fraction)
            {
                return this.numerator * fraction.denominator == fraction.numerator * this.denominator;
            }
            else if (obj is int integer)
            {
                return this == integer;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return numerator.GetHashCode() ^ denominator.GetHashCode();
        }
        public static bool operator <(Fraction a, int b)
        {
            return a.numerator < b * a.denominator;
        }
        public static bool operator <(int b, Fraction a)
        {
            return b * a.denominator < a.numerator;
        }
        public static bool operator >(Fraction a, int b)
        {
            return a.numerator > b * a.denominator;
        }
        public static bool operator >(int b, Fraction a)
        {
            return b * a.denominator > a.numerator;
        }
        public static Fraction swap(Fraction a)
        {
            return new Fraction(a.denominator, a.numerator);
        }
        public static bool operator <=(Fraction a, int b)
        {
            return a.numerator <= b * a.denominator;
        }
        public static bool operator <=(int b, Fraction a)
        {
            return b * a.denominator <= a.numerator;
        }
        public static bool operator >=(Fraction a, int b)
        {
            return a.numerator >= b * a.denominator;
        }
        public static bool operator >=(int b, Fraction a)
        {
            return b * a.denominator >= a.numerator;
        }
        public static bool operator <(Fraction a, Fraction b)
        {
            if (b == null)
            {
                return false;
            }
            return a.numerator * b.denominator < b.numerator * a.denominator;
        }
        public static bool operator >(Fraction a, Fraction b)
        {
            if (b == null)
            {
                return true;
            }
            return a.numerator * b.denominator > b.numerator * a.denominator;
        }

        public static implicit operator double(Fraction a)
        {
            return (double)a.numerator / (double)a.denominator;
        }
        public static Fraction DecimalToFraction(double decimalValue, int precision = 6)
        {
            decimalValue = Math.Round(decimalValue, precision);
            const double tolerance = 1.0E-10; 
            double numerator = decimalValue;
            double denominator = 1;
            while (Math.Abs(numerator - Math.Round(numerator)) > tolerance)
            {
                numerator *= 10;
                denominator *= 10;
            }
            int num = (int)Math.Round(numerator);
            int denom = (int)Math.Round(denominator);
            return Simplify(num, denom);
        }
        public override string ToString()
        {
            if (denominator == 1)
            {
                return numerator.ToString();
            }
            return $"{numerator}/{denominator}";
        }

    }
}


