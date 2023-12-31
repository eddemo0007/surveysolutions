using System.Threading;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IBackupRestoreService
    {
        Task<string> BackupAsync();
        Task<string> BackupAsync(string backupToFolderPath);
        Task RestoreAsync(string backupFilePath);
        RestorePackageInfo GetRestorePackageInfo(string restoreFolder);
        Task SendBackupAsync(string filePath, CancellationToken token);
        Task SendLogsAsync(CancellationToken token);
    }
}
