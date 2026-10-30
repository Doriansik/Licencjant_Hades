using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapGeneratorKret : MonoBehaviour
{
    [SerializeField] private int currentFloor = 1;
    [SerializeField] private float worldRoomSpacing = 60f;
    [SerializeField] private float elevationOffset = 200f;
    [SerializeField] private float uiCellSize = 50f;
    
    [SerializeField] private RectTransform minimapContainer;
    [SerializeField] private UICellKret uiCellPrefab;
    [SerializeField] private Transform worldRoomsContainer;
    
    [SerializeField] private List<RoomDataSOKret> roomDataPool;

    [SerializeField] private Sprite startIcon;
    [SerializeField] private Sprite combatIcon;
    [SerializeField] private Sprite treasureIcon;
    [SerializeField] private Sprite specialIcon;
    [SerializeField] private Sprite eventIcon;
    [SerializeField] private Sprite bossIcon;
    [SerializeField] private Sprite exitIcon;

    private int[] floorPlan;
    private int floorPlanCount;
    private int targetRoomCount;
    private const int StartIndex = 45;
    
    private List<UICellKret> spawnedUICells = new();
    private List<GameObject> spawnedWorldRooms = new();
    private Dictionary<int, RoomType> assignedRooms = new();
    private Dictionary<int, RoomControllerKret> spawnedRoomControllers = new();

    void Start()
    {
        GenerateFloor();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            GenerateFloor();
        }
    }

    public void GenerateFloor()
    {
        if (currentFloor == 1) targetRoomCount = 8;
        else if (currentFloor == 2) targetRoomCount = 10;
        else targetRoomCount = 11;

        int safetyLimit = 500;
        while (safetyLimit-- > 0)
        {
            ClearMap();
            if (BuildLayout() && AssignRoomTypes())
            {
                SpawnMapVisuals();
                LinkAllRooms(); 
                return;
            }
        }
    }

    private void ClearMap()
    {
        foreach (var cell in spawnedUICells)
        {
            if (cell != null) Destroy(cell.gameObject);
        }
        foreach (var room in spawnedWorldRooms)
        {
            if (room != null) Destroy(room);
        }
        
        spawnedUICells.Clear();
        spawnedWorldRooms.Clear();
        assignedRooms.Clear();
        spawnedRoomControllers.Clear(); 
        floorPlan = new int[100];
        floorPlanCount = 0;
    }

    private bool BuildLayout()
    {
        Queue<int> queue = new Queue<int>();
        queue.Enqueue(StartIndex);
        floorPlan[StartIndex] = 1;
        floorPlanCount = 1;

        while (queue.Count > 0 && floorPlanCount < targetRoomCount)
        {
            int current = queue.Dequeue();
            List<int> validNeighbours = GetAvailableNeighbours(current);

            for (int i = 0; i < validNeighbours.Count; i++)
            {
                int temp = validNeighbours[i];
                int randomIndex = Random.Range(i, validNeighbours.Count);
                validNeighbours[i] = validNeighbours[randomIndex];
                validNeighbours[randomIndex] = temp;
            }

            foreach (int next in validNeighbours)
            {
                if (GetNeighbourCount(next) <= 1 && Random.value < 0.65f)
                {
                    floorPlan[next] = 1;
                    floorPlanCount++;
                    queue.Enqueue(next);
                    if (floorPlanCount >= targetRoomCount) break;
                }
            }
        }
        return floorPlanCount >= targetRoomCount;
    }

    private bool AssignRoomTypes()
    {
        assignedRooms[StartIndex] = RoomType.Start;
        Dictionary<int, int> distances = CalculateDistancesFrom(StartIndex);
        List<int> deadEnds = new List<int>();

        for (int i = 0; i < 100; i++)
        {
            if (floorPlan[i] == 1 && i != StartIndex && GetNeighbourCount(i) == 1)
            {
                deadEnds.Add(i);
            }
        }

        if (deadEnds.Count < 3) return false;

        deadEnds.Sort((a, b) => distances[b].CompareTo(distances[a]));
        int bossOrExitIndex = -1;

        foreach (int index in deadEnds)
        {
            int aboveIndex = index - 10;
            bool roomAbove = (aboveIndex >= 0 && floorPlan[aboveIndex] == 1);
            if (!roomAbove)
            {
                bossOrExitIndex = index;
                break;
            }
        }

        if (bossOrExitIndex == -1) return false;
        deadEnds.Remove(bossOrExitIndex);

        if (currentFloor == 3)
        {
            assignedRooms[bossOrExitIndex] = RoomType.Boss;
            int bossDoorNeighbour = GetFirstActiveNeighbour(bossOrExitIndex);
            if (bossDoorNeighbour != -1 && bossDoorNeighbour != StartIndex)
            {
                assignedRooms[bossDoorNeighbour] = RoomType.Event;
                deadEnds.Remove(bossDoorNeighbour);
            }
        }
        else
        {
            assignedRooms[bossOrExitIndex] = RoomType.Exit;
        }

        int treasureIndex = -1;
        int specialIndex = -1;

        for (int i = 0; i < deadEnds.Count; i++)
        {
            int candidateA = deadEnds[i];
            for (int j = i + 1; j < deadEnds.Count; j++)
            {
                int candidateB = deadEnds[j];
                if (!AreAdjacent(candidateA, candidateB))
                {
                    treasureIndex = candidateA;
                    specialIndex = candidateB;
                    break;
                }
            }
            if (treasureIndex != -1) break;
        }

        if (treasureIndex == -1 || specialIndex == -1) return false;

        assignedRooms[treasureIndex] = RoomType.Treasure;
        assignedRooms[specialIndex] = RoomType.Special;

        for (int i = 0; i < 100; i++)
        {
            if (floorPlan[i] == 1 && !assignedRooms.ContainsKey(i))
            {
                assignedRooms[i] = RoomType.Combat;
            }
        }
        return true;
    }

    private void SpawnMapVisuals()
    {
        foreach (var pair in assignedRooms)
        {
            int index = pair.Key;
            RoomType type = pair.Value;
            int x = index % 10;
            int y = index / 10;

            SpawnUICell(index, type, x, y);
            SpawnWorldRoom(index, type, x, y);
        }
    }

    private void SpawnUICell(int index, RoomType type, int x, int y)
    {
        int startX = StartIndex % 10;
        int startY = StartIndex / 10;
        
        Vector2 position = new Vector2((x - startX) * uiCellSize, (y - startY) * uiCellSize);
        UICellKret cell = Instantiate(uiCellPrefab, minimapContainer);
        cell.GetComponent<RectTransform>().anchoredPosition = position;
        
        Sprite iconToUse = GetSpriteForType(type);
        cell.Setup(index, type, iconToUse);
        spawnedUICells.Add(cell);
    }

    private void SpawnWorldRoom(int index, RoomType type, int x, int y)
    {
        GameObject prefabToSpawn = GetRandomPrefabForType(type);
        if (prefabToSpawn == null) 
        {
            return;
        }

        int startX = StartIndex % 10;
        int startY = StartIndex / 10;

        float elevation = index * elevationOffset;

        Vector3 position = new Vector3((x - startX) * worldRoomSpacing, elevation, (y - startY) * worldRoomSpacing);
        GameObject roomInstance = Instantiate(prefabToSpawn, position, Quaternion.identity, worldRoomsContainer);
        spawnedWorldRooms.Add(roomInstance);

        RoomControllerKret controller = roomInstance.GetComponent<RoomControllerKret>();
        if (controller != null)
        {
            bool hasUp = (y < 9 && floorPlan[index + 10] == 1);
            bool hasDown = (y > 0 && floorPlan[index - 10] == 1);
            bool hasRight = (x < 9 && floorPlan[index + 1] == 1);
            bool hasLeft = (x > 0 && floorPlan[index - 1] == 1);
            
            controller.SetupDoors(hasUp, hasDown, hasRight, hasLeft);
            spawnedRoomControllers[index] = controller; 
        }

        if (type == RoomType.Start)
        {
            TeleportPlayerToStartRoom(roomInstance);
        }
    }

    private void LinkAllRooms()
    {
        foreach (var kvp in spawnedRoomControllers)
        {
            int index = kvp.Key;
            RoomControllerKret currentRoom = kvp.Value;

            int x = index % 10;
            int y = index / 10;

            if (y < 9 && floorPlan[index + 10] == 1 && spawnedRoomControllers.ContainsKey(index + 10))
            {
                currentRoom.LinkRoom(spawnedRoomControllers[index + 10], "UP");
            }
            if (y > 0 && floorPlan[index - 10] == 1 && spawnedRoomControllers.ContainsKey(index - 10))
            {
                currentRoom.LinkRoom(spawnedRoomControllers[index - 10], "DOWN");
            }
            if (x < 9 && floorPlan[index + 1] == 1 && spawnedRoomControllers.ContainsKey(index + 1))
            {
                currentRoom.LinkRoom(spawnedRoomControllers[index + 1], "RIGHT");
            }
            if (x > 0 && floorPlan[index - 1] == 1 && spawnedRoomControllers.ContainsKey(index - 1))
            {
                currentRoom.LinkRoom(spawnedRoomControllers[index - 1], "LEFT");
            }
        }
    }

    private void TeleportPlayerToStartRoom(GameObject startRoomInstance)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Transform spawnPoint = startRoomInstance.transform.Find("Spawn");
            if (spawnPoint != null)
            {
                player.transform.position = spawnPoint.position;
            }
            else
            {
                player.transform.position = startRoomInstance.transform.position;
            }
        }
    }

    private GameObject GetRandomPrefabForType(RoomType type)
    {
        foreach (var data in roomDataPool)
        {
            if (data.roomType == type && data.roomPrefabs.Count > 0)
            {
                int rnd = Random.Range(0, data.roomPrefabs.Count);
                return data.roomPrefabs[rnd];
            }
        }
        return null;
    }

    private Sprite GetSpriteForType(RoomType type)
    {
        return type switch
        {
            RoomType.Start => startIcon,
            RoomType.Combat => combatIcon,
            RoomType.Treasure => treasureIcon,
            RoomType.Special => specialIcon,
            RoomType.Event => eventIcon,
            RoomType.Boss => bossIcon,
            RoomType.Exit => exitIcon,
            _ => null
        };
    }

    private List<int> GetAvailableNeighbours(int index)
    {
        List<int> neighbours = new List<int>();
        int x = index % 10;
        int y = index / 10;

        if (x > 1 && floorPlan[index - 1] == 0) neighbours.Add(index - 1);
        if (x < 8 && floorPlan[index + 1] == 0) neighbours.Add(index + 1);
        if (y > 1 && floorPlan[index - 10] == 0) neighbours.Add(index - 10);
        if (y < 8 && floorPlan[index + 10] == 0) neighbours.Add(index + 10);

        return neighbours;
    }

    private int GetNeighbourCount(int index)
    {
        int count = 0;
        int x = index % 10;
        int y = index / 10;

        if (x > 0 && floorPlan[index - 1] == 1) count++;
        if (x < 9 && floorPlan[index + 1] == 1) count++;
        if (y > 0 && floorPlan[index - 10] == 1) count++;
        if (y < 9 && floorPlan[index + 10] == 1) count++;

        return count;
    }

    private int GetFirstActiveNeighbour(int index)
    {
        int x = index % 10;
        int y = index / 10;

        if (x > 0 && floorPlan[index - 1] == 1) return index - 1;
        if (x < 9 && floorPlan[index + 1] == 1) return index + 1;
        if (y > 0 && floorPlan[index - 10] == 1) return index - 10;
        if (y < 9 && floorPlan[index + 10] == 1) return index + 10;

        return -1;
    }

    private bool AreAdjacent(int a, int b)
    {
        return a == b - 1 || a == b + 1 || a == b - 10 || a == b + 10;
    }

    private Dictionary<int, int> CalculateDistancesFrom(int start)
    {
        Dictionary<int, int> dist = new Dictionary<int, int>();
        Queue<int> q = new Queue<int>();
        
        dist[start] = 0;
        q.Enqueue(start);

        while (q.Count > 0)
        {
            int curr = q.Dequeue();
            int currDist = dist[curr];
            int[] offsets = { -1, 1, -10, 10 };

            foreach (int off in offsets)
            {
                int next = curr + off;
                if (next >= 0 && next < 100 && floorPlan[next] == 1 && !dist.ContainsKey(next))
                {
                    dist[next] = currDist + 1;
                    q.Enqueue(next);
                }
            }
        }
        return dist;
    }
}