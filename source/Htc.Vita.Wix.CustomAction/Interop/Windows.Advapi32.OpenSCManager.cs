using System;
using System.Runtime.InteropServices;

namespace Htc.Vita.Wix.CustomAction.Interop
{
    internal static partial class Windows
    {
        internal static partial class Advapi32
        {
            /**
             * https://msdn.microsoft.com/en-us/library/windows/desktop/ms684323.aspx
             */
            [DllImport(Libraries.Windows_advapi32,
                    CallingConvention = CallingConvention.Winapi,
                    CharSet = CharSet.Unicode,
                    ExactSpelling = true,
                    SetLastError = true)]
            public static extern IntPtr OpenSCManagerW(
                    [In] string machineName,
                    [In] string databaseName,
                    SCMAccessRight desiredAccess
            );
        }
    }
}
