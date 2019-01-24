using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MolListViewGenerator : MonoBehaviour
{

    public GameObject MoleculeContainer;
    // Use this for initialization
    void Start()
    {
        List<GameObject> molsList = new List<GameObject>();
        foreach (Transform molecule in MoleculeContainer.transform)
        {
            molsList.Add(molecule.gameObject);
        }
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
