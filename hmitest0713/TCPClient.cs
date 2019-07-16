using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace hmitest0713
{
    class TCPClient
    {
        List<Socket> theSockets = new List<Socket>();
        Dictionary<string, Socket> sockets = new Dictionary<string, Socket>();
        private ManualResetEvent timeoutEvent = new ManualResetEvent(false);
        string str;
        private bool IsConnected;
        private object lockObjConnect = new object();
        public Socket keySocket;
        public byte[] receive = new byte[1024];
        

        public bool ClientStart(IPEndPoint ipe)
        {
            if (!IsConnected)
            {
                return ConnectSocket(ipe);           
            }
            return false;
        }

        private void MessageShow(string str)
        {
            string s = str + Environment.NewLine;
            
        }

        private bool ConnectSocket(IPEndPoint ipe)
        {
            keySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                keySocket.BeginConnect(ipe, new AsyncCallback(AsyncConnectCallback), keySocket);

            }
            catch (Exception ex)
            {
                str = ex.ToString();
                MessageShow(str);
                return false;
            }
            timeoutEvent.Reset();
            if (timeoutEvent.WaitOne(10000, false))
            {
                if (IsConnected)
                {
                    sockets.Add(ipe.ToString(), keySocket);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                str = "connect time out";
                MessageShow(str);
                return false;
            }
        }

        private void AsyncConnectCallback(IAsyncResult ar)
        {
            lock (lockObjConnect)
            {
                Socket socket = (Socket)ar.AsyncState;
                try
                {
                    socket.EndConnect(ar);
                    IsConnected = true;
                    //SetXinTiao(socket);
                    //StartKeepAlive(socket);
                }
                catch (Exception e)
                {
                    str = e.ToString();
                    MessageShow(str);
                    IsConnected = false;
                    return;
                }
                finally
                {
                    timeoutEvent.Set();
                }
            }
        }

        private void SetXinTiao(Socket socket)
        {
            byte[] inValue = new byte[] { 1, 0, 0, 0, 0x88, 0x13, 0, 0, 0xd0, 0x07, 0, 0 };// 首次探测时间5 秒, 间隔侦测时间2 秒
            socket.IOControl(IOControlCode.KeepAliveValues, inValue, null);
        }

        private void StartKeepAlive(Socket socket)
        {
            StateObject state = new StateObject();
            state.workSocket = socket;
            socket.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE,
                0, new AsyncCallback(AsyncReceiveCallback), state);
        }

        private void AsyncReceiveCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            try
            {

                int receiveLen = state.workSocket.EndReceive(ar);
                if (receiveLen > 0)
                {
                    receive = state.buffer;
                    state.workSocket.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE,
                0, new AsyncCallback(AsyncReceiveCallback), state);
                }
                else
                {

                    if (state.workSocket.Connected == true)
                    {
                        socketDisconnected(state.workSocket);
                    }
                }
            }
            catch (Exception ex)
            {
                str = ex.ToString() + Environment.NewLine;
           
                socketDisconnected(state.workSocket);
                return;
            }
        }

        public byte[] socketReceive()
        {
            try
            {
                byte[] receiveBuff=new byte[1024];
                if (keySocket == null)
                {
                    return null;
                }
                int len = keySocket.Receive(receiveBuff);
                if (len == 0)
                {

                }
                else
                {
                    byte[] buff = new byte[len];
                    for(int i = 0; i < len; i++)
                    {
                        buff[i] = receiveBuff[i];
                    }
                    return buff;
                }

            }
            catch (Exception ex)
            {
                
            }
            return null;
        }

        private void socketDisconnected(Socket workSocket)
        {
            workSocket.Close();
        }

        class StateObject
        {
            public Socket workSocket = null;
            public const int BUFFER_SIZE = 1024;
            public byte[] buffer = new byte[BUFFER_SIZE];
            public StringBuilder sb = new StringBuilder();
        }
        public bool socketSend(Socket socket, byte[] sendMessage)
        {

            return SendData(socket, sendMessage);

        }

        private bool checkSocketState(Socket socket)
        {
            if (socket.Connected == true)
            {
                return true;
            }
            return false;
        }

        private bool SendData(Socket socket, byte[] sendMessage)
        {
            if (sendMessage == null)
            {
                return false;
            }
            try
            {
           
                int sendLen = socket.Send(sendMessage);
                if (sendLen < 1)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                str = ex.ToString();
                MessageShow(str);
                return false;
            }
            return true;
        }
    }
}
