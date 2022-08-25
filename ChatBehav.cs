using Mirror;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ChatBehav : NetworkBehaviour
{
    [SerializeField] private Text chatText = null;
    [SerializeField] private InputField inputField = null;
    [SerializeField] private GameObject canvas = null;
    public GameObject panel;
    public bool hidden = false;

    private static event Action<string> OnMessage;

    // Called when the a client is connected to the server
    public override void OnStartAuthority()
    {
        canvas.SetActive(true);

        OnMessage += HandleNewMessage;
    }

    // Called when a client has exited the server
    [ClientCallback]
    private void OnDestroy()
    {
        if (!hasAuthority) { return; }

        OnMessage -= HandleNewMessage;
    }

    // When a new message is added, update the Scroll View's Text to include the new message
    private void HandleNewMessage(string message)
    {
        chatText.text += message;
    }

    // When a client hits the enter button, send the message in the InputField
    [Client]
    public void Send()
    {
        if (!Input.GetKeyDown(KeyCode.Return)) { return; }
        if (string.IsNullOrWhiteSpace(inputField.text)) { return; }
        Player pl = gameObject.GetComponent<Player>();
        CmdSendMessage(inputField.text, pl.playerName);
        inputField.text = string.Empty;
        pl.RecordScore("Message", 1);
    }

    [Command]
    private void CmdSendMessage(string message, string plName)
    {
        // Validate message
        RpcHandleMessage($"[{plName}]: {message}");
    }

    [ClientRpc]
    private void RpcHandleMessage(string message)
    {
        OnMessage?.Invoke($"\n{message}");
    }

    public void HideShowChat()
    {
        if (hidden)
        {
            panel.transform.position = new Vector3(panel.transform.position.x + 600, panel.transform.position.y, panel.transform.position.z);
            hidden = false;
        } else
        {
            panel.transform.position = new Vector3(panel.transform.position.x - 600, panel.transform.position.y, panel.transform.position.z);
            hidden = true;
        }
    }
}
