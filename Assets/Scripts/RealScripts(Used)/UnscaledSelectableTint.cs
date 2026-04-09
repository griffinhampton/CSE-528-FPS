using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UnscaledSelectableTint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Tooltip("If true, disables the built-in Selectable transition so this script is the only tint driver.")]
    [SerializeField] private bool disableBuiltinTransitions = true;

    private Selectable selectable;
    private Graphic target;

    private Color normal;
    private Color highlighted;
    private Color pressed;
    private Color disabled;
    private float fadeDuration;

    private Color current;
    private Color targetColor;

    private bool isHovered;
    private bool isPressed;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
        if (selectable == null)
        {
            enabled = false;
            return;
        }

        target = selectable.targetGraphic;
        if (target == null)
        {
            enabled = false;
            return;
        }

        ColorBlock cb = selectable.colors;
        normal = cb.normalColor;
        highlighted = cb.highlightedColor;
        pressed = cb.pressedColor;
        disabled = cb.disabledColor;
        fadeDuration = Mathf.Max(0f, cb.fadeDuration);

        current = target.color;
        targetColor = ResolveTarget();

        if (disableBuiltinTransitions)
        {
            selectable.transition = Selectable.Transition.None;
        }
    }

    private void OnEnable()
    {
        if (target != null)
        {
            targetColor = ResolveTarget();
            current = target.color;
        }
    }

    private void Update()
    {
        if (target == null || selectable == null) return;

        targetColor = ResolveTarget();

        float t = fadeDuration <= 0f ? 1f : (Time.unscaledDeltaTime / fadeDuration);
        current = Color.Lerp(current, targetColor, Mathf.Clamp01(t));
        target.color = current;
    }

    private Color ResolveTarget()
    {
        if (selectable == null || !selectable.IsInteractable()) return disabled;
        if (isPressed) return pressed;
        if (isHovered) return highlighted;
        return normal;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        isPressed = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (selectable != null && selectable.IsInteractable())
        {
            isPressed = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }
}
