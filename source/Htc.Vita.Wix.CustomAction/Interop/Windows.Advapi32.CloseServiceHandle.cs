using System;
using System.Runtime.InteropServices;

namespace Htc.Vita.Wix.CustomAction.Interop
{
    internal static partial class Windows
    {
        internal static partial class Advapi32
        {
            /**
             * https://msdn.microsoft.com/en-us/library/windows/desktop/ms682028.aspx
             */
            [DllImport(Libraries.Windows_advapi32,
                    CallingConvention = CallingConvention.Winapi,
                    CharSet = CharSet.Unicode,
                    ExactSpelling = true,
                    SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CloseServiceHandle(
                    IntPtr hSCObject
            );
        }
    }
}
