using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchingForPlaneController : MonoBehaviour
{

    public Text searchingText;
    List<string> searchingMessages;
    int i = 0;
    void Start()
    {
        searchingMessages = new List<string> { "Searching for Plane", "Searching for Plane.", "Searching for Plane..", "Searching for Plane..." };
        StartMessageCoroutine();
    }

    // Update is called once per frame
    void Update()
    {

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
