using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SRV_1
{
    public partial class Form1 : Form
    {
        static Queue<string> mes = new Queue<string>();
        Thread send;
        Thread get;
        Thread generator;
        Semaphore sem;
        List<Thread> pool;
        int count;
        int cc = 0;
        bool works;
        public Form1()
        {
            InitializeComponent();
        }
        //Обычная очередь
        public void sending()
        {
            for(int i = 0; i < numericUpDown1.Value; i++)
            {
                string message = (i + 1).ToString();
                lock (mes)
                {
                    mes.Enqueue(message);
                }
                BeginInvoke((Action)(() => { textBox1.Text += message + '\r' + '\n'; }));
                Thread.Sleep(300);
            }
           
        }

        public void getter()
        {
            while (true)
            {
                if (mes.Count != 0)
                {
                    lock (mes)
                    {
                        BeginInvoke((Action)(() => { textBox2.Text += mes.Dequeue() + '\r' + '\n'; }));
                    }
                }
                Thread.Sleep(600);
            }
        }
        //Пулл потоков 
        public void generate()
        {
            count = Convert.ToInt32(numericUpDown1.Value);
            sem = new Semaphore(3, count);
            for (int i = 0; i < count; i++)
            {
                lock (mes)
                {
                    mes.Enqueue((i + 1).ToString() + '\r' + '\n');
                    Invoke((MethodInvoker)(() => textBox1.Text += (i + 1).ToString()));
                   
                }
                Thread.Sleep(200);
            }
        }

        public void work(object index)
        {
            while (works)
            {
                Thread.Sleep(300);
                sem.WaitOne();
                
                lock (mes)
                {
                    if (mes.Count!=0)
                    {
                        Thread.Sleep(300);
                        BeginInvoke((Action)(() => { textBox2.Text += mes.Dequeue() + " got by" + index.ToString() + '\r'+'\n'; }));
                        

                    }
                    else
                    {
                        Thread.Sleep(200);
                    }
                 
                }
                
                Thread.Sleep(1000);
                sem.Release(1);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            send = new Thread(sending);
            get = new Thread(getter);
            send.Start();
            get.Start();
        }
       

        private void button2_Click(object sender, EventArgs e)
        {
            
            send?.Abort();
            get?.Abort();
            works = false;
            generator?.Abort();
            foreach (var p in pool ?? new List<Thread>())
                 p.Abort();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            works= true;
            generator = new Thread(generate);
            pool = new List<Thread>();
            generator.Start();
            for(int i = 0; i < numericUpDown1.Value; i++)
            {
                pool.Add(new Thread(work));
                pool[i].Start(i);
            }
        }
    }
}
