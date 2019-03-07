using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ChemviewHelper {
    public enum MoleculeSubType { Basic, Drugs, Enantiomers, All}

    public static T ParseEnum<T>(string value)
    {
        return (T)MoleculeSubType.Parse(typeof(T), value, true);
    }

    private static void ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                    message, 0);
                toastObject.Call("show");
            }));
        }
    }
}
