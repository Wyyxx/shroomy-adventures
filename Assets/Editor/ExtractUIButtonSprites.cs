using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class ExtractUIButtonSprites
{
    private const string SourceAssetPath = "Assets/Sprites/UI/Botones/4Z_2101.w017.n001.354B.p15.354.jpg";
    private const string OutputFolderAssetPath = "Assets/Sprites/UI/Botones/Extracted";
    private const string OptionsPanelBackgroundPath = OutputFolderAssetPath + "/options_panel_board.png";
    private const string OptionsCloseButtonPath = OutputFolderAssetPath + "/options_close_button.png";

    private sealed class ComponentBounds
    {
        public int MinX;
        public int MinY;
        public int MaxX;
        public int MaxY;
        public int Count;

        public int Width => MaxX - MinX + 1;
        public int Height => MaxY - MinY + 1;
        public float Aspect => (float)Width / Height;
    }

    [MenuItem("Tools/Shroomy/Extract UI Button Sprites")]
    public static void ExtractAndAssign()
    {
        var projectDirectory = Directory.GetParent(Application.dataPath);
        if (projectDirectory == null)
        {
            Debug.LogError("Unable to resolve the project root directory.");
            return;
        }

        var projectRoot = projectDirectory.FullName;
        var sourceAbsolutePath = Path.Combine(projectRoot, SourceAssetPath);
        var outputAbsolutePath = Path.Combine(projectRoot, OutputFolderAssetPath);
        Directory.CreateDirectory(outputAbsolutePath);

        var sourceBytes = File.ReadAllBytes(sourceAbsolutePath);
        var sourceTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        sourceTexture.LoadImage(sourceBytes, false);

        var pixels = sourceTexture.GetPixels();
        var visited = new bool[sourceTexture.width * sourceTexture.height];
        var queue = new Queue<int>();
        var components = new List<ComponentBounds>();

        for (var y = 0; y < sourceTexture.height; y++)
        {
            for (var x = 0; x < sourceTexture.width; x++)
            {
                var startIndex = y * sourceTexture.width + x;
                if (visited[startIndex])
                {
                    continue;
                }

                visited[startIndex] = true;
                if (IsBackground(pixels[startIndex]))
                {
                    continue;
                }

                var bounds = new ComponentBounds
                {
                    MinX = x,
                    MaxX = x,
                    MinY = y,
                    MaxY = y,
                    Count = 0,
                };

                queue.Enqueue(startIndex);
                while (queue.Count > 0)
                {
                    var index = queue.Dequeue();
                    var currentX = index % sourceTexture.width;
                    var currentY = index / sourceTexture.width;
                    bounds.Count++;

                    if (currentX < bounds.MinX) bounds.MinX = currentX;
                    if (currentX > bounds.MaxX) bounds.MaxX = currentX;
                    if (currentY < bounds.MinY) bounds.MinY = currentY;
                    if (currentY > bounds.MaxY) bounds.MaxY = currentY;

                    EnqueueIfForeground(index - 1, currentX > 0, visited, pixels, queue);
                    EnqueueIfForeground(index + 1, currentX < sourceTexture.width - 1, visited, pixels, queue);
                    EnqueueIfForeground(index - sourceTexture.width, currentY > 0, visited, pixels, queue);
                    EnqueueIfForeground(index + sourceTexture.width, currentY < sourceTexture.height - 1, visited, pixels, queue);
                }

                if (bounds.Count >= 4000)
                {
                    components.Add(bounds);
                }
            }
        }

        var sortedComponents = components
            .OrderByDescending(component => component.MaxY)
            .ThenBy(component => component.MinX)
            .ToList();

        var exportedPaths = new List<string>();
        for (var i = 0; i < sortedComponents.Count; i++)
        {
            var fileName = BuildFileName(sortedComponents[i], i);
            exportedPaths.Add(ExportComponent(sourceTexture, pixels, sortedComponents[i], outputAbsolutePath, fileName));
        }

        AssetDatabase.Refresh();

        foreach (var assetPath in exportedPaths)
        {
            ConfigureImportedSprite(assetPath);
        }

        AssetDatabase.SaveAssets();
        AssignMenuButtons();

        Object.DestroyImmediate(sourceTexture);
        Debug.Log($"Extracted {exportedPaths.Count} sprites into {OutputFolderAssetPath}");
    }

    [MenuItem("Tools/Shroomy/Polish Main Menu Buttons")]
    public static void PolishMainMenuButtons()
    {
        ApplyButtonPolish("PlayButton", new Vector2(360f, 104f), -145f, "JUGAR");
        ApplyButtonPolish("OptionsButton", new Vector2(360f, 104f), -275f, "OPCIONES");
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    [MenuItem("Tools/Shroomy/Polish Options Panel")]
    public static void PolishOptionsPanel()
    {
        EnsureOptionsPanelBackground();
        EnsureOptionsCloseButtonBackground();

        var panel = GameObject.Find("OptionsPanel");
        if (panel == null)
        {
            Debug.LogWarning("OptionsPanel was not found.");
            return;
        }

        panel.transform.SetAsLastSibling();

        var backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(OptionsPanelBackgroundPath);
        var panelImage = panel.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.sprite = backgroundSprite;
            panelImage.overrideSprite = backgroundSprite;
            panelImage.type = Image.Type.Simple;
            panelImage.color = Color.white;
            panelImage.preserveAspect = false;
        }

        var panelRect = panel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.anchorMin = new Vector2(0.16f, 0.03f);
            panelRect.anchorMax = new Vector2(0.84f, 0.78f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
        }

        var contentArea = panel.transform.Find("ContentArea") as RectTransform;
        if (contentArea != null)
        {
            contentArea.anchorMin = new Vector2(0f, 0f);
            contentArea.anchorMax = new Vector2(1f, 1f);
            contentArea.sizeDelta = new Vector2(-150f, -320f);
            contentArea.anchoredPosition = new Vector2(0f, -110f);
        }

        StyleTitle(panel.transform.Find("TitleText"));
        StyleCloseButton(panel.transform.Find("BtnCerrar"));
        StyleTabBar(panel.transform.Find("TabBar"));
        StylePrimaryAction(panel.transform.Find("ContentArea/PanelGeneral/BtnPantallaCompleta"), "PANTALLA COMPLETA");
        StyleLabel(panel.transform.Find("ContentArea/PanelGeneral/VolumenLabel"), "VOLUMEN");
        StyleLabel(panel.transform.Find("ContentArea/PanelGeneral/GraficosLabel"), "GRAFICOS");
        StyleSlider(panel.transform.Find("ContentArea/PanelGeneral/SliderVolumen"));
        StyleDropdown(panel.transform.Find("ContentArea/PanelGeneral/DropdownGraficos"));

        var controlsPanel = panel.transform.Find("ContentArea/PanelControles");
        if (controlsPanel != null)
        {
            var controlTexts = controlsPanel.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
            foreach (var text in controlTexts)
            {
                text.color = new Color32(255, 235, 194, 255);
                text.fontSize = 28f;
                text.fontStyle = TMPro.FontStyles.Bold;
                text.outlineWidth = 0.15f;
                text.outlineColor = new Color32(63, 27, 8, 255);
            }
        }

        EditorUtility.SetDirty(panel);
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    private static bool IsBackground(Color color)
    {
        return color.r > 0.94f && color.g > 0.94f && color.b > 0.94f;
    }

    private static void EnqueueIfForeground(int index, bool canVisit, bool[] visited, Color[] pixels, Queue<int> queue)
    {
        if (!canVisit || visited[index])
        {
            return;
        }

        visited[index] = true;
        if (!IsBackground(pixels[index]))
        {
            queue.Enqueue(index);
        }
    }

    private static string ExportComponent(Texture2D sourceTexture, Color[] pixels, ComponentBounds component, string outputAbsolutePath, string fileName)
    {
        const int padding = 8;
        var minX = Mathf.Max(0, component.MinX - padding);
        var maxX = Mathf.Min(sourceTexture.width - 1, component.MaxX + padding);
        var minY = Mathf.Max(0, component.MinY - padding);
        var maxY = Mathf.Min(sourceTexture.height - 1, component.MaxY + padding);
        var width = maxX - minX + 1;
        var height = maxY - minY + 1;

        var extracted = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var pixel = pixels[(minY + y) * sourceTexture.width + (minX + x)];
                if (IsBackground(pixel))
                {
                    pixel.a = 0f;
                }

                extracted.SetPixel(x, y, pixel);
            }
        }

        extracted.Apply();

        var absolutePath = Path.Combine(outputAbsolutePath, fileName);
        File.WriteAllBytes(absolutePath, extracted.EncodeToPNG());
        Object.DestroyImmediate(extracted);
        return $"{OutputFolderAssetPath}/{fileName}";
    }

    private static string BuildFileName(ComponentBounds component, int index)
    {
        if (component.Aspect > 2.4f && component.Width > 500 && component.Height > 140 && component.Height < 320)
        {
            return $"button_wide_{index + 1:00}.png";
        }

        if (component.Aspect > 0.8f && component.Aspect < 1.2f && component.Width > 120 && component.Width < 320)
        {
            return $"button_square_{index + 1:00}.png";
        }

        if (component.Aspect > 0.75f && component.Aspect < 1.3f && component.Width > 120 && component.Width < 320)
        {
            return $"button_round_{index + 1:00}.png";
        }

        if (component.Aspect > 0.8f && component.Aspect < 1.4f && component.Width > 180 && component.Height > 150)
        {
            return $"icon_{index + 1:00}.png";
        }

        return $"ui_piece_{index + 1:00}.png";
    }

    private static void ConfigureImportedSprite(string assetPath)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.isReadable = false;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.filterMode = FilterMode.Bilinear;
        importer.SaveAndReimport();
    }

    private static void AssignMenuButtons()
    {
        var playButton = GameObject.Find("PlayButton");
        var optionsButton = GameObject.Find("OptionsButton");
        if (playButton == null || optionsButton == null)
        {
            Debug.LogWarning("PlayButton or OptionsButton were not found in the active scene.");
            return;
        }

        var extractedSprites = AssetDatabase.FindAssets("t:Sprite", new[] { OutputFolderAssetPath })
            .Select(AssetDatabase.GUIDToAssetPath)
            .OrderBy(path => path)
            .ToList();

        var wideSprites = extractedSprites
            .Where(path => Path.GetFileName(path).StartsWith("button_wide_"))
            .Select(path => AssetDatabase.LoadAssetAtPath<Sprite>(path))
            .Where(sprite => sprite != null)
            .ToList();

        if (wideSprites.Count < 2)
        {
            Debug.LogWarning("Not enough wide button sprites were extracted to assign PlayButton and OptionsButton.");
            return;
        }

        ApplySprite(playButton, wideSprites[0]);
        ApplySprite(optionsButton, wideSprites[1]);
        EditorUtility.SetDirty(playButton);
        EditorUtility.SetDirty(optionsButton);
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    private static void ApplySprite(GameObject target, Sprite sprite)
    {
        var image = target.GetComponent<Image>();
        if (image == null)
        {
            return;
        }

        image.sprite = sprite;
        image.overrideSprite = sprite;
        image.type = Image.Type.Simple;
        image.color = Color.white;
        image.preserveAspect = false;
        image.SetNativeSize();

        var rectTransform = target.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(250f, 70f);
        }
    }

    private static void ApplyButtonPolish(string buttonName, Vector2 size, float anchoredY, string label)
    {
        var buttonObject = GameObject.Find(buttonName);
        if (buttonObject == null)
        {
            Debug.LogWarning($"Button '{buttonName}' was not found.");
            return;
        }

        var buttonRect = buttonObject.GetComponent<RectTransform>();
        if (buttonRect != null)
        {
            buttonRect.sizeDelta = size;
            buttonRect.anchoredPosition = new Vector2(buttonRect.anchoredPosition.x, anchoredY);
        }

        var buttonImage = buttonObject.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = Color.white;
            buttonImage.preserveAspect = false;
            buttonImage.raycastTarget = true;
        }

        var buttonText = buttonObject.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = label;
            buttonText.fontSize = 34f;
            buttonText.enableAutoSizing = true;
            buttonText.fontSizeMin = 28f;
            buttonText.fontSizeMax = 38f;
            buttonText.fontStyle = TMPro.FontStyles.Bold;
            buttonText.color = new Color32(88, 44, 14, 255);
            buttonText.faceColor = new Color32(88, 44, 14, 255);
            buttonText.outlineColor = new Color32(255, 237, 180, 255);
            buttonText.outlineWidth = 0.18f;
            buttonText.alignment = TMPro.TextAlignmentOptions.Center;
            buttonText.characterSpacing = 1.5f;
            buttonText.enableWordWrapping = false;
            buttonText.margin = new Vector4(22f, 8f, 22f, 10f);
            buttonText.raycastTarget = false;

            var textRect = buttonText.rectTransform;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            EditorUtility.SetDirty(buttonText);
        }

        EditorUtility.SetDirty(buttonObject);
    }

    private static void EnsureOptionsPanelBackground()
    {
        if (AssetDatabase.LoadAssetAtPath<Sprite>(OptionsPanelBackgroundPath) != null)
        {
            return;
        }

        var projectDirectory = Directory.GetParent(Application.dataPath);
        if (projectDirectory == null)
        {
            return;
        }

        var root = projectDirectory.FullName;
        var sourceAbsolutePath = Path.Combine(root, SourceAssetPath);
        var outputAbsolutePath = Path.Combine(root, OptionsPanelBackgroundPath);
        Directory.CreateDirectory(Path.GetDirectoryName(outputAbsolutePath)!);

        var sourceBytes = File.ReadAllBytes(sourceAbsolutePath);
        var sourceTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        sourceTexture.LoadImage(sourceBytes, false);

        var crop = new RectInt(1914, 2336, 1889, 1392);
        var extracted = new Texture2D(crop.width, crop.height, TextureFormat.RGBA32, false);
        for (var y = 0; y < crop.height; y++)
        {
            for (var x = 0; x < crop.width; x++)
            {
                var color = sourceTexture.GetPixel(crop.x + x, crop.y + y);
                if (IsBackground(color))
                {
                    color.a = 0f;
                }

                extracted.SetPixel(x, y, color);
            }
        }

        extracted.Apply();
        File.WriteAllBytes(outputAbsolutePath, extracted.EncodeToPNG());
        Object.DestroyImmediate(extracted);
        Object.DestroyImmediate(sourceTexture);

        AssetDatabase.Refresh();
        ConfigureImportedSprite(OptionsPanelBackgroundPath);
    }

    private static void EnsureOptionsCloseButtonBackground()
    {
        if (AssetDatabase.LoadAssetAtPath<Sprite>(OptionsCloseButtonPath) != null)
        {
            return;
        }

        var projectDirectory = Directory.GetParent(Application.dataPath);
        if (projectDirectory == null)
        {
            return;
        }

        var root = projectDirectory.FullName;
        var sourceAbsolutePath = Path.Combine(root, SourceAssetPath);
        var outputAbsolutePath = Path.Combine(root, OptionsCloseButtonPath);
        Directory.CreateDirectory(Path.GetDirectoryName(outputAbsolutePath)!);

        var sourceBytes = File.ReadAllBytes(sourceAbsolutePath);
        var sourceTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        sourceTexture.LoadImage(sourceBytes, false);

        var crop = new RectInt(3029, 1265, 337, 350);
        var extracted = new Texture2D(crop.width, crop.height, TextureFormat.RGBA32, false);
        for (var y = 0; y < crop.height; y++)
        {
            for (var x = 0; x < crop.width; x++)
            {
                var color = sourceTexture.GetPixel(crop.x + x, crop.y + y);
                if (IsBackground(color))
                {
                    color.a = 0f;
                }

                extracted.SetPixel(x, y, color);
            }
        }

        extracted.Apply();
        File.WriteAllBytes(outputAbsolutePath, extracted.EncodeToPNG());
        Object.DestroyImmediate(extracted);
        Object.DestroyImmediate(sourceTexture);

        AssetDatabase.Refresh();
        ConfigureImportedSprite(OptionsCloseButtonPath);
    }

    private static void StyleTitle(Transform? titleTransform)
    {
        if (titleTransform == null)
        {
            return;
        }

        var rect = titleTransform.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.20f, 0.81f);
            rect.anchorMax = new Vector2(0.80f, 0.91f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        var text = titleTransform.GetComponent<TMPro.TextMeshProUGUI>();
        if (text == null)
        {
            return;
        }

        text.text = "OPCIONES";
        text.fontSize = 56f;
        text.enableAutoSizing = true;
        text.fontSizeMin = 42f;
        text.fontSizeMax = 62f;
        text.fontStyle = TMPro.FontStyles.Bold;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.color = new Color32(255, 231, 176, 255);
        text.faceColor = new Color32(255, 231, 176, 255);
        text.outlineColor = new Color32(73, 31, 10, 255);
        text.outlineWidth = 0.24f;
        text.characterSpacing = 3f;
    }

    private static void StyleCloseButton(Transform? closeTransform)
    {
        if (closeTransform == null)
        {
            return;
        }

        var rect = closeTransform.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.90f, 0.80f);
            rect.anchorMax = new Vector2(0.955f, 0.88f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        var image = closeTransform.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(OptionsCloseButtonPath);
            image.overrideSprite = image.sprite;
            image.type = Image.Type.Simple;
            image.color = Color.white;
        }

        var text = closeTransform.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
        if (text == null)
        {
            return;
        }

        text.text = "X";
        text.fontSize = 30f;
        text.fontStyle = TMPro.FontStyles.Bold;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.color = new Color32(94, 37, 14, 255);
        text.outlineWidth = 0.18f;
        text.outlineColor = new Color32(255, 231, 176, 255);
    }

    private static void StyleTabBar(Transform? tabBarTransform)
    {
        if (tabBarTransform == null)
        {
            return;
        }

        var rect = tabBarTransform.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.15f, 0.64f);
            rect.anchorMax = new Vector2(0.85f, 0.76f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        var layout = tabBarTransform.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        if (layout != null)
        {
            layout.spacing = 32f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.padding = new RectOffset(18, 18, 8, 8);
        }

        StyleTabButton(tabBarTransform.Find("BtnGeneral"), "GENERAL", true);
        StyleTabButton(tabBarTransform.Find("BtnControles"), "CONTROLES", false);
    }

    private static void StyleTabButton(Transform? buttonTransform, string label, bool selected)
    {
        if (buttonTransform == null)
        {
            return;
        }

        var rect = buttonTransform.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(250f, 78f);
        }

        var image = buttonTransform.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(OutputFolderAssetPath + "/button_wide_03.png");
            image.overrideSprite = image.sprite;
            image.type = Image.Type.Simple;
            image.color = selected
                ? new Color32(255, 255, 255, 255)
                : new Color32(214, 184, 138, 255);
        }

        var text = buttonTransform.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
        if (text == null)
        {
            return;
        }

        text.text = label;
        text.fontSize = 31f;
        text.enableAutoSizing = true;
        text.fontSizeMin = 24f;
        text.fontSizeMax = 34f;
        text.fontStyle = TMPro.FontStyles.Bold;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.color = selected ? new Color32(102, 49, 17, 255) : new Color32(70, 39, 19, 255);
        text.outlineWidth = 0.18f;
        text.outlineColor = selected
            ? new Color32(255, 243, 203, 255)
            : new Color32(246, 226, 183, 255);
        text.characterSpacing = 1.8f;
        text.margin = new Vector4(22f, 8f, 22f, 10f);
    }

    private static void StylePrimaryAction(Transform? buttonTransform, string label)
    {
        if (buttonTransform == null)
        {
            return;
        }

        var image = buttonTransform.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(OutputFolderAssetPath + "/button_wide_01.png");
            image.overrideSprite = image.sprite;
            image.type = Image.Type.Simple;
            image.color = Color.white;
        }

        var rect = buttonTransform.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.28f, 0.46f);
            rect.anchorMax = new Vector2(0.72f, 0.58f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        var text = buttonTransform.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
        if (text == null)
        {
            return;
        }

        text.text = label;
        text.fontSize = 30f;
        text.enableAutoSizing = true;
        text.fontSizeMin = 24f;
        text.fontSizeMax = 34f;
        text.fontStyle = TMPro.FontStyles.Bold;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.color = new Color32(88, 44, 14, 255);
        text.outlineColor = new Color32(255, 237, 180, 255);
        text.outlineWidth = 0.18f;
        text.margin = new Vector4(24f, 10f, 24f, 10f);
    }

    private static void StyleLabel(Transform? labelTransform, string textValue)
    {
        if (labelTransform == null)
        {
            return;
        }

        var text = labelTransform.GetComponent<TMPro.TextMeshProUGUI>();
        if (text == null)
        {
            return;
        }

        text.text = textValue;
        text.fontSize = 32f;
        text.fontStyle = TMPro.FontStyles.Bold;
        text.color = new Color32(255, 235, 194, 255);
        text.outlineColor = new Color32(63, 27, 8, 255);
        text.outlineWidth = 0.18f;
        text.characterSpacing = 2f;
    }

    private static void StyleSlider(Transform? sliderTransform)
    {
        if (sliderTransform == null)
        {
            return;
        }

        var background = sliderTransform.Find("Background");
        var fill = sliderTransform.Find("Fill Area/Fill");
        var handle = sliderTransform.Find("Handle Slide Area/Handle");

        SetImageColor(background, new Color32(74, 38, 20, 220));
        SetImageColor(fill, new Color32(233, 164, 46, 255));
        SetImageColor(handle, new Color32(255, 244, 214, 255));

        if (background != null)
        {
            var bgRect = background.GetComponent<RectTransform>();
            if (bgRect != null)
            {
                bgRect.offsetMin = new Vector2(0f, 26f);
                bgRect.offsetMax = new Vector2(0f, -26f);
            }
        }

        if (handle != null)
        {
            var handleRect = handle.GetComponent<RectTransform>();
            if (handleRect != null)
            {
                handleRect.sizeDelta = new Vector2(34f, 34f);
            }
        }
    }

    private static void StyleDropdown(Transform? dropdownTransform)
    {
        if (dropdownTransform == null)
        {
            return;
        }

        var image = dropdownTransform.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color32(62, 33, 24, 240);
        }

        var rect = dropdownTransform.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.36f, 0.26f);
            rect.anchorMax = new Vector2(0.75f, 0.39f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        var caption = dropdownTransform.Find("Label")?.GetComponent<TMPro.TextMeshProUGUI>();
        if (caption != null)
        {
            caption.fontSize = 26f;
            caption.fontStyle = TMPro.FontStyles.Bold;
            caption.color = new Color32(255, 231, 186, 255);
            caption.outlineWidth = 0.14f;
            caption.outlineColor = new Color32(59, 26, 8, 255);
        }

        var arrowImage = dropdownTransform.Find("Arrow")?.GetComponent<Image>();
        if (arrowImage != null)
        {
            arrowImage.color = new Color32(255, 216, 129, 255);
        }

        var templateImage = dropdownTransform.Find("Template")?.GetComponent<Image>();
        if (templateImage != null)
        {
            templateImage.color = new Color32(44, 24, 18, 245);
        }
    }

    private static void SetImageColor(Transform? transform, Color color)
    {
        if (transform == null)
        {
            return;
        }

        var image = transform.GetComponent<Image>();
        if (image != null)
        {
            image.color = color;
        }
    }
}
