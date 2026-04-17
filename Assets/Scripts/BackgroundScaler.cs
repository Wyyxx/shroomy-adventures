using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Scales a SpriteRenderer to always cover the full camera view.
/// Uses non-uniform scaling to stretch and fill the entire viewport.
/// Attach to the MapBackground GameObject.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundScaler : MonoBehaviour
{
    private Coroutine fitRoutine;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        RestartFitRoutine();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (fitRoutine != null)
        {
            StopCoroutine(fitRoutine);
            fitRoutine = null;
        }
    }

    private void Start()
    {
        RestartFitRoutine();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RestartFitRoutine();
    }

    private void RestartFitRoutine()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        if (fitRoutine != null)
        {
            StopCoroutine(fitRoutine);
        }

        fitRoutine = StartCoroutine(FitWhenCameraIsReady());
    }

    private IEnumerator FitWhenCameraIsReady()
    {
        const float timeoutSeconds = 2f;
        float elapsed = 0f;

        while (elapsed < timeoutSeconds)
        {
            if (TryFitToCamera())
            {
                fitRoutine = null;
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // One last attempt in case the camera became available on the timeout frame.
        TryFitToCamera();
        fitRoutine = null;
    }

    public void FitToCamera()
    {
        RestartFitRoutine();
    }

    public bool TryFitToCamera()
    {
        Camera cam = ResolveCamera();
        if (cam == null) return false;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return false;

        // Get the camera's visible area in world units
        float cameraHeight = 2f * cam.orthographicSize;
        float cameraWidth = cameraHeight * cam.aspect;

        // Get the sprite's native size in world units
        float spriteWidth = sr.sprite.bounds.size.x;
        float spriteHeight = sr.sprite.bounds.size.y;

        // Use non-uniform scale to stretch-fill the entire camera view
        float scaleX = (cameraWidth / spriteWidth) * 1.02f; // tiny margin
        float scaleY = (cameraHeight / spriteHeight) * 1.02f;

        transform.localScale = new Vector3(scaleX, scaleY, 1f);

        // Center on camera position
        transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
        return true;
    }

    private Camera ResolveCamera()
    {
        if (Camera.main != null && Camera.main.isActiveAndEnabled)
        {
            return Camera.main;
        }

        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera camera in cameras)
        {
            if (camera.isActiveAndEnabled)
            {
                return camera;
            }
        }

        return null;
    }
}
