using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

namespace Watermelon
{
    public class UIGameGirlsView : UIPage, IPopupWindow//, IPausePopup
    {
        [SerializeField] private RectTransform safeAreaTransform;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private RectTransform panelRectTransform;
        [SerializeField] private Button closeButton;

        [SerializeField] private CanvasGroup uiContent;

        [Header("Static Layout")]
        [SerializeField] private Image headerBackground;
        [SerializeField] private Text titleText;
        [SerializeField] private Button normalTabButton;
        [SerializeField] private Button galleryTabButton;
        [SerializeField] private Button specialTabButton;
        [SerializeField] private Image galleryBadgeImage;
        [SerializeField] private Image specialBadgeImage;
        [SerializeField] private Image girlsFrameImage;
        [SerializeField] private RectTransform girlsContentRoot;
        [SerializeField] private Image filterBarImage;
        [SerializeField] private RectTransform galleryRoot;
        [SerializeField] private RectTransform rewardRoot;

        [Header("Sprites")]
        // [SerializeField] private Sprite panelSprite;
        // [SerializeField] private Sprite boardSprite;
        // [SerializeField] private Sprite tabSelectedSprite;
        // [SerializeField] private Sprite tabUnselectedSprite;
        // [SerializeField] private Sprite badgeSprite;
        // [SerializeField] private Sprite frameSprite;
        // [SerializeField] private Sprite cardSprite;
        // [SerializeField] private Sprite cardInnerSprite;
        // [SerializeField] private Sprite lockSprite;
        // [SerializeField] private Sprite filterBoxSprite;
        // [SerializeField] private Sprite filterResetSprite;
        // [SerializeField] private Sprite filterConfirmSprite;
        // [SerializeField] private Sprite sampleGirlSprite;

        private const string VIDEO_CACHE_FOLDER_NAME = ServerUtil.MENU_VIDEO_CACHE_FOLDER_NAME;//"MenuBackgroundVideoCache";
        private const string IMAGE_FILE_EXTENSION = ".png";
        private const int MAX_VISIBLE_ITEM_COUNT = 18;

        private float backgroundAlpha;
        private RectTransform girlsItemTemplate;
        private ScrollRect girlsScrollRect;
        private RectTransform girlsViewportRect;
        private GridLayoutGroup girlsGridLayout;
        private ContentSizeFitter girlsContentSizeFitter;
        private PoolGeneric<RectTransform> girlsItemPool;
        private string girlsItemPoolName;
        private Toggle filterToggle;
        private Text filterToggleText;
        private bool suppressFilterToggleCallback;
        private int girlsColumnCount = 3;
        private Vector2 girlsCellSize;
        private Vector2 girlsSpacing;
        private RectOffset girlsPadding;
        private int currentFirstVisibleRow = -1;

        private readonly Dictionary<string, Sprite> previewSpritesByPath = new Dictionary<string, Sprite>();
        private readonly List<Texture2D> previewTextures = new List<Texture2D>();
        private readonly List<GalleryItemData> currentGirlsItems = new List<GalleryItemData>();
        private readonly List<RectTransform> pooledGirlsItems = new List<RectTransform>();

        public bool IsOpened => canvas.enabled;

        public override void Init()
        {
            ResolveReferences();
            CacheGirlsTemplate();
            InitializeGirlsPool();

            backgroundAlpha = backgroundImage != null ? backgroundImage.color.a : 0.0f;

            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseButtonClicked);

            if (backgroundImage != null)
                backgroundImage.AddEvent(EventTriggerType.PointerDown, OnBackgroundClicked);


            BindFilterToggle();

            GameGirlsLikeController.OnLikeStateChanged += OnLikeStateChanged;
            GameGirlsLikeController.OnFilterModeChanged += OnFilterModeChanged;
        }

        private void OnDestroy()
        {
            GameGirlsLikeController.OnLikeStateChanged -= OnLikeStateChanged;
            GameGirlsLikeController.OnFilterModeChanged -= OnFilterModeChanged;

            if (girlsScrollRect != null)
                girlsScrollRect.onValueChanged.RemoveListener(OnGirlsScrollValueChanged);

            girlsItemPool?.Destroy();
            girlsItemPool = null;

            for (int i = 0; i < previewTextures.Count; i++)
            {
                if (previewTextures[i] != null)
                    Destroy(previewTextures[i]);
            }

            foreach (Sprite sprite in previewSpritesByPath.Values)
            {
                if (sprite != null)
                    Destroy(sprite);
            }

            previewTextures.Clear();
            previewSpritesByPath.Clear();
        }

