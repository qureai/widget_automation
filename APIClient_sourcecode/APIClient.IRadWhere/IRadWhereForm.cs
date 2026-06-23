using APIClient.Common.Enums;
using APIClient.Common.Extensions;
using APIClient.Common.Forms;
using APIClient.Common.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using Nuance.RadWhere;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Windows.Forms;
using EventLog = APIClient.Common.Models.EventLog;

namespace APIClient.IRadWhere
{
    public partial class IRadWhereForm : BaseForm
    {
        private TcpChannel _channel;
        private string _localHost = "";
        private string _remotePort = "9090";
        private string _remoteHost = "localhost";
        private string _location = "";
        private UserLoggedInEventWrapper _userLoggedIn_w;
        private UserLoggedOutEventWrapper _userLoggedOut_w;
        private ReportOpenedEventWrapper _reportOpened_w;
        private ReportClosedEventWrapper _reportClosed_w;
        private ReportChangedEventWrapper _reportChanged_w;
        private AccessionNumbersChangedEventWrapper _accessionNumbersChanged_w;
        private PrefetchRequestedEventWrapper _prefetchRequested_w;
        private TerminatedEventWrapper _terminated_w;
        private DictationStartedEventWrapper _dictationStarted_w;
        private DictationStoppedEventWrapper _dictationStopped_w;
        private AudioTranscribedEventWrapper _audioTranscribed_w;
        private Nuance.RadWhere.IRadWhere _rw = null;
        private static IList<MethodInfo> iRadWhereMethodInfos = new List<MethodInfo>();
        private static List<EventInfo> iRadWhereEventInfos = new List<EventInfo>();
        private MethodInfo selectedApiMethod;

        public IRadWhereForm()
        {
            InitializeComponent();
            this.Load += BaseForm_Load;
        }

        #region UI Events

        private void IRadWhereForm_Load(object sender, EventArgs e)
        {
            toolTipSettings.SetToolTip(btnSettings, "Settings");
            SetIRadWhereUIState();
            AddLogToClientDataGrid += IRadWhereAddEventLog;
            this.Type.SortMode = DataGridViewColumnSortMode.NotSortable;
            this.Message.SortMode = DataGridViewColumnSortMode.NotSortable;
            this.Time.HeaderCell.SortGlyphDirection = SortOrder.Descending;
            ccbEventsFilter.Text = CCB_EVENTS_FILTER_DEFAULT_TEXT;
        }

        private void IRadWhereForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            IRadWhereDisconnect(sender, e);
        }

        private void btnConnectToPSClient_Click(object sender, EventArgs e)
        {
            bool created = false;

            try
            {
				Cursor.Current = Cursors.WaitCursor;

				AddClientEventLog("Attempting to launch/connect to PowerScribe...", EventTypeEnum.APIRequest);

				btnConnectToPSClient.EnableWithUIStyle(false);// do this to prevent double-click

                Initialize();
                Start(ref created);
            }
            catch (Exception ex)
            {
                AddClientEventLog($"Error occured while connecting to PS Client. {ex}.", EventTypeEnum.Error);
            }
            finally
            {
                btnConnectToPSClient.EnableWithUIStyle(!created);
                Cursor.Current = Cursors.Default;
            }
		}

		private void btnTerminatePSClient_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                if (_rw == null)
                {
                    AddClientEventLog("Cannot terminate PowerScribe Client, since it is not connected.", EventTypeEnum.Error);
                    return;
                }

