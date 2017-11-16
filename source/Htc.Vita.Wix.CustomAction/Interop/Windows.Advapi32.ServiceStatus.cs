using System.Runtime.InteropServices;

namespace Htc.Vita.Wix.CustomAction.Interop
{
    internal static partial class Windows
    {
        internal static partial class Advapi32
        {
            /**
             * https://msdn.microsoft.com/en-us/library/windows/desktop/ms685996.aspx
             */
            [StructLayout(LayoutKind.Sequential)]
            public struct SERVICE_STATUS
            {
                public SERVICE_TYPE dwServiceType;

                public CURRENT_STATE dwCurrentState;

                public CONTROL_ACCEPTED dwControlAccepted;

                uint dwWin32ExitCode;

                uint dwServiceSpecificExitCode;

                uint dwCheckPoint;

                uint dwWaitHint;
            }
        }
    }
}
