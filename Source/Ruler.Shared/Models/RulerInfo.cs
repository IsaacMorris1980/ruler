using System;
using System.Windows;
using System.Runtime.CompilerServices;


using Newtonsoft.Json;

using Ruler.Contracts.Models;
using Ruler.Contracts.Enums;


namespace Ruler.Shared.Models
{
    public class RulerInfo : ModelBase,IRulerInfo
    {
        [JsonProperty("Width")]
        public int Width
        {
            get;
            set;
        }
        [JsonProperty("Height")]
        public int Height
        {
            get;
            set;
        }

        /// <summary>
        /// TODO
        /// </summary>
        [JsonProperty("IsVertical")]
        public bool IsVertical
        {
            get;
            set;
        }
        
        [JsonProperty("Opacity")]
        public double Opacity
        {
            get;
            set;
        }

        /// <summary>
        /// TODO
        /// </summary>
        [JsonProperty("ShowToolTip")]
        public bool ShowToolTip
        {
            get;
            set;
        }

        /// <summary>
        /// TODO
        /// </summary>
        [JsonProperty("IsLocked")]
        public bool IsLocked
        {
            get;
            set;
        }

        [JsonProperty("TopMost")]
        public bool TopMost
        {
            get;
            set;
        }
        [JsonProperty("Top")]
        public int Top
        {
            get;
            set;
        }
        [JsonProperty("Left")]
        public int Left
        {    get;
            set;
        }
        [JsonProperty("SaveType")]
        public SaveTypes SaveType
        {
            get;
            set;
        }
        // The property the Serializer uses
        [JsonProperty("DisplayLocation")]
        public string DisplayLocationString
        {
            get => $"{Left},{Top}"; 
            set
            {
                var parts = value.Split(',');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out int x) &&
                    int.TryParse(parts[1], out int y))
                {
                    Left = x;
                    Top = y;
                }
            }
        }
        [JsonIgnore]
      public RulerGuideline Guideline
        {
            get;
            set;
        } = new RulerGuideline();
        public bool IsGuidelineEnabled
        {
            get;
            set;
        }
        public double GuidelineLocation { get; set; }

        public override string ToString()
        {
            return $"[RulerInfo Details]" + Environment.NewLine +
                   $"  IsVertical: {IsVertical}" + Environment.NewLine +
                   $"  Size: {Width}x{Height}" + Environment.NewLine +
                   $"  Location: {Left},{Top} (Display: {DisplayLocationString})" + Environment.NewLine +
                   $"  Opacity: {Opacity}" + Environment.NewLine +
                   $"  TopMost: {TopMost} | ShowToolTip: {ShowToolTip}" + Environment.NewLine +
                   $"  IsLocked: {IsLocked}" + Environment.NewLine +
                   $"  Guideline: {Guideline.ToString()}" + Environment.NewLine +
                   $"  SaveType: {SaveType}";
        }
        public void ToggleOrientation()
        {
            IsVertical = !IsVertical;

            // Swap dimensions
            var temp = Width;
            Width = Height;
            Height = temp;
        }
    }

}