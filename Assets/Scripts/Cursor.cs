using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Texture2D cursorNormal;
    public Texture2D cursorHover;

    public static CursorManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetNormal();
    }

    public void SetNormal()
    {
        Cursor.SetCursor(cursorNormal, new Vector2(cursorNormal.width / 2f, cursorNormal.height / 2f), CursorMode.Auto);
    }

    public void SetHover()
    {
        Cursor.SetCursor(cursorHover, new Vector2(cursorHover.width / 2f, cursorHover.height / 2f), CursorMode.Auto);
    }
}