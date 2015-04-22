using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace UDP_SOCK
{
    // Simple sample program used for testing the UDP class
    class Program
    {
        static void Main(string[] args)
        {
            UDP_SOCK server = new UDP_SOCK(2000);

            Console.WriteLine("Bound to port 1000");

            server.startReceive(handlerEcho);

            while (true)
            {
                string msg = Console.ReadLine();

                if (String.IsNullOrEmpty(msg))
                    break;  // end the program
                
                // Send a msg via a udp Broadcast
                server.send(Encoding.ASCII.GetBytes("BROADCAST: " + msg));
                // Send a msg to a specific addr
                server.send(Encoding.ASCII.GetBytes(msg), "127.0.0.1");
            }

            Console.Write("Closing...");
            server.stopReceive();   // kill the receive thread
            server.close();         // close the socket
            Console.WriteLine("Closed");
        }

        // Function for testing the udp Class
        static void handlerEcho(IPEndPoint addr, Byte[] msg)
        {
            Console.WriteLine("Msg received from: {0}:{1}\n{2}\n", addr.Address, addr.Port, Encoding.ASCII.GetString(msg));
        }
    }
}
