using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FlipSprite(bool facingRight)
    {
        spriteRenderer.flipX = !facingRight;
    }
}