        private void ResolveReferences()
        {
            if (backgroundImage == null)
                backgroundImage = transform.Find("ImageMask")?.GetComponent<Image>();

            if (panelRectTransform == null)
                panelRectTransform = transform.Find("UIContent") as RectTransform;

            if (closeButton == null)
                closeButton = FindDeepComponent<Button>("ButtonClose#Button");

            if (girlsContentRoot == null)
                girlsContentRoot = FindDeepTransform("GirlsContent#Transform") as RectTransform;

            if (galleryRoot == null)
                galleryRoot = FindDeepTransform("GirlsGalleryView#GameObject") as RectTransform;

            if (rewardRoot == null)
                rewardRoot = FindDeepTransform("GirlsRewardContent#Transform") as RectTransform;

            if (girlsScrollRect == null)
                girlsScrollRect = FindDeepComponent<ScrollRect>("GirlsView");

            if (girlsViewportRect == null && girlsScrollRect != null)
                girlsViewportRect = girlsScrollRect.viewport != null ? girlsScrollRect.viewport : girlsScrollRect.transform as RectTransform;

            if (girlsGridLayout == null && girlsContentRoot != null)
                girlsGridLayout = girlsContentRoot.GetComponent<GridLayoutGroup>();

            if (girlsContentSizeFitter == null && girlsContentRoot != null)
                girlsContentSizeFitter = girlsContentRoot.GetComponent<ContentSizeFitter>();

            if (filterToggle == null)
                filterToggle = FindDeepComponent<Toggle>("ToggleFilter#Toggle");

            if (filterToggleText == null && filterToggle != null)
                filterToggleText = FindChildRecursive(filterToggle.transform, "text")?.GetComponent<Text>();
        }

        public override void PlayShowAnimation()
        {
            if (backgroundImage != null)
                backgroundImage.SetAlpha(backgroundAlpha);

            if (panelRectTransform != null)
            {
                panelRectTransform.anchoredPosition = Vector2.zero;
                panelRectTransform.localScale = Vector3.one;
            }

            ApplyFilterToggleState();
            RefreshGirlsItems();

            UIController.OnPageOpened(this);

            uiContent.alpha = 0;
            uiContent.DOFade(1.0f, 0.3f);//, 0, true, UpdateMethod.Update);
        }

        public override void PlayHideAnimation()
        {
            UIController.OnPageClosed(this);
        }

        private void OnBackgroundClicked(BaseEventData eventData)
        {
            HideSelf();
        }

        private void OnCloseButtonClicked()
        {
            HideSelf();
        }

        private void HideSelf()
        {
            if (IsPageDisplayed)
                UIController.HidePage(this);
        }

        private void CacheGirlsTemplate()
        {
            if (girlsContentRoot == null || girlsContentRoot.childCount == 0)
                return;

            if (girlsItemTemplate == null)
                girlsItemTemplate = girlsContentRoot.GetChild(0) as RectTransform;
        }

        private void InitializeGirlsPool()
        {
            if (girlsItemTemplate == null || girlsContentRoot == null)
                return;

            if (girlsGridLayout != null)
            {
                girlsCellSize = girlsGridLayout.cellSize;
                girlsSpacing = girlsGridLayout.spacing;
                girlsPadding = girlsGridLayout.padding;
                girlsColumnCount = ResolveGirlsColumnCount();
                girlsGridLayout.enabled = false;
            }
            else
            {
                girlsCellSize = girlsItemTemplate.rect.size;
                girlsSpacing = Vector2.zero;
                girlsPadding = new RectOffset();
            }

            if (girlsContentSizeFitter != null)
                girlsContentSizeFitter.enabled = false;

            girlsItemPoolName ??= $"{name}_GirlsItemPool_{GetInstanceID()}";
            girlsItemPool ??= new PoolGeneric<RectTransform>(girlsItemTemplate.gameObject, girlsItemPoolName, girlsContentRoot);

            EnsurePooledGirlsItemCapacity(MAX_VISIBLE_ITEM_COUNT);

            girlsItemTemplate.gameObject.SetActive(false);

            if (girlsScrollRect != null)
            {
                girlsScrollRect.onValueChanged.RemoveListener(OnGirlsScrollValueChanged);
                girlsScrollRect.onValueChanged.AddListener(OnGirlsScrollValueChanged);
            }
        }

