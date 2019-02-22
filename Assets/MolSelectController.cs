using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MolSelectController : MonoBehaviour {
    public Camera FirstPersonCamera;
	// Use this for initialization
	void Start () {
        GetComponent<UIFader>().FadeInWithScale(gameObject, gameObject.transform.localScale);
	}

    public void FadeOutSheet()
    {
        GetComponent<UIFader>().FadeOutWithScale(gameObject);
    }

    // Update is called once per frame
    void Update () {
        transform.rotation = Quaternion.LookRotation(transform.position - FirstPersonCamera.transform.position);
    }
}
