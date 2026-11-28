using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
public class CountTeam : NetworkBehaviour
{
    [Header("Identificación del equipo")]
    public string teamName = "Equipo A";

    // Contador sincronizado de huevos dentro de la canasta (1 huevo = 1 punto)
    public NetworkVariable<int> eggsInBasket = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private readonly List<ulong> eggsInsideIds = new List<ulong>();

    private void Awake()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            box.isTrigger = true;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        eggsInBasket.OnValueChanged += OnBasketCountChanged;

        if (IsServer)
        {
            eggsInsideIds.Clear();
            eggsInBasket.Value = 0;
        }

        Debug.Log($"[{teamName}] Puntuación inicial: {eggsInBasket.Value}");
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        eggsInBasket.OnValueChanged -= OnBasketCountChanged;

        if (IsServer)
        {
            eggsInsideIds.Clear();
        }
    }

    private void OnBasketCountChanged(int previousValue, int newValue)
    {
        Debug.Log($"[{teamName}] Huevos en canasta: {newValue} (Puntos: {newValue})");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
        {
            return;
        }

        NetworkObject eggNetObj = other.GetComponentInParent<NetworkObject>();
        if (eggNetObj == null || !eggNetObj.CompareTag("Egg"))
        {
            return;
        }

        if (!eggsInsideIds.Contains(eggNetObj.NetworkObjectId))
        {
            eggsInsideIds.Add(eggNetObj.NetworkObjectId);
            eggsInBasket.Value = eggsInsideIds.Count;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer)
        {
            return;
        }

        NetworkObject eggNetObj = other.GetComponentInParent<NetworkObject>();
        if (eggNetObj == null || !eggNetObj.CompareTag("Egg"))
        {
            return;
        }

        if (eggsInsideIds.Remove(eggNetObj.NetworkObjectId))
        {
            eggsInBasket.Value = eggsInsideIds.Count;
        }
    }
}