using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ForestLevel : NetworkBehaviour
{
    private NetworkManager networkManager;
    [SerializeField]
    private NetworkObject playerPrefab;
    [SerializeField]
    private NetworkObject entityPrefab;

    private CinemachineVirtualCamera cinemachineVirtualCamera;

    private void OnEnable()
    {
        networkManager = NetworkManager.Singleton;

        networkManager.SceneManager.OnLoadComplete += HandleOnSceneLoaded;

        cinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    private void HandleOnSceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (IsServer)
        {
            foreach (ulong playerId in NetworkManager.ConnectedClientsIds)
            {
                SpawnPlayer(playerId);
                SpawnEntity();
            }
        }

        networkManager.SceneManager.OnLoadComplete -= HandleOnSceneLoaded;
    }

    private void SpawnPlayer(ulong clientId)
    {
        NetworkObject player = Instantiate(playerPrefab, playerPrefab.transform.position, Quaternion.identity);
        Transform playerCameraRoot = player.transform.GetChild(0);
        cinemachineVirtualCamera.Follow = playerCameraRoot;

        player.SpawnAsPlayerObject(clientId, destroyWithScene: true);
    }

    private void SpawnEntity()
    {
        if (entityPrefab == null) return;

        NetworkObject entity = Instantiate(entityPrefab, entityPrefab.transform.position, Quaternion.identity);

        entity.Spawn(destroyWithScene: true);
    }
}
