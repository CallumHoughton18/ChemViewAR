using Assets.Models;
using GoogleARCore;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoleculeController : MonoBehaviour
{

    float speed = 100;

    Vector3 distance;
    public Vector3 planePosition;
    float xPos;
    float yPos;

    Collider collider;
    Rigidbody molRigidBody;

    public float initialFingersDistance;
    public Vector3 initialScale;
    public Quaternion initRotation;
    public Vector3 initialHaloScale;
    public static Transform ScaleTransform;
    private bool isUpsideDown = false;

    float rotationSpeed = 100;

    public bool isSelected = true;
    public bool rotateMolecule = false;

    private float sensitivty = 0.5f;
    private Vector3 fingerRef;
    private Vector3 rotOffset;
    private Vector3 rotation = Vector3.zero;
    public bool userRotatingMolecule = false;

    public string moleculeName;
    public string moleculeInfo;

    string wikiAPITemplateQuery = "https://en.wikipedia.org/api/rest_v1/page/summary/MOLNAME";

    public GameObject molInfoSheet;
    public Sprite molImage;

    public bool displayingInfoSheet = false;
    public bool isScaling = false;

    // Use this for initialization
    IEnumerator Start()
    {
        initRotation = transform.rotation;

        molRigidBody = transform.GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        moleculeName = transform.name.Replace("(Clone)", string.Empty);

        string query = wikiAPITemplateQuery.Replace("MOLNAME", moleculeName);
        using (WWW wikiReq = new WWW(query))
        {
            yield return wikiReq;
            WikiInfo wikiObj = JsonConvert.DeserializeObject<WikiInfo>(wikiReq.text);
            moleculeInfo = wikiObj.extract;

            using (WWW www = new WWW(wikiObj.thumbnail.source))
            {
                // Wait for download to complete
                yield return www;

                // assign texture

                molImage = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        molRigidBody.velocity = Vector3.zero;
        molRigidBody.angularVelocity = Vector3.zero;

        Behaviour highlight = (Behaviour)GetComponent("Halo");

        //transform.Rotate(Vector3.up, speed * Time.deltaTime); //starts molecule rotation
        if (rotateMolecule == true)
        {
            RotateMolecule();
        }

        int fingersOnScreen = 0;

        foreach (Touch touch in Input.touches)
        {
            fingersOnScreen++; //Counts num touches on screen


            if (fingersOnScreen == 2 && isSelected == true && userRotatingMolecule == false) //enable 'pinch' motion
            {
                isScaling = true;
                if (touch.phase == TouchPhase.Began)
                {
                    initialFingersDistance = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
                    initialScale = ScaleTransform.localScale;
                }
                else
                {
                    float currentFingersDistance = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);

                    float scaleFactor = currentFingersDistance / initialFingersDistance;

                    transform.localScale = initialScale * scaleFactor;

                }
            }
        }
    }

    private void LateUpdate()
    {

        if (userRotatingMolecule)
        {
            Quaternion desiredRotation = transform.rotation;

            DetectRotationAndPinch.Calculate();

            if (Mathf.Abs(DetectRotationAndPinch.turnAngleDelta) > 0)
            { // rotate
                Vector3 rotationDeg = Vector3.zero;
                rotationDeg.z = -DetectRotationAndPinch.turnAngleDelta;
                desiredRotation *= Quaternion.Euler(rotationDeg);
                transform.rotation = desiredRotation;
            }

            else
            {
                float rotX = DetectRotationAndPinch.fingerDistance.x * 20 * Mathf.Deg2Rad;
                float rotY = DetectRotationAndPinch.fingerDistance.y * 20 * Mathf.Deg2Rad;


                transform.Rotate(Vector3.up, rotX, Space.Self);
                transform.Rotate(Vector3.right, rotY, Space.Self);
            }
        }

    }

    public void DisplayInfoSheet(Camera player, bool display)
    {
        if (display == true && displayingInfoSheet == false && isSelected == true)
        {
            displayingInfoSheet = true;

            try
            {
                Vector3 colliderSize = collider.bounds.size;
                Vector3 placement = new Vector3(transform.position.x + colliderSize.x, transform.position.y + colliderSize.y, transform.position.z + colliderSize.z);
                var sheet = Instantiate(molInfoSheet, placement, transform.rotation);

                sheet.transform.LookAt(player.transform);
                sheet.transform.Rotate(0, 180, 0);

                sheet.transform.parent = transform.parent;

                _ShowAndroidToastMessage("Displaying Mol info...");
            }
            catch (Exception ex)
            {
                _ShowAndroidToastMessage(ex.ToString());
            }
        }

        else if (display == false && isSelected == true)
        {
            displayingInfoSheet = false;
            foreach (Transform child in transform.parent)
            {
                if (child.tag == "infosheet")
                {
                    _ShowAndroidToastMessage("Destroying Info Sheet");
                    Destroy(child.gameObject);
                }
            }
        }


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
        if (isSelected == true)
        {
            ScaleTransform = transform;
            distance = Camera.main.WorldToScreenPoint(transform.position);
            xPos = Input.mousePosition.x - distance.x;
            yPos = Input.mousePosition.y - distance.y;
        }

        if (userRotatingMolecule)
        {
            fingerRef = Input.mousePosition;

            if (Vector3.Dot(transform.up, Vector3.down) > 0)
            {
                _ShowAndroidToastMessage("mol upside down");
                isUpsideDown = true;
            }

            else
                isUpsideDown = false;
        }
    }

    void OnMouseDrag()
    {


        if (Input.touchCount == 1 && isSelected == true && userRotatingMolecule == false) //factor in camera movement to molecule movement also
        {
            try
            {
                distance = Camera.main.WorldToScreenPoint(transform.position);
                Vector2 touchMovement = Input.GetTouch(0).deltaPosition;

                Vector3 fingerPos = new Vector3(Input.mousePosition.x - xPos,
                         Input.mousePosition.y - yPos, distance.z);

                Vector3 fingerPosWorld = Camera.main.ScreenToWorldPoint(fingerPos);

                Vector3 fingerPosDelta =
                    new Vector3(distance.x + touchMovement.x,
                    distance.y + touchMovement.y, distance.z);

                Vector3 worldPos = Camera.main.ScreenToWorldPoint(fingerPosDelta);

                if (Vector3.Distance(fingerPosWorld, worldPos) < 0.15) // cannot directly set to fingerPosDelta as molecule then does not move with camera.
                {
                    transform.parent.position = fingerPosWorld;
                    transform.position = fingerPosWorld;
                }
            }

            catch (Exception e)
            {
                _ShowAndroidToastMessage(e.ToString());
            }
        }


        //if (userRotatingMolecule == true && isSelected == true)
        //{
        //    rotOffset = (Input.mousePosition - fingerRef);

        //    float rotX = Input.GetAxis("Mouse X") * 85 * Mathf.Deg2Rad;
        //    float rotY = Input.GetAxis("Mouse Y") * 85 * Mathf.Deg2Rad;


        //    transform.Rotate(Vector3.up, rotX, Space.Self);
        //    transform.Rotate(Vector3.right, -rotY, Space.Self);


        //    fingerRef = Input.mousePosition;
        //}
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

