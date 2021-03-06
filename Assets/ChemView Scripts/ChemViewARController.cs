﻿using System;
using System.Collections.Generic;
using System.Linq;
using GoogleARCore;
using GoogleARCore.Examples.Common;
using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

/// <summary>
/// Controls the HelloAR example.
/// </summary>
public class ChemViewARController : MonoBehaviour
{

    /// <summary>
    /// The first-person camera being used to render the passthrough camera image (i.e. AR background).
    /// </summary>
    public Camera FirstPersonCamera;

    public MoleculeController selectedMol;
    private MoleculeController _lastSelectedMol;

    public GameObject UICanvas;
    public GameObject ControlsCanvas;
    private UIController uIController;

    private bool prevDoubleTap = false;
    private bool firstSurface = true;

    /// <summary>
    /// A prefab for tracking and visualizing detected planes.
    /// </summary>
    public GameObject DetectedPlanePrefab;

    /// <summary>
    /// A model to place when a raycast from a user touch hits a plane.
    /// </summary>
    public GameObject loadedChemModel;

    /// <summary>
    /// A gameobject parenting UI for displaying the "searching for planes" snackbar.
    /// </summary>
    public GameObject SearchingForPlaneUI;

    /// <summary>
    /// The rotation in degrees need to apply to model when the model is placed.
    /// </summary>
    private const float k_ModelRotation = 180.0f;

    /// <summary>
    /// A list to hold all planes ARCore is tracking in the current frame. This object is used across
    /// the application to avoid per-frame allocations.
    /// </summary>
    private List<DetectedPlane> m_AllPlanes = new List<DetectedPlane>();

    /// <summary>
    /// True if the app is in the process of quitting due to an ARCore connection error, otherwise false.
    /// </summary>
    private bool m_IsQuitting = false;

    public bool rotateSelectedMolecule;

    public bool UserRotating = false;

    public bool enableVelocity;


    public void MoleculePhysicsToggle(bool newValue)
    {
        enableVelocity = newValue;

        if (enableVelocity && selectedMol.displayingInfoSheet)
        {
            uIController.TurnOffToggle(2);
            selectedMol.DisplayInfoSheet(FirstPersonCamera, false);
        }

        else
        {
            selectedMol.StopMotion();
        }
    }

    public void MoleculeUserRotationToggle(bool newValue)
    {
        if (selectedMol != null)
        {
            MoleculeController selectedMolScript = selectedMol.GetComponentInChildren<MoleculeController>();
            selectedMolScript.userRotatingMolecule = newValue;
            UserRotating = newValue;
        }

    }

    public void DisplayMolInfoSheet(bool newValue)
    {
        if (selectedMol != null)
        {
            if (enableVelocity && newValue)
            {
                _ShowAndroidToastMessage("Disable physics to show information sheet!");
                uIController.TurnOffToggle(2);
            }


            else if (enableVelocity == false)
            {
                MoleculeController selectedMolScript = selectedMol.GetComponentInChildren<MoleculeController>();
                selectedMolScript.DisplayInfoSheet(FirstPersonCamera, newValue);
            }
        }
    }

    public void Start()
    {
        uIController = ControlsCanvas.GetComponent<UIController>();

        Shader.SetGlobalColor("_OutlineColor", Color.green);
        Shader.SetGlobalFloat("_Outline", 0.003f);

        uIController.SearchingForPlanes = true;

    }

    public void Update()
    {
        _UpdateApplicationLifecycle();

        // Hide snackbar when currently tracking at least one plane.
        Session.GetTrackables<DetectedPlane>(m_AllPlanes);
        bool showSearchingUI = true;
        for (int i = 0; i < m_AllPlanes.Count; i++)
        {
            if (m_AllPlanes[i].TrackingState == TrackingState.Tracking)
            {
                showSearchingUI = false;
                break;
            }
        }


        if (showSearchingUI != uIController.SearchingForPlanes)
        {
            if (firstSurface == true)
            {
                ChemviewHelper.ShowAndroidToastMessage("Double tap on a surface to spawn a molecule");
                firstSurface = false;
            }

            uIController.SearchingForPlanes = showSearchingUI;
            SearchingForPlaneUI.GetComponent<SearchingForPlaneController>().MovePosition(showSearchingUI);

            if (showSearchingUI)
            {
                SearchingForPlaneUI.GetComponent<SearchingForPlaneController>().StartMessageCoroutine();
                uIController.FadeOut();
            }

            else
            {
                uIController.FadeIn();
            }
        }

        Touch touch;

        // If the player has not touched the screen, we are done with this update.
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }


