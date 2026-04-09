using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class MainMenuTipRotator : MonoBehaviour
{
    private enum TipOrder
    {
        Ascending = 0,
        Descending = 1,
    }

    [Header("Output")]
    [Tooltip("TMP text to overwrite. If null, uses the TMP_Text on this GameObject.")]
    [SerializeField] private TMP_Text targetText;

    [Tooltip("Tips/phrases to rotate through (in order, looping).")]
    [SerializeField] private List<string> tips = new List<string>();

    [Tooltip("Order to play tips.")]
    [SerializeField] private TipOrder order = TipOrder.Descending;

    [Header("Timing")]
    [Tooltip("Seconds before the first tip is shown.")]
    [Min(0f)]
    [SerializeField] private float initialDelaySeconds = 0f;

    [Tooltip("Seconds between tips.")]
    [Min(0.1f)]
    [SerializeField] private float intervalSeconds = 20f;

    [Tooltip("If true, uses unscaled time (works even if Time.timeScale = 0).")]
    [SerializeField] private bool useUnscaledTime = true;

    private Coroutine loop;
    private int index = -1;

    private void Awake()
    {
        if (targetText == null)
        {
            targetText = GetComponent<TMP_Text>();
        }
    }

    private void OnEnable()
    {
        if (loop == null)
        {
            loop = StartCoroutine(Loop());
        }
    }

    private void OnDisable()
    {
        if (loop != null)
        {
            StopCoroutine(loop);
            loop = null;
        }
    }

    private IEnumerator Loop()
    {
        if (initialDelaySeconds > 0f)
        {
            if (useUnscaledTime) yield return new WaitForSecondsRealtime(initialDelaySeconds);
            else yield return new WaitForSeconds(initialDelaySeconds);
        }

        while (true)
        {
            ShowNextTip();

            float wait = Mathf.Max(0.1f, intervalSeconds);
            if (useUnscaledTime) yield return new WaitForSecondsRealtime(wait);
            else yield return new WaitForSeconds(wait);
        }
    }

    private void ShowNextTip()
    {
        if (targetText == null) return;
        if (tips == null || tips.Count == 0) return;

        if (index < 0)
        {
            // First tip shown.
            index = order == TipOrder.Descending ? tips.Count - 1 : 0;
        }
        else
        {
            if (order == TipOrder.Descending)
            {
                index--;
                if (index < 0) index = tips.Count - 1;
            }
            else
            {
                index++;
                if (index >= tips.Count) index = 0;
            }
        }

        targetText.text = tips[index] ?? string.Empty;
    }
}
