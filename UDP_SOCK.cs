using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace UDP_SOCK
{
    class UDP_SOCK
    {
        private int prt;
        private volatile bool rx;
        private UdpClient sct;
        private Thread rcvThread;

        public Exception lastError {private get; set;}
        public delegate void handlerFunc(IPEndPoint addr, Byte[] msg);

        public UDP_SOCK(UdpClient socket)
        {
            // set the soket to the passed udp socket
            sct = socket;
        }
        public UDP_SOCK(int port)
        {
            prt = port;
            lastError = null;
            try
            {
                // try to open a new socket on the specifed port
                sct = new UdpClient(port);
                // set the receive time out to 1 second
                sct.Client.ReceiveTimeout = 1000;
                // enable broadcast
                sct.EnableBroadcast = true;
            }
            catch (SocketException e)
            {
                lastError = e;
                close();
            }
        }
        public UDP_SOCK(int port, int timeOut)
        {
            prt = port;
            lastError = null;
            try
            {
                // try to open a new socket on the specifed port
                sct = new UdpClient(port);
                // set the receive time out
                sct.Client.ReceiveTimeout = timeOut;
                // enable broadcast
                sct.EnableBroadcast = true;
            }
            catch (SocketException e)
            {
                lastError = e;
                close();
            }
        }
        ~UDP_SOCK()
        {
            close();
        }
        public void close()
        {
            if (sct != null && sct.Client.IsBound)
                sct.Close();
        }
        public void clearError()
        {
            lastError = null;
        }
        public bool send(Byte[] msg)
        {
            try
            {
                sct.Send(msg, msg.Length, new IPEndPoint(IPAddress.Broadcast, prt));
            }
            catch (SocketException e)
            {
                lastError = e;
                return false;
            }
            catch (Exception e)
            {
                lastError = e;
                return false;
            }
            return true;
        }
        public bool send(Byte[] msg, String ip)
        {
            try
            {
                sct.Send(msg, msg.Length, ip, prt);
            }
            catch (SocketException e)
            {
                lastError = e;
                return false;
            }
            catch (Exception e)
            {
                lastError = e;
                return false;
            }
            return true;
        }
        public bool startReceive(handlerFunc handler)
        {
            rx = true;
            rcvThread = new Thread(() => receive(handler));
            try
            {
                // start the thread
                rcvThread.Start();
            }
            catch (ThreadStateException e)
            {
                lastError = e;
                return false;
            }
            catch (OutOfMemoryException e)
            {
                lastError = e;
                return false;
            }
            catch (Exception e)
            {
                lastError = e;
                return false;
            }
            return true;
        }
        public bool stopReceive()
        {
            // set rx to false to break the loop
            rx = false;

            // sleep for x2 the time out to allow the thread to exit
            Thread.Sleep(sct.Client.ReceiveTimeout * 2);

            // check if the thread has ended
            if (rcvThread.IsAlive)
            {
                // attempt to abort the thread
                rcvThread.Abort();

                // check if it is still alive
                if (rcvThread.IsAlive)
                    return false;   // failed to end the thread
            }

            return true;
        }
        private void receive(handlerFunc handler)
        {
            IPEndPoint addr = new IPEndPoint(IPAddress.Any, prt);
            // continue receiving until rx is set to false or a socket does not exsit
            while (rx && sct != null)
            {
                try
                {
                    // receive the msg
                    Byte[] msg = sct.Receive(ref addr);
                    // send to the handler function
                    handler(addr, msg);
                }
                catch (TimeoutException e) { } // ignore the timeout and carry on
                // if this exception occurrs try to exit gracefully
                catch (ThreadAbortException e)
                {
                    rx = false;
                    lastError = e;
                    break;  // break out of the while loop
                }
                catch (SocketException e)
                {
                    lastError = e;
                }
                catch (Exception e)
                {
                    lastError = e;
                }
            }
        }
    }
}
