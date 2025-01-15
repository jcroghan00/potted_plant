using UnityEngine;

public class SwingPoint : MonoBehaviour
{
    public Color baseColor;
    public Color selectedColor;

    private SpriteRenderer _spriteRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    void Awake() 
    {
        GameObject player = GameObject.Find("Player");
        PlayerController playerCont = player.GetComponent<PlayerController>();
        playerCont.addSwingPoint(gameObject);
    }

    public void select() 
    {
        _spriteRenderer.color = selectedColor;
    }

    public void unselect() 
    {
        _spriteRenderer.color = baseColor;
    }
}
