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
    GameObject user;

    public Text headerText;
    public Text bodyText;
    public Image molImage;

    Quaternion initRotation;
    Vector3 initPosition;
    // Use this for initialization
    void Start()
    {
        user = GameObject.FindWithTag("MainCamera");

        initRotation = transform.localRotation;
        initPosition = transform.localPosition;

        parentMol = transform.parent.GetComponentInChildren<MoleculeController>();
        headerText.text = parentMol.moleculeName;
        bodyText.text = parentMol.moleculeInfo;
        molImage.sprite = parentMol.molImage;

        GetComponent<UIFader>().FadeIn();

        DetermineSheetScale();
    }
    

    private void OnDestroy()
    {
        GetComponent<UIFader>().FadeOut();
    }

    private void OnCollisionEnter(Collision collision)
    {
    }

    public void DetermineSheetScale()
    {
        float distance = Vector3.Distance(user.transform.position, parentMol.gameObject.transform.position);

        if (distance <= 0.8)
            gameObject.transform.localScale = gameObject.transform.localScale / 2;


        else if (distance <= 1)
            gameObject.transform.localScale = gameObject.transform.localScale / 1.8f;


        else if (distance <= 1.2)
            gameObject.transform.localScale = gameObject.transform.localScale / 1.6f;


        else if (distance <= 1.4)
            gameObject.transform.localScale = gameObject.transform.localScale / 1.4f;

        else if (distance <= 1.6)
            gameObject.transform.localScale = gameObject.transform.localScale / 1.2f;
    }

    // Update is called once per frame
    void Update()
    {
        if (parentMol.userRotatingMolecule == false && parentMol.rotateMolecule == false)
        {
            initPosition = transform.localPosition;
        }

        transform.rotation = Quaternion.LookRotation(transform.position - user.transform.position);
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
