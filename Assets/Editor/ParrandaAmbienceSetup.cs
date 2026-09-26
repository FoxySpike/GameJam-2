using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>One-time scene authoring, never runs in a player or enters Play Mode.</summary>
[InitializeOnLoad]
public static class ParrandaAmbienceSetup
{
    private const string ScenePath = "Assets/Scenes/Nivel 1 - La parranda MVP.unity";
    private const string Output = "Assets/Ambiente/ParrandaMVP";
    private const string RootName = "Ambiente Parranda MVP";
    private static bool busy;

    static ParrandaAmbienceSetup()
    {
        EditorApplication.delayCall += Apply;
        EditorSceneManager.sceneOpened += (scene, mode) =>
        {
            if (!busy && scene.path == ScenePath) EditorApplication.delayCall += Apply;
        };
    }

    [MenuItem("Tools/Parranda/Integrar ambiente MVP")]
    public static void Apply()
    {
        if (busy || EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += Apply;
            return;
        }

        busy = true;
        var scene = SceneManager.GetSceneByPath(ScenePath);
        bool openedHere = !scene.IsValid() || !scene.isLoaded;
        bool success = false;
        try
        {
            if (openedHere) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            var roots = scene.GetRootGameObjects();
            var existing = roots.FirstOrDefault(r => r.name == RootName);
            if (existing != null && existing.GetComponent<ParrandaMusicPlayer>() != null)
            {
                ApplySeatedAssignments(scene);
                success = true;
                return;
            }

            bool wasDirty = scene.isDirty;
            EnsureFolder(Output);
            // Copies keep rig/import changes local to this integration, even if the
            // original Ambiente files are later used by a different scene.
            string remyPath = PrepareModel("Remy (1)", false);
            string ch21Path = PrepareModel("Ch21_nonPBR", false);
            var salsa = PrepareClip("Salsa Dancing", true);
            var samba = PrepareClip("Samba Dancing", true);
            var sitting = PrepareClip("Sitting", false);
            var idle = PrepareClip("Sitting Idle", true);
            var salsaController = Controller("Salsa", salsa);
            var sambaController = Controller("Samba", samba);
            var seatedController = Controller("Sentado", sitting);
            var seatedIdleController = Controller("SentadoIdle", idle);

            var transforms = roots.SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
            var packs = transforms.Where(t => t.name == "Speaker Pack #1" || t.name == "Speaker Pack #1 (1)").ToArray();
            if (packs.Length != 2) throw new InvalidOperationException("Se esperaban los dos Speaker Pack #1 en Parranda MVP.");
            var chair = FindFreeChair(roots);
            if (chair == null) throw new InvalidOperationException("No se encontro una silla libre de la escena para Ch21.");

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Integrar ambiente Parranda MVP");
            try
            {
                var root = new GameObject(RootName);
                SceneManager.MoveGameObjectToScene(root, scene);
                Undo.RegisterCreatedObjectUndo(root, "Crear ambiente");

                AddNpc(remyPath, "NPC_AMBIENTE_Remy_Salsa", root.transform,
                    new Vector3(1.2f, -0.03f, -3.8f), Quaternion.Euler(0, 160, 0), salsaController);
                var seated = AddNpc(ch21Path, "NPC_AMBIENTE_Ch21_Sentado", root.transform,
                    Vector3.zero, Quaternion.identity, seatedController);
                PlaceOnChair(seated, chair, idle, ch21Path);

                int animated = 0;
                foreach (var actor in transforms.Where(t => t.name == "NPC_BARRIO 1" ||
                             t.name == "NPC_BARRIO 3" || t.name == "NPC_COCINERO"))
                {
                    // Also protect by behaviour, in case names change in the future.
                    if (actor.GetComponentInChildren<NPCApproachPlayer>(true) != null ||
                        actor.GetComponentInChildren<NPCDrinkOffer>(true) != null) continue;
                    var animator = actor.GetComponentInChildren<Animator>(true);
                    if (animator == null || animator.avatar == null || !animator.avatar.isHuman)
                        throw new InvalidOperationException("El NPC no tiene un Avatar humanoide: " + actor.name);
                    Undo.RecordObject(animator, "Animar NPC de ambiente");
                    animator.runtimeAnimatorController = actor.name == "NPC_BARRIO 1" ? salsaController :
                        actor.name == "NPC_BARRIO 3" ? seatedIdleController : sambaController;
                    animator.applyRootMotion = false;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(animator);
                    animated++;
                }

                if (animated != 3)
                    throw new InvalidOperationException("No se encontraron los tres NPCs de ambiente esperados. No se modificara al seguidor.");

                var sources = new List<AudioSource>();
                foreach (var pack in packs)
                {
                    // The remaining imported speaker groups are empty parent objects.
                    // Prefer explicitly placed empty GameObjects if the user added them.
                    var empties = pack.GetComponentsInChildren<Transform>(true)
                        .Where(t => t != pack && t.gameObject.activeInHierarchy && t.name.StartsWith("GameObject") &&
                                    t.GetComponent<Renderer>() == null).Take(2).ToArray();
                    if (empties.Length == 0)
                        empties = pack.Cast<Transform>().Where(t => t.gameObject.activeInHierarchy &&
                            t.GetComponent<Renderer>() == null && t.GetComponentInChildren<Renderer>() != null).Take(2).ToArray();
                    if (empties.Length == 0) empties = new[] { pack };
                    foreach (var emitter in empties)
                    {
                        var source = Undo.AddComponent<AudioSource>(emitter.gameObject);
                        source.playOnAwake = false;
                        source.loop = false;
                        source.volume = 0.45f / empties.Length;
                        source.spatialBlend = 1f;
                        source.rolloffMode = AudioRolloffMode.Linear;
                        source.minDistance = 2f;
                        source.maxDistance = 28f;
                        source.dopplerLevel = 0f;
                        source.spread = 45f;
                        source.priority = 128;
                        sources.Add(source);
                    }
                }

                string[] songNames = {
                    "alex-morgan-latin-salsa-caliente-537472",
                    "vjgalaxy-vallenato-con-ritmo-o-aire-de-son-01-503262",
                    "vjgalaxy-vallenato-con-ritmo-o-aire-de-son-04-503259"
                };
                var music = Undo.AddComponent<ParrandaMusicPlayer>(root);
                var settings = new SerializedObject(music);
                var songsProperty = settings.FindProperty("songs");
                songsProperty.arraySize = songNames.Length;
                for (int i = 0; i < songNames.Length; i++)
                {
                    string path = CopyAsset(songNames[i], ".mp3");
                    var importer = (AudioImporter)AssetImporter.GetAtPath(path);
                    importer.forceToMono = true;
                    var sample = importer.defaultSampleSettings;
                    sample.loadType = AudioClipLoadType.Streaming;
                    importer.defaultSampleSettings = sample;
                    importer.SaveAndReimport();
                    songsProperty.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                }
                var speakersProperty = settings.FindProperty("speakers");
                speakersProperty.arraySize = sources.Count;
                for (int i = 0; i < sources.Count; i++)
                    speakersProperty.GetArrayElementAtIndex(i).objectReferenceValue = sources[i];
                settings.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.MarkSceneDirty(scene);
                AssetDatabase.SaveAssets();
                // Never silently save work that was already unsaved in the user's scene.
                if (!wasDirty && !EditorSceneManager.SaveScene(scene))
                    throw new IOException("No se pudo guardar Parranda MVP.");
                Undo.CollapseUndoOperations(undoGroup);
                success = true;
                string report = "Ambiente integrado en Parranda MVP. Dos NPCs nuevos, " + animated +
                    " NPCs existentes animados, " + sources.Count + " emisores 3D y tres canciones sincronizadas. " +
                    (wasDirty ? "La escena tenia cambios sin guardar: guardar con Ctrl+S." : "Escena guardada.");
                Directory.CreateDirectory("Temp");
                File.WriteAllText("Temp/ParrandaAmbienceSetup.txt", report);
                Debug.Log(report, root);
            }
            catch
            {
                Undo.RevertAllDownToGroup(undoGroup);
                throw;
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
        finally
        {
            if (openedHere && success && scene.IsValid() && scene.isLoaded)
                EditorSceneManager.CloseScene(scene, true);
            busy = false;
        }
    }

    private static void ApplySeatedAssignments(Scene scene)
    {
        bool wasDirty = scene.isDirty;
        bool changed = false;
        foreach (var actor in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)))
        {
            string controllerName = actor.name == "NPC_BARRIO 3" ? "SentadoIdle" :
                actor.name == "NPC_AMBIENTE_Ch21_Sentado" ? "Sentado" : null;
            if (controllerName == null) continue;
            var animator = actor.GetComponentInChildren<Animator>(true);
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(Output + "/" + controllerName + ".controller");
            if (animator == null || controller == null)
                throw new InvalidOperationException("Falta el Animator o el controlador de " + actor.name);
            if (animator.runtimeAnimatorController == controller && !animator.applyRootMotion) continue;
            Undo.RecordObject(animator, "Asignar animacion sentada");
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            PrefabUtility.RecordPrefabInstancePropertyModifications(animator);
            changed = true;
        }
        if (!changed) return;
        EditorSceneManager.MarkSceneDirty(scene);
        if (!wasDirty) EditorSceneManager.SaveScene(scene);
        Debug.Log("Parranda: NPC_BARRIO 3 usa solo Sitting Idle; Ch21 usa solo Sitting. " +
            (wasDirty ? "Guardar la escena con Ctrl+S." : "Escena guardada."));
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder("Assets/Ambiente", "ParrandaMVP");
    }

