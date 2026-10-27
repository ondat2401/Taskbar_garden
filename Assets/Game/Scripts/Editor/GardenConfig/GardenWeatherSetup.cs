using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace IdleGarden.Editor
{
    public static class GardenWeatherSetup
    {
        private const string SettingsPath = "Assets/Game/_Configs/Garden/Weather.asset";

        [MenuItem("Tools/Idle Garden/Setup Weather Sync F14")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Exit Play Mode before setting up weather sync.");
            var controller = Object.FindFirstObjectByType<GardenController>();
            if (!controller || !controller.hud || !controller.hud.moneyLabel)
                throw new System.InvalidOperationException("Open GardenPrototype with its HUD before setting up weather sync.");

            var settings = GetOrCreateSettings();
            var bar = controller.hud.moneyLabel.transform.parent as RectTransform;
            if (!bar) throw new System.InvalidOperationException("Garden Bar RectTransform was not found.");

            var labelTransform = bar.Find("Weather");
            Text label;
            if (!labelTransform)
            {
                var labelObject = new GameObject("Weather", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                Undo.RegisterCreatedObjectUndo(labelObject, "Create weather status");
                labelObject.transform.SetParent(bar, false);
                label = labelObject.GetComponent<Text>();
            }
            else
            {
                label = labelTransform.GetComponent<Text>();
                if (!label) label = Undo.AddComponent<Text>(labelTransform.gameObject);
            }

            Undo.RecordObject(label, "Style weather status");
            label.font = controller.hud.moneyLabel.font;
            label.fontSize = 10;
            label.color = GardenUiPalette.TextMuted;
            label.alignment = TextAnchor.MiddleRight;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.raycastTarget = false;
            label.text = "WEATHER loading · Open-Meteo";
            var labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(1, 0);
            labelRect.anchorMax = Vector2.one;
            labelRect.pivot = new Vector2(1, .5f);
            labelRect.offsetMin = new Vector2(-412, 0);
            labelRect.offsetMax = new Vector2(-194, 0);

            var moneyRect = controller.hud.moneyLabel.rectTransform;
            Undo.RecordObject(moneyRect, "Make room for weather status");
            moneyRect.offsetMax = new Vector2(-420, moneyRect.offsetMax.y);

            var sync = controller.GetComponent<GardenWeatherSync>();
            if (!sync) sync = Undo.AddComponent<GardenWeatherSync>(controller.gameObject);
            Undo.RecordObject(sync, "Assign weather settings and label");
            var serialized = new SerializedObject(sync);
            serialized.FindProperty("settings").objectReferenceValue = settings;
            serialized.FindProperty("statusLabel").objectReferenceValue = label;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(label);
            EditorUtility.SetDirty(sync);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(controller.gameObject.scene))
                throw new System.InvalidOperationException("Could not save the garden scene after weather setup.");
            Debug.Log("F14 weather sync configured. Default location: Ho Chi Minh City; change Weather.asset to use another place.");
        }

        private static GardenWeatherSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<GardenWeatherSettings>(SettingsPath);
            if (settings) return settings;
            Directory.CreateDirectory("Assets/Game/_Configs/Garden");
            settings = ScriptableObject.CreateInstance<GardenWeatherSettings>();
            AssetDatabase.CreateAsset(settings, SettingsPath);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssetIfDirty(settings);
            return settings;
        }
    }
}
