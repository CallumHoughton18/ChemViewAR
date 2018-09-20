﻿using Assets.Models;
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

    public float initialFingersDistance;
    public Vector3 initialScale;
    public Vector3 initialHaloScale;
    public static Transform ScaleTransform;

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

    // Use this for initialization
    IEnumerator Start()
    {

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


            if (fingersOnScreen == 2 && isSelected == true) //enable 'pinch' motion
            {
                isScaling = true;
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

    public void DisplayInfoSheet(Camera player, bool display)
    {
        if (display == true && displayingInfoSheet == false && isSelected == true)
        {
            displayingInfoSheet = true;

            try
            {
                Vector3 placement = new Vector3(player.transform.position.x + 0.8f, player.transform.position.y, player.transform.position.z + 1.5f);
                var sheet = Instantiate(molInfoSheet, placement, transform.rotation);

                sheet.transform.LookAt(player.transform);
                sheet.transform.Rotate(0, 180, 0);

                sheet.transform.parent = transform;

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
            foreach (Transform child in transform)
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

    }

    void OnMouseDrag()
    {


        if (Input.touchCount == 1 && isSelected == true) //factor in camera movement to molecule movement also
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
                transform.position = fingerPosWorld;

        }


        if (userRotatingMolecule == true && isSelected == true)
        {
            float hor = 5 * Input.GetAxis("Mouse X");
            float ver = 5 * Input.GetAxis("Mouse Y");

            transform.Rotate(ver, -hor, 0);
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

