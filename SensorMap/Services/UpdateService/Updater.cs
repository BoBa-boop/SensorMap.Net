using Velopack;
using Velopack.Sources;

namespace SensorMap.Services.UpdateService
{
    internal class Updater
    {
        private const string RepoURL = "https://github.com/BoBa-boop/SensorMap.Net";
        private readonly UpdateManager _mgr = new UpdateManager(new GithubSource(RepoURL,accessToken:null,prerelease:false));

        private UpdateInfo? _info;

        public bool IsInstalled => _mgr.IsInstalled;
        public string? CurrentVersion => _mgr.CurrentVersion?.ToString();
        public string? NewVersion => _info?.TargetFullRelease.Version.ToString();

        public async Task<bool> CheckAsync()
        {
            if(!_mgr.IsInstalled) return false;
            _info = await _mgr.CheckForUpdatesAsync();
            return _info != null;
        }
        public async Task DownloadAsync(Action<int>? progress = null)
        {
            if (_info == null) return;
            await _mgr.DownloadUpdatesAsync(_info,progress);
        }

        public void ApplyAndRestart()
        {
            if(_info == null) return;
            _mgr.ApplyUpdatesAndRestart(_info);
        }
    }
}
