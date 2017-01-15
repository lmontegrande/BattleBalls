/* InstantVR Network Avatar
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.5.0
 * date: May 24, 2016
 *
 * - added check on handMovements existence
 */

 using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;

[RequireComponent(typeof(NetworkManager))]
public class NetworkManagerStarter : MonoBehaviour {

    public bool forceHost = false;
    private NetworkManager nwMan;
    string url = null;
    int _port;
    string _mode;

    void Start() {
        nwMan = GetComponent<NetworkManager>();

        if (url != null)
            nwMan.networkAddress = url;

        if (nwMan.networkAddress == Network.player.ipAddress ||
            nwMan.networkAddress.ToLower() == "localhost" ||
            nwMan.networkAddress == "127.0.0.1" ||
            forceHost) {
            nwMan.StartServer();
        } else {
            nwMan.StartClient();
        }
    }

    /*
    void WriteServerSettings() {
        StreamWriter stream = new StreamWriter(Application.persistentDataPath  + "/serverout.txt");
        stream.Write("Player:192.168.76.117:25000");
        stream.Close();
    }

    public static void ReadServerSettings(out string url, out int port, out string mode) {
        url = "";
        port = -1;
        mode = "";
        try {
            if (System.IO.File.Exists(Application.persistentDataPath + "/server.txt")) {
                StreamReader stream = new StreamReader(Application.persistentDataPath + "/server.txt");
                string[] ss = stream.ReadLine().Split(":"[0]);
                mode = ss[0];
                url = ss[1];
                port = int.Parse(ss[2]);
            }
        }
        catch (Exception e) {
            Debug.LogWarning("File could not be read: " + e.Message);
        }
    }
    */
}
