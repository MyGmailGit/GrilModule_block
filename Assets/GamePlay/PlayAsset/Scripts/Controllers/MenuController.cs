using UnityEngine;
using UnityEngine.SceneManagement;
using VideoSystem;
using Watermelon.SkinStore;

namespace Watermelon
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField] UIController uiController;

        private static ParticlesController particlesController;
        private static FloatingTextController floatingTextController;
        private static SkinController skinController;
        private static SkinStoreController skinStoreController;
        private static PUController puController;

        private void Awake()
        {
            GameData gameData = GameData.Data;
            if (gameData == null)
                Debug.LogError("GameData is null. Please add the Game Settings component to the Project Init Settings and link Game Data scriptable object.");

            // Cache components
            gameObject.CacheComponent(out particlesController);
            gameObject.CacheComponent(out floatingTextController);
            gameObject.CacheComponent(out skinController);
            // gameObject.CacheComponent(out skinStoreController);
            gameObject.CacheComponent(out puController);

            // Initialize UI Controller to let other classes use UIController.GetPage method
            uiController.Init();

            // Initialize other controllers
            particlesController.Init();
            floatingTextController.Init();

            puController.Init();

            skinController.Init();
            // skinStoreController.Init(skinController);

            // Initialize currency cloud and pages
            uiController.InitPages();

            AnalyticsController.OnLoadingEnd();
        }

        private void Start()
        {
            // Display default page
            UIController.ShowPage<UIMainMenu>();

            VideoSerilNumberManager.Instance.surpriseData.ResetCurrentPlaying();
            VideoSerilNumberManager.Instance.specialData.ResetCurrentPlaying();
        }

        public static void OnMapLevelClicked(int levelID)
        {
            LoadGame(levelID);

            // if (LivesSystem.Lives > 0 || LivesSystem.InfiniteMode)
            // {
            //     LivesSystem.LockLife();
            //     LoadGame(levelID);
            // }
            // else
            // {
            //     UIAddLivesPanel.Show((lifeRecieved) =>
            //     {
            //         if (lifeRecieved)
            //         {
            //             LivesSystem.LockLife();
            //             LoadGame(levelID);
            //         }
            //     });
            // }
        }

        public static void LoadGame(int levelID)
        {
            ActiveSession session = ActiveSession.Current;
            session.SetLevelIndex(levelID);

            Overlay.Show(0.3f, () =>
            {
                Unload(() =>
                {
                    SceneManager.LoadScene(GameConsts.SCENE_GAME);
                });
            }, true);
        }

        public static void LoadSpecialGame(int levelID)
        {
            ActiveSession session = ActiveSession.Current;
            session.SetSpecialLevelIndex(levelID);

            Overlay.Show(0.3f, () =>
            {
                // Unload(() =>
                // {z
                SceneManager.LoadScene(GameConsts.SCENE_GAME);
                // });
            }, true);
        }

        public static void Unload(SimpleCallback onUnloaded)
        {
            // Do menu unload

            // AdsManager.ShowInterstitial((result) =>
            // {
            //     onUnloaded?.Invoke();
            // }, "EnterLevel");

            onUnloaded?.Invoke();
        }

        public static void OnPlayButtonClicked()
        {
            LevelSave levelSave = SaveController.GetSaveObject<LevelSave>();

            OnMapLevelClicked(levelSave.MaxReachedLevelIndex);
        }
    }
}