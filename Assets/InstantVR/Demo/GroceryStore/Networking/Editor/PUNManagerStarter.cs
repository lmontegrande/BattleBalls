using UnityEditor;
using UnityEngine;
using System.IO;

namespace IVR {

    [CustomEditor(typeof(PUNManagerStarter))]
    public class PUNManagerStarter_Editor : Editor {
        public override void OnInspectorGUI() {
#if IVR_PHOTON
            base.OnInspectorGUI();
#else
            EditorGUILayout.HelpBox("Photon Unity Networking not found. Please download the package from the Unity Asset Store", MessageType.Warning, true);
#endif

            InstantVR_Edge_Editor.CheckPhotonDefine();
        }
    }
}