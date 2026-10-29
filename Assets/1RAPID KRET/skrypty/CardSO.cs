using UnityEngine;

public enum CardActionType { BigSlash, Projectile, DashHit }

[CreateAssetMenu(fileName = "New Card", menuName = "Rapid/Card SO")]
public class CardSO : ScriptableObject
{
    public string cardName;
    public int powerValue;
    public Sprite cardIcon;
    public CardActionType actionType;
    
    [Header("Tylko dla Projectile")]
    public GameObject projectilePrefab;
}