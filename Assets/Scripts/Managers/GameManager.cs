using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using Unity.Collections;

public class GameManager : NetworkSingleton<GameManager>
{
    public static bool isNetworkManagerSpawned = false;
    public static bool isGameStarted;
}
