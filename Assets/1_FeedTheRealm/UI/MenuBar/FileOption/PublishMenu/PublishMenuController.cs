using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using API;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.Gameplay.Creatables;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.UI.Common;
using FTR.Core.Common.Config;
using FTRShared.Runtime.Models;
using FTRShared.UI.AuthMenu;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.UI.MenuBar.FileOption.PublishMenu
{
    [RequireComponent(typeof(UnityEngine.UIElements.UIDocument))]
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
        private ZoneService zoneService;

        [SerializeField]
        private SubscriptionService subscriptionService;

        [SerializeField]
        private AssetsService assetsService;

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

        private PublishMenuView view;

        private WorldData currentWorldData;
        private List<int> availableZones;
        private HashSet<int> selectedZones = new();
        private bool publishAllZones = true;
        private bool isAuthFlowActive;

        void OnEnable()
        {
            var root = GetComponent<UnityEngine.UIElements.UIDocument>().rootVisualElement;
            view = new PublishMenuView();
            view.Initialize(root);

            currentWorldData = dataPersistenceManager.GetWorldData(worldSelector.selectedWorld);
            bool isFirstPublish = string.IsNullOrEmpty(currentWorldData?.worldId);

            view.SetWorldName(currentWorldData?.worldName);
            view.SetPublishWorldDataToggle(true, !isFirstPublish);
            view.SetPublishCreatablesToggle(true);

            view.OnPublishClicked += OnPublishClicked;
            view.OnCloseClicked += CloseMenu;
            view.OnLoginClicked += OnLoginClicked;
            view.OnAllZonesClicked += OnAllZonesClicked;
            view.OnZoneClicked += OnZoneClicked;

            PopulateZoneGroup();
        }

        void OnDisable()
        {
            view.OnPublishClicked -= OnPublishClicked;
            view.OnCloseClicked -= CloseMenu;
            view.OnLoginClicked -= OnLoginClicked;
            view.OnAllZonesClicked -= OnAllZonesClicked;
            view.OnZoneClicked -= OnZoneClicked;
        }

        private void PopulateZoneGroup()
        {
            availableZones = dataPersistenceManager.ListZones(worldSelector.selectedWorld);

            view.ConfigureZoneScrollView();
            view.ClearZones();
            view.AddAllZonesButton();

            foreach (var zoneId in availableZones)
                view.AddZoneButton(zoneId);

            view.SetAllZonesSelected(true);
            selectedZones.Clear();
            publishAllZones = true;
        }

        private void OnAllZonesClicked()
        {
            publishAllZones = true;
            selectedZones.Clear();
            view.SetAllZonesSelected(true);
            view.DeselectAllZoneButtons();
        }

        private void OnZoneClicked(int zoneId)
        {
            publishAllZones = false;
            view.SetAllZonesSelected(false);

            if (selectedZones.Contains(zoneId))
            {
                selectedZones.Remove(zoneId);
                view.SetZoneSelected(zoneId, false);

                if (selectedZones.Count == 0)
                    OnAllZonesClicked();
            }
            else
            {
                selectedZones.Add(zoneId);
                view.SetZoneSelected(zoneId, true);
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
                return;

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
                SavePrePublishState();
                view.SetPublishButtonEnabled(false);

                await ValidateBeforePublish();
                await PublishWorldData();
                await PublishCreatables();
                await PublishSprites();
                await PublishZoneData();

                ToastNotification.Show("World published successfully!", "success", Color.green);
                CloseMenu();
            }
            catch (Exception ex)
            {
                logger.Log($"[PublishMenu] Error: {ex.Message}", this, Logging.LogType.Error);
                ToastNotification.Show($"Error publishing: {ex.Message}", "error", Color.red);
            }
            finally
            {
                view.SetPublishButtonEnabled(true);
            }
        }

        private void SavePrePublishState()
        {
            dataPersistenceManager.SaveWorldMetadata(currentWorldData);
            dataPersistenceManager.SaveZone(
                currentWorldData.worldName,
                worldSelector.selectedZoneId
            );
            dataPersistenceManager.SaveCreatables(currentWorldData.worldName);
        }

        private async Task ValidateBeforePublish()
        {
            var errors = new List<string>();

            ValidateSprites(errors);
            await ValidateModels(errors);

            if (errors.Count > 0)
                throw new Exception($"{string.Join("\n", errors)}");
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
                return;

            foreach (var zoneId in GetZonesToPublish())
            {
                var zoneData = dataPersistenceManager.GetZoneData(
                    worldSelector.selectedWorld,
                    zoneId
                );
                if (zoneData == null || zoneData.objectPlacementData.Count == 0)
                    continue;

                var newModels = await GetNewModelsForZoneAsync(zoneData);

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

        private async Task PublishWorldData()
        {
            if (currentWorldData == null)
                throw new Exception("No world data to publish.");

            if (string.IsNullOrEmpty(currentWorldData.worldId))
            {
                await PublishWorldAsNew();
            }
            else if (view.PublishWorldData)
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

            logger.Log(
                $"[PublishMenu] Publishing creatables -> weapons: {creatablesData.weaponItems?.Count ?? 0}, consumables: {creatablesData.consumableItems?.Count ?? 0}, cosmetics: {creatablesData.cosmetics?.Count ?? 0}",
                this,
                Logging.LogType.Info
            );

            var (error, statusCode) = await worldService.PublishCreatables(
                creatablesData,
                currentWorldData.worldId,
                session.APIToken
            );

            if (!string.IsNullOrEmpty(error))
            {
                throw new Exception($"Failed to publish creatables: {error} (status {statusCode})");
            }

            logger.Log(
                $"[PublishMenu] Creatables published successfully (status {statusCode}).",
                this,
                Logging.LogType.Info
            );
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
            int price
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

        private async Task PublishModels(ZoneData zoneData)
        {
            if (zoneData.objectPlacementData.Count == 0)
            {
                logger.Log(
                    "[WorldPublisher] No structures in zone, skipping model upload.",
                    this,
                    Logging.LogType.Info
                );
                return;
            }

            var newModels = await GetNewModelsForZoneAsync(zoneData);
            if (newModels.Count == 0)
                return;

            var modelRequests = BuildModelRequests(newModels);

            string error = await modelService.UploadModels(
                modelRequests,
                currentWorldData.worldId,
                session.APIToken
            );

            logger.Log(
                $"[WorldPublisher] Successfully uploaded {newModels.Count} new models. for zone {zoneData.zoneId}",
                this,
                Logging.LogType.Info
            );

            if (!string.IsNullOrEmpty(error))
                throw new Exception($"Failed to upload models: {error}");
        }

        private List<ModelRequest> BuildModelRequests(List<StructureData> newModels)
        {
            var modelRequests = new List<ModelRequest>();

            foreach (var model in newModels)
            {
                var modelPath = dataPersistenceManager.GetModelFilepath(model.id);
                if (string.IsNullOrEmpty(modelPath))
                {
                    throw new Exception(
                        $"Model file for '{model.id}' not found. Please ensure all model files are present before publishing."
                    );
                }
                modelRequests.Add(new ModelRequest { id = model.id, filePath = modelPath });
            }

            return modelRequests;
        }

        private async Task<List<StructureData>> GetNewModelsForZoneAsync(ZoneData zoneData)
        {
            var existingModels = await modelService.ListWorldModels(
                currentWorldData.worldId,
                session.APIToken
            );

            return FilterNewModels(zoneData, existingModels);
        }

        private static List<StructureData> FilterNewModels<T>(
            ZoneData zoneData,
            IReadOnlyDictionary<string, T> existingModels
        )
        {
            if (zoneData?.objectPlacementData == null || zoneData.objectPlacementData.Count == 0)
                return new List<StructureData>();

            return zoneData
                .objectPlacementData.GroupBy(s => s.id)
                .Select(g => g.First())
                .Where(s => !existingModels.ContainsKey(s.id))
                .ToList();
        }
    }
}
