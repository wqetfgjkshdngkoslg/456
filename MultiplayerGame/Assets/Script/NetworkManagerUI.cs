using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.Events;

public class NetworkManagerUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [SerializeField] private Button HostButton;
    [SerializeField] private Button ServerButton;
    [SerializeField] private Button ClientButton;
    

    private void Awake()
    {

        HostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            Debug.Log("Host 시작");
        });

        ClientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            Debug.Log("Client 시작");
        });

        ServerButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
            Debug.Log("Server 시작");
        });


        //HostButton.AddEvent(() => NetworkManager.Singleton.StartHost());
        //ServerButton.AddEvent(() => NetworkManager.Singleton.StartServer());
        //ClientButton.AddEvent(() => NetworkManager.Singleton.StartClient());
    }
}

//public static class Extension
//{
//    public static void AddEvent(this Button button, UnityAction action)
//    {
//        button.onClick.RemoveAllListeners();
//        button.onClick.AddListener(action);
//    }
//}
