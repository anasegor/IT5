using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;

using System.Windows.Forms;

namespace IT5
{
    public partial class Form1 : Form
    {
        private double A1, A2, A3;
        private double m1,m2, m3;
        private double sigma1, sigma2, sigma3;
        private double fd, d=0;
        double eps;
        private int N;
        float dt;
        private double[] sign0;
        private double[] spectr0;
        private double[] sign;
        private PointF[] sign0_points;
        private PointF[] spectr0_points;
        private PointF[] sign_points;
        Cmplx[] Cspectr0 ;
        
        public Graphics graphics1;
        public Graphics graphics2;
        Pen pen1 = new Pen(Color.DarkRed, 2f);
        Pen pen2 = new Pen(Color.Black, 2f);
        Pen pen3 = new Pen(Color.Blue, 2f);
        public Form1()
        {
            InitializeComponent();
            this.Text = "Фазовая проблема";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            for_A1.Text = "10";
            for_A2.Text = "7";
            for_A3.Text = "9";
            for_m1.Text = "1";
            for_m2.Text = "2";
            for_m3.Text = "3";
            for_sigma1.Text = "0,12";
            for_sigma2.Text = "0,08";
            for_sigma3.Text = "0,05";
            for_fd.Text = "30";
            for_N.Text = "7";
            for_eps.Text = "0,000001";
        }
        //обработчики кнопок
        private void CreateSignal_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            pictureBox1.Update();
            pictureBox2.Image = null;
            pictureBox2.Update();
            graphics1 = pictureBox1.CreateGraphics();
            graphics2 = pictureBox2.CreateGraphics();
            LSistema();
            //Array.Clear(sign0,0,N);
            sign0 = new double[N];
            sign0_points = new PointF[N];
            dt= (float)(1 / fd);
            CreateX(sign0,  sign0_points);
            PainNet(graphics1, pictureBox1, pen2, sign0_points, N / fd, N);
            PaintGraph(graphics1, pictureBox1, pen1, sign0_points, N / fd, N);
            spectr0 = new double[N];
            spectr0_points = new PointF[N];
            Cspectr0 = new Cmplx[N];
            Cspectr0=BPF_real(sign0, N, -1);
            float df = (float)fd / (N - 1);
            spectr0 = spectr_module(Cspectr0);
            for (int i = 0; i < N; i++)
                spectr0_points[i] = new PointF((float)(i * df), (float)spectr0[i]);
            PainNet(graphics2, pictureBox2, pen2, spectr0_points, fd, N);
            PaintGraph(graphics2, pictureBox2, pen1, spectr0_points, fd, N);

        }
        private void timer1_Tick_1(object sender, EventArgs e)
        {

            // создание буфера для нового кадра
            Bitmap Image = new Bitmap(Width, Height);
            Graphics gbuf = Graphics.FromImage(Image);
            // (создание фона)
            gbuf.Clear(Color.White);
            // тут должна идти ваша графика
            PainNet(gbuf, pictureBox1, pen2, sign0_points, N / fd, N);
            PaintGraph(gbuf, pictureBox1, pen1, sign0_points, N / fd, N);
            PaintGraph(gbuf, pictureBox1, pen3, sign_points, N / fd, N);
            for_d.Text = d.ToString();

            // теперь нужно скопировать кадр на канвас формы
            graphics1.DrawImageUnscaled(Image, 0, 0);
            // освобождаем задействованные в операции ресурсы
            gbuf.Dispose();
            Image.Dispose();
        }
        Thread th;
        private void RestoreSignal_Click(object sender, EventArgs e)
        {
            sign = new double[N];
            sign_points = new PointF[N];
            Array.Clear(sign, 0, N);
            timer1.Enabled = true;
            th = new Thread(() => {
                AlgorithmFienup(spectr0, ref sign, ref sign_points);
                timer1.Enabled = false;
            });
            th.Start();
            
        }

        private void Invert_Click(object sender, EventArgs e)
        {
            Array.Reverse(sign);
            for (int i = 0; i < N; i++)
                sign_points[i] = new PointF((float)(i * dt), (float)sign[i]);
            graphics1.Clear(Color.White);
            PainNet(graphics1, pictureBox1, pen2, sign0_points, N / fd, N);
            PaintGraph(graphics1, pictureBox1, pen1, sign0_points, N / fd, N);
            PaintGraph(graphics1, pictureBox1, pen3, sign_points, N / fd, N);

            for_d.Text = d.ToString();
        }
        