    private static string CopyAsset(string name, string extension)
    {
        string destination = Output + "/" + name + extension;
        if (!File.Exists(destination) && !AssetDatabase.CopyAsset("Assets/Ambiente/" + name + extension, destination))
            throw new IOException("No se pudo preparar " + name);
        AssetDatabase.ImportAsset(destination, ImportAssetOptions.ForceSynchronousImport);
        return destination;
    }

    private static string PrepareModel(string name, bool importAnimation)
    {
        string path = CopyAsset(name, ".fbx");
        var importer = (ModelImporter)AssetImporter.GetAtPath(path);
        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.importAnimation = importAnimation;
        importer.SaveAndReimport();
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        var animator = model.GetComponent<Animator>();
        if (animator == null || animator.avatar == null || !animator.avatar.isValid || !animator.avatar.isHuman)
            throw new InvalidOperationException("No se pudo crear el Avatar humanoide de " + name);
        return path;
    }

    private static AnimationClip PrepareClip(string name, bool loop)
    {
        string path = PrepareModel(name, true);
        var importer = (ModelImporter)AssetImporter.GetAtPath(path);
        var clips = importer.defaultClipAnimations;
        foreach (var clip in clips)
        {
            clip.loopTime = loop;
            clip.loopPose = loop;
            clip.lockRootRotation = true;
            clip.lockRootHeightY = true;
            clip.lockRootPositionXZ = true;
            clip.keepOriginalOrientation = true;
            clip.keepOriginalPositionY = true;
            clip.keepOriginalPositionXZ = true;
        }
        importer.clipAnimations = clips;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
            .First(c => !c.name.StartsWith("__preview__"));
    }

