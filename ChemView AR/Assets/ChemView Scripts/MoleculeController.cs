using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoleculeController : MonoBehaviour {

    float speed = 100;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(Vector3.up, speed * Time.deltaTime);
    }

    public void Highlight()
    {
        Behaviour highlighted = (Behaviour)GetComponent("Halo");
        highlighted.enabled = true;
    }

    public void Dehighlight()
    {
        Behaviour highlighted = (Behaviour)GetComponent("Halo");
        highlighted.enabled = false;
    }
}
