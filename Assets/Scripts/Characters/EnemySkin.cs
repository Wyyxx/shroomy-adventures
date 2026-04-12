using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Assigns a random enemy skin at runtime and handles idle animation + hit flash effect.
/// Attach to the Enemy prefab alongside the SpriteRenderer.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class EnemySkin : MonoBehaviour
{
    [Header("Skins Disponibles (Idle Sprite Sheets)")]
    public Sprite[] lizardIdle;
    public Sprite[] shamanIdle;
    public Sprite[] spiderIdle;
    public Sprite[] thiefIdle;

    [Header("Hit Sprites (opcional)")]
    public Sprite[] lizardHit;

    [Header("Configuración")]
    [Tooltip("Velocidad de la animación idle (frames por segundo)")]
    public float idleFrameRate = 8f;

    [Header("Hit Effect")]
    public Color hitColor = Color.red;
    public float hitFlashDuration = 0.1f;
    public int hitFlashCount = 2;
    public float hitShakeIntensity = 0.1f;

    // Internal
    private SpriteRenderer sr;
    private Sprite[] currentIdleFrames;
    private Sprite[] currentHitFrames;
    private int currentFrame = 0;
    private float frameTimer = 0f;
    private bool isPlayingHit = false;
    private Vector3 originalPosition;
    private string activeSkinName;
    private bool skinReady = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        // Hide the sprite until the skin is assigned to prevent the default sprite from flashing
        sr.sprite = null;

        // Flip to face left
        sr.flipX = true;
    }

    void Start()
    {
        originalPosition = transform.localPosition;

        // Pick a random skin (0-3)
        int skinIndex = Random.Range(0, 4);

        switch (skinIndex)
        {
            case 0:
                activeSkinName = "Lizard";
                currentIdleFrames = lizardIdle;
                currentHitFrames = lizardHit;
                break;
            case 1:
                activeSkinName = "Shaman";
                currentIdleFrames = shamanIdle;
                currentHitFrames = null;
                break;
            case 2:
                activeSkinName = "Spider";
                currentIdleFrames = spiderIdle;
                currentHitFrames = null;
                break;
            case 3:
                activeSkinName = "Thief";
                currentIdleFrames = thiefIdle;
                currentHitFrames = null;
                break;
        }

        // Filter out any null sprites from the arrays
        currentIdleFrames = FilterNullSprites(currentIdleFrames);
        currentHitFrames = FilterNullSprites(currentHitFrames);

        // Set the first frame immediately
        if (currentIdleFrames != null && currentIdleFrames.Length > 0)
        {
            sr.sprite = currentIdleFrames[0];
            skinReady = true;
            Debug.Log($"EnemySkin: {activeSkinName} asignado con {currentIdleFrames.Length} frames de idle");
        }
        else
        {
            Debug.LogWarning($"EnemySkin: No se encontraron sprites idle para skin index {skinIndex}");
        }
    }

    /// <summary>
    /// Removes any null entries from a sprite array to prevent flickering.
    /// </summary>
    private Sprite[] FilterNullSprites(Sprite[] sprites)
    {
        if (sprites == null) return null;

        List<Sprite> filtered = new List<Sprite>();
        foreach (Sprite s in sprites)
        {
            if (s != null) filtered.Add(s);
        }
        return filtered.Count > 0 ? filtered.ToArray() : null;
    }

    void Update()
    {
        if (!skinReady || isPlayingHit) return;
        if (currentIdleFrames == null || currentIdleFrames.Length <= 1) return;

        frameTimer += Time.deltaTime;
        float frameInterval = 1f / idleFrameRate;

        if (frameTimer >= frameInterval)
        {
            // Only advance one frame per check, reset timer cleanly
            frameTimer = 0f;
            currentFrame = (currentFrame + 1) % currentIdleFrames.Length;
            sr.sprite = currentIdleFrames[currentFrame];
        }
    }

    /// <summary>
    /// Call this when the enemy takes damage to play the hit effect.
    /// </summary>
    public void PlayHitEffect()
    {
        if (!isPlayingHit && gameObject.activeInHierarchy)
        {
            StartCoroutine(HitEffectCoroutine());
        }
    }

    private IEnumerator HitEffectCoroutine()
    {
        isPlayingHit = true;
        originalPosition = transform.localPosition;

        // If we have hit frames, show them briefly
        if (currentHitFrames != null && currentHitFrames.Length > 0)
        {
            float hitFrameDuration = hitFlashDuration;
            foreach (Sprite hitSprite in currentHitFrames)
            {
                sr.sprite = hitSprite;
                yield return new WaitForSeconds(hitFrameDuration);
            }
        }

        // Flash red + shake effect
        for (int i = 0; i < hitFlashCount; i++)
        {
            // Flash to hit color
            sr.color = hitColor;

            // Shake
            transform.localPosition = originalPosition + (Vector3)Random.insideUnitCircle * hitShakeIntensity;
            yield return new WaitForSeconds(hitFlashDuration);

            // Return to normal
            sr.color = Color.white;
            transform.localPosition = originalPosition;
            yield return new WaitForSeconds(hitFlashDuration * 0.5f);
        }

        // Ensure we're back to normal
        sr.color = Color.white;
        transform.localPosition = originalPosition;

        // Resume idle from current frame
        isPlayingHit = false;
    }
}
