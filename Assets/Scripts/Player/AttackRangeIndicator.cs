using UnityEngine;

[DisallowMultipleComponent]
public class AttackRangeIndicator : MonoBehaviour
{
    private static readonly Quaternion GroundRotation =
        Quaternion.Euler(90f, 0f, 0f);

    [SerializeField] private SpriteRenderer ringRenderer;
    [SerializeField] private SpriteRenderer fillRenderer;
    [SerializeField, Min(0f)] private float heightOffset = 0.08f;

    private Transform followTarget;
    private bool isRuntimeIndicator;

    private void Awake()
    {
        if (ringRenderer == null)
        {
            Transform ring = transform.Find("OuterRing");
            ringRenderer = ring != null
                ? ring.GetComponent<SpriteRenderer>()
                : null;
        }

        if (fillRenderer == null)
        {
            Transform fill = transform.Find("InnerFill");
            fillRenderer = fill != null
                ? fill.GetComponent<SpriteRenderer>()
                : null;
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
            GroundRotation);
    }

    public static AttackRangeIndicator CreateSkillIndicator(
        AttackRangeIndicator prefab,
        Transform target,
        float radius)
    {
        if (prefab == null || target == null)
        {
            return null;
        }

        AttackRangeIndicator indicator = Instantiate(prefab);
        indicator.name = "RangeIndicator";
        indicator.followTarget = target;
        indicator.isRuntimeIndicator = true;
        indicator.transform.SetParent(null);
        indicator.transform.SetPositionAndRotation(
            target.position + Vector3.up * indicator.heightOffset,
            GroundRotation);
        indicator.Show(radius);
        return indicator;
    }

    public void Show(float radius)
    {
        if (ringRenderer == null ||
            ringRenderer.sprite == null ||
            radius <= 0f)
        {
            Hide();
            return;
        }

        float spriteDiameter = ringRenderer.sprite.bounds.size.x;
        if (spriteDiameter <= 0f)
        {
            Hide();
            return;
        }

        float scale = radius * 2f / spriteDiameter;
        transform.localScale = new Vector3(scale, scale, 1f);
        ringRenderer.enabled = true;

        if (fillRenderer != null)
        {
            fillRenderer.enabled = true;
        }
    }

    public void Hide()
    {
        if (ringRenderer != null)
        {
            ringRenderer.enabled = false;
        }

        if (fillRenderer != null)
        {
            fillRenderer.enabled = false;
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
}
