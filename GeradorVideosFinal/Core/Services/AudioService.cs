using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Application.interfaces;
using ElevenLabs;
using RestSharp;
using KokoroSharp;
using KokoroSharp.Core;
using KokoroSharp.Processing;
using KokoroSharp.Utilities;

using Microsoft.ML.OnnxRuntime;

using System.Diagnostics;

using NAudio.Wave;

namespace Services
{
    public class AudioService : IAudioService
    {

        /// <summary>
        /// Gera áudio a partir do texto fornecido utilizando a API da ElevenLabs.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public async Task<byte[]> GenerateFinalAudio(string text)
        {
            try
            {
                string apiKey = "sk_79f3ee464faa45f4cce1b11bf146b18d8879526a19762cd4";

                using var client = new ElevenLabsClient(apiKey);

                var voices = await client.Voices.GetVoicesAsync();
                var voice = voices.Voices.FirstOrDefault(x => x.VoiceId == "7lu3ze7orhWaNeSPowWx");
                if (voice == null)
                    throw new Exception("Nenhuma voz encontrada.");

                var audioStream = await client.TextToSpeech.CreateTextToSpeechByVoiceIdStreamAsync(voice.VoiceId, text);

                return audioStream.ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao gerar áudio: {ex.Message}");
            }
        }

        /// <summary>
        /// Calcula a duração do áudio em segundos a partir dos bytes do arquivo de áudio.
        /// </summary>
        /// <param name="AudioFileBytes"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>

        /// 

        public async Task GenerateTemporaryAudio(string text, string path)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "piper",
                Arguments = $"-m C:\\piper\\pt_BR-faber-medium.onnx -c C:\\piper\\pt_BR-faber-medium.onnx.json -f {path}",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };

            process.Start();

            await process.StandardInput.WriteAsync(text);
            process.StandardInput.Close();

            await process.WaitForExitAsync();
        }

        public async Task GenerateTemporaryAudio(string text, string modelPath, string path)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = $"{AppContext.BaseDirectory}\\..\\..\\..\\..\\Assets\\piper\\piper.exe",
                Arguments = $"-m \"{AppContext.BaseDirectory}\\..\\..\\..\\..\\Assets\\piper\\pt_BR-faber-medium.onnx\" -c \"{AppContext.BaseDirectory}\\..\\..\\..\\..\\Assets\\piper\\pt_BR-faber-medium.onnx.json\" -f {path}",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };

            process.Start();

            await process.StandardInput.WriteAsync(text);
            process.StandardInput.Close();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                throw new Exception($"Erro ao gerar áudio");


            if (process.ExitCode != 0)
                throw new Exception($"Erro ao gerar áudio");
        }

        //public async Task GenerateTemporaryAudio(string text, string modelPath, string path)
        //{
        //    KokoroVoiceManager.GetVoices(KokoroLanguage.BrazilianPortuguese, KokoroGender.Male).ForEach(v => Console.WriteLine(v.Name));

        //    KokoroVoice? Voice = KokoroVoiceManager.GetVoices(KokoroLanguage.BrazilianPortuguese, KokoroGender.Male)[0];

        //    var Synthesizer = new KokoroWavSynthesizer(modelPath);


        //    byte[] AudioBytes = Synthesizer.Synthesize(text, Voice);

        //    File.WriteAllBytes(path, AudioBytes);

        //    KokoroTTS tts = KokoroTTS.LoadModel();
        //    KokoroVoice heartVoice = KokoroVoiceManager.GetVoice("af_heart");
        //    SynthesisHandle handle = tts.SpeakFast(text, heartVoice);
        //    handle.OnSpeechStarted = write => Console.WriteLine("Speech started");
        //    handle.OnSpeechCompleted = write => Console.WriteLine("Speech completed");

        //    KokoroVoiceManager.LoadVoicesFromPath("C:\\KokoroModel");

        //    foreach (var voice in KokoroVoiceManager.GetVoices(KokoroLanguage.BrazilianPortuguese)) { Debug.WriteLine(voice.Name); }

            

        //    if (Voice == null)
        //        throw new Exception("Voz não encontrada.");

        //    tts.OnSpeechStarted += (s) => Debug.WriteLine($"Started:   {new string(s.PhonemesToSpeak)}");
        //    tts.OnSpeechProgressed += (p) => Debug.WriteLine($"Progress:  {new string(p.SpokenText_BestGuess)}");
        //    tts.OnSpeechCompleted += (c) => Debug.WriteLine($"Completed: {new string(c.PhonemesSpoken)}");
        //    tts.OnSpeechCanceled += (c) => Debug.WriteLine($"Canceled:  {new string(c.SpokenText_BestGuess)}");

            ////KokoroTTS tts = KokoroTTS.LoadModel(); 
            ////KokoroVoice heartVoice = KokoroVoiceManager.GetVoice("af_heart");
            ////SynthesisHandle handle = tts.SpeakFast(text, heartVoice);
            ////handle.OnSpeechStarted = write => Console.WriteLine("Speech started");           
            ////handle.OnSpeechCompleted = write => Console.WriteLine("Speech completed");

            //using KokoroTTS tts = KokoroTTS.LoadModel();

            //KokoroVoiceManager.LoadVoicesFromPath("C:\\KokoroModel");

            //foreach (var voice in KokoroVoiceManager.GetVoices(KokoroLanguage.BrazilianPortuguese)) { Debug.WriteLine(voice.Name); }

            //KokoroVoice? Voice = KokoroVoiceManager.GetVoices(KokoroLanguage.BrazilianPortuguese, KokoroGender.Male)[0];

            //if (Voice == null)
            //    throw new Exception("Voz não encontrada.");

            //tts.OnSpeechStarted += (s) => Debug.WriteLine($"Started:   {new string(s.PhonemesToSpeak)}");
            //tts.OnSpeechProgressed += (p) => Debug.WriteLine($"Progress:  {new string(p.SpokenText_BestGuess)}");
            //tts.OnSpeechCompleted += (c) => Debug.WriteLine($"Completed: {new string(c.PhonemesSpoken)}");
            //tts.OnSpeechCanceled += (c) => Debug.WriteLine($"Canceled:  {new string(c.SpokenText_BestGuess)}");

            //KokoroWavSynthesizer wavSynthesizer = new KokoroWavSynthesizer(modelPath);

            //byte[] audioBytes = await wavSynthesizer.SynthesizeAsync(text, Voice);

            //using var writer = new WaveFileWriter(path, KokoroPlayback.waveFormat);
            //writer.Write(audioBytes, 0, audioBytes.Length);


        //    KokoroWavSynthesizer wavSynthesizer = new KokoroWavSynthesizer(modelPath);

        //    byte[] audioBytes = await wavSynthesizer.SynthesizeAsync(text, Voice);

        //    using var writer = new WaveFileWriter(path, KokoroPlayback.waveFormat);
        //    writer.Write(audioBytes, 0, audioBytes.Length);

        //}

        public double GetAudioDuration(string filePath)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffprobe",
                    Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{filePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            string output = process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            if (double.TryParse(output, System.Globalization.CultureInfo.InvariantCulture, out double duracao))
                return duracao;

            throw new Exception(process.StandardError.ReadToEnd().Trim());
        }
    }
}
