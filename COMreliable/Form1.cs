using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace COMreliable
{
    public partial class Form1 : Form
    {
        MySerialPort _mySerialPort;
        public Form1()
        {
            InitializeComponent();
        }

        private void btn_Start_Click(object sender, EventArgs e)
        {
            _mySerialPort = new MySerialPort("COM9:");
            _mySerialPort.eventOnMessage += new MySerialPort.delegateOnMessage(_mySerialPort_eventOnMessage);
        }

        void _mySerialPort_eventOnMessage(object sender, MySerialPort.mySerialEventArgs e)
        {
            addLog(e.sMsg);
        }
        delegate void SetTextCallback(string text);
        public void addLog(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.txtLog.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(addLog);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                if (txtLog.Text.Length > 2000)
                    txtLog.Text = "";
                txtLog.Text += text + "\r\n";
                txtLog.SelectionLength = 0;
                txtLog.SelectionStart = txtLog.Text.Length - 1;
                txtLog.ScrollToCaret();
            }
        }

        private void Form1_Closed(object sender, EventArgs e)
        {
            if (_mySerialPort != null)
            {
                _mySerialPort.Dispose();
                _mySerialPort = null;
            }
        }

        private void btn_Stop_Click(object sender, EventArgs e)
        {
            if (_mySerialPort != null)
            {
                _mySerialPort.Dispose();
                _mySerialPort = null;
            }
        }
    }
}