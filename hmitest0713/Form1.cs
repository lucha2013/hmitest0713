using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hmitest0713
{
    public partial class Form1 : Form
    {
        static IPAddress ip = IPAddress.Parse("192.168.0.11");
        static int port = 5000;
        private IPEndPoint ipe = new IPEndPoint(ip, port);
        private TCPClient tCPClient = new TCPClient();
        private PLCDrive pLCDrive;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            bool startClient=tCPClient.ClientStart(ipe);
            if (!startClient) return;
            pLCDrive = new PLCDrive(tCPClient);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            string address = "d100";
            float[] f = pLCDrive.ReadFloat(address, 1);
            textBox1.Text = f[0].ToString();
        }
    }
}
