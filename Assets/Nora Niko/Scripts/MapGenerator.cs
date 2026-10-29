using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Ustawienia Piętra")]
    [Range(1, 3)]
    [SerializeField] private int currentFloor = 1;
    [SerializeField] private float cellSize = 1.5f;

    [Header("Prefab")]
    [SerializeField] private Cell cellPrefab;

    [Header("Sprite References")]
    [SerializeField] private Sprite startIcon; //cell icon
    [SerializeField] private Sprite combatIcon;
    [SerializeField] private Sprite treasureIcon;
    [SerializeField] private Sprite specialIcon;
    [SerializeField] private Sprite eventIcon;
    [SerializeField] private Sprite bossIcon;
    [SerializeField] private Sprite exitIcon;

    //10x10 grid representation (1 = occupied room, 0 = empty)
    private int[] floorPlan;
    private int floorPlanCount;
    private int targetRoomCount;
    private const int StartIndex = 45; //center of the 10x10 layout

    private List<Cell> spawnedCells = new();
    private Dictionary<int, RoomType> assignedRooms = new();

    void Start()
    {
        GenerateFloor();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerateFloor();
        }
    }

    public void GenerateFloor()
    {
        //target floor limit
        if (currentFloor == 1) targetRoomCount = 8;
        else if (currentFloor == 2) targetRoomCount = 10;
        else targetRoomCount = 11;

        //safety counter to avoid infinite loops
        int safetyLimit = 500;
        while (safetyLimit-- > 0)
        {
            ClearMap();

            if (BuildLayout() && AssignRoomTypes())
            {
                SpawnMapVisuals();
                return;
            }
        }

        Debug.LogError("Failed to generate map.");
    }

    private void ClearMap()
    {
        foreach (var cell in spawnedCells)
        {
            if (cell != null) Destroy(cell.gameObject);
        }
        spawnedCells.Clear();
        assignedRooms.Clear();
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

            //shuffles neighbours for non-deterministic generation
            for (int i = 0; i < validNeighbours.Count; i++)
            {
                int temp = validNeighbours[i];
                int randomIndex = Random.Range(i, validNeighbours.Count);
                validNeighbours[i] = validNeighbours[randomIndex];
                validNeighbours[randomIndex] = temp;
            }

            foreach (int next in validNeighbours)
            {
                //restricts neighbours to prevent 2x2 blocks
                if (GetNeighbourCount(next) <= 1 && Random.value < 0.65f)
                {
                    floorPlan[next] = 1;
                    floorPlanCount++;
                    queue.Enqueue(next);

                    if (floorPlanCount >= targetRoomCount)
                        break;
                }
            }
        }

        return floorPlanCount >= targetRoomCount;
    }

    private bool AssignRoomTypes()
    {
        //START ROOM
        assignedRooms[StartIndex] = RoomType.Start;

        Dictionary<int, int> distances = CalculateDistancesFrom(StartIndex);

        //gathers all dead ends (rooms with strictly 1 neighbour)
        List<int> deadEnds = new List<int>();
        for (int i = 0; i < 100; i++)
        {
            if (floorPlan[i] == 1 && i != StartIndex && GetNeighbourCount(i) == 1)
            {
                deadEnds.Add(i);
            }
        }

        //we require at least 3 dead ends (Boss/Exit, Treasure, Special)
        if (deadEnds.Count < 3) return false;

        //1. CHOOSE BOSS ROOM (only on floor3) OR EXIT ROOM (floor 1 & 2)
        //furthest from spawn + NO room directly above (index - 10)
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

            //room before boss must be an event room
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

        //2. TREASURE ROOM & SPECIAL ROOM (must be dead ends and never next to each other)
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

        //3. THE REST OF THE ROOMS ARE COMBAT ROOMS
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
            Vector2 position = new Vector2(x * cellSize, -y * cellSize);

            Cell cell = Instantiate(cellPrefab, position, Quaternion.identity, transform);
            Sprite iconToUse = GetSpriteForType(type);

            cell.Setup(index, type, iconToUse);
            spawnedCells.Add(cell);
        }
    }

    private Sprite GetSpriteForType(RoomType type)
    {
        return type switch
        {
            RoomType.Start => startIcon != null ? startIcon : combatIcon,
            RoomType.Combat => combatIcon,
            RoomType.Treasure => treasureIcon,
            RoomType.Special => specialIcon,
            RoomType.Event => eventIcon,
            RoomType.Boss => bossIcon,
            RoomType.Exit => exitIcon != null ? exitIcon : bossIcon,
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