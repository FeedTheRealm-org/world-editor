using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using API;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.UI.Common;
using FTR.Core.Common.Config;
using FTRShared.Runtime.Models;
using FTRShared.UI.AuthMenu;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.UI.MenuBar.FileOption.PublishMenu
{
    [RequireComponent(typeof(UIDocument))]
    public class PublishMenuController : MenuController
    {
        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private Config config;

        [SerializeField]
        private WorldService worldService;

        [SerializeField]
        private SpriteService spriteService;

        [SerializeField]
        private ModelService modelService;

        [SerializeField]
        private AssetsService assetsService;

        [SerializeField]
        private ZoneService zoneService;

        [SerializeField]
        private SubscriptionService subscriptionService;

        [SerializeField]
        private Session.Session session;

        [Inject]
        private DataPersistenceManager dataPersistenceManager;

        [Inject]
        private WorldSelector worldSelector;

        [Inject]
        private WorldUIObjectProvider worldUIObjectProvider;

        [Inject]
        private CreatablesManager creatablesManager;

        private Button publishButton;
        private Button loginButton;
        private Button closeButton;
        private Label worldNameLabel;
        private Toggle publishCreatablesToggle;
        private Toggle publishWorldDataToggle;
        private VisualElement zoneGroup;

        private WorldData currentWorldData;
        private List<int> availableZones;
        private HashSet<int> selectedZones = new();
        private bool publishAllZones = true;
        private bool isAuthFlowActive;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            publishButton = root.Q<Button>("Publish");
            loginButton = root.Q<Button>("Login");
            closeButton = root.Q<Button>("Close");
            worldNameLabel = root.Q<Label>("WorldName");
            publishCreatablesToggle = root.Q<Toggle>("PublishCreatables");
            publishWorldDataToggle = root.Q<Toggle>("PublishWorldData");
            zoneGroup = root.Q<VisualElement>("ZoneGroup");

            currentWorldData = dataPersistenceManager.GetWorldData(worldSelector.selectedWorld);
            worldNameLabel.text = currentWorldData?.worldName ?? "No world loaded";

            bool isFirstPublish = string.IsNullOrEmpty(currentWorldData?.worldId);
            publishWorldDataToggle.value = true;
            publishWorldDataToggle.SetEnabled(!isFirstPublish);
            publishCreatablesToggle.value = true;

            PopulateZoneGroup();

            publishButton.clicked += OnPublishClicked;
            closeButton.clicked += CloseMenu;
            loginButton.clicked += OnLoginClicked;
        }

        void OnDisable()
        {
            publishButton.clicked -= OnPublishClicked;
            closeButton.clicked -= CloseMenu;
            loginButton.clicked -= OnLoginClicked;
        }

        private void PopulateZoneGroup()
        {
            availableZones = dataPersistenceManager.ListZones(worldSelector.selectedWorld);

            if (zoneGroup is ScrollView scrollView)
            {
                scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
                scrollView.verticalScrollerVisibility = ScrollerVisibility.Auto;
                scrollView.contentContainer.style.flexDirection = FlexDirection.Column;
                scrollView.contentContainer.style.width = Length.Percent(100);
            }

            zoneGroup.Clear();
            selectedZones.Clear();
            publishAllZones = true;

            var allButton = CreateZoneButton("All Zones", true);
            allButton.clicked += () => OnAllZonesClicked(allButton);
            zoneGroup.Add(allButton);

            foreach (var zoneId in availableZones)
            {
                var id = zoneId;
                var button = CreateZoneButton($"Zone {id}", false);
                button.clicked += () => OnZoneClicked(button, id, allButton);
                zoneGroup.Add(button);
            }
        }

        private Button CreateZoneButton(string text, bool selected)
        {
            var button = new Button { text = text };
            button.style.marginBottom = 2;
            button.style.marginLeft = 2;
            button.style.marginRight = 2;
            button.style.flexShrink = 0;
            button.style.alignSelf = Align.Stretch;
            button.style.width = Length.Percent(100);
            button.style.marginBottom = 4;
            button.style.backgroundColor = selected
                ? new StyleColor(new Color(0.2f, 0.6f, 0.2f))
                : new StyleColor(Color.black);
            button.style.color = new StyleColor(Color.white);
            button.style.borderTopLeftRadius = 6;
            button.style.borderTopRightRadius = 6;
            button.style.borderBottomLeftRadius = 6;
            button.style.borderBottomRightRadius = 6;
            return button;
        }

        private void SetButtonSelected(Button button, bool selected)
        {
            button.style.backgroundColor = selected
                ? new StyleColor(new Color(0.2f, 0.6f, 0.2f))
                : new StyleColor(Color.black);
        }

        private void OnAllZonesClicked(Button allButton)
        {
            publishAllZones = true;
            selectedZones.Clear();
            SetButtonSelected(allButton, true);
            foreach (var button in zoneGroup.Query<Button>().ToList())
                if (button != allButton)
                    SetButtonSelected(button, false);
        }

        private void OnZoneClicked(Button button, int zoneId, Button allButton)
        {
            publishAllZones = false;
            SetButtonSelected(allButton, false);

            if (selectedZones.Contains(zoneId))
            {
                selectedZones.Remove(zoneId);
                SetButtonSelected(button, false);
                if (selectedZones.Count == 0)
                    OnAllZonesClicked(allButton);
            }
            else
            {
                selectedZones.Add(zoneId);
                SetButtonSelected(button, true);
            }
        }

        private List<int> GetZonesToPublish() =>
            publishAllZones ? availableZones : new List<int>(selectedZones);

        private static bool IsAuthMenuOpen()
        {
            return GameObject.Find("LoginMenu") != null
                || GameObject.Find("SignUpMenu") != null
                || GameObject.Find("VerifyCodeMenu") != null;
        }

        private async void OnLoginClicked()
        {
            if (isAuthFlowActive || IsAuthMenuOpen())
            {
                return;
            }
            var menuBarGameObject = worldUIObjectProvider.menuBarGameObject;
            var loginMenuObject = worldUIObjectProvider.loginMenuObject;
            var signUpMenuObject = worldUIObjectProvider.signUpMenuObject;
            var verifyCodeMenuObject = worldUIObjectProvider.verifyCodeMenuObject;
            isAuthFlowActive = true;
            try
            {
                GameObject loginMenu = resolver.Instantiate(loginMenuObject);
                var loginObj = loginMenu;
                loginObj.name = "LoginMenu";
                var signUpObj = resolver.Instantiate(signUpMenuObject);
                signUpObj.name = "SignUpMenu";
                var verifyCodeObj = resolver.Instantiate(verifyCodeMenuObject);
                verifyCodeObj.name = "VerifyCodeMenu";
                var authFlowManager = new AuthFlowManager(loginObj, signUpObj, verifyCodeObj);
                authFlowManager.OnAuthComplete += () =>
                {
                    authFlowManager.Destroy();
                    isAuthFlowActive = false;
                };
                authFlowManager.Initialize();
            }
            catch
            {
                isAuthFlowActive = false;
                throw;
            }
        }

        private async void OnPublishClicked()
        {
            try
            {
                dataPersistenceManager.SaveWorldMetadata(currentWorldData);
                dataPersistenceManager.SaveZone(
                    currentWorldData.worldName,
                    worldSelector.selectedZoneId
                );
                dataPersistenceManager.SaveCreatables(currentWorldData.worldName);
                publishButton.SetEnabled(false);
                await ValidateBeforePublish();
                await PublishWorldData();
                await PublishCreatables();
                await PublishSprites();
                await PublishZoneData();
                ToastNotification.Show("World published successfully!", "success", Color.green);
                CloseMenu();
                worldSelector.selectedWorldId = currentWorldData.worldId;
            }
            catch (Exception ex)
            {
                logger.Log($"[PublishMenu] Error: {ex.Message}", this, Logging.LogType.Error);
                ToastNotification.Show($"Error publishing: {ex.Message}", "error", Color.red);
            }
            finally
            {
                publishButton.SetEnabled(true);
            }
        }

        // ---- Validation Functions ----

        private async Task ValidateBeforePublish()
        {
            var errors = new List<string>();

            ValidateSprites(errors);
            await ValidateModels(errors);
            await ValidateSubscription(errors);

            if (errors.Count > 0)
                throw new Exception($"{string.Join("\n", errors)}");
        }

        private async Task ValidateSubscription(List<string> errors)
        {
            if (subscriptionService == null)
            {
                logger.Log(
                    "[PublishMenu] Missing SubscriptionService.",
                    this,
                    Logging.LogType.Error
                );
                return;
            }

            var (data, error, statusCode) = await subscriptionService.GetSubscription();

            if (!string.IsNullOrEmpty(error))
            {
                if (statusCode == 404)
                {
                    errors.Add("No active subscription found. Please subscribe to publish zones.");
                }
                else
                {
                    errors.Add($"Failed to verify subscription: {error}");
                }
                return;
            }

            if (
                data == null
                || string.Equals(data.status, "canceled", StringComparison.OrdinalIgnoreCase)
            )
            {
                errors.Add("Subscription is canceled or inactive. Please renew to publish.");
                return;
            }

            int freeZonesCount = data.slots - data.used_slots;
            int newZonesToPublish = 0;

            List<int> publishedZones = new List<int>();
            if (!string.IsNullOrEmpty(currentWorldData?.worldId))
            {
                var (zones, zError, zStatusCode) = await zoneService.GetZonesList(
                    currentWorldData.worldId,
                    session.APIToken
                );
                if (string.IsNullOrEmpty(zError) && zones != null)
                {
                    publishedZones = zones;
                }
            }

            var zonesToPublish = GetZonesToPublish();
            foreach (var zoneId in zonesToPublish)
            {
                var zoneData = dataPersistenceManager.GetZoneData(
                    worldSelector.selectedWorld,
                    zoneId
                );
                if (zoneData == null)
                    continue;

                if (!publishedZones.Contains(zoneId))
                {
                    newZonesToPublish++;
                }
            }

            if (newZonesToPublish > freeZonesCount)
            {
                errors.Add(
                    $"You are trying to publish {newZonesToPublish} new zone(s), but only have {freeZonesCount} free zone(s) available."
                );
            }
        }

        private void ValidateSprites(List<string> errors)
        {
            var creatablesData = dataPersistenceManager.GetCreatables(worldSelector.selectedWorld);
            if (creatablesData == null)
                return;

            var allItems = new List<ItemData>();
            allItems.AddRange(creatablesData.weaponItems);
            allItems.AddRange(creatablesData.consumableItems);

            foreach (var item in allItems)
            {
                if (string.IsNullOrEmpty(item.spriteFilePath))
                    continue;
                string fullPath = Path.Combine(config.SpritesDirectory, item.spriteFilePath);
                if (!File.Exists(fullPath))
                    errors.Add($"Missing sprite for '{item.name}': {fullPath}");
            }
        }

        private async Task ValidateModels(List<string> errors)
        {
            if (string.IsNullOrEmpty(currentWorldData?.worldId))
                return; // first publish, no existing models to compare against

            foreach (var zoneId in GetZonesToPublish())
            {
                var zoneData = dataPersistenceManager.GetZoneData(
                    worldSelector.selectedWorld,
                    zoneId
                );
                if (zoneData == null || zoneData.objectPlacementData.Count == 0)
                    continue;

                var existingModels = await modelService.ListWorldModels(
                    currentWorldData.worldId,
                    session.APIToken
                );

                var newModels = zoneData
                    .objectPlacementData.GroupBy(s => s.id)
                    .Select(g => g.First())
                    .Where(s => !existingModels.ContainsKey(s.id))
                    .ToList();

                foreach (var model in newModels)
                {
                    var modelPath = dataPersistenceManager.GetModelFilepath(model.id);
                    if (string.IsNullOrEmpty(modelPath) || !File.Exists(modelPath))
                        errors.Add(
                            $"Missing model file for '{model.structureName}' (id: {model.id})"
                        );
                }
            }
        }

        // ---- Publish Functions ----

        private async Task PublishWorldData()
        {
            if (currentWorldData == null)
                throw new Exception("No world data to publish.");

            if (string.IsNullOrEmpty(currentWorldData.worldId))
            {
                await PublishWorldAsNew();
            }
            else if (publishWorldDataToggle.value)
            {
                var (updatedId, error, statusCode) = await worldService.UpdateWorld(
                    currentWorldData,
                    session.APIToken
                );

                if (!string.IsNullOrEmpty(error))
                {
                    if (statusCode == 404)
                    {
                        logger.Log(
                            "[PublishMenu] World ID not found on server, retrying publish as a new world.",
                            this,
                            Logging.LogType.Warning
                        );

                        await PublishWorldAsNew(
                            "World data published successfully after fallback!"
                        );
                        return;
                    }

                    throw new Exception(
                        $"Failed to update world data: {error} (status {statusCode})"
                    );
                }

                ToastNotification.Show("World data updated successfully!", "info", Color.aliceBlue);
            }
        }

        private async Task PublishWorldAsNew(
            string successMessage = "World data published successfully!"
        )
        {
            var (publishedId, error, statusCode) = await worldService.PublishWorld(
                currentWorldData,
                session.APIToken
            );
            if (string.IsNullOrEmpty(publishedId) || !string.IsNullOrEmpty(error))
                throw new Exception($"Failed to publish world data: {error} (status {statusCode})");

            currentWorldData.worldId = publishedId;
            currentWorldData.published_at = DateTime.Now;
            dataPersistenceManager.SaveWorldMetadata(currentWorldData);
            ToastNotification.Show(successMessage, "info", Color.aliceBlue);
        }

        private async Task PublishCreatables()
        {
            CreatablesData creatablesData = dataPersistenceManager.GetCreatables(
                worldSelector.selectedWorld
            );
            if (creatablesData == null)
            {
                logger.Log("No creatables data to publish.", this, Logging.LogType.Info);
                return;
            }
            await worldService.PublishCreatables(
                creatablesData,
                currentWorldData.worldId,
                session.APIToken
            );
        }

        private async Task PublishZoneData()
        {
            foreach (var zoneId in GetZonesToPublish())
            {
                var zoneData = dataPersistenceManager.GetZoneData(
                    worldSelector.selectedWorld,
                    zoneId
                );
                if (zoneData == null)
                    continue;

                await PublishModels(zoneData);

                var (_, error, statusCode) = await zoneService.PublishZone(
                    currentWorldData.worldId,
                    zoneData,
                    session.APIToken
                );
                if (!string.IsNullOrEmpty(error))
                {
                    logger.Log(
                        $"Failed to publish zone {zoneId}: {error})",
                        this,
                        Logging.LogType.Error
                    );
                    ToastNotification.Show($"Failed to publish zone {zoneId}.", error, Color.red);
                    continue;
                }
            }
        }

        private async Task PublishSprites()
        {
            var creatablesData = dataPersistenceManager.GetCreatables(worldSelector.selectedWorld);
            var spritesRequest = BuildSpritesRequest(creatablesData);

            AssertSpriteFilesExist(spritesRequest);
            AssertAssetsServiceAvailable();

            await spriteService.UploadSprites(
                spritesRequest,
                currentWorldData.worldId,
                session.APIToken
            );

            if (creatablesData?.cosmetics != null)
            {
                await PublishCosmetics(creatablesData);
            }
        }

        private SpritesRequest BuildSpritesRequest(CreatablesData creatablesData)
        {
            var request = new SpritesRequest();
            if (creatablesData == null)
                return request;

            var allItems = new List<ItemData>();
            allItems.AddRange(creatablesData.weaponItems);
            allItems.AddRange(creatablesData.consumableItems);

            foreach (var item in allItems)
            {
                if (!string.IsNullOrEmpty(item.spriteFilePath))
                {
                    request.ids.Add(item.id);
                    request.spritePath.Add(
                        Path.Combine(config.SpritesDirectory, item.spriteFilePath)
                    );
                }
            }

            return request;
        }

        private void AssertSpriteFilesExist(SpritesRequest request)
        {
            var missingSprites = request.spritePath.Where(path => !File.Exists(path)).ToList();

            if (missingSprites.Count > 0)
            {
                var missing = string.Join("\n", missingSprites);
                throw new Exception($"Missing sprite files:\n{missing}");
            }
        }

        private void AssertAssetsServiceAvailable()
        {
            if (assetsService == null)
            {
                logger.Log(
                    "PublishCosmetics: assetsService is unassigned in PublishMenuController.",
                    this,
                    Logging.LogType.Error
                );
                throw new Exception("AssetsService is missing.");
            }
        }

        private async Task PublishCosmetics(CreatablesData creatablesData)
        {
            try
            {
                var categoryIds = await FetchCategoryLookupAsync();
                var uploadCache = new Dictionary<string, string>();
                bool anyChanges = false;

                foreach (var cosmetic in creatablesData.cosmetics)
                {
                    cosmetic.OnAfterDeserialize();
                    bool cosmeticChanged = await ProcessCosmeticAsync(
                        cosmetic,
                        categoryIds,
                        uploadCache
                    );
                    anyChanges |= cosmeticChanged;
                }

                if (anyChanges)
                {
                    PersistCosmeticChanges(creatablesData);
                }
            }
            catch (Exception ex)
            {
                logger.Log(
                    $"PublishCosmetics Error: {ex.Message}\n{ex.StackTrace}",
                    this,
                    Logging.LogType.Error
                );
                throw;
            }
        }

        private async Task<Dictionary<string, string>> FetchCategoryLookupAsync()
        {
            var response = await assetsService.GetCategoriesAsync();

            if (response?.category_list == null)
            {
                logger.Log(
                    "PublishCosmetics: Failed to fetch categories from AssetsService.",
                    this,
                    Logging.LogType.Error
                );
                throw new Exception(
                    "Failed to fetch cosmetic categories from AssetsService. Cannot proceed with cosmetic publishing."
                );
            }

            return response
                .category_list.Where(c =>
                    !string.IsNullOrEmpty(c.category_name) && !string.IsNullOrEmpty(c.category_id)
                )
                .ToDictionary(c => c.category_name, c => c.category_id);
        }

        private async Task<bool> ProcessCosmeticAsync(
            CosmeticData cosmetic,
            Dictionary<string, string> categoryIds,
            Dictionary<string, string> uploadCache
        )
        {
            bool changed = false;

            foreach (var categoryName in cosmetic.categories.Keys.ToList())
            {
                if (categoryName == "EarringL")
                    continue;

                var entry = cosmetic.categories[categoryName];
                bool entryChanged = await ProcessCategoryEntryAsync(
                    cosmetic,
                    categoryName,
                    entry,
                    categoryIds,
                    uploadCache
                );
                changed |= entryChanged;
            }

            return changed;
        }

        private async Task<bool> ProcessCategoryEntryAsync(
            CosmeticData cosmetic,
            string categoryName,
            CosmeticCategoryEntry entry,
            Dictionary<string, string> categoryIds,
            Dictionary<string, string> uploadCache
        )
        {
            if (string.IsNullOrEmpty(entry.sprite_path))
                return false;

            string spritePath = entry.sprite_path;
            string fullPath = Path.Combine(config.SpritesDirectory, spritePath);
            if (!File.Exists(fullPath))
            {
                logger.Log(
                    $"PublishCosmetics: Sprite file not found for category '{categoryName}': {fullPath}",
                    this,
                    Logging.LogType.Error
                );
                throw new Exception(
                    $"Missing sprite file for cosmetic '{cosmetic.id}' category '{categoryName}': {fullPath}"
                );
            }

            if (
                !categoryIds.TryGetValue(categoryName, out string categoryId)
                || string.IsNullOrEmpty(categoryId)
            )
            {
                logger.Log(
                    $"PublishCosmetics: Category '{categoryName}' not found on server for cosmetic '{cosmetic.id}'.",
                    this,
                    Logging.LogType.Error
                );
                throw new Exception(
                    $"Category '{categoryName}' not found on server for cosmetic '{cosmetic.id}'. Cannot publish cosmetic."
                );
            }

            string existingSpriteId = ResolveExistingSpriteId(entry, uploadCache, fullPath);
            var resp = await UploadOrLinkSpriteAsync(
                categoryId,
                categoryName,
                fullPath,
                existingSpriteId,
                entry.price
            );

            if (resp == null || string.IsNullOrEmpty(resp.sprite_id))
            {
                logger.Log(
                    $"PublishCosmetics: Failed to upload/link sprite for cosmetic '{cosmetic.id}' category '{categoryName}'. Server returned null or empty sprite_id.",
                    this,
                    Logging.LogType.Error
                );
                throw new Exception(
                    $"Failed to upload/link sprite for cosmetic '{cosmetic.id}' category '{categoryName}'. AssetsService returned invalid response."
                );
            }

            if (!uploadCache.ContainsKey(fullPath))
                uploadCache[fullPath] = resp.sprite_id;

            entry.url_id = resp.sprite_id;

            string currentFileName = Path.GetFileNameWithoutExtension(spritePath);
            if (currentFileName != resp.sprite_id)
            {
                string ext = Path.GetExtension(spritePath);
                string newFileName = resp.sprite_id + ext;
                string newFullPath = Path.Combine(config.SpritesDirectory, newFileName);

                if (!File.Exists(newFullPath))
                {
                    File.Move(fullPath, newFullPath);
                    logger.Log($"Renamed sprite file '{spritePath}' → '{newFileName}'", this);
                }

                foreach (var otherEntry in cosmetic.categories.Values)
                {
                    if (otherEntry.sprite_path == spritePath)
                        otherEntry.sprite_path = newFileName;
                }

                if (!uploadCache.ContainsKey(newFullPath))
                    uploadCache[newFullPath] = resp.sprite_id;
                uploadCache.Remove(fullPath);
            }

            return true;
        }

        private static string ResolveExistingSpriteId(
            CosmeticCategoryEntry entry,
            Dictionary<string, string> uploadCache,
            string fullPath
        )
        {
            if (!string.IsNullOrEmpty(entry.url_id))
                return entry.url_id;

            return uploadCache.TryGetValue(fullPath, out var cachedId) ? cachedId : null;
        }

        private async Task<SpriteResponse> UploadOrLinkSpriteAsync(
            string categoryId,
            string categoryName,
            string fullPath,
            string existingSpriteId,
            float price
        )
        {
            if (!string.IsNullOrEmpty(existingSpriteId))
            {
                var (resp, statusCode) = await assetsService.LinkSpriteByIdAsync(
                    categoryId,
                    existingSpriteId,
                    currentWorldData.worldId,
                    price
                );

                if (resp != null)
                    return resp;

                if (statusCode != 404)
                {
                    logger.Log(
                        $"PublishCosmetics: LinkSpriteByIdAsync failed for category '{categoryName}' with status {statusCode}.",
                        this,
                        Logging.LogType.Error
                    );
                    throw new Exception(
                        $"AssetsService.LinkSpriteByIdAsync failed for category '{categoryName}' with HTTP status {statusCode}."
                    );
                }

                logger.Log(
                    $"PublishCosmetics: Sprite '{existingSpriteId}' not found on server (404), falling back to upload for category '{categoryName}'.",
                    this,
                    Logging.LogType.Warning
                );
            }

            var uploadResp = await assetsService.UploadSpriteAsync(
                categoryId,
                fullPath,
                currentWorldData.worldId,
                price
            );

            if (uploadResp == null || string.IsNullOrEmpty(uploadResp.sprite_id))
            {
                logger.Log(
                    $"PublishCosmetics: UploadSpriteAsync failed for category '{categoryName}'.",
                    this,
                    Logging.LogType.Error
                );
                throw new Exception(
                    $"AssetsService.UploadSpriteAsync failed for category '{categoryName}'. Server returned invalid response."
                );
            }

            return uploadResp;
        }

        private void PersistCosmeticChanges(CreatablesData creatablesData)
        {
            dataPersistenceManager.SaveCreatablesData(worldSelector.selectedWorld, creatablesData);
            SyncCosmeticsInMemory(creatablesData);
        }

        private void SyncCosmeticsInMemory(CreatablesData publishedData)
        {
            var inMemoryCosmetics = creatablesManager.GetAll<Cosmetic>();

            foreach (var publishedCosmetic in publishedData.cosmetics)
            {
                var inMemory = inMemoryCosmetics.FirstOrDefault(c => c.Id == publishedCosmetic.id);
                if (inMemory == null)
                    continue;

                inMemory.data.categories = new Dictionary<string, CosmeticCategoryEntry>(
                    publishedCosmetic.categories.ToDictionary(
                        kvp => kvp.Key,
                        kvp => new CosmeticCategoryEntry(
                            kvp.Value.sprite_path,
                            kvp.Value.url_id,
                            kvp.Value.price
                        )
                    )
                );

                inMemory.data.OnBeforeSerialize();
            }
        }

        private async Task PublishModels(ZoneData zoneData)
        {
            var existingModels = await modelService.ListWorldModels(
                currentWorldData.worldId,
                session.APIToken
            );

            var modelRequests = new List<ModelRequest>();
            var queuedIds = new HashSet<string>(existingModels.Keys);

            CollectModelRequests(
                zoneData.objectPlacementData.Select(s => s.id),
                queuedIds,
                modelRequests
            );

            CollectModelRequests(
                zoneData
                    .chestPlacements.SelectMany(c =>
                        new[] { c.closedChestModelData?.modelId, c.opendedChestModelData?.modelId }
                    )
                    .Where(id => !string.IsNullOrEmpty(id)),
                queuedIds,
                modelRequests
            );

            if (modelRequests.Count == 0)
            {
                logger.Log("[WorldPublisher] No new models to upload.", this, Logging.LogType.Info);
                return;
            }

            string error = await modelService.UploadModels(
                modelRequests,
                currentWorldData.worldId,
                session.APIToken
            );

            if (!string.IsNullOrEmpty(error))
                throw new Exception($"Failed to upload models: {error}");

            logger.Log(
                $"[WorldPublisher] Successfully uploaded {modelRequests.Count} new models for zone {zoneData.zoneId}.",
                this,
                Logging.LogType.Info
            );
        }

        private void CollectModelRequests(
            IEnumerable<string> modelIds,
            HashSet<string> queuedIds,
            List<ModelRequest> modelRequests
        )
        {
            foreach (var id in modelIds.Distinct())
            {
                if (queuedIds.Contains(id))
                    continue;

                var modelPath = dataPersistenceManager.GetModelFilepath(id);
                if (string.IsNullOrEmpty(modelPath))
                    throw new Exception(
                        $"Model file for '{id}' not found. Please ensure all model files are present before publishing."
                    );

                modelRequests.Add(new ModelRequest { id = id, filePath = modelPath });
                queuedIds.Add(id);
            }
        }
    }
}
