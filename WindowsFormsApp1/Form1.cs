using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        string json;
        Item items;
        SerialPort port1;
        int baudrate = 9600;
        bool A1 = true;
        bool A2 = true;
        bool A4 = true;
        bool isOpenCom = false;

        public Form1()
        {
            InitializeComponent();

            OpenCom();

            this.notifyIcon1.MouseDoubleClick += new MouseEventHandler(notifyIcon1_MouseDoubleClick);
            this.Resize += new System.EventHandler(this.Form1_Resize);

            WindowState = FormWindowState.Minimized;
            //ShowIcon = false;
            ShowInTaskbar = false;
            Hide();
            notifyIcon1.Visible = true;
            timer1.Start();
        }

        public class Item
        {
            public string portName;
            public string app1;
            public string app2;
            public string app3;
        }



        private void Form1_Resize(object sender, EventArgs e)
        {

            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;

            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            notifyIcon1.Visible = false;
            WindowState = FormWindowState.Normal;
        }
        

        public void LoadJson()
        {
            using (StreamReader r = new StreamReader("file.json"))
            {
                json = r.ReadToEnd();
                items = JsonConvert.DeserializeObject<Item>(json);
            }
        }

    

        private void button1_Click(object sender, EventArgs e)
        {
            OpenCom();            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CloseCom();
        }

        private void OpenCom()
        {
            A4 = true;
            A2 = true;
            A1 = true;
            isOpenCom = true;
            LoadJson();
            try
            {
                port1 = new SerialPort(items.portName, baudrate);
                port1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(port1_DataReceived);
                port1.Open();
                //this.Invoke(new EventHandler(delegate {
                    progressBar1.Value = 100;
                //}));
            }
            catch (Exception)
            {
                textBox1.Text = "Невозможно открыть последовательный порт " + items.portName.ToString();
            }
        }

        private void CloseCom()
        {
            port1.Close();
            port1.Dispose();
            this.Invoke(new EventHandler(delegate  {
                progressBar1.Value = 0;
            }));
            A4 = false;
            A2 = false;
            A1 = false;
            isOpenCom = false;
        }

        private void port1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            string command = port1.ReadExisting();
            Process[] proc = Process.GetProcesses();

            foreach (Process process in proc)
            {

                if ("HDcamera" == process.ProcessName)                
                    A1 = false;
                                                 
                if (items.app1 == process.ProcessName)
                    A2 = false;               

                if (items.app3 == process.ProcessName)
                    A4 = false;

            }
         

            if (command == "A1" && A1)
            {
                A1 = false;
                
                try
                {                   
                    Process.Start(items.app1);
                    CloseCom();
                }
                catch (Exception)
                {
                    this.Invoke(new EventHandler(delegate
                    {
                        textBox1.Text = "Не удается запустить приложение 1   ";
                    }));
                }
            }

            if (command == "A2" && A2)
            {
                A2 = false;
                try
                {
                    Process.Start(items.app2);
                }
                catch (Exception)
                {
                    this.Invoke(new EventHandler(delegate
                    {
                        textBox1.Text = "Не удается запустить приложение 2";
                    }));
                }
            }

            if (command == "A4" && A4)
            {
                A4 = false;
                try
                {
                    Process.Start(items.app3);                   
                }
                catch (Exception)
                {
                    this.Invoke(new EventHandler(delegate
                    {
                        textBox1.Text = "Не удается запустить приложение 3";
                    }));
                }
            }


        }
       
        private void timer1_Tick(object sender, EventArgs e)
        {
            A4 = true;
            A2 = true;
            A1 = true;
        }


        private void timerReOpen_Tick(object sender, EventArgs e)
        {
            Process[] proc = Process.GetProcesses();
            bool isHDcamera = false;

            foreach (Process process in proc)
            {
                if (process.ProcessName == "HDcamera")
                {             
                    CloseCom();
                    isHDcamera = true;
                    break;
                }                               
            }

            if (!isHDcamera && !isOpenCom)
                OpenCom();


            /*for(int i=0; i<proc.Length-1; i++)
            {
                if (proc[i].ProcessName == "HDcamera")
                {
                    flagHDcamera = true;
                    break;
                }
            }*/

            
        }
    }
}
