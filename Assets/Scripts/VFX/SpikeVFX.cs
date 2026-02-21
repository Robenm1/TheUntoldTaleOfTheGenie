using UnityEngine;

public class SpikeVFX : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lifetime = 2f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
