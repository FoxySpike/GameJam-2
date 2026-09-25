using UnityEditor;
using UnityEngine;

// One-shot refinement: retain the imported rest pose and swing only the thighs.
[InitializeOnLoad]
public static class SoftenPolloWalkLegs
{
    static SoftenPolloWalkLegs()
    {
        EditorApplication.delayCall += Apply;
    }

    private static void Apply()
    {
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animaciones/Pollo/Pollo_Caminar.anim");
        if (clip == null) return;

        // Five degrees either side of the exact imported local rotation.
        SetSwing(clip, "metarig/spine/thigh.L", 318.95953f, 5f);
        SetSwing(clip, "metarig/spine/thigh.R", 319.05637f, -5f);

        // Keep the lower legs at their original pose: only the hip bones move.
        Remove(clip, "metarig/spine/thigh.L/shin.L");
        Remove(clip, "metarig/spine/thigh.R/shin.R");

        EditorUtility.SetDirty(clip);
        AssetDatabase.SaveAssets();
        Debug.Log("Pollo_Caminar refinada con pasos cortos desde la pose de reposo.");
    }

    private static void SetSwing(AnimationClip clip, string path, float rest, float amount)
    {
        var curve = new AnimationCurve(
            new Keyframe(0f, rest + amount),
            new Keyframe(.25f, rest),
            new Keyframe(.5f, rest - amount),
            new Keyframe(.75f, rest),
            new Keyframe(1f, rest + amount));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.x"), curve);
    }

    private static void Remove(AnimationClip clip, string path)
    {
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.x"), null);
    }
}
