using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ZonePoint : NetworkBehaviour
{
    public NextMinigameLoader nextMinigameLoader;
    public float delayBeforeNextScene = 3f;

    private List<ulong> aliveClients = new List<ulong>();
    private bool roundFinished = false;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        roundFinished = false;
        aliveClients.Clear();

        // Guardamos todos los jugadores conectados al empezar la ronda
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null && client.PlayerObject.CompareTag("Player"))
            {
                aliveClients.Add(client.ClientId);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || roundFinished) return;
        if (!other.CompareTag("Player")) return;

        // Buscamos el jugador aunque el Collider esté en un objeto hijo
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;

        ulong clientId = player.OwnerClientId;

        // Si ya no está en la lista de vivos, significa que ya cayó antes
        if (!aliveClients.Contains(clientId)) return;

        // Lo quitamos de vivos y calculamos su puesto (de atrás hacia adelante)
        aliveClients.Remove(clientId);
        int position = aliveClients.Count + 1;
        int pts = GetPointsForPosition(position);

        player.AddPoints(pts);
        Debug.Log($"Jugador {clientId} cayó -> Puesto {position} (+{pts} pts)");

        // Si queda 1 solo jugador vivo, gana el 1.er puesto y acaba la ronda
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
                Debug.Log($"¡Jugador {winnerId} queda 1.er puesto (+8 pts)!");
            }
        }
    }

    private int GetPointsForPosition(int position)
    {
        if (position == 1) return 8;
        if (position == 2) return 6;
        if (position == 3) return 4;
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