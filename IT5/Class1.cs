using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT5
{
    // Быстрое фурье преобразование iss= -1 -prjamoe  +1 -obratnoe, спектр записывается в вектор сигнала
    //MyClass[] arr = new MyClass[N];
    // for(int i=0;i<N;i++)
    //{
    //arr[i] = new MyClass();
    //}
    public class Cmplx
    {
        public double re;
        public double im;

        public Cmplx() { re = im = 0; }
        public Cmplx(double x, double y) { re = x; im = y; }
        public static Cmplx operator *(Cmplx x, Cmplx y)
        {
            Cmplx z = new Cmplx();
            z.re = x.re * y.re - x.im * y.im;
            z.im = x.re * y.im + y.re * x.im;
            return z;
        }

        //----------------------------------------------------------------
        public static Cmplx operator /(Cmplx x, Cmplx y)
        {
            Cmplx z = new Cmplx();
            double y2 = y.re * y.re + y.im * y.im;
            if (y2 < 10e-40) return z;
            z.re = (x.re * y.re + x.im * y.im) / y2;
            z.im = (y.re * x.im - x.re * y.im) / y2;
            return z;
        }

        public static Cmplx operator /(Cmplx x, double y)
        {
            Cmplx z = new Cmplx();
            if (y < 10e-40) return z;
            z.re = x.re / y;
            z.im = x.im / y;
            return z;
        }

        public static Cmplx operator +(Cmplx x, Cmplx y)
        {
            Cmplx z = new Cmplx
            {
                re = x.re + y.re,
                im = x.im + y.im
            };
            return z;
        }

        public static Cmplx operator -(Cmplx x, Cmplx y)
        {
            Cmplx z = new Cmplx();
            z.re = x.re - y.re;
            z.im = x.im - y.im;
            return z;
        }

        /*public static cmplx operator = (cmplx c)
        {
            re = c.re;
            im = c.im;
            return *this;
        }*/
        public static void Fourier(int v_size, ref Cmplx[] F, double iss)
        {
            Cmplx temp = new Cmplx();
            Cmplx w;
            Cmplx c = new Cmplx();
            long i, i1, j, j1, istep;
            long m, mmax;
            long n = v_size;
            double fn, r1, theta;
            fn = (double)n;
            double r = Math.PI * iss;
            j = 1;
            for (i = 1; i <= n; i++)
            {
                i1 = i - 1;
                if (i < j)
                {
                    j1 = j - 1;
                    temp.im = F[j1].im;
                    temp.re = F[j1].re;
                    F[j1].im = F[i1].im;
                    F[j1].re = F[i1].re;
                    F[i1].im = temp.im;
                    F[i1].re = temp.re;
                }
                m = n / 2;
                while (j > m) { j -= m; m = (m + 1) / 2; }
                j += m;
            }
            mmax = 1;
            while (mmax < n)
            {
                istep = 2 * mmax;
                r1 = r / (double)mmax;
                for (m = 1; m <= mmax; m++)
                {
                    theta = r1 * (double)(m - 1);
                    w = new Cmplx(Math.Cos(theta), Math.Sin(theta));
                    for (i = m - 1; i < n; i += istep)
                    {
                        j = i + mmax;
                        c.im = F[j].im;
                        c.re = F[j].re;
                        temp.im = (w * c).im;
                        temp.re = (w * c).re;
                        F[j].im = (F[i] - temp).im;
                        F[j].re = (F[i] - temp).re;
                        F[i].im = (F[i] + temp).im;
                        F[i].re = (F[i] + temp).re;
                    }
                }
                mmax = istep;
            }
            if (iss > 0) for (i = 0; i < n; i++) { F[i].re /= fn; F[i].im /= fn; }
            return;
        }
    }
}
