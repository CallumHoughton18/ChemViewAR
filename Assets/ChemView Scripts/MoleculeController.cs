using Assets.Models;
using GoogleARCore;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoleculeController : MonoBehaviour
{

    float speed = 100;

    Vector3 distance;
    public Vector3 planePosition;
    float xPos;
    float yPos;
    float xDistance;
    float yDistance;

    float rotationTime;
    bool recordRotTime = false;
    Vector2 initRotationFingerPos;
    bool pitchRotation;
    float totalZRotation;

    float scaleForOutline = 1;
    Collider collider;
    Rigidbody molRigidBody;
    GameObject sheet;

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

    public float HighlightThicknessFactor;

    // Use this for initialization
    IEnumerator Start()
    {
        initRotation = transform.rotation;
        BeginningScale = transform.localScale;
        molRigidBody = transform.GetComponent<Rigidbody>();
        molRigidBody.maxAngularVelocity = 15;
        collider = GetComponent<Collider>();

        Shader.SetGlobalFloat("_Outline", (0.003f * scaleForOutline) * HighlightThicknessFactor);

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
        //transform.Rotate(Vector3.up, speed * Time.deltaTime); //starts molecule rotation
        if (rotateMolecule == true)
        {
            RotateMolecule();
        }

        int fingersOnScreen = 0;

        foreach (Touch touch in Input.touches)
        {
            fingersOnScreen++; //Counts num touches on screen


            if (fingersOnScreen == 2 && isSelected == true && MainController.UserRotating == false) //enable 'pinch' motion
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

                    scaleForOutline = Vector3.SqrMagnitude(transform.localScale) / Vector3.SqrMagnitude(BeginningScale);
                    Shader.SetGlobalFloat("_Outline", (0.003f * scaleForOutline) * HighlightThicknessFactor);
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

        if (MainController.enableVelocity && userRotatingMolecule && recordRotTime)
            rotationTime += Time.deltaTime;
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
                //transform.Rotate(desiredRotation.eulerAngles);
                transform.rotation = desiredRotation;
                pitchRotation = true;

                if ((totalZRotation < 0) != (DetectRotationAndPinch.turnAngleDelta < 0)) /// otherwise rotating one way then the other would give final force as 0, ie no spin.
                {
                    totalZRotation = 0;
                    rotationTime = 0;
                }


                totalZRotation += DetectRotationAndPinch.turnAngleDelta;
            }

            else
            {
                /// use for generating force if physics enabled
                xDistance = DetectRotationAndPinch.fingerPoint.x * 15 * Mathf.Deg2Rad;
                yDistance = DetectRotationAndPinch.fingerPoint.y * 15 * Mathf.Deg2Rad;

            }
        }

    }

    public void DisplayInfoSheet(Camera player, bool display)
    {
        if (display == true && displayingInfoSheet == false && isSelected == true && MainController.enableVelocity == false)
        {

            displayingInfoSheet = true;

            Vector3 colliderSize = collider.bounds.size;
            Vector3 placement = new Vector3(transform.position.x, transform.position.y + (colliderSize.y), transform.position.z + (colliderSize.z * 0.7f));

            if (sheet == null)
                sheet = Instantiate(molInfoSheet, placement, transform.rotation);

            else
                sheet.GetComponent<UIFader>().FadeIn();


            sheet.transform.parent = transform.parent;

        }

        else if (display == false && isSelected == true)
        {
            displayingInfoSheet = false;
            foreach (Transform child in transform.parent)
            {
                if (child.tag == "infosheet")
                {
                    child.GetComponent<UIFader>().FadeOut();
                }
            }
        }


    }
    public void Highlight()
    {
        Shader.SetGlobalFloat("_Outline", (0.003f * scaleForOutline) * HighlightThicknessFactor);
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


        if (MainController.enableVelocity && userRotatingMolecule && recordRotTime == false)
        {
            initRotationFingerPos = DetectRotationAndPinch.fingerPoint;
            recordRotTime = true;
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
                    molRigidBody.velocity = Vector3.zero;


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

        else if (userRotatingMolecule && Input.touchCount == 1)
        {
            float XaxisRotation = Input.GetAxis("Mouse X") * 0.05f;
            float YaxisRotation = Input.GetAxis("Mouse Y") * 0.05f;
            // select the axis by which you want to rotate the GameObject
            if (XaxisRotation < 1 && YaxisRotation < 1)
            {
                transform.RotateAround(Vector3.up, XaxisRotation);
                transform.RotateAround(Vector3.left, YaxisRotation);
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

        if (userRotatingMolecule && MainController.enableVelocity)
        {
            molRigidBody.angularVelocity = Vector3.zero;

            if (pitchRotation)
            {
                float force = GenerateForce(totalZRotation, rotationTime) / 40;
                molRigidBody.AddTorque(0, 0, force);
            }

            else
            {

                molRigidBody.AddTorque(transform.up * GenerateForce(xDistance, rotationTime));
                molRigidBody.AddTorque(transform.right * GenerateForce(yDistance, rotationTime));
            }

            pitchRotation = false;
            totalZRotation = 0;
            recordRotTime = false;
            rotationTime = 0;
        }
    }

    float GenerateForce(float distance, float time)
    {
        _ShowAndroidToastMessage(distance.ToString());
        float vel = distance / time;
        float accel = vel / time;

        float force = molRigidBody.mass * accel;

        return force;
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

