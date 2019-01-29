using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MolListViewGenerator : MonoBehaviour
{

    public GameObject MoleculeContainer;
    public GameObject ChemViewARControllerOBJ;
    public GameObject buttonPrefab;
    public GameObject contentPanel;
    List<Button> moleculeButtons = new List<Button>();
    List<GameObject> molsList = new List<GameObject>();
    // Use this for initialization
    void Start()
    {
    }

    public void GenListItems(List<GameObject> molecules, GameObject chemViewARControllerOBJ)
    {
        try
        {
            _ShowAndroidToastMessage(molecules.Count.ToString());
            //MoleculeContainer = molContainer;
            //ChemViewARControllerOBJ = chemViewARControllerOBJ;
            foreach (GameObject molecule in molecules)
            {
                GameObject molButton = Instantiate(buttonPrefab) as GameObject;
                molButton.transform.SetParent(contentPanel.transform, false);
                molButton.transform.localScale = Vector3.one;
                molButton.GetComponentInChildren<Text>().text = molecule.name;
                //molButton.GetComponent<Button>().onClick.AddListener(() => MoleculeClick(mo));
            }
        }

        catch (Exception e)
        {
            _ShowAndroidToastMessage(e.ToString());
        }
    }

    public void MoleculeClick(string molClicked)
    {
        _ShowAndroidToastMessage(molClicked);
        //GameObject newSelectedMol = molsList.Where(mol => mol.name == molClicked).FirstOrDefault();
        //ChemViewARController ChemController = ChemViewARControllerOBJ.GetComponent<ChemViewARController>();
        //ChemController.loadedChemModel = newSelectedMol;
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
