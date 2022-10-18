using System;
using System.IO;
using System.Collections; 
using System.IO.Ports;
using System.Text.RegularExpressions; 
using System.Threading;
using System.Threading.Tasks; 
using System.Net; 
using System.Net.Sockets;
using System.Text; 

namespace VersaMonitor
{
    public class CommunicationController
    {
        
        static bool BytesRecvd = false; 
        static string name;
        static string errorListName,comlogname = "";
        static byte[] rcvbuf = new byte[128]; 
        static int baud;
        static string matchcom = @"COM\d{1,2}";
        static string matchbaud = @"\d{1,5}";
        static bool logging = false; 
        static bool polling = false; 
        static bool exit = false;
        static ArrayList list; //contains all of the readings of the leak detector
        static bool isRunningCleanup = false;
        static bool SendCustom;
        static AsciiCommand custom; 
        public static IPAddress IP { get; private set; } 
        static int comPort = 5226;
        static IPEndPoint localEP, remoteEP; 
        static Socket comSocket;
        static bool isConnected;
        static Task connection;

        static int numCommands, lastNumCommands, numBadCommands = 0;






        public async Task Connect(IPAddress ip)
        {
            IP = ip;

            remoteEP = new IPEndPoint(IP, comPort);
            localEP = new IPEndPoint(IPAddress.Any, 5000);

            comSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //comSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, true);
            comSocket.Bind(localEP);
            await comSocket.ConnectAsync(remoteEP);
        }




        public async void RunEthernet()
        {
            //using StreamWriter logfile = new("Data" + DateTime.Now.ToString("u").Trim('/',' ','-','.',':','Z') + ".csv"); 
            bool wasLogging = false;
            bool sending = true;
            bool error = false;
         
            Timer rcvTimeout = new Timer((a) => { sending = true; }, null, Timeout.Infinite, Timeout.Infinite);

            DateTime lastdt = DateTime.Now;  
            int commandindex = 0; 
            AsciiCommand current = SerialCommands.cycle[0];
            string cmd = "";
            int test = 0;

            new Timer((a) =>

            {
                lastNumCommands = numCommands;
                numCommands = 0;
            }, null, 1000, 1000);

           
          
            char[] trimEnd  = new char[] { '\r', '\u0006','\u0000' }; 


            while (!error && !exit)            
            {
                
                if (sending && polling)
                {

                    if(SendCustom)
                    {
                        current = custom;
                        commandindex--; //resend the previous command
                        SendCustom = false; 
                    }
                    Array.Clear(rcvbuf, 0, rcvbuf.Length);
                    comSocket.Send(Encoding.UTF8.GetBytes(current.cmd + "\r"));
                    sending = false;
                     
                    rcvTimeout.Change(30000, Timeout.Infinite); 
                }
                else if(!sending && polling)
                {
                    if(comSocket.Available > 0)
                    {
                        int numBytesRecvd = comSocket.Receive(rcvbuf,SocketFlags.None); 
                        rcvTimeout.Change(Timeout.Infinite, Timeout.Infinite);


                        if (current.ackloc != 255 &&  rcvbuf[current.ackloc] != 0x06)
                        {
                            string errorstr = $"{DateTime.Now},{current.cmd.TrimEnd(trimEnd)},{current.ackloc},{Encoding.UTF8.GetString(rcvbuf)}";
                            
                            sending = true; //try to resend the broken command
                            numBadCommands++; 
                        }
                        else
                        {
                            

                            SerialCommands.Parse(rcvbuf, current.ParseOpt);
                            numCommands++; 
                        
                            if (commandindex >= SerialCommands.cycle.Length)
                                commandindex = 0;


                            TimeSpan span = DateTime.Now - lastdt;
                            string res = Encoding.UTF8.GetString(rcvbuf);
                            res = res.Trim(trimEnd);
                            
                            string ComLogString = DateTime.Now.ToString("MM/dd/yy,HH:mm:ss.fff") + $",{current.cmd},{res},{span.TotalMilliseconds}"; //this will log every command and how long it took
                            


                            //prepare for next command
                            lastdt = DateTime.Now; 
                            current = SerialCommands.cycle[commandindex];
                            commandindex++;
                            sending = true;
                            Array.Clear(rcvbuf, 0, rcvbuf.Length); 
                        }
                    }
                }
                Thread.Sleep(10);
                wasLogging = logging; 
            }            
        }

        public void ToggleLD(object state)
        {
            SendCustom = true;
            if (LD.TestMode == Mode.Standby)
            {
                custom = SerialCommands.StartCycle;
            }
            else
                custom = SerialCommands.StopCycle; 
        }

        public void StartCal(object state)
        {
            SendCustom = true;
            custom = SerialCommands.StartAutoCal; 
        }

        
    }
}
