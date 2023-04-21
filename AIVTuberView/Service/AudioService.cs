using CefSharp;
using CefSharp.WinForms;
using Concentus.Oggfile;
using Concentus.Structs;
using Google.Cloud.TextToSpeech.V1;
using NAudio.Wave;

namespace AIVTuberView.Service
{
    internal class AudioService
    {
        private readonly TextToSpeechClient client;
        private readonly VoiceSelectionParams voiceSelection;
        private readonly AudioConfig audioConfig;
        private readonly ChromiumWebBrowser live2DBrowser;
        public AudioService(ChromiumWebBrowser live2DBrowser)
        {
            client = new TextToSpeechClientBuilder
            {
                Credential = ConfigService.Instance.Credential
            }.Build();
            voiceSelection = new VoiceSelectionParams
            {
                Name = "en-US-Standard-H",
                LanguageCode = "en-US"
            };
            audioConfig = new AudioConfig
            {
                AudioEncoding = AudioEncoding.OggOpus,
                Pitch = 1.5,
                SpeakingRate = 0.8
            };
            this.live2DBrowser = live2DBrowser;
        }
        /// <summary>
        /// Read the text loudly
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task Speech(string message)
        {
            SynthesisInput input = new()
            {
                Text = message
            };
            SynthesizeSpeechResponse response = await client.SynthesizeSpeechAsync(input, voiceSelection, audioConfig);
            using (Stream output = new MemoryStream())
            {
                using MemoryStream pcmStream = new();
                response.AudioContent.WriteTo(output);
                OpusDecoder decoder = OpusDecoder.Create(48000, 1);
                OpusOggReadStream oggIn = new(decoder, output);
                while (oggIn.HasNextPacket)
                {
                    short[] packet = oggIn.DecodeNextPacket();
                    if (packet != null)
                    {
                        for (int i = 0; i < packet.Length; i++)
                        {
                            byte[] bytes = BitConverter.GetBytes(packet[i]);
                            pcmStream.Write(bytes, 0, bytes.Length);
                        }
                    }
                }
                pcmStream.Position = 0;
                RawSourceWaveStream wavStream = new(pcmStream, new WaveFormat(48000, 1));
                ISampleProvider sampleProvider = wavStream.ToSampleProvider();
                WaveFileWriter.CreateWaveFile16("wwwroot\\voice.wav", sampleProvider);
            }
            Console.WriteLine("Triggering Speech SDK...");
            live2DBrowser.ExecuteScriptAsync("playAudio(new Audio(\"voice.wav\"))");
        }
    }
}
