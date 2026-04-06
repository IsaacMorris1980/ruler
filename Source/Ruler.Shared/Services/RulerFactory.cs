using System;
namespace Ruler.Shared
{
    /// <summary>
    /// Centralized factory for creating and manipulating RulerInfo objects.
    /// Handles default state, deep copying, and command-line parsing.
    /// </summary>
    public class RulerFactory
    {
            /// <summary>
            /// Creates a RulerInfo object with standard application defaults.
            /// </summary>
            public static RulerInfo CreateDefault()
            {
                return new RulerInfo
                {
                    Width = 400,
                    Height = 75,
                    Opacity = 0.6,
                    ShowToolTip = true,
                    IsLocked = false,
                    TopMost = true,
                    IsVertical = false,
                    SaveType = SaveTypes.all
                };
            }
            /// <summary>
            /// Maps properties from a source RulerInfo to a destination RulerInfo.
            /// Useful for "stripping" data during save or cloning objects.
            /// </summary>
            public static void CopyValues(RulerInfo source, RulerInfo target)
            {
                if (source == null || target == null) return;

                target.Width = source.Width;
                target.Height = source.Height;
                target.Left = source.Left;
                target.Top = source.Top;
                target.Opacity = source.Opacity;
                target.IsVertical = source.IsVertical;
                target.IsLocked = source.IsLocked;
                target.TopMost = source.TopMost;
                target.ShowToolTip = source.ShowToolTip;
                target.SaveType = source.SaveType;
            }
            /// <summary>
            /// Parses an array of string arguments (from CLI) into a RulerInfo object.
            /// </summary>
            public static RulerInfo CreateFromArgs(string[] args)
            {
                // Start with defaults so if args are missing, we have a valid object
                var info = CreateDefault();

                if (args == null || args.Length == 0) return info;

                // Mapping: 0:W, 1:H, 2:Vertical, 3:Opacity, 4:Tooltip, 5:Locked, 6:Topmost, 7:Left, 8:Top, 9:SaveType
                if (args.Length > 0) info.Width = ParseInt(args[0], (int)info.Width);
                if (args.Length > 1) info.Height = ParseInt(args[1], (int)info.Height);
                if (args.Length > 2) info.IsVertical = ParseBool(args[2], info.IsVertical);
                if (args.Length > 3) info.Opacity = ParseDouble(args[3], info.Opacity);
                if (args.Length > 4) info.ShowToolTip = ParseBool(args[4], info.ShowToolTip);
                if (args.Length > 5) info.IsLocked = ParseBool(args[5], info.IsLocked);
                if (args.Length > 6) info.TopMost = ParseBool(args[6], info.TopMost);
                if (args.Length > 7) info.Left = ParseDouble(args[7], info.Left);
                if (args.Length > 8) info.Top = ParseDouble(args[8], info.Top);

                if (args.Length > 9 && Enum.TryParse<SaveTypes>(args[9], true, out var saveType))
                {
                    info.SaveType = saveType;
                }

                return info;
            }
            #region Private Parsing Helpers
            private static int ParseInt(string value, int defaultValue)
            {
                return int.TryParse(value, out int result) ? result : defaultValue;
            }
            private static double ParseDouble(string value, double defaultValue)
            {
                return double.TryParse(value, out double result) ? result : defaultValue;
            }
            private static bool ParseBool(string value, bool defaultValue)
            {
                return bool.TryParse(value, out bool result) ? result : defaultValue;
            }
        #endregion
        public static string ConvertToParameters(RulerInfo rulerInfo)
        {
            return string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}", rulerInfo.Width, rulerInfo.Height, rulerInfo.IsVertical, rulerInfo.Opacity, rulerInfo.ShowToolTip, rulerInfo.IsLocked, rulerInfo.TopMost, rulerInfo.Left, rulerInfo.Top, rulerInfo.SaveType);
        }
    }
}
