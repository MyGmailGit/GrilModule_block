using System;

namespace Watermelon
{
    [StaticUnload]
    public static class GameGirlsHomeVideoController
    {
        private const string SAVE_ID = "Game Girls Home Video";

        private static GameGirlsHomeVideoSave save;

        public static event Action<string> OnHomeVideoChanged;

        public static bool HasCustomSelection
        {
            get
            {
                GameGirlsHomeVideoSave homeVideoSave = GetSave();
                return homeVideoSave != null && homeVideoSave.HasCustomSelection && !string.IsNullOrEmpty(homeVideoSave.SelectedLevelId);
            }
        }

        public static string SelectedLevelId
        {
            get
            {
                GameGirlsHomeVideoSave homeVideoSave = GetSave();
                if (homeVideoSave == null || !homeVideoSave.HasCustomSelection)
                    return null;

                return string.IsNullOrEmpty(homeVideoSave.SelectedLevelId) ? null : homeVideoSave.SelectedLevelId;
            }
        }

        public static bool IsSelected(string fileId)
        {
            if (string.IsNullOrEmpty(fileId))
                return false;

            return HasCustomSelection && string.Equals(SelectedLevelId, fileId, StringComparison.Ordinal);
        }

        public static bool SetSelectedFileId(string fileId)
        {
            if (string.IsNullOrEmpty(fileId))
                return false;

            GameGirlsHomeVideoSave homeVideoSave = GetSave();
            if (homeVideoSave == null)
                return false;

            if (homeVideoSave.HasCustomSelection && string.Equals(homeVideoSave.SelectedLevelId, fileId, StringComparison.Ordinal))
                return false;

            homeVideoSave.HasCustomSelection = true;
            homeVideoSave.SelectedLevelId = fileId;

            SaveController.MarkAsSaveIsRequired();
            OnHomeVideoChanged?.Invoke(fileId);
            return true;
        }

        private static GameGirlsHomeVideoSave GetSave()
        {
            if (!SaveController.IsSaveLoaded)
                return null;

            if (save == null)
                save = SaveController.GetSaveObject<GameGirlsHomeVideoSave>(SAVE_ID);

            return save;
        }

        private static void UnloadStatic()
        {
            save = null;
            OnHomeVideoChanged = null;
        }
    }
}
