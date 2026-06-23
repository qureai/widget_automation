using System.Windows.Forms;

namespace APIClient.Common.Extensions
{
    public static class ControlExtensions
	{
		public static void EnableWithUIStyle(this Button button, bool enabled)
		{
			if (enabled)
			{
				button.Invoke((MethodInvoker)delegate
				{
					button.Enabled = true;
					button.FlatStyle = FlatStyle.Flat;
					button.Font = new System.Drawing.Font(button.Font, System.Drawing.FontStyle.Bold);
				});
			}
			else
			{
				button.Invoke((MethodInvoker)delegate
				{
					button.Enabled = false;
					button.FlatStyle = FlatStyle.System;
					button.Font = new System.Drawing.Font(button.Font, System.Drawing.FontStyle.Regular);
				});
			}
		}
	}
}
