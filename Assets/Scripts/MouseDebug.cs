using UnityEngine;
using UnityEngine.InputSystem;

public class MouseDebug : MonoBehaviour
{
    private BoxCollider2D boxCollider;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mouseScreen = Mouse.current.position.ReadValue();
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
            mouseWorld.z = 0;

            Debug.Log("Click World Pos: " + mouseWorld);
            Debug.Log("Wire Transform Pos: " + transform.position);

            if (boxCollider != null)
            {
                Vector2 colliderCenter = (Vector2)transform.position + boxCollider.offset;
                Debug.Log("Collider Center: " + colliderCenter);
                Debug.Log("Collider Bounds: " + boxCollider.bounds);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (boxCollider != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(boxCollider.bounds.center, boxCollider.bounds.size);
        }
    }
}