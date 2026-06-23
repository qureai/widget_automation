using APIClient.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace APIClient.Common.Extensions
{
    public static class DataGridViewExtensions
	{

		public static bool ExportEventLogs(this DataGridView dgv)
		{
			bool success = false;

			if (dgv.Rows.Count == 0)
			{
				MessageBox.Show("No event logs to export.", "Info");
				return false;
			}

			string fileName = Assembly.GetEntryAssembly().GetName().Name + ".EventLog." + DateTime.Now.ToString("MMddyyyyHHmmss");

			var saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "Text files (*.txt)|*.txt|Csv files (*.csv)|*.csv|All files (*.*)|*.*";
			saveFileDialog.FilterIndex = 1;
			saveFileDialog.RestoreDirectory = false;
			saveFileDialog.InitialDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
			saveFileDialog.FileName = fileName + ".txt";

			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				try
				{
					if (saveFileDialog.FileName.EndsWith("csv", StringComparison.OrdinalIgnoreCase))
					{
						success = dgv.ExportTo_Csv(saveFileDialog.FileName, ',');
					}
					else if (saveFileDialog.FileName.EndsWith("txt", StringComparison.OrdinalIgnoreCase))
					{
						success = dgv.ExportTo_Txt(saveFileDialog.FileName);
					}
					else
					{
						MessageBox.Show($"Invalid file name and extension {saveFileDialog.FileName}.", "Error");
						return false;
					}

					if (success)
					{
						MessageBox.Show($"Successfully exported event logs to file {saveFileDialog.FileName}.", "Info");
					}
					else
					{
						MessageBox.Show($"Failed to export event logs to file {saveFileDialog.FileName}.", "Error");
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Exception occured while exporting event logs. {ex.Message}.");
				}
			}

			return success;
		}

		public static bool ExportTo_Txt(this DataGridView dgv, string fileName, int padding = 5)
		{
			if ((dgv.Rows.Count == 0) || (dgv.Columns.Count == 0) || string.IsNullOrWhiteSpace(fileName))
			{
				return false;
			}

			// Convert negative padding to positive
			padding = Math.Abs(padding);

			string text;

			// Max Character Lengths of each column
			int[] maxLengths = new int[dgv.Columns.Count];
			for (int j  = 0; j < dgv.Columns.Count; j++)
			{
				text = dgv.Columns[j].HeaderText ?? string.Empty;
				maxLengths[j] = text.Length;
				foreach (DataGridViewRow row in dgv.Rows)
				{
					text = row.Cells[j].Value?.ToString() ?? string.Empty;
					int length = text.Length;
					if (length > maxLengths[j])
					{
						maxLengths[j] = length;
					}
				}
			}

			using (StreamWriter sw = new StreamWriter(fileName, false))
			{
				List<int> columnIndexes = new List<int>();
				// Saving Column Headers
				for (int i = 0; i < dgv.Columns.Count; i++)
				{
					text  = dgv.Columns[i].HeaderText ?? string.Empty;
					if (string.IsNullOrWhiteSpace(text))
					{
						continue;
					}
					sw.Write(text.PadRight(maxLengths[i] + padding));
					columnIndexes.Add(i);
				}
				sw.WriteLine();

				// Saving Rows
				for (int i = 0; i < dgv.Rows.Count; i++)
				{
					foreach (int j in columnIndexes)
					{
						text = dgv.Rows[i].Cells[j].Value?.ToString() ?? string.Empty;
						sw.Write(text.PadRight(maxLengths[j] + padding));
					}
					sw.WriteLine();
				}

				sw.Close();
			}

			return true;
		}

		public static bool ExportTo_Csv(this DataGridView dgv, string fileName, char separator)
		{
			if ((dgv.Rows.Count == 0) || (dgv.Columns.Count == 0) || string.IsNullOrWhiteSpace(fileName))
			{
				return false;
			}
		
			string[] outputCsv = new string[dgv.Rows.Count + 1];

			// Saving Column Headers
			string columnNames = "";
			string text;
			List<int> columnIndexes = new List<int>();
			for (int j = 0; j < dgv.Columns.Count; j++)
			{
				text = dgv.Columns[j].HeaderText;
				if (string.IsNullOrWhiteSpace(text))
				{
					continue;
				}
				columnNames += text;
				columnIndexes.Add(j);
				if (j < dgv.Columns.Count - 1)
				{
					columnNames += separator;
				}
			}
			columnNames = columnNames.TrimEnd(separator);
			outputCsv[0] += columnNames;

			// Saving Rows
			for (int i = 0; i < dgv.Rows.Count; i++)
			{
				foreach (int j in columnIndexes)
				{
					outputCsv[i + 1] += dgv.Rows[i].Cells[j].Value?.ToString() ?? string.Empty;
					if (j < columnIndexes.Count - 1)
					{
						outputCsv[i + 1] += separator;
					}
				}
			}
			
			File.WriteAllLines(fileName, outputCsv, Encoding.UTF8);

			return true;
		}

        public static void AddEventLog(this DataGridView dgv, EventLog eventLog)
        {
            var eventLogString = string.Empty;
            var dgvr = new DataGridViewRow();
            if (!string.IsNullOrWhiteSpace(eventLog?.ReportParams?.PlainText)
                || !string.IsNullOrWhiteSpace(eventLog?.ReportParams?.RichText))
            {
                dgvr.HeaderCell.Value = "R";
                eventLogString = JsonConvert.SerializeObject(eventLog);
            }
			dgvr.Tag = eventLog;
            dgvr.CreateCells(dgv, new string[] { eventLog.Time, eventLog.EventType.ToString(), eventLog.Message, eventLogString });

			if (dgv.Columns["Time"].HeaderCell.SortGlyphDirection == SortOrder.Descending)
			{
                dgv.Rows.Insert(0, dgvr);
            }
			else
			{
                dgv.Rows.Add(dgvr);
            }
        }
    }
}