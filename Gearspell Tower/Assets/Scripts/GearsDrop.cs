using System;
using System.Collections;
using UnityEngine;

public class GearsDrop : MonoBehaviour
{
    [SerializeField] private Sprite[] gearsSprite = new Sprite[3];
    [SerializeField] private float throwDistance = 1.0f;
    [SerializeField] private float lifetime = 0.5f;
    private SpriteRenderer spriteRenderer;
    private int gearsNumber = 1;

    public int GearsNumber { get; set; }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (gearsNumber < 10) spriteRenderer.sprite = gearsSprite[0];
        else if (gearsNumber < 25) spriteRenderer.sprite = gearsSprite[1];
        else spriteRenderer.sprite = gearsSprite[2];

        StartCoroutine(LifetimeRoutine());
    }

    private IEnumerator LifetimeRoutine()
    {
        float flyDuration = 0.25f;
        float fadeDuration = 0.2f;
        float elapsed = 0f;

        // Случайный разлёт
        float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = UnityEngine.Random.Range(0.3f, throwDistance);
        Vector3 flyStart = transform.position;
        Vector3 flyTarget = transform.position + IsometricExtension.IsoVector(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;

        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flyDuration;
            float easedT = 1f - (1f - t) * (1f - t);
            transform.position = Vector3.Lerp(flyStart, flyTarget, easedT);
            yield return null;
        }

        yield return new WaitForSeconds(lifetime - flyDuration - fadeDuration);

        // Анимация исчезновения
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * 0.3f;
        elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            float easedT = t * t;

            transform.position = Vector3.Lerp(startPos, endPos, easedT);

            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 1f - easedT;
                spriteRenderer.color = color;
            }

            yield return null;
        }

        G.ResourceManager?.AddGears(gearsNumber);
        Destroy(gameObject);
    }
}
