using HandyControl.Controls;
using Velopack;
using Velopack.Sources;

namespace SensorMap.Services.UpdateService
{
    internal class Updater
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private const string RepoURL = "https://github.com/BoBa-boop/SensorMap.Net";
        private readonly UpdateManager _mgr = new UpdateManager(new GithubSource(RepoURL,accessToken:null,prerelease:false));

        private UpdateInfo? _info;

        public bool IsInstalled => _mgr.IsInstalled;
        public string? CurrentVersion => _mgr.CurrentVersion?.ToString();
        public string? NewVersion => _info?.TargetFullRelease.Version.ToString();

        public async Task<bool> CheckAsync()
        {
            try
            {
                if (!_mgr.IsInstalled) return false;
                _info = await _mgr.CheckForUpdatesAsync();
                return _info != null;
            }
            catch(Exception ex)
            {
                Logger.Error(ex.Message);
                Growl.Error("Возникла ошибка при проверки наличия обновлений");
                return false;
            }
        }
        public async Task DownloadAsync(Action<int>? progress = null)
        {
            try
            {
                if (_info == null) return;
                await _mgr.DownloadUpdatesAsync(_info, progress);
            }
            catch(Exception ex)
            {
                Growl.Error("Возникла ошибка при загрузки обновлений");
                Logger.Error(ex.Message);
                return;
            }
        }

        public void ApplyAndRestart()
        {
            try
            {
                if (_info == null) return;
                _mgr.ApplyUpdatesAndRestart(_info);
            }
            catch(Exception ex)
            {
                Growl.Error("Возникла ошибка при установки обновлений");
                Logger.Error(ex.Message);
            }
        }
    }
}
