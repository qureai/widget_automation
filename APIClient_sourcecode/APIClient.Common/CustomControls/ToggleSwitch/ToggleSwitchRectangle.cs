using System.Drawing;
using System.Drawing.Drawing2D;

namespace APIClient.Common.CustomControls.ToggleSwitch
{
    internal class ToggleSwitchRectangle
    {
        private GraphicsPath graphicsPath;
        private float radius;
        private readonly float width;
        private readonly float height;
        private readonly float x;
        private readonly float y;

        public GraphicsPath Path => graphicsPath;

        public RectangleF Rect => new RectangleF(x, y, width, height);

        public float Radius
        {
            get => radius;
            set => radius = value;
        }

        public ToggleSwitchRectangle() { }

        public ToggleSwitchRectangle(float width, float height, float radius, float x = 0f, float y = 0f)
        {
            graphicsPath = new GraphicsPath();
            this.radius = radius;
            this.width = width;
            this.height = height;
            this.x = x;
            this.y = y;

            if (radius <= 0f)
            {
                graphicsPath.AddRectangle(new RectangleF(x, y, width, height));
            }
            else
            {
                graphicsPath.AddArc(new RectangleF(x, y, 2f * radius, 2f * radius), 180f, 90f);
                graphicsPath.AddArc(new RectangleF(width - (2f * radius) - 1f, x, 2f * radius, 2f * radius), 270f, 90f);
                graphicsPath.AddArc(new RectangleF(width - (2f * radius) - 1f, height - (2f * radius) - 1f, 2f * radius, 2f * radius), 0f, 90f);
                graphicsPath.AddArc(new RectangleF(x, height - (2f * radius) - 1f, 2f * radius, 2f * radius), 90f, 90f);
                graphicsPath.CloseAllFigures();
            }
        }
    }
}