    private static AnimatorController Controller(string name, AnimationClip first, AnimationClip idle = null)
    {
        string path = Output + "/" + name + ".controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (controller != null) return controller;
        controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        var machine = controller.layers[0].stateMachine;
        var entry = machine.AddState(first.name);
        entry.motion = first;
        machine.defaultState = entry;
        if (idle != null)
        {
            var rest = machine.AddState(idle.name);
            rest.motion = idle;
            var transition = entry.AddTransition(rest);
            transition.hasExitTime = true;
            transition.exitTime = 1f;
            transition.hasFixedDuration = true;
            transition.duration = 0.2f;
        }
        return controller;
    }

    private static GameObject AddNpc(string path, string name, Transform parent, Vector3 position,
        Quaternion rotation, RuntimeAnimatorController controller)
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        var actor = (GameObject)PrefabUtility.InstantiatePrefab(model, parent);
        Undo.RegisterCreatedObjectUndo(actor, "Agregar NPC");
        actor.name = name;
        actor.transform.SetPositionAndRotation(position, rotation);
        var animator = actor.GetComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        PrefabUtility.RecordPrefabInstancePropertyModifications(animator);
        PrefabUtility.RecordPrefabInstancePropertyModifications(actor);
        PrefabUtility.RecordPrefabInstancePropertyModifications(actor.transform);
        // Resolve the existing URP materials instead of displaying imported FBX shaders.
        foreach (var renderer in actor.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            var materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] == null) continue;
                string materialName = materials[i].name.Replace(" (Instance)", "");
                var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Ambiente/Materials/" + materialName + ".mat");
                if (material == null)
                    material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Ambiente/Materials/" + materialName + "_Diffuse.mat");
                if (material == null)
                {
                    // FBX can suffix duplicated material names with 1 or _ncl1_1.
                    string[] candidates = { "Remy_Body_Diffuse", "Remy_Bottom_Diffuse", "Remy_Hair_Diffuse",
                        "Remy_Shoes_Diffuse", "Remy_Top_Diffuse", "Ch21_1001_Diffuse", "Ch21_1002_Diffuse" };
                    string match = candidates.FirstOrDefault(candidate => materialName.StartsWith(candidate, StringComparison.Ordinal));
                    if (match != null)
                        material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Ambiente/Materials/" + match + ".mat");
                }
                if (material != null) materials[i] = material;
            }
            renderer.sharedMaterials = materials;
            PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
        }
        return actor;
    }

    private static Renderer FindFreeChair(GameObject[] roots)
    {
        var actors = roots.SelectMany(r => r.GetComponentsInChildren<Animator>(true))
            .Select(a => a.transform.position).ToArray();
        return roots.SelectMany(r => r.GetComponentsInChildren<MeshRenderer>())
            .Where(r => r.sharedMaterials.Any(m => m != null && m.name.Contains("Kursi")) &&
                        r.bounds.size.y > 0.5f && r.bounds.size.y < 1.5f &&
                        actors.All(p => Vector3.Distance(p, r.bounds.center) > 1f))
            .OrderBy(r => Vector3.Distance(r.bounds.center, new Vector3(2, 0, -3)))
            .FirstOrDefault();
    }

    private static void PlaceOnChair(GameObject actor, Renderer chair, AnimationClip idle, string modelPath)
    {
        Bounds bounds = chair.bounds;
        // The upper part of a plastic chair is the backrest. Infer the actual facing
        // direction from its mesh, independent of the imported FBX axes.
        var mesh = chair.GetComponent<MeshFilter>().sharedMesh;
        Vector3 back = Vector3.zero;
        int count = 0;
        // Mesh data from imported assets can be read by the editor even with Read/Write off.
        foreach (var vertex in mesh.vertices)
        {
            Vector3 world = chair.transform.TransformPoint(vertex);
            if (world.y < bounds.min.y + bounds.size.y * 0.75f) continue;
            back += world;
            count++;
        }
        Vector3 forward = count > 0 ? bounds.center - back / count : chair.transform.forward;
        forward.y = 0;
        if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
        Quaternion rotation = Quaternion.LookRotation(forward.normalized);
        Vector3 seat = new Vector3(bounds.center.x, bounds.min.y + bounds.size.y * 0.48f + 0.09f, bounds.center.z);

        // Sample a temporary model only to calculate the hip offset for placement.
        // No animation is run on or baked into the scene's actual skeleton.
        var previewScene = EditorSceneManager.NewPreviewScene();
        Vector3 hipOffset;
        try
        {
            var preview = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(modelPath), previewScene);
            preview.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            idle.SampleAnimation(preview, 0f);
            hipOffset = preview.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Hips).position - preview.transform.position;
        }
        finally { EditorSceneManager.ClosePreviewScene(previewScene); }
        actor.transform.SetPositionAndRotation(seat - rotation * hipOffset, rotation);
        PrefabUtility.RecordPrefabInstancePropertyModifications(actor.transform);
    }
}
