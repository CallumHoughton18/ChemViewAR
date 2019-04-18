using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MolListInfo : MonoBehaviour {

    public Text molName;
    public Image molpreviewImage;
    public Text subText1;
    public Text subText2;
    public Text subText3;
    public Text subText4;

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
        subText1.text = selectedMol.molInfo1;
        subText2.text = selectedMol.molInfo2;
        subText3.text = selectedMol.molInfo3;
        subText4.text = selectedMol.molInfo4;
        molpreviewImage.sprite = selectedMol.molImage;


    }
}
