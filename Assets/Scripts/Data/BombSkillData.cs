using UnityEngine;

[CreateAssetMenu(fileName = "BombSkillData", menuName = "Survival/Skills/Bomb")]
public class BombSkillData : ScriptableObject
{
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField, Min(0.01f)] private float explosionEffectScale = 1.25f;
    [SerializeField, Min(0f)] private float fuseDuration = 2f;
    [SerializeField, Min(0f)] private float baseDamage = 50f;
    [SerializeField, Min(0f)] private float explosionRadius = 5f;
    [SerializeField, Min(0f)] private float cooldown = 12f;

    public GameObject BombPrefab => bombPrefab;
    public GameObject ExplosionEffectPrefab => explosionEffectPrefab;
    public float ExplosionEffectScale => explosionEffectScale;
    public float FuseDuration => fuseDuration;
    public float BaseDamage => baseDamage;
    public float ExplosionRadius => explosionRadius;
    public float Cooldown => cooldown;
}