        private void ShiftT_Click(object sender, EventArgs e)
        {
            double error = 1000;
            double buff;
            double[] aarr = new double[N];

            // создаем отдельный массив для расчетов
            double[] arr = (double[])sign.Clone();

            // проходимся по всем отсчетам
            for (int j = 0; j < N; j++)
            {
                buff = 0;
                // сдвигаем график на один отсчет
                for (int i = 0; i < (N - 1); i++)
                {
                    arr[i] = aarr[i + 1];
                    buff += (arr[i] - sign0[i]) * (arr[i] - sign0[i]);   // считаем среднеквадратичную оценку (СКО)
                }
                arr[N - 1] = aarr[0];
                buff += (arr[N - 1] - sign0[N - 1]) * (arr[N - 1] - sign0[N - 1]);
                buff /= N;

                for (int i = 0; i < N; i++)
                {
                    aarr[i] = arr[i];
                }

                // ищем самую маленькую СКО из всех сдвигов - значения с таким СКО и будет наш сдвинутый график
                if (buff < error)
                {
                    error = buff;
                    for (int i = 0; i < N; i++)
                    {
                        sign[i] = arr[i];
                    }
                }
            }

            //double energy=0;
            //double[] srav = new double[N];
            //double[] clone = (double[])sign.Clone();
            //int count = 0;
            //while (true)
            //{
            //    for (int i = 0; i < N; i++)
            //        srav[i] = Math.Abs(sign0[i] - clone[i]);
            //    energy = 100 * Es(srav) / Es(sign0);
            //    if (energy < 2) break;
            //    count++;
            //    // сдвиг вправо
            //    double temp = clone[clone.Length - 1];//сохраняем последний элемент
            //    Array.Copy(clone, 0, clone, 1, clone.Length - 1);//откуда, с какого начинать, куда, с какого записыватть в новом,сколько
            //    clone[0] = temp;
            //    if(count==N+1)
            //    {
            //        Array.Reverse(clone);
            //    }
            //    if (count == 2*N + 1)
            //    {
            //        MessageBox.Show("Приближение не найдено", "Внимание!");
            //        break;
            //    }


            //}
            //sign = clone;


            for (int i = 0; i < N; i++)
                sign_points[i] = new PointF((float)(i * dt), (float)sign[i]);
            graphics1.Clear(Color.White);
            PainNet(graphics1, pictureBox1, pen2, sign0_points, N / fd, N);
            PaintGraph(graphics1, pictureBox1, pen1, sign0_points, N / fd, N);
            PaintGraph(graphics1, pictureBox1, pen3, sign_points, N / fd, N);

        }
        //методы
       
        double Dcalc(double[] a, double[] b)
        {
            d = 0;
            for (int j = 0; j < N; j++)
                d += (a[j] - b[j]) * (a[j] - b[j]);
            return d/N ;
        }

        double Gauss(double A, double mu, double sigma, double i, double fd)
        {
            return  A / (Math.Sqrt(2 * Math.PI) * sigma) * Math.Exp(-((i / fd - mu) * (i / fd - mu)) / (sigma * sigma));
        }
        public void CreateX(double[] a, PointF[] a_points)
        {

            for (int i = 0; i < a.Length; i++)
            {
                a[i] = (Gauss(A1, m1, sigma1, i, fd)+ Gauss(A2, m2, sigma2, i, fd)+ Gauss(A3, m3, sigma3, i, fd));
                a_points[i] = new PointF(i * dt, (float)a[i]);
            }
        }
        Cmplx[] BPF_real(double[] a,int n, int flag)//для действ сигналов
        {
            Cmplx[] arr = new Cmplx[n];
            for (int i = 0; i < n; i++)
                arr[i] = new Cmplx(a[i], 0);
            Cmplx.Fourier(n, ref arr, flag);
            return arr;
        }
        Cmplx[] BPF(Cmplx[] arr, int n, int flag)
        {
            Cmplx[] arr1 = new Cmplx[n];
            for (int i = 0; i < n; i++)
                arr1[i] = new Cmplx(arr[i].re, arr[i].im);
            Cmplx.Fourier(n, ref arr1, flag);
            return arr1;
        }
        //получить модуль спектра
        double[] spectr_module(Cmplx[] inn)
        {
            double[] outt = new double[inn.Length];
            for (int i = 0; i < inn.Length; i++)
		        outt[i]= Math.Sqrt(inn[i].re * inn[i].re + inn[i].im * inn[i].im);
            return outt;
        }
        Cmplx[] change_spectr(double[] X, double[] fi)
        {
            Cmplx[] x= new Cmplx[X.Length];
             for (int i = 0; i < X.Length; i++)
             {
                Cmplx z=new Cmplx(X[i]*Math.Cos(fi[i]), X[i] * Math.Sin(fi[i]));
                x[i]=z;
             }
	         return x;
        }
        double[] apply_conditions(Cmplx[] inp)// учитывая условия задачи (действительность и неотрицательность сигнала)
        {
            double[] outt = new double[inp.Length];
            for (int i = 0; i < inp.Length; i++) 
            {
                double real = inp[i].re;
                if (real < 0) real = 0;
		        outt[i]=real;
            }
            return outt;
        }

