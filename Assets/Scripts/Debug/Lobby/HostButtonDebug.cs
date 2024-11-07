using Mirror;
using UnityEngine;

public class HostButtonDebug : MonoBehaviour {

    public NetworkManager Network;

    public void StartHosting() {
        Network.StartHost();
    }

    public void TryConnectToDebugServer() {
        Network.StartClient();
    }

}