        private int ResolveGirlsColumnCount()
        {
            if (girlsGridLayout == null)
                return 1;

            switch (girlsGridLayout.constraint)
            {
                case GridLayoutGroup.Constraint.FixedColumnCount:
                    return Mathf.Max(1, girlsGridLayout.constraintCount);

                case GridLayoutGroup.Constraint.FixedRowCount:
                    return Mathf.Max(1, girlsGridLayout.constraintCount);

                default:
                    float availableWidth = 0.0f;
                    if (girlsViewportRect != null)
                    {
                        availableWidth = girlsViewportRect.rect.width - girlsPadding.left - girlsPadding.right;
                    }
                    else if (girlsContentRoot != null)
                    {
                        availableWidth = girlsContentRoot.rect.width - girlsPadding.left - girlsPadding.right;
                    }

                    float step = girlsCellSize.x + girlsSpacing.x;
                    if (availableWidth <= 0.0f || step <= 0.0f)
                        return 1;

                    return Mathf.Max(1, Mathf.FloorToInt((availableWidth + girlsSpacing.x) / step));
            }
        }

        private void RefreshGirlsItems()
        {
            if (girlsContentRoot == null || girlsItemTemplate == null)
                return;

            currentGirlsItems.Clear();
            currentGirlsItems.AddRange(BuildUnlockedGirlsItems());

            if (currentGirlsItems.Count == 0)
            {
                HideAllPooledGirlsItems();
                UpdateGirlsContentHeight(0);
                ResetGirlsScrollPosition();
                return;
            }

            UpdateGirlsContentHeight(currentGirlsItems.Count);
            currentFirstVisibleRow = -1;
            ResetGirlsScrollPosition();
            RefreshVisibleGirlsItems(true);
        }

        private void EnsurePooledGirlsItemCapacity(int requiredCount)
        {
            if (girlsItemPool == null)
                return;

            girlsItemPool.CreatePoolObjects(requiredCount);

            for (int i = pooledGirlsItems.Count; i < girlsItemPool.pooledObjects.Count; i++)
            {
                RectTransform pooledItem = girlsItemPool.pooledObjects[i];
                pooledItem.SetParent(girlsContentRoot, false);
                pooledItem.gameObject.SetActive(false);
                pooledGirlsItems.Add(pooledItem);
            }
        }

        private void HideAllPooledGirlsItems()
        {
            for (int i = 0; i < pooledGirlsItems.Count; i++)
            {
                if (pooledGirlsItems[i] != null)
                    pooledGirlsItems[i].gameObject.SetActive(false);
            }
        }

        private void UpdateGirlsContentHeight(int itemCount)
        {
            if (girlsContentRoot == null)
                return;

            int rowCount = Mathf.Max(1, Mathf.CeilToInt((float)itemCount / girlsColumnCount));
            float height = girlsPadding.top + girlsPadding.bottom + rowCount * girlsCellSize.y + Mathf.Max(0, rowCount - 1) * girlsSpacing.y;

            Vector2 sizeDelta = girlsContentRoot.sizeDelta;
            sizeDelta.y = height;
            girlsContentRoot.sizeDelta = sizeDelta;
        }

        private List<GalleryItemData> BuildUnlockedGirlsItems()
        {
            List<GalleryItemData> items = new List<GalleryItemData>();
            bool showLikedOnly = GameGirlsLikeController.ShowLikedOnly;

            LevelSave levelSave = SaveController.GetSaveObject<LevelSave>();
            int maxUnlockedLevelId = Mathf.Max(1, (levelSave != null ? levelSave.MaxReachedLevelIndex : 0) + 1);

            string cacheDirectory = Path.Combine(Application.persistentDataPath, VIDEO_CACHE_FOLDER_NAME);
            for (int levelId = 1; levelId <= maxUnlockedLevelId; levelId++)
            {
                string fileId = GetGalleryFileId(levelId);
                string imagePath = Path.Combine(cacheDirectory, fileId + IMAGE_FILE_EXTENSION);
                if (!File.Exists(imagePath))
                    continue;

                bool isLiked = GameGirlsLikeController.IsLiked(fileId);
                if (showLikedOnly && !isLiked)
                    continue;

                items.Add(new GalleryItemData
                {
                    LevelId = levelId,
                    FileId = fileId,
                    ImagePath = imagePath,
                    IsLiked = isLiked,
                });
            }

            return items;
        }

