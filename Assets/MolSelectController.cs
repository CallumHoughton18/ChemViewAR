using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MolSelectController : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GetComponent<UIFader>().FadeInWithScale(gameObject, new Vector3(1, 1, 1));
	}

    public void DestroySheet()
    {
        GetComponent<UIFader>().FadeOutWithScale(gameObject);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
