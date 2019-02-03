using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MolListInfo : MonoBehaviour {

    public Text molName;
    public Image molpreviewImage;
    public Text molarMass;
    public Text chemFormula;
    public Text meltingPoint;
    public Text boilingPoint;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetMolSelectInfoSheet(ChemViewARController chemviewController)
    {
        MoleculeController selectedMol = chemviewController.loadedChemModel.GetComponentInChildren<MoleculeController>();
        molName.text = selectedMol.moleculeName;
        molarMass.text = "Molar Mass: " + selectedMol.molarMass;
        chemFormula.text = "Chemical Formula: " + selectedMol.chemFormula;
        meltingPoint.text = "Melting Point: " + selectedMol.meltingPoint;
        boilingPoint.text = "Boiling Point: " + selectedMol.boilingPoint;
        molpreviewImage.sprite = selectedMol.molpreviewImage;


    }
}
