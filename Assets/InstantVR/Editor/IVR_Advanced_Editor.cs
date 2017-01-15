/* InstantVR Advanced Editor
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.6.0
 * date: September 23, 2016
 * 
 * - Added Leap Motion checks
 */

using UnityEngine;
using UnityEditor;
using System.IO;

namespace IVR {
    [InitializeOnLoad]
    public class IVR_Advanced_Editor {
        static IVR_Advanced_Editor() {
            CheckIVRAdvancedDefine();
            CheckPlayMakerDefine();
            CheckLeapDefine();
        }

        private static void CheckIVRAdvancedDefine() {
            InstantVR_Editor.GlobalDefine("INSTANTVR_ADVANCED");
        }

        #region PlayMaker
        private static void CheckPlayMakerDefine() {
            if (isPlayMakerInstalled()) {
                InstantVR_Editor.GlobalDefine("PLAYMAKER");
            } else {
                InstantVR_Editor.GlobalUndefine("PLAYMAKER");
            }
        }

        private static bool isPlayMakerInstalled() {
            string path = Application.dataPath + "/PlayMaker/Editor/PlayMakerEditor.dll";
            return File.Exists(path);
        }
        #endregion

        #region Leap
        public static void CheckLeapDefine() {
            if (isLeapInstalled()) {
                InstantVR_Editor.GlobalDefine("IVR_LEAP");
            } else {
                InstantVR_Editor.GlobalUndefine("IVR_LEAP");
            }
        }

        private static bool isLeapInstalled() {
            string path1 = Application.dataPath + "/Plugins/x86/LeapC.dll";
            string path2 = Application.dataPath + "/Plugins/x86_64/LeapC.dll";
            return File.Exists(path1) && File.Exists(path2);
        }
        #endregion
    }
}