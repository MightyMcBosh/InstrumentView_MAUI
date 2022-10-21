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
using System.Reflection.Emit;
using System.Transactions;

namespace VersaMonitor
{
    public class CommunicationController
    {

        public event OnConnectionChangeHandler ConnectionChange; 
        
        static bool BytesRecvd = false; 
        static string name;
        static string errorListName,comlogname = "";
        static byte[] rcvbuf = new byte[128]; 
        static int baud;
        static string matchcom = @"COM\d{1,2}";
        static string matchbaud = @"\d{1,5}";
        static bool logging = false; 
        static bool polling = true; 
        static bool exit = false;
        static ArrayList list; //contains all of the readings of the leak detector
        static bool isRunningCleanup = false;
        static bool SendCustom;
        static AsciiCommand custom; 
        public static IPAddress IP { get; private set; } 
        static int comPort = 5226;
        static IPEndPoint localEP, remoteEP; 
        static Socket comSocket;
        

   

        static int numCommands, lastNumCommands, numBadCommands = 0;

        static int port = 5102;




        public async Task<bool> Connect(IPAddress ip)
        {
            IP = ip;

            remoteEP = new IPEndPoint(IP, comPort);
            localEP = new IPEndPoint(IPAddress.Any, port++);

            try
            {
                comSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //comSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, true);
                comSocket.Bind(localEP);
                comSocket.Connect(remoteEP);
                Console.WriteLine($"Connected ! to {remoteEP}");
                LD.Connected = true;
                return true; 
            }
            catch (Exception)
            {
                Console.WriteLine($"Could not connect to {remoteEP}");                
            }
            return false; 
        }




        public async Task RunEthernet()
        {
            //using StreamWriter logfile = new("Data" + DateTime.Now.ToString("u").Trim('/',' ','-','.',':','Z') + ".csv"); 
            bool wasLogging = false;
            bool sending = true;
            bool error = false;
            bool found = false; 
         
            Timer rcvTimeout = new Timer((a) => { sending = true; }, null, Timeout.Infinite, Timeout.Infinite);

            NetworkStream iostream = new NetworkStream(comSocket); 
            

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

            StringBuilder builder = new StringBuilder(); 
            try
            {
                while (!error && !exit)
                {

                    if (sending)
                    {

                        if (SendCustom)
                        {
                            current = custom;
                            commandindex--; //resend the previous command
                            SendCustom = false;
                        }
                        Array.Clear(rcvbuf, 0, rcvbuf.Length);
                        string str = current.cmd + "\r";
                        byte[] b = Encoding.UTF8.GetBytes(str);
                        //Console.WriteLine($"cmd sent - {current.cmd}");

                        iostream.Write(b, 0, b.Length);
                        sending = false;
                        builder.Clear();

                        rcvTimeout.Change(30000, Timeout.Infinite);
                    }
                    else if (!sending)
                    {
                       
                        while(iostream.DataAvailable && !found)
                        {
                            int tmp = iostream.ReadByte();
                            builder.Append((char)tmp);
                            if (tmp == 0x06)
                                found = true; 
                            
                        }
                        if (found)
                        {
                            found = false; 
                            string resp = builder.ToString();
                            builder.Clear(); 
                            rcvTimeout.Change(Timeout.Infinite, Timeout.Infinite);

                           //Console.WriteLine(resp);


                            if (current.ackloc != 255 && resp[current.ackloc] != 0x06)
                            {
                                char t = resp[current.ackloc];
                                Console.WriteLine($"character at index {current.ackloc} is {t}"); 
                                string errorstr = $"{DateTime.Now},{current.cmd.TrimEnd(trimEnd)},{current.ackloc},{Encoding.UTF8.GetString(rcvbuf)}";
                                Console.WriteLine(errorstr);
                                sending = true; //try to resend the broken command
                                numBadCommands++;
                            }
                            else
                            {


                                SerialCommands.Parse(resp, current.ParseOpt);
                                numCommands++;

                                if (++commandindex >= SerialCommands.cycle.Length)
                                    commandindex = 0;
                              


                                TimeSpan span = DateTime.Now - lastdt;
                                string res = Encoding.UTF8.GetString(rcvbuf);
                                res = res.Trim(trimEnd);

                                //prepare for next command
                                lastdt = DateTime.Now;
                                current = SerialCommands.cycle[commandindex];
                                
                                sending = true;
                                Array.Clear(rcvbuf, 0, rcvbuf.Length);
                            }
                        }
                    }
                    Thread.Sleep(20);
                    wasLogging = logging;
                }
            }
            catch (Exception)
            {
                LD.Connected = false;
                try
                {
                    iostream.Dispose();
                    iostream = null;
                }
                catch (Exception)
                {

                }

                try
                {
                    comSocket.Close();
                    comSocket.Dispose();
                }
                catch (Exception)
                {

                }
                
            }
            LD.Connected = false; 
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
