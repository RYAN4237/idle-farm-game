using UnityEngine;
using System.Collections;

public class HarvestFX : MonoBehaviour
{
    public static HarvestFX Instance { get; private set; }

    [Header("Particles")]
    [SerializeField] private GameObject _particlePrefab;
    [SerializeField] private int _particleCount = 6;
    [SerializeField] private float _particleSpeed = 3f;
    [SerializeField] private float _particleLifetime = 0.8f;

    [Header("Bounce")]
    [SerializeField] private float _bounceScale = 1.3f;
    [SerializeField] private float _bounceDuration = 0.25f;

    [Header("Audio")]
    [SerializeField] private AudioClip _harvestClip;
    private AudioSource _audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    public void PlayAt(Transform target)
    {
        if (target == null) return;
        StartCoroutine(BounceRoutine(target));
        SpawnParticles(target.position);
        if (_harvestClip != null)
            _audioSource.PlayOneShot(_harvestClip, 0.6f);
    }

    IEnumerator BounceRoutine(Transform target)
    {
        if (target == null) yield break;
        Vector3 original = target.localScale;
        Vector3 big = original * _bounceScale;
        float half = _bounceDuration * 0.4f;

        float t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float p = t / half;
            target.localScale = Vector3.Lerp(original, big, EaseOutBack(p));
            yield return null;
        }

        t = 0f;
        float rest = _bounceDuration - half;
        while (t < rest)
        {
            t += Time.deltaTime;
            float p = t / rest;
            target.localScale = Vector3.Lerp(big, original, EaseOutBounce(p));
            yield return null;
        }
        target.localScale = original;
    }

    void SpawnParticles(Vector3 worldPos)
    {
        for (int i = 0; i < _particleCount; i++)
        {
            float angle = (360f / _particleCount) * i + Random.Range(-15f, 15f);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            StartCoroutine(ParticleRoutine(worldPos, dir));
        }
    }

    IEnumerator ParticleRoutine(Vector3 startPos, Vector2 direction)
    {
        GameObject go;
        SpriteRenderer sr;

        if (_particlePrefab != null)
        {
            go = Instantiate(_particlePrefab, startPos, Quaternion.identity);
            sr = go.GetComponent<SpriteRenderer>();
        }
        else
        {
            go = new GameObject("HarvestParticle");
            go.transform.position = startPos;
            sr = go.AddComponent<SpriteRenderer>();
            sr.color = new Color(1f, 0.85f, 0.2f, 1f);
            sr.sprite = CreateCircleSprite();
            go.transform.localScale = Vector3.one * 0.15f;
            sr.sortingOrder = 100;
        }

        float elapsed = 0f;
        float speed = _particleSpeed * Random.Range(0.7f, 1.3f);
        Color startColor = sr.color;

        while (elapsed < _particleLifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _particleLifetime;

            go.transform.position += (Vector3)(direction * speed * Time.deltaTime);
            speed *= 0.95f;

            float alpha = t < 0.5f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.5f) / 0.5f);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            float scale = Mathf.Lerp(0.15f, 0.05f, t);
            go.transform.localScale = Vector3.one * scale;

            yield return null;
        }
        Destroy(go);
    }

    static Sprite _cachedCircle;
    static Sprite CreateCircleSprite()
    {
        if (_cachedCircle != null) return _cachedCircle;
        int size = 16;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size / 2f;
        float radius = center - 1f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                tex.SetPixel(x, y, dist <= radius ? Color.white : Color.clear);
            }
        tex.Apply();
        _cachedCircle = Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
        return _cachedCircle;
    }

    static float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    static float EaseOutBounce(float t)
    {
        if (t < 1f / 2.75f) return 7.5625f * t * t;
        if (t < 2f / 2.75f) { t -= 1.5f / 2.75f; return 7.5625f * t * t + 0.75f; }
        if (t < 2.5f / 2.75f) { t -= 2.25f / 2.75f; return 7.5625f * t * t + 0.9375f; }
        t -= 2.625f / 2.75f; return 7.5625f * t * t + 0.984375f;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
