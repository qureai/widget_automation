using APIClient.Common.Extensions;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace APIClient.Common.CustomControls.ToggleSwitch
{
    public class ToggleSwtich : Control
    {
        private float diameter;
        private ToggleSwitchRectangle rectangle;
        private RectangleF circle;
        private float artis;
        private bool isOn;
        private bool textEnabled;
        private string onText = "";
        private string offText = "";
        private Color borderColor;
        private Color onColor;
        private Color offColor;
        private Timer ticker = new Timer();

        public event ToggleChangedEventHandler ToggleChanged;
        public delegate void ToggleChangedEventHandler(object sender, EventArgs e);

        public ToggleSwtich()
        {
            Cursor = Cursors.Hand;
            DoubleBuffered = true;
            artis = 4f;
            diameter = 30f;
            textEnabled = true;
            rectangle = new ToggleSwitchRectangle(2f * diameter, diameter + 2f, diameter / 2f, 1f, 1f);
            circle = new RectangleF(1f, 1f, diameter, diameter);
            borderColor = Color.LightGray;
            ticker.Tick += new EventHandler(ticker_Tick);
            ticker.Interval = 1;
            onColor = Color.FromArgb(94, 148, 255);
            offColor = Color.DarkGray;
            ForeColor = Color.White;
            onText = "On";
            offText = "Off";
        }

        public bool IsOn
        {
            get => isOn;
            set
            {
                ticker?.Stop();
                isOn = value;
                ticker?.Start();
                if (ToggleChanged != null)
                {
                    ToggleChanged(this, EventArgs.Empty);
                }
            }
        }

        public bool TextEnabled
        {
            get => textEnabled;
            set
            {
                textEnabled = value;
                Invalidate();
            }
        }

        public string OnText
        {
            get => onText;
            set
            {
                onText = value;
                Invalidate();
            }
        }

        public string OffText
        {
            get => offText;
            set
            {
                offText = value;
                Invalidate();
            }
        }

        public Color BorderColor
        {
            get => borderColor;
            set
            {
                borderColor = value;
                Invalidate();
            }
        }

        public Color OnColor
        {
            get => onColor;
            set
            {
                onColor = value;
                Invalidate();
            }
        }

        public Color OffColor
        {
            get => offColor;
            set
            {
                offColor = value;
                Invalidate();
            }
        }

        protected override Size DefaultSize => new Size(60, 35);

        protected override void OnEnabledChanged(EventArgs e)
        {
            Invalidate();
            base.OnEnabledChanged(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isOn = !isOn;
                IsOn = isOn;
                base.OnMouseClick(e);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            if (Enabled)
            {
                Pen pen;
                using (var brush = new SolidBrush(isOn ? onColor : offColor))
                {
                    e.Graphics.FillPath(brush, rectangle.Path);
                }
                using (pen = new Pen(borderColor, 2f))
                {
                    e.Graphics.DrawPath(pen, rectangle.Path);
                }
                if (textEnabled)
                {
                    //Microsoft Sans Serif
                    using (var font = new Font("Century Gothic", 8.2f * diameter / 30f, FontStyle.Bold))
                    {
                        var brush = new SolidBrush(ForeColor);
                        var height = TextRenderer.MeasureText(onText, font).Height;
                        var num = (diameter - height) / 2f;
                        e.Graphics.DrawString(onText, font, brush, 5f, num + 1f);
                        height = TextRenderer.MeasureText(offText, font).Height;
                        num = (diameter - height) / 2f;
                        e.Graphics.DrawString(offText, font, brush, diameter + 2f, num + 1f);
                    }
                    using (var brush = new SolidBrush("#FFFFFF".HexToSystemColor()))
                    {
                        e.Graphics.FillEllipse(brush, circle);
                    }
                    using (pen = new Pen(Color.LightGray, 1.2f))
                    {
                        e.Graphics.DrawEllipse(pen, circle);
                    }
                }
                else
                {
                    using (var brush = new SolidBrush("#FFFFFF".HexToSystemColor()))
                    {
                        using (var brushEllipse = new SolidBrush("#FFFFFF".HexToSystemColor()))
                        {
                            e.Graphics.FillPath(brush, rectangle.Path);
                            e.Graphics.FillEllipse(brushEllipse, circle);
                            e.Graphics.DrawEllipse(Pens.DarkGray, circle);
                        }
                    }
                }
            }
            base.OnPaint(e);
        }

        protected override void OnResize(EventArgs e)
        {
            Width = (Height - 2) * 2;
            diameter = Width / 2;
            artis = 4f * diameter * 30f;
            rectangle = new ToggleSwitchRectangle(2f * diameter, diameter + 2f, diameter / 2f, 1f, 1f);
            circle = new RectangleF(!isOn ? 1f : (Width - diameter - 1f), 1f, diameter, diameter);
            base.OnResize(e);
        }

        private void ticker_Tick(object sender, EventArgs e)
        {
            var x = circle.X;
            if (isOn)
            {
                if ((x + artis) <= (Width - diameter - 1f))
                {
                    x += artis;
                    circle = new RectangleF(x, 1f, diameter, diameter);
                    base.Invalidate();
                }
                else
                {
                    x = Width - diameter - 1f;
                    circle = new RectangleF(x, 1f, diameter, diameter);
                    base.Invalidate();
                    ticker.Stop();
                }
            }
            else if ((x - artis) >= 1f)
            {
                x -= artis;
                circle = new RectangleF(x, 1f, diameter, diameter);
            }
            else
            {
                x = 1f;
                circle = new RectangleF(x, 1f, diameter, diameter);
                base.Invalidate();
                ticker.Stop();
            }
        }
    }
}
