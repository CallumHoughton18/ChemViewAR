using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoleculeController : MonoBehaviour {

    float speed = 100;

    Vector3 distance;
    public Vector3 planePosition;
    float xPos;
    float yPos;

    public float initialFingersDistance;
    public Vector3 initialScale;
    public Vector3 initialHaloScale;
    public static Transform ScaleTransform;

    float rotationSpeed = 100;

    public bool isSelected = false;
    public bool rotateMolecule = false;
    public bool userRotatingMolecule = false;

    public GameObject molInfoSheet;

    // Use this for initialization
    void Start () { 
	}
	
	// Update is called once per frame
	void Update () {

        Behaviour highlight = (Behaviour)GetComponent("Halo");

        //transform.Rotate(Vector3.up, speed * Time.deltaTime); //starts molecule rotation
        if (rotateMolecule == true)
        {
            RotateMolecule();
        }

        if (this.transform.position.y <= planePosition.y)
        {
            this.transform.position = new Vector3(transform.position.x, planePosition.y, transform.position.z);
        }

        int fingersOnScreen = 0;

        foreach (Touch touch in Input.touches)
        {
            fingersOnScreen++; //Counts num touches on screen


            if (fingersOnScreen == 2) //enable 'pinch' motion
            {

                if (touch.phase == TouchPhase.Began)
                {
                    initialFingersDistance = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
                    initialScale = ScaleTransform.localScale;
                    //initialHaloScale = highlight.transform.localScale; needs work
                }
                else
                {
                    float currentFingersDistance = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);

                    float scaleFactor = currentFingersDistance / initialFingersDistance;

                    //highlight.transform.localScale = initialHaloScale * scaleFactor; needs work

                    ScaleTransform.localScale = initialScale * scaleFactor;
                }
            }
        }

    }

    public void DisplayInfoSheet(Camera player)
    {

        Vector3 placement = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        var sheet = Instantiate(molInfoSheet, placement, transform.rotation);

        sheet.transform.LookAt(player.transform);

        sheet.transform.parent = transform;

        _ShowAndroidToastMessage("Displaying Mol info...");

    }
    public void Highlight()
    {
        Behaviour highlighted = (Behaviour)GetComponent("Halo");
        highlighted.enabled = true;
        isSelected = true;
    }

    public void Dehighlight()
    {
        Behaviour highlighted = (Behaviour)GetComponent("Halo");
        highlighted.enabled = false;
        isSelected = false;
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }


    void OnMouseDown()
    {
        if (userRotatingMolecule == false)
        {
            Highlight();
            distance = Camera.main.WorldToScreenPoint(transform.position);
            xPos = Input.mousePosition.x - distance.x;
            yPos = Input.mousePosition.y - distance.y;

            ScaleTransform = transform;
        }

    }

    void OnMouseDrag()
    {
        if (userRotatingMolecule == false)
        {
            Highlight();
            if (yPos < 0)
            {
                yPos = 0;
            }
            Vector3 curPos =
             new Vector3(Input.mousePosition.x - xPos,
                         Input.mousePosition.y - yPos, distance.z);

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(curPos);
            transform.position = worldPos;
        }

        else if (userRotatingMolecule == true)
        {
            Highlight();
            float rotX = Input.GetAxis("Mouse X") * rotationSpeed * Mathf.Deg2Rad;
            float rotY = Input.GetAxis("Mouse Y") * rotationSpeed * Mathf.Deg2Rad;

            transform.Rotate(Vector3.up, -rotX, Space.World);
            transform.Rotate(Vector3.right, rotY, Space.World);
        }

    }

    public void RotateMolecule()
    {
        transform.Rotate(Vector3.up, speed * Time.deltaTime);  
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