        void AlgorithmFienup( double[] sp0, ref double[] sign_new, ref PointF[] sign_points_new)
        {
            
            int n = sp0.Length;
            double[] fi = new double[n];
            double[] buf0= new double[n];
            // double[] real_x_fien = new double[n];//сигнал с реальной неотрицательной частью
            Random rnd = new Random();
            for (int i = 1; i < n; i++)//заполним фазы случайными значениями
                fi[i] = rnd.NextDouble()*2*Math.PI;
            Cmplx[] Cspectr1 = change_spectr(sp0, fi);//формируем спектр из истинного модуля и случ фазы
            Cmplx[] Csign1=BPF(Cspectr1,  n, 1);//получаем сигнал обратным фурье
            sign_new = apply_conditions(Csign1);//применяем условия к сигналу
            do
            {
                buf0 = (double[])sign_new.Clone();//сохраняем сигнал на предыдущем шаге
                Cspectr1 = BPF_real(sign_new, n, -1); //получаем комплексный спектр из сигнала с реальной частью с помощью фурье
                for (int i = 0; i < n; i++)//подменяем модуль спектра на истинный
                {
                    double k = (spectr_module(Cspectr1)[i] / sp0[i]);
                    Cspectr1[i].re = Cspectr1[i].re / k;
                    Cspectr1[i].im = Cspectr1[i].im / k;
                }
                Csign1 = BPF(Cspectr1, n, 1);//получаем комплексн сигнал обратным фурье
                sign_new = apply_conditions(Csign1);//применяем условия к сигналу
                for (int i = 0; i < N; i++)
                    sign_points_new[i] = new PointF((float)(i * dt), (float)sign_new[i]);
                d = Dcalc(sign_new, buf0);
                
            } while (d > eps);

            //sign_new =(double[]) real_x_fien.Clone();//сохраняем сигнал
        }


