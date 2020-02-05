using SpackiBot.Logging;
using SpackiBot.Services.AssetService;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SpackiBot.Services.FFmpeg
{
    public class FFmpegService : IServiceStatus
    {
        private LoggingSection _loggingSection;

        private AssetService.AssetService _assetService;
        private AssetFolder _FFmpegFolder;
        private string _FFmpegFile;

        private ServiceStatus _serviceStatus;
        ServiceStatus IServiceStatus.ServiceStatus { get => _serviceStatus; }

        public FFmpegService(AssetService.AssetService assetService)
        {
            _assetService = assetService;

            _serviceStatus = ServiceStatus.Starting;
            _loggingSection = new LoggingSection("FFmpeg-Service");

            try
            {
                _FFmpegFolder = assetService.FindSub(new string[] { "Externals", "FFmpeg" }, StringComparison.Ordinal);
            }
            catch (DirectoryNotFoundException)
            {
                _serviceStatus = ServiceStatus.Failed;
                _loggingSection.Critical($"Could not find Externals folder for FFmpeg binaries.");
                return;
            }

            _FFmpegFile = _FFmpegFolder.FilterFiles("ffmpeg", StringComparison.OrdinalIgnoreCase).FirstOrDefault();
            if (_FFmpegFile == null)
            {
                _serviceStatus = ServiceStatus.Failed;
                _loggingSection.Critical($"Could not find FFmpeg binaries in {_FFmpegFolder.Information.FullName}");
            }
            else
                _serviceStatus = ServiceStatus.Enabled;
        }

        public Process ReadAudio(string path) => Process.Start(new ProcessStartInfo
        {
            FileName = _FFmpegFile,
            Arguments = $"-hide_banner -loglevel panic -ac 2 -f s16le -ar 48000 pipe:1 -i \"{path}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
        });
    }
}