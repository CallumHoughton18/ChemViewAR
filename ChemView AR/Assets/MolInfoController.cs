using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;
using Assets.Models;

public class MolInfoController : MonoBehaviour
{
    public Text headerText;
    public Text bodyText;
    public Image molImage;
    string wikiAPITemplateQuery = "https://en.wikipedia.org/api/rest_v1/page/summary/MOLNAME";
    // Use this for initialization
    void Start()
    {
        MoleculeController parentMol = transform.parent.GetComponent<MoleculeController>();
        headerText.text = parentMol.moleculeName;
        bodyText.text = parentMol.moleculeInfo;
        molImage.sprite = parentMol.molImage;
        _ShowAndroidToastMessage("test" + headerText.text);
    }

    // Update is called once per frame
    void Update()
    {

    }


    private void _ShowAndroidToastMessage(string message)
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
