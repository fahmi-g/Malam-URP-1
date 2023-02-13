using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class EntityBehaviour : NetworkBehaviour
{
    private enum EntityState {patroling, attacking}
    private EntityState entityState;

    private NetworkManager networkManager;
    private NavMeshAgent navMeshAgent;

    public LayerMask obstacleLayerMask;
    public LayerMask playerLayerMask;

    private float entitySightDistance = 15f;
    private Transform targetTransform;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer) return;

        entityState = EntityState.patroling;

        networkManager = NetworkManager.Singleton;

        navMeshAgent = GetComponent<NavMeshAgent>();

        StartCoroutine(CoroutineForCheck());
    }

    private IEnumerator CoroutineForCheck()
    {
        while (true)
        {
            EntitySightCheck();
            MoveToTarget();

            yield return new WaitForSeconds(.03f);
        }
    }

    private void EntitySightCheck()
    {
        if (networkManager.ConnectedClientsList.Count < 1) return;

        Transform playerTransform;
        Vector3 directionToTarget;
        foreach (NetworkClient player in networkManager.ConnectedClientsList)
        {
            playerTransform = player.PlayerObject.transform;
            directionToTarget = (playerTransform.position - (transform.position + Vector3.up));

            if (Vector3.Angle(transform.forward, directionToTarget.normalized) <= 85f &&
                !Physics.Raycast(transform.position + Vector3.up, directionToTarget.normalized, entitySightDistance, obstacleLayerMask) &&
                (directionToTarget).sqrMagnitude <= entitySightDistance * entitySightDistance)
            {
                if (entityState != EntityState.attacking)
                    entityState = EntityState.attacking;

                targetTransform = playerTransform;
            }
            else
            {
                if(entityState != EntityState.patroling)
                    entityState = EntityState.patroling;
            }
        }
    }

    private void MoveToTarget()
    {
        if (entityState == EntityState.attacking)
        {
            navMeshAgent.SetDestination(targetTransform.position);
        }
    }
}
