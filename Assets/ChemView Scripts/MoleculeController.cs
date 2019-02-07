using Assets.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using UnityEngine;
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
    float xPos;
    float yPos;
    float xDistance;
    float yDistance;

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
        molRigidBody = transform.GetComponent<Rigidbody>();
        molRigidBody.maxAngularVelocity = 15;
        collider = GetComponent<Collider>();

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
                sheet.GetComponent<UIFader>().FadeInWithScale(sheet,new Vector3(0.005f, 0.005f, 0.005f));


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
        DetectRotationAndPinch.Calculate();
        xDistance = DetectRotationAndPinch.fingerPoint.x * 15 * Mathf.Deg2Rad;
        yDistance = DetectRotationAndPinch.fingerPoint.y * 15 * Mathf.Deg2Rad;

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
                RotateLeftRight(XaxisRotation, YaxisRotation);
            }
        }

        else if (userRotatingMolecule && Input.touchCount == 2)
        {
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

    public void RotateLeftRight(float rotateLeftRight, float rotateUpDown)
    {
        float sensitivity = 10f;

        Camera ARCam = MainController.FirstPersonCamera.GetComponent<Camera>();

        Vector3 distance = ARCam.transform.position - gameObject.transform.position;

        //float angleRadius = Vector3.SignedAngle(distance, gameObject.transform.position, Vector3.forward);
        //float angleRadius = Vector2.SignedAngle(new Vector2(distance.x, distance.y), new Vector2(gameObject.transform.position.x, gameObject.transform.position.y));

        Vector3 relativeUp = ARCam.transform.TransformDirection(Vector3.up);
        Vector3 relativeRight = ARCam.transform.TransformDirection(Vector3.right);
        Vector3 molRelUp = transform.InverseTransformDirection(relativeUp);

        Vector3 molRelRight = transform.InverseTransformDirection(relativeRight);
        Quaternion rotateBy = Quaternion.AngleAxis(rotateLeftRight / gameObject.transform.localScale.x * sensitivity, molRelUp)
            * Quaternion.AngleAxis(-rotateUpDown / gameObject.transform.localScale.x * sensitivity, molRelRight);


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

        //float angle = Vector3.Angle(MainController.FirstPersonCamera.GetComponent<Camera>().transform.forward, transform.forward);
        //float angleRadius = Vector2.SignedAngle(new Vector2(distance.x, distance.y), new Vector2(gameObject.transform.position.x, gameObject.transform.position.y));

        //if (IsBetween(angle, 110, 180)) //behind
        //{
        //    rotateLeftRightSign= 1;
        //    rotateUpDownSign = -1;
        //}

        //else if (IsBetween(angle, 50, 110)) //sides
        //{
        //    rotateLeftRightSign= 1;
        //    rotateUpDownSign= 1;
        //    _ShowAndroidToastMessage(angle.ToString());
        //}

        //else if (IsBetween(angle, 0, 50)) //in front
        //{
        //    rotateLeftRightSign= 1;
        //    rotateUpDownSign= -1;
        //}

        //else
        //{
        //    rotateLeftRightSign= -1;
        //    rotateUpDownSign= 1;
        //}
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

