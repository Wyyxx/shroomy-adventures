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
    public enum EnemyVisualType { Lizard, Shaman, Spider, Thief, Minotaur }
    
    [Header("Tipo Visual Fijo")]
    public EnemyVisualType myVisualType;
    
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
        //sr.sprite = null;

        //Cambio de codigo parpadeo
        sr.enabled = false;

        // Flip to face left
        sr.flipX = true;
    }

    void Start()
    {
        originalPosition = transform.localPosition;

       switch (myVisualType)
        {
            case EnemyVisualType.Lizard:
                activeSkinName = "Lizard";
                currentIdleFrames = lizardIdle;
                currentHitFrames = lizardHit;
                break;
            case EnemyVisualType.Shaman:
                activeSkinName = "Shaman";
                currentIdleFrames = shamanIdle;
                currentHitFrames = null;
                break;
            case EnemyVisualType.Spider:
                activeSkinName = "Spider";
                currentIdleFrames = spiderIdle;
                currentHitFrames = null;
                break;
            case EnemyVisualType.Thief:
                activeSkinName = "Thief";
                currentIdleFrames = thiefIdle;
                currentHitFrames = null;
                break;
            case EnemyVisualType.Minotaur:
                activeSkinName = "Minotaur";
                // Asigna aquí los frames del minotauro cuando los tengas
                break;
        }

        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.enemyName = activeSkinName;
            // Actualiza el texto del nombre si ya existe
            if (enemy.localNameText != null)
                enemy.localNameText.text = activeSkinName;
        }

        // Filter out any null sprites from the arrays
        currentIdleFrames = FilterNullSprites(currentIdleFrames);
        currentHitFrames = FilterNullSprites(currentHitFrames);

        // Set the first frame immediately
        if (currentIdleFrames != null && currentIdleFrames.Length > 0)
        {
            sr.sprite = currentIdleFrames[0];
            //agregado de codigo por parpadeo
            sr.enabled = true;
            skinReady = true;
            Debug.Log($"EnemySkin: {activeSkinName} asignado con {currentIdleFrames.Length} frames de idle");
        }
        else
        {
            Debug.LogWarning($"EnemySkin: No se asignaron frames de idle para {activeSkinName}");
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
            // Filtra nulls Y sprites con rect vacío
            if (s != null && s.rect.width > 0 && s.rect.height > 0)
            {
                filtered.Add(s);
            }
        }

        if (filtered.Count > 0)
        {
            Debug.Log($"Sprites válidos: {filtered.Count} de {sprites.Length}");
            return filtered.ToArray();
        }
        return null;
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
            //cambio de codigo parpadeo
            //frameTimer = 0f;
            frameTimer -= frameInterval;
            currentFrame = (currentFrame + 1) % currentIdleFrames.Length;
            Sprite nextSprite = currentIdleFrames[currentFrame];
            if (nextSprite != null)
            {
                sr.sprite = nextSprite;
            }
            else
            {
                // Frame vacío encontrado, regresa al frame 0
                currentFrame = 0;
                sr.sprite = currentIdleFrames[0];
                Debug.LogWarning($"Frame vacío detectado en índice {currentFrame}, regresando a frame 0");
            }
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
                //cambios de codigo parpadeo
                if (!gameObject.activeInHierarchy) yield break;
                sr.sprite = hitSprite;
                yield return new WaitForSeconds(hitFrameDuration);
            }
        }

        // Flash red + shake effect
        for (int i = 0; i < hitFlashCount; i++)
        {
            //cambio de codigo parpadeo
            if (!gameObject.activeInHierarchy) yield break;

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
