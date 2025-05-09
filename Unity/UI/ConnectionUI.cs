using UnityEngine;
using TMPro;
using EXO.WebClient;
using System.Collections.Generic;
using System.Linq;

public class ConnectionUI : MonoBehaviour
{
    public ExoNetworkManager networkManager;
    public GameObject clientNamePanelPrefab;
    public Transform contentTransform;

    public TMP_InputField nameText;
    public TMP_InputField roomNameText;
    public TMP_InputField roomKeyText;

    private readonly List<ClientNamePanel> clientPanels = new();

    private void Start()
    {
        networkManager.OnClientStart += OnClientStartHandler;
        networkManager.OnHostStart += OnHostStartHandler;
        networkManager.OnClientJoin += OnClietJoinHandler;
        networkManager.OnClientLeft += OnClientLeftHandler;
    }

    private void OnClientLeftHandler(object sender, ExoClient e)
    {

        var toRemove = clientPanels.FirstOrDefault(c => c.clientID == e.ID);
        clientPanels.Remove(toRemove);

        if (toRemove != null)
        {
            Destroy(toRemove.gameObject);
        }


    }

    private void OnClietJoinHandler(object sender, ExoClient e)
    {
        var panel = Instantiate(clientNamePanelPrefab, contentTransform).GetComponent<ClientNamePanel>();
        panel.Init(e.ID, e.Name);
        clientPanels.Add(panel);

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
        networkManager.StartClient(roomKeyText.text, nameText.text);
    }

    public void HostConnect()
    {
        networkManager.StartHost(roomNameText.text, nameText.text);
    }

}