        if (touch.tapCount == 1 && UserRotating == false)
        {
            if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                Ray raycast = FirstPersonCamera.ScreenPointToRay(touch.position);
                RaycastHit raycastHit;
                if (Physics.Raycast(raycast, out raycastHit))
                {

                    if (raycastHit.collider.tag == "Molecule" && UserRotating == false)
                    {
                        if (selectedMol == null)
                        {
                            selectedMol = raycastHit.collider.GetComponentInChildren<MoleculeController>();
                            _lastSelectedMol = selectedMol;
                            selectedMol.isSelected = true;
                            selectedMol.Highlight();
                            uIController.SetToggles(selectedMol);
                        }

                        else
                        {
                            if (selectedMol != raycastHit.collider.GetComponentInChildren<MoleculeController>())
                            {
                                selectedMol.Dehighlight();
                                UserRotating = false;
                                uIController.TurnOffToggles();

                                selectedMol = raycastHit.collider.GetComponentInChildren<MoleculeController>();
                                _lastSelectedMol = selectedMol;
                                selectedMol.isSelected = true;
                                selectedMol.Highlight();
                                uIController.SetToggles(selectedMol);
                            }
                        }
                    }
                }
                else
                {
                    if (selectedMol != null && UserRotating == false)
                    {
                        selectedMol.Dehighlight();
                        MoleculeController selectedMolScript = selectedMol.GetComponentInChildren<MoleculeController>();
                        selectedMolScript.isSelected = false;
                        selectedMol = null;
                        uIController.TurnOffToggles();
                        UserRotating = false;


                    }
                }
                return;
            }
        }

        if (touch.tapCount == 2)
        {
            if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId) && UserRotating == false)
            {
                SpawnMolecule();
                return;
            }
        }

    }

    /// <summary>
    /// Check and update the application lifecycle.
    /// </summary>
    private void _UpdateApplicationLifecycle()
    {
        // Exit the app when the 'back' button is pressed.
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        // Only allow the screen to sleep when not tracking.
        if (Session.Status != SessionStatus.Tracking)
        {
            const int lostTrackingSleepTimeout = 15;
            Screen.sleepTimeout = lostTrackingSleepTimeout;
        }
        else
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        if (m_IsQuitting)
        {
            return;
        }

        // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            _ShowAndroidToastMessage("Camera permission is needed to run this application.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
        else if (Session.Status.IsError())
        {
            _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
    }

    /// <summary>
    /// Actually quit the application.
    /// </summary>
    private void _DoQuit()
    {
        Application.Quit();
    }

    public void DeleteSelectedMolecule()
    {

        if (_lastSelectedMol != null)
        {
            uIController.TurnOffToggles();
            _lastSelectedMol.Dehighlight();
            _lastSelectedMol.Destroy();

        }

    }

    public void DeleteAllMolecules()
    {
        _ShowAndroidToastMessage("Destroying all molecules");
        List<GameObject> mols = GameObject.FindGameObjectsWithTag("Molecule").Where(x => x.transform.parent.name.ToLower().Contains("anchor")).ToList();

        foreach (var mol in mols)
        {
            Destroy(mol);
        }

    }

    private void SpawnMolecule()
    {
        try
        {
            Touch touch = Input.GetTouch(0);
            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                TrackableHitFlags.FeaturePointWithSurfaceNormal;

            if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
            {
                // Use hit pose and camera pose to check if hittest is from the
                // back of the plane, if it is, no need to create the anchor.
                if ((hit.Trackable is DetectedPlane) &&
                    Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position,
                        hit.Pose.rotation * Vector3.up) < 0)
                {
                    Debug.Log("Hit at back of the current DetectedPlane");
                }
                else
                {
                    // Instantiate chemical model at the hit pose.
                    var molObj = Instantiate(loadedChemModel, hit.Pose.position, hit.Pose.rotation);
                    molObj.SetActive(true);
                    molObj.GetComponentInChildren<MoleculeController>().enabled = true;
                    molObj.transform.GetChild(0).position = hit.Pose.position;

                    Collider[] myColliders = molObj.GetComponentsInChildren<Collider>();
                    Bounds moleculeBounds = new Bounds(transform.position, Vector3.zero);
                    foreach (Collider nextCollider in myColliders)
                    {
                        moleculeBounds.Encapsulate(nextCollider.bounds);
                    }

                    molObj.GetComponentInChildren<MoleculeController>().moleculeBounds = moleculeBounds;

                    molObj.transform.Translate(0, molObj.GetComponentInChildren<MoleculeController>().moleculeBounds.extents.y, 0, Space.World);

                    // Compensate for the hitPose rotation facing away from the raycast (i.e. camera).
                    molObj.transform.Rotate(0, k_ModelRotation, 0, Space.Self);

                    //rotate the loaded molecule

                    // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
                    // world evolves.
                    var anchor = hit.Trackable.CreateAnchor(hit.Pose);

                    // Make molecule model a child of the anchor.
                    molObj.transform.parent = anchor.transform;

                    if (selectedMol != null)
                    {
                        selectedMol.Dehighlight();
                    }

                    MoleculeController selectedMolScript = molObj.GetComponentInChildren<MoleculeController>();
                    selectedMolScript.planePosition = anchor.transform.position;
                    selectedMolScript.Highlight();
                    selectedMol = selectedMolScript;
                    _lastSelectedMol = selectedMol;
                    uIController.SetToggles(selectedMol);

                }
            }
        }

        catch (Exception e)
        {
            _ShowAndroidToastMessage(e.ToString());
        }

    }

    /// <summary>
    /// Show an Android toast message.
    /// </summary>
    /// <param name="message">Message string to show in the toast.</param>
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

