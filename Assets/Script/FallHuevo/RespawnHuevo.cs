using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RespawnHuevo : NetworkBehaviour
{
    [Header("Tag de los puntos de aparición")]
    public string respawnTag = "RespawnHuevo";

    private List<Transform> spawnPoints = new List<Transform>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer)
        {
            return;
        }

        FindSpawnPoints();
        AssignTeamsAndTeleportPlayers();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        spawnPoints.Clear();
    }

    private void FindSpawnPoints()
    {
        spawnPoints.Clear();

        GameObject[] foundObjects = GameObject.FindGameObjectsWithTag(respawnTag);
        for (int i = 0; i < foundObjects.Length; i++)
        {
            if (foundObjects[i] != null)
            {
                spawnPoints.Add(foundObjects[i].transform);
            }
        }
    }

    private void AssignTeamsAndTeleportPlayers()
    {
        if (!IsServer || NetworkManager.Singleton == null || spawnPoints.Count == 0)
        {
            return;
        }

        List<PlayerController> players = new List<PlayerController>();

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null && client.PlayerObject.CompareTag("Player"))
            {
                PlayerController player = client.PlayerObject.GetComponentInParent<PlayerController>();
                if (player != null)
                {
                    players.Add(player);
                }
            }
        }

        int totalPlayers = players.Count;
        if (totalPlayers == 0)
        {
            return;
        }

        int team1Count = (totalPlayers + 1) / 2;

        int team1Index = 0;
        int team2Index = 0;

        for (int i = 0; i < players.Count; i++)
        {
            PlayerController player = players[i];
            int teamNumber = (i < team1Count) ? 1 : 2;

            Transform targetPoint = GetSpawnPointForTeam(teamNumber, ref team1Index, ref team2Index);
            if (targetPoint != null)
            {
                TeleportPlayer(player, targetPoint.position);
                Debug.Log($"Jugador {player.OwnerClientId} -> Equipo {teamNumber} en {targetPoint.name}");
            }
        }
    }

    private Transform GetSpawnPointForTeam(int teamNumber, ref int team1Index, ref int team2Index)
    {
        if (spawnPoints.Count == 0)
        {
            return null;
        }

        if (spawnPoints.Count == 1)
        {
            return spawnPoints[0];
        }

        int half = spawnPoints.Count / 2;

        if (teamNumber == 1)
        {
            int idx = team1Index % half;
            team1Index++;
            return spawnPoints[idx];
        }
        else
        {
            int secondHalfCount = spawnPoints.Count - half;
            int idx = half + (team2Index % secondHalfCount);
            team2Index++;
            return spawnPoints[idx];
        }
    }

    private void TeleportPlayer(PlayerController player, Vector3 targetPos)
    {
        if (!IsServer || player == null)
        {
            return;
        }

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
}