using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DrunkenLensMotion))]
public sealed class DrunkenLensMotionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();
        var controller = ((DrunkenLensMotion)target).GetComponentInParent<DrunkenVision>();
        if (controller != null)
            EditorGUILayout.LabelField("Control del peso", controller.IsManualPreview ? "Manual" : "Estado de alcohol");
        EditorGUILayout.HelpBox("Durante Play puedes mover Weight directamente: el control pasa a manual. " +
            "Los ajustes del componente durante Play son temporales; guarda tus valores definitivos fuera de Play.", MessageType.Info);
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            var motion = (DrunkenLensMotion)target;
            if (GUILayout.Button("Previsualizar mareo (Weight = 1)")) motion.PreviewEffects();
            if (GUILayout.Button("Volver al estado de alcohol")) motion.ResumeAutomatic();
        }
    }
}
