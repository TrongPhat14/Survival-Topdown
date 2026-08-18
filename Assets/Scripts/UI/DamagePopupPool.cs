using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[DisallowMultipleComponent]
public class DamagePopupPool : MonoBehaviour
{
    private ObjectPool<DamagePopup> pool;
    private GameObject popupPrefab;

    public bool IsInitialized => pool != null;

    public void Initialize(GameObject prefab, int initialSize, int maxSize)
    {
        if (IsInitialized)
        {
            return;
        }

        if (prefab == null || !prefab.TryGetComponent(out DamagePopup _))
        {
            Debug.LogError("DamagePopupPool requires a prefab with a DamagePopup component.", this);
            return;
        }

        popupPrefab = prefab;
        initialSize = Mathf.Max(1, initialSize);
        maxSize = Mathf.Max(initialSize, maxSize);

        pool = new ObjectPool<DamagePopup>(
            CreatePopup,
            null,
            OnReturnedToPool,
            OnDestroyedByPool,
            false,
            initialSize,
            maxSize);

        Prewarm(initialSize);
    }

    public void Show(float damage, Vector3 position)
    {
        if (!IsInitialized)
        {
            return;
        }

        DamagePopup popup = pool.Get();
        popup.transform.position = position;
        popup.gameObject.SetActive(true);
        popup.Show(damage, position);
    }

    public void Release(DamagePopup popup)
    {
        if (popup == null)
        {
            return;
        }

        if (IsInitialized)
        {
            pool.Release(popup);
        }
        else
        {
            Destroy(popup.gameObject);
        }
    }

    private DamagePopup CreatePopup()
    {
        GameObject popupObject = Instantiate(popupPrefab, transform);
        DamagePopup popup = popupObject.GetComponent<DamagePopup>();
        popup.SetPool(this);
        popupObject.SetActive(false);
        return popup;
    }

    private void OnReturnedToPool(DamagePopup popup)
    {
        popup.transform.SetParent(transform, false);
        popup.gameObject.SetActive(false);
    }

    private void OnDestroyedByPool(DamagePopup popup)
    {
        if (popup != null)
        {
            Destroy(popup.gameObject);
        }
    }

    private void Prewarm(int count)
    {
        List<DamagePopup> popups = new List<DamagePopup>(count);

        for (int i = 0; i < count; i++)
        {
            popups.Add(pool.Get());
        }

        for (int i = 0; i < popups.Count; i++)
        {
            pool.Release(popups[i]);
        }
    }

    private void OnDestroy()
    {
        pool?.Clear();
        pool = null;
    }
}
