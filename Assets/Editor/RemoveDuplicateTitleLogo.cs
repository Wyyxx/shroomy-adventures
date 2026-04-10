using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class RemoveDuplicateTitleLogo
{
    public static void Execute()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("Canvas not found");
            return;
        }

        int count = 0;
        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
        {
            var child = canvas.transform.GetChild(i);
            if (child.name == "TitleLogo")
            {
                count++;
                if (count > 1)
                {
                    Debug.Log($"Deleting duplicate TitleLogo at sibling index {i}");
                    Undo.DestroyObjectImmediate(child.gameObject);
                }
            }
        }

        if (count <= 1)
            Debug.Log("No duplicate TitleLogo found.");
        else
            Debug.Log($"Removed {count - 1} duplicate(s).");
    }
}
