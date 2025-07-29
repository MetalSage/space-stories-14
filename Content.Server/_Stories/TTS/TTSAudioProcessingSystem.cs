using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Content.Shared._Stories.SCCVars;
using Robust.Shared.Configuration;

namespace Content.Server._Stories.TTS
{
    public sealed class TtsAudioProcessingSystem : EntitySystem
    {
        [Dependency] private readonly IConfigurationManager _cfg = default!;

        private ISawmill _sawmill = default!;
        private bool _radioEffectEnabled = false;
        private string _ffmpegPath = "ffmpeg";
        private string _ffmpegArgs = "";

        public override void Initialize()
        {
            base.Initialize();
            _sawmill = Logger.GetSawmill("tts.processing");

            _cfg.OnValueChanged(SCCVars.TTSRadioEffect, v => _radioEffectEnabled = v, true);
            _cfg.OnValueChanged(SCCVars.TTSFfmpegPath, v => _ffmpegPath = v, true);
            _cfg.OnValueChanged(SCCVars.TTSFfmpegArguments, v => _ffmpegArgs = v, true);
        }

        public async Task<byte[]> ApplyRadioEffect(byte[] oggData)
        {
            return await ApplyEffect(oggData, _ffmpegArgs, "standard radio");
        }

        private async Task<byte[]> ApplyEffect(byte[] oggData, string ffmpegArgs, string effectName)
        {
            if (!_radioEffectEnabled)
                return oggData;

            if (string.IsNullOrWhiteSpace(_ffmpegPath))
            {
                _sawmill.Warning("ffmpeg path is not configured. Disabling radio effect.");
                _radioEffectEnabled = false;
                return oggData;
            }

            if (string.IsNullOrWhiteSpace(ffmpegArgs))
            {
                _sawmill.Warning($"ffmpeg arguments for {effectName} are not configured. The effect will not be applied.");
                return oggData;
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = ffmpegArgs,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using var process = new Process { StartInfo = processStartInfo };

                if (!process.Start())
                {
                    _sawmill.Error($"Failed to start ffmpeg process for {effectName} effect.");
                    return oggData;
                }

                using var memoryStream = new MemoryStream();
                var outputTask = process.StandardOutput.BaseStream.CopyToAsync(memoryStream);
                var errorTask = process.StandardError.ReadToEndAsync();

                await process.StandardInput.BaseStream.WriteAsync(oggData, 0, oggData.Length);

                process.StandardInput.Close();

                await Task.WhenAll(outputTask, errorTask);
                await process.WaitForExitAsync();

                var errorOutput = await errorTask;
                if (process.ExitCode != 0)
                {
                    _sawmill.Error($"ffmpeg for {effectName} effect exited with code {process.ExitCode}. Stderr: {errorOutput}");
                    return oggData;
                }

                var processedData = memoryStream.ToArray();
                if (processedData.Length == 0)
                {
                    _sawmill.Warning($"ffmpeg for {effectName} effect produced an empty output. Stderr: {errorOutput}. Falling back to original audio.");
                    return oggData;
                }

                _sawmill.Debug($"Successfully applied {effectName} effect to TTS audio. Original size: {oggData.Length}, new size: {processedData.Length}");
                return processedData;
            }
            catch (Win32Exception)
            {
                _sawmill.Error($"ffmpeg not found at path '{_ffmpegPath}'. Disabling radio effect.");
                _radioEffectEnabled = false;
                return oggData;
            }
            catch (Exception e)
            {
                _sawmill.Error($"An exception occurred while running ffmpeg for {effectName} effect: {e}");
                return oggData;
            }
        }
    }
}
