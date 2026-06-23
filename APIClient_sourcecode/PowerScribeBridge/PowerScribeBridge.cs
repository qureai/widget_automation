using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using RadWhereAxLib;

namespace PowerScribeBridge
{
    public class Startup
    {
        private static RadWhereCtrl _rwAx = null;
        private static Action<object> _callback;

        public async Task<object> Invoke(dynamic input)
        {
            try
            {
                if (input == null)
                {
                    return new { success = false, error = "Input is null" };
                }

                var action = input.action as string;
                if (string.IsNullOrEmpty(action))
                {
                    return new { success = false, error = "Action is null or empty" };
                }

                // Store callback for event handling - try multiple approaches for edge-js compatibility
                _callback = null;
                if (input.callback != null)
                {
                    try
                    {
                        // Try casting as Action<object> first
                        _callback = input.callback as Action<object>;
                        if (_callback == null)
                        {
                            // Try using dynamic invocation for edge-js
                            _callback = new Action<object>(obj => {
                                try
                                {
                                    input.callback(obj);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error calling callback: {ex}");
                                }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error setting up callback: {ex}");
                    }
                }
                
                Console.WriteLine($"Callback stored: {_callback != null}");
                
                Console.WriteLine($"Invoke called with action: {action}");
                
                switch (action)
                {
                    case "connect":
                        return await Connect();
                    case "disconnect":
                        return await Disconnect();
                    default:
                        return new { success = false, error = $"Unknown action: {action}" };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Invoke: {ex}");
                return new { success = false, error = $"Error in Invoke: {ex.Message}" };
            }
        }

        private async Task<object> Connect()
        {
            try
            {
                Console.WriteLine("Connect called");
                
                if (_rwAx != null)
                {
                    Console.WriteLine("Already connected");
                    return new { success = true, message = "Already connected" };
                }

                Console.WriteLine("Creating new RadWhereCtrl");
                _rwAx = new RadWhereCtrl();
                
                Console.WriteLine("Registering event handlers");
                _rwAx.ReportClosed += new _IRadWhereCtrlEvents_ReportClosedEventHandler(ReportClosed);
                _rwAx.ReportOpened += new _IRadWhereCtrlEvents_ReportOpenedEventHandler(ReportOpened);
                _rwAx.UserLoggedIn += new _IRadWhereCtrlEvents_UserLoggedInEventHandler(UserLoggedIn);
                _rwAx.UserLoggedOut += new _IRadWhereCtrlEvents_UserLoggedOutEventHandler(UserLoggedOut);
                
                Console.WriteLine("Starting RadWhere connection");
                var success = _rwAx.Start();
                if (!success)
                {
                    Console.WriteLine("Failed to start RadWhere connection");
                    _rwAx = null;
                    return new { success = false, error = "Failed to start RadWhere connection" };
                }

                Console.WriteLine("Connected successfully");
                return new { success = true, message = "Connected successfully" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Connect: {ex}");
                if (_rwAx != null)
                {
                    try
                    {
                        _rwAx.ReportClosed -= new _IRadWhereCtrlEvents_ReportClosedEventHandler(ReportClosed);
                        _rwAx.ReportOpened -= new _IRadWhereCtrlEvents_ReportOpenedEventHandler(ReportOpened);
                        _rwAx.UserLoggedIn -= new _IRadWhereCtrlEvents_UserLoggedInEventHandler(UserLoggedIn);
                        _rwAx.UserLoggedOut -= new _IRadWhereCtrlEvents_UserLoggedOutEventHandler(UserLoggedOut);
                    }
                    catch (Exception unregEx)
                    {
                        Console.WriteLine($"Error unregistering event handlers: {unregEx}");
                    }
                    _rwAx = null;
                }
                return new { success = false, error = $"Error in Connect: {ex.Message}" };
            }
        }

        private async Task<object> Disconnect()
        {
            try
            {
                Console.WriteLine("Disconnect called");
                
                if (_rwAx == null)
                {
                    Console.WriteLine("Already disconnected");
                    return new { success = true, message = "Already disconnected" };
                }

                Console.WriteLine("Unregistering event handlers");
                try
                {
                    _rwAx.ReportClosed -= new _IRadWhereCtrlEvents_ReportClosedEventHandler(ReportClosed);
                    _rwAx.ReportOpened -= new _IRadWhereCtrlEvents_ReportOpenedEventHandler(ReportOpened);
                    _rwAx.UserLoggedIn -= new _IRadWhereCtrlEvents_UserLoggedInEventHandler(UserLoggedIn);
                    _rwAx.UserLoggedOut -= new _IRadWhereCtrlEvents_UserLoggedOutEventHandler(UserLoggedOut);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error unregistering event handlers: {ex}");
                }
                
                _rwAx = null;
                _callback = null;
                Console.WriteLine("Disconnected successfully");
                return new { success = true, message = "Disconnected successfully" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Disconnect: {ex}");
                return new { success = false, error = $"Error in Disconnect: {ex.Message}" };
            }
        }

        private void ReportClosed(string siteName, string accessionNumber, int status, bool isAddendum, string plainText, string richText)
        {
            try
            {
                Console.WriteLine($"ReportClosed called with SiteName: {siteName}, AccessionNumber: {accessionNumber}");
                if (_callback != null)
                {
                    var eventData = new
                    {
                        type = "ReportClosed",
                        siteName = siteName ?? string.Empty,
                        accessionNumber = accessionNumber ?? string.Empty,
                        status = status,
                        isAddendum = isAddendum,
                        plainText = plainText ?? string.Empty,
                        richText = richText ?? string.Empty,
                        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                    _callback(eventData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ReportClosed: {ex}");
            }
        }

        private void ReportOpened(string siteName, string accessionNumber, int status, bool isAddendum, string plainText, string richText)
        {
            try
            {
                Console.WriteLine($"ReportOpened called with SiteName: {siteName}, AccessionNumber: {accessionNumber}");
                if (_callback != null)
                {
                    var eventData = new
                    {
                        type = "ReportOpened",
                        siteName = siteName ?? string.Empty,
                        accessionNumber = accessionNumber ?? string.Empty,
                        status = status,
                        isAddendum = isAddendum,
                        plainText = plainText ?? string.Empty,
                        richText = richText ?? string.Empty,
                        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                    _callback(eventData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ReportOpened: {ex}");
            }
        }

        private void UserLoggedIn(string username)
        {
            try
            {
                Console.WriteLine($"UserLoggedIn called with Username: {username}");
                if (_callback != null)
                {
                    var eventData = new
                    {
                        type = "UserLoggedIn",
                        username = username ?? string.Empty,
                        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                    _callback(eventData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UserLoggedIn: {ex}");
            }
        }

        private void UserLoggedOut(string username)
        {
            try
            {
                Console.WriteLine($"UserLoggedOut called with Username: {username}");
                if (_callback != null)
                {
                    var eventData = new
                    {
                        type = "UserLoggedOut",
                        username = username ?? string.Empty,
                        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                    _callback(eventData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UserLoggedOut: {ex}");
            }
        }
    }
} 