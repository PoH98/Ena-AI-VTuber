using CefSharp;
using CefSharp.WinForms;
using Concentus.Oggfile;
using Concentus.Structs;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace AIVTuberView.Service
{
    internal class COEIROINKAudioService
    {
        private readonly HttpClient coeiroinkClient;
        private readonly ChromiumWebBrowser live2DBrowser;
        public COEIROINKAudioService(ChromiumWebBrowser live2DBrowser) 
        { 
            coeiroinkClient = new HttpClient();
            coeiroinkClient.BaseAddress = new Uri("http://localhost:50031/");
            this.live2DBrowser = live2DBrowser;
        }

        public async Task Speech(string message)
        {
            bool question = false;
            var phrasesResponse = await GetPhrases(message);
            if (message.EndsWith("?"))
            {
                question = true;
            }
            var json = new StringContent("{\"accent_phrases\":" + phrasesResponse + ",\"speedScale\":1,\"pitchScale\":0,\"intonationScale\":1,\"volumeScale\":2,\"prePhonemeLength\":0,\"postPhonemeLength\":0,\"outputSamplingRate\":44100,\"outputStereo\":false,\"kana\":\"\"}", Encoding.UTF8, "application/json");
            var audioResponse = await coeiroinkClient.PostAsync("/synthesis?speaker="+ConfigService.Instance.Speaker+"&enable_interrogative_upspeak=" + question, json);
            using (Stream output = await audioResponse.Content.ReadAsStreamAsync())
            {
                RawSourceWaveStream wavStream = new(output, new WaveFormat(44100, 1));
                ISampleProvider sampleProvider = wavStream.ToSampleProvider();
                WaveFileWriter.CreateWaveFile16("wwwroot\\voice.wav", sampleProvider);
            }
            Console.WriteLine("Triggering Speech SDK...");
            live2DBrowser.ExecuteScriptAsync("playAudio(new Audio(\"voice.wav\"))");
        }

        private async Task<string> GetPhrases(string message)
        {
            var encode = HttpUtility.UrlEncode(message);
            var response = await coeiroinkClient.PostAsync("/accent_phrases?text=" + encode + "&speaker=" + ConfigService.Instance.Speaker, null);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
