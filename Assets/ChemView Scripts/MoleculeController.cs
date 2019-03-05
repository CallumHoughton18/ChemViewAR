﻿using Assets.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using UnityEngine;

public class MolRelDirection
{
    public Vector3 molRelUp { get; set; }
    public Vector3 molrelRight { get; set; }
}

public class MoleculeController : MonoBehaviour
{
    public Sprite molpreviewImage;
    public string chemFormula;
    public string molarMass;
    public string meltingPoint;
    public string boilingPoint;
    public ChemviewHelper.MoleculeSubType moleculeSubType;

    float speed = 100;

    Vector3 distance;
    public Vector3 planePosition;
    Vector3 newPosition;
    float xPos;
    float yPos;
    float mouseDownMousePosX;
    float mouseDownMousePosY;
    Vector2 mouseDownPixelPos;

    float rotationTime;
    bool recordRotTime = false;
    Vector2 initRotationFingerPos;
    bool pitchRotation;
    float totalZRotation;
    int rotateLeftRightSign;
    int rotateUpDownSign;
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
    bool collidingWithSurface = false;

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

    IEnumerator Start()
    {
        initRotation = transform.rotation;
        BeginningScale = transform.localScale;
        molRigidBody = GetComponent<Rigidbody>();
        molRigidBody.maxAngularVelocity = 15;
        collider = GetComponent<Collider>();
        InvokeRepeating("ReduceAngularVelocity", 0, 1.0f);

        Shader.SetGlobalFloat("_Outline", (0.003f * scaleForOutline) * HighlightThicknessFactor);

        string query = wikiAPITemplateQuery.Replace("MOLNAME", moleculeName);
        using (WWW wikiReq = new WWW(query))
        {
            yield return wikiReq;
            WikiInfo wikiObj = JsonConvert.DeserializeObject<WikiInfo>(wikiReq.text);
            if (string.IsNullOrEmpty(moleculeInfo))
                moleculeInfo = wikiObj.extract;

            using (WWW www = new WWW(wikiObj.thumbnail.source))
            {
                /// Wait for download to complete
                yield return www;

                /// assign texture

                if (molImage == null)
                    molImage = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
            }

        }
    }

    void ReduceAngularVelocity()
    {
        molRigidBody.angularVelocity = molRigidBody.angularVelocity * 0.6f;
    }
    // Update is called once per frame
    void Update()
    {
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

        if (MainController.enableVelocity && userRotatingMolecule && recordRotTime)
            rotationTime += Time.deltaTime;
    }

    private void LateUpdate()
    {

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
                sheet.GetComponent<UIFader>().FadeInWithScale(sheet, new Vector3(0.005f, 0.005f, 0.005f));


            sheet.transform.parent = transform.parent;

        }

