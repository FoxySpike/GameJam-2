using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Authors only the Level 1 scene; does not change shared UI/player prefabs.</summary>
[InitializeOnLoad]
public static class ParrandaPresentationSetup
{
    private const string ScenePath = "Assets/Scenes/Nivel 1 - La parranda MVP.unity";
    private const int Version = 3;
    private static bool busy;

    static ParrandaPresentationSetup()
    {
        EditorApplication.delayCall += Apply;
        EditorSceneManager.sceneOpened += (scene, mode) =>
        {
            if (!busy && scene.path == ScenePath) EditorApplication.delayCall += Apply;
        };
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredEditMode) EditorApplication.delayCall += Apply;
        };
    }

    [MenuItem("Tools/Parranda/Actualizar llamada, textos y salida")]
    public static void Apply()
    {
        if (busy || EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += Apply;
            return;
        }
        busy = true;
        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        bool openedHere = !scene.IsValid() || !scene.isLoaded;
        bool success = false;
        int undo = -1;
        try
        {
            if (openedHere) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            var roots = scene.GetRootGameObjects();
            var flow = All<Level1FlowController>(roots).Single();
            var flowSettings = new SerializedObject(flow);
            if (flowSettings.FindProperty("presentationVersion").intValue >= Version)
            {
                success = true;
                return;
            }
            bool wasDirty = scene.isDirty;
            var call = All<WifeCallSequenceController>(roots).Single();
            var triggers = All<SceneTrigger>(roots).ToArray();
            var exit = triggers.Single(t => t.SceneToLoad == "Nivel-2-Asadero");
            var font = All<TMP_Text>(roots).Select(t => t.font).FirstOrDefault(f => f != null);

            Undo.IncrementCurrentGroup();
            undo = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Actualizar presentacion de Parranda MVP");
            if (flowSettings.FindProperty("presentationVersion").intValue < 2)
            {
                TranslateScene(roots);
                Set(flow, "initialObjective", "GET DRUNK");
                if (flowSettings.FindProperty("presentationVersion").intValue < 1)
                    BuildPhone(flow.transform, call, font);
                Set(call, "callerName", "Wife");
                Set(call, "nextObjective", "GET THE CHICKEN - GO TO THE CHICKEN SHOP");
                var callSettings = new SerializedObject(call);
                var dialogueLines = callSettings.FindProperty("dialogueLines");
                dialogueLines.arraySize = 2;
                dialogueLines.GetArrayElementAtIndex(0).stringValue = "Where are you?";
                dialogueLines.GetArrayElementAtIndex(1).stringValue = "And where's the chicken? Bring it home!";
                callSettings.ApplyModifiedProperties();
                foreach (var trigger in triggers)
                {
                    Set(trigger, "level1Flow", flow);
                    Set(trigger, "permanentlyLocked", trigger != exit);
                }
                foreach (var previousMarker in All<LevelExitBeacon>(roots).ToArray())
                    Undo.DestroyObjectImmediate(previousMarker.gameObject);
            }
            foreach (var trigger in triggers) BuildExitCube(trigger);
            Set(flow, "presentationVersion", Version);
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!wasDirty && !EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException("No se pudo guardar Parranda MVP.");
            Undo.CollapseUndoOperations(undo);
            success = true;
            Debug.Log("Parranda MVP: textos en ingles, telefono lateral y salidas bloqueadas. " +
                "Solo el asadero se abre al terminar la llamada. " +
                (wasDirty ? "Guardar los cambios de la escena con Ctrl+S." : "Escena guardada."), flow);
        }
        catch (Exception exception)
        {
            if (undo >= 0) Undo.RevertAllDownToGroup(undo);
            Debug.LogException(exception);
        }
        finally
        {
            if (openedHere && success && scene.IsValid() && scene.isLoaded)
                EditorSceneManager.CloseScene(scene, true);
            busy = false;
        }
    }

    private static IEnumerable<T> All<T>(GameObject[] roots) where T : Component =>
        roots.SelectMany(r => r.GetComponentsInChildren<T>(true));

    private static void Set(Object target, string name, object value)
    {
        var serialized = new SerializedObject(target);
        var property = serialized.FindProperty(name);
        if (property == null) throw new InvalidOperationException(target.name + ": missing property " + name);
        if (value is string text) property.stringValue = text;
        else if (value is bool flag) property.boolValue = flag;
        else if (value is int number) property.intValue = number;
        else if (value is float real) property.floatValue = real;
        else property.objectReferenceValue = (Object)value;
        serialized.ApplyModifiedProperties();
    }

    private static void TranslateScene(GameObject[] roots)
    {
        var translations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "ACEPTAR", "ACCEPT" }, { "RECHAZAR", "DECLINE" },
            { "SOBRIO", "SOBER" }, { "EMBORRÁCHATE", "GET DRUNK" }, { "EMBORRACHATE", "GET DRUNK" },
            { "CONSIGUE EL POLLO", "GET THE CHICKEN" }, { "VUELTO MIERDA", "WASTED" },
            { "Esposa", "Wife" }, { "Parcero", "Buddy" }, { "Llamada entrante...", "Incoming call..." },
            { "TELÉFONO · LLAMADA ENTRANTE", "PHONE - INCOMING CALL" },
            { "¿Dónde estás?", "Where are you?" }, { "¿Y EL POLLO?", "And where's the chicken?" },
            { "[E] Hablar", "[F] Talk" }, { "[F] Hablar", "[F] Talk" },
            { "[F] Beber", "[F] Drink" }, { "[E] Beber", "[F] Drink" },
            { "Beber", "Drink" }, { "Hablar", "Talk" }, { "Interactuar", "Interact" },
            { "[E] RECOGER POLLO", "[F] PICK UP CHICKEN" },
            { "Objetivo", "Objective" }, { "Resistencia", "Stamina" }, { "Equilibrio", "Balance" }
        };
        string Translate(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            if (value.Contains("WASD:"))
                return "WASD: Move   |   Shift: Sprint   |   F: Interact   |   Mouse: Look";
            value = value.Replace("[E]", "[F]");
            if (value == "[F] RECOGER POLLO") return "[F] PICK UP CHICKEN";
            return translations.TryGetValue(value.Trim(), out var translated) ? translated : value;
        }
        foreach (var text in All<TMP_Text>(roots))
        {
            string translated = Translate(text.text);
            if (translated == text.text) continue;
            Undo.RecordObject(text, "Translate visible text");
            text.text = translated;
            PrefabUtility.RecordPrefabInstancePropertyModifications(text);
        }
        foreach (var text in All<Text>(roots))
        {
            string translated = Translate(text.text);
            if (translated == text.text) continue;
            Undo.RecordObject(text, "Translate visible text");
            text.text = translated;
            PrefabUtility.RecordPrefabInstancePropertyModifications(text);
        }
        foreach (var dialogue in All<NPCDialogueController>(roots))
        {
            Set(dialogue, "characterName", "Buddy");
            Set(dialogue, "attention", "Come on, have a drink!");
            Set(dialogue, "offer", "How about a drink?");
            Set(dialogue, "accepted", "Cheers!");
            Set(dialogue, "rejected", "All right, maybe later.");
        }
        foreach (var offer in All<NPCDrinkOffer>(roots)) Set(offer, "interactionPrompt", "[F] Talk");
        foreach (var drink in All<DrinkInteractable>(roots)) Set(drink, "interactionPrompt", "[F] Drink");
        foreach (var hud in All<AlcoholHUD>(roots)) Set(hud, "wastedLabel", "Wasted");
        foreach (var chicken in All<ChickenCarryController>(roots)) Set(chicken, "pickupPrompt", "[F] PICK UP CHICKEN");
    }

    private static void BuildPhone(Transform parent, WifeCallSequenceController call, TMP_FontAsset font)
    {
        var oldSettings = new SerializedObject(call);
        var oldPanel = oldSettings.FindProperty("phonePanel").objectReferenceValue as GameObject;
        var blackout = oldSettings.FindProperty("blackout").objectReferenceValue as CanvasGroup;
        if (oldPanel != null)
        {
            Undo.RecordObject(oldPanel, "Hide old call panel");
            oldPanel.SetActive(false);
            PrefabUtility.RecordPrefabInstancePropertyModifications(oldPanel);
        }
        if (blackout != null)
        {
            Undo.RecordObject(blackout, "Disable blackout");
            blackout.alpha = 0f;
            blackout.blocksRaycasts = false;
            blackout.interactable = false;
            Undo.RecordObject(blackout.gameObject, "Hide blackout");
            blackout.gameObject.SetActive(false);
            PrefabUtility.RecordPrefabInstancePropertyModifications(blackout);
            PrefabUtility.RecordPrefabInstancePropertyModifications(blackout.gameObject);
        }

        var canvasObject = new GameObject("Wife Call Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvasObject.transform.SetParent(parent, false);
        Undo.RegisterCreatedObjectUndo(canvasObject, "Create phone canvas");
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 40;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        // No GraphicRaycaster and no Buttons: this is a visual notification only.
        var phone = Panel("Phone", canvasObject.transform, new Vector2(300, 520), new Vector2(-38, 80), new Color(0.035f, 0.04f, 0.05f));
        phone.anchorMin = phone.anchorMax = new Vector2(1, 0);
        phone.pivot = new Vector2(1, 0);
        var group = phone.gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;
        var screen = Panel("Screen", phone, new Vector2(278, 494), Vector2.zero, new Color(0.065f, 0.09f, 0.12f));
        Panel("Earpiece", screen, new Vector2(68, 5), new Vector2(0, 230), new Color(0.025f, 0.03f, 0.04f));
        Label("Network", screen, "PARRANDA     4G", new Vector2(200, 22), new Vector2(0, 204), 13, new Color(0.6f, 0.67f, 0.72f), font);
        var status = Label("Call Status", screen, "INCOMING CALL", new Vector2(244, 26), new Vector2(0, 153), 16, new Color(0.4f, 0.9f, 0.72f), font);
        var portrait = Panel("Contact", screen, new Vector2(78, 78), new Vector2(0, 88), new Color(0.86f, 0.62f, 0.28f));
        portrait.GetComponent<Image>().sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        Label("Contact Initial", portrait, "W", new Vector2(65, 65), Vector2.zero, 36, Color.white, font);
        var caller = Label("Caller", screen, "Wife", new Vector2(244, 44), new Vector2(0, 20), 30, Color.white, font);
        var timer = Label("Call Timer", screen, "", new Vector2(244, 22), new Vector2(0, -18), 15, new Color(0.6f, 0.67f, 0.72f), font);
        var caption = Panel("Caption", screen, new Vector2(246, 108), new Vector2(0, -94), new Color(0.10f, 0.14f, 0.18f));
        var dialogue = Label("Call Dialogue", caption, "Incoming call...", new Vector2(222, 92), Vector2.zero, 20, Color.white, font);
        dialogue.enableAutoSizing = true;
        dialogue.fontSizeMin = 17;
        dialogue.fontSizeMax = 20;
        var handset = Panel("Call Symbol", screen, new Vector2(52, 52), new Vector2(0, -189), new Color(0.79f, 0.22f, 0.23f));
        handset.GetComponent<Image>().sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        Panel("Handset Bridge", handset, new Vector2(26, 6), new Vector2(0, 3), Color.white);
        Panel("Handset Left", handset, new Vector2(8, 13), new Vector2(-11, -1), Color.white);
        Panel("Handset Right", handset, new Vector2(8, 13), new Vector2(11, -1), Color.white);
        Panel("Home Indicator", screen, new Vector2(65, 4), new Vector2(0, -231), new Color(0.45f, 0.5f, 0.55f));

        Set(call, "phonePanel", phone.gameObject);
        Set(call, "phoneGroup", group);
        Set(call, "callerText", caller);
        Set(call, "dialogueText", dialogue);
        Set(call, "statusText", status);
        Set(call, "timerText", timer);
        Set(call, "slideDuration", 0.35f);
        Set(call, "incomingCallDuration", 2f);
        Set(call, "lineDuration", 3.2f);
        Set(call, "callerName", "Wife");
        Set(call, "nextObjective", "GET THE CHICKEN - GO TO THE CHICKEN SHOP");
        var settings = new SerializedObject(call);
        var lines = settings.FindProperty("dialogueLines");
        lines.arraySize = 2;
        lines.GetArrayElementAtIndex(0).stringValue = "Where are you?";
        lines.GetArrayElementAtIndex(1).stringValue = "And where's the chicken? Bring it home!";
        settings.ApplyModifiedProperties();
        phone.gameObject.SetActive(false);
    }

    private static RectTransform Panel(string name, Transform parent, Vector2 size, Vector2 position, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        image.type = Image.Type.Sliced;
        return rect;
    }

    private static TMP_Text Label(string name, Transform parent, string value, Vector2 size,
        Vector2 position, float fontSize, Color color, TMP_FontAsset font)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<TextMeshProUGUI>();
        text.rectTransform.anchorMin = text.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        text.rectTransform.sizeDelta = size;
        text.rectTransform.anchoredPosition = position;
        if (font != null) text.font = font;
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    private static void BuildExitCube(SceneTrigger exit)
    {
        var existing = exit.GetComponentInChildren<LevelExitBeacon>(true);
        if (existing != null)
        {
            Set(existing, "exit", exit);
            var settings = new SerializedObject(existing);
            var visual = settings.FindProperty("visuals").objectReferenceValue as GameObject;
            if (visual != null)
            {
                Undo.RecordObject(visual, "Hide locked exit placeholder");
                visual.SetActive(false);
            }
            return;
        }
        var collider = exit.GetComponent<Collider>();
        if (collider == null) throw new InvalidOperationException(exit.name + " needs a collider.");
        var root = new GameObject("Exit Marker");
        root.transform.SetParent(exit.transform, true);
        root.transform.position = collider.bounds.center + Vector3.up * (collider.bounds.extents.y + 2f);
        Undo.RegisterCreatedObjectUndo(root, "Create exit placeholder");
        var marker = root.AddComponent<LevelExitBeacon>();
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(cube.GetComponent<Collider>());
        cube.name = "Exit Marker Cube";
        cube.transform.SetParent(root.transform, false);
        Set(marker, "exit", exit);
        Set(marker, "visuals", cube);
        // The marker controller activates only the cube belonging to an available exit.
        cube.SetActive(false);
    }
}
