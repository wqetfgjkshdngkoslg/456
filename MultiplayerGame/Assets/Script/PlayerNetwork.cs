using System.Collections;
using System.Collections.Generic;

//using System.Diagnostics;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 플레이어 이동, 점프, 총알 발사 처리
/// </summary>
public class PlayerNetwork : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 7f;
    public float rotationSpeed = 10f;
    public float jumpForce = 5f;

    private Rigidbody rb;
    private bool isGrounded = true;

    [Header("Shooting Settings")]
    public GameObject BulletPrefab;
    public Transform BulletSpawner;
    public float bulletSpeed = 20.0f;       // 총알 속도 (m/s)
    public float bulletRange = 200.0f;      // 총알 사거리 (m)
    private float bulletLifetime;           // 자동 삭제 시간 계산용

    //  NetworkVariable 예시
    private NetworkVariable<int> randomNumber = new NetworkVariable<int>();
    private NetworkVariable<MyCustomData> customData = new NetworkVariable<MyCustomData>();

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        bulletLifetime = bulletRange / bulletSpeed;
    }

    private void Update()
    {
        if (!IsOwner) return;

        HandleMovement();
        HandleJump();
        HandleShooting();

     }

    private void HandleMovement()
    {
        Vector3 moveDir = Vector3.zero;

        if (UnityEngine.Input.GetKey(KeyCode.W)) moveDir.z += 1f;
        if (UnityEngine.Input.GetKey(KeyCode.S)) moveDir.z -= 1f;
        if (UnityEngine.Input.GetKey(KeyCode.A)) moveDir.x -= 1f;
        if (UnityEngine.Input.GetKey(KeyCode.D)) moveDir.x += 1f;

        if (moveDir != Vector3.zero)
        {
            Vector3 horizontalMove = new Vector3(moveDir.x, 0, moveDir.z).normalized;
            transform.position += horizontalMove * moveSpeed * Time.deltaTime;

            Quaternion toRotation = Quaternion.LookRotation(horizontalMove, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleJump()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    private void HandleShooting()
    {
        if (UnityEngine.Input.GetMouseButtonDown(0))
        {
            FireServerRpc();
        }
    }

    [ServerRpc]
    private void FireServerRpc(ServerRpcParams rpcParams = default)
    {
        StartCoroutine(FireThreeBullets());
    }

    private IEnumerator FireThreeBullets()
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject bullet = Instantiate(BulletPrefab, BulletSpawner.position, BulletSpawner.rotation);

            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            if (bulletRb != null)
            {
                bulletRb.isKinematic = false;
                bulletRb.linearVelocity = BulletSpawner.forward * bulletSpeed;
            }

            NetworkObject bulletNetObj = bullet.GetComponent<NetworkObject>();
            bulletNetObj.Spawn();

            StartCoroutine(DestroyBulletAfterTime(bulletNetObj, bulletLifetime));

            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator DestroyBulletAfterTime(NetworkObject bulletNetObj, float time)
    {
        yield return new WaitForSeconds(time);

        if (IsServer && bulletNetObj != null && bulletNetObj.IsSpawned)
        {
            bulletNetObj.Despawn();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    public struct MyCustomData : INetworkSerializable
    {
        public int _int;
        public bool _bool;

        public FixedString128Bytes message;


        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref message);

        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
            cam.target = transform;  // 카메라 타겟 = 내 플레이어
        }
    }


}





