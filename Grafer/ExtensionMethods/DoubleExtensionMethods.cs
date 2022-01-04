using System;

namespace Grafer.ExtensionMethods
{
    public static class DoubleExtensionMethods
    {
        const double degreeRatio = Math.PI / 180; // Poměr stupňů.

        //Převede číslo na radiány.
        public static double ToDegrees(this double value)
        {
            return value / degreeRatio;
        }

        //Převede radiány na stupně.
        public static double ToNumerical(this double value)
        {
            return value * degreeRatio;
        }
    }
}
