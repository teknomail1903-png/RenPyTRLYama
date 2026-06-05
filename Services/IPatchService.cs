using System.Threading;
using System.Threading.Tasks;
using RenPyTRLauncher.Models;

namespace RenPyTRLauncher.Services
{
    public interface IPatchService
    {
        Task<PatchInstallResult> InstallPatchAsync(Game game, string gameRootFolder, User user, CancellationToken cancellationToken = default);
        Task<string> ResolveDownloadUrlAsync(Game game, CancellationToken cancellationToken = default);
    }
}
