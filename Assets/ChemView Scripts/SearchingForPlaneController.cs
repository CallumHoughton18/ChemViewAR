using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchingForPlaneController : MonoBehaviour
{

    public Text searchingText;
    bool _showDialog = true;
    private float _visibleYPos;
    private float _hiddenYPos;
    private float _movementIncrement;
    List<string> searchingMessages;
    int messageIndex = 0;
    void Start() //TOOD: add way to cancel couritine
    {
        searchingMessages = new List<string> { "Searching for Plane", "Searching for Plane.", "Searching for Plane..", "Searching for Plane...", "Point at a Flat Surface to Generate a Plane!" };
        StartMessageCoroutine();
        RectTransform rt = GetComponent<RectTransform>();
        _visibleYPos = rt.transform.position.y + 5;
        transform.position = new Vector3(transform.position.x, _visibleYPos, transform.position.z);
        _hiddenYPos = _visibleYPos - rt.rect.height / 2;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_showDialog && gameObject.transform.position.y >= _hiddenYPos)
        {
            gameObject.transform.position = new Vector3(transform.position.x, transform.position.y - _movementIncrement, transform.position.z);

            if (!_showDialog && gameObject.transform.position.y <= _hiddenYPos)
            {
                gameObject.SetActive(false);
            }
        }
        else if (_showDialog && gameObject.transform.position.y < _visibleYPos)
        {
            gameObject.transform.position = new Vector3(transform.position.x, transform.position.y + _movementIncrement, transform.position.z);
        }

        else if (_showDialog)
        {
            {
                gameObject.transform.position = new Vector3(transform.position.x, _visibleYPos, transform.position.z);
            }
        }
    }

    public void MovePosition(bool showDialog)
    {

        if (showDialog)
        {
            gameObject.SetActive(true);
            _movementIncrement = Mathf.Abs((_visibleYPos - transform.position.y) / 3); //not completely at position it was originally in still?
            messageIndex = 0;
            //StartMessageCoroutine();
        }

        else
        {
            _movementIncrement = Mathf.Abs((_hiddenYPos - transform.position.y) / 3);
            //StopCoroutine("SetText");
        }

        _showDialog = showDialog;

    }

    public void StartMessageCoroutine()
    {
        searchingText.text = searchingMessages[0];
        StartCoroutine("SetText");

    }

    IEnumerator SetText()
    {
        while (_showDialog)
        {
            string message = searchingMessages[messageIndex];

            searchingText.text = message;
            messageIndex++;

            if (messageIndex > searchingMessages.Count - 1)
            {
                messageIndex = 0;
                searchingText.alignment = TextAnchor.MiddleCenter;
                yield return new WaitForSeconds(3f);
            }

            else
            {
                searchingText.alignment = TextAnchor.MiddleLeft;
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
