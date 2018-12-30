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
}
