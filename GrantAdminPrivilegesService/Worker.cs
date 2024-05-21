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

        private List<string> ReadUsernamesFromFile(string filePath)
        {
            List<string> usernames = new List<string>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    usernames.Add(line);
                }
            }

            return usernames;
        }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "users.txt");

        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        // Read the txt file to get a list of usernames
        List<string> usernames = ReadUsernamesFromFile(filePath);
        // Loop through each username and call the GrantAdminPrivileges method
        foreach (string username in usernames)
        {
            try
            {
                GrantAdminPrivileges(username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to grant admin privileges");
            }
        }
        await Task.Delay(1000, stoppingToken); // Delay to prevent continuous execution
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
