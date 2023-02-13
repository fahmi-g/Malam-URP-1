using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private const float maxHealth = 100;
    private NetworkVariable<float> health;

    private void Awake()
    {
        health = new NetworkVariable<float>(maxHealth);
    }
}
