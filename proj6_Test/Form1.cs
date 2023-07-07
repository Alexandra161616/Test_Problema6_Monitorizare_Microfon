using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace proj6_Test
{
    public partial class Form1 : Form
    {
        private bool isMicrophoneUsed;
        private WaveInEvent waveIn;
        public Form1()
        {
            InitializeComponent();
            InitializeMicrophoneListener();
        }

        private void InitializeMicrophoneListener()
        {
            waveIn = new WaveInEvent();
            waveIn.WaveFormat = new WaveFormat(44100, 1); // Mono, 44.1kHz
            waveIn.DataAvailable += OnMicrophoneDataAvailable;
            waveIn.StartRecording();
        }

        private void OnMicrophoneDataAvailable(object sender, WaveInEventArgs e)
        {
            //Analizeaza date audio pentru a determina daca microfonul e utilizat

            //Vom presupune ca orice sunet care nu e liniste inseamna ca microfonul e folosit
            float maxSample = 0;
            for (int i = 0; i < e.BytesRecorded; i += 2)
            {
                short sample = BitConverter.ToInt16(e.Buffer, i);
                float sampleValue = sample / 32768f;
                if (Math.Abs(sampleValue) > maxSample)
                {
                    maxSample = Math.Abs(sampleValue);
                }
            }

            bool currentlyUsed = maxSample > 0.1; // Ajusteaza pragul dupa cum e nevoie

            if (currentlyUsed != isMicrophoneUsed)
            {
                isMicrophoneUsed = currentlyUsed;
                if (isMicrophoneUsed)
                {
                    string applicationName = GetApplicationUsingMicrophone();
                    NotifyMicrophoneUsage(applicationName);
                }
            }
        }

        private string GetApplicationUsingMicrophone()
        {
            IntPtr foregroundWindowHandle = GetForegroundWindow();
            uint foregroundProcessId;
            GetWindowThreadProcessId(foregroundWindowHandle, out foregroundProcessId);

            try
            {
                var process = Process.GetProcessById((int)foregroundProcessId);
                return process.ProcessName;
            }
            catch (ArgumentException)
            {
                return "Unknown";
            }
        }

        private void NotifyMicrophoneUsage(string applicationName)
        {
            MessageBox.Show($"Microfonul e utilizat de aplicatia: {applicationName}");
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            waveIn.StopRecording();
            waveIn.Dispose();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Visible = false;
            this.Opacity = 0;
        }
    }
}
