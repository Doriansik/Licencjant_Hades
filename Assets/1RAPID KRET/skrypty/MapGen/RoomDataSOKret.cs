using UnityEngine;
using System.Collections.Generic;

public enum RoomType 
{ 
    None, Start, Combat, Treasure, Special, Event, Boss, Exit 
}

[CreateAssetMenu(fileName = "NewRoomData", menuName = "Kret/RoomData")]
public class RoomDataSOKret : ScriptableObject
{
    public RoomType roomType;
    public List<GameObject> roomPrefabs;
}