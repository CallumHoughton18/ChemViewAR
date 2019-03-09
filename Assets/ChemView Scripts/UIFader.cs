using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFader : MonoBehaviour {

    public CanvasGroup UICanvasGroup;

    public void FadeIn()
    {
        StartCoroutine(FadeCanvasGroup(UICanvasGroup, UICanvasGroup.alpha, 1));
        UICanvasGroup.interactable = true;
    }

    public void FadeOut()
    {
        UICanvasGroup.interactable = false;
        StartCoroutine(FadeCanvasGroup(UICanvasGroup, UICanvasGroup.alpha, 0));
    }

    public void FadeInWithScale(GameObject gameObject, Vector3 newScale)
    {
        StartCoroutine(FadeCanvasGroup(UICanvasGroup, UICanvasGroup.alpha, 1));
        StartCoroutine(ScaleGameObject(gameObject, new Vector3(0,0,0), newScale));
        UICanvasGroup.interactable = true;
    }

    public void FadeOutWithScale(GameObject gameObject)
    {
        UICanvasGroup.interactable = false;
        StartCoroutine(FadeCanvasGroup(UICanvasGroup, UICanvasGroup.alpha, 0));
        StartCoroutine(ScaleGameObject(gameObject, gameObject.transform.localScale, new Vector3(0, 0, 0)));
    }

    public IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float start, float end, float lerpTime = 0.5f)
    {
        float _timeStartedLerping = Time.time;
        float timeSinceStarted = Time.time - _timeStartedLerping;
        float percentageComplete = timeSinceStarted / lerpTime;

        while (true)
        {
            timeSinceStarted = Time.time - _timeStartedLerping;
            percentageComplete = timeSinceStarted / lerpTime;

            float currentValue = Mathf.Lerp(start, end, percentageComplete);

            canvasGroup.alpha = currentValue;

            if (percentageComplete >= 1) break;

            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator ScaleGameObject(GameObject gameObject, Vector3 originalScale, Vector3 newScale, float lerpTime = 0.5f)
    {
        float currentTime = 0.0f;

        do
        {
            gameObject.transform.localScale = Vector3.Lerp(originalScale, newScale, currentTime / lerpTime);
            currentTime += Time.deltaTime;
            yield return null;
        } while (currentTime <= lerpTime);
    }
}
