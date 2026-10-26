using UnityEngine;

public enum RoomType
{
    None,
    Start,
    Combat,
    Treasure,
    Special,
    Event,
    Boss,
    Exit //transition to the next floor(s)
}

public class Cell : MonoBehaviour
{
    public int index;
    public RoomType roomType = RoomType.None;

    [SerializeField] private SpriteRenderer iconRenderer;
    [SerializeField] private SpriteRenderer backgroundRenderer;

    public void Setup(int cellIndex, RoomType type, Sprite icon)
    {
        index = cellIndex;
        roomType = type;

        if (iconRenderer != null)
        {
            iconRenderer.sprite = icon;
        }
    }
}