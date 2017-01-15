/* InstantVR Photon/PUN Networking Manager Starter
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.5.2
 * date: August 7, 2016
 *
 * - Updated to Photon 1.73 use
 */

 using UnityEngine;

public class PUNManagerStarter : MonoBehaviour {
#if IVR_PHOTON
    public string roomName;
    public string gameVersion;

    public int sendRate = 25;

    public GameObject playerPrefab;

    void Start () {
        PhotonNetwork.sendRate = sendRate;
        PhotonNetwork.sendRateOnSerialize = sendRate;
        PhotonNetwork.ConnectUsingSettings(gameVersion);
    }

    private void OnConnectedToMaster() {
        RoomOptions roomOptions = new RoomOptions() { IsVisible = false, MaxPlayers = 4 };
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    private void OnPhotonJoinRoomFailed() {
        Debug.LogError("Could not joint the " + roomName + " room");
    }

    private void OnJoinedRoom() {
        foreach (RoomInfo game in PhotonNetwork.GetRoomList()) {
            Debug.Log(game.name + " " + game.playerCount + "//" + game.maxPlayers);
        }
        Debug.Log("Joined "+ roomName +" room");
        PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity, 0);
    }
#endif
}
