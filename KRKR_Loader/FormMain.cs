using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.Remoting;
using System.Text;
using System.Windows.Forms;
using EasyHook;

namespace KRKR_Loader
{
    public partial class FormMain : Form
    {
        int TargetPID = 0;
        static String ChannelName = null;

        public FormMain(string[] args)
        {
            InitializeComponent();

            if (args.Length > 0)
            {
                textBox1.Text = args[0];
                textBox2.Text = args.Length == 1 ? string.Empty : args[1];
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (ProcessController.Run(textBox1.Text, textBox2.Text))
            {
                TargetPID = ProcessController.pInfo.dwProcessId;
            }

            RemoteHooking.IpcCreateServer<InjectorInterface>(ref ChannelName, WellKnownObjectMode.SingleCall);

            RemoteHooking.Inject(
                TargetPID,
                "Injector.dll",
                "Injector.dll",
                ChannelName);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "*.exe|*.exe|*.*|*.*";
                if(ofd.ShowDialog()==DialogResult.OK)
                {
                    textBox1.Text = ofd.FileName;
                }
            }
        }
    }
}
