using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class TimedBomb : MonoBehaviour
{
    private float fuseDuration;
    private float damage;
    private float explosionRadius;
    private LayerMask targetLayers;
    private Vector3 initialScale;
    private Coroutine fuseRoutine;
    private AttackRangeIndicator rangeIndicator;
    private VfxPool vfxPool;
    private GameObject explosionEffectPrefab;
    private float explosionEffectScale = 1f;

    public void Arm(
        float fuse,
        float explosionDamage,
        float radius,
        LayerMask layers,
        AttackRangeIndicator indicatorTemplate,
        VfxPool effectPool,
        GameObject explosionEffect,
        float effectScale)
    {
        fuseDuration = Mathf.Max(0f, fuse);
        damage = Mathf.Max(0f, explosionDamage);
        explosionRadius = Mathf.Max(0f, radius);
        targetLayers = layers;
        vfxPool = effectPool;
        explosionEffectPrefab = explosionEffect;
        explosionEffectScale = Mathf.Max(0.01f, effectScale);
        initialScale = transform.localScale;
        rangeIndicator?.Dispose();
        rangeIndicator = AttackRangeIndicator.CreateSkillIndicator(
            indicatorTemplate,
            transform,
            explosionRadius);

        if (fuseRoutine != null)
        {
            StopCoroutine(fuseRoutine);
        }

        fuseRoutine = StartCoroutine(FuseRoutine());
    }

    private IEnumerator FuseRoutine()
    {
        float elapsed = 0f;

        while (elapsed < fuseDuration)
        {
            elapsed += Time.deltaTime;
            float pulse = 1f + Mathf.Sin(elapsed * 12f) * 0.08f;
            transform.localScale = initialScale * pulse;
            yield return null;
        }

        rangeIndicator?.Dispose();
        rangeIndicator = null;

        if (vfxPool != null && explosionEffectPrefab != null)
        {
            vfxPool.Play(
                explosionEffectPrefab,
                transform.position,
                explosionEffectPrefab.transform.rotation,
                explosionEffectScale);
        }

        CameraShake.PlayBombExplosion();
        SoundManager.PlayAt(SoundId.Bomb, transform.position);
        AreaDamageUtility.Apply(
            transform.position,
            explosionRadius,
            damage,
            targetLayers);

        fuseRoutine = null;
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        rangeIndicator?.Dispose();
        rangeIndicator = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.35f, 0.1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
