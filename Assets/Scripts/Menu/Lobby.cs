using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking.Types;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

struct LobbyPlayerData : INetworkSerializable, IEquatable<LobbyPlayerData>
{
    public ulong playerId;
    public FixedString64Bytes playerName;
    public bool isLobbyReady;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref isLobbyReady);
    }

    // IEquatable
    public bool Equals(LobbyPlayerData other)
    {
        return playerId == other.playerId && playerName.Equals(other.playerName) && isLobbyReady == other.isLobbyReady;
    }

    public override bool Equals(object obj)
    {
        return obj is LobbyPlayerData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(playerId, playerName, isLobbyReady);
    }
    //~IEquatable
}

public class Lobby : NetworkBehaviour
{

    [Header("Players")]
    [SerializeField] private TextMeshProUGUI[] playerNames;
    [Header("Buttons")]
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject readyButton;

    private NetworkManager networkManager;

    private NetworkList<LobbyPlayerData> lobbyPlayerDatas;

    private void OnEnable()
    {
        networkManager = NetworkManager.Singleton;

        lobbyPlayerDatas = new NetworkList<LobbyPlayerData>();

        networkManager.SceneManager.OnLoadComplete += HandleOnSceneLoaded;
    }

    private void HandleOnSceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        networkManager.ConnectionApprovalCallback = ConnectionApproval;

        networkManager.SceneManager.OnLoadComplete -= HandleOnSceneLoaded;
    }

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            ShowHostButton();
        }
        if (IsClient)
        {
            lobbyPlayerDatas.OnListChanged += HandleOnListChanged;
        }
        if (IsServer)
        {
            networkManager.OnClientConnectedCallback += HandleClientConnected;
            networkManager.OnClientDisconnectCallback += HandleClientDisconnect;

            //Add hostPlayerData to the network list
            foreach (NetworkClient clients in networkManager.ConnectedClientsList)
            {
                AddPlayerDataToNetworkList(clients.ClientId);
            }
        }

        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            lobbyPlayerDatas.OnListChanged -= HandleOnListChanged;
        }
        if (IsServer)
        {
            networkManager.OnClientConnectedCallback -= HandleClientConnected;
            networkManager.OnClientDisconnectCallback -= HandleClientDisconnect;
        }

        base.OnNetworkDespawn();
    }

    private void HandleClientConnected(ulong clientId)
    {
        AddPlayerDataToNetworkList(clientId);
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        RemovePlayerDataFromNetworkList(clientId);
        PlayerManager.RemovePlayerData(clientId);
    }

    private void HandleOnListChanged(NetworkListEvent<LobbyPlayerData> changeEvent)
    {
        UpdateLobbyInfoUI();
    }

    private void ShowHostButton()
    {
        startButton.SetActive(true);
        readyButton.SetActive(false);
    }

    private void AddPlayerDataToNetworkList(ulong clientId)
    {
        StructPlayerData? playerData = PlayerManager.GetPlayerData(clientId);

        if (playerData.HasValue)
        {
            lobbyPlayerDatas.Add(new LobbyPlayerData()
            {
                playerId = clientId,
                playerName = playerData.Value.playerName,
                isLobbyReady = playerData.Value.isLobbyReady
            });
        }
    }

    private void RemovePlayerDataFromNetworkList(ulong clientId)
    {
        for (int i = 0; i < lobbyPlayerDatas.Count; i++)
        {
            if (lobbyPlayerDatas[i].playerId == clientId)
            {
                lobbyPlayerDatas.RemoveAt(i);
                break;
            }
        }
    }

    private void UpdateLobbyInfoUI()
    {
        for (int i = 0; i < lobbyPlayerDatas.Count; i++)
        {
            playerNames[i].text = lobbyPlayerDatas[i].playerName.ToString() != ""? lobbyPlayerDatas[i].playerName.ToString() : "Unkown";
            playerNames[i].GetComponentInChildren<RawImage>().color = lobbyPlayerDatas[i].isLobbyReady ? Color.green : Color.red;
        }

        for (int i = 0; i < playerNames.Length; i++)
        {
            if ((i + 1) > lobbyPlayerDatas.Count)
            {
                playerNames[i].text = "-";
                playerNames[i].GetComponentInChildren<RawImage>().color = Color.grey;
            }
        }
    }

    private void ConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        string playerPayload = Encoding.ASCII.GetString(request.Payload);

        StructPlayerData playerData = JsonUtility.FromJson<StructPlayerData>(playerPayload);

        PlayerManager.AddPlayerData(request.ClientNetworkId, playerData);

        if (networkManager.ConnectedClientsIds.Count < 4)
        {
            response.Approved = true;
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void RequestToggleReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        PlayerManager.ToggleLobbyPlayerState(serverRpcParams.Receive.SenderClientId);

        for (int i = 0; i < lobbyPlayerDatas.Count; i++)
        {
            if (lobbyPlayerDatas[i].playerId == serverRpcParams.Receive.SenderClientId)
            {
                lobbyPlayerDatas[i] = new LobbyPlayerData()
                {
                    playerId = lobbyPlayerDatas[i].playerId,
                    playerName = lobbyPlayerDatas[i].playerName,
                    isLobbyReady = !lobbyPlayerDatas[i].isLobbyReady
                };
            }
        }
    }

    private bool IsAllGuestsReady()
    {
        foreach (LobbyPlayerData lobbyPlayer in lobbyPlayerDatas)
        {
            if (!lobbyPlayer.isLobbyReady)
                return false;
        }

        return true;
    }


    #region LobbyPanel
    public void ToggleReady()
    {
        RequestToggleReadyServerRpc();
    }

    public void StartGame()
    {
        RequestToggleReadyServerRpc();
        if (IsAllGuestsReady() && IsHost)
        {
            networkManager.SceneManager.LoadScene("Main", LoadSceneMode.Single);
        }
    }

    public void Leave()
    {
        if (IsHost)
        {
            PlayerManager.ClearPlayerData();
        }

        networkManager.Shutdown();
        SceneManager.LoadScene("OfflineMenuUI");
    }
    #endregion
}
