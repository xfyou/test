using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;


namespace Serial___uart
{
    public partial class Form1 : Form
    {
        public enum DataMode { Text, Hex }
        private Byte[] portByteReceive = new Byte[65535];    //大缓冲区数据
        private int bufferLength;                           //大缓冲区长度
        private int head;
        private int tail;
        private int[] angle = new int[360];
        

        public Form1()
        {
            InitializeComponent();
            
        }

         //十六进制转换字节数组
        private byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }

        //字节数组转换十六进制
        private string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
            return sb.ToString().ToUpper();
        }
        private void port_DataReceived()
        {
             // 获取字节长度
            int length = serialPort1.BytesToRead;
            // 创建字节数组
            byte[] temp = new byte[length];
            // 读取缓冲区的数据到数组
            serialPort1.Read(temp, 0, length);
            // 显示读取的数据到数据窗口
            //Log(LogMsgType.Incoming, ByteArrayToHexString(buffer) + "\n");

           //把数据放到大缓冲区
            for (int i = 0; i < length; i++)
            {
                portByteReceive[bufferLength] = temp[i];
                bufferLength++;
            }

            //寻找完整的帧数据
            for (int i = bufferLength - 1; i >= 0; i--)
            {
                if (portByteReceive[i] == 0x7E && portByteReceive[i - 1] != 0x7E)
                {
                    port_DataHandle(i + 1, portByteReceive);
                    for (int j = i + 1; j < bufferLength; j++)
                    {
                        portByteReceive[j - i - 1] = portByteReceive[j];    //把后面的半帧数据放置到数组最前
                    }
                    bufferLength = bufferLength - (i + 1);
                    for (int j = bufferLength; j < 65535; j++)
                    {
                        portByteReceive[j] = 0;
                    }
                    return;
                }
            }            
/*            int length = serialPort1.BytesToRead;
            // 创建字节数组
            byte[] buffer = new byte[length];
            // 读取缓冲区的数据到数组
            serialPort1.Read(buffer, 0, length);
            String read;
            read = ByteArrayToHexString(buffer);
            textBox1.Text += read;
            textBox1.SelectionStart = textBox1.TextLength;
            textBox1.ScrollToCaret();*/
        }

        private void port_DataHandle(int bytes, byte[] buffer)
        {
            byte[] temp = new byte[bytes];
            for (int i = 0; i < bytes; i++)
            {
                temp[i] = buffer[i];
            }
            String read;
            read = ByteArrayToHexString(temp);
            textBox1.Text += read;
            textBox1.SelectionStart = textBox1.TextLength;
            textBox1.ScrollToCaret();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort1.IsOpen)
                {
                    MessageBox.Show("串口已打开", "提示");
                }
                else
                {
                    serialPort1.PortName = comboBox1.Text;
                    serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);
                    serialPort1.DataBits = Convert.ToInt32(comboBox3.Text);
                    switch (comboBox4.Text)
                    {
                        case "1": serialPort1.StopBits = StopBits.One; break;
                        case "1.5": serialPort1.StopBits = StopBits.OnePointFive; break;
                        case "2": serialPort1.StopBits = StopBits.Two; break;
                    }
                    serialPort1.Open();
                    timer1.Enabled = true;
                }
            }
            catch
            {
                MessageBox.Show("串口无法使用或已被占用", "提示");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (string s in SerialPort.GetPortNames())
            {
                this.comboBox1.Items.Add(s);
            }
            comboBox1.SelectedIndex = 2;
            comboBox2.SelectedIndex = 5;
            comboBox3.SelectedIndex = 3;
            comboBox4.SelectedIndex = 0;
            head = 0;
            tail = 10;
            for (int i = 0; i < 360; i++)
            {
                angle[i] = i + 1;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
            timer1.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Byte[] send = new Byte[30];
            int i = 0;
            send[i++] = 0x7E;
            send[i++] = 0x00;
            send[i++] = 0x2B;
            send[i++] = 0x03;
            send[i++] = 0x31;
            send[i++] = 0x41;
            for (int j = head; j < tail; j++)
            {
                send[i++] = (Byte)(angle[j] & 0xFF);
                send[i++] = (Byte)(angle[j] >> 8);
            }
            send[i++] = 0x2B;
            send[i++] = 0x00;
            send[i++] = 0x7E;
            serialPort1.Write(send, 0, i);
            head = (head + 10) % 360;
            if (tail == 350)
            {
                tail += 10;
            }
            else
            {
                tail = (tail + 10) % 360;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            port_DataReceived();
        }
    }
}
