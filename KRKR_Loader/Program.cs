using System;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Windows.Forms;
using EasyHook;

namespace KRKR_Loader
{
    class Program
    {
        static String ChannelName = null;

        [STAThread]
        public static void Main(string[] args)
        {
                try
                {
                    Config.Register(
                        "hehe you weak",
                        "KRKR_Loader.exe",
                        "Injector.dll");
                }
                catch (ApplicationException)
                {
                    MessageBox.Show("This is an administrative task!", "Permission denied...", MessageBoxButtons.OK);

                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain(args));


                Console.Read();
        }
    }
}