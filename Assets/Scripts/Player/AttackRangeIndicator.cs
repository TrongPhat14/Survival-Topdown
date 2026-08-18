using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class AttackRangeIndicator : MonoBehaviour
{
    [SerializeField] private WeaponController weapon;
    [SerializeField] private Material lineMaterial;
    [SerializeField, Range(24, 128)] private int segments = 72;
    [SerializeField, Min(0.01f)] private float lineWidth = 0.08f;

    private LineRenderer lineRenderer;
    private GameInput gameInput;
    private Transform followTarget;
    private float displayRadius;
    private float heightOffset = 0.08f;
    private bool listenForAttackInput = true;
    private bool isRuntimeIndicator;

    private void Awake()
    {
        if (weapon == null && listenForAttackInput)
        {
            weapon = GetComponentInParent<WeaponController>();
        }

        EnsureLineRenderer();
        ConfigureLine();

        if (listenForAttackInput && weapon != null)
        {
            displayRadius = weapon.AttackRange;
        }

        BuildCircle(displayRadius);
        Hide();
    }

    private void OnEnable()
    {
        if (!listenForAttackInput)
        {
            Show(displayRadius);
            return;
        }

        gameInput = GameInput.Instance;

        if (gameInput != null)
        {
            gameInput.AttackStarted += ShowAttackRange;
            gameInput.AttackReleased += Hide;
        }
    }

    private void OnDisable()
    {
        if (gameInput != null)
        {
            gameInput.AttackStarted -= ShowAttackRange;
            gameInput.AttackReleased -= Hide;
            gameInput = null;
        }

        Hide();
    }

    private void LateUpdate()
    {
        if (!isRuntimeIndicator)
        {
            return;
        }

        if (followTarget == null)
        {
            Dispose();
            return;
        }

        transform.SetPositionAndRotation(
            followTarget.position + Vector3.up * heightOffset,
            Quaternion.identity);
    }

    public static AttackRangeIndicator CreateSkillIndicator(
        AttackRangeIndicator template,
        Transform target,
        float radius)
    {
        if (template == null || target == null)
        {
            return null;
        }

        GameObject indicatorObject = new GameObject("SkillRangeIndicator");
        indicatorObject.SetActive(false);
        indicatorObject.layer = template.gameObject.layer;

        AttackRangeIndicator indicator =
            indicatorObject.AddComponent<AttackRangeIndicator>();

        indicator.weapon = null;
        indicator.lineMaterial = template.lineMaterial;
        indicator.segments = template.segments;
        indicator.lineWidth = template.lineWidth;
        indicator.followTarget = target;
        indicator.displayRadius = Mathf.Max(0f, radius);
        indicator.heightOffset = template.transform.localPosition.y;
        indicator.listenForAttackInput = false;
        indicator.isRuntimeIndicator = true;

        indicatorObject.transform.position =
            target.position + Vector3.up * indicator.heightOffset;
        indicatorObject.SetActive(true);
        return indicator;
    }

    public void Show(float radius)
    {
        displayRadius = Mathf.Max(0f, radius);
        BuildCircle(displayRadius);

        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
        }
    }

    public void Hide()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }

    public void Dispose()
    {
        Hide();

        if (isRuntimeIndicator)
        {
            Destroy(gameObject);
        }
    }

    private void ShowAttackRange()
    {
        if (weapon != null)
        {
            Show(weapon.AttackRange);
        }
    }

    private void EnsureLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
    }

    private void ConfigureLine()
    {
        lineRenderer.sharedMaterial = lineMaterial;
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.numCornerVertices = 2;
        lineRenderer.numCapVertices = 0;
        lineRenderer.alignment = LineAlignment.View;
        lineRenderer.textureMode = LineTextureMode.Stretch;
        lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
    }

    private void BuildCircle(float radius)
    {
        if (lineRenderer == null)
        {
            return;
        }

        lineRenderer.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            lineRenderer.SetPosition(i, new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius));
        }
    }
}
