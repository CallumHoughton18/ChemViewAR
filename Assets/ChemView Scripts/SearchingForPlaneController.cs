using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchingForPlaneController : MonoBehaviour
{

    public Text searchingText;
    bool _showDialog = false;
    private float _visibleYPos;
    private float _hiddenYPos;
    private float _movementIncrement;
    List<string> searchingMessages;
    int i = 0;
    void Start()
    {
        searchingMessages = new List<string> { "Searching for Plane", "Searching for Plane.", "Searching for Plane..", "Searching for Plane..." };
        StartMessageCoroutine();
        _visibleYPos = transform.position.y;
        RectTransform rt = GetComponent<RectTransform>();
        _hiddenYPos = _visibleYPos - rt.rect.height / 2;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_showDialog && gameObject.transform.position.y >= _hiddenYPos)
        {
            gameObject.transform.position = new Vector3(transform.position.x, transform.position.y - _movementIncrement, transform.position.z);

            //if (gameObject.transform.position.y >= _hiddenYPos)
            //    gameObject.SetActive(false);
        }
        else if (_showDialog && gameObject.transform.position.y <= _visibleYPos)
        {
            //gameObject.SetActive(true);
            gameObject.transform.position = new Vector3(transform.position.x, transform.position.y + _movementIncrement, transform.position.z);
        }
    }

    public void MovePosition(bool showDialog)
    {

        if (showDialog)
        {
            _movementIncrement = Mathf.Abs((_visibleYPos - transform.position.y) / 3);
        }

        else
        {
            _movementIncrement = Mathf.Abs((_hiddenYPos - transform.position.y) / 3);
        }

        _showDialog = showDialog;

    }

    public void StartMessageCoroutine()
    {
        searchingText.text = searchingMessages[0];
        StartCoroutine(SetText());



    }

    IEnumerator SetText()
    {
        while (gameObject.activeSelf)
        {
            string message = searchingMessages[i];

            searchingText.text = message;
            i++;

            if (i > searchingMessages.Count - 1)
                i = 0;

            yield return new WaitForSeconds(1f);
        }
    }
}
