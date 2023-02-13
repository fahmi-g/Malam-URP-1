using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using Unity.Collections;
using Unity.VisualScripting;

public static class PlayerManager
{
    //We have to modify this in server (because it shares between server instances only) (client can modify but will other client cannot access it)
    //Host or server can modify, even when the network is not started host can modify for all.(because we use the this static class instance of the host)
    //Because of that we also need to access/get it by/via/if we are a server. (get it by server and modify the network variable)
                                                                               //NetworkVariable can only be modified by the sever
    public static Dictionary<ulong, StructPlayerData> playersDataDict = new Dictionary<ulong, StructPlayerData>();

    public static void AddPlayerData(ulong clientId, StructPlayerData playerData)
    {
        playersDataDict.Add(clientId, playerData);
    }

    public static StructPlayerData? GetPlayerData(ulong clientId)
    {
        if (playersDataDict.TryGetValue(clientId, out StructPlayerData playerData))
        {
            return playerData;
        }

        return null;
    }

    public static void RemovePlayerData(ulong clientId)
    {
        playersDataDict.Remove(clientId);
    }

    public static void ClearPlayerData()
    {
        playersDataDict.Clear();
    }


    public static void ToggleLobbyPlayerState(ulong clientId)
    {
        playersDataDict[clientId] = new StructPlayerData()
        {
            playerName = playersDataDict[clientId].playerName,
            isLobbyReady = !playersDataDict[clientId].isLobbyReady
        };
    }
}
