using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Media;

namespace PCMediaAutomation.Helpers
{
    public class Aimp
    {
        private string _path;
        private volatile object _lock;

        public Aimp()
        {
            _path = ConfigurationManager.AppSettings["AIMP:Path"].ToString();

            if (string.IsNullOrWhiteSpace(_path))
                throw new ArgumentNullException("AIMP:Path");

            _lock = new object();
        }


        public void Play()
        {
            Process.Start(_path, "/PLAY");
        }

        public void Pause()
        {
            Process.Start(_path, "/PAUSE");
        }

        public void Stop()
        {
            Process.Start(_path, "/STOP");
        }

        public float GetVolume()
        {
            return AudioControl.GetMasterVolume() * 100;
        }

        public void SetVolume(float value)
        {
            Task.Factory.StartNew(() =>
            {
                lock (_lock)
                    AudioControl.SetMasterVolume(value / 100);

            });
        }

        public void VolumeFadeOut(Action callback)
        {
            var current = AudioControl.GetMasterVolume();

            Task.Factory.StartNew(() =>
            {

                lock (_lock)
                {

                    for (float i = current; i > 0; i -= 0.02f )
                    {
                        if (i < 0)
                            i = 0;

                        AudioControl.SetMasterVolume(i);
                        Thread.Sleep(50);
                    }

                    callback?.Invoke();

                }

            });
        }

        public void PlayBDay()
        {
            Process.Start(_path, "/ADD_PLAY \"D:\\Piquitirri\\cumpleaños feliz.mp3\"");
        }

        public void LaunchBDay()
        {
            VolumeFadeOut(() =>
            {
                Stop();
                Thread.Sleep(500);
                SetVolume(90f);
                PlayBDay();
            });
        }
    }
}