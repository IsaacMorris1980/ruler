using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ruler.Shared.Services
{
    /// <summary>
    /// A standalone service dedicated to high-performance file transfers.
    /// </summary>
    public class DownloadService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private bool _disposed = false;

        public DownloadService(HttpClient httpClient = null)
        {
            // Reuse an existing HttpClient if provided (best practice)
            _httpClient = httpClient ?? new HttpClient();

            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "Ruler-App-Downloader");
            }
        }

        /// <summary>
        /// Downloads a file with progress reporting and cancellation support.
        /// </summary>
        public async Task DownloadFileAsync(
            string url,
            string destinationPath,
            IProgress<double> progress = null,
            CancellationToken ct = default)
        {
            // Prepare the destination directory
            string directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Start the request with ResponseHeadersRead to save memory
            using (var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct))
            {
                response.EnsureSuccessStatusCode();

                long? totalBytes = response.Content.Headers.ContentLength;

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    var buffer = new byte[8192];
                    long totalRead = 0;
                    int read;

                    while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, read, ct);
                        totalRead += read;

                        // Report progress to the UI
                        if (totalBytes.HasValue)
                        {
                            progress?.Report((double)totalRead / totalBytes.Value);
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient?.Dispose();
                _disposed = true;
            }
        }
    }
}
