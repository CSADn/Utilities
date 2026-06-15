using NAudio.CoreAudioApi;

namespace ContolMic
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                ToggleMic();
                //GetMasterVolume();

                Console.ReadKey();
            }
        }

        static void ToggleMic()
        {
            var enumerator = new MMDeviceEnumerator();
            var recordingDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);

            if (recordingDevice == null)
            {
                throw new Exception("Device is null");
            }

            var audioEndpoint = recordingDevice.AudioEndpointVolume;

            if (audioEndpoint.Mute == true)
            {
                audioEndpoint.Mute = false;
                audioEndpoint.MasterVolumeLevelScalar = 1;
            }
            else
            {
                audioEndpoint.Mute = true;
                audioEndpoint.MasterVolumeLevelScalar = 0;
            }
        }

        static void GetMasterVolume()
        {
            var enumerator = new MMDeviceEnumerator();
            var playingDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            if (playingDevice == null)
            {
                throw new Exception("Device is null");
            }

            var audioEndpoint = playingDevice.AudioEndpointVolume;
            var volume = audioEndpoint.MasterVolumeLevelScalar;
            var mute = audioEndpoint.Mute;

            Console.WriteLine($"{volume} ({mute})");

            audioEndpoint.MasterVolumeLevelScalar = 0.4f;
        }
    }
}
