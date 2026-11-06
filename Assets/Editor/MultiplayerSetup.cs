using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MultiplayerSetup
{
    private const string LobbyScenePath = "Assets/Scenes/Lobby.unity";
    private const string PlayerPrefabPath = "Assets/Prefabs/Player.prefab";
    private const string PlayerMaterialPath = "Assets/Prefabs/Player/RED.mat";
    private const string DefaultPrefabsPath = "Assets/DefaultNetworkPrefabs.asset";
    private const string PromptKey = "FallRats.MultiplayerSetupPrompted";

    [InitializeOnLoadMethod]
    private static void PromptSetupOnLoad()
    {
        EditorApplication.delayCall += () =>
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (EditorPrefs.GetBool(PromptKey, false))
            {
                return;
            }

            EditorPrefs.SetBool(PromptKey, true);

            bool configure = EditorUtility.DisplayDialog(
                "Fall Rats - Multiplayer",
                "¿Configuro el multiplayer ahora?\n\n" +
                "Se creará Assets/Prefabs/Player.prefab, se añadirá el NetworkManager a Lobby " +
                "y se conectará la cámara al jugador local.",
                "Sí, configurar",
                "Ahora no");

            if (configure)
            {
                Configure();
            }
        };
    }

    [MenuItem("Fall Rats/Configurar Multiplayer")]
    public static void Configure()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "Multiplayer",
                "Sal del Play Mode antes de ejecutar esto.",
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

        GameObject playerPrefab = GetOrCreatePlayerPrefab();
        if (playerPrefab == null)
        {
            return;
        }

        RemoveOldScenePlayer();

        NetworkManager networkManager = GetOrCreateNetworkManager(playerPrefab);
        SetupCamera();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = networkManager.gameObject;
        EditorGUIUtility.PingObject(networkManager.gameObject);

        Debug.Log("[MultiplayerSetup] Listo: NetworkManager, Player.prefab y CameraFollow configurados.");
    }

    private static GameObject GetOrCreatePlayerPrefab()
    {
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        if (existingPrefab != null)
        {
            return existingPrefab;
        }

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        GameObject oldPlayer = GameObject.Find("Capsule");
        PlayerController oldController = oldPlayer != null
            ? oldPlayer.GetComponent<PlayerController>()
            : null;

        Material material = AssetDatabase.LoadAssetAtPath<Material>(PlayerMaterialPath);
        if (oldPlayer != null)
        {
            MeshRenderer oldRenderer = oldPlayer.GetComponent<MeshRenderer>();
            if (oldRenderer != null && oldRenderer.sharedMaterial != null)
            {
                material = oldRenderer.sharedMaterial;
            }
        }

        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0f, 1.5f, 0f);
        player.transform.rotation = Quaternion.identity;
        player.transform.localScale = Vector3.one;

        MeshRenderer renderer = player.GetComponent<MeshRenderer>();
        if (renderer != null && material != null)
        {
            renderer.sharedMaterial = material;
        }

        Rigidbody rigidbody = player.AddComponent<Rigidbody>();
        rigidbody.mass = 1f;
        rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        player.AddComponent<NetworkObject>();

        NetworkTransform networkTransform = player.AddComponent<NetworkTransform>();
        networkTransform.AuthorityMode = NetworkTransform.AuthorityModes.Server;

        PlayerController controller = player.AddComponent<PlayerController>();
        if (oldController != null)
        {
            controller.speed = oldController.speed;
            controller.jumpForce = oldController.jumpForce;
        }

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(player, PlayerPrefabPath);
        Object.DestroyImmediate(player);
        AssetDatabase.ImportAsset(PlayerPrefabPath, ImportAssetOptions.ForceUpdate);

        return prefab;
    }

    private static void RemoveOldScenePlayer()
    {
        GameObject oldPlayer = GameObject.Find("Capsule");
        if (oldPlayer == null)
        {
            return;
        }

        Object.DestroyImmediate(oldPlayer);
        Debug.Log("[MultiplayerSetup] El Capsule de la escena fue eliminado; ahora se spawnea Player.prefab por cliente.");
    }

    private static NetworkManager GetOrCreateNetworkManager(GameObject playerPrefab)
    {
        NetworkManager networkManager = Object.FindFirstObjectByType<NetworkManager>();

        if (networkManager == null)
        {
            GameObject networkManagerObject = new GameObject("NetworkManager");
            networkManager = networkManagerObject.AddComponent<NetworkManager>();
        }

        if (networkManager.NetworkConfig == null)
        {
            networkManager.NetworkConfig = new NetworkConfig();
        }

        UnityTransport transport = networkManager.GetComponent<UnityTransport>();
        if (transport == null)
        {
            transport = networkManager.gameObject.AddComponent<UnityTransport>();
        }

        networkManager.NetworkConfig.NetworkTransport = transport;
        networkManager.NetworkConfig.PlayerPrefab = playerPrefab;
        networkManager.NetworkConfig.EnableSceneManagement = true;
        networkManager.NetworkConfig.TickRate = 30;
        networkManager.RunInBackground = true;

        transport.SetConnectionData("127.0.0.1", 7777);

        NetworkPrefabsList defaultPrefabs = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(DefaultPrefabsPath);
        if (defaultPrefabs == null)
        {
            defaultPrefabs = ScriptableObject.CreateInstance<NetworkPrefabsList>();
            AssetDatabase.CreateAsset(defaultPrefabs, DefaultPrefabsPath);
        }

        if (!defaultPrefabs.Contains(playerPrefab))
        {
            defaultPrefabs.Add(new NetworkPrefab { Prefab = playerPrefab });
            EditorUtility.SetDirty(defaultPrefabs);
        }

        if (networkManager.NetworkConfig.Prefabs == null)
        {
            networkManager.NetworkConfig.Prefabs = new NetworkPrefabs();
        }

        if (!networkManager.NetworkConfig.Prefabs.NetworkPrefabsLists.Contains(defaultPrefabs))
        {
            networkManager.NetworkConfig.Prefabs.NetworkPrefabsLists.Add(defaultPrefabs);
        }

        EditorUtility.SetDirty(networkManager);
        return networkManager;
    }

    private static void SetupCamera()
    {
        Camera camera = Camera.main;

        if (camera == null)
        {
            Debug.LogWarning("[MultiplayerSetup] No encontré una cámara con el tag MainCamera.");
            return;
        }

        if (camera.GetComponent<CameraFollow>() == null)
        {
            camera.gameObject.AddComponent<CameraFollow>();
        }

        camera.transform.position = new Vector3(0f, 5f, -8f);
        EditorUtility.SetDirty(camera.gameObject);
    }
}
