using UnityEngine;
using UnityEngine.UI;

public class UICellKret : MonoBehaviour
{
    public int index;
    public RoomType roomType = RoomType.None;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;

    public void Setup(int cellIndex, RoomType type, Sprite icon)
    {
        index = cellIndex;
        roomType = type;
        if (iconImage != null)
        {
            iconImage.sprite = icon;
        }
    }
}