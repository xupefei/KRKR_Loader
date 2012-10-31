using System;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using EasyHook;
using KRKR_Loader;

namespace Injector
{
    public class Main : EasyHook.IEntryPoint
    {
        static InjectorInterface Interface;
        LocalHook CreateFileHook;
        Stack<String> Queue = new Stack<String>();

        public Main(RemoteHooking.IContext InContext,String InChannelName)
        {
            // connect to host...
            Interface = RemoteHooking.IpcConnectClient<InjectorInterface>(InChannelName);

            Interface.Ping();
        }

        public void Run(RemoteHooking.IContext InContext,String InChannelName)
        {
            // install hook...
            try
            {

                CreateFileHook = LocalHook.Create(
                    LocalHook.GetProcAddress("kernel32.dll", "LoadLibraryA"),
                    new DLoadLibraryA(LoadLibraryA_Hooked),
                    this);

                CreateFileHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
            }
            catch (Exception ExtInfo)
            {
                Interface.ReportException(ExtInfo);

                return;
            }

            Interface.IsInstalled(RemoteHooking.GetCurrentProcessId());

            RemoteHooking.WakeUpProcess();

            // wait for host process termination...
            try
            {
                while (true)
                {
                    Thread.Sleep(500);

                    // transmit newly monitored file accesses...
                    if (Queue.Count > 0)
                    {
                        String[] Package = null;

                        lock (Queue)
                        {
                            Package = Queue.ToArray();

                            Queue.Clear();
                        }

                        Interface.OnLoadLibraryA(RemoteHooking.GetCurrentProcessId(), Package);
                    }
                    else
                        Interface.Ping();
                }
            }
            catch
            {
                // Ping() will raise an exception if host is unreachable
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi, SetLastError = true)]
        private delegate IntPtr DLoadLibraryA(IntPtr lpFileName);

        // just use a P-Invoke implementation to get native API access from C# (this step is not necessary for C++.NET)
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true,
            CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr LoadLibraryA(IntPtr lpFileName);

        // this is where we are intercepting all file accesses!
        static IntPtr LoadLibraryA_Hooked(IntPtr lpFileName)
        {
            string name = Marshal.PtrToStringAnsi(lpFileName);

            if (name.EndsWith(".tpm"))
            {
                Main This = (Main)HookRuntimeInfo.Callback;

                This.Queue.Push(name);

                IntPtr h = LoadLibraryA(lpFileName);
                Interface.ModifyModule(h);

                Thread.Sleep(2000);

                return h;
            }

            return LoadLibraryA(lpFileName);
        }
    }
}
