using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EggSpawner : NetworkBehaviour
{
    [Header("Prefab y cantidad")]
    public NetworkObject eggPrefab;
    public int eggCount = 25;
    public float spawnInterval = 1f;

    [Header("Área de aparición (centro y tamaño)")]
    public Vector3 spawnCenter = Vector3.zero;
    public Vector3 spawnAreaSize = new Vector3(14f, 0f, 10f);

    private readonly List<NetworkObject> spawnedEggs = new List<NetworkObject>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer)
        {
            return;
        }

        StartCoroutine(SpawnEggsRoutine());
    }

    public override void OnNetworkDespawn()
    {
        StopAllCoroutines();

        base.OnNetworkDespawn();

        if (!IsServer)
        {
            return;
        }

        for (int i = 0; i < spawnedEggs.Count; i++)
        {
            NetworkObject egg = spawnedEggs[i];
            if (egg != null && egg.IsSpawned)
            {
                egg.Despawn(true);
            }
        }

        spawnedEggs.Clear();
    }

    private IEnumerator SpawnEggsRoutine()
    {
        if (eggPrefab == null)
        {
            yield break;
        }

        for (int i = 0; i < eggCount; i++)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (!IsServer || eggPrefab == null)
            {
                yield break;
            }

            float randomX = Random.Range(-spawnAreaSize.x * 0.5f, spawnAreaSize.x * 0.5f);
            float randomY = Random.Range(-spawnAreaSize.y * 0.5f, spawnAreaSize.y * 0.5f);
            float randomZ = Random.Range(-spawnAreaSize.z * 0.5f, spawnAreaSize.z * 0.5f);

            Vector3 spawnPos = transform.position + spawnCenter + new Vector3(randomX, randomY, randomZ);

            NetworkObject eggInstance = Instantiate(eggPrefab, spawnPos, Quaternion.identity);
            eggInstance.Spawn(true);
            spawnedEggs.Add(eggInstance);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + spawnCenter, spawnAreaSize);
    }
}
