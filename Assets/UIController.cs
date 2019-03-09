using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Toggle rotateToggle;
    public Toggle physicsToggle;
    public Toggle infoToggle;
    public GameObject ChemViewARControllerOBJ;
    private GameObject molSelect;
    public List<GameObject> molsList;
    public GameObject MoleculeContainer;
    ChemViewARController ChemController;
    public CanvasGroup UICanvasGroup;
    public GameObject molSelectCanvas;

    private void OnGUI()
    {
    }
    // Use this for initialization
    void Start()
    {
        ChemController = ChemViewARControllerOBJ.GetComponent<ChemViewARController>();
        foreach (Transform molecule in MoleculeContainer.transform)
        {
            molsList.Add(molecule.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void FadeIn()
    {

        GetComponent<UIFader>().FadeIn();
        UICanvasGroup.interactable = true;

    }

    public void FadeOut()
    {
        UICanvasGroup.interactable = false;
        GetComponent<UIFader>().FadeOut();
        TurnOffToggle(1);
    }

    public void DropdownValueChanged(Dropdown change)
    {
        GameObject newSelectedMol = molsList.Where(mol => mol.name == (change.options[change.value].text)).FirstOrDefault();
        ChemViewARController ChemController = ChemViewARControllerOBJ.GetComponent<ChemViewARController>();
        ChemController.loadedChemModel = newSelectedMol;
        _ShowAndroidToastMessage("Molecule changed to: " + newSelectedMol.name);

    }

    public void SetToggles(MoleculeController _selectedMol)
    {

        if (_selectedMol.displayingInfoSheet == true)
        {
            infoToggle.isOn = true;
        }
        else
        {
            infoToggle.isOn = false;
        }

        rotateToggle.enabled = true;
        physicsToggle.enabled = true;
        infoToggle.enabled = true;

    }

    public void TurnOffToggle(int i) /// 1 = rotate, 2 = info
    {
        switch (i)
        {
            case 1:
                rotateToggle.isOn = false;
                break;

            case 2:
                infoToggle.isOn = false;
                break;

            default:
                break;
        }
    }

    public void TurnOffToggles()
    {
        try
        {
            rotateToggle.enabled = false;
            rotateToggle.isOn = false;
            infoToggle.enabled = false;
            infoToggle.isOn = false;
        }

        catch (Exception e)
        {
            _ShowAndroidToastMessage(e.ToString());
        }

    }

    public void SpawnMolSelectCanvas(bool spawnSheet)
    {

        Camera camera = ChemController.FirstPersonCamera;
        try
        {
            if (spawnSheet)
            {
                if (molSelect != null)
                    Destroy(molSelect);

                molSelect = Instantiate(molSelectCanvas) as GameObject;
                molSelect.transform.position = camera.transform.position + (camera.transform.forward * 1);
                molSelect.transform.rotation = camera.transform.rotation;
                molSelect.GetComponentInChildren<MolListViewGenerator>().GenListItems(molsList, ChemController);
            }

            else if (!spawnSheet && molSelect != null)
            {
                molSelect.GetComponent<MolSelectController>().FadeOutSheet();
            }
        }

        catch (Exception e)
        {
            _ShowAndroidToastMessage(e.ToString());
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
