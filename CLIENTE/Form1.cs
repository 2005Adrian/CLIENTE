using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Threading;
using System.Net.Sockets;
using System.IO;

namespace Cliente
{
    public partial class Form1 : Form
    {
        static private NetworkStream? stream;
        static private StreamWriter? streamw;
        static private StreamReader? streamr;
        static private TcpClient client = new TcpClient();
        static private string nick = "unknown";

        private delegate void DAddItem(string s);

        private void AddItem(string s)
        {
            if (listBox1.InvokeRequired)
            {
                var d = new DAddItem(AddItem);
                Invoke(d, new object[] { s });
            }
            else
            {
                listBox1.Items.Add(s);
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (streamw != null)
            {
                streamw.WriteLine(textBox1.Text);
                streamw.Flush();
                textBox1.Clear();
            }
        }

        void Listen()
        {
            while (client.Connected)
            {
                try
                {
                    string? message = streamr?.ReadLine();
                    if (message != null)
                    {
                        AddItem(message);
                    }
                }
                catch (IOException ex)
                {
                    MessageBox.Show("Conexión perdida: " + ex.Message);
                    Application.Exit();
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show("Error al actualizar la interfaz: " + ex.Message);
                    Application.Exit();
                }
            }
        }

        void Conectar()
        {
            try
            {
                client.Connect("192.168.166.65", 8000);
                if (client.Connected)
                {
                    stream = client.GetStream();
                    streamw = new StreamWriter(stream);
                    streamr = new StreamReader(stream);

                    streamw.WriteLine(nick);
                    streamw.Flush();

                    Thread t = new Thread(Listen)
                    {
                        IsBackground = true
                    };
                    t.Start();
                }
                else
                {
                    MessageBox.Show("Servidor no disponible");
                    Application.Exit();
                }
            }
            catch (SocketException ex)
            {
                MessageBox.Show("Error al conectar con el servidor: " + ex.Message);
                Application.Exit();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            label1.Visible = false;
            textBox2.Visible = false;
            button2.Visible = false;
            listBox1.Visible = true;
            textBox1.Visible = true;
            Enviar.Visible = true;

            nick = textBox2.Text;

            Conectar();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (client != null && client.Connected)
            {
                streamw?.Close();
                streamr?.Close();
                stream?.Close();
                client.Close();
            }
        }
    }
}