        private void ConfigureGirlsItem(RectTransform itemTransform, GalleryItemData itemData)
        {
            itemTransform.anchorMin = new Vector2(0.0f, 1.0f);
            itemTransform.anchorMax = new Vector2(0.0f, 1.0f);
            itemTransform.sizeDelta = girlsCellSize;
            itemTransform.localScale = Vector3.one;

            Image previewImage = FindChildRecursive(itemTransform, "ImageGirls")?.GetComponent<Image>();
            if (previewImage != null)
            {
                Sprite previewSprite = GetPreviewSprite(itemData.ImagePath);
                previewImage.sprite = previewSprite;
                // previewImage.preserveAspect = true;
                previewImage.enabled = previewSprite != null;
            }

            SetChildActive(itemTransform, "ImageGirlsJY", false);
            SetChildActive(itemTransform, "ImageGirlsLoad", false);
            SetChildActive(itemTransform, "ImageSpecial", false);
            SetChildActive(itemTransform, "ImageHeart", itemData.IsLiked);
            SetChildActive(itemTransform, "ImageDown", true);

            Button showButton = FindChildRecursive(itemTransform, "ButtonShow")?.GetComponent<Button>();
            if (showButton != null)
            {
                showButton.onClick.RemoveAllListeners();
                showButton.interactable = true;
                // showButton.onClick.AddListener(() => UIGameGirlsShowView.ShowForItem(itemData.LevelId, itemData.FileId));
            }
        }

        private void OnGirlsScrollValueChanged(Vector2 _)
        {
            RefreshVisibleGirlsItems();
        }

        private void RefreshVisibleGirlsItems(bool forceRefresh = false)
        {
            if (girlsContentRoot == null || girlsViewportRect == null)
                return;

            if (currentGirlsItems.Count == 0)
            {
                HideAllPooledGirlsItems();
                return;
            }

            float rowStep = girlsCellSize.y + girlsSpacing.y;
            if (rowStep <= 0.0f)
                rowStep = Mathf.Max(1.0f, girlsCellSize.y);

            float scrollY = Mathf.Max(0.0f, girlsContentRoot.anchoredPosition.y);
            int firstVisibleRow = Mathf.Max(0, Mathf.FloorToInt(scrollY / rowStep));
            if (!forceRefresh && firstVisibleRow == currentFirstVisibleRow)
                return;

            currentFirstVisibleRow = firstVisibleRow;

            int pooledRowCapacity = Mathf.Max(1, Mathf.CeilToInt((float)MAX_VISIBLE_ITEM_COUNT / girlsColumnCount));
            int viewportRowCount = Mathf.Max(1, Mathf.CeilToInt(girlsViewportRect.rect.height / rowStep) + 1);
            int visibleRowCount = Mathf.Min(pooledRowCapacity, viewportRowCount + 1);

            int firstVisibleIndex = firstVisibleRow * girlsColumnCount;
            if (firstVisibleIndex >= currentGirlsItems.Count)
            {
                firstVisibleIndex = 0;
                firstVisibleRow = 0;
                currentFirstVisibleRow = 0;
            }

            int remainingItemCount = Mathf.Max(0, currentGirlsItems.Count - firstVisibleIndex);
            int visibleItemCount = Mathf.Min(remainingItemCount, visibleRowCount * girlsColumnCount);

            EnsurePooledGirlsItemCapacity(Mathf.Max(visibleItemCount, MAX_VISIBLE_ITEM_COUNT));

            for (int i = 0; i < pooledGirlsItems.Count; i++)
            {
                RectTransform pooledItem = pooledGirlsItems[i];
                if (pooledItem == null)
                    continue;

                if (i < visibleItemCount)
                {
                    int dataIndex = firstVisibleIndex + i;
                    GalleryItemData itemData = currentGirlsItems[dataIndex];

                    pooledItem.gameObject.SetActive(true);
                    pooledItem.gameObject.name = $"Girls_{itemData.FileId}";

                    ConfigureGirlsItem(pooledItem, itemData);
                    PositionGirlsItem(pooledItem, dataIndex);
                    pooledItem.SetAsLastSibling();
                }
                else
                {
                    pooledItem.gameObject.SetActive(false);
                }
            }
        }

