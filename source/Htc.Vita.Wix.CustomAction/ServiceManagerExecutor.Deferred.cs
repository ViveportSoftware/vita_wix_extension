using System;
using System.Runtime.InteropServices;
using Htc.Vita.Wix.CustomAction.Interop;
using Microsoft.Deployment.WindowsInstaller;

namespace Htc.Vita.Wix.CustomAction
{
    internal partial class ServiceManagerExecutor
    {
        internal class Deferred : AbstractActionExecutor
        {
            public Deferred(Session session) : base("ServiceManagerExecutor.Deferred", session)
            {
            }

            protected override ActionResult OnExecute()
            {
                var customActionData = Session.CustomActionData;
                foreach (var item in customActionData)
                {
                    ManageService(item.Key, item.Value);
                }
                return ActionResult.Success;
            }

            private void ManageService(string name, string startType)
            {
                var exists = CheckIfExistsInWindows(name);
                if (!exists)
                {
                    Log("Can not find Windows service " + name + ". Skipped");
                    return;
                }

                var type = ConvertToStartType(startType);
                var serviceInfo = QueryStartTypeInWindows(name);
                if (serviceInfo != null && serviceInfo.StartType == type)
                {
                    Log("Windows service " + name + " has the same start type: " + startType + ". Skipped");
                    return;
                }

                serviceInfo = ChangeStartTypeInWindows(name, type);
                if (serviceInfo != null && serviceInfo.StartType == type)
                {
                    Log("Windows service " + name + " has been changed start type to " + startType);
                }
            }

            private ServiceInfo ChangeStartTypeInWindows(
                    string serviceName,
                    StartType startType)
            {
                if (string.IsNullOrWhiteSpace(serviceName))
                {
                    return new ServiceInfo
                    {
                            ServiceName = serviceName,
                            ErrorCode = Windows.ERROR_INVALID_NAME,
                            ErrorMessage = "Service name \"" + serviceName + "\" is invalid"
                    };
                }

                var managerHandle = Windows.Advapi32.OpenSCManagerW(
                        null,
                        null,
                        Windows.Advapi32.SCMAccessRight.SC_MANAGER_CONNECT
                );
                if (managerHandle == IntPtr.Zero)
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    return new ServiceInfo
                    {
                            ServiceName = serviceName,
                            ErrorCode = errorCode,
                            ErrorMessage = "Can not open Windows service controller manager, error code: " + errorCode
                    };
                }

                var serviceInfo = new ServiceInfo
                {
                        ServiceName = serviceName,
                        StartType = startType
                };
                var serviceHandle = Windows.Advapi32.OpenServiceW(
                        managerHandle,
                        serviceName,
                        Windows.Advapi32.ServiceAccessRight.SERVICE_CHANGE_CONFIG |
                                Windows.Advapi32.ServiceAccessRight.SERVICE_QUERY_CONFIG |
                                Windows.Advapi32.ServiceAccessRight.SERVICE_QUERY_STATUS
                );
                if (serviceHandle == IntPtr.Zero)
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    serviceInfo.ErrorCode = errorCode;
                    serviceInfo.ErrorMessage = "Can not open Windows service \"" + serviceName + "\", error code: " + errorCode;
                }
                else
                {
                    var success = Windows.Advapi32.ChangeServiceConfigW(
                            serviceHandle,
                            Windows.Advapi32.SERVICE_TYPE.SERVICE_NO_CHANGE,
                            ConvertToWindows(startType),
                            Windows.Advapi32.ERROR_CONTROL_TYPE.SERVICE_NO_CHANGE,
                            null,
                            null,
                            IntPtr.Zero,
                            null,
                            null,
                            null,
                            null
                    );
                    if (!success)
                    {
                        var errorCode = Marshal.GetLastWin32Error();
                        serviceInfo.ErrorCode = errorCode;
                        serviceInfo.ErrorMessage = "Can not change Windows service \"" + serviceName + "\" config, error code: " + errorCode;
                    }

                    serviceInfo = UpdateCurrentStateInWindows(serviceHandle, serviceInfo);

                    Windows.Advapi32.CloseServiceHandle(serviceHandle);
                }

                Windows.Advapi32.CloseServiceHandle(managerHandle);
                return serviceInfo;
            }

