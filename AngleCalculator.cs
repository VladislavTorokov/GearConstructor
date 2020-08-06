using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GearConsole
{
    public static class AngleCalculator
    {
        private const double pi = Math.PI;
        //Рассчет функции инвалюты
        public static double InvoluteAngle(double inv)
        {
            return (Math.Pow(3 * inv, 1 / 3.0) - (2 * inv) / 5.0 + (9 / 175.0) * Math.Pow(3, 2 / 3.0) * Math.Pow(inv, 5 / 3.0) - (2 / 175.0) * Math.Pow(3, 1 / 3.0) * Math.Pow(inv, 7 / 3.0) - (144 / 67375.0) * Math.Pow(inv, 3) + (3258 / 3128125.0) * Math.Pow(3, 2 / 3.0) * Math.Pow(inv, 11 / 3.0) - (49711 / 153278125.0) * Math.Pow(3, 1 / 3.0) * Math.Pow(inv, 13 / 3.0));
        }

        public static double InvoluteValue(double angle)
        {
            return (Math.Tan(angle) - angle);
        }

        public static double AngleToRadian(double angle)
        {
            return angle * pi / 180.0;

        }
    }
}
