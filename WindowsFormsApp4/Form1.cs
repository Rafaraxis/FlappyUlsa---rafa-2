using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio;
using NAudio.Wave;
using NAudio.Dsp;
using System.Diagnostics;
using System.Threading;

namespace WindowsFormsApp4
{
    public partial class juego : Form
    {
        int contadormovimiento = 5;
        bool volararriba = false;
        int distancia = 0;
        Random posicionrandom = new Random();
        WaveIn wavein;
        WaveFormat formato;
       

        public juego()
        {
            InitializeComponent();
            //teclado espacio
            this.KeyPreview = true;
            //cuando el juego comienze
            iniciarjuego();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Inicializar wavein
            wavein = new WaveIn();
            wavein.WaveFormat = new WaveFormat(44100, 16, 1);
            formato = wavein.WaveFormat;

            wavein.DataAvailable += OnDaraAvailable;
            
            wavein.StartRecording();

            //Cuando el tubo se mueva aparecere los nuevos tubos.
            if (tuboabajo.Location.X > -140)
            {
                tuboabajo.Location = new Point((tuboabajo.Location.X) - 1, tuboabajo.Location.Y);
                tuboarriba.Location = new Point((tuboarriba.Location.X) - 1, tuboarriba.Location.Y);
            }
            else
            {
                //en la palabra distancia es donde apareceran los nuevos tubos con diferente lugar 
                distancia = posicionrandom.Next(-170, 118);
                tuboabajo.Location = new Point(400, 319 + distancia);
                tuboarriba.Location = new Point(400, -173 + distancia);
            }
        }
        void OnDaraAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;
            int bytesGrabados = e.BytesRecorded;


            double nummuestras = bytesGrabados / 2;
            int exponente = 1;
            int numeroMuestrasComplejas = 0;
            int bitsMaximos = 0;
            do
            {
                bitsMaximos = (int)Math.Pow(2, exponente);
                exponente++;

            } while (bitsMaximos < nummuestras);



            exponente -= 2;
            numeroMuestrasComplejas = bitsMaximos / 2;

            Complex[] muestrasComplejas =
                new Complex[numeroMuestrasComplejas];

            for (int i = 0; i < bytesGrabados; i += 2)
            {
                short muestra = (short)(buffer[i + 1] << 8 | buffer[i]);


                float muestra32bits = (float)muestra / 32768.0f;
               
                if (i / 2 < numeroMuestrasComplejas)
                {
                    muestrasComplejas[i / 2].X = muestra32bits;
                }




            }

            FastFourierTransform.FFT(true, exponente, muestrasComplejas);
            float[] valoresAbsolutos =
                new float[muestrasComplejas.Length];

            for (int i = 0; i < muestrasComplejas.Length; i++)
            {
                valoresAbsolutos[i] = (float)
                Math.Sqrt((muestrasComplejas[i].X * muestrasComplejas[i].X) +
                (muestrasComplejas[i].Y * muestrasComplejas[i].Y));
            }
            int indiceMaximo =
                valoresAbsolutos.ToList().IndexOf(
                    valoresAbsolutos.Max());
            float frecuenciaFundamental =
                (float)(indiceMaximo * wavein.WaveFormat.SampleRate) /
                (float)valoresAbsolutos.Length;
            


            if (frecuenciaFundamental > 500 && frecuenciaFundamental < 1000)
            {
                volararriba = true;
            }

            if (frecuenciaFundamental > 0 && frecuenciaFundamental < 499)
            {
                volararriba = false;
            }


        }
        public void iniciarjuego()
        {
            jugador.Location = new Point(19, 225);
            //Aqui le dices a la palabra distancia que siempre sea random 
            distancia = posicionrandom.Next(-160, 118);
            //al poner distancia a los tubos
            tuboarriba.Location = new Point(270, -173 - distancia);
            tuboabajo.Location = new Point(270, 319 - distancia);
            //el numero de puntaje siempre conenzara con 0
            puntios.Text = "0";

        }
        private void timer1_Tick_1(object sender, EventArgs e)
        {
            //Animacion de alas pero no se refleja 
            int cantidadmovimientos = 55;
            if (contadormovimiento <= cantidadmovimientos)
            {
                jugador.Image = Properties.Resources.tenorio;
                contadormovimiento++;
            }

            if ((contadormovimiento > cantidadmovimientos / 2) && (contadormovimiento <= cantidadmovimientos * 2))
            {
                jugador.Image = Properties.Resources.tenorio;
                contadormovimiento++;
            }

            contadormovimiento = (contadormovimiento > cantidadmovimientos * 2) ? 0 : contadormovimiento;

            int ly = jugador.Location.Y;
            int lx = jugador.Location.X;

            //que se va callendo al alumno
            if (volararriba)
            {
                ly = ly - 15;
                volararriba = false;
            }
            else
            {
                ly++;
            }

            jugador.Location = new Point(jugador.Location.X, ly);

            //cuando el alumno choca con un obstaculo se reiniciara el juego
            if ((jugador.Bounds.IntersectsWith(reprobado.Bounds))||(jugador.Bounds.IntersectsWith(tuboarriba.Bounds)) || (jugador.Bounds.IntersectsWith(tuboabajo.Bounds)))
            {
                iniciarjuego();
                wavein.StopRecording();
            }
            //el puntaje seguira al alumno
            puntios.Location = new Point(jugador.Location.X + 30, jugador.Location.Y - 25);
            puntios.Text = (tuboarriba.Location.X == jugador.Location.X) ? Convert.ToString
                ((Convert.ToInt32(puntios.Text) + 1)).ToString() : puntios.Text; 

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
        
        }

        private void juego_Keypress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Space))
            {
                volararriba = true;
            }
        }
    }

}
