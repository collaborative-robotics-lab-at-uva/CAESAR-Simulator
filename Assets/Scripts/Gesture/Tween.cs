using System;
using System.Collections;
using UnityEngine;

public static class Tween
{
    public static float GetEasedValue(float x, Easing easingFunction)
    {
        switch (easingFunction)
        {
            case Easing.Linear:
                return x; 
            case Easing.InSine:
                return 1 - Mathf.Cos((x * Mathf.PI) / 2);
            
            case Easing.OutSine:
                return Mathf.Sin((x * Mathf.PI) / 2);
            case Easing.InOutSine:
                
                return -(Mathf.Cos(Mathf.PI * x) - 1) / 2;
            case Easing.InQuad:
                
                return x * x;
            case Easing.OutQuad:
                
                return 1 - (1 - x) * (1 - x);
            case Easing.InOutQuad:
                
                return x < 0.5 ? 2 * x * x : 1 - Mathf.Pow(-2 * x + 2, 2) / 2;
            case Easing.InCubic:
               
                return x * x * x;
            case Easing.OutCubic:
                
                return 1 - Mathf.Pow(1 - x, 3);
            case Easing.InOutCubic:
                
                return x < 0.5 ? 4 * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 3) / 2;
            case Easing.InQuart:
                
                return x * x * x * x;
            case Easing.OutQuart:
               
                return 1 - Mathf.Pow(1 - x, 4);
            case Easing.InOutQuart:
                
                return x < 0.5 ? 8 * x * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 4) / 2;
            case Easing.InQuint:
                
                return x * x * x * x * x;
            case Easing.OutQuint:
                return 1 - Mathf.Pow(1 - x, 5);

            case Easing.InOutQuint:
                return x < 0.5 ? 16 * x * x * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 5) / 2;

            case Easing.InExpo:
                return x == 0 ? 0 : Mathf.Pow(2, 10 * x - 10);

            case Easing.OutExpo:
                return x == 1 ? 1 : 1 - Mathf.Pow(2, -10 * x);

            case Easing.InOutExpo:
                return x == 1 ? 1 : x < 0.5 ? Mathf.Pow(2, 20 * x - 10) / 2 : (2 - Mathf.Pow(2, -20 * x + 10)) / 2;

            case Easing.InCirc:
                return 1 - Mathf.Sqrt(1 - Mathf.Pow(x, 2));

            case Easing.OutCirc:
                return Mathf.Sqrt(1 - Mathf.Pow(x - 1, 2));

            case Easing.InOutCirc:
                return x < 0.5 ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * x, 2))) / 2

                : (Mathf.Sqrt(1 - Mathf.Pow(-2 * x + 2, 2)) + 1) / 2;
            case Easing.InBack:
                float c1 = 1.70158f;
                float c3 = c1 + 1;

                return c3 * x * x * x - c1 * x * x;

            case Easing.OutBack:
                float c4 = 1.70158f;
                float c5 = c4 + 1;
                return 1 + c4 * Mathf.Pow(x - 1, 3) + c5 * Mathf.Pow(x - 1, 2);

            case Easing.InOutBack:
                float c6 = 1.70158f;
                float c7 = c6 * 1.525f;

                return x < 0.5f ? (Mathf.Pow(2 * x, 2) * ((c6 + 1) * 2 * x - c7)) / 2
                    : (Mathf.Pow(2 * x - 2, 2) * ((c7 + 1) * (x * 2 - 2) + c7) + 2) / 2;

            case Easing.InElastic:
                float c10 = (2 * Mathf.PI) / 3;
                return x == 0
                    ? 0 : x == 1
                    ? 1 : -Mathf.Pow(2, 10 * x - 10) * Mathf.Sin((x * 10f - 10.75f) * c10);
            case Easing.OutElastic:
                float c11 = (2 * Mathf.PI) / 3;
                return x == 0
                    ? 0 : x == 1
                    ? 1 : -Mathf.Pow(2, 10 * x - 10) * Mathf.Sin((x * 10f - 10.75f) * c11);
            case Easing.InOutElastic:
                float c15 = (2f * Mathf.PI) / 4.5f;

                return x == 0
                    ? 0 : x == 1
                    ? 1 : x < 0.5f
                    ? -(Mathf.Pow(2, 20 * x - 10) * Mathf.Sin((20f * x - 11.125f) * c15)) / 2
                    : (Mathf.Pow(2, -20 * x + 10) * Mathf.Sin((20f * x - 11.125f) * c15)) / 2 + 1;

            case Easing.InBounce:
                return -easeOutBounce(x);

            case Easing.OutBounce:
                return easeOutBounce(x);

                
            case Easing.InOutBounce:

                return x < 0.5
                    ? (1 - easeOutBounce(1 - 2 * x)) / 2
                    : (1 + easeOutBounce(2 * x - 1)) / 2;
            default:
                return x;
        }
    }
    private static float easeOutBounce(float x)
    {
        float n1 = 7.5625f;
        float d1 = 2.75f;

        if (x< 1 / d1) {
            return n1* x * x;
        } else if (x< 2 / d1) {
            return n1* (x -= 1.5f / d1) * x + 0.75f;
        } else if (x< 2.5 / d1) {
            return n1* (x -= 2.25f / d1) * x + 0.9375f;
        } else {
            return n1* (x -= 2.625f / d1) * x + 0.984375f;
        }
    }

    public enum Easing
    {
        Linear,
        InSine,
        OutSine,
        InOutSine,
        InQuad,
        OutQuad,
        InOutQuad,
        InCubic,
        OutCubic,
        InOutCubic,
        InQuart,
        OutQuart,
        InOutQuart,
        InQuint,
        OutQuint,
        InOutQuint,
        InExpo,
        OutExpo,
        InOutExpo,
        InCirc,
        OutCirc,
        InOutCirc,
        InBack,
        OutBack,
        InOutBack,
        InElastic,
        OutElastic,
        InOutElastic,
        InBounce,
        OutBounce,
        InOutBounce,
    }


    /// <summary>
    /// Part of the Optional Tweening portion. Implement this function to automatically change a passed in value.
    /// To call this function, it will look like
    /// float x;
    /// StartCoroutine(Tween.ExecuteCoroutine((result) => x = result, startPosition.x, endPosition.x, time, Tween.Easing.Linear));
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="time"></param>
    /// <param name="easingFunction"></param>
    /// <returns></returns>

    public static IEnumerator ExecuteCoroutine(Action<float> callback, float start, float end, float time, Easing easingFunction)
    {
        float timeelapsed = 0;
        float dif = end-start;
        while (timeelapsed < time){
            timeelapsed += Time.deltaTime;
            start += GetEasedValue(start, easingFunction);
            callback(start);
            yield return null;
        }
        
        
    }

}
