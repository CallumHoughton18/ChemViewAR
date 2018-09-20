using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{

    public Dropdown dropdown;
    public Toggle rotateToggle;
    public Toggle spinToggle;
    public Toggle infoToggle;
    public GameObject ChemViewARControllerOBJ;
    public List<GameObject> molsArray;
    public GameObject MoleculeContainer;
    ChemViewARController ChemController;

    private void OnGUI()
    {
    }
    // Use this for initialization
    void Start()
    {

        ChemController = ChemViewARControllerOBJ.GetComponent<ChemViewARController>();
        foreach (Transform molecule in MoleculeContainer.transform)
        {
            molsArray.Add(molecule.gameObject);
        }

        PopulateDropDown();

        dropdown = GetComponent<Dropdown>();
        dropdown.onValueChanged.AddListener(delegate
        {
            DropdownValueChanged(dropdown);
        });
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void DropdownValueChanged(Dropdown change)
    {
        GameObject newSelectedMol = molsArray.Where(mol => mol.name == (change.options[change.value].text)).FirstOrDefault();
        ChemViewARController ChemController = ChemViewARControllerOBJ.GetComponent<ChemViewARController>();
        ChemController.loadedChemModel = newSelectedMol;
        _ShowAndroidToastMessage("Molecule changed to: " + newSelectedMol.name);

    }

    public void SetToggles(MoleculeController _selectedMol)
    {
        rotateToggle.enabled = true;
        spinToggle.enabled = true;
        infoToggle.enabled = true;

        if (_selectedMol.rotateMolecule == true)
        {
            spinToggle.isOn = true;
        }

        if (_selectedMol.displayingInfoSheet == true)
        {
            infoToggle.isOn = true;
        }
    }

    public void TurnOffToggles()
    {
        rotateToggle.enabled = false;
        rotateToggle.isOn = false;
        spinToggle.enabled = false;
        spinToggle.isOn = false;
        infoToggle.enabled = false;
        infoToggle.isOn = false;

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
