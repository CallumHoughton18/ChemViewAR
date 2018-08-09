using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoleculeController : MonoBehaviour {

    float speed = 100;

    Vector3 distance;
    float xPos;
    float yPos;

    public float initialFingersDistance;
    public Vector3 initialScale;
    public Vector3 initialHaloScale;
    public static Transform ScaleTransform;

    float f_lastX = 0.0f;
    float f_difX = 0.5f;
    float f_steps = 0.0f;
    int i_direction = 1;

    float f_lastY = 0.0f;
    float f_difY = 0.5f;

    public bool isSelected = false;
    public bool rotateMolecule = false;
    public bool userRotatingMolecule = false;

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

        if (userRotatingMolecule == true)
        {
            UserRotation();
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

    }

    public void RotateMolecule()
    {
        transform.Rotate(Vector3.up, speed * Time.deltaTime);  
    }

    public void UserRotation()
    {
        Highlight();
        if (Input.GetMouseButtonDown(0))
        {
            f_difX = 0.0f;
            f_difY = 0.0f;
        }
        else if (Input.GetMouseButton(0))
        {
            f_difX = Mathf.Abs(f_lastX - Input.GetAxis("Mouse X"));
            f_difY = Mathf.Abs(f_lastY - Input.GetAxis("Mouse Y"));

            if (f_lastX < Input.GetAxis("Mouse X"))
            {
                i_direction = -1;
                transform.Rotate(Vector3.up, -f_difX);
            }

            if (f_lastX > Input.GetAxis("Mouse X"))
            {
                i_direction = 1;
                transform.Rotate(Vector3.up, f_difX);
            }

            if (f_lastY < Input.GetAxis("Mouse Y"))
            {
                i_direction = -1;
                transform.Rotate(Vector3.left);
            }

            if (f_lastY > Input.GetAxis("Mouse Y"))
            {
                i_direction = 1;
                transform.Rotate(Vector3.left);
            }

            f_lastX = -Input.GetAxis("Mouse X");
            f_lastY = -Input.GetAxis("Mouse Y");
        }
        else
        {
            if (f_difX > 0.5f) f_difX -= 0.05f;
            if (f_difX < 0.5f) f_difX += 0.05f;

            transform.Rotate(Vector3.up, f_difX * i_direction);

            if (f_difY > 0.5f) f_difY -= 0.05f;
            if (f_difY < 0.5f) f_difY += 0.05f;

            transform.Rotate(Vector3.left, f_difY * i_direction);
        }
    }

}

