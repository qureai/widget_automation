using APIClient.Common.Enums;
using APIClient.Common.Extensions;
using APIClient.Common.Forms;
using APIClient.Common.Models;
using Newtonsoft.Json;
using RadWhereAxLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace APIClient.RadWhereAx
{
	public partial class RadWhereAxForm : BaseForm
	{
		public RadWhereCtrl _rwAx = null;
		private static IList<MethodInfo> radWhereAxMethodInfos = new List<MethodInfo>();
		private static List<EventInfo> radWhereAxEventInfos = new List<EventInfo>();
		private MethodInfo selectedApiMethod;
        private static readonly AutoResetEvent _terminateEvent = new AutoResetEvent(false);

        const int TERMINATE_MAXTIMEOUT = 2000; // 2 seconds in maximum

        #region Constructors

        public RadWhereAxForm()
		{
			InitializeComponent();
			this.Load += BaseForm_Load;
		}

		#endregion

		#region UI Events

		private void RadWhereAxForm_Load(object sender, EventArgs e)
		{
			toolTipSettings.SetToolTip(btnSettings, "Settings");
			SetRadWhereAxUIState();
			AddLogToClientDataGrid += RadWhereAxAddEventLog;
			this.Type.SortMode = DataGridViewColumnSortMode.NotSortable;
			this.Message.SortMode = DataGridViewColumnSortMode.NotSortable;
			this.Time.HeaderCell.SortGlyphDirection = SortOrder.Descending;
			ccbEventsFilter.Text = CCB_EVENTS_FILTER_DEFAULT_TEXT;
		}

		private void btnConnectToPSClient_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
            AddClientEventLog("Attempting to launch/connect to PowerScribe...", EventTypeEnum.APIRequest);
            
			Connect();
			//TTP 4086
			if (_retryConnect)
			{
				_retryConnect = false;
				Connect();
			}
		}

		private void btnTerminatePSClient_Click(object sender, EventArgs e)
		{
			try
			{
				Cursor.Current = Cursors.WaitCursor;

				if (_rwAx == null)
				{
					AddClientEventLog("Can not terminate PowerScribe Client, since it is not connected.", EventTypeEnum.Error);
					return;
				}

				AddClientEventLog("Termination Pending...", EventTypeEnum.APIRequest);
				var success = _rwAx.Terminate();
				_skipConnectException = true;   //TTP 4086
				if (success)
				{
                    AddClientEventLog($"Termination Succeeded.", EventTypeEnum.APIResponse);

                    // Disconnect after termination event is received and processed.
                    _terminateEvent.WaitOne(TimeSpan.FromMilliseconds(TERMINATE_MAXTIMEOUT));
                    RadWhereAxDisconnect(this, null);
				}
				else
				{
					AddClientEventLog($"Termination Failed.", EventTypeEnum.APIResponse);
				}
			}
			catch (Exception ex)
			{
				AddClientEventLog($"Error occurred while terminating PS Client. {ex}.", EventTypeEnum.Error);
			}
		}

		private void btnSettings_Click(object sender, EventArgs e)
		{
			try
			{
				var settingsDialog = new SettingsDialog(apiEvents);

				if (settingsDialog.ShowDialog() == DialogResult.OK)
				{
					apiEvents?.ForEach(s => s.IsSelected = false);

					settingsDialog.SelectedEvents?.ForEach(eventName =>
					{
						apiEvents.First(se => se.EventName == eventName).IsSelected = true;
					});

					SetupRadWhereAxEventHandlers(settingsDialog.SelectedEvents);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error occured while setting RadWhereCOM API events. {ex}", "Error");
			}
		}

		private void btnLogin_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				if (_rwAx == null)
				{
					AddClientEventLog("You must connect to PS client before Login.", EventTypeEnum.Error);
					return;
				}

				var pwd = (ModifierKeys == Keys.Shift) ? null : txtPassword.Text;
				AddClientEventLog("Login Pending...", EventTypeEnum.APIRequest);
				var success = _rwAx.LoginEx(txtUsername.Text, pwd);
				AddClientEventLog($"Login {(success ? "Succeeded" : "Failed")}.", EventTypeEnum.APIResponse);
			}
			catch (Exception ex)
			{
				AddClientEventLog($"Login Error: {ex}", EventTypeEnum.Error);
			}
		}

		private void btnLogout_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				if (_rwAx == null)
				{
					AddClientEventLog("Cannot logout from PowerScribe Client, since it is not connected.", EventTypeEnum.Error);
					return;
				}
				AddClientEventLog("Logout Pending...", EventTypeEnum.APIRequest);
				var success = _rwAx.LogoutEx();
				AddClientEventLog($"Logout {(success ? "Succeeded" : "Failed")}.", EventTypeEnum.APIResponse);
			}
			catch (Exception ex)
			{
				AddClientEventLog($"Logout Error: {ex}", EventTypeEnum.Error);
			}
		}

		private void btnLaunch_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
                AddClientEventLog("Attempting to launch/connect to PowerScribe...", EventTypeEnum.APIRequest);
                btnConnectToPSClient.EnableWithUIStyle(false);

				_rwAx = new RadWhereCtrl();
				RegisterRadWhereAxEvents();
                
				var pwd = (ModifierKeys == Keys.Shift) ? null : txtPassword.Text;
				_rwAx.Launch(txtUsername.Text, pwd, txtServer.Text);
                AddClientEventLog("PowerScribe Launched / Connected successfully.", EventTypeEnum.APIResponse);
            }
            catch (Exception ex)
			{
				AddClientEventLog($"Launch Error: {ex}", EventTypeEnum.Error);
			}
		}

		private void cmbApiProperty_SelectedIndexChanged(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			txtApiPropertyValue.Clear();
			var selectedProperty = cmbApiProperty.SelectedItem.ToString();

			txtApiPropertyValue.Text = string.Empty;

			if (selectedProperty == IRadWhereApiPropertiesEnum.AccessionNumbers.ToString())
			{
				txtApiPropertyValue.Text = _rwAx?.accessionNumbers != null ? string.Join(",", _rwAx.accessionNumbers) : string.Empty;
			}
			else if (selectedProperty == IRadWhereApiPropertiesEnum.Username.ToString())
			{
				txtApiPropertyValue.Text = _rwAx?.userName ?? string.Empty;
			}
			else if (selectedProperty == IRadWhereApiPropertiesEnum.SiteName.ToString())
			{
				txtApiPropertyValue.Text = _rwAx?.siteName ?? string.Empty;
			}
		}

		private void toggleMinimized_Click(object sender, EventArgs e)
		{
			if (_rwAx != null)
				_rwAx.Minimized = toggleMinimized.IsOn;
		}

		private void toggleAlwaysOnTop_Click(object sender, EventArgs e)
		{
			if (_rwAx != null)
				_rwAx.AlwaysOnTop = toggleAlwaysOnTop.IsOn;
		}

		private void toggleRestrictedSession_Click(object sender, EventArgs e)
		{
			if (_rwAx != null)
				_rwAx.RestrictedSession = toggleRestrictedSession.IsOn;
		}

		private void toggleRestrictedWorkflow_Click(object sender, EventArgs e)
		{
			if (_rwAx != null)
				_rwAx.RestrictedWorkflow = toggleRestrictedWorkflow.IsOn;
		}

		private void toggleShowEventsOnly_Click(object sender, EventArgs e)
		{
			toggleShowEventsOnly.Enabled = false;
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				dgvEventLogging.Rows.Clear();

				foreach (var eventLog in eventLogs)
				{
					if (SkipLoggingEventToGrid(eventLog))
						continue;

					dgvEventLogging.AddEventLog(eventLog);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Exception occured while filtering events log grid. {ex}.", "Error");
			}
			finally
			{
				toggleShowEventsOnly.Enabled = true;
			}
		}

		private void cmbApiMethod_SelectedIndexChanged(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			pnlParameters.Controls.Clear();

			if (string.IsNullOrWhiteSpace(cmbApiMethod.SelectedItem.ToString()) || cmbApiMethod.SelectedItem.ToString() == CMB_API_METHOD_DEFAULT_VALUE)
			{
				btnExecute.EnableWithUIStyle(false);
			}
			else
			{
				selectedApiMethod = radWhereAxMethodInfos.FirstOrDefault(m => m.Name == cmbApiMethod.SelectedItem.ToString());
				if (selectedApiMethod != null)
				{
					var xAxis = 5;
					var yAxis = 5;
					var parameters = selectedApiMethod.GetParameters();
					if (parameters != null && parameters.Any())
					{
						var textInfo = new CultureInfo("en-US", false).TextInfo;
						foreach (var param in parameters)
						{
							var paramName = textInfo.ToTitleCase(param.Name);
							if (param.ParameterType == typeof(bool))
							{
								pnlParameters.Controls.Add(new CheckBox { Name = $"cbx{param.Name}Param", Text = paramName, Location = new Point(xAxis, yAxis), Width = Convert.ToInt32(pnlParameters.Width * 0.9), ForeColor = Color.Black });
								yAxis = yAxis + 20;
							}
							else
							{
								pnlParameters.Controls.Add(new Label { Name = $"lbl{param.Name}Param", Text = paramName, Location = new Point(xAxis, yAxis), Margin = new Padding(0), Height = 15, Width = Convert.ToInt32(pnlParameters.Width * 0.9), ForeColor = Color.Black });
								var txtBox = new TextBox { Name = $"txt{param.Name}Param", Location = new Point(xAxis, yAxis + 15), Width = Convert.ToInt32(pnlParameters.Width * 0.9) };
								if (param.Name.ToLower() == "password")
									txtBox.PasswordChar = '*';
								pnlParameters.Controls.Add(txtBox);
								yAxis = yAxis + 40;
							}
						}
					}
					else
					{
						pnlParameters.Controls.Add(new Label { Name = $"lblNoParams", Text = "No parameters required.", Location = new Point(xAxis, yAxis), Width = Convert.ToInt32(pnlParameters.Width * 0.9) });
					}
					btnExecute.EnableWithUIStyle(true);
				}
				else
				{
					AddClientEventLog($"API method {cmbApiMethod.SelectedItem} is not valid.", EventTypeEnum.Error);
				}
			}
		}

		private void btnExecute_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				var parameters = selectedApiMethod.GetParameters();
				var paramValues = GetParamValues(parameters, pnlParameters);

				switch (selectedApiMethod.Name)
				{
					case nameof(_rwAx.LoginEx):
						LoginEx(paramValues);
						break;
					case nameof(_rwAx.Login):
						Login(paramValues);
						break;
					case nameof(_rwAx.LogoutEx):
						btnLogout_Click(sender, e);
						break;
					case nameof(_rwAx.Logout):
						Logout();
						break;
					case nameof(_rwAx.Stop):
						Stop();
						break;
					case nameof(_rwAx.StartListening):
						StartListening();
						break;
					case nameof(_rwAx.StopListening):
						StopListening();
						break;
					case nameof(_rwAx.Terminate):
						btnTerminatePSClient_Click(sender, e);
						break;
					case nameof(_rwAx.OpenReport):
						OpenReport(paramValues);
						break;
					case nameof(_rwAx.OpenReportEx):
						OpenReportEx(paramValues);
						break;
					case nameof(_rwAx.CloseReport):
						CloseReport(paramValues);
						break;
					case nameof(_rwAx.CancelReport):
						CancelReport(paramValues);
						break;
					case nameof(_rwAx.SaveReport):
						SaveReport(paramValues);
						break;
					case nameof(_rwAx.AssociateOrders):
						AssociateOrders(paramValues);
						break;
					case nameof(_rwAx.AssociateOrdersEx):
						AssociateOrdersEx(paramValues);
						break;
					case nameof(_rwAx.DissociateOrders):
						DissociateOrders(paramValues);
						break;
					case nameof(_rwAx.PreviewOrders):
						PreviewOrders(paramValues);
						break;
					case nameof(_rwAx.InsertAutoText):
						InsertAutoText(paramValues);
						break;
					case nameof(_rwAx.CreateNewReport):
						CreateNewReport(paramValues);
						break;
					default:
						break;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Exception occured while executing API method '{selectedApiMethod?.Name}'. {ex}.", "Error");
			}
		}

		private void ccbEventsFilter_DropDownClosed(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				ccbEventsFilter.Text = ccbEventsFilter.CheckedItems.Count > 0 ? CCB_EVENTS_FILTER_CHECKED_TEXT : CCB_EVENTS_FILTER_DEFAULT_TEXT;

				if (ccbEventsFilter.ValueChanged)
				{
					dgvEventLogging.Rows.Clear();

					foreach (var eventLog in eventLogs)
					{
						if (SkipLoggingEventToGrid(eventLog))
							continue;

						dgvEventLogging.AddEventLog(eventLog);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Exception occured while filtering events log grid. {ex}.", "Error");
			}
		}

		private void btnClearEventLogs_Click(object sender, EventArgs e)
		{
			if (dgvEventLogging.Rows.Count == 0)
				return;

			if (MessageBox.Show("Are you sure to clear all event logs", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
			{
				eventLogs.Clear();
				dgvEventLogging.Rows.Clear();
				dgvEventLogging.Refresh();
			}
		}

		private void dgvEventLogging_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			var dgv = sender as DataGridView;
			var eventLogValue = dgv?.Rows[dgv.CurrentCell.RowIndex]?.Cells["EventLog"]?.Value?.ToString();
			if (!string.IsNullOrWhiteSpace(eventLogValue))
			{
				var eventLog = JsonConvert.DeserializeObject<EventLog>(eventLogValue);
				if (!string.IsNullOrWhiteSpace(eventLog?.ReportParams?.PlainText)
					|| !string.IsNullOrWhiteSpace(eventLog?.ReportParams?.RichText))
				{
					var reportTextDialog = new ReportTextDialog(eventLog);
					reportTextDialog.ShowDialog();
				}
			}
		}

		private void btnExportEventLogs_Click(object sender, EventArgs e)
		{
			this.dgvEventLogging.ExportEventLogs();
		}

		#endregion

		#region RadWhereAx Events

		void RW_AccessionNumbersChanged(string siteName, string accessionNumbers, int status, bool isAddendum, string plainText, string richText)
		{
			var reportParams = new ReportParams
			{
				SiteName = siteName,
				AccessionNumbers = accessionNumbers.Split(new char[1] { ',' }),
				Status = (ReportStatus)status,
				IsAddendum = isAddendum,
				PlainText = plainText,
				RichText = richText,
				ImpendingStatus = (ReportStatus)status
			};
			AccessionNumbersChanged(reportParams);
		}

		void RW_AudioTranscribed(bool textRecognized)
		{
			AudioTranscribed(textRecognized);
		}

		void RW_DictationStarted()
		{
			DictationStarted();
		}

		void RW_DictationStopped()
		{
			DictationStopped();
		}

		void RW_ErrorOccurred(int errCode, string message)
		{
			ErrorOccurred(errCode, message);
		}

		void RW_Launched()
		{
			OnConnected();
		}

		void RW_OpenStudy(string mrn, string accessions)
		{
			OpenStudy(mrn, accessions);
		}

		void RW_ReportChanged(string siteName, string accessionNumbers, int status, bool isAddendum, string plainText, string richText)
		{
			var reportParams = new ReportParams
			{
				SiteName = siteName,
				AccessionNumbers = accessionNumbers.Split(new char[1] { ',' }),
				PatientIdentifier = string.Empty,
				Status = (ReportStatus)status,
				IsAddendum = isAddendum,
				PlainText = plainText,
				RichText = richText,
				ImpendingStatus = (ReportStatus)status
			};
			ReportChanged(reportParams);
		}

		void RW_ReportClosed(string siteName, string accessionNumbers, int status, bool isAddendum, string plainText, string richText)
		{
			var reportParams = new ReportParams
			{
				SiteName = siteName,
				AccessionNumbers = accessionNumbers.Split(new char[1] { ',' }),
				Status = (ReportStatus)status,
				IsAddendum = isAddendum,
				PlainText = plainText,
				RichText = richText,
				ImpendingStatus = (ReportStatus)status
			};
			ReportClosed(reportParams);
		}

		void RW_ReportClosed2(string siteName, string accessionNumbers, int status, bool isAddendum, string plainText, string richText, int impendingStatus)
		{
			var reportParams = new ReportParams
			{
				SiteName = siteName,
				AccessionNumbers = accessionNumbers.Split(new char[1] { ',' }),
				Status = (ReportStatus)status,
				IsAddendum = isAddendum,
				PlainText = plainText,
				RichText = richText,
				ImpendingStatus = (ReportStatus)status
			};
			ReportClosed2(reportParams);
		}

		void RW_ReportOpened(string siteName, string accessionNumbers, int status, bool isAddendum, string plainText, string richText)
		{
			var reportParams = new ReportParams
			{
				SiteName = siteName,
				AccessionNumbers = accessionNumbers.Split(new char[1] { ',' }),
				Status = (ReportStatus)status,
				IsAddendum = isAddendum,
				PlainText = plainText,
				RichText = richText,
				ImpendingStatus = (ReportStatus)status
			};
			ReportOpened(reportParams);
		}

		void RW_Terminated()
		{
            _terminateEvent.Set();
            Terminated();
		}

		void RW_UserLoggedIn(string userName)
		{
			UserLoggedIn(userName);
		}

		void RW_UserLoggedOut(string userName)
		{
			UserLoggedOut(userName);
		}

		#endregion

		#region RadWhereAx Methods

		private void AssociateOrders(Dictionary<string, object> paramValues)
		{
			try
			{
				AddClientEventLog($"{nameof(AssociateOrders)} Pending...", EventTypeEnum.APIRequest);

				var numOfOrdersAssociated = _rwAx.AssociateOrders(paramValues.ElementAt(0).Value.ToString());

				AddClientEventLog($"There are {numOfOrdersAssociated} number of orders/accessions associated by AssociateOrders", EventTypeEnum.APIResponse);

				if (numOfOrdersAssociated > 0 && cmbApiProperty.SelectedItem.ToString() == IRadWhereApiPropertiesEnum.AccessionNumbers.ToString())
					txtApiPropertyValue.Text = _rwAx?.accessionNumbers != null ? string.Join(",", _rwAx.accessionNumbers) : string.Empty;
			}
			catch (Exception ex)
			{
				AddClientEventLog($"{nameof(AssociateOrders)} Error: {ex}", EventTypeEnum.Error);
			}
		}

		private void AssociateOrdersEx(Dictionary<string, object> paramValues)
		{
			try
			{
				var siteName = paramValues.ElementAt(0).Value.ToString();
				var accNum = paramValues.ElementAt(1).Value.ToString();
				var newAccNumbsers = paramValues.ElementAt(2).Value.ToString();

				AddClientEventLog($"{nameof(AssociateOrdersEx)} Pending...", EventTypeEnum.APIRequest);
				var numOfOrdersAssociated = _rwAx.AssociateOrdersEx(siteName, accNum, newAccNumbsers);
				AddClientEventLog($"There are {numOfOrdersAssociated} orders/accessions affected by AssociateOrdersEx.", EventTypeEnum.APIResponse);
			}
			catch (Exception ex)
			{
				AddClientEventLog($"{nameof(AssociateOrdersEx)} Error: {ex}", EventTypeEnum.Error);
			}
		}

		private void CancelReport(Dictionary<string, object> paramValues)
		{
			try
			{
				AddClientEventLog($"{nameof(CancelReport)} Pending...", EventTypeEnum.APIRequest);

				var success = _rwAx.CancelReport(Convert.ToBoolean(paramValues.ElementAt(0).Value));

				AddClientEventLog($"{nameof(CancelReport)} {(success ? "Succeeded" : "Failed")}.", EventTypeEnum.APIResponse);
			}
			catch (Exception ex)
			{
				AddClientEventLog($"{nameof(CancelReport)} Error: {ex}", EventTypeEnum.Error);
			}
		}

		private void CloseReport(Dictionary<string, object> paramValues)
		{
			try
			{
				AddClientEventLog($"{nameof(CloseReport)} Pending...", EventTypeEnum.APIRequest);

				var success = _rwAx.CloseReport(Convert.ToBoolean(paramValues.ElementAt(0).Value),
					Convert.ToBoolean(paramValues.ElementAt(1).Value));

				AddClientEventLog($"{nameof(CloseReport)} {(success ? "Succeeded" : "Failed")}.", EventTypeEnum.APIResponse);
			}
			catch (Exception ex)
			{
				AddClientEventLog($"{nameof(CloseReport)} Error: {ex}", EventTypeEnum.Error);
			}
		}

		private void CreateNewReport(Dictionary<string, object> paramValues)
		{
			try
			{
				AddClientEventLog($"{nameof(CreateNewReport)} Pending...", EventTypeEnum.APIRequest);
				_rwAx.CreateNewReport(paramValues.ElementAt(0).Value.ToString());
				AddClientEventLog($"{nameof(CreateNewReport)} Completed.", EventTypeEnum.APIResponse);
			}
			catch (Exception ex)
			{
				AddClientEventLog($"{nameof(CreateNewReport)} Error: {ex}", EventTypeEnum.Error);
			}
		}

		private void DissociateOrders(Dictionary<string, object> paramValues)
		{
			try
			{
				AddClientEventLog($"{nameof(DissociateOrders)} Pending...", EventTypeEnum.APIRequest);

				var success = _rwAx.DissociateOrders(paramValues.ElementAt(0).Value.ToString());

				AddClientEventLog(string.Format("DissociateOrders {0} {1}", paramValues.ElementAt(0).Value.ToString(), success ? "succeeded" : "failed"), EventTypeEnum.APIResponse);

				if (success && cmbApiProperty.SelectedItem.ToString() == IRadWhereApiPropertiesEnum.AccessionNumbers.ToString())
					txtApiPropertyValue.Text = _rwAx?.accessionNumbers != null ? string.Join(",", _rwAx.accessionNumbers) : string.Empty;
			}
			catch (Exception ex)
			{
				AddClientEventLog($"{nameof(DissociateOrders)} Error: {ex}", EventTypeEnum.Error);
			}
		}

		private void InsertAutoText(Dictionary<string, object> paramValues)
		{
			try
			{
				var autoTextName = paramValues.ElementAt(0).Value.ToString();
				if (autoTextName?.Trim().ToLower() == "null")
					autoTextName = null;
				else
					autoTextName = autoTextName.Replace("\\n", "\n");

				AddClientEventLog($"{nameof(InsertAutoText)} Pending...", EventTypeEnum.APIRequest);
				var success = _rwAx.InsertAutoText(autoTextName, Convert.ToBoolean(paramValues.ElementAt(1).Value));
				AddClientEventLog($"{nameof(InsertAutoText)} {(success ? "Succeeded" : "Failed")}.", EventTypeEnum.APIResponse);
			}
			catch (Exception ex)
			{
				AddClientEventLog($"{nameof(InsertAutoText)} Error: {ex}", EventTypeEnum.Error);
			}
		}

		private void Login(Dictionary<string, object> paramValues)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				if (_rwAx == null)
				{
					AddClientEventLog("You must connect to PS client before Login.", EventTypeEnum.Error);
					return;
				}

				var username = paramValues.ElementAt(0).Value.ToString();
				var password = paramValues.ElementAt(1).Value.ToString();

				AddClientEventLog("Login Pending...", EventTypeEnum.APIRequest);
				_rwAx.Login(username, password);
				AddClientEventLog($"Login Completed.", EventTypeEnum.APIResponse);
			}
			catch (Exception ex)
			{
				AddClientEventLog($"Login Error: {ex}", EventTypeEnum.Error);
			}
		}

		private void LoginEx(Dictionary<string, object> paramValues)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				if (_rwAx == null)
				{
					AddClientEventLog("You must connect to PS client before Login.", EventTypeEnum.Error);
					return;
				}

				var username = paramValues.ElementAt(0).Value.ToString();
				var password = paramValues.ElementAt(1).Value.ToString();

				AddClientEventLog("LoginEx Pending...", EventTypeEnum.APIRequest);
				var success = _rwAx.LoginEx(username, password);
				AddClientEventLog($"LoginEx {(success ? "Succeeded" : "Failed")}.", EventTypeEnum.APIResponse);
			}
			catch (Exception ex)
			{
				AddClientEventLog($"LoginEx Error: {ex}", EventTypeEnum.Error);
			}
		}

		private void Logout()
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				if (_rwAx == null)
				{
					AddClientEventLog("Cannot logout from PowerScribe Client, since it is not connected.", EventTypeEnum.Error);
					return;
				}
				AddClientEventLog("Logout Pending...", EventTypeEnum.APIRequest);
				_rwAx.Logout();
				AddClientEventLog($"Logout Completed.", EventTypeEnum.APIResponse);
			}
			catch (Exception ex)
			{
				AddClientEventLog($"Logout Error: {ex}", EventTypeEnum.Error);
			}
		}

		private void OpenReport(Dictionary<string, object> paramValues)
		{
			try
			{
				AddClientEventLog($"{nameof(OpenReport)} Pending...", EventTypeEnum.APIRequest);

				var success = _rwAx.OpenReport(paramValues.ElementAt(0).Value.ToString(), paramValues.ElementAt(1).Value.ToString());

				AddClientEventLog($"{nameof(OpenReport)} {(success ? "Succeeded" : "Failed")}.", EventTypeEnum.APIResponse);
			}
			catch (Exception ex)
			{
				AddClientEventLog($"{nameof(OpenReport)} Error: {ex}", EventTypeEnum.Error);
			}
		}

		private void OpenReportEx(Dictionary<string, object> paramValues)
		{
			try
			{
				AddClientEventLog($"{nameof(OpenReportEx)} Pending...", EventTypeEnum.APIRequest);

				var success = _rwAx.OpenReportEx(paramValues.ElementAt(0).Value.ToString(),
					paramValues.ElementAt(1).Value.ToString(),
					paramValues.ElementAt(1).Value.ToString());

				AddClientEventLog($"{nameof(OpenReportEx)} {(success ? "Succeeded" : "Failed")}.", EventTypeEnum.APIResponse);
			}
			catch (Exception ex)
			{
				AddClientEventLog($"{nameof(OpenReportEx)} Error: {ex}", EventTypeEnum.Error);
			}
		}

		private void PreviewOrders(Dictionary<string, object> paramValues)
		{
			try
			{
				var siteName = paramValues.ElementAt(0).Value.ToString();
				if (siteName?.Trim().ToLower() == "null")
					siteName = null;
				var accNumbsers = paramValues.ElementAt(1).Value.ToString();

				AddClientEventLog($"{nameof(PreviewOrders)} Pending...", EventTypeEnum.APIRequest);
				var success = _rwAx.PreviewOrders(siteName, accNumbsers);
				AddClientEventLog($"{nameof(PreviewOrders)} {(success ? "Succeeded" : "Failed")}.", EventTypeEnum.APIResponse);
			}
			catch (Exception ex)
			{
				AddClientEventLog($"{nameof(PreviewOrders)} Error: {ex}", EventTypeEnum.Error);
			}
		}

		private void SaveReport(Dictionary<string, object> paramValues)
		{
			try
			{
				AddClientEventLog($"{nameof(SaveReport)} Pending...", EventTypeEnum.APIRequest);

				var success = _rwAx.SaveReport(Convert.ToBoolean(paramValues.ElementAt(0).Value));

				AddClientEventLog($"{nameof(SaveReport)} {(success ? "Succeeded" : "Failed")}.", EventTypeEnum.APIResponse);
			}
			catch (Exception ex)
			{
				AddClientEventLog($"{nameof(SaveReport)} Error: {ex}", EventTypeEnum.Error);
			}
		}

		private void StartListening()
		{
			try
			{
				if (_rwAx == null)
				{
					AddClientEventLog("Cannot StartListening from PowerScribe Client, since it is not connected.", EventTypeEnum.Error);
					return;
				}
				AddClientEventLog("StartListening Pending...", EventTypeEnum.APIRequest);
				_rwAx.StartListening();
				AddClientEventLog($"StartListening Completed.", EventTypeEnum.APIResponse);
			}
			catch (Exception ex)
			{
				AddClientEventLog($"StartListening Error: {ex}", EventTypeEnum.Error);
			}
		}

		private void Stop()
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				if (_rwAx == null)
				{
					AddClientEventLog("Cannot Stop PowerScribe Client, since it is not connected.", EventTypeEnum.Error);
					return;
				}
				AddClientEventLog("Stop Pending...", EventTypeEnum.APIRequest);
				_rwAx.Stop();
				AddClientEventLog($"Stop Completed.", EventTypeEnum.APIResponse);
                btnConnectToPSClient.EnableWithUIStyle(true);
            }
            catch (Exception ex)
			{
				AddClientEventLog($"Stop Error: {ex}", EventTypeEnum.Error);
			}
		}

		private void StopListening()
		{
			try
			{
				if (_rwAx == null)
				{
					AddClientEventLog("Cannot StopListening from PowerScribe Client, since it is not connected.", EventTypeEnum.Error);
					return;
				}
				AddClientEventLog("StopListening Pending...", EventTypeEnum.APIRequest);
				_rwAx.StopListening();
				AddClientEventLog($"StopListening Completed.", EventTypeEnum.APIResponse);
			}
			catch (Exception ex)
			{
				AddClientEventLog($"StopListening Error: {ex}", EventTypeEnum.Error);
			}
		}

		#endregion

		#region Private Methods

		private delegate void OnRadWhereAxConnectedDelegate();

		private void Connect()
		{
			btnConnectToPSClient.EnableWithUIStyle(false); // do this to prevent double-click
			try
			{
				_rwAx = new RadWhereCtrl();
				RegisterRadWhereAxEvents();

				bool success = false;
				success = _rwAx.Start();
				
				if (!success)
				{
					AddClientEventLog("Failed to connect to PowerScribe. Check RadWhereAx log file for details.", EventTypeEnum.APIResponse);
					_rwAx = null;
					return;
				}
				OnConnected();
                AddClientEventLog("Successfully Connected to PowerScribe.", EventTypeEnum.APIResponse);
            }
            catch (System.Net.Sockets.SocketException ex)
			{
				_rwAx = null;
				AddClientEventLog(ex.ToString(), EventTypeEnum.Error);
				btnConnectToPSClient.EnableWithUIStyle(true);
			}
			catch (Exception ex)
			{
				_rwAx = null;
				//TTP 4086
				if (_skipConnectException)
				{
					_skipConnectException = false;
					_retryConnect = true;
					return;
				}
				AddClientEventLog(ex.ToString(), EventTypeEnum.Error);
				btnConnectToPSClient.EnableWithUIStyle(true);
			}
		}

		private void OnConnected()
		{
			if (InvokeRequired)
			{
				var outputDelegate = new OnRadWhereAxConnectedDelegate(OnConnectedD);
				Invoke(outputDelegate);
			}
			else
				OnConnectedD();
		}

		private void OnConnectedD()
		{
			//Populate API Methods
			List<string> methodNames = new List<string> { "-- Select API Method --" };
			radWhereAxMethodInfos.Clear();
			radWhereAxMethodInfos = typeof(IRadWhereCtrl).GetApiMethodInfos().ToList();
			methodNames.AddRange(radWhereAxMethodInfos.Select(m => m.Name)
				.Where(m => m != nameof(_rwAx.Start) && m != nameof(_rwAx.Launch))
				.OrderBy(m => m)
				.ToList());
			cmbApiMethod.DataSource = methodNames;

			//Populate API Events
			//Note that API events are implemented as methods in the _IRadWhereCtrlEvents interface
			apiEvents.Clear();
			apiEvents.AddRange(typeof(_IRadWhereCtrlEvents).GetApiMethodInfos()
				.Select(e => new PSEvent
				{
					EventName = e.Name
				}));

			//Populate API Properties
			cmbApiProperty.DataSource = typeof(IRadWhereApiPropertiesEnum).GetApiProperties()
				.Where(p => p != IRadWhereApiPropertiesEnum.PatientIdentifier.ToString())
				.ToList();

			//Set controls
			ccbEventsFilter.Items.Clear();
			apiEvents.ForEach(e => ccbEventsFilter.Items.Add(e.EventName));
			toggleMinimized.IsOn = _rwAx.Minimized;
			toggleAlwaysOnTop.IsOn = _rwAx.AlwaysOnTop;
			toggleRestrictedSession.IsOn = _rwAx.RestrictedSession;
			toggleRestrictedWorkflow.IsOn = _rwAx.RestrictedWorkflow;

			Disconnect -= RadWhereAxDisconnect;
			Disconnect += RadWhereAxDisconnect;

			SetRadWhereAxUIState();
		}

		private void RegisterRadWhereAxEvents()
		{
			_rwAx.AccessionNumbersChanged += new _IRadWhereCtrlEvents_AccessionNumbersChangedEventHandler(RW_AccessionNumbersChanged);
			_rwAx.AudioTranscribed += new _IRadWhereCtrlEvents_AudioTranscribedEventHandler(RW_AudioTranscribed);
			_rwAx.DictationStarted += new _IRadWhereCtrlEvents_DictationStartedEventHandler(RW_DictationStarted);
			_rwAx.DictationStopped += new _IRadWhereCtrlEvents_DictationStoppedEventHandler(RW_DictationStopped);
			_rwAx.ErrorOccurred += new _IRadWhereCtrlEvents_ErrorOccurredEventHandler(RW_ErrorOccurred);
			_rwAx.Launched += new _IRadWhereCtrlEvents_LaunchedEventHandler(RW_Launched);
			_rwAx.OpenStudy += new _IRadWhereCtrlEvents_OpenStudyEventHandler(RW_OpenStudy);
			_rwAx.ReportChanged += new _IRadWhereCtrlEvents_ReportChangedEventHandler(RW_ReportChanged);
			_rwAx.ReportClosed += new _IRadWhereCtrlEvents_ReportClosedEventHandler(RW_ReportClosed);
			_rwAx.ReportClosed2 += new _IRadWhereCtrlEvents_ReportClosed2EventHandler(RW_ReportClosed2);
			_rwAx.ReportOpened += new _IRadWhereCtrlEvents_ReportOpenedEventHandler(RW_ReportOpened);
			_rwAx.Terminated += new _IRadWhereCtrlEvents_TerminatedEventHandler(RW_Terminated);
			_rwAx.UserLoggedIn += new _IRadWhereCtrlEvents_UserLoggedInEventHandler(RW_UserLoggedIn);
			_rwAx.UserLoggedOut += new _IRadWhereCtrlEvents_UserLoggedOutEventHandler(RW_UserLoggedOut);

            AddClientEventLog("Added/updated event subscriptions.", EventTypeEnum.APIResponse);
        }

        private void UnregisterRadWhereAxEvents()
		{
			_rwAx.AccessionNumbersChanged -= new _IRadWhereCtrlEvents_AccessionNumbersChangedEventHandler(RW_AccessionNumbersChanged);
			_rwAx.AudioTranscribed -= new _IRadWhereCtrlEvents_AudioTranscribedEventHandler(RW_AudioTranscribed);
			_rwAx.DictationStarted -= new _IRadWhereCtrlEvents_DictationStartedEventHandler(RW_DictationStarted);
			_rwAx.DictationStopped -= new _IRadWhereCtrlEvents_DictationStoppedEventHandler(RW_DictationStopped);
			_rwAx.ErrorOccurred -= new _IRadWhereCtrlEvents_ErrorOccurredEventHandler(RW_ErrorOccurred);
			_rwAx.Launched -= new _IRadWhereCtrlEvents_LaunchedEventHandler(RW_Launched);
			_rwAx.OpenStudy -= new _IRadWhereCtrlEvents_OpenStudyEventHandler(RW_OpenStudy);
			_rwAx.ReportChanged -= new _IRadWhereCtrlEvents_ReportChangedEventHandler(RW_ReportChanged);
			_rwAx.ReportClosed -= new _IRadWhereCtrlEvents_ReportClosedEventHandler(RW_ReportClosed);
			_rwAx.ReportClosed2 -= new _IRadWhereCtrlEvents_ReportClosed2EventHandler(RW_ReportClosed2);
			_rwAx.ReportOpened -= new _IRadWhereCtrlEvents_ReportOpenedEventHandler(RW_ReportOpened);
			_rwAx.Terminated -= new _IRadWhereCtrlEvents_TerminatedEventHandler(RW_Terminated);
			_rwAx.UserLoggedIn -= new _IRadWhereCtrlEvents_UserLoggedInEventHandler(RW_UserLoggedIn);
			_rwAx.UserLoggedOut -= new _IRadWhereCtrlEvents_UserLoggedOutEventHandler(RW_UserLoggedOut);
		}

		private void RadWhereAxAddEventLog(object sender, EventLog eventLog)
		{
			try
			{
				eventLogs.Add(eventLog);

				if (SkipLoggingEventToGrid(eventLog))
					return;

				dgvEventLogging.AddEventLog(eventLog);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Exception occured while adding event log. {ex.Message}.", "Error");
			}
		}

		private void SetupRadWhereAxEventHandlers(List<string> selectedEvents)
		{
			try
			{
				if (_rwAx == null)
				{
					AddClientEventLog("Not connected to PowerScribe Client, for setting RadWhereAx event handlers.", EventTypeEnum.Error);
					return;
				}

				if (selectedEvents != null)
					UnregisterRadWhereAxEvents();

				if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwAx.AccessionNumbersChanged)) != null)
					_rwAx.AccessionNumbersChanged += new _IRadWhereCtrlEvents_AccessionNumbersChangedEventHandler(RW_AccessionNumbersChanged);

				if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwAx.AudioTranscribed)) != null)
					_rwAx.AudioTranscribed += new _IRadWhereCtrlEvents_AudioTranscribedEventHandler(RW_AudioTranscribed);

				if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwAx.DictationStarted)) != null)
					_rwAx.DictationStarted += new _IRadWhereCtrlEvents_DictationStartedEventHandler(RW_DictationStarted);

				if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwAx.DictationStopped)) != null)
					_rwAx.DictationStopped += new _IRadWhereCtrlEvents_DictationStoppedEventHandler(RW_DictationStopped);

				if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwAx.ErrorOccurred)) != null)
					_rwAx.ErrorOccurred += new _IRadWhereCtrlEvents_ErrorOccurredEventHandler(RW_ErrorOccurred);

				if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwAx.Launched)) != null)
					_rwAx.Launched += new _IRadWhereCtrlEvents_LaunchedEventHandler(RW_Launched);

				if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwAx.OpenStudy)) != null)
					_rwAx.OpenStudy += new _IRadWhereCtrlEvents_OpenStudyEventHandler(RW_OpenStudy);

				if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwAx.ReportChanged)) != null)
					_rwAx.ReportChanged += new _IRadWhereCtrlEvents_ReportChangedEventHandler(RW_ReportChanged);

				if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwAx.ReportClosed)) != null)
					_rwAx.ReportClosed += new _IRadWhereCtrlEvents_ReportClosedEventHandler(RW_ReportClosed);

				if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwAx.ReportClosed2)) != null)
					_rwAx.ReportClosed2 += new _IRadWhereCtrlEvents_ReportClosed2EventHandler(RW_ReportClosed2);

				if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwAx.ReportOpened)) != null)
					_rwAx.ReportOpened += new _IRadWhereCtrlEvents_ReportOpenedEventHandler(RW_ReportOpened);

				if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwAx.Terminated)) != null)
					_rwAx.Terminated += new _IRadWhereCtrlEvents_TerminatedEventHandler(RW_Terminated);

				if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwAx.UserLoggedIn)) != null)
					_rwAx.UserLoggedIn += new _IRadWhereCtrlEvents_UserLoggedInEventHandler(RW_UserLoggedIn);

				if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwAx.UserLoggedOut)) != null)
					_rwAx.UserLoggedOut += new _IRadWhereCtrlEvents_UserLoggedOutEventHandler(RW_UserLoggedOut);

				AddClientEventLog("Updated event subscriptions.");
			}
			catch (Exception ex)
			{
				AddClientEventLog($"Error occured while setting RadWhereCOM event handlers. {ex}", EventTypeEnum.Error);
			}
		}

		private bool SkipLoggingEventToGrid(EventLog eventLog)
		{
			var canSkip = false;

			if (eventLog.EventType == EventTypeEnum.Default)
				canSkip = true;

			if (toggleShowEventsOnly.IsOn && eventLog.EventType != EventTypeEnum.PowerScribeEvent)
				canSkip = true;

			if (eventLog.EventType == EventTypeEnum.PowerScribeEvent && ccbEventsFilter.CheckedItems.Count > 0)
			{
				if (!ccbEventsFilter.CheckedItems.Contains(eventLog.PowerScribeEventName))
					canSkip = true;
			}

			return canSkip;
		}

		private void RadWhereAxDisconnect(object sender, EventArgs e)
		{
			if (_rwAx == null)
				return;

			try
			{
				UnregisterRadWhereAxEvents();
				_rwAx = null;
				SetRadWhereAxUIState();
			}
			catch (Exception ex)
			{
				AddClientEventLog(ex.ToString(), EventTypeEnum.Error);
				//TTP 4086
				if (_skipConnectException)
				{
					_skipConnectException = false;
				}
				else
				{
					MessageBox.Show(ex.Message);
				}
			}
		}

		private void SetRadWhereAxUIState()
		{
			var connected = _rwAx != null;

			btnExecute.EnableWithUIStyle(connected && !Equals(cmbApiMethod?.SelectedItem, CMB_API_METHOD_DEFAULT_VALUE));
			btnConnectToPSClient.EnableWithUIStyle(!connected);
			cmbApiProperty.Enabled = connected;
			txtApiPropertyValue.Enabled = connected;
			toggleMinimized.Enabled = connected;
			toggleAlwaysOnTop.Enabled = connected;
			toggleRestrictedSession.Enabled = connected;
			toggleRestrictedWorkflow.Enabled = connected;
			cmbApiMethod.Enabled = connected;
			pnlParameters.Enabled = connected;
		}

		#endregion
	}
}
