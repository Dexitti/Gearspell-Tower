using System;
using System.Collections;
using UnityEngine;

public class GearsDrop : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    int gearsNumber = 1;
    float lifetime = 0.5f;

    public int GearsNumber { get; set; }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        StartCoroutine(LifetimeRoutine());
    }

    private IEnumerator LifetimeRoutine()
    {
        float fadeDuration = 0.15f;
        yield return new WaitForSeconds(lifetime - fadeDuration);

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * 0.3f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            transform.position = Vector3.Lerp(startPos, endPos, t);

            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 1f - t;
                spriteRenderer.color = color;
            }

            yield return null;
        }

        G.ResourceManager?.AddGears(gearsNumber);
        Destroy(gameObject);
    }
}
