using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MolListViewGenerator : MonoBehaviour
{

    public GameObject MoleculeContainer;
    public ChemViewARController chemViewController;
    public Button buttonPrefab;
    public GameObject contentPanel;
    List<Button> moleculeButtons = new List<Button>();
    List<GameObject> molsList = new List<GameObject>();
    public MolListInfo molListInfoSheet;
    // Use this for initialization
    void Start()
    {
    }

    public void GenListItems(List<GameObject> molecules, ChemViewARController chemViewARController)
    {

        //MoleculeContainer = molContainer;
        //ChemViewARControllerOBJ = chemViewARControllerOBJ;
        foreach (GameObject molecule in molecules)
        {
            string molName = molecule.name;
            Button molButton = Instantiate(buttonPrefab) as Button;
            molButton.transform.SetParent(contentPanel.transform, false);
            molButton.transform.localScale = Vector3.one;
            molButton.GetComponentInChildren<Text>().text = molName;
            molButton.onClick.AddListener(() => MoleculeClick(molName));
        }

        molsList = molecules;
        chemViewController = chemViewARController;

    }

    public void MoleculeClick(string molClicked)
    {
        try
        {
            Debug.Log(molClicked);
            //_ShowAndroidToastMessage(molClicked);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            //_ShowAndroidToastMessage(e.ToString());
        }
        GameObject newSelectedMol = molsList.Where(mol => mol.name == molClicked).FirstOrDefault();
        chemViewController.loadedChemModel = newSelectedMol;
        molListInfoSheet.SetMolSelectInfoSheet(chemViewController);
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
