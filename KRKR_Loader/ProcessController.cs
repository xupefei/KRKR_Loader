using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace KRKR_Loader
{
    class ProcessController
    {
        [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
        public class SECURITY_ATTRIBUTES
        {
            public int nLength;
            public string lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public int lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public int wShowWindow;
            public int cbReserved2;
            public byte lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Ansi)]
        private static extern bool CreateProcess(StringBuilder lpApplicationName,
                                                StringBuilder lpCommandLine,
                                                 SECURITY_ATTRIBUTES lpProcessAttributes,
                                                 SECURITY_ATTRIBUTES lpThreadAttributes,
                                                 bool bInheritHandles,
                                                 int dwCreationFlags,
                                                 StringBuilder lpEnvironment,
                                                 StringBuilder lpCurrentDirectory,
                                                 ref STARTUPINFO lpStartupInfo,
                                                 ref PROCESS_INFORMATION lpProcessInformation
            );

        [DllImport("Kernel32.dll", CharSet = CharSet.Ansi)]
        static extern uint ResumeThread(IntPtr hThread);

        public static PROCESS_INFORMATION pInfo = new PROCESS_INFORMATION();

        public static bool Run(string target,string parm)
        {
            STARTUPINFO sInfo = new STARTUPINFO();
            SECURITY_ATTRIBUTES pSec = new SECURITY_ATTRIBUTES();
            SECURITY_ATTRIBUTES tSec = new SECURITY_ATTRIBUTES();
            pSec.nLength = Marshal.SizeOf(pSec);
            tSec.nLength = Marshal.SizeOf(tSec);

            return CreateProcess(new StringBuilder(target),
                          new StringBuilder(parm),
                          null,
                          null,
                          false,
                          0x00000004,
                          null,
                          null,
                          ref sInfo,
                          ref pInfo);
        }

        public static uint Resume(IntPtr hThread)
        {
            return ResumeThread(hThread);
        }
    }
}