            private bool CheckIfExistsInWindows(string serviceName)
            {
                if (string.IsNullOrWhiteSpace(serviceName))
                {
                    return false;
                }

                var managerHandle = Windows.Advapi32.OpenSCManagerW(
                        null,
                        null,
                        Windows.Advapi32.SCMAccessRight.SC_MANAGER_CONNECT
                );
                if (managerHandle == IntPtr.Zero)
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    Log("Can not open Windows service controller manager, error code: " + errorCode);
                    return false;
                }

                var serviceHandle = Windows.Advapi32.OpenServiceW(
                        managerHandle,
                        serviceName,
                        Windows.Advapi32.ServiceAccessRight.SERVICE_QUERY_CONFIG
                );
                if (serviceHandle == IntPtr.Zero)
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    if (errorCode != Windows.ERROR_SERVICE_DOES_NOT_EXIST)
                    {
                        Log("Can not open Windows service \"" + serviceName + "\", error code: " + errorCode);
                    }
                    return false;
                }

                Windows.Advapi32.CloseServiceHandle(serviceHandle);
                return true;
            }

            private ServiceInfo QueryStartTypeInWindows(string serviceName)
            {
                if (string.IsNullOrWhiteSpace(serviceName))
                {
                    return new ServiceInfo
                    {
                            ServiceName = serviceName,
                            ErrorCode = Windows.ERROR_INVALID_NAME,
                            ErrorMessage = "Service name \"" + serviceName + "\" is invalid"
                    };
                }

                var managerHandle = Windows.Advapi32.OpenSCManagerW(
                        null,
                        null,
                        Windows.Advapi32.SCMAccessRight.SC_MANAGER_CONNECT
                );
                if (managerHandle == IntPtr.Zero)
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    return new ServiceInfo
                    {
                            ServiceName = serviceName,
                            ErrorCode = errorCode,
                            ErrorMessage = "Can not open Windows service controller manager, error code: " + errorCode
                    };
                }

                var serviceInfo = new ServiceInfo
                {
                        ServiceName = serviceName
                };
                var serviceHandle = Windows.Advapi32.OpenServiceW(
                        managerHandle,
                        serviceName,
                        Windows.Advapi32.ServiceAccessRight.SERVICE_QUERY_CONFIG | Windows.Advapi32.ServiceAccessRight.SERVICE_QUERY_STATUS
                );
                if (serviceHandle == IntPtr.Zero)
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    serviceInfo.ErrorCode = errorCode;
                    serviceInfo.ErrorMessage = "Can not open Windows service \"" + serviceName + "\", error code: " + errorCode;
                }
                else
                {
                    const uint bytesAllocated = 8192;
                    var serviceConfigPtr = Marshal.AllocHGlobal((int)bytesAllocated);
                    try
                    {
                        uint bytes;
                        var success = Windows.Advapi32.QueryServiceConfigW(
                                serviceHandle,
                                serviceConfigPtr,
                                bytesAllocated,
                                out bytes
                        );
                        if (success)
                        {
                            var serviceConfig = (Windows.Advapi32.QUERY_SERVICE_CONFIG)Marshal.PtrToStructure(
                                    serviceConfigPtr,
                                    typeof(Windows.Advapi32.QUERY_SERVICE_CONFIG)
                            );
                            serviceInfo.StartType = ConvertFromWindows(serviceConfig.dwStartType);
                        }
                        else
                        {
                            var errorCode = Marshal.GetLastWin32Error();
                            serviceInfo.ErrorCode = errorCode;
                            serviceInfo.ErrorMessage = "Can not query Windows service \"" + serviceName + "\" config, error code: " + errorCode;
                        }
                    }
                    catch (Exception e)
                    {
                        Log("Can not query Windows service \"" + serviceName + "\" start type: " + e.Message);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(serviceConfigPtr);
                    }

                    serviceInfo = UpdateCurrentStateInWindows(serviceHandle, serviceInfo);

                    Windows.Advapi32.CloseServiceHandle(serviceHandle);
                }

                Windows.Advapi32.CloseServiceHandle(managerHandle);
                return serviceInfo;
            }

            private ServiceInfo UpdateCurrentStateInWindows(
                    IntPtr serviceHandle,
                    ServiceInfo serviceInfo)
            {
                if (serviceHandle == IntPtr.Zero || serviceInfo == null)
                {
                    return serviceInfo;
                }

                var status = new Windows.Advapi32.SERVICE_STATUS();
                var success = Windows.Advapi32.QueryServiceStatus(
                        serviceHandle,
                        ref status
                );
                if (success)
                {
                    serviceInfo.CurrentState = ConvertFromWindows(status.dwCurrentState);
                }
                else if (serviceInfo.ErrorCode != 0)
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    serviceInfo.ErrorCode = errorCode;
                    serviceInfo.ErrorMessage = "Can not query Windows service \"" + serviceInfo.ServiceName + "\" status, error code: " + errorCode;
                }

                return serviceInfo;
            }

            public StartType ConvertToStartType(string startType)
            {
                if ("Automatic".Equals(startType))
                {
                    return StartType.Automatic;
                }
                if ("Disabled".Equals(startType))
                {
                    return StartType.Disabled;
                }
                if ("Manual".Equals(startType))
                {
                    return StartType.Manual;
                }
                Log("Can not convert service start type " + startType + ". Use Automatic as fallback type");
                return StartType.Automatic;
            }

            private Windows.Advapi32.START_TYPE ConvertToWindows(StartType startType)
            {
                if (startType == StartType.Disabled)
                {
                    return Windows.Advapi32.START_TYPE.SERVICE_DISABLED;
                }
                if (startType == StartType.Manual)
                {
                    return Windows.Advapi32.START_TYPE.SERVICE_DEMAND_START;
                }
                if (startType == StartType.Automatic)
                {
                    return Windows.Advapi32.START_TYPE.SERVICE_AUTO_START;
                }
                Log("Can not convert service start type " + startType + " in Windows. Use SERVICE_AUTO_START as fallback type");
                return Windows.Advapi32.START_TYPE.SERVICE_AUTO_START;
            }

            private StartType ConvertFromWindows(Windows.Advapi32.START_TYPE startType)
            {
                if (startType == Windows.Advapi32.START_TYPE.SERVICE_AUTO_START)
                {
                    return StartType.Automatic;
                }
                if (startType == Windows.Advapi32.START_TYPE.SERVICE_DEMAND_START)
                {
                    return StartType.Manual;
                }
                if (startType == Windows.Advapi32.START_TYPE.SERVICE_DISABLED)
                {
                    return StartType.Disabled;
                }
                Log("Can not convert Windows service start type " + startType + ". Use Automatic as fallback type");
                return StartType.Automatic;
            }

            private CurrentState ConvertFromWindows(Windows.Advapi32.CURRENT_STATE currentState)
            {
                if (currentState == Windows.Advapi32.CURRENT_STATE.SERVICE_CONTINUE_PENDING)
                {
                    return CurrentState.ContinuePending;
                }
                if (currentState == Windows.Advapi32.CURRENT_STATE.SERVICE_PAUSED)
                {
                    return CurrentState.Paused;
                }
                if (currentState == Windows.Advapi32.CURRENT_STATE.SERVICE_PAUSE_PENDING)
                {
                    return CurrentState.PausePending;
                }
                if (currentState == Windows.Advapi32.CURRENT_STATE.SERVICE_RUNNING)
                {
                    return CurrentState.Running;
                }
                if (currentState == Windows.Advapi32.CURRENT_STATE.SERVICE_START_PENDING)
                {
                    return CurrentState.StartPending;
                }
                if (currentState == Windows.Advapi32.CURRENT_STATE.SERVICE_STOPPED)
                {
                    return CurrentState.Stopped;
                }
                if (currentState == Windows.Advapi32.CURRENT_STATE.SERVICE_STOP_PENDING)
                {
                    return CurrentState.StopPending;
                }
                Log("Can not convert Windows service current state " + currentState + ". Use Unknown as fallback state");
                return CurrentState.Unknown;
            }
        }

        internal enum StartType
        {
            Unknown = 0,
            NotAvailable = 1,
            Disabled = 2,
            Manual = 3,
            Automatic = 4,
            DelayedAutomatic = 5
        }

        internal enum CurrentState
        {
            Unknown = 0,
            NotAvailable = 1,
            Stopped = 2,
            StartPending = 3,
            StopPending = 4,
            Running = 5,
            ContinuePending = 6,
            PausePending = 7,
            Paused = 8
        }

        internal class ServiceInfo
        {
            public string ServiceName { get; set; }
            public CurrentState CurrentState { get; set; } = CurrentState.Unknown;
            public StartType StartType { get; set; } = StartType.Unknown;
            public int ErrorCode { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}
