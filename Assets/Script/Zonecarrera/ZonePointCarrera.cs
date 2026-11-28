using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ZonePointCarrera : NetworkBehaviour
{
    public NextMinigameLoader nextMinigameLoader;
    public Transform respawnFinish;
    public float delayBeforeNextScene = 3f;

    private List<ulong> finishedClients = new List<ulong>();
    private int totalPlayersAtStart = 0;
    private bool roundFinished = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        roundFinished = false;
        finishedClients.Clear();
        totalPlayersAtStart = 0;

        if (!IsServer || NetworkManager.Singleton == null)
        {
            return;
        }

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null && client.PlayerObject.CompareTag("Player"))
            {
                totalPlayersAtStart++;
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        StopAllCoroutines();
        finishedClients.Clear();
        totalPlayersAtStart = 0;
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

        if (finishedClients.Contains(clientId))
        {
            return;
        }

        finishedClients.Add(clientId);

        int position = finishedClients.Count;
        int pts = GetPointsForPosition(position);

        player.AddPoints(pts);
        Debug.Log($"Jugador {clientId} llegó a meta - Puesto {position} (+{pts} pts)");

        TeleportToFinish(player);

        if (totalPlayersAtStart > 0 && finishedClients.Count >= totalPlayersAtStart)
        {
            StartCoroutine(NextRoundRoutine());
        }
    }

    private void TeleportToFinish(PlayerController player)
    {
        if (!IsServer)
        {
            return;
        }

        if (player == null || respawnFinish == null)
        {
            return;
        }

        Vector3 targetPos = respawnFinish.position;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            rb.position = targetPos;
        }

        player.transform.position = targetPos;
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