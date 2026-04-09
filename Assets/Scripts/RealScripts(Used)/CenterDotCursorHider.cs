using UnityEngine;
using UnityEngine.UI;

public class CenterDotCursorHider : MonoBehaviour
{
    [Header("Cursor")]
    [SerializeField] private bool hideCursor = true;
    [SerializeField] private bool lockCursor = true;

    [Tooltip("If true, shows the cursor and hides the dot while PauseMenuUI.IsPaused is true.")]
    [SerializeField] private bool respectPauseMenu = true;

    [Header("Center Dot")]
    [SerializeField] private bool showCenterDot = true;
    [SerializeField] private float dotSizePixels = 4f;
    [SerializeField] private Color dotColor = Color.red;

    [Tooltip("Optional. If null, a Screen Space Overlay canvas will be created.")]
    [SerializeField] private Canvas canvas;

    private Image dotImage;

    private void Awake()
    {
        EnsureDot();
        ApplyState(force: true);
    }

    private void OnEnable()
    {
        ApplyState(force: true);
    }

    private void OnDisable()
    {
        // Best-effort restore.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (dotImage != null)
        {
            dotImage.enabled = false;
        }
    }

    private void Update()
    {
        ApplyState(force: false);
    }

    private void ApplyState(bool force)
    {
        bool paused = false;
        if (respectPauseMenu)
        {
            paused = PauseMenuUI.IsPaused;
        }

        // Fallback: pausing often sets timeScale=0; ensure UI remains usable.
        if (!paused && Time.timeScale == 0f)
        {
            paused = true;
        }

        bool shouldHideCursor = hideCursor && !paused;
        bool shouldLockCursor = lockCursor && !paused;
        bool shouldShowDot = showCenterDot && !paused;

        Cursor.visible = !shouldHideCursor;
        Cursor.lockState = shouldLockCursor ? CursorLockMode.Locked : CursorLockMode.None;

        if (dotImage != null)
        {
            dotImage.enabled = shouldShowDot;
            dotImage.color = dotColor;
            dotImage.rectTransform.sizeDelta = Vector2.one * Mathf.Max(1f, dotSizePixels);
        }
    }

    private void EnsureDot()
    {
        if (!showCenterDot) return;

        if (canvas == null)
        {
            Canvas existing = FindFirstObjectByType<Canvas>();
            if (existing != null && existing.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                canvas = existing;
            }
            else
            {
                GameObject canvasGO = new GameObject("CenterDotCanvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = short.MaxValue;

                CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                canvasGO.AddComponent<GraphicRaycaster>();

                DontDestroyOnLoad(canvasGO);
            }
        }

        if (dotImage != null) return;

        GameObject dotGO = new GameObject("CenterDot");
        dotGO.transform.SetParent(canvas.transform, false);

        RectTransform rt = dotGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.one * Mathf.Max(1f, dotSizePixels);

        dotImage = dotGO.AddComponent<Image>();
        dotImage.color = dotColor;
        dotImage.raycastTarget = false;

        // Default Unity UI sprite is fine for a dot.
        dotImage.type = Image.Type.Simple;
    }
}
