using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace COMreliable
{
    class MySerialPort:IDisposable
    {
        static SerialPort _serialPort=null;
        string _sPort = "COM1:";
        Thread _keepAliveThread;
        bool _bRunKeepAliveThread = true;
        
        public MySerialPort(String sPort)
        {
            _sPort = sPort;
            openPort();
            startThread();
        }
        
        public void Dispose()
        {
            stopThread();
            closePort();
        }

        void openPort()
        {
            if (_serialPort != null)
                closePort();
            try
            {
                _serialPort = new SerialPort(_sPort, 9600, Parity.None, 8, StopBits.One);
                _serialPort.Handshake = Handshake.None;
                _serialPort.Open();
                _serialPort.DataReceived += new SerialDataReceivedEventHandler(_serialPort_DataReceived);
                _serialPort.ErrorReceived += new SerialErrorReceivedEventHandler(_serialPort_ErrorReceived);
                this.onUpdateMessage(new mySerialEventArgs("port opened"));
            }
            catch (Exception ex)
            {
            }
        }

        void keepAlive()
        {
            this.onUpdateMessage(new mySerialEventArgs("keepAlive thread started"));
            int count = 0;
            int max = 30;
            while (_bRunKeepAliveThread)
            {
                try
                {
                    count++;
                    if (count == max)
                    {
                        System.Diagnostics.Debug.WriteLine("...keepalive");
                        this.writeCOM(new byte[] { 0 });
                        count = 0;
                    }
                    Thread.Sleep(1000);
                }
                catch (ThreadAbortException ex)
                {
                    this.onUpdateMessage(new mySerialEventArgs("keepAlive thread abort"));
                }
                catch (SystemException ex)
                {
                    System.Diagnostics.Debug.WriteLine("...SystemException");
                    closePort();
                    openPort();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("...Exception");
                    //restart
                    closePort();
                    openPort();
                }
            }
            this.onUpdateMessage(new mySerialEventArgs("keepAlive thread ended"));

        }

        public void writeCOM(string s)
        {
            if (_serialPort != null && _serialPort.IsOpen)
                _serialPort.Write(s);
        }

        public void writeCOM(byte[] b)
        {
            if (_serialPort != null && _serialPort.IsOpen){
                _serialPort.Write(b, 0, b.Length);
            }
        }

        void _serialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("_serialPort_ErrorReceived: " + e.EventType.ToString());
        }

        void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("_serialPort_DataReceived: " + e.EventType.ToString());
            string sRecv = _serialPort.ReadExisting();
            System.Diagnostics.Debug.WriteLine("recv=" + sRecv);
            this.onUpdateMessage(new mySerialEventArgs(sRecv));
        }

        void closePort()
        {
            if (_serialPort == null)
                return;
            try
            {
                _serialPort.ErrorReceived -= _serialPort_ErrorReceived;
                _serialPort.DataReceived -= _serialPort_DataReceived;
                _serialPort.Close();
                _serialPort.Dispose();
                _serialPort = null;
                this.onUpdateMessage(new mySerialEventArgs("port closed"));
            }
            catch (Exception ex)
            {
            }
        }

        void startThread()
        {
            //if (_keepAliveThread != null)
            //    stopThread();
            if (_keepAliveThread == null)
            {
                _keepAliveThread = new Thread(new ThreadStart(keepAlive));
                _keepAliveThread.Name = "keepAlive";
                _keepAliveThread.Start();
            }
        }

        void stopThread()
        {
            _bRunKeepAliveThread = false;
            Thread.Sleep(1000);
            if (!_keepAliveThread.Join(1000))
                _keepAliveThread.Abort();
            _keepAliveThread = null;
        }

        public delegate void delegateOnMessage(object sender, mySerialEventArgs e);
        public event delegateOnMessage eventOnMessage;
        void onUpdateMessage(mySerialEventArgs e){
            delegateOnMessage local=this.eventOnMessage;
            if (local != null)
                local(this, e);
        }
        public class mySerialEventArgs:EventArgs{
            public string sMsg = "";
            public mySerialEventArgs(string s)
            {
                sMsg = s;
            }
        }
    }
}
