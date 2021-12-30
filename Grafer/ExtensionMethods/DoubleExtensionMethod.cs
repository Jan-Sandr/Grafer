using System;

namespace Grafer.ExtensionMethods
{
    public static class DoubleExtensionMethod
    {
        const double degreeRatio = Math.PI / 180;

        public static double ToDegrees(this double value)
        {
            return value / degreeRatio;
        }

        public static double ToNumerical(this double value)
        {
            return value * degreeRatio;
        }
    }
}
