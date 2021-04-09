using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Shapes;
using static SKD.Common.Utils.Helpers;

namespace SKD.Common.Utils
{
    public class DoubleToDouble_MultiplyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var factor = (double)parameter;
            var val = (double)value;
            return val * factor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var factor = (double)parameter;
            var val = (double)value;
            return val / factor;
        }
    }

    public class IntToString_CurrencyInputConverterGBP : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (int)value;
            return val > 0 ? $"₤{val / 100}" : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (string)value;
            if (int.TryParse(val?.Replace("₤", string.Empty), out int result))
                return result * 100;
            return string.IsNullOrEmpty(val?.Replace("₤", string.Empty)) ? -2 : -1;
        }
    }

    public class IntToString_CurrencyConverterGBP : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double coeff = parameter is double d ? d : 1;
            var val = (int)value;
            var ret = (coeff * val / 100d);
            return ret.ToString(ret % 1 == 0 ? "0" : "0.00");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double coeff = parameter is double d ? d : 1;
            var val = (string)value;
            if (double.TryParse(val, out double result))
                return (int)(result * 100 / coeff);
            return -1;
        }
    }

    public class IntToString_NumberInputConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (int)value;
            return val > 0 ? val.ToString() : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (string)value;
            if (int.TryParse(val, out int result))
                return result;
            return string.IsNullOrEmpty(val) ? -2 : -1;
        }
    }

    public class IntToBool_GreaterThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return ((value is int i) ? i : (double)value) > ((parameter is int j) ? j : (double)parameter);
            }
            catch { return value != null; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? 1d : 0d;
        }
    }

    public class DoubleToDouble_AddConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value + (double)parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value - (double)parameter;
        }
    }

    public class DoubleToDouble_SignInLogoRotationConverter : BindableObject, IValueConverter
    {
        public static readonly BindableProperty PageHeightProperty = BindableProperty.Create(nameof(PageHeight), typeof(double), typeof(DoubleToDouble_SignInLogoRotationConverter), propertyChanged: OnPageHeightChanged);

        private static void OnPageHeightChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var converter = bindable as DoubleToDouble_SignInLogoRotationConverter;
            converter!.factor = 360d / ((double)newValue * -0.35d);
        }

        public double PageHeight { get => (double)GetValue(PageHeightProperty); set => SetValue(PageHeightProperty, value); }
        private double factor;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (double)value;
            return val < 0 ? val * factor : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value / factor;
        }
    }

    public class ANDConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = true;
            foreach (var o in values)
                if (o is bool b)
                    result &= b;
            return result;
        }

        public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return default;
        }
    }

    public class ORConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = false;
            foreach (var o in values)
                if (o is bool b)
                    result |= b;
            return result;
        }

        public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return default;
        }
    }

    public class NOTConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is bool b && !b;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is bool b && !b;

    }

    public class ProductConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Any() ?? false)
            {
                if (values[0] is int)
                    return values.Select(x => asInt(x)).Product() * asInt(parameter);
                if (values[0] is double)
                    return values.Select(x => asDouble(x)).Product() * asDouble(parameter);
            }
            return default;

            static int asInt(object o) => o switch
            {
                "" => 0,
                int i => i,
                string s => int.Parse(s),
                _ => 1
            };

            static double asDouble(object? o) => o switch
            {
                "" => 0d,
                double d => d,
                string s => double.Parse(s, CultureInfo.InvariantCulture),
                _ => 1d
            };
        }

        public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return default;
        }
    }

    public class EqualityConverter : IValueConverter
    {
        public object? TrueValue { get; set; }
        public object? FalseValue { get; set; }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == parameter ? TrueValue : FalseValue;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == TrueValue ? parameter : null;
        }
    }

    public class TernaryConverter : IValueConverter
    {
        public object? TrueValue { get; set; }
        public object? FalseValue { get; set; }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is true ? TrueValue : FalseValue;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == TrueValue;
        }
    }

    public class AccentColourGradientConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is Color c)
                {
                    return new RadialGradientBrush(new GradientStopCollection()
                    {
                        new GradientStop(c, 0),
                        new GradientStop(c.AddLuminosity(-0.2d), 1)
                    },
                        new Point(0, 0), 1);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RadialGradientBrush b)
                return b.GradientStops.FirstOrDefault()?.Color ?? Color.Default;
            return Color.Default;
        }
    }

    public abstract class MorphConverter : IValueConverter
    {
        public abstract object? Convert(object value, Type targetType, object parameter, CultureInfo culture);
        public abstract object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);

        public double MinT { get; set; }
        public double MaxT { get; set; }

        protected double MapT(object value)
        {
            double rangeT = MaxT - MinT;
            double midT = (MinT + MaxT) / 2d;
            return ((double)value - midT) / rangeT + 0.5d;
        }
    }

    public class PathMorphStop
    {
        public string Data { get; set; } = string.Empty;
        public double Offset { get; set; }
    }

    public class PathMorphStopCollection : Collection<PathMorphStop> { }

    [ContentProperty(nameof(Stops))]
    public class PathMorphConverter : MorphConverter
    {
        private PathMorphStopCollection? _stops;
        private PointFrame[]? pointFrames;
        private IEnumerable<string>? format;

        public PathMorphStopCollection Stops
        {
            get => _stops ?? new PathMorphStopCollection(); set
            {
                _stops = value;
                pointFrames = value.OrderBy(x => x.Offset).Select(x => new PointFrame(GetPoints(x.Data), x.Offset)).ToArray();
                format = value[0].Data.Split(' ').Select(x => x.Contains(",") ? "?" : x);
            }
        }

        private static IEnumerable<Point> GetPoints(string pathData)
            => pathData.Split(' ').Where(x => x.Contains(",")).Select(x =>
            {
                var coords = x.Split(',');
                return new Point(double.Parse(coords[0], CultureInfo.InvariantCulture), double.Parse(coords[1], CultureInfo.InvariantCulture));
            });

        private static readonly PathGeometryConverter geometryConverter = new PathGeometryConverter();

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double t = MapT(value);
            IEnumerable<Point>? pointsA = default, pointsB = default;
            double tA = default, tB = default;
            for (int i = 1; i < pointFrames!.Length; i++)
            {
                if (pointFrames[i].Offset >= t)
                {
                    var frameA = pointFrames[i - 1];
                    (pointsA, tA) = (frameA.Points, frameA.Offset);
                    var frameB = pointFrames[i];
                    (pointsB, tB) = (frameB.Points, frameB.Offset);
                    break;
                }
            }
            var newPoints = pointsA.Zip(pointsB, (a, b) => Lerp(a, b, (t - tA) / (tB - tA))).ToArray();
            StringBuilder result = new StringBuilder();
            int pointIndex = 0;
            foreach (string s in format!)
            {
                if (s == "?")
                {
                    result.Append(newPoints[pointIndex].GetCoordString());
                    pointIndex++;
                }
                else
                    result.Append(s);
                result.Append(' ');
            }
            return geometryConverter.ConvertFromInvariantString(result.ToString().Trim());
        }

        private class PointFrame
        {
            public PointFrame(IEnumerable<Point> points, double offset)
                => (Points, Offset) = (points, offset);
            public IEnumerable<Point> Points { get; }
            public double Offset { get; }
        }

        public override object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return default;
        }
    }

    [ContentProperty(nameof(GradientStops))]
    public class ColourMorphConverter : MorphConverter
    {
        public GradientStopCollection? GradientStops { get; set; }

        public override object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double t = MapT(value);
            Color colourA = default, colourB = default;
            double tA = default, tB = default;
            var sortedStops = GradientStops.OrderBy(x => x.Offset).ToArray();
            for (int i = 1; i < GradientStops!.Count; i++)
            {
                if (sortedStops[i].Offset >= t)
                {
                    var stopA = sortedStops[i - 1];
                    (colourA, tA) = (stopA.Color, stopA.Offset);
                    var stopB = sortedStops[i];
                    (colourB, tB) = (stopB.Color, stopB.Offset);
                    break;
                }
            }
            var colour = Lerp(colourA, colourB, (t - tA) / (tB - tA));
            if (targetType.IsAssignableFrom(typeof(Color)))
                return colour;
            if (targetType.IsAssignableFrom(typeof(SolidColorBrush)))
                return new SolidColorBrush(colour);
            return default;
        }

        public override object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return default;
        }
    }
}
