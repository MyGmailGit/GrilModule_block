using System;
using System.Collections.Generic;

namespace Watermelon
{
    [StaticUnload]
    public static class GameGirlsLikeController
    {
        private const string SAVE_ID = "Game Girls Likes";

        private static GameGirlsLikeSave save;

        public static event Action<string, bool> OnLikeStateChanged;
        public static event Action<bool> OnFilterModeChanged;

        public static bool IsLiked(string fileId)
        {
            if (string.IsNullOrEmpty(fileId))
                return false;

            GameGirlsLikeSave likeSave = GetSave();
            return likeSave != null && likeSave.LikedFileIds.Contains(fileId);
        }

        public static void SetLiked(string fileId, bool isLiked)
        {
            if (string.IsNullOrEmpty(fileId))
                return;

            GameGirlsLikeSave likeSave = GetSave();
            if (likeSave == null)
                return;

            List<string> likedFileIds = likeSave.LikedFileIds;
            bool wasLiked = likedFileIds.Contains(fileId);
            if (wasLiked == isLiked)
                return;

            if (isLiked)
            {
                likedFileIds.Add(fileId);
            }
            else
            {
                likedFileIds.Remove(fileId);
            }

            SaveController.MarkAsSaveIsRequired();
            OnLikeStateChanged?.Invoke(fileId, isLiked);
        }

        public static bool ShowLikedOnly
        {
            get
            {
                GameGirlsLikeSave likeSave = GetSave();
                return likeSave != null && likeSave.ShowLikedOnly;
            }
        }

        public static void SetShowLikedOnly(bool showLikedOnly)
        {
            GameGirlsLikeSave likeSave = GetSave();
            if (likeSave == null || likeSave.ShowLikedOnly == showLikedOnly)
                return;

            likeSave.ShowLikedOnly = showLikedOnly;
            SaveController.MarkAsSaveIsRequired();
            OnFilterModeChanged?.Invoke(showLikedOnly);
        }

        private static GameGirlsLikeSave GetSave()
        {
            if (!SaveController.IsSaveLoaded)
                return null;

            if (save == null)
                save = SaveController.GetSaveObject<GameGirlsLikeSave>(SAVE_ID);

            return save;
        }

        private static void UnloadStatic()
        {
            save = null;

            OnLikeStateChanged = null;
            OnFilterModeChanged = null;
        }
    }
}
