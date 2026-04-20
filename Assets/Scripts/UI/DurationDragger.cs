using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DurationDragger : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public TextMeshProUGUI label;
    public float sensitivity = 0.1f;
    public float stepSize = 5f;
    public bool snapToStep = true;

    private float accumulatedDelta = 0f;
    private bool isDragging = false;
    private Vector3 originalScale;
    private Color originalColor;

    void Start()
    {
        if (label == null) label = GetComponent<TextMeshProUGUI>();
        if (label != null)
        {
            originalScale = label.transform.localScale;
            originalColor = label.color;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (FocusSystem.Instance == null || FocusSystem.Instance.IsRunning) return;
        isDragging = true;
        accumulatedDelta = 0f;
        if (label != null)
        {
            label.transform.localScale = originalScale * 1.15f;
            label.color = new Color(1f, 0.9f, 0.3f, 1f);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || FocusSystem.Instance == null || FocusSystem.Instance.IsRunning) return;
        accumulatedDelta += eventData.delta.x * sensitivity;
        if (snapToStep)
        {
            if (Mathf.Abs(accumulatedDelta) >= stepSize)
            {
                float steps = Mathf.Floor(Mathf.Abs(accumulatedDelta) / stepSize) * Mathf.Sign(accumulatedDelta);
                float snappedCurrent = Mathf.Round(FocusSystem.Instance.focusDurationMinutes / stepSize) * stepSize;
                float newDuration = snappedCurrent + steps * stepSize;
                FocusSystem.Instance.SetDuration(newDuration);
                accumulatedDelta -= steps * stepSize;
                UpdateLabel();
            }
        }
        else
        {
            float newDuration = FocusSystem.Instance.focusDurationMinutes + accumulatedDelta;
            FocusSystem.Instance.SetDuration(newDuration);
            accumulatedDelta = 0f;
            UpdateLabel();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        if (label != null)
        {
            label.transform.localScale = originalScale;
            label.color = originalColor;
        }
    }

    void UpdateLabel()
    {
        if (label != null && FocusSystem.Instance != null)
            label.text = Mathf.FloorToInt(FocusSystem.Instance.focusDurationMinutes) + " min";
    }
}
