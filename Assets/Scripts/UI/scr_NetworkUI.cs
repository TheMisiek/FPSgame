using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class scr_NetworkUI : NetworkBehaviour
{
    [SerializeField] private Button _hostButton;
    [SerializeField] private Button _clientButton;
    [SerializeField] private TextMeshProUGUI _playerCountText;

    private NetworkVariable<int> _playerNum = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);

    private void Awake()
    {
        _hostButton.onClick.AddListener(() =>
        {
            Cursor.visible = false;
            NetworkManager.Singleton.StartHost();
        });
        _clientButton.onClick.AddListener(() =>
        {
            Cursor.visible = false;
            NetworkManager.Singleton.StartClient();
        });
    }

    private void Update()
    {
        _playerCountText.text = "Players: " +_playerNum.Value.ToString() + "/2";

        if (!IsServer) return;
        _playerNum.Value = NetworkManager.Singleton.ConnectedClients.Count;
    }
}
