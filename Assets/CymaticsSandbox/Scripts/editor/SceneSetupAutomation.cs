using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public static class Automation
{
    [MenuItem("Tools/Automate Scene Setup")]
    public static void AutomateSceneSetup()
    {
        try
        {
            Debug.Log("Starting scene setup automation...");

            // Create TrayBase prefab
            GameObject trayBase = new GameObject("TrayBase");
            trayBase.AddComponent<MeshFilter>();
            MeshCollider meshCollider = trayBase.AddComponent<MeshCollider>();
            meshCollider.convex = false;
            MeshRenderer meshRenderer = trayBase.AddComponent<MeshRenderer>();

            Material trayMat = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(trayMat, "Assets/CymaticsSandbox/Materials/TrayMat.mat");
            meshRenderer.material = trayMat;

            PrefabUtility.SaveAsPrefabAsset(trayBase, "Assets/CymaticsSandbox/Prefabs/TrayBase.prefab");
            GameObject.DestroyImmediate(trayBase);

            Debug.Log("TrayBase prefab created.");

            // Create XR Rig prefab
            GameObject xrOrigin = new GameObject("XR Origin");
            xrOrigin.AddComponent<UnityEngine.XR.Interaction.Toolkit.XRInteractionManager>();
            // Removed OpenXRSettings as it cannot be added as a component

            PrefabUtility.SaveAsPrefabAsset(xrOrigin, "Assets/CymaticsSandbox/Prefabs/VR_Rig.prefab");
            GameObject.DestroyImmediate(xrOrigin);

            Debug.Log("XR Rig prefab created.");

            // Create Canvas Sliders prefab
            GameObject canvas = new GameObject("Canvas");
            Canvas canvasComponent = canvas.AddComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.WorldSpace;
            RectTransform rectTransform = canvas.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(1.2f, 0.8f);
            rectTransform.position = new Vector3(0, 1.4f, 1.2f);

            string[] sliderNames = { "Depth", "Viscosity", "Damping", "WaveSpeed", "LightIntensity", "Flicker", "LightTemp", "LightAngle", "LightYaw", "ExciteFreq", "ExciteAmp" };
            foreach (string sliderName in sliderNames)
            {
                GameObject slider = new GameObject(sliderName);
                slider.transform.SetParent(canvas.transform);
                slider.AddComponent<Slider>();
            }

            PrefabUtility.SaveAsPrefabAsset(canvas, "Assets/CymaticsSandbox/UI/Canvas_Sliders.prefab");
            GameObject.DestroyImmediate(canvas);

            Debug.Log("Canvas Sliders prefab created.");

            // Build Scene
            GameObject vrRig = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CymaticsSandbox/Prefabs/VR_Rig.prefab")) as GameObject;
            GameObject trayBaseInstance = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CymaticsSandbox/Prefabs/TrayBase.prefab")) as GameObject;

            trayBaseInstance.AddComponent<WaveSolver>();
            trayBaseInstance.AddComponent<TraySwitcher>();
            trayBaseInstance.AddComponent<ExporterPNG>();
            trayBaseInstance.AddComponent<ExporterSTL>();

            GameObject overheadLight = new GameObject("OverheadLight");
            Light light = overheadLight.AddComponent<Light>();
            light.type = LightType.Spot;
            overheadLight.transform.position = new Vector3(0, 2, 0);
            overheadLight.transform.rotation = Quaternion.Euler(90, 0, 0);
            overheadLight.AddComponent<LightController>();

            GameObject canvasSliders = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CymaticsSandbox/UI/Canvas_Sliders.prefab")) as GameObject;
            UI_SliderLinker sliderLinker = canvasSliders.AddComponent<UI_SliderLinker>();
            sliderLinker.WaveSolver = trayBaseInstance.GetComponent<WaveSolver>();
            sliderLinker.TraySwitcher = trayBaseInstance.GetComponent<TraySwitcher>();
            sliderLinker.LightCtrl = overheadLight.GetComponent<LightController>();

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            Debug.Log("Scene setup automation complete!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error during scene setup automation: {ex.Message}");
        }
    }
}