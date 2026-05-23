using Ruler.Contracts.Services.Updates;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Services.Updates
{
    public class HttpDownloadService : IHttpDownloadService, IDisposable
    {
        private WebClient _webClient;
        private bool _isDisposed;

        // --- EVENTS ---
        public event Action<int> ProgressChanged;
        public event Action DownloadCompleted;

        public HttpDownloadService()
        {
            // Forces modern, secure TLS 1.2/1.3 cryptographic protocols for .NET Framework 4.8
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | (SecurityProtocolType)3072;
        }

        /// <summary>
        /// Downloads an asset from a web server asynchronously while broadcasting progress metrics.
        /// </summary>
        public async Task DownloadFileAsync(string fileUrl, string destinationPath)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                throw new ArgumentNullException(nameof(fileUrl));
            if (string.IsNullOrWhiteSpace(destinationPath))
                throw new ArgumentNullException(nameof(destinationPath));

            // Ensure the target directory exists before starting the stream
            string directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Clean up any stale instances before creating a new request thread
            ResetWebClient();

            _webClient = new WebClient();
            _webClient.Headers.Add("User-Agent", "RulerApp-Download-Engine");

            // Wire up internal native event pipeline mappings
            _webClient.DownloadProgressChanged += OnDownloadProgressChanged;
            _webClient.DownloadFileCompleted += OnDownloadFileCompleted;

            try
            {
                var uri = new Uri(fileUrl);
                await _webClient.DownloadFileTaskAsync(uri, destinationPath);
            }
            catch (Exception)
            {
                // If a partial download fails mid-stream, wipe out the corrupted file artifact
                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }
                throw;
            }
        }

        // --- INTERNAL EVENT BRIDGE PIPELINES ---

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // Bubble the raw percentage value straight out to listening services or view models
            ProgressChanged?.Invoke(e.ProgressPercentage);
        }

        private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // Let the Task exception handling pass the fault naturally to the consumer
                return;
            }

            if (e.Cancelled)
            {
                return;
            }

            DownloadCompleted?.Invoke();
        }

        private void ResetWebClient()
        {
            if (_webClient != null)
            {
                _webClient.DownloadProgressChanged -= OnDownloadProgressChanged;
                _webClient.DownloadFileCompleted -= OnDownloadFileCompleted;
                _webClient.Dispose();
                _webClient = null;
            }
        }

        // --- DISPOSAL MANIFEST ENGINE ---

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    ResetWebClient();
                }
                _isDisposed = true;
            }
        }
    }
}
