using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
[RequireComponent(typeof(PoisonStatus))]
public class PoisonStatusView : MonoBehaviour
{
    private static readonly int ZTestModeId = Shader.PropertyToID("_ZTestMode");

    [Header("Aura")]
    [SerializeField] private Sprite auraSprite;
    [SerializeField] private Color auraColor = new Color(0.25f, 1f, 0.12f, 0.18f);
    [SerializeField, Min(0.1f)] private float auraDiameter = 1.5f;
    [SerializeField, Min(0f)] private float pulseAmount = 0.12f;
    [SerializeField, Min(0.1f)] private float pulseSpeed = 3f;

    [Header("Status Label")]
    [SerializeField] private Vector3 labelOffset = new Vector3(0f, 3.2f, 0f);
    [SerializeField] private Color labelColor = new Color(0.45f, 1f, 0.12f, 1f);

    private PoisonStatus poisonStatus;
    private SpriteRenderer auraRenderer;
    private TMP_Text statusLabel;
    private Camera targetCamera;
    private Vector3 auraBaseScale;
    private bool isVisible;

    private void Awake()
    {
        poisonStatus = GetComponent<PoisonStatus>();
        CreateAura();
        CreateLabel();
        SetVisible(false);
    }

    private void OnEnable()
    {
        poisonStatus.PoisonStarted += HandlePoisonStarted;
        poisonStatus.PoisonEnded += HandlePoisonEnded;

        if (poisonStatus.IsPoisoned)
        {
            HandlePoisonStarted();
        }
    }

    private void OnDisable()
    {
        poisonStatus.PoisonStarted -= HandlePoisonStarted;
        poisonStatus.PoisonEnded -= HandlePoisonEnded;
        SetVisible(false);
    }

    private void Update()
    {
        if (!isVisible)
        {
            return;
        }

        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        auraRenderer.transform.localScale = auraBaseScale * pulse;

        float normalizedPulse = pulseAmount > Mathf.Epsilon
            ? (pulse + pulseAmount - 1f) / (2f * pulseAmount)
            : 0.5f;
        Color color = auraColor;
        color.a *= Mathf.Lerp(0.7f, 1f, normalizedPulse);
        auraRenderer.color = color;
        statusLabel.SetText("POISON  {0:0.0}s", poisonStatus.RemainingDuration);
    }

    private void LateUpdate()
    {
        if (!isVisible)
        {
            return;
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            return;
        }

        Vector3 forward = statusLabel.transform.position - targetCamera.transform.position;
        if (forward.sqrMagnitude > 0.001f)
        {
            statusLabel.transform.rotation = Quaternion.LookRotation(
                forward,
                targetCamera.transform.up);
        }
    }

    private void HandlePoisonStarted()
    {
        SetVisible(true);
    }

    private void HandlePoisonEnded()
    {
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        isVisible = visible;

        if (auraRenderer != null)
        {
            auraRenderer.gameObject.SetActive(visible);
        }

        if (statusLabel != null)
        {
            statusLabel.gameObject.SetActive(visible);
        }
    }

    private void CreateAura()
    {
        GameObject auraObject = new GameObject("PoisonAura");
        auraObject.layer = gameObject.layer;
        auraObject.transform.SetParent(transform, false);
        auraObject.transform.localPosition = new Vector3(0f, 0.04f, 0f);
        auraObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        auraRenderer = auraObject.AddComponent<SpriteRenderer>();
        auraRenderer.sprite = auraSprite;
        auraRenderer.color = auraColor;
        auraRenderer.sortingOrder = 3;

        float spriteDiameter = auraSprite != null
            ? auraSprite.bounds.size.x
            : 1f;
        float scale = spriteDiameter > Mathf.Epsilon
            ? auraDiameter / spriteDiameter
            : auraDiameter;
        auraBaseScale = new Vector3(scale, scale, 1f);
        auraObject.transform.localScale = auraBaseScale;
    }

    private void CreateLabel()
    {
        GameObject labelObject = new GameObject("PoisonStatusLabel");
        labelObject.layer = gameObject.layer;
        labelObject.transform.SetParent(transform, false);
        labelObject.transform.localPosition = labelOffset;
        labelObject.transform.localScale = Vector3.one * 0.25f;

        statusLabel = labelObject.AddComponent<TextMeshPro>();
        statusLabel.alignment = TextAlignmentOptions.Center;
        statusLabel.color = labelColor;
        statusLabel.fontSize = 4f;
        statusLabel.fontStyle = FontStyles.Bold;
        statusLabel.textWrappingMode = TextWrappingModes.NoWrap;
        statusLabel.raycastTarget = false;
        statusLabel.rectTransform.sizeDelta = new Vector2(12f, 2f);

        Renderer labelRenderer = statusLabel.GetComponent<Renderer>();
        if (labelRenderer != null)
        {
            labelRenderer.sortingOrder = 101;
        }

        Material overlayMaterial = statusLabel.fontMaterial;
        if (overlayMaterial != null)
        {
            overlayMaterial.renderQueue = (int)RenderQueue.Overlay;

            if (overlayMaterial.HasProperty(ZTestModeId))
            {
                overlayMaterial.SetFloat(
                    ZTestModeId,
                    (float)CompareFunction.Always);
            }
        }
    }
}
