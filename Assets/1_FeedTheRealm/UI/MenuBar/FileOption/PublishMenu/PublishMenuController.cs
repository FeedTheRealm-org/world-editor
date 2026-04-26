using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using API;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.WorldObjects.Provider;
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
            CreatablesData creatablesData = dataPersistenceManager.GetCreatables(
                worldSelector.selectedWorld
            );

            var spritesRequest = new SpritesRequest();

            List<ItemData> allItems = new List<ItemData>();
            if (creatablesData != null)
            {
                allItems.AddRange(creatablesData.weaponItems);
                allItems.AddRange(creatablesData.consumableItems);
            }

            foreach (var item in allItems)
            {
                if (!string.IsNullOrEmpty(item.spriteFilePath))
                    spritesRequest.ids.Add(item.id);
                // TODO: this should be abstracted, the publish controller should not know about weapon's sprite path structure
                spritesRequest.spritePath.Add(
                    Path.Combine(config.SpritesDirectory, item.spriteFilePath)
                );
            }

            var missingSprites = spritesRequest
                .spritePath.Where(path => !File.Exists(path))
                .ToList();

            if (missingSprites.Count > 0)
            {
                var missing = string.Join("\n", missingSprites);
                throw new Exception($"Missing sprite files:\n{missing}");
            }

            if (assetsService == null)
            {
                logger.Log(
                    "PublishCosmetics: assetsService is unassigned in PublishMenuController.",
                    this,
                    Logging.LogType.Error
                );
                throw new Exception("AssetsService is missing.");
            }

            await spriteService.UploadSprites(
                spritesRequest,
                currentWorldData.worldId,
                session.APIToken
            );

            if (creatablesData != null && creatablesData.cosmetics != null)
            {
                await PublishCosmetics(creatablesData);
            }
        }

        private async Task PublishCosmetics(CreatablesData creatablesData)
        {
            try
            {
                var categoryListResp = await assetsService.GetCategoriesAsync();
                if (categoryListResp == null || categoryListResp.category_list == null)
                {
                    logger.Log(
                        "PublishCosmetics: Failed to fetch categories.",
                        this,
                        Logging.LogType.Error
                    );
                    return;
                }

                var categoryIds = new Dictionary<string, string>();
                foreach (var cat in categoryListResp.category_list)
                {
                    if (
                        !string.IsNullOrEmpty(cat.category_name)
                        && !string.IsNullOrEmpty(cat.category_id)
                    )
                    {
                        categoryIds[cat.category_name] = cat.category_id;
                    }
                }

                var localPathToUploadedSpriteId = new Dictionary<string, string>();
                bool changed = false;

                foreach (var cosmetic in creatablesData.cosmetics)
                {
                    cosmetic.OnAfterDeserialize();

                    var keys = cosmetic.category_sprites.Keys.ToList();
                    foreach (var categoryName in keys)
                    {
                        var spritePath = cosmetic.category_sprites[categoryName];
                        if (string.IsNullOrEmpty(spritePath))
                            continue;

                        string fullPath = Path.Combine(config.SpritesDirectory, spritePath);
                        if (!File.Exists(fullPath))
                            continue;

                        if (
                            !categoryIds.TryGetValue(categoryName, out string categoryId)
                            || string.IsNullOrEmpty(categoryId)
                        )
                        {
                            logger.Log(
                                $"PublishCosmetics: Category '{categoryName}' not found on server.",
                                this,
                                Logging.LogType.Warning
                            );
                            continue;
                        }

                        string existingSpriteId = null;

                        if (
                            cosmetic.category_urls.TryGetValue(categoryName, out var alreadySavedId)
                            && !string.IsNullOrEmpty(alreadySavedId)
                        )
                        {
                            if (
                                localPathToUploadedSpriteId.TryGetValue(fullPath, out var freshId)
                                && freshId != alreadySavedId
                            )
                            {
                                existingSpriteId = freshId;
                            }
                            else
                            {
                                existingSpriteId = alreadySavedId;
                                localPathToUploadedSpriteId[fullPath] = alreadySavedId;
                            }
                        }
                        else if (
                            !localPathToUploadedSpriteId.TryGetValue(fullPath, out existingSpriteId)
                        )
                        {
                            foreach (var kvp in cosmetic.category_sprites)
                            {
                                if (
                                    kvp.Value == spritePath
                                    && cosmetic.category_urls.TryGetValue(kvp.Key, out var savedId)
                                    && !string.IsNullOrEmpty(savedId)
                                )
                                {
                                    existingSpriteId = savedId;
                                    break;
                                }
                            }

                            if (!string.IsNullOrEmpty(existingSpriteId))
                                localPathToUploadedSpriteId[fullPath] = existingSpriteId;
                        }

                        var resp = await UploadOrLinkSpriteAsync(
                            categoryId,
                            categoryName,
                            fullPath,
                            existingSpriteId,
                            cosmetic.price
                        );

                        if (resp != null && !string.IsNullOrEmpty(resp.sprite_id))
                        {
                            localPathToUploadedSpriteId[fullPath] = resp.sprite_id;
                            cosmetic.category_urls[categoryName] = resp.sprite_id;

                            string currentFileName = Path.GetFileNameWithoutExtension(spritePath);
                            if (currentFileName != resp.sprite_id)
                            {
                                string ext = Path.GetExtension(spritePath);
                                string newFileName = resp.sprite_id + ext;
                                string newFullPath = Path.Combine(
                                    config.SpritesDirectory,
                                    newFileName
                                );

                                if (!File.Exists(newFullPath))
                                {
                                    File.Move(fullPath, newFullPath);
                                }

                                var keysToUpdate = cosmetic.category_sprites.Keys.ToList();
                                foreach (var key in keysToUpdate)
                                {
                                    if (cosmetic.category_sprites[key] == spritePath)
                                        cosmetic.category_sprites[key] = newFileName;
                                }

                                localPathToUploadedSpriteId[newFullPath] = resp.sprite_id;
                                localPathToUploadedSpriteId.Remove(fullPath);

                                spritePath = newFileName;
                                fullPath = newFullPath;
                            }

                            changed = true;
                        }
                        else
                        {
                            logger.Log(
                                $"PublishCosmetics: Failed to upload/link sprite for category '{categoryName}'.",
                                this,
                                Logging.LogType.Error
                            );
                        }
                    }
                }

                if (changed)
                {
                    dataPersistenceManager.SaveCreatablesData(
                        worldSelector.selectedWorld,
                        creatablesData
                    );
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
                Debug.Log(
                    $"Linking existing sprite for category '{categoryName}' with sprite ID '{existingSpriteId}' from world id '{currentWorldData.worldId}' with price {price}"
                );

                var (resp, statusCode) = await assetsService.LinkSpriteByIdAsync(
                    categoryId,
                    existingSpriteId,
                    currentWorldData.worldId,
                    price
                );

                if (resp != null)
                {
                    return resp;
                }

                if (statusCode != 404)
                    return null;

                logger.Log(
                    $"PublishCosmetics: Sprite '{existingSpriteId}' not found on server (404), falling back to upload for category '{categoryName}'.",
                    this,
                    Logging.LogType.Warning
                );
            }

            Debug.Log(
                $"Uploading new sprite for category '{categoryName}' from world id '{currentWorldData.worldId}' with price {price}"
            );

            return await assetsService.UploadSpriteAsync(
                categoryId,
                fullPath,
                currentWorldData.worldId,
                price
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

            var existingModels = await modelService.ListWorldModels(
                currentWorldData.worldId,
                session.APIToken
            );

            var newModels = zoneData
                .objectPlacementData.GroupBy(s => s.id)
                .Select(g => g.First())
                .Where(s => !existingModels.ContainsKey(s.id))
                .ToList();

            if (newModels.Count == 0)
                return;

            List<ModelRequest> modelRequests = new();

            // Validate all model files exist before attempting upload
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
    }
}
