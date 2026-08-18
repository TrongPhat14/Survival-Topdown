using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(TextMeshPro))]
public class DamagePopup : MonoBehaviour
{
    private static readonly int ZTestModeId = Shader.PropertyToID("_ZTestMode");

    [SerializeField, Min(0.1f)] private float duration = 0.8f;
    [SerializeField, Min(0f)] private float riseDistance = 0.8f;

    private TMP_Text valueText;
    private DamagePopupPool ownerPool;
    private Camera targetCamera;
    private Color initialColor;
    private Vector3 initialScale;
    private Vector3 startPosition;
    private float elapsed;
    private bool isPlaying;

    private void Awake()
    {
        valueText = GetComponent<TMP_Text>();
        initialColor = valueText.color;
        initialScale = transform.localScale;

        Renderer popupRenderer = GetComponent<Renderer>();
        if (popupRenderer != null)
        {
            popupRenderer.sortingOrder = 100;
        }

        Material overlayMaterial = valueText.fontMaterial;
        if (overlayMaterial != null)
        {
            overlayMaterial.renderQueue = (int)RenderQueue.Overlay;

            if (overlayMaterial.HasProperty(ZTestModeId))
            {
                overlayMaterial.SetFloat(ZTestModeId, (float)CompareFunction.Always);
            }
        }
    }

    public void SetPool(DamagePopupPool pool)
    {
        ownerPool = pool;
    }

    public void Show(float damage, Vector3 position)
    {
        startPosition = position;
        transform.position = position;
        transform.localScale = initialScale;
        valueText.color = initialColor;
        valueText.SetText("-{0:0}", damage);
        targetCamera = Camera.main;
        elapsed = 0f;
        isPlaying = true;
    }

    private void Update()
    {
        if (!isPlaying)
        {
            return;
        }

        elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsed / duration);
        transform.position = startPosition + Vector3.up * (riseDistance * progress);

        Color color = initialColor;
        color.a = 1f - progress;
        valueText.color = color;
        transform.localScale = initialScale * Mathf.Lerp(1f, 1.15f, progress);

        if (progress >= 1f)
        {
            isPlaying = false;
            ownerPool?.Release(this);
        }
    }

    private void LateUpdate()
    {
        if (!isPlaying)
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

        Vector3 forward = transform.position - targetCamera.transform.position;
        if (forward.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(forward, targetCamera.transform.up);
        }
    }

    private void OnDisable()
    {
        isPlaying = false;
    }
}
