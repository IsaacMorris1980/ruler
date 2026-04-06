using System;
using System.Linq;
using System.ComponentModel;
using System.Reflection;
namespace Ruler.Shared
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Reads the [Description] attribute of an Enum value.
        /// Falls back to .ToString() if no description is found.
        /// </summary>
        public static string GetDescription(this Enum value)
        {
            // 1. Get the type of the Enum
            Type type = value.GetType();
            // 2. Get the specific field name (the name of the enum member)
            string name = Enum.GetName(type, value);
            if (name == null) return value.ToString();
            // 3. Use Reflection to get the FieldInfo for that name
            FieldInfo field = type.GetField(name);
            if (field == null) return name;
            // 4. Look for the DescriptionAttribute on that field
            var attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false)
                                .Cast<DescriptionAttribute>()
                                .FirstOrDefault();
            // 5. Return the description if it exists, otherwise the name
            return attribute != null ? attribute.Description : name;
        }
    }
}
