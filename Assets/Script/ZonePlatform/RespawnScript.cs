using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RespawnScript : NetworkBehaviour
{
    public static bool canDrop = false;

    private readonly Dictionary<GameObject, Transform> assigned = new Dictionary<GameObject, Transform>();
    private readonly List<Transform> freeSpawns = new List<Transform>();
    private bool countdownStarted = false;

    public override void OnNetworkSpawn()
    {
        canDrop = false;
        freeSpawns.Clear();
        assigned.Clear();

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Respawn"))
            freeSpawns.Add(go.transform);
    }

    private void Update()
    {
        if (!IsServer || canDrop) return;

        // 1. Asigna un spawn libre al azar a cada jugador y lo mantiene flotando
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (!assigned.ContainsKey(p) && freeSpawns.Count > 0)
            {
                int r = Random.Range(0, freeSpawns.Count);
                assigned[p] = freeSpawns[r];
                freeSpawns.RemoveAt(r);
            }

            if (assigned.TryGetValue(p, out Transform spot))
                HoldPlayerClientRpc(p.GetComponent<NetworkObject>().NetworkObjectId, spot.position);
        }

        // 2. Sin número fijo: en cuanto todos los conectados estén en su sitio, empieza la cuenta atrás
        int connectedCount = NetworkManager.Singleton.ConnectedClientsList.Count;
        if (!countdownStarted && connectedCount > 0 && assigned.Count >= connectedCount)
        {
            countdownStarted = true;
            StartCoroutine(ReleaseAfterDelay());
        }
    }

    private IEnumerator ReleaseAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        ReleaseClientRpc();
    }

    [ClientRpc]
    private void HoldPlayerClientRpc(ulong netId, Vector3 pos)
    {
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (p.TryGetComponent<NetworkObject>(out var no) && no.NetworkObjectId == netId)
            {
                p.transform.position = pos;
                if (p.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;
                break;
            }
        }
    }

    [ClientRpc]
    private void ReleaseClientRpc()
    {
        canDrop = true;
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
            if (p.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = false;
    }
}