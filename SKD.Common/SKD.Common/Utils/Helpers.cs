using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace SKD.Common.Utils
{
    public static class Helpers
    {
        public static double Lerp(double a, double b, double t) => (a * (1 - t)) + (b * t);
        public static Point Lerp(Point a, Point b, double t) => new Point(Lerp(a.X, b.X, t), Lerp(a.Y, b.Y, t));
        public static Color Lerp(Color a, Color b, double t) => new Color(Lerp(a.R, b.R, t), Lerp(a.G, b.G, t), Lerp(a.B, b.B, t), Lerp(a.A, b.A, t));
    }
}
