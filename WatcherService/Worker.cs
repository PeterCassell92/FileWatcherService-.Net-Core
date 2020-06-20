using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Security.Permissions;

namespace WatcherService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private FileSystemWatcher _folderWatcher;
        private readonly string _inputFolder;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            _inputFolder = @"C:\Users\Peter\Documents\watchfolder";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.CompletedTask;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Service Starting");
            if (!Directory.Exists(_inputFolder))
            {
                _logger.LogWarning($"Please make sure the InputFolder [{_inputFolder}] exists, then restart the service.");
                return Task.CompletedTask;
            }

            _logger.LogInformation($"Binding Events from Input Folder: {_inputFolder}");
            _folderWatcher = new FileSystemWatcher(_inputFolder, "*.TXT")
            {
                NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName |
                                  NotifyFilters.DirectoryName
            };
            _folderWatcher.Created += Input_OnChanged;
            _folderWatcher.Changed += Input_OnChanged;
            _folderWatcher.Deleted += Input_OnChanged;
            _folderWatcher.Renamed += Input_OnChanged;
            _folderWatcher.EnableRaisingEvents = true;

            return base.StartAsync(cancellationToken);
        }

        protected void Input_OnChanged(object source, FileSystemEventArgs e)
        {
            _logger.LogInformation("Change type " + e.ChangeType.ToString());

            switch (e.ChangeType)
            {
                case( WatcherChangeTypes.Created):
                    _logger.LogInformation($"File Created at [{e.FullPath}]");
                    break;

                case (WatcherChangeTypes.Deleted):
                    _logger.LogInformation($"File Deleted from [{e.FullPath}]");
                    break;

                case (WatcherChangeTypes.Changed):
                    _logger.LogInformation($"File Changed at [{e.FullPath}]" );
                    break;

                case (WatcherChangeTypes.Renamed):
                    _logger.LogInformation($"File Renamed [{e.FullPath}]");
                    break;

                default:
                    break;
            }              
         _logger.LogInformation("Done with Inbound Change Event");
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Service");
            _folderWatcher.EnableRaisingEvents = false;
            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _logger.LogInformation("Disposing Service");
            _folderWatcher.Dispose();
            base.Dispose();
        }
    }
}
