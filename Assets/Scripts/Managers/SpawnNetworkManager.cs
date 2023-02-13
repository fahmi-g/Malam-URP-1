using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnNetworkManager : MonoBehaviour
{
    [SerializeField] private GameObject networkManager;

    private void Start()
    {
        if (!GameManager.isNetworkManagerSpawned)
        {
            Instantiate(networkManager);
            GameManager.isNetworkManagerSpawned = true;
        }
    }
}
