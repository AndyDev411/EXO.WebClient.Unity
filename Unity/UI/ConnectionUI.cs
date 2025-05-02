using UnityEngine;
using TMPro;
using EXO.WebClient;
using System;

public class ConnectionUI : MonoBehaviour
{
    public ExoNetworkManager networkManager;

    public TMP_InputField nameText;
    public TMP_InputField roomNameText;
    public TMP_InputField roomKeyText;

    private void Start()
    {
        networkManager.OnClientStart += OnClientStartHandler;
        networkManager.OnHostStart += OnHostStartHandler;
    }

    private void OnHostStartHandler(object sender, ExoNetworkManager e)
    {
        roomKeyText.text = e.RoomKey;
    }

    private void OnClientStartHandler(object sender, ExoNetworkManager e)
    {
        roomNameText.text = e.RoomName;
    }

    public void ClientConnect()
    {
        networkManager.StartClient(roomKeyText.text);
    }

    public void HostConnect()
    {
        networkManager.StartHost(roomNameText.text);
    }

}
