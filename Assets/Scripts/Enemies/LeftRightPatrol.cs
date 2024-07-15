using UnityEngine;

public class LeftRightPatrol : MonoBehaviour
{
    public bool FoundEdge { get; private set; }
    private void OnTriggerExit2D(Collider2D other)
    {
        FoundEdge = true;
        FlipTransform();
    }

    public void ClearFoundEdge()
    {
        FoundEdge = false;
    }

    private void FlipTransform()
    {
        transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);
    }
}
