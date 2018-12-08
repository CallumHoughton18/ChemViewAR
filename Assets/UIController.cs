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
    public Toggle physicsToggle;
    public Toggle infoToggle;
    public GameObject ChemViewARControllerOBJ;
    public List<GameObject> molsArray;
    public GameObject MoleculeContainer;
    ChemViewARController ChemController;
    public CanvasGroup UICanvasGroup;

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

    public void FadeIn()
    {

        StartCoroutine(FadeCanvasGroup(UICanvasGroup, UICanvasGroup.alpha, 1));
        UICanvasGroup.interactable = true;

    }

    public void FadeOut()
    {
        UICanvasGroup.interactable = false;
        StartCoroutine(FadeCanvasGroup(UICanvasGroup, UICanvasGroup.alpha, 0));
    }

    public IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float start, float end, float lerpTime = 0.5f)
    {
        float _timeStartedLerping = Time.time;
        float timeSinceStarted = Time.time - _timeStartedLerping;
        float percentageComplete = timeSinceStarted / lerpTime;

        while (true)
        {
            timeSinceStarted = Time.time - _timeStartedLerping;
            percentageComplete = timeSinceStarted / lerpTime;

            float currentValue = Mathf.Lerp(start, end, percentageComplete);

            canvasGroup.alpha = currentValue;

            if (percentageComplete >= 1) break;

            yield return new WaitForFixedUpdate();
        }
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
