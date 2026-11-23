using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Crea en la escena Lobby una plataforma de red lista para probar:
/// cubo + collider sólido + trigger encima + NetworkObject + PlatformDown.
/// </summary>
public static class PlatformDownSetup
{
    private const string LobbyScenePath = "Assets/Scenes/Lobby.unity";
    private const string PlatformName = "PlatformDown";
    private const string MaterialFolder = "Assets/Materials";
    private const string MaterialPath = "Assets/Materials/PlatformDown.mat";

    [MenuItem("Fall Rats/Plataforma que desaparece (PlatformDown)")]
    public static void CreatePlatform()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "PlatformDown",
                "Sal del Play Mode antes de crear la plataforma.",
                "OK");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();

        if (scene.path != LobbyScenePath)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            scene = EditorSceneManager.OpenScene(LobbyScenePath, OpenSceneMode.Single);
        }

        GameObject existing = GameObject.Find(PlatformName);
        if (existing != null)
        {
            Selection.activeGameObject = existing;
            EditorGUIUtility.PingObject(existing);
            Debug.Log("[PlatformDown] Ya existe una plataforma en la escena.");
            return;
        }

        // 1. Cubo base
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = PlatformName;
        platform.transform.position = new Vector3(0f, 0.5f, 4f);
        platform.transform.localScale = new Vector3(4f, 1f, 4f);

        MeshRenderer renderer = platform.GetComponent<MeshRenderer>();
        Material material = GetOrCreateMaterial();
        if (renderer != null && material != null)
        {
            renderer.sharedMaterial = material;
        }

        // 2. Trigger encima del cubo para detectar jugadores
        BoxCollider trigger = platform.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0f, 0.75f, 0f);
        trigger.size = new Vector3(1f, 0.5f, 1f);

        // 3. Componentes de red
        NetworkObject networkObject = platform.AddComponent<NetworkObject>();
        platform.AddComponent<PlatformDown>();

        EditorUtility.SetDirty(networkObject);
        EditorUtility.SetDirty(platform);

        // 4. Guardar la escena
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        // 5. Comprobar que NGO generó el hash de objeto de escena
        SerializedObject serialized = new SerializedObject(networkObject);
        SerializedProperty hash = serialized.FindProperty("GlobalObjectIdHash");
        if (hash != null && hash.uintValue == 0)
        {
            Debug.LogWarning(
                "[PlatformDown] GlobalObjectIdHash = 0. Guarda o reimporta la escena para que NGO lo genere.");
        }

        Selection.activeGameObject = platform;
        EditorGUIUtility.PingObject(platform);

        Debug.Log(
            "[PlatformDown] Creada en Lobby en (0, 0.5, 4). " +
            "Inicia Host y sube encima: verás la cuenta atrás de 4 segundos y desaparecerá.");
    }

    private static Material GetOrCreateMaterial()
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material != null)
        {
            return material;
        }

        if (!AssetDatabase.IsValidFolder(MaterialFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        material = new Material(shader)
        {
            name = "PlatformDown"
        };

        Color color = new Color(1f, 0.55f, 0.15f);
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        AssetDatabase.CreateAsset(material, MaterialPath);
        return material;
    }
}
