using UnityEngine;
using Unity.Netcode;

public class BulletAction : NetworkBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // 로컬에서 충돌 감지 → 서버에 요청
        if (!IsServer)
        {
            NetworkObject netObj = collision.gameObject.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                RequestCollisionServerRpc(netObj.NetworkObjectId);
            }
            return;
        }

        HandleCollision(collision.gameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestCollisionServerRpc(ulong targetNetworkObjectId, ServerRpcParams rpcParams = default)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out NetworkObject targetObj))
        {
            HandleCollision(targetObj.gameObject);
        }
    }

    private void HandleCollision(GameObject collidedObj)
    {
        if (collidedObj == null) return;

        NetworkObject bulletNetObj = GetComponent<NetworkObject>();

        if (collidedObj.CompareTag("Player"))
        {
            NetworkObject playerNetObj = collidedObj.GetComponent<NetworkObject>();
            playerNetObj?.Despawn(); // 플레이어 삭제
            bulletNetObj?.Despawn(); // 총알 삭제
        }
        else
        {
            bulletNetObj?.Despawn(); // Player가 아닌 네트워크 오브젝트도 삭제
        }
    }
}
