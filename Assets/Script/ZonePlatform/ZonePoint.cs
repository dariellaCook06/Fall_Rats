using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ZonePoint : NetworkBehaviour
{
    public NextMinigameLoader nextMinigameLoader;
    public TeleportLost teleportLost;
    public float delayBeforeNextScene = 3f;

    private List<ulong> aliveClients = new List<ulong>();
    private bool roundFinished = false;

    private void Awake()
    {
        if (teleportLost == null)
        {
            teleportLost = FindFirstObjectByType<TeleportLost>();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        roundFinished = false;
        aliveClients.Clear();

        if (!IsServer || NetworkManager.Singleton == null)
        {
            return;
        }

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null && client.PlayerObject.CompareTag("Player"))
            {
                aliveClients.Add(client.ClientId);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        StopAllCoroutines();
        aliveClients.Clear();
        roundFinished = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || roundFinished)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null)
        {
            return;
        }

        ulong clientId = player.OwnerClientId;

        if (!aliveClients.Contains(clientId))
        {
            return;
        }

        aliveClients.Remove(clientId);
        int position = aliveClients.Count + 1;
        int pts = GetPointsForPosition(position);

        player.AddPoints(pts);
        Debug.Log($"Jugador {clientId} cayó - Puesto {position} (+{pts} pts)");

        if (teleportLost != null)
        {
            teleportLost.SendPlayerToLost(player);
        }

        if (aliveClients.Count == 1)
        {
            GiveFirstPlace(aliveClients[0]);
            StartCoroutine(NextRoundRoutine());
        }
        else if (aliveClients.Count == 0)
        {
            StartCoroutine(NextRoundRoutine());
        }
    }

    private void GiveFirstPlace(ulong winnerId)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(winnerId, out var client) &&
            client.PlayerObject != null)
        {
            PlayerController winner = client.PlayerObject.GetComponentInParent<PlayerController>();
            if (winner != null)
            {
                winner.AddPoints(8);
                Debug.Log($"Jugador {winnerId} queda 1er puesto (+8 pts)");
            }
        }
    }

    private int GetPointsForPosition(int position)
    {
        if (position == 1)
        {
            return 8;
        }

        if (position == 2)
        {
            return 6;
        }

        if (position == 3)
        {
            return 4;
        }

        return 2;
    }

    private IEnumerator NextRoundRoutine()
    {
        roundFinished = true;
        yield return new WaitForSeconds(delayBeforeNextScene);

        if (nextMinigameLoader != null)
        {
            nextMinigameLoader.LoadRandomMinigame();
        }
    }
}