				AddClientEventLog($"Termination Pending...", EventTypeEnum.APIRequest);
				var success = _rw.Terminate();
                _skipConnectException = true;   //TTP 4086
				if (success)
				{
                    AddClientEventLog($"Termination Succeeded.", EventTypeEnum.APIResponse);
                    IRadWhereDisconnect(this, null);
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

                    SetupIRadWhereEventHandlers(settingsDialog.SelectedEvents);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occured while setting IRadWhere API events. {ex}", "Error");
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                if (_rw == null)
                {
                    AddClientEventLog("You must connect to PS client before Login.", EventTypeEnum.Error);
                    return;
                }
                var pwd = (ModifierKeys == Keys.Shift) ? null : txtPassword.Text;
                AddClientEventLog("Login Pending...", EventTypeEnum.APIRequest);
                var success = _rw.Login(txtUsername.Text, pwd);
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
                if (_rw == null)
                {
                    AddClientEventLog("Cannot logout from PowerScribe Client, since it is not connected.", EventTypeEnum.Error);
                    return;
                }
                AddClientEventLog("Logout Pending...", EventTypeEnum.APIRequest);
                var success = _rw.Logout();
                AddClientEventLog($"Logout {(success ? "Succeeded" : "Failed")}.", EventTypeEnum.APIResponse);
            }
            catch (Exception ex)
            {
                AddClientEventLog($"Logout Error: {ex}", EventTypeEnum.Error);
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
                txtApiPropertyValue.Text = _rw?.AccessionNumbers != null ? string.Join(",", _rw.AccessionNumbers) : string.Empty;
            }
            else if (selectedProperty == IRadWhereApiPropertiesEnum.Username.ToString())
            {
                txtApiPropertyValue.Text = _rw?.Username ?? string.Empty;
            }
            else if (selectedProperty == IRadWhereApiPropertiesEnum.SiteName.ToString())
            {
                txtApiPropertyValue.Text = _rw?.SiteName ?? string.Empty;
            }
            else if (selectedProperty == IRadWhereApiPropertiesEnum.PatientIdentifier.ToString())
            {
                txtApiPropertyValue.Text = _rw?.PatientIdentifier ?? string.Empty;
            }
        }

        private void toggleMinimized_Click(object sender, EventArgs e)
        {
            if (_rw != null)
            {
                _rw.Minimized = toggleMinimized.IsOn;
            }
        }

        private void toggleAlwaysOnTop_Click(object sender, EventArgs e)
        {
            if (_rw != null)
            {
                _rw.AlwaysOnTop = toggleAlwaysOnTop.IsOn;
            }
        }

        private void toggleRestrictedSession_Click(object sender, EventArgs e)
        {
            if (_rw != null)
            {
                _rw.RestrictedSession = toggleRestrictedSession.IsOn;
            }
        }

        private void toggleRestrictedWorkflow_Click(object sender, EventArgs e)
        {
            if (_rw != null)
            {
                _rw.RestrictedWorkflow = toggleRestrictedWorkflow.IsOn;
            }
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
                selectedApiMethod = iRadWhereMethodInfos.FirstOrDefault(m => m.Name == cmbApiMethod.SelectedItem.ToString());
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
                    case nameof(_rw.OpenReport):
                        OpenReport(paramValues);
                        break;
                    case nameof(_rw.CloseReport):
                        CloseReport(paramValues);
                        break;
                    case nameof(_rw.CancelReport):
                        CancelReport(paramValues);
                        break;
                    case nameof(_rw.SaveReport):
                        SaveReport(paramValues);
                        break;
                    case nameof(_rw.AssociateOrders):
                        AssociateOrders(paramValues);
                        break;
                    case nameof(_rw.DissociateOrders):
                        DissociateOrders(paramValues);
                        break;
                    case nameof(_rw.AssociateOrdersEx):
                        AssociateOrdersEx(paramValues);
                        break;
                    case nameof(_rw.PreviewOrders):
                        PreviewOrders(paramValues);
                        break;
                    case nameof(_rw.InsertAutoText):
                        InsertAutoText(paramValues);
                        break;
                    case nameof(_rw.Login):
                        Login(paramValues);
						break;
                    case nameof(_rw.Logout):
                        Logout();
                        break;
                    case nameof(_rw.Terminate):
                        btnTerminatePSClient_Click(sender, e);
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

		private void btnExportEventLogs_Click(object sender, EventArgs e)
		{
			this.dgvEventLogging.ExportEventLogs();
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

        #endregion

        #region IRadWhere Events

        private void UserLoggedIn(object sender, LoginEventArgs e)
        {
            UserLoggedIn(e.Username);
        }

        private void UserLoggedOut(object sender, LoginEventArgs e)
        {
            UserLoggedOut(e.Username);
        }

        protected void ReportOpened(object sender, ReportEventArgs e)
        {
            ReportOpened(ConvertReportArgsToParams(e));
        }

        protected void ReportClosed(object sender, ReportEventArgs e)
        {
            ReportClosed(ConvertReportArgsToParams(e));
        }

        protected void ReportChanged(object sender, ReportEventArgs e)
        {
            ReportChanged(ConvertReportArgsToParams(e));
        }

        protected void AccessionNumbersChanged(object sender, ReportEventArgs e)
        {
            AccessionNumbersChanged(ConvertReportArgsToParams(e));
        }

        protected void PrefetchRequested(object sender, EventArgs e)
        {
            PrefetchRequested();
        }

        private void Terminated(object sender, EventArgs e)
        {
            Terminated();
        }

        protected void DictationStarted(object sender, EventArgs e)
        {
            DictationStarted();
        }

        protected void DictationStopped(object sender, EventArgs e)
        {
            DictationStopped();
        }

        protected void AudioTranscribed(object sender, AudioTranscribedEventArgs e)
        {
            AudioTranscribed(e.TextRecognized);
        }

        #endregion

        #region IRadWhere Methods

        private void OpenReport(Dictionary<string, object> paramValues)
        {
            try
            {
                AddClientEventLog($"{nameof(OpenReport)} Pending...", EventTypeEnum.APIRequest);

                var success = _rw.OpenReport(paramValues.ElementAt(0).Value.ToString(),
                                        ConvertObjectToArray(paramValues.ElementAt(1).Value),
                                        paramValues.ElementAt(2).Value.ToString());

                AddClientEventLog($"{nameof(OpenReport)} {(success ? "Succeeded" : "Failed")}.", EventTypeEnum.APIResponse);
            }
            catch (Exception ex)
            {
                AddClientEventLog($"{nameof(OpenReport)} Error: {ex}", EventTypeEnum.Error);
            }
        }

        private void CloseReport(Dictionary<string, object> paramValues)
        {
            try
            {
                AddClientEventLog($"{nameof(CloseReport)} Pending...", EventTypeEnum.APIRequest);

                var success = _rw.CloseReport(Convert.ToBoolean(paramValues.ElementAt(0).Value),
                    Convert.ToBoolean(paramValues.ElementAt(1).Value));

                AddClientEventLog($"{nameof(CloseReport)} {(success ? "Succeeded" : "Failed")}.", EventTypeEnum.APIResponse);
            }
            catch (Exception ex)
            {
                AddClientEventLog($"{nameof(CloseReport)} Error: {ex}", EventTypeEnum.Error);
            }
        }

        private void CancelReport(Dictionary<string, object> paramValues)
        {
            try
            {
                AddClientEventLog($"{nameof(CancelReport)} Pending...", EventTypeEnum.APIRequest);

                var success = _rw.CancelReport(Convert.ToBoolean(paramValues.ElementAt(0).Value));

                AddClientEventLog($"{nameof(CancelReport)} {(success ? "Succeeded" : "Failed")}.", EventTypeEnum.APIResponse);
            }
            catch (Exception ex)
            {
                AddClientEventLog($"{nameof(CancelReport)} Error: {ex}", EventTypeEnum.Error);
            }
        }

        private void SaveReport(Dictionary<string, object> paramValues)
        {
            try
            {
                AddClientEventLog($"{nameof(SaveReport)} Pending...", EventTypeEnum.APIRequest);

                var success = _rw.SaveReport(Convert.ToBoolean(paramValues.ElementAt(0).Value));

                AddClientEventLog($"{nameof(SaveReport)} {(success ? "Succeeded" : "Failed")}.", EventTypeEnum.APIResponse);
            }
            catch (Exception ex)
            {
                AddClientEventLog($"{nameof(SaveReport)} Error: {ex}", EventTypeEnum.Error);
            }
        }

        private void AssociateOrders(Dictionary<string, object> paramValues)
        {
            try
            {
                AddClientEventLog($"{nameof(AssociateOrders)} Pending...", EventTypeEnum.APIRequest);

                var numOfOrdersAssociated = _rw.AssociateOrders(ConvertObjectToArray(paramValues.ElementAt(0).Value));
               
                AddClientEventLog($"There are {numOfOrdersAssociated} number of orders/accessions associated by AssociateOrders.", EventTypeEnum.APIResponse);
                
                if(numOfOrdersAssociated > 0 && cmbApiProperty.SelectedItem.ToString() == IRadWhereApiPropertiesEnum.AccessionNumbers.ToString())
                    txtApiPropertyValue.Text = _rw?.AccessionNumbers != null ? string.Join(",", _rw.AccessionNumbers) : string.Empty;
            }
            catch (Exception ex)
            {
                AddClientEventLog($"{nameof(AssociateOrders)} Error: {ex}", EventTypeEnum.Error);
            }
        }

        private void DissociateOrders(Dictionary<string, object> paramValues)
        {
            try
            {
                AddClientEventLog($"{nameof(DissociateOrders)} Pending...", EventTypeEnum.APIRequest);

                var orderNumbers = ConvertObjectToArray(paramValues.ElementAt(0).Value);
                var success = _rw.DissociateOrders(orderNumbers);
                
                AddClientEventLog(string.Format("DissociateOrders {0} {1}", string.Join(",", orderNumbers), success ? "succeeded" : "failed"), EventTypeEnum.APIResponse);
                
                if (success && cmbApiProperty.SelectedItem.ToString() == IRadWhereApiPropertiesEnum.AccessionNumbers.ToString())
                    txtApiPropertyValue.Text = _rw?.AccessionNumbers != null ? string.Join(",", _rw.AccessionNumbers) : string.Empty;
            }
            catch (Exception ex)
            {
                AddClientEventLog($"{nameof(DissociateOrders)} Error: {ex}", EventTypeEnum.Error);
            }
        }

        private void AssociateOrdersEx(Dictionary<string, object> paramValues)
        {
            try
            {
                var siteName = paramValues.ElementAt(0).Value.ToString();
                var accNum = paramValues.ElementAt(1).Value.ToString();
                var orderNumbsers = ConvertObjectToArray(paramValues.ElementAt(2).Value);

                AddClientEventLog($"{nameof(AssociateOrdersEx)} Pending...", EventTypeEnum.APIRequest);
                var numOfOrdersAssociated = _rw.AssociateOrdersEx(siteName, accNum, orderNumbsers);
                AddClientEventLog($"There are {numOfOrdersAssociated} orders/accessions affected by AssociateOrdersEx.", EventTypeEnum.APIResponse);
            }
            catch (Exception ex)
            {
                AddClientEventLog($"{nameof(AssociateOrdersEx)} Error: {ex}", EventTypeEnum.Error);
            }
        }

        private void PreviewOrders(Dictionary<string, object> paramValues)
        {
            try
            {
                var siteName = paramValues.ElementAt(0).Value.ToString();
                if (siteName?.Trim().ToLower() == "null")
                    siteName = null;
                var accNumbsers = ConvertObjectToArray(paramValues.ElementAt(1).Value);
                var patientIdentifier = paramValues.ElementAt(2).Value.ToString();

                AddClientEventLog($"{nameof(PreviewOrders)} Pending...", EventTypeEnum.APIRequest);

                var success = _rw.PreviewOrders(siteName, accNumbsers, patientIdentifier);

                AddClientEventLog($"{nameof(PreviewOrders)} {(success ? "Succeeded" : "Failed")}.", EventTypeEnum.APIResponse);
            }
            catch (Exception ex)
            {
                AddClientEventLog($"{nameof(PreviewOrders)} Error: {ex}", EventTypeEnum.Error);
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
                var success = _rw.InsertAutoText(autoTextName, Convert.ToBoolean(paramValues.ElementAt(1).Value));
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
				if (_rw == null)
				{
					AddClientEventLog("You must connect to PS client before Login.", EventTypeEnum.Error);
					return;
				}

				var username = paramValues.ElementAt(0).Value.ToString();
				var password = paramValues.ElementAt(1).Value.ToString();

				AddClientEventLog("Login Pending...", EventTypeEnum.APIRequest);
				var success = _rw.Login(username, password);
				AddClientEventLog($"Login {(success ? "Succeeded" : "Failed")}.", EventTypeEnum.APIResponse);
			}
			catch (Exception ex)
			{
				AddClientEventLog($"Login Error: {ex}", EventTypeEnum.Error);
			}
		}

		private void Logout()
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				if (_rw == null)
				{
					AddClientEventLog("Cannot logout from PowerScribe Client, since it is not connected.", EventTypeEnum.Error);
					return;
				}
				AddClientEventLog("Logout Pending...", EventTypeEnum.APIRequest);
				var success = _rw.Logout();
				AddClientEventLog($"Logout {(success ? "Succeeded" : "Failed")}.", EventTypeEnum.APIResponse);
			}
			catch (Exception ex)
			{
				AddClientEventLog($"Logout Error: {ex}", EventTypeEnum.Error);
			}
		}

		#endregion

		#region Private Methods

		private void Start(ref bool started)
        {
			AddClientEventLog("Start...");

			// Try to connect without launching RadWhere. If that fails, we'll launch
			started = Connect();

			if (started == false)
            {
				try
				{
					//launch RadWhere (if not already running)
					try
					{
						started = LaunchPSClient();
					}
					catch (Exception ex)
					{
						AddClientEventLog("The specified launch point to PowerScribe may be invalid. An exception occurred attempting to start the process:\r\n" + ex.ToString(), EventTypeEnum.Error);
					}
					if (started)
					{
						AddClientEventLog("PowerScribe launched successfully.", EventTypeEnum.APIResponse);
						AddClientEventLog("Attempting to connect to PowerScribe...");

						const int interval = 2; //seconds between retries
						TimeSpan timeout = new TimeSpan(0, 0, 60); // max time before giving up
						TimeSpan elapsed = new TimeSpan(0, 0, 0);

						// Connect to PS360. When we have successfully connected, the 
						// application is ready to accept logins.
						DateTime firstAttemptTime = DateTime.Now;
						DateTime attemptTime = DateTime.Now;
						bool connected = Connect();
						while (!connected && elapsed < timeout)
						{
							System.Windows.Forms.Application.DoEvents();
							elapsed = DateTime.Now - firstAttemptTime;
							TimeSpan lastAttempt = DateTime.Now - attemptTime;
							AddClientEventLog(String.Format("Last connection attempt: {0} seconds ago; total elapsed time: {1} seconds",
								lastAttempt.Seconds, elapsed.Seconds));
							if (lastAttempt.Seconds < interval)
							{
								System.Threading.Thread.Sleep((interval * 1000) - lastAttempt.Milliseconds);
							}
							attemptTime = DateTime.Now;
							try { connected = Connect(); }
							catch (Exception ex)
							{
								AddClientEventLog("Error connecting to PowerScribe: " + ex.ToString(), EventTypeEnum.Error);
								started = false;
								return;
							}
						}
						if (elapsed >= timeout)
						{
							AddClientEventLog("Timeout occurred waiting for PowerScribe to accept connection request", EventTypeEnum.Error);
							started = false;
							return;
						}
						else
						{
							AddClientEventLog("Connected to PowerScribe successfully.");
						}
					}
				}
				catch (Exception e)
				{
					started = false;
					AddClientEventLog("Error starting PowerScribe: " + e.Message, EventTypeEnum.Error);
				}
			}



		}

        private void Initialize()
        {
            try
            {
                RegistryView regView = RegistryView.Default;
                if (Environment.Is64BitOperatingSystem)
                {
                    AddClientEventLog("Windows OS: 64 bit");
                    if (Environment.Is64BitProcess)
                    {
                        AddClientEventLog("Process: 64 bit");
                        regView = RegistryView.Registry64;
                    }
                    else
                    {
                        AddClientEventLog("Process: 32 bit");
                    }
                }
                else
                {
                    AddClientEventLog("Windows OS and process: 32 bit");
                }

                RegistryKey regkey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, regView).OpenSubKey(@"Software\Nuance\RadWhereCOM");
                if (null != regkey)
                {
                    _remotePort = (string)regkey.GetValue("RemotePort");
                    if (string.IsNullOrEmpty(_remotePort)) _remotePort = "9090";

                    _remoteHost = (string)regkey.GetValue("RemoteHost");
                    if (string.IsNullOrEmpty(_remoteHost)) _remoteHost = "localhost";

                    _localHost = (string)regkey.GetValue("LocalHost");
                    if (string.IsNullOrEmpty(_localHost)) _localHost = "";

                    _location = (string)regkey.GetValue("Location");
                    if (String.IsNullOrEmpty(_location))
                        _location = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), "PowerScribe One.lnk");


                    AddClientEventLog("LocalHost: " + _localHost);
                }
                else
                {
					AddClientEventLog("RadWhereCOM did not initialize properly. One or more configuration options are missing from the system registry. Re-install the RadWhereConnectorSetup.", EventTypeEnum.Error);
				}
			}
            catch
            {
				AddClientEventLog("RadWhereCOM did not initialize properly. One or more configuration options are missing from the system registry. Re-install the RadWhereConnectorSetup.", EventTypeEnum.Error);
			}

			AddClientEventLog(string.Format("Initialized successfully. Configuration options:\r\nremoting port:{0}; remoting host:{1}; local host:{2}",
				_remotePort, _remoteHost, _localHost));
		}

		private bool Connect()
        {
            try
            {
                AddClientEventLog("Connection Pending...");

                //register channel
                if (null == _channel)
                {
                    //register channel and connect to server
                    IDictionary props = new ListDictionary
                    {
                        ["port"] = 0, // have the Remoting system pick a unique listening port (required for callbacks)
                        ["name"] = string.Empty // have the Remoting system pick a unique name
                    };
                    if (!string.IsNullOrEmpty(_localHost))
                    {
                        props["bindTo"] = _localHost;
                    }

                    BinaryServerFormatterSinkProvider serverProv = new BinaryServerFormatterSinkProvider();
                    serverProv.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
                    _channel = new TcpChannel(props, new BinaryClientFormatterSinkProvider(), serverProv);
                    ChannelServices.RegisterChannel(_channel, true);
                }

                _rw = (Nuance.RadWhere.IRadWhere)Activator.GetObject(typeof(Nuance.RadWhere.IRadWhere), string.Format("tcp://{0}:{1}/Nuance.RadWhere", _remoteHost, _remotePort));

                if (!SetupIRadWhereEventHandlers())
                {
                    return false;
                }

				//Populate API Methods
				List<string> methodNames = new List<string>() { "-- Select API Method --" };
				iRadWhereMethodInfos.Clear();
				iRadWhereMethodInfos = typeof(Nuance.RadWhere.IRadWhere).GetApiMethodInfos().ToList();
				methodNames.AddRange(iRadWhereMethodInfos.Select(m => m.Name)
					.OrderBy(m => m)
                    .ToList());
                cmbApiMethod.DataSource = methodNames;

                //Populate API Events
                apiEvents.Clear();
				apiEvents.AddRange(typeof(Nuance.RadWhere.IRadWhere).GetApiEvents()
	                .Select(e => new PSEvent
	                {
		                EventName = e.Name
	                }));

				//Populate API Properties
				cmbApiProperty.DataSource = typeof(IRadWhereApiPropertiesEnum).GetApiProperties();
                
                //Set controls
                ccbEventsFilter.Items.Clear();
                apiEvents.ForEach(e => ccbEventsFilter.Items.Add(e.EventName));
                toggleMinimized.IsOn = _rw.Minimized;
                toggleAlwaysOnTop.IsOn = _rw.AlwaysOnTop;
                toggleRestrictedSession.IsOn = _rw.RestrictedSession;
                toggleRestrictedWorkflow.IsOn = _rw.RestrictedWorkflow;
                Disconnect += IRadWhereDisconnect;
                SetIRadWhereUIState();

                AddClientEventLog("Successfully connected to PowerScribe.", EventTypeEnum.APIResponse);
                return true;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _rw = null;
				_channel = null;

                if (ex.Message.ToUpper().Contains("REFUSED"))
                {
					AddClientEventLog("Connect(): SocketException Message REFUSED.");
                }
                else
                {
                    AddClientEventLog($"An unexpected exception occurred when connecting to PowerScribe {ex}.");
                }

                return false;
            }
            catch (Exception ex)
            {
                _rw = null;
                //TTP 4086
                if (_skipConnectException)
                {
                    _skipConnectException = false;
                    _retryConnect = true;
                    return false;
                }
                AddClientEventLog(ex.ToString());
                return false;
            }
        }


        private bool LaunchPSClient()
        {
			try
            {
				// Assume started
				bool started = false;
				ProcessStartInfo sInfo;
				if (_location.ToLower().StartsWith("http://") || _location.ToLower().StartsWith("https://"))    // Web launch must be done with rundll32 so that non-IE browsers will work too (TTP 5528).
				{
					sInfo = new ProcessStartInfo("rundll32.exe", "dfshim.dll,ShOpenVerbApplication " + _location);
					AddClientEventLog("Launching PowerScribe web browser process using command line: " + "rundll32.exe dfshim.dll,ShOpenVerbApplication " + _location);
				}
				else
				{
					sInfo = new ProcessStartInfo(_location, "");
					AddClientEventLog("Launching PowerScribe process using file: " + _location);
				}
				Process p = Process.Start(sInfo);
				started = true;
				return started;
			}
			catch (Exception ex)
            {
				AddClientEventLog("Error launching PowerScribe: " + ex.Message, EventTypeEnum.Error);
				return false;
			}
		}

        private void IRadWhereAddEventLog(object sender, Common.Models.EventLog eventLog)
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

        private bool SetupIRadWhereEventHandlers(List<string> selectedEvents = null)
        {
            try
            {
                if (_rw == null)
                {
                    AddClientEventLog("Not connected to PS Client, for setting IRadWhere event handlers.");
                    return false;
                }

                if (selectedEvents != null)
                    UnSubscribeIRadWhereEvents();

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rw.UserLoggedIn)) != null)
                {
                    _userLoggedIn_w = new UserLoggedInEventWrapper();
                    _userLoggedIn_w.UserLoggedInArrivedLocally += new UserLoggedInHandler(UserLoggedIn);
                    _rw.UserLoggedIn += new UserLoggedInHandler(_userLoggedIn_w.LocallyHandleUserLoggedIn);
                }

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rw.UserLoggedOut)) != null)
                {
                    _userLoggedOut_w = new UserLoggedOutEventWrapper();
                    _userLoggedOut_w.UserLoggedOutArrivedLocally += new UserLoggedOutHandler(UserLoggedOut);
                    _rw.UserLoggedOut += new UserLoggedOutHandler(_userLoggedOut_w.LocallyHandleUserLoggedOut);
                }

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rw.ReportClosed)) != null)
                {
                    _reportClosed_w = new ReportClosedEventWrapper();
                    _reportClosed_w.ReportClosedArrivedLocally += new ReportClosedHandler(ReportClosed);
                    _rw.ReportClosed += new ReportClosedHandler(_reportClosed_w.LocallyHandleReportClosed);
                }

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rw.ReportChanged)) != null)
                {
                    _reportChanged_w = new ReportChangedEventWrapper();
                    _reportChanged_w.ReportChangedArrivedLocally += new ReportChangedHandler(ReportChanged);
                    _rw.ReportChanged += new ReportChangedHandler(_reportChanged_w.LocallyHandleReportChanged);
                }

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rw.ReportOpened)) != null)
                {
                    _reportOpened_w = new ReportOpenedEventWrapper();
                    _reportOpened_w.ReportOpenedArrivedLocally += new ReportOpenedHandler(ReportOpened);
                    _rw.ReportOpened += new ReportOpenedHandler(_reportOpened_w.LocallyHandleReportOpened);
                }

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rw.AccessionNumbersChanged)) != null)
                {
                    _accessionNumbersChanged_w = new AccessionNumbersChangedEventWrapper();
                    _accessionNumbersChanged_w.AccessionNumbersChangedArrivedLocally += new AccessionNumbersChangedHandler(AccessionNumbersChanged);
                    _rw.AccessionNumbersChanged += new AccessionNumbersChangedHandler(_accessionNumbersChanged_w.LocallyHandleAccessionNumbersChanged);
                }

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rw.PrefetchRequested)) != null)
                {
                    _prefetchRequested_w = new PrefetchRequestedEventWrapper();
                    _prefetchRequested_w.PrefetchRequestedArrivedLocally += new PrefetchRequestedHandler(PrefetchRequested);
                    _rw.PrefetchRequested += new PrefetchRequestedHandler(_prefetchRequested_w.LocallyHandlePrefetchRequested);
                }

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rw.Terminated)) != null)
                {
                    _terminated_w = new TerminatedEventWrapper();
                    _terminated_w.TerminatedArrivedLocally += new TerminatedHandler(Terminated);
                    _rw.Terminated += new TerminatedHandler(_terminated_w.LocallyHandleTerminated);
                }

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rw.DictationStarted)) != null)
                {
                    _dictationStarted_w = new DictationStartedEventWrapper();
                    _dictationStarted_w.DictationStartedArrivedLocally += new DictationStartedHandler(DictationStarted);
                    _rw.DictationStarted += new DictationStartedHandler(_dictationStarted_w.LocallyHandleDictationStarted);
                }

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rw.DictationStopped)) != null)
                {
                    _dictationStopped_w = new DictationStoppedEventWrapper();
                    _dictationStopped_w.DictationStoppedArrivedLocally += new DictationStoppedHandler(DictationStopped);
                    _rw.DictationStopped += new DictationStoppedHandler(_dictationStopped_w.LocallyHandleDictationStopped);
                }

                if (selectedEvents == null || selectedEvents.FirstOrDefault(e => e == nameof(_rw.AudioTranscribed)) != null)
                {
                    _audioTranscribed_w = new AudioTranscribedEventWrapper();
                    _audioTranscribed_w.AudioTranscribedArrivedLocally += new AudioTranscribedHandler(AudioTranscribed);
                    _rw.AudioTranscribed += new AudioTranscribedHandler(_audioTranscribed_w.LocallyHandleAudioTranscribed);
                }
                AddClientEventLog("Added/updated event subscriptions.", EventTypeEnum.APIResponse);
            }
            catch (Exception ex)
            {
                AddClientEventLog($"Error occured while setting IRadWhere event handlers. {ex}");
                return false;
            }

            return true;
        }

        private void IRadWhereDisconnect(object sender, EventArgs e)
        {
            if (_rw == null)
                return;

            try
            {
                UnSubscribeIRadWhereEvents();

                ChannelServices.UnregisterChannel(_channel);
                RemotingServices.Disconnect(this);

                _rw = null;
                _channel = null;

                SetIRadWhereUIState();
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

        private void UnSubscribeIRadWhereEvents()
        {
            try
            {
                _rw.DictationStarted -= _dictationStarted_w.LocallyHandleDictationStarted;
                _rw.DictationStopped -= _dictationStopped_w.LocallyHandleDictationStopped;
                _rw.PrefetchRequested -= _prefetchRequested_w.LocallyHandlePrefetchRequested;
                _rw.ReportChanged -= _reportChanged_w.LocallyHandleReportChanged;
                _rw.ReportClosed -= _reportClosed_w.LocallyHandleReportClosed;
                _rw.ReportOpened -= _reportOpened_w.LocallyHandleReportOpened;
                _rw.Terminated -= _terminated_w.LocallyHandleTerminated;
                _rw.UserLoggedIn -= _userLoggedIn_w.LocallyHandleUserLoggedIn;
                _rw.UserLoggedOut -= _userLoggedOut_w.LocallyHandleUserLoggedOut;
                _rw.AudioTranscribed -= _audioTranscribed_w.LocallyHandleAudioTranscribed;
                _rw.AccessionNumbersChanged -= _accessionNumbersChanged_w.LocallyHandleAccessionNumbersChanged;
            }
            catch (Exception ex)
            {
				AddClientEventLog(ex.ToString(), EventTypeEnum.Error);
			}
		}

		private void SetIRadWhereUIState()
        {
            var connected = _rw != null;

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

        private ReportParams ConvertReportArgsToParams(ReportEventArgs e)
        {
            return new ReportParams
            {
                SiteName = e.SiteName,
                AccessionNumbers = e.AccessionNumbers,
                Status = (ReportStatus)e.Status,
                IsAddendum = e.IsAddendum,
                PlainText = e.PlainText,
                RichText = e.RichText,
                ImpendingStatus = (ReportStatus)e.Status,
                SentAsPreliminary = e.SentAsPreliminary,
                SignerUsername = e.Signer?.Username,
                SignerSiteIdentifier = e.Signer?.SiteIdentifier ?? string.Empty,
                DictatorUsername = e.Dictator?.Username,
                DictatorSiteIdentifier = e.Dictator?.SiteIdentifier ?? string.Empty
            };
        }

        #endregion
    }
}
