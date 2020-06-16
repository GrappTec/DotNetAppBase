﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotNetAppBase.Std.Worker.Base;
using Microsoft.Extensions.Logging;

namespace DotNetAppBase.Std.Worker.Files
{
    public class FileSystemWatcherService : HostedServiceBase
    {
        private readonly string _watchPath;
        private readonly string _watchFilter;

        public FileSystemWatcherService(string watchPath, string watchFilter)
        {
            _watchPath = watchPath;
            _watchFilter = watchFilter;
        }

        public string WatchPath => _watchPath;

        public string WatchFilter => _watchFilter;

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Whatching {WatchPath}");

            using var watcher = new FileSystemWatcher
                {
                    Path = WatchPath,
                    Filter = WatchFilter,
                    IncludeSubdirectories = false
                };

            watcher.Created +=
                (sender, args) => Task.Run(() => InternalHandleCreated(args, cancellationToken), cancellationToken);

            watcher.Changed +=
                (sender, args) => Task.Run(() => InternalHandleChanged(args, cancellationToken), cancellationToken);

            watcher.Deleted +=
                (sender, args) => Task.Run(() => InternalHandleDeleted(args, cancellationToken), cancellationToken);

            watcher.Renamed +=
                (sender, args) => Task.Run(() => InternalHandleRenamed(args, cancellationToken), cancellationToken);

            watcher.Error +=
                (sender, args) => Task.Run(() => InternalHandleError(args, cancellationToken), cancellationToken);

            await InternalBeforeBeginWatch(cancellationToken);

            watcher.EnableRaisingEvents = true;

            await Task.Delay(-1, cancellationToken);
        }

        protected virtual async Task InternalBeforeBeginWatch(CancellationToken cancellationToken) => await Task.CompletedTask;

        protected virtual void InternalHandleCreated(FileSystemEventArgs args, CancellationToken cancellationToken) { }

        protected virtual void InternalHandleChanged(FileSystemEventArgs args, CancellationToken cancellationToken) { }

        protected virtual void InternalHandleDeleted(FileSystemEventArgs args, CancellationToken cancellationToken) { }

        protected virtual void InternalHandleRenamed(RenamedEventArgs args, CancellationToken cancellationToken) { }

        protected virtual void InternalHandleError(ErrorEventArgs args, CancellationToken cancellationToken) { }
    }
}