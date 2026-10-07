using System;
using System.Collections.Generic;

namespace Watermelon
{
    [StaticUnload]
    public static class GameGirlsDownloadController
    {
        private const string SAVE_ID = "Game Girls Downloaded";

        private static GameGirlsDownloadSave save;

        public static event Action<string, bool> OnDownloadStateChanged;

        public static bool IsDownloaded(string fileId)
        {
            if (string.IsNullOrEmpty(fileId))
                return false;

            GameGirlsDownloadSave downloadSave = GetSave();
            return downloadSave != null && downloadSave.DownloadedFileIds.Contains(fileId);
        }

        public static bool ShouldShowDownloadHint(string fileId)
        {
            return !IsDownloaded(fileId);
        }

        public static void MarkAsDownloaded(string fileId)
        {
            if (string.IsNullOrEmpty(fileId))
                return;

            GameGirlsDownloadSave downloadSave = GetSave();
            if (downloadSave == null)
                return;

            List<string> downloadedFileIds = downloadSave.DownloadedFileIds;
            if (downloadedFileIds.Contains(fileId))
                return;

            downloadedFileIds.Add(fileId);
            SaveController.MarkAsSaveIsRequired();
            OnDownloadStateChanged?.Invoke(fileId, true);
        }

        private static GameGirlsDownloadSave GetSave()
        {
            if (!SaveController.IsSaveLoaded)
                return null;

            if (save == null)
                save = SaveController.GetSaveObject<GameGirlsDownloadSave>(SAVE_ID);

            return save;
        }

        private static void UnloadStatic()
        {
            save = null;
            OnDownloadStateChanged = null;
        }
    }
}
