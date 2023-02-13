using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldCamera : MonoBehaviour
{
    private void OnEnable()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete += HandleOnSceneLoaded;
    }

    private void HandleOnSceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        gameObject.SetActive(false);

        NetworkManager.Singleton.SceneManager.OnLoadComplete += HandleOnSceneLoaded;
    }
}
