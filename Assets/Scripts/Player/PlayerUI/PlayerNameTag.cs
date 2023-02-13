using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameTag : NetworkBehaviour
{
    private Transform playerCamera;
    [SerializeField] private TextMeshProUGUI playerName;

    public override void OnNetworkSpawn()
    {
        if(Camera.main)
            playerCamera = Camera.main.transform;

        if (IsServer)
        {
            //We should get the PlayerManager data using server otherwise we will get local/offline PlayerManager (we using Server PlayerManager Instance)
            //  if we, nameHolder = PlayerManager.GetPlayerData(OwnerClientId).Value.playerName (not using argument/parameter), it will set the server variable
            //  so if we, playerName.text = this.nameHolder.ToString(); //the local nameHolder is empty, because we assigning this with clientRpc (called in client only)
            SetNameClientRpc(PlayerManager.GetPlayerData(OwnerClientId).Value.playerName);
        }
    }

    [ClientRpc]
    private void SetNameClientRpc(FixedString64Bytes name)
    {
        playerName.text = name.ToString();
    }

    private void FixedUpdate()
    {
        if(playerCamera != null)
            transform.LookAt(playerCamera.position);
    }
}
