using APIClient.Common.Enums;
using APIClient.Common.Extensions;
using APIClient.Common.Forms;
using APIClient.Common.Models;
using Commissure.PACSConnector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace APIClient.RadWhereCOM
{
	public partial class RadWhereCOMForm : BaseForm
    {
        private Commissure.PACSConnector.RadWhereCOM _rwCOM = null;
        private static IList<MethodInfo> radWhereCOMMethodInfos = new List<MethodInfo>();
        private static List<EventInfo> radWhereCOMEventInfos = new List<EventInfo>();
        private MethodInfo selectedApiMethod;

        #region Constructors

        public RadWhereCOMForm()
        {
            InitializeComponent();
            this.Load += BaseForm_Load;
        }

        #endregion

        #region UI Events

        private void RadWhereCOMForm_Load(object sender, EventArgs e)
        {
            toolTipSettings.SetToolTip(btnSettings, "Settings");
            SetRadWhereCOMUIState();
            AddLogToClientDataGrid += RadWhereCOMAddEventLog;
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

                if (_rwCOM == null)
                {
                    AddClientEventLog("Can not terminate PowerScribe Client, since it is not connected.", EventTypeEnum.Error);
                    return;
                }

				AddClientEventLog("Termination Pending...", EventTypeEnum.APIRequest);
				var success = _rwCOM.Terminate();
                _skipConnectException = true;   //TTP 4086
                if (success)
                {
                    AddClientEventLog($"Termination Succeeded.", EventTypeEnum.APIResponse);
                    RadWhereCOMDisconnect(this, null);
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

                    SetupRadWhereCOMEventHandlers(settingsDialog.SelectedEvents);
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
                if (_rwCOM == null)
                {
                    AddClientEventLog("You must connect to PS client before Login.", EventTypeEnum.Error);
                    return;
                }

                var pwd = (ModifierKeys == Keys.Shift) ? null : txtPassword.Text;
                AddClientEventLog("Login Pending...", EventTypeEnum.APIRequest);
                var success = _rwCOM.LoginEx(txtUsername.Text, pwd);
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
                if (_rwCOM == null)
                {
                    AddClientEventLog("Cannot logout from PowerScribe Client, since it is not connected.", EventTypeEnum.Error);
                    return;
                }
                AddClientEventLog("Logout Pending...", EventTypeEnum.APIRequest);
                var success = _rwCOM.LogoutEx();
                AddClientEventLog($"Logout {(success ? "Succeeded" : "Failed")}.", EventTypeEnum.APIResponse);
            }
            catch (Exception ex)
            {
                AddClientEventLog($"Logout Error: {ex}", EventTypeEnum.Error);
            }
        }

        private void btnLaunch_Click(object sender, System.EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                AddClientEventLog("Attempting to launch/connect to PowerScribe...", EventTypeEnum.APIRequest);
                btnConnectToPSClient.EnableWithUIStyle(false);

                _rwCOM = new Commissure.PACSConnector.RadWhereCOM();
                RegisterRadWhereCOMEvents();

                var pwd = (ModifierKeys == Keys.Shift) ? null : txtPassword.Text;
                _rwCOM.Launch(txtUsername.Text, pwd, txtServer.Text);
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
                txtApiPropertyValue.Text = _rwCOM?.AccessionNumbers != null ? string.Join(",", _rwCOM.AccessionNumbers) : string.Empty;
            }
            else if (selectedProperty == IRadWhereApiPropertiesEnum.Username.ToString())
            {
                txtApiPropertyValue.Text = _rwCOM?.Username ?? string.Empty;
            }
            else if (selectedProperty == IRadWhereApiPropertiesEnum.SiteName.ToString())
            {
                txtApiPropertyValue.Text = _rwCOM?.SiteName ?? string.Empty;
            }
        }

        private void toggleMinimized_Click(object sender, EventArgs e)
        {
            if (_rwCOM != null)
                _rwCOM.Minimized = toggleMinimized.IsOn;
        }

        private void toggleAlwaysOnTop_Click(object sender, EventArgs e)
        {
            if (_rwCOM != null)
                _rwCOM.AlwaysOnTop = toggleAlwaysOnTop.IsOn;
        }

        private void toggleRestrictedSession_Click(object sender, EventArgs e)
        {
            if (_rwCOM != null)
                _rwCOM.RestrictedSession = toggleRestrictedSession.IsOn;
        }

        private void toggleRestrictedWorkflow_Click(object sender, EventArgs e)
        {
            if (_rwCOM != null)
                _rwCOM.RestrictedWorkflow = toggleRestrictedWorkflow.IsOn;
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

            toggleShowEventsOnly.Enabled = true;
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
                selectedApiMethod = radWhereCOMMethodInfos.FirstOrDefault(m => m.Name == cmbApiMethod.SelectedItem.ToString());
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
                    case nameof(_rwCOM.LoginEx):
                        LoginEx(paramValues);
                        break;
                    case nameof(_rwCOM.Login):
                        Login(paramValues);
                        break;
                    case nameof(_rwCOM.LogoutEx):
                        btnLogout_Click(sender, e);
                        break;
                    case nameof(_rwCOM.Logout):
                        Logout();
                        break;
                    case nameof(_rwCOM.Stop):
                        Stop();
                        break;
                    case nameof(_rwCOM.Terminate):
                        btnTerminatePSClient_Click(sender, e);
                        break;
                    case nameof(_rwCOM.OpenReport):
                        OpenReport(paramValues);
                        break;
                    case nameof(_rwCOM.OpenReportEx):
                        OpenReportEx(paramValues);
                        break;
                    case nameof(_rwCOM.CloseReport):
                        CloseReport(paramValues);
                        break;
                    case nameof(_rwCOM.CancelReport):
                        CancelReport(paramValues);
                        break;
                    case nameof(_rwCOM.SaveReport):
                        SaveReport(paramValues);
                        break;
                    case nameof(_rwCOM.AssociateOrders):
                        AssociateOrders(paramValues);
                        break;
                    case nameof(_rwCOM.AssociateOrdersEx):
                        AssociateOrdersEx(paramValues);
                        break;
                    case nameof(_rwCOM.DissociateOrders):
                        DissociateOrders(paramValues);
                        break;
                    case nameof(_rwCOM.PreviewOrders):
                        PreviewOrders(paramValues);
                        break;
                    case nameof(_rwCOM.InsertAutoText):
                        InsertAutoText(paramValues);
                        break;
                    case nameof(_rwCOM.CreateNewReport):
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

		#region RadWhereCOM events

		void _rwCOM_UserLoggedIn(string userName)
        {
            UserLoggedIn(userName);
        }

        void _rwCOM_UserLoggedOut(string userName)
        {
            UserLoggedOut(userName);
        }

        void _rwCOM_ReportOpened(string siteName, string accessionNumbers, int status, bool isAddendum, string plainText, string richText)
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

        void _rwCOM_ReportClosed(string siteName, string accessionNumbers, int status, bool isAddendum, string plainText, string richText)
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

        void _rwCOM_ReportClosed2(string siteName, string accessionNumbers, int status, bool isAddendum, string plainText, string richText, int impendingStatus)
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

        private void _rwCOM_ReportClosed3(string siteName, string accessionNumbers, 
            int status, bool isAddendum, string plainText, string richText, 
            int impendingStatus, bool sentAsPrelim, string signer, string signerId, string dictator, string dictatorId)
        {
            var reportParams = new ReportParams
            {
                SiteName = siteName,
                AccessionNumbers = accessionNumbers.Split(new char[1] { ',' }),
                Status = (ReportStatus)status,
                IsAddendum = isAddendum,
                PlainText = plainText,
                RichText = richText,
                ImpendingStatus = (ReportStatus)impendingStatus,
                SentAsPreliminary = sentAsPrelim,
                SignerUsername = signer,
                SignerSiteIdentifier = signerId ?? string.Empty,
                DictatorUsername = dictator,
                DictatorSiteIdentifier = dictatorId ?? string.Empty
            };

            ReportClosed3(reportParams);
        }

        void _rwCOM_ReportChanged(string siteName, string accessionNumbers, int status, bool isAddendum, string plainText, string richText)
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

        void _rwCOM_AccessionNumbersChanged(string siteName, string accessionNumbers, int status, bool isAddendum, string plainText, string richText)
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

        void _rwCOM_DictationStarted()
        {
            DictationStarted();
        }

        void _rwCOM_DictationStopped()
        {
            DictationStopped();
        }

        void _rwCOM_AudioTranscribed(bool textRecognized)
        {
            AudioTranscribed(textRecognized);
        }

        void _rwCOM_Terminated()
        {
            Terminated();
        }

        void _rwCOM_Launched()
        {
            OnConnected();
        }

        void _rwCOM_PrefetchRequested(string siteName, string accessionNumbers)
        {
            PrefetchRequested();
        }

        void _rwCOM_ReportFinished(int reportStatus)
        {
            ReportFinished(reportStatus);
        }

        void _rwCOM_ErrorOccurred(int errCode, string message)
        {
            ErrorOccurred(errCode, message);
        }

		#endregion

		#region RadWhereCOM Methods

		private void AssociateOrders(Dictionary<string, object> paramValues)
		{
			try
			{
				AddClientEventLog($"{nameof(AssociateOrders)} Pending...", EventTypeEnum.APIRequest);

				var numOfOrdersAssociated = _rwCOM.AssociateOrders(paramValues.ElementAt(0).Value.ToString());

				AddClientEventLog($"There are {numOfOrdersAssociated} number of orders/accessions associated by AssociateOrders", EventTypeEnum.APIResponse);

				if (numOfOrdersAssociated > 0 && cmbApiProperty.SelectedItem.ToString() == IRadWhereApiPropertiesEnum.AccessionNumbers.ToString())
					txtApiPropertyValue.Text = _rwCOM?.AccessionNumbers != null ? string.Join(",", _rwCOM.AccessionNumbers) : string.Empty;
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
				var numOfOrdersAssociated = _rwCOM.AssociateOrdersEx(siteName, accNum, newAccNumbsers);
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

				var success = _rwCOM.CancelReport(Convert.ToBoolean(paramValues.ElementAt(0).Value));

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

				var success = _rwCOM.CloseReport(Convert.ToBoolean(paramValues.ElementAt(0).Value),
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
				_rwCOM.CreateNewReport(paramValues.ElementAt(0).Value.ToString(), Convert.ToBoolean(paramValues.ElementAt(1).Value));
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

				var success = _rwCOM.DissociateOrders(paramValues.ElementAt(0).Value.ToString());

				AddClientEventLog(string.Format("DissociateOrders {0} {1}", paramValues.ElementAt(0).Value.ToString(), success ? "succeeded" : "failed"), EventTypeEnum.APIResponse);

				if (success && cmbApiProperty.SelectedItem.ToString() == IRadWhereApiPropertiesEnum.AccessionNumbers.ToString())
					txtApiPropertyValue.Text = _rwCOM?.AccessionNumbers != null ? string.Join(",", _rwCOM.AccessionNumbers) : string.Empty;
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
				var success = _rwCOM.InsertAutoText(autoTextName, Convert.ToBoolean(paramValues.ElementAt(1).Value));
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
				if (_rwCOM == null)
				{
					AddClientEventLog("You must connect to PS client before Login.", EventTypeEnum.Error);
					return;
				}

				var username = paramValues.ElementAt(0).Value.ToString();
				var password = paramValues.ElementAt(1).Value.ToString();
				var server = paramValues.ElementAt(2).Value.ToString();

				AddClientEventLog("Login Pending...", EventTypeEnum.APIRequest);
				_rwCOM.Login(username, password, server);
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
				if (_rwCOM == null)
				{
					AddClientEventLog("You must connect to PS client before Login.", EventTypeEnum.Error);
					return;
				}

				var username = paramValues.ElementAt(0).Value.ToString();
				var password = paramValues.ElementAt(1).Value.ToString();

				AddClientEventLog("LoginEx Pending...", EventTypeEnum.APIRequest);
				var success = _rwCOM.LoginEx(username, password);
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
				if (_rwCOM == null)
				{
					AddClientEventLog("Cannot logout from PowerScribe Client, since it is not connected.", EventTypeEnum.Error);
					return;
				}
				AddClientEventLog("Logout Pending...", EventTypeEnum.APIRequest);
				_rwCOM.Logout();
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

                var success = _rwCOM.OpenReport(paramValues.ElementAt(0).Value.ToString(), paramValues.ElementAt(1).Value.ToString());

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

                var success = _rwCOM.OpenReportEx(paramValues.ElementAt(0).Value.ToString(),
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
				var success = _rwCOM.PreviewOrders(siteName, accNumbsers);
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

                var success = _rwCOM.SaveReport(Convert.ToBoolean(paramValues.ElementAt(0).Value));

                AddClientEventLog($"{nameof(SaveReport)} {(success ? "Succeeded" : "Failed")}.", EventTypeEnum.APIResponse);
            }
            catch (Exception ex)
            {
                AddClientEventLog($"{nameof(SaveReport)} Error: {ex}", EventTypeEnum.Error);
            }
        }

		private void Stop()
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				if (_rwCOM == null)
				{
					AddClientEventLog("Cannot Stop PowerScribe Client, since it is not connected.", EventTypeEnum.Error);
					return;
				}
				AddClientEventLog("Stop Pending...", EventTypeEnum.APIRequest);
				_rwCOM.Stop();
				AddClientEventLog($"Stop Completed.", EventTypeEnum.APIResponse);
                btnConnectToPSClient.EnableWithUIStyle(true);
			}
			catch (Exception ex)
			{
				AddClientEventLog($"Stop Error: {ex}", EventTypeEnum.Error);
			}
		}

		#endregion

		#region Private Methods

		private delegate void OnRadWhereCOMConnectedDelegate();

		private void Connect()
        {
            btnConnectToPSClient.EnableWithUIStyle(false); // do this to prevent double-click
            try
            {
                _rwCOM = new Commissure.PACSConnector.RadWhereCOM();
                RegisterRadWhereCOMEvents();

                bool success = false;
                _rwCOM.Start(ref success);

                if (!success)
                {
                    AddClientEventLog("Failed to start PowerScribe. Check RadWhereCOM log file for details.", EventTypeEnum.APIResponse);
                    _rwCOM = null;
                    return;
                }
                OnConnected();
                AddClientEventLog("Successfully Connected to PowerScribe.", EventTypeEnum.APIResponse);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _rwCOM = null;
                AddClientEventLog(ex.ToString(), EventTypeEnum.Error);
                btnConnectToPSClient.EnableWithUIStyle(true);
            }
            catch (Exception ex)
            {
                _rwCOM = null;
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
				var outputDelegate = new OnRadWhereCOMConnectedDelegate(OnConnectedD);
				Invoke(outputDelegate);
			}
			else
				OnConnectedD();
		}

		private void OnConnectedD()
		{
			//Populate API Methods
			List<string> methodNames = new List<string> { "-- Select API Method --" };
			List<MethodInfo> methodInfos = typeof(ISpeechIntegrationRW).GetApiMethodInfos().ToList();
			methodInfos.AddRange(typeof(ISpeechIntegration).GetApiMethodInfos());
			radWhereCOMMethodInfos.Clear();
			radWhereCOMMethodInfos = methodInfos;
			methodNames.AddRange(radWhereCOMMethodInfos.Select(m => m.Name)
                .Where(m => m != nameof(_rwCOM.Start) && m != nameof(_rwCOM.Launch))
                .OrderBy(m => m)
                .ToList());
			cmbApiMethod.DataSource = methodNames;

			//Populate API Events
			apiEvents.Clear();
			apiEvents.AddRange(typeof(Commissure.PACSConnector.RadWhereCOM).GetApiEvents()
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
			toggleMinimized.IsOn = _rwCOM.Minimized;
			toggleAlwaysOnTop.IsOn = _rwCOM.AlwaysOnTop;
			toggleRestrictedSession.IsOn = _rwCOM.RestrictedSession;
			toggleRestrictedWorkflow.IsOn = _rwCOM.RestrictedWorkflow;

			Disconnect -= RadWhereCOMDisconnect;
			Disconnect += RadWhereCOMDisconnect;

			SetRadWhereCOMUIState();
		}

		private IList<MethodInfo> GetApiMethodsInfos()
		{
			List<MethodInfo> methodInfos = typeof(ISpeechIntegrationRW).GetApiMethodInfos().ToList();
			methodInfos.AddRange(typeof(ISpeechIntegration).GetApiMethodInfos());
			return methodInfos;
		}

		private void RegisterRadWhereCOMEvents()
        {
            _rwCOM.AccessionNumbersChanged += new RWC_AccessionNumbersChangedHandler(_rwCOM_AccessionNumbersChanged);
            _rwCOM.ReportClosed += new RWC_ReportClosedHandler(_rwCOM_ReportClosed);
            _rwCOM.ReportClosed2 += new RWC_ReportClosed2Handler(_rwCOM_ReportClosed2);
            _rwCOM.ReportClosed3 += new RWC_ReportClosed3Handler(_rwCOM_ReportClosed3);
            _rwCOM.ReportOpened += new RWC_ReportOpenedHandler(_rwCOM_ReportOpened);
            _rwCOM.Terminated += new RWC_TerminatedHandler(_rwCOM_Terminated);
            _rwCOM.UserLoggedIn += new RWC_UserLoggedInHandler(_rwCOM_UserLoggedIn);
            _rwCOM.UserLoggedOut += new RWC_UserLoggedOutHandler(_rwCOM_UserLoggedOut);
            _rwCOM.DictationStarted += new RWC_DictationStartedHandler(_rwCOM_DictationStarted);
            _rwCOM.DictationStopped += new RWC_DictationStoppedHandler(_rwCOM_DictationStopped);
            _rwCOM.AudioTranscribed += new RWC_AudioTranscribedHandler(_rwCOM_AudioTranscribed);
            _rwCOM.ReportChanged += new RWC_ReportChangedHandler(_rwCOM_ReportChanged);
            _rwCOM.ErrorOccurred += new RWC_ErrorOccured(_rwCOM_ErrorOccurred);
            _rwCOM.Launched += new RWC_Launched(_rwCOM_Launched);
            _rwCOM.PrefetchRequested += new RWC_PrefetchRequestedHandler(_rwCOM_PrefetchRequested);
            _rwCOM.ReportFinished += new ReportFinishedHandler(_rwCOM_ReportFinished);

            AddClientEventLog("Added/updated event subscriptions.", EventTypeEnum.APIResponse);
        }

        private void UnRegisterRadWhereCOMEvents()
        {
            _rwCOM.AccessionNumbersChanged -= new RWC_AccessionNumbersChangedHandler(_rwCOM_AccessionNumbersChanged);
            _rwCOM.ReportClosed -= new RWC_ReportClosedHandler(_rwCOM_ReportClosed);
            _rwCOM.ReportClosed2 -= new RWC_ReportClosed2Handler(_rwCOM_ReportClosed2);
            _rwCOM.ReportClosed3 -= new RWC_ReportClosed3Handler(_rwCOM_ReportClosed3);
            _rwCOM.ReportOpened -= new RWC_ReportOpenedHandler(_rwCOM_ReportOpened);
            _rwCOM.Terminated -= new RWC_TerminatedHandler(_rwCOM_Terminated);
            _rwCOM.UserLoggedIn -= new RWC_UserLoggedInHandler(_rwCOM_UserLoggedIn);
            _rwCOM.UserLoggedOut -= new RWC_UserLoggedOutHandler(_rwCOM_UserLoggedOut);
            _rwCOM.DictationStarted -= new RWC_DictationStartedHandler(_rwCOM_DictationStarted);
            _rwCOM.DictationStopped -= new RWC_DictationStoppedHandler(_rwCOM_DictationStopped);
            _rwCOM.AudioTranscribed -= new RWC_AudioTranscribedHandler(_rwCOM_AudioTranscribed);
            _rwCOM.ReportChanged -= new RWC_ReportChangedHandler(_rwCOM_ReportChanged);
            _rwCOM.ErrorOccurred -= new RWC_ErrorOccured(_rwCOM_ErrorOccurred);
            _rwCOM.Launched -= new RWC_Launched(_rwCOM_Launched);
            _rwCOM.PrefetchRequested -= new RWC_PrefetchRequestedHandler(_rwCOM_PrefetchRequested);
            _rwCOM.ReportFinished -= new ReportFinishedHandler(_rwCOM_ReportFinished);
        }

        private void RadWhereCOMAddEventLog(object sender, EventLog eventLog)
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

        private void SetupRadWhereCOMEventHandlers(List<string> selectedEvents = null)
        {
            try
            {
                if (_rwCOM == null)
                {
                    AddClientEventLog("Not connected to PowerScribe Client, for setting RadWhereCOM event handlers.", EventTypeEnum.Error);
                    return;
                }

                if (selectedEvents != null)
                    UnRegisterRadWhereCOMEvents();

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwCOM.AccessionNumbersChanged)) != null)
                    _rwCOM.AccessionNumbersChanged += new RWC_AccessionNumbersChangedHandler(_rwCOM_AccessionNumbersChanged);

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwCOM.ReportClosed)) != null)
                    _rwCOM.ReportClosed += new RWC_ReportClosedHandler(_rwCOM_ReportClosed);

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwCOM.ReportClosed2)) != null)
                    _rwCOM.ReportClosed2 += new RWC_ReportClosed2Handler(_rwCOM_ReportClosed2);

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwCOM.ReportClosed3)) != null)
                    _rwCOM.ReportClosed3 += new RWC_ReportClosed3Handler(_rwCOM_ReportClosed3);

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwCOM.ReportOpened)) != null)
                    _rwCOM.ReportOpened += new RWC_ReportOpenedHandler(_rwCOM_ReportOpened);

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwCOM.Terminated)) != null)
                    _rwCOM.Terminated += new RWC_TerminatedHandler(_rwCOM_Terminated);

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwCOM.UserLoggedIn)) != null)
                    _rwCOM.UserLoggedIn += new RWC_UserLoggedInHandler(_rwCOM_UserLoggedIn);

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwCOM.UserLoggedOut)) != null)
                    _rwCOM.UserLoggedOut += new RWC_UserLoggedOutHandler(_rwCOM_UserLoggedOut);

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwCOM.DictationStarted)) != null)
                    _rwCOM.DictationStarted += new RWC_DictationStartedHandler(_rwCOM_DictationStarted);

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwCOM.DictationStopped)) != null)
                    _rwCOM.DictationStopped += new RWC_DictationStoppedHandler(_rwCOM_DictationStopped);

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwCOM.AudioTranscribed)) != null)
                    _rwCOM.AudioTranscribed += new RWC_AudioTranscribedHandler(_rwCOM_AudioTranscribed);

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwCOM.ReportChanged)) != null)
                    _rwCOM.ReportChanged += new RWC_ReportChangedHandler(_rwCOM_ReportChanged);

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwCOM.ErrorOccurred)) != null)
                    _rwCOM.ErrorOccurred += new RWC_ErrorOccured(_rwCOM_ErrorOccurred);

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwCOM.Launched)) != null)
                    _rwCOM.Launched += new RWC_Launched(_rwCOM_Launched);

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwCOM.PrefetchRequested)) != null)
                    _rwCOM.PrefetchRequested += new RWC_PrefetchRequestedHandler(_rwCOM_PrefetchRequested);

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rwCOM.ReportFinished)) != null)
                    _rwCOM.ReportFinished += new ReportFinishedHandler(_rwCOM_ReportFinished);

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

		private void RadWhereCOMDisconnect(object sender, EventArgs e)
        {
			if (_rwCOM == null)
				return;

			try
			{
				UnRegisterRadWhereCOMEvents();
				_rwCOM = null;
				SetRadWhereCOMUIState();
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

        private void SetRadWhereCOMUIState()
        {
            var connected = _rwCOM != null;

			btnExecute.EnableWithUIStyle(connected && !Equals(cmbApiMethod?.SelectedItem , CMB_API_METHOD_DEFAULT_VALUE));
            btnConnectToPSClient.EnableWithUIStyle(!connected);
			cmbApiProperty.Invoke((MethodInvoker)delegate { cmbApiProperty.Enabled = connected; });
            txtApiPropertyValue.Invoke((MethodInvoker)delegate { txtApiPropertyValue.Enabled = connected; });
            toggleMinimized.Invoke((MethodInvoker)delegate { toggleMinimized.Enabled = connected; });
            toggleAlwaysOnTop.Invoke((MethodInvoker)delegate { toggleAlwaysOnTop.Enabled = connected; });
            toggleRestrictedSession.Invoke((MethodInvoker)delegate { toggleRestrictedSession.Enabled = connected; });
            toggleRestrictedWorkflow.Invoke((MethodInvoker)delegate { toggleRestrictedWorkflow.Enabled = connected; });
            cmbApiMethod.Invoke((MethodInvoker)delegate { cmbApiMethod.Enabled = connected; });
            pnlParameters.Invoke((MethodInvoker)delegate { pnlParameters.Enabled = connected; });
        }

		#endregion
	}
}
