using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MolListViewGenerator : MonoBehaviour
{

    public GameObject MoleculeContainer;
    public GameObject ChemViewARControllerOBJ;
    List<Button> moleculeButtons = new List<Button>();
    GameObject buttonPrefab;
    List<GameObject> molsList = new List<GameObject>();
    // Use this for initialization
    void Start()
    {
        foreach (Transform molecule in MoleculeContainer.transform)
        {
            GameObject molButton = Instantiate(buttonPrefab);
            var button = GetComponent<Button>();
            button.GetComponent<Text>().text = molecule.name;
            button.onClick.AddListener(() => MoleculeClick(molecule.name));

            molsList.Add(molecule.gameObject);
        }
    }

    public void MoleculeClick(string molClicked)
    {
        GameObject newSelectedMol = molsList.Where(mol => mol.name == molClicked).FirstOrDefault();
        ChemViewARController ChemController = ChemViewARControllerOBJ.GetComponent<ChemViewARController>();
        ChemController.loadedChemModel = newSelectedMol;
    }

    void GenerateListItems( List<GameObject>molsList)
    {

        foreach (var mol in molsList)
        {
            //buttons objs generated and added to parent
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
