using System.Drawing;

namespace APIClient.Common.Extensions
{
    internal static class ColorExtensions
    {
        public static Color HexToSystemColor(this string hex) => ColorTranslator.FromHtml(hex);
    }
}
