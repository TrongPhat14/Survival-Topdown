using UnityEngine;

[CreateAssetMenu(fileName = "DashSkillData", menuName = "Survival/Skills/Dash Explosion")]
public class DashSkillData : ScriptableObject
{
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField, Min(0f)] private float distance = 3f;
    [SerializeField, Min(0.01f)] private float duration = 0.5f;
    [SerializeField, Min(0f)] private float baseDamage = 15f;
    [SerializeField, Min(0f)] private float explosionRadius = 3f;
    [SerializeField, Min(0f)] private float cooldown = 6f;

    public GameObject ExplosionEffectPrefab => explosionEffectPrefab;
    public float Distance => distance;
    public float Duration => duration;
    public float BaseDamage => baseDamage;
    public float ExplosionRadius => explosionRadius;
    public float Cooldown => cooldown;
}
