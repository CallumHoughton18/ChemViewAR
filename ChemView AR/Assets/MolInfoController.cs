using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;
using Assets.Models;

public class MolInfoController : MonoBehaviour
{
    MoleculeController parentMol;

    public Text headerText;
    public Text bodyText;
    public Image molImage;

    Quaternion initRotation;
    Vector3 initPosition;
    // Use this for initialization
    void Start()
    {
        initRotation = transform.localRotation;
        initPosition = transform.localPosition;

        parentMol = transform.parent.GetComponentInChildren<MoleculeController>();
        headerText.text = parentMol.moleculeName;
        bodyText.text = parentMol.moleculeInfo;
        molImage.sprite = parentMol.molImage;
        _ShowAndroidToastMessage("test" + headerText.text);
    }

    // Update is called once per frame
    void Update()
    {
        if (parentMol.userRotatingMolecule == false && parentMol.rotateMolecule == false)
        {
            initPosition = transform.localPosition;
        }
    }

    private void LateUpdate()
    {

        if (parentMol.userRotatingMolecule || parentMol.rotateMolecule)
        {
            transform.localPosition = initPosition;
        }

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
