using ChatApi.server.Context;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace ChatApi.server.Services.Backgrounds
{
    public class OptimizeFiles : IHostedService, IDisposable
    {
        private Timer? _timer;

        private readonly IServiceScopeFactory _scopeFactory;

        public OptimizeFiles(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

        public Task StartAsync(CancellationToken cancellationToken)
        {

            _timer = new Timer(async _ => await DoWorkAsync(cancellationToken), null, TimeSpan.Zero, Constants.OPTIMIZE_FILES_AFTER);
            return Task.CompletedTask;
        }

        private async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DataBaseContext>();

                var uploadFolder = Constants.GetUploadsFolder();

                await CleanUnusedFilesAsync(db, uploadFolder, cancellationToken);
                await CleanUnusedAttachmentsAsync(db, uploadFolder, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OptimizeFiles] Error: {ex.Message}");
            }
        }


        private async Task CleanUnusedFilesAsync(DataBaseContext db, string uploadFolder, CancellationToken cancellationToken)
        {
            var validPaths = new HashSet<string>(await db.Attachments
                .Select(a => a.FilePath)
                .ToListAsync(cancellationToken));

            foreach (var fullPath in Directory.GetFiles(uploadFolder, "*.*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(uploadFolder, fullPath);
                if (!validPaths.Contains(relativePath))
                {
                    try
                    {
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                            Console.WriteLine($"[OptimizeFiles] Deleted file: {fullPath}");
                        }
                    }
                    catch
                    {
                        Console.WriteLine($"[OptimizeFiles] Failed to delete {fullPath}");
                    }
                }
            }
        }


        private async Task CleanUnusedAttachmentsAsync(DataBaseContext db, string uploadFolder, CancellationToken cancellationToken)
        {
            var unusedAttachments = await db.Attachments
                .Where(a => a.MessageId == null &&
                          !db.Profiles.Any(p => p.ImageId == a.Id) &&
                          !db.Servers.Any(s => s.ImageId == a.Id))
                .ToListAsync(cancellationToken);

            if (!unusedAttachments.Any())
                return;

            var removedPaths = new HashSet<string>(
                unusedAttachments.Select(a => a.FilePath),
                StringComparer.OrdinalIgnoreCase);

            db.Attachments.RemoveRange(unusedAttachments);
            await db.SaveChangesAsync(cancellationToken);

            var remainingPaths = new HashSet<string>(await db.Attachments
                .Select(a => a.FilePath)
                .ToListAsync(cancellationToken));

            foreach (var path in removedPaths)
            {
                if (!remainingPaths.Contains(path))
                {
                    var fullPath = Path.Combine(uploadFolder, path);
                    try
                    {
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                            Console.WriteLine($"[OptimizeFiles] Deleted attachment file: {fullPath}");
                        }
                    }
                    catch
                    {
                        Console.WriteLine($"[OptimizeFiles] Failed to delete {fullPath}");
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose() => _timer?.Dispose();
    }
}
