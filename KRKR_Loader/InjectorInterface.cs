using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KRKR_Loader
{
    public class InjectorInterface : MarshalByRefObject
    {
        public void IsInstalled(Int32 InClientPID)
        {
            Console.WriteLine("KRKR Loader has been installed in target {0}.\r\n", InClientPID);

            ProcessController.Resume(ProcessController.pInfo.hThread);
        }

        public void OnLoadLibraryA(Int32 InClientPID, string[] InLines)
        {
            foreach (var s in InLines)
            {
                Console.WriteLine(s);
            }
        }

        public void ReportException(Exception InInfo)
        {
            Console.WriteLine("The target process has reported an error:\r\n" + InInfo.ToString());
        }

        public void Ping()
        {
        }

        public void ModifyModule(IntPtr handle)
        {
            Process p = Process.GetProcessById(ProcessController.pInfo.dwProcessId);

            ProcessModule tpm=null;

            foreach(ProcessModule m in p.Modules)
            {
                if(m.ModuleName.EndsWith(".tpm"))
                    tpm = m;
            }

            Process.Start("MemoryPatcher.exe", String.Format("{0} {1} {2}", p.Id, tpm.BaseAddress, tpm.ModuleMemorySize));
        }

    }

}