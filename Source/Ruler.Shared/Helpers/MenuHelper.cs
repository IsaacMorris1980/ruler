using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ruler.Shared.Helpers
{
    public static class MenuHelper
    {
        public static ObservableCollection<MenuItemModel> CreateRangeMenuItems(
       double start,
       double end,
       double step,
       Func<double, string> labelSelector, // Replaces formatString with a custom rule
       Func<double, object> valueSelector,
       ICommand command)
        {
            var items = new ObservableCollection<MenuItemModel>();

            for (double i = start; i <= end + 0.0001; i += step)
            {
                items.Add(new MenuItemModel()
                {
                    Header = labelSelector(i), // Evaluates the lambda for each number
                    Value = valueSelector(i),
                    Command = command,
                    IsCheckable = true
                });
            }

            return items;
        }
        public static ObservableCollection<MenuItemModel> CreateListMenuItems<T>(
    IEnumerable<T> values,
    Func<T, string> labelSelector,
    ICommand command)
        {
            var items = new ObservableCollection<MenuItemModel>();

            foreach (var val in values)
            {
                items.Add(new MenuItemModel()
                {
                    Header = labelSelector(val),
                    Value = val,
                    Command = command,
                    IsCheckable = true
                });
            }

            return items;
        }
        public static void UpdateMenuChecks(IEnumerable<MenuItemModel> menuItems, object currentValue, double tolerance = 0.001)
        {
            if (menuItems == null || currentValue == null) return;

            foreach (var item in menuItems)
            {
                // Recursively check nested submenus (e.g., Opacity -> sub-items)
                if (item.Items != null && item.Items.Count > 0)
                {
                    UpdateMenuChecks(item.Items, currentValue, tolerance);
                }

                // Evaluate check state if a CommandParameter exists
                if (item.Value != null)
                {
                    item.IsChecked = AreValuesEqual(item.Value, currentValue, tolerance);
                }
                else
                {
                    item.IsChecked = false;
                }
            }
        }

        private static bool AreValuesEqual(object param, object current, double tolerance)
        {
            // 1. Direct equality check (handles strings, enums, booleans, identical types)
            if (param.Equals(current))
                return true;

            // 2. Universal numeric comparison (handles int, double, float, decimal cross-comparisons)
            if (IsNumeric(param) && IsNumeric(current))
            {
                try
                {
                    double dParam = Convert.ToDouble(param);
                    double dCurrent = Convert.ToDouble(current);
                    return Math.Abs(dParam - dCurrent) < tolerance;
                }
                catch
                {
                    // Fallback if casting encounters an unexpected edge case
                }
            }

            return false;
        }
        public static bool TryExtractValue<T>(object parameter, out T result)
        {
            result = default;
            if (parameter == null) return false;

            // 1. Unwrap if it's wrapped inside a MenuItemModel
            object rawValue = parameter is MenuItemModel model ? model.Value : parameter;

            // 2. Direct match (Works automatically for Enums like SaveTypes, exact classes, structs, etc.)
            if (rawValue is T directVal)
            {
                result = directVal;
                return true;
            }

            // 3. Safe string parsing conversions
            if (rawValue is string strVal)
            {
                strVal = strVal.Trim();

                // Handle Enums if T is an Enum type (e.g., SaveTypes)
                if (typeof(T).IsEnum)
                {
                    try
                    {
                        result = (T)Enum.Parse(typeof(T), strVal, true);
                        return true;
                    }
                    catch { return false; }
                }

                // Handle standard numeric parsing without unwanted division
                if (typeof(T) == typeof(double) && double.TryParse(strVal, out double dParsed))
                {
                    result = (T)(object)dParsed;
                    return true;
                }
                if (typeof(T) == typeof(int) && int.TryParse(strVal, out int iParsed))
                {
                    result = (T)(object)iParsed;
                    return true;
                }
            }

            return false;
        }
        public static bool TryGetRelativeOpacityDelta(object parameter, out double relativeDelta)
        {
            relativeDelta = 0;
            // Unwrap the value if it came from a MenuItemModel
            object raw = parameter is MenuItemModel model ? model.Value : parameter;

            if (raw is string strVal)
            {
                strVal = strVal.Trim();
                // Check for relative steps like "+5" or "-5"
                if ((strVal.StartsWith("+") || strVal.StartsWith("-")) && double.TryParse(strVal, out double delta))
                {
                    relativeDelta = delta / 100.0; // Converts -5 to -0.05
                    return true;
                }
            }
            return false;
        }

        public static bool TryExtractOpacityValue(object parameter, out double opacityValue)
        {
            opacityValue = 0;
            // Unwrap the value if it came from a MenuItemModel
            object raw = parameter is MenuItemModel model ? model.Value : parameter;

            if (raw == null) return false;

            // Pattern match using switch expression to clean up type checks
            switch (raw)
            {
                case double dVal:
                    opacityValue = dVal > 1.0 ? dVal / 100.0 : dVal;
                    return true;

                case int iVal:
                    opacityValue = iVal > 1.0 ? iVal / 100.0 : iVal;
                    return true;

                case string sVal:
                    string clean = sVal.Replace("%", "").Trim();
                    if (double.TryParse(clean, out double parsed))
                    {
                        opacityValue = parsed > 1.0 ? parsed / 100.0 : parsed;
                        return true;
                    }
                    break;
            }

            return false;
        }
        private static bool IsNumeric(object value)
        {
            if (value == null) return false;

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
                default:
                    return false;
            }
        }
    }

}

