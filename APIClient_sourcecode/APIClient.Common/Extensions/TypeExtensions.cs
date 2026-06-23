using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace APIClient.Common.Extensions
{
	public static class TypeExtensions
	{
		public static IEnumerable<EventInfo> GetApiEvents(this Type type)
		{
			return type.GetEvents(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				.OrderBy(e => e.Name);
		}

		public static IEnumerable<string> GetApiProperties(this Type type)
		{
			List<string> propertyNames = new List<string> { "-- Select API Property --" };
			propertyNames.AddRange(Enum.GetNames(type));
			return propertyNames;
		}

		public static IEnumerable<MethodInfo> GetApiMethodInfos(this Type type)
		{
			return type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				.Where(m => !m.Attributes.HasFlag(MethodAttributes.SpecialName))
				.OrderBy(m => m.Name);
		}

		#region Enum Extensions

		public static string GetDescription(this Enum enumValue)
		{
			string description = enumValue.ToString();

			var field = enumValue.GetType().GetField(enumValue.ToString());
			if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
			{
				description = attribute.Description;
			}
			return description;
		}

		#endregion

	}
}
