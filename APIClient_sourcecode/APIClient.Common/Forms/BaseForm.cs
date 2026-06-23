using APIClient.Common.Enums;
using APIClient.Common.Extensions;
using APIClient.Common.Helpers;
using APIClient.Common.Models;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace APIClient.Common.Forms
{
    public class BaseForm : Form
    {
        protected const string CMB_API_PROPERTY_DEFAULT_VALUE = "-- Select API Property --";
        protected const string CMB_API_METHOD_DEFAULT_VALUE = "-- Select API Method --";
        protected const string CCB_EVENTS_FILTER_DEFAULT_TEXT = "Select Events To Filter";
        protected const string CCB_EVENTS_FILTER_CHECKED_TEXT = "Events Filtered";
        protected bool _skipConnectException = false; //TTP 4086
        protected bool _retryConnect = false; //TTP 4086
        protected List<PSEvent> apiEvents = new List<PSEvent>();
        protected List<EventLog> eventLogs = new List<EventLog>();

        #region Event Handlers

        protected event DisconnectEventHandler Disconnect;
        protected delegate void DisconnectEventHandler(object sender, EventArgs e);

        protected event AddLogHandler AddLogToClientDataGrid;
        protected delegate void AddLogHandler(object sender, EventLog eventLog);

        protected delegate void AddLogDelegate(EventLog eventLog);

        #endregion

        #region IRadWhereEvents

        protected void BaseForm_Load(object sender, EventArgs e)
        {
            ConfigureLogger();
        }

        protected void UserLoggedIn(string username)
        {
            AddPSEventLog("UserLoggedIn: userName: " + username);
        }

        protected void UserLoggedOut(string username)
        {
            AddPSEventLog("UserLoggedOut: userName: " + username);
        }

        protected void OpenStudy(string mrn, string accessionNumber)
        {
			AddPSEventLog("OpenStudy: AccessionNumber: " + accessionNumber);
		}

        protected void ReportOpened(ReportParams e)
        {
            AddPSEventLog($"ReportOpened {ReportParamsToString(e)}", e);
        }

        protected void ReportClosed(ReportParams e)
        {
            AddPSEventLog($"ReportClosed {ReportParamsToString(e)}", e);
        }

        protected void ReportClosed2(ReportParams e)
        {
            AddPSEventLog($"ReportClosed2 {ReportParamsToString(e, true)}", e);
        }

        protected void ReportClosed3(ReportParams e)
        {
            AddPSEventLog($"ReportClosed3 {ReportParamsToString(e, true)}", e);
        }

        protected void ReportChanged(ReportParams e)
        {
            AddPSEventLog($"ReportChanged {ReportParamsToString(e)}", e);
        }

        protected void AccessionNumbersChanged(ReportParams e)
        {
            AddPSEventLog($"AccessionNumbersChanged  {ReportParamsToString(e)}", e);
        }

        protected void PrefetchRequested()
        {
            AddPSEventLog("PrefetchRequested");
        }

        protected void DictationStarted()
        {
            AddPSEventLog("DictationStarted");
        }

        protected void DictationStopped()
        {
            AddPSEventLog("DictationStopped");
        }

        protected void AudioTranscribed(bool textRecognized)
        {
            AddPSEventLog(string.Format("AudioTranscribed {0} text", textRecognized ? "with" : "without"));
        }

        protected void ReportFinished(int reportStatus)
        {
            ReportFinishedEnum reportFinishedEnum = (ReportFinishedEnum)reportStatus;

            // Logging the report finished status in user friendly format aka description.
			AddPSEventLog($"ReportFinished Status: {reportFinishedEnum.GetDescription()} ({reportStatus})");            
		}

		protected void ErrorOccurred(int errCode, string message)
        {
            AddPSEventLog(string.Format("Erroroccurred. Error code: {0} Message:{1}", errCode, message));
        }

        protected void Terminated()
        {
            //TTP 4086
            _skipConnectException = true;
            AddPSEventLog("PowerScribe Client Terminated.");
            if (Disconnect != null)
            {
                Disconnect(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Protected Methods

        protected void AddClientEventLog(string message, EventTypeEnum eventType = EventTypeEnum.Default)
        {
            var eventLog = new EventLog
            {
                EventType = eventType, 
                Message = message
            };
            if (InvokeRequired)
            {
                var outputDelegate = new AddLogDelegate(AddLogD);
                Invoke(outputDelegate, new object[] { eventLog });
            }
            else
                AddLogD(eventLog);
        }

        protected string ReportParamsToString(ReportParams e, bool includeImpedingStatus = false)
        {
            string statusName = string.Format("{0} ({1})", (e.Status >= 0) ? e.Status.ToString() : "Discarded", (int)e.Status);
			string impedingStatueName = (e.ImpendingStatus >= 0) ? e.ImpendingStatus.ToString() : "Discarded";

            string result = string.Format("Status: {0}; Accessions: {1}; Addendum: {2}; Patient ID: {3}; Site Name: {4}; SentAsPrelim: {5}; Signer: {6}; Dictator: {7}",
                statusName,
                string.Join(",", e.AccessionNumbers),
                e.IsAddendum,
                e.PatientIdentifier ?? string.Empty,
                e.SiteName ?? string.Empty,
                e.SentAsPreliminary,
                !string.IsNullOrWhiteSpace(e.SignerUsername) ? $"{e.SignerUsername}/{e.SignerSiteIdentifier}" : "null",
                !string.IsNullOrWhiteSpace(e.DictatorUsername) ? $"{e.DictatorUsername}/{e.DictatorSiteIdentifier}" : "null");

            if (includeImpedingStatus)
                result += string.Format("; ImpendingStatus: {0} ({1})", impedingStatueName, (int)e.ImpendingStatus);

            return result;
        }

        protected Dictionary<string, object> GetParamValues(ParameterInfo[] parameters, Panel pnlParameters)
        {
            var paramValueDictionary = new Dictionary<string, object>();

            foreach (var param in parameters)
            {
                if (param.ParameterType == typeof(string))
                {
                    paramValueDictionary.Add(param.Name, ((TextBox)pnlParameters.Controls[$"txt{param.Name}Param"]).Text);
                }
                else if (param.ParameterType == typeof(string[]))
                {
                    paramValueDictionary.Add(param.Name, ((TextBox)pnlParameters.Controls[$"txt{param.Name}Param"]).Text.Split(',').Select(a => a.Trim()).ToArray());
                }
                else if (param.ParameterType == typeof(bool))
                {
                    paramValueDictionary.Add(param.Name, ((CheckBox)pnlParameters.Controls[$"cbx{param.Name}Param"]).Checked);
                }
            }

            return paramValueDictionary;
        }

        protected string[] ConvertObjectToArray(object obj)
        {
            return ((IEnumerable)obj).Cast<object>().Select(p => p.ToString()).ToArray();
        }

		#endregion

		#region Private Methods

		private void ConfigureLogger()
        {
            var appLogger = new LoggerConfiguration();
            appLogger.WriteTo.File(path: LogHelper.LogPath,
                restrictedToMinimumLevel: LogHelper.LogLevel,
                rollOnFileSizeLimit: true,
                fileSizeLimitBytes: LogHelper.FileSizeLimitBytes,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: LogHelper.RetainedFileCountLimit);
            Log.Logger = appLogger.MinimumLevel.Debug().CreateLogger();
        }

        private void AddPSEventLog(string message, ReportParams reportParams = null, [CallerMemberName] string callerMemberName = "")
        {
            var eventLog = new EventLog
            {
                EventType = EventTypeEnum.PowerScribeEvent,
                PowerScribeEventName = callerMemberName,
                Message = message,
                ReportParams = new ReportParams
                {
                    PlainText = reportParams?.PlainText,
                    RichText = reportParams?.RichText
                }
            };

            if (InvokeRequired)
            {
                var outputDelegate = new AddLogDelegate(AddLogD);
                Invoke(outputDelegate, new object[] { eventLog });
            }
            else
                AddLogD(eventLog);
        }

        private void AddLogD(EventLog eventLog)
        {
            try
            {
                eventLog.TimeStamp = DateTime.Now;

                if (AddLogToClientDataGrid != null)
                    AddLogToClientDataGrid(this, eventLog);

                switch (eventLog?.EventType)
                {
                    case EventTypeEnum.Error:
                        Log.Error(eventLog.Message);
                        break;
                    case EventTypeEnum.PowerScribeEvent:
                    case EventTypeEnum.APIRequest:
                    case EventTypeEnum.APIResponse:
                        Log.Information(eventLog.Message);
                        break;
                    default:
                        Log.Debug(eventLog.Message);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception occured while adding error message to event log. {ex}");
            }
        }

        #endregion
    }
}
