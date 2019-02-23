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
    double panelHeight;
    public Dropdown filterDropDown;
    List<Button> moleculeButtons = new List<Button>();
    List<GameObject> molsList = new List<GameObject>();
    RectTransform panelRect;
    public MolListInfo molListInfoSheet;
    // Use this for initialization
    void Start()
    {
        filterDropDown.onValueChanged.AddListener(delegate { FilterDropDownChanged(filterDropDown); });
        filterDropDown.ClearOptions();
        List<string> filterOptions = Enum.GetNames(typeof(ChemviewHelper.MoleculeSubType)).ToList();
        filterDropDown.AddOptions(filterOptions);
        filterDropDown.value = Convert.ToInt32(ChemviewHelper.MoleculeSubType.All);
        panelRect = contentPanel.GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        double newPanelHeight = panelRect.sizeDelta.y;
        if (panelHeight != newPanelHeight)
        {
            panelHeight = newPanelHeight;
            GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 1f;
        }
    }

    public void GenListItems(List<GameObject> molecules, ChemViewARController chemViewARController, ChemviewHelper.MoleculeSubType filter = ChemviewHelper.MoleculeSubType.All)
    {
        foreach (Transform child in contentPanel.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (GameObject molecule in molecules)
        {
            if (filter == ChemviewHelper.MoleculeSubType.All)
            {
                GenMolButton(molecule);
            }

            else
            {
                if (molecule.GetComponentInChildren<MoleculeController>().moleculeSubType == filter)
                {
                    GenMolButton(molecule);
                }
            }
        }

        molsList = molecules;
        chemViewController = chemViewARController;

    }

    public void GenMolButton(GameObject molecule)
    {
        string molName = molecule.name;
        Button molButton = Instantiate(buttonPrefab) as Button;
        molButton.transform.SetParent(contentPanel.transform, false);
        molButton.transform.localScale = Vector3.one;
        molButton.GetComponentInChildren<Text>().text = molName;
        molButton.onClick.AddListener(() => MoleculeClick(molName));
    }

    void FilterDropDownChanged(Dropdown change)
    {
        ChemviewHelper.MoleculeSubType selectedMolTypeFilter = (ChemviewHelper.MoleculeSubType)change.value;
        int i = 0;

        GenListItems(molsList, chemViewController, selectedMolTypeFilter);
        //LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponentInChildren<RectTransform>());

        Debug.Log(i.ToString());

    }


    public void MoleculeClick(string molClicked)
    {
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
