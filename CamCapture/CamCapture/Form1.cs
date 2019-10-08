using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using NAudio.Wave;

namespace CamCapture
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();  
        }

        private FilterInfoCollection CameraCapture;
        private FilterInfoCollection AudioS;
        private VideoCaptureDevice video;

        private void Form1_Load(object sender, EventArgs e)
        {
            CameraCapture = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            AudioS = new FilterInfoCollection(FilterCategory.AudioInputDevice);

            foreach (FilterInfo Device in CameraCapture)
            {
                comboBox1.Items.Add(Device.Name);
            }

            comboBox1.SelectedIndex = 0;
            video = new VideoCaptureDevice();

            foreach (FilterInfo Device in AudioS)
            {
                comboBoxAudio.Items.Add(Device.Name);
            }

            comboBoxAudio.SelectedIndex = 0;
        }

        WaveIn sourceStream = null;
        DirectSoundOut waveOut = null;
        WaveFileWriter waveWriter = null;

        private void Button1_Click(object sender, EventArgs e)
        {
            video = new VideoCaptureDevice(CameraCapture[comboBox1.SelectedIndex].MonikerString);
            video.NewFrame += new NewFrameEventHandler(Video_NewFrame);
            video.SnapshotFrame += Video_SnapshotFrame;
            video.Start();

            
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        private void Video_SnapshotFrame(object sender, NewFrameEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        private void Video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            video.Stop();
            pictureBox1.Image = null;
            pictureBox1.Invalidate();
            pictureBox2.Image = null;
            pictureBox2.Invalidate();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            video.Stop();
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = (Bitmap)pictureBox1.Image.Clone();
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            if (video.IsRunning == true)
            {
                video.Stop();
            }
            Application.Exit(null);
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void Button6_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image != null)
            {
                Bitmap varBmp = new Bitmap(pictureBox2.Image);
                varBmp.Save(@"D:\a.png", System.Drawing.Imaging.ImageFormat.Png);
                varBmp.Dispose();
            }
            else
            {
                MessageBox.Show("Null exception");
            }
        }

        private void Button7_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image != null)
            {
                byte[] BinaryCode = (byte[])new ImageConverter().ConvertTo(pictureBox2.Image, typeof(byte[]));
                var fs = new FileStream("D:\\convert", FileMode.Create, FileAccess.Write);

                var a = BitConverter.ToString(BinaryCode)
                    .Split('-') // -> [] // AA-03-FF...
                    .Select(b => Convert.ToString(Convert.ToInt64(b, 16), 2));

                /*
                string hexString = BitConverter.ToString(BinaryCode);
                string[] hexArray = hexString.Split('-'); // -> [] // AA-03-FF...
                List<string> binnaryArray = new List<string>();
                foreach (var hex in hexArray)
                {
                    long hexAsDec = Convert.ToInt64(hex, 16);
                    string bin = Convert.ToString(hexAsDec, 2);
                    binnaryArray.Add(bin);
                }*/

                File.WriteAllText(@"D:\convert.txt", string.Join(Environment.NewLine, a));
            }
        }

        private void Button8_Click(object sender, EventArgs e)
        {
            int deviceNumber = comboBoxAudio.SelectedIndex;

            sourceStream = new WaveIn();
            sourceStream.DeviceNumber = deviceNumber;
            sourceStream.WaveFormat = new WaveFormat(44100, NAudio.Wave.WaveIn.GetCapabilities(deviceNumber).Channels);

            WaveInProvider waveIn = new WaveInProvider(sourceStream);

            waveOut = new DirectSoundOut();
            waveOut.Init(waveIn);

            sourceStream.StartRecording();
            waveOut.Play();
        }

        private void Button9_Click(object sender, EventArgs e)
        {
            if (comboBoxAudio.Items.Count == 0) return;

            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Wave File (*.wav)|*.wav;";
            if (save.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            int deviceNumber = comboBoxAudio.SelectedIndex;

            sourceStream = new WaveIn();
            sourceStream.DeviceNumber = deviceNumber;
            sourceStream.WaveFormat = new WaveFormat(44100, WaveIn.GetCapabilities(deviceNumber).Channels);

            sourceStream.DataAvailable += new EventHandler<WaveInEventArgs>(sourceStream_DataAvailable);
            waveWriter = new WaveFileWriter(save.FileName, sourceStream.WaveFormat);

            sourceStream.StartRecording();
        }

        private void sourceStream_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveWriter == null)
                return;

            waveWriter.WriteData(e.Buffer, 0, e.BytesRecorded);
            waveWriter.Flush();
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }

            if (sourceStream != null)
            {
                sourceStream.StopRecording();
                sourceStream.Dispose();
                sourceStream = null;
            }

            if (waveWriter != null)
            {
                waveWriter.Dispose();
                waveWriter = null;
            }
        }
    }
}
