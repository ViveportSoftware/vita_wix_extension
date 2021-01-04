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
                            ErrorCode = (int) Windows.Error.InvalidName,
                            ErrorMessage = "Service name \"" + serviceName + "\" is invalid"
                    };
                }

                var managerHandle = Windows.OpenSCManagerW(
                        null,
                        null,
                        Windows.ServiceControlManagerAccessRight.Connect
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
                var serviceHandle = Windows.OpenServiceW(
                        managerHandle,
                        serviceName,
                        Windows.ServiceAccessRight.ChangeConfig |
                                Windows.ServiceAccessRight.QueryConfig |
                                Windows.ServiceAccessRight.QueryStatus
                );
                if (serviceHandle == IntPtr.Zero)
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    serviceInfo.ErrorCode = errorCode;
                    serviceInfo.ErrorMessage = "Can not open Windows service \"" + serviceName + "\", error code: " + errorCode;
                }
                else
                {
                    var success = Windows.ChangeServiceConfigW(
                            serviceHandle,
                            Windows.ServiceType.NoChange,
                            ConvertToWindows(startType),
                            Windows.ErrorControlType.NoChange,
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

                    Windows.CloseServiceHandle(serviceHandle);
                }

                Windows.CloseServiceHandle(managerHandle);
                return serviceInfo;
            }

            private bool CheckIfExistsInWindows(string serviceName)
            {
                if (string.IsNullOrWhiteSpace(serviceName))
                {
                    return false;
                }

                var managerHandle = Windows.OpenSCManagerW(
                        null,
                        null,
                        Windows.ServiceControlManagerAccessRight.Connect
                );
                if (managerHandle == IntPtr.Zero)
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    Log("Can not open Windows service controller manager, error code: " + errorCode);
                    return false;
                }

                var serviceHandle = Windows.OpenServiceW(
                        managerHandle,
                        serviceName,
                        Windows.ServiceAccessRight.QueryConfig
                );
                if (serviceHandle == IntPtr.Zero)
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    if (errorCode != (int) Windows.Error.ServiceDoesNotExist)
                    {
                        Log("Can not open Windows service \"" + serviceName + "\", error code: " + errorCode);
                    }
                    return false;
                }

                Windows.CloseServiceHandle(serviceHandle);
                return true;
            }

            private ServiceInfo QueryStartTypeInWindows(string serviceName)
            {
                if (string.IsNullOrWhiteSpace(serviceName))
                {
                    return new ServiceInfo
                    {
                            ServiceName = serviceName,
                            ErrorCode = (int) Windows.Error.InvalidName,
                            ErrorMessage = "Service name \"" + serviceName + "\" is invalid"
                    };
                }

                var managerHandle = Windows.OpenSCManagerW(
                        null,
                        null,
                        Windows.ServiceControlManagerAccessRight.Connect
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
                var serviceHandle = Windows.OpenServiceW(
                        managerHandle,
                        serviceName,
                        Windows.ServiceAccessRight.QueryConfig | Windows.ServiceAccessRight.QueryStatus
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
                        uint bytes = 0;
                        var success = Windows.QueryServiceConfigW(
                                serviceHandle,
                                serviceConfigPtr,
                                bytesAllocated,
                                ref bytes
                        );
                        if (success)
                        {
                            var serviceConfig = (Windows.QueryServiceConfig)Marshal.PtrToStructure(
                                    serviceConfigPtr,
                                    typeof(Windows.QueryServiceConfig)
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

                    Windows.CloseServiceHandle(serviceHandle);
                }

                Windows.CloseServiceHandle(managerHandle);
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

                var status = new Windows.ServiceStatus();
                var success = Windows.QueryServiceStatus(
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

            private Windows.StartType ConvertToWindows(StartType startType)
            {
                if (startType == StartType.Disabled)
                {
                    return Windows.StartType.Disabled;
                }
                if (startType == StartType.Manual)
                {
                    return Windows.StartType.DemandStart;
                }
                if (startType == StartType.Automatic)
                {
                    return Windows.StartType.AutoStart;
                }
                Log("Can not convert service start type " + startType + " in Windows. Use SERVICE_AUTO_START as fallback type");
                return Windows.StartType.AutoStart;
            }

            private StartType ConvertFromWindows(Windows.StartType startType)
            {
                if (startType == Windows.StartType.AutoStart)
                {
                    return StartType.Automatic;
                }
                if (startType == Windows.StartType.DemandStart)
                {
                    return StartType.Manual;
                }
                if (startType == Windows.StartType.Disabled)
                {
                    return StartType.Disabled;
                }
                Log("Can not convert Windows service start type " + startType + ". Use Automatic as fallback type");
                return StartType.Automatic;
            }

            private CurrentState ConvertFromWindows(Windows.CurrentState currentState)
            {
                if (currentState == Windows.CurrentState.ContinuePending)
                {
                    return CurrentState.ContinuePending;
                }
                if (currentState == Windows.CurrentState.Paused)
                {
                    return CurrentState.Paused;
                }
                if (currentState == Windows.CurrentState.PausePending)
                {
                    return CurrentState.PausePending;
                }
                if (currentState == Windows.CurrentState.Running)
                {
                    return CurrentState.Running;
                }
                if (currentState == Windows.CurrentState.StartPending)
                {
                    return CurrentState.StartPending;
                }
                if (currentState == Windows.CurrentState.Stopped)
                {
                    return CurrentState.Stopped;
                }
                if (currentState == Windows.CurrentState.StopPending)
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
