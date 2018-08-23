using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;
using Assets.Models;

public class MolInfoController : MonoBehaviour {
    public Text headerText;
    public Text bodyText;
    string wikiAPITemplateQuery = "https://en.wikipedia.org/api/rest_v1/page/summary/MOLNAME";
    // Use this for initialization
    IEnumerator Start () {

        var parentName = transform.parent.name.Replace("(Clone)", string.Empty);
        headerText.text = parentName;
        string query = wikiAPITemplateQuery.Replace("MOLNAME", parentName);
        _ShowAndroidToastMessage(query);

        using (WWW wikiReq = new WWW(query))
        {
            yield return wikiReq;
            WikiInfo wikiObj = JsonConvert.DeserializeObject<WikiInfo>(wikiReq.text);
            bodyText.text = wikiObj.extract;

        }
    }
	
	// Update is called once per frame
	void Update () {
		
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
