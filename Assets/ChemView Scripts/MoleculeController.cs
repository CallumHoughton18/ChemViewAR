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
    float scaleForOutline = 1;
    Collider collider;
    Rigidbody molRigidBody;

    public float initialFingersDistance;
    public Vector3 initialScale;
    public Vector3 BeginningScale;
    public Quaternion initRotation;
    public Vector3 initialHaloScale;
    Vector3 NewMolPos;
    Vector3 prevPos;
    public static Transform ScaleTransform;
    private bool offsetY = false;
    private bool offsetX = false;

    float rotationSpeed = 100;

    public bool isSelected = true;
    public bool rotateMolecule = false;

    public bool userRotatingMolecule = false;

    public string moleculeName;
    public string moleculeInfo;

    string wikiAPITemplateQuery = "https://en.wikipedia.org/api/rest_v1/page/summary/MOLNAME";

    public GameObject molInfoSheet;
    public Sprite molImage;

    public bool displayingInfoSheet = false;
    public bool isScaling = false;

    public ChemViewARController MainController;
    GameObject MainControllerObject;
    bool noPhysics = true;

    // Use this for initialization
    IEnumerator Start()
    {
        initRotation = transform.rotation;
        BeginningScale = transform.localScale;
        molRigidBody = transform.GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        moleculeName = transform.name.Replace("(Clone)", string.Empty);
        Shader.SetGlobalFloat("_Outline", 0.005f * scaleForOutline);

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

        MainController = MainControllerObject.GetComponent<ChemViewARController>();
    }

    // Update is called once per frame
    void Update()
    {

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

                    /// issue here, two molecules using same _outline value looks bad
                    scaleForOutline = Vector3.SqrMagnitude(transform.localScale) / Vector3.SqrMagnitude(BeginningScale);
                    Shader.SetGlobalFloat("_Outline", 0.005f * scaleForOutline);
                }
            }

            else
                isScaling = false;
        }

        if (MainController.enableVelocity == false)
        {
            molRigidBody.velocity = Vector3.zero;
            molRigidBody.angularVelocity = Vector3.zero;
            molRigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        else if (MainController.enableVelocity)
        {
            molRigidBody.constraints = RigidbodyConstraints.None;
            //molRigidBody.useGravity = true;
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
                float rotX = DetectRotationAndPinch.fingerPoint.x * 15 * Mathf.Deg2Rad;
                float rotY = DetectRotationAndPinch.fingerPoint.y * 15 * Mathf.Deg2Rad;

                transform.Rotate(Vector3.right, rotY, Space.Self);
                transform.Rotate(Vector3.up, rotX, Space.Self);

                if (MainController.enableVelocity)
                {
                    transform.Rotate(Vector3.right, rotY * Time.deltaTime);
                    transform.Rotate(Vector3.up, rotX * Time.deltaTime);
                }

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
        Shader.SetGlobalFloat("_Outline", 0.005f * scaleForOutline);
        GameObject HighlightGameObj = GameObject.Find("HighlightShaderObj");
        MeshRenderer hightlightRenderer = HighlightGameObj.GetComponent<MeshRenderer>();
        Shader highlightShader = hightlightRenderer.materials[0].shader;

        foreach (Transform child in transform)
        {
            child.GetComponent<MeshRenderer>().materials[0].shader = highlightShader;
        }

        isSelected = true;
    }

    public void Dehighlight()
    {
        GameObject HighlightGameObj = GameObject.Find("DehighlightShaderObj");
        MeshRenderer hightlightRenderer = HighlightGameObj.GetComponent<MeshRenderer>();
        Shader highlightShader = hightlightRenderer.materials[0].shader;

        foreach (Transform child in transform)
        {
            child.GetComponent<MeshRenderer>().materials[0].shader = highlightShader;
        }

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
    }

    void OnMouseDrag()
    {
        if (Input.touchCount == 1 && isSelected == true && userRotatingMolecule == false) //factor in camera movement to molecule movement also
        {
            try
            {
                prevPos = transform.position;
                if (MainController.enableVelocity)
                {
                    molRigidBody.velocity = Vector3.zero;
                    molRigidBody.angularVelocity = Vector3.zero;
                }

                distance = Camera.main.WorldToScreenPoint(transform.position);
                Vector2 touchMovement = Input.GetTouch(0).deltaPosition;

                Vector3 fingerPos = new Vector3(Input.mousePosition.x - xPos,
                         Input.mousePosition.y - yPos, distance.z);

                NewMolPos = Camera.main.ScreenToWorldPoint(fingerPos);

                Vector3 fingerPosDelta =
                    new Vector3(distance.x + touchMovement.x,
                    distance.y + touchMovement.y, distance.z);

                Vector3 worldPos = Camera.main.ScreenToWorldPoint(fingerPosDelta);

                if (Vector3.Distance(NewMolPos, worldPos) < 0.15) // cannot directly set to fingerPosDelta as molecule then does not move with camera.
                {
                    transform.parent.position = NewMolPos;
                    transform.position = NewMolPos;
                }
            }

            catch (Exception e)
            {
                _ShowAndroidToastMessage(e.ToString());
            }
        }
    }

    private void OnMouseUp()
    {
        if (MainController.enableVelocity && !userRotatingMolecule && !isScaling)
        {
            Vector3 throwVector = transform.position - prevPos;
            float throwSpeed = throwVector.magnitude / Time.deltaTime;
            Vector3 throwVelocity = throwSpeed * throwVector.normalized;
            molRigidBody.velocity = throwVelocity;
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

