using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerCameraBob : MonoBehaviour
{
    private CharacterController characterController;
    
    private Vector3 bobOffset;
    private float frequency = 10f;
    private float amplitude = .05f;

    private bool isClient;

    private void Awake()
    {
        bobOffset = Vector3.zero;
        characterController = GetComponentInParent<CharacterController>();
        isClient = NetworkManager.Singleton.IsClient;
    }

    private void CameraBob()
    {
        bobOffset.x = Mathf.Cos(Time.time * frequency/2f) * amplitude*3f * Time.deltaTime;
        bobOffset.y = Mathf.Sin(Time.time * frequency * 2f) * amplitude*5f * Time.deltaTime;

        transform.localPosition += bobOffset;
    }

    private void FixedUpdate()
    {
        if (isClient && characterController.velocity.magnitude > 3f)
        {
            CameraBob();
        }
    }
}