        else if (display == false && isSelected == true)
        {
            displayingInfoSheet = false;
            foreach (Transform child in transform.parent)
            {
                if (child.tag == "infosheet")
                {
                    child.GetComponent<UIFader>().FadeOutWithScale(child.gameObject);
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
        mouseDownMousePosX = Input.mousePosition.x;
        mouseDownMousePosX = Input.mousePosition.y;
        mouseDownPixelPos = Input.GetTouch(0).position;

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

    private void FixedUpdate()
    {
        try
        {
            if (NewMolPos != Vector3.zero && NewMolPos != null) //TODO: add physics toggle compatibility.
            {
                Vector3 newPos = (NewMolPos - transform.position);
                Vector3 velocity = newPos * 1.0f / Time.fixedDeltaTime;
                molRigidBody.velocity = velocity;
            }

            if (MainController.enableVelocity == false)
            {
                molRigidBody.angularVelocity = Vector3.zero;
                molRigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            }

            else if (MainController.enableVelocity && collidingWithSurface == false)
            {
                molRigidBody.constraints = RigidbodyConstraints.None;
            }
        }

        catch (Exception e)
        {
            _ShowAndroidToastMessage(e.ToString());
        }
    }

    void OnMouseDrag()
    {
        //xDistance = DetectRotationAndPinch.fingerPoint.x * 15 * Mathf.Deg2Rad;
        //yDistance = DetectRotationAndPinch.fingerPoint.y * 15 * Mathf.Deg2Rad;

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

                Vector3 possibleNewMolPos = Camera.main.ScreenToWorldPoint(fingerPos);

                Vector3 fingerPosDelta =
                    new Vector3(distance.x + touchMovement.x,
                    distance.y + touchMovement.y, distance.z);

                Vector3 worldPos = Camera.main.ScreenToWorldPoint(fingerPosDelta);

                if (Vector3.Distance(possibleNewMolPos, worldPos) < 0.15) // cannot directly set to fingerPosDelta as molecule then does not move with camera.
                {
                    NewMolPos = possibleNewMolPos;
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
                RotateLeftRight(XaxisRotation, YaxisRotation);
            }
        }

        else if (userRotatingMolecule && Input.touchCount == 2)
        {
            DetectRotationAndPinch.Calculate();
            Quaternion desiredRotation = transform.rotation;

            if (Mathf.Abs(DetectRotationAndPinch.turnAngleDelta) > 0)
            { // rotate
                Vector3 rotationDeg = Vector3.zero;
                rotationDeg.z = -DetectRotationAndPinch.turnAngleDelta;
                pitchRotation = true;
                RotateZ(rotationDeg.z);
                totalZRotation += -rotationDeg.z;

                if ((totalZRotation < 0) != (DetectRotationAndPinch.turnAngleDelta < 0)) /// otherwise rotating one way then the other would give final force as 0, ie no spin.
                {
                    totalZRotation = 0;
                    rotationTime = 0;
                }


            }
        }
    }


    private MolRelDirection GetMolRelativeDirection()
    {
        Camera ARCam = MainController.FirstPersonCamera.GetComponent<Camera>();

        Vector3 relativeUp = ARCam.transform.TransformDirection(Vector3.up);
        Vector3 relativeRight = ARCam.transform.TransformDirection(Vector3.right);
        Vector3 molRelUp = transform.InverseTransformDirection(relativeUp);
        Vector3 molRelRight = transform.InverseTransformDirection(relativeRight);
        MolRelDirection molRelDirection = new MolRelDirection { molrelRight = molRelRight, molRelUp = molRelUp };
        return molRelDirection;

    }
    public void RotateLeftRight(float rotateLeftRight, float rotateUpDown)
    {
        float sensitivity = 10f;

        MolRelDirection molRelDirection = GetMolRelativeDirection();

        Quaternion rotateBy = Quaternion.AngleAxis(rotateLeftRight / gameObject.transform.localScale.x * sensitivity, molRelDirection.molRelUp)
            * Quaternion.AngleAxis(-rotateUpDown / gameObject.transform.localScale.x * sensitivity, molRelDirection.molrelRight);


        transform.Rotate(rotateBy.eulerAngles);
    }

    public void RotateZ(float ZRotation)
    {
        float sensitivity = 0.5f;

        Camera ARCam = MainController.FirstPersonCamera.GetComponent<Camera>();

        Vector3 relativeForward = ARCam.transform.TransformDirection(Vector3.forward);
        Vector3 molRelForward = transform.InverseTransformDirection(relativeForward);

        Quaternion rotateBy = Quaternion.AngleAxis(-ZRotation / gameObject.transform.localScale.x * sensitivity, molRelForward);
        transform.Rotate(rotateBy.eulerAngles);
    }

    public bool IsBetween(float num, float lower, float upper)
    {
        bool isBetween = false;
        return isBetween ? lower <= num && num <= upper : lower < num && num < upper;
    }

    public void StopMotion()
    {
        molRigidBody.velocity = Vector3.zero;
        molRigidBody.angularVelocity = Vector3.zero;
    }

    private void OnMouseUp()
    {
        Vector2 deltaFingerPos = Input.GetTouch(0).position - mouseDownPixelPos;
        float xDistance = Input.GetTouch(0).deltaPosition.x * 15 * Mathf.Deg2Rad;
        float yDistance = Input.GetTouch(0).deltaPosition.y * 15 * Mathf.Deg2Rad;
        //float mousePosX = Input.mousePosition.x;
        //float mousePosY = Input.mousePosition.y;

        //float xDis = Math.Abs(mouseDownMousePosX - mousePosX);
        //float yDis = Math.Abs(mouseDownMousePosY - mousePosY);

        if (MainController.enableVelocity && !userRotatingMolecule && !isScaling && Math.Abs(deltaFingerPos.x) > 0.5 && Math.Abs(deltaFingerPos.y) > 0.5)
        {
            NewMolPos = Vector3.zero; //so velocity not updated in late update
            Vector3 throwVector = transform.position - prevPos;
            float throwSpeed = throwVector.magnitude / Time.deltaTime;
            Vector3 throwVelocity = throwSpeed * throwVector.normalized;
            molRigidBody.velocity = throwVelocity;
        }

        if (userRotatingMolecule && MainController.enableVelocity && Math.Abs(deltaFingerPos.x) >  0.5 && Math.Abs(deltaFingerPos.y) > 0.5)
        {
            NewMolPos = Vector3.zero;
            molRigidBody.angularVelocity = Vector3.zero;

            if (pitchRotation)
            {
                float force = GenerateForce(totalZRotation, rotationTime) / 40;
                molRigidBody.AddTorque(0, 0, force);
            }

            else
            {

                MolRelDirection molRelDirection = GetMolRelativeDirection();

                molRigidBody.AddTorque((molRelDirection.molRelUp) * GenerateForce(xDistance, rotationTime));
                molRigidBody.AddRelativeTorque((molRelDirection.molrelRight) * GenerateForce(-yDistance, rotationTime));


            }

            pitchRotation = false;
            totalZRotation = 0;
            recordRotTime = false;
            rotationTime = 0;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!MainController.enableVelocity)
        {
            molRigidBody.velocity = Vector3.zero;
        }

        if (collision.gameObject.name == "ChemViewSurface" && NewMolPos != Vector3.zero) //is being dragged and colliding with surface
        {
           // _ShowAndroidToastMessage("Surface Collision");
            collidingWithSurface = true;
            molRigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.name == "ChemViewSurface")
        {
            collidingWithSurface = false;
            //_ShowAndroidToastMessage("Collision Left");
            //molRigidBody.constraints = RigidbodyConstraints.None;
        }
    }

    float GenerateForce(float distance, float time)
    {
        float vel = distance / time;
        float accel = vel / time;

        float force = molRigidBody.mass * accel;

        return force;
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

