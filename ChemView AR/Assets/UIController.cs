using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    public Dropdown dropdown;
    public GameObject ChemViewARControllerOBJ;
    public GameObject[] molsArray;

	// Use this for initialization
	void Start () {
        molsArray = Resources.LoadAll<GameObject>("Prefabs");
        PopulateDropDown();

        dropdown = GetComponent<Dropdown>();
        dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(dropdown);
        });
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void DropdownValueChanged(Dropdown change)
    {
        GameObject newSelectedMol = Array.Find(molsArray, mol => mol.name.Equals(change.options[change.value].text));
        ChemViewARController ChemController = ChemViewARControllerOBJ.GetComponent<ChemViewARController>();
        ChemController.loadedChemModel = newSelectedMol;
        _ShowAndroidToastMessage("Molecule changed to: " + newSelectedMol.name);

    }

    void PopulateDropDown()
    {
        
        List<string> mols = new List<string>();

        foreach (var mol in molsArray)
        {
            mols.Add(mol.name);
        }

        dropdown.ClearOptions();
        dropdown.AddOptions(mols);
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
