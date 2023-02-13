using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject changeNamePanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject joinPanel;

    [Header("Inputs")]
    [SerializeField] private TMP_InputField renameInputField;
    [SerializeField] private TMP_InputField IPInputField;

    private NetworkManager networkManager;

    private void Start()
    {
        networkManager = NetworkManager.Singleton;
        networkManager.OnClientConnectedCallback += HandleOnClientConnected;
    }

    private void HandleOnClientConnected(ulong clientId)
    {
        if (networkManager.IsServer)
        {
            networkManager.SceneManager.LoadScene("OnlineLobbyUI", LoadSceneMode.Single);
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (networkManager.ConnectedClientsList.Count < 4)
        {
            response.Approved = true;
        }
        else
        {
            response.Approved = false;
        }
    }


    #region MenuPanel
    public void ChangeName()
    {
        changeNamePanel.SetActive(true);
        menuPanel.SetActive(false);
    }

    public void Host()
    {
        //TODO: Can be clicked twice, trowing an error "ApprovalCheck Should be called once"
        networkManager.ConnectionApprovalCallback = ApprovalCheck;

        AddHostPlayerData();

        networkManager.StartHost();
    }

    private void AddHostPlayerData()
    {
        //If we dont put 0 as hostId, after we joining as client the localDlientId becomes 1
        PlayerManager.AddPlayerData(0, new StructPlayerData { playerName = renameInputField.text });
    }

    public void Join()
    {
        joinPanel.SetActive(true);
        menuPanel.SetActive(false);
    }
    public void Quit()
    {
        Application.Quit();
    }
    #endregion

    #region ChangeName
    public void Rename()
    {
        menuPanel.SetActive(true);
        changeNamePanel.SetActive(false);
    }
    #endregion

    #region JoinPanel
    public void Connect()
    {
        string ip = "127.0.0.1";
        /*if (IPInputField.text != null || IPInputField.text.Length > 0)
        {
            ip = IPInputField.text;
        }*/

        networkManager.GetComponent<UnityTransport>().SetConnectionData(ip, (ushort)7777, "0.0.0.0");

        networkManager.StartClient();

        networkManager.NetworkConfig.ConnectionData = GetPlayerPayloadBytes();
    }

    private byte[] GetPlayerPayloadBytes()
    {
        string playerPayload = JsonUtility.ToJson(new StructPlayerData()
        {
            playerName = renameInputField.text
        });

        byte[] playerPayloadBytes = Encoding.ASCII.GetBytes(playerPayload);

        return playerPayloadBytes;
    }

    public void JoinBack()
    {
        menuPanel.SetActive(true);
        joinPanel.SetActive(false);
    }
    #endregion
}