        private void PositionGirlsItem(RectTransform itemTransform, int dataIndex)
        {
            int row = dataIndex / girlsColumnCount;
            int column = dataIndex % girlsColumnCount;

            float x = girlsPadding.left + column * (girlsCellSize.x + girlsSpacing.x) + girlsCellSize.x * itemTransform.pivot.x;
            float y = -girlsPadding.top - row * (girlsCellSize.y + girlsSpacing.y) - girlsCellSize.y * (1.0f - itemTransform.pivot.y);

            itemTransform.anchoredPosition = new Vector2(x, y);
        }

        private void BindFilterToggle()
        {
            if (filterToggle == null)
                return;

            filterToggle.onValueChanged.RemoveListener(OnFilterToggleValueChanged);
            filterToggle.onValueChanged.AddListener(OnFilterToggleValueChanged);
        }

        private void ApplyFilterToggleState()
        {
            if (filterToggle == null)
                return;

            suppressFilterToggleCallback = true;
            filterToggle.SetIsOnWithoutNotify(GameGirlsLikeController.ShowLikedOnly);
            suppressFilterToggleCallback = false;

            UpdateFilterToggleText(GameGirlsLikeController.ShowLikedOnly);
        }

        private void OnFilterToggleValueChanged(bool isOn)
        {
            if (suppressFilterToggleCallback)
                return;

            GameGirlsLikeController.SetShowLikedOnly(isOn);
            UpdateFilterToggleText(isOn);
            RefreshGirlsItems();
        }

        private void UpdateFilterToggleText(bool showLikedOnly)
        {
            if (filterToggleText != null)
                filterToggleText.text = showLikedOnly ? "Like" : "All";
        }

        private void OnLikeStateChanged(string fileId, bool isLiked)
        {
            if (IsPageDisplayed)
                RefreshGirlsItems();
        }

        private void OnFilterModeChanged(bool showLikedOnly)
        {
            ApplyFilterToggleState();

            if (IsPageDisplayed)
                RefreshGirlsItems();
        }

        private void ResetGirlsScrollPosition()
        {
            if (girlsScrollRect == null)
                return;

            Canvas.ForceUpdateCanvases();
            if (girlsContentRoot != null)
            {
                Vector2 anchoredPosition = girlsContentRoot.anchoredPosition;
                anchoredPosition.y = 0.0f;
                girlsContentRoot.anchoredPosition = anchoredPosition;
            }

            currentFirstVisibleRow = -1;
            girlsScrollRect.verticalNormalizedPosition = 1.0f;
            RefreshVisibleGirlsItems(true);
        }

        private Sprite GetPreviewSprite(string imagePath)
        {
            if (previewSpritesByPath.TryGetValue(imagePath, out Sprite cachedSprite) && cachedSprite != null)
                return cachedSprite;

            byte[] imageBytes;
            try
            {
                imageBytes = File.ReadAllBytes(imagePath);
            }
            catch
            {
                return null;
            }

            if (imageBytes == null || imageBytes.Length == 0)
                return null;

            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(imageBytes))
            {
                Destroy(texture);
                return null;
            }

            texture.name = Path.GetFileNameWithoutExtension(imagePath);
            previewTextures.Add(texture);

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            sprite.name = texture.name;
            previewSpritesByPath.Add(imagePath, sprite);

            return sprite;
        }

        private string GetGalleryFileId(int levelId)
        {
            return levelId.ToString();//ServerUtil.GetVideoFileName(levelId);//$"100{Mathf.Max(1, levelId):00}";
        }

        private void SetChildActive(Transform parent, string childName, bool isActive)
        {
            Transform child = FindChildRecursive(parent, childName);
            if (child != null)
                child.gameObject.SetActive(isActive);
        }

        private T FindDeepComponent<T>(string objectName) where T : Component
        {
            Transform child = FindChildRecursive(transform, objectName);
            if (child == null)
                return null;

            return child.GetComponent<T>();
        }

        private Transform FindDeepTransform(string objectName)
        {
            return FindChildRecursive(transform, objectName);
        }

        private Transform FindChildRecursive(Transform parent, string objectName)
        {
            if (parent == null)
                return null;

            if (parent.name == objectName)
                return parent;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform result = FindChildRecursive(parent.GetChild(i), objectName);
                if (result != null)
                    return result;
            }

            return null;
        }

        private sealed class GalleryItemData
        {
            public int LevelId { get; set; }
            public string FileId { get; set; }
            public string ImagePath { get; set; }
            public bool IsLiked { get; set; }
        }
    }
}