        //РИСОВАЛКА
        public void LSistema()
        {

            if (for_A1.Text != "" || for_A2.Text != "" || for_A3.Text != "")
            {
                A1 = Convert.ToDouble(for_A1.Text);
                A2 = Convert.ToDouble(for_A2.Text);
                A3 = Convert.ToDouble(for_A3.Text);
            }
            else { MessageBox.Show("параметры A по умолчанию", "Внимание!"); }
            if (for_m1.Text != "" || for_m2.Text != "" || for_m3.Text != "")
            {
                m1 = Convert.ToDouble(for_m1.Text);
                m2 = Convert.ToDouble(for_m2.Text);
                m3 = Convert.ToDouble(for_m3.Text);
            }
            else { MessageBox.Show("параметры v по умолчанию", "Внимание!"); }
            if (for_sigma1.Text != "" || for_sigma2.Text != "" || for_sigma3.Text != "")
            {
                sigma1 = Convert.ToDouble(for_sigma1.Text);
                sigma2 = Convert.ToDouble(for_sigma2.Text);
                sigma3 = Convert.ToDouble(for_sigma3.Text);
            }
            else { MessageBox.Show("параметры f по умолчанию", "Внимание!"); }
            if (for_fd.Text != "" || for_N.Text != "")
            {
                fd = Convert.ToDouble(for_fd.Text);
                eps = Convert.ToDouble(for_eps.Text);
                N = Convert.ToInt32(for_N.Text);
                N = (int)Math.Pow(2, N);

            }
            else { MessageBox.Show("параметры f_d,N,a по умолчанию", "Внимание!"); }
        }
        static public int padding = 10;
        static public int left_keys_padding = 20;
        static public int actual_left = 30;
        static public int actual_top = 10;
        public void PainNet(Graphics gr, PictureBox pictureBox, Pen penG, PointF[] points, double toX, int n)//Отрисовка сетки с подписями
        {
            PointF[] copy_points = (PointF[])points.Clone();
            int wX, hX;
            wX = pictureBox.Width;
            hX = pictureBox.Height;
            Point KX1 = new Point(30, hX - 10);
            Point KX2 = new Point(wX - 10, hX - 10);
            gr.DrawLine(penG, KX1, KX2);
            Point KY1 = new Point(30, 10);
            Point KY2 = new Point(30, hX - 10);
            gr.DrawLine(penG, KY1, KY2);
            int actual_width = wX - 2 * padding - left_keys_padding;
            int actual_height = hX - 2 * padding;
            int actual_bottom = actual_top + actual_height;
            int actual_right = actual_left + actual_width;
            float maxY = GetMaxY(copy_points, n);
            int grid_size = 11;
            Pen GridPen = new Pen(Color.Gray, 1f);
            PointF K1, K2, K3, K4;
            for (double i = 0.5; i < grid_size; i += 1.0)
            {
                //вертикальная
                K1 = new PointF((float)(actual_left + i * actual_width / grid_size), actual_top);
                K2 = new PointF((float)(actual_left + i * actual_width / grid_size), actual_bottom);
                gr.DrawLine(GridPen, K1, K2);
                double v = 0 + i * (toX - 0) / grid_size;
                string s1 = v.ToString("0.00");
                gr.DrawString(s1, new Font("Arial", 7), Brushes.Green, actual_left + (float)i * actual_width / grid_size, actual_bottom + 0);

                K3 = new PointF(actual_left, (float)(actual_top + i * actual_height / grid_size));
                K4 = new PointF(actual_right, (float)(actual_top + i * actual_height / grid_size));
                gr.DrawLine(GridPen, K3, K4);
                double g = 0 + i * (double)(maxY / grid_size);
                string s2 = g.ToString("0.00");
                gr.DrawString(s2, new Font("Arial", 7), Brushes.Green, actual_left - left_keys_padding, actual_bottom - (float)i * actual_height / grid_size);
            }

        }
        static public void PaintGraph(Graphics gr, PictureBox pictureBox, Pen penG, PointF[] points, double toX, int n)//Отрисовка графика
        {
            PointF[] copy_points = (PointF[])points.Clone();
            int wX, hX;
            wX = pictureBox.Width;
            hX = pictureBox.Height;
            int actual_width = wX - 2 * padding - left_keys_padding;
            int actual_height = hX - 2 * padding;
            int actual_bottom = actual_top + actual_height;
            int actual_right = actual_left + actual_width;
            float maxY = GetMaxY(copy_points, n); ;
            PointF actual_tb = new PointF(actual_top, actual_bottom);//для y
            PointF actual_rl = new PointF(actual_right, actual_left);//для x
            PointF from_toX = new PointF(0, (float)(toX));
            PointF from_toY = new PointF(0, maxY * (float)1.2);
            convert_range_graph(copy_points, actual_rl, actual_tb, from_toX, from_toY);
            gr.DrawLines(penG, copy_points);
        }
        static public float GetMaxY(PointF[] points, int n)
        {
            float m = 0;
            for (int i = 0; i < n; i++)
                if (m < Math.Abs(points[i].Y)) m = Math.Abs(points[i].Y);//макс значение Y
            return m;
        }
        static public void convert_range_graph(PointF[] data, PointF actual_rl, PointF actual_tb, PointF from_toX, PointF from_toY)
        {
            //actual-размер:X-top/right Y-right,left
            //from_to: X-мин, Y-макс
            float kx = (actual_rl.X - actual_rl.Y) / (from_toX.Y - from_toX.X);
            float ky = (actual_tb.X - actual_tb.Y) / (from_toY.Y - from_toY.X);
            for (int i = 0; i < data.Length; i++)
            {
                data[i].X = (data[i].X - from_toX.X) * kx + actual_rl.Y;
                data[i].Y = (data[i].Y - from_toY.X) * ky + actual_tb.Y;
            }
        }

    }
}
