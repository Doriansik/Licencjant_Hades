using UnityEngine;

public class RoomControllerKret : MonoBehaviour
{
    [SerializeField] private GameObject upEnviro;
    [SerializeField] private GameObject downEnviro;
    [SerializeField] private GameObject rightEnviro;
    [SerializeField] private GameObject leftEnviro;

    [SerializeField] private GameObject upDoorVisual;
    [SerializeField] private GameObject downDoorVisual;
    [SerializeField] private GameObject rightDoorVisual;
    [SerializeField] private GameObject leftDoorVisual;

    [SerializeField] private RoomTransitionKret upTransition;
    [SerializeField] private RoomTransitionKret downTransition;
    [SerializeField] private RoomTransitionKret rightTransition;
    [SerializeField] private RoomTransitionKret leftTransition;

    public Transform upArrival;
    public Transform downArrival;
    public Transform rightArrival;
    public Transform leftArrival;

    public void SetupDoors(bool hasUp, bool hasDown, bool hasRight, bool hasLeft)
    {
        if (upEnviro) upEnviro.SetActive(!hasUp);
        if (downEnviro) downEnviro.SetActive(!hasDown);
        if (rightEnviro) rightEnviro.SetActive(!hasRight);
        if (leftEnviro) leftEnviro.SetActive(!hasLeft);

        if (upDoorVisual) 
        {
            upDoorVisual.SetActive(hasUp);
            if (hasUp && !upDoorVisual.activeInHierarchy)
                Debug.LogWarning($"[UWAGA] {gameObject.name}: Skrypt włączył UP DOOR, ale obiekt nadal jest ukryty! Sprawdź, czy folder 'Doors' jest włączony w prefabie!");
        }
        if (downDoorVisual) downDoorVisual.SetActive(hasDown);
        if (rightDoorVisual) rightDoorVisual.SetActive(hasRight);
        if (leftDoorVisual) leftDoorVisual.SetActive(hasLeft);

        if (upTransition) upTransition.gameObject.SetActive(hasUp);
        if (downTransition) downTransition.gameObject.SetActive(hasDown);
        if (rightTransition) rightTransition.gameObject.SetActive(hasRight);
        if (leftTransition) leftTransition.gameObject.SetActive(hasLeft);
    }

    public void LinkRoom(RoomControllerKret targetRoom, string direction)
    {
        if (targetRoom == null) return;

        if (direction == "UP" && upTransition != null)
        {
            if (targetRoom.downArrival == null) Debug.LogError($"Target Room {targetRoom.gameObject.name} is missing DOWN ARRIVAL/SPAWN point!");
            upTransition.targetTeleportPoint = targetRoom.downArrival;
        }
        else if (direction == "DOWN" && downTransition != null)
        {
            if (targetRoom.upArrival == null) Debug.LogError($"Target Room {targetRoom.gameObject.name} is missing UP ARRIVAL/SPAWN point!");
            downTransition.targetTeleportPoint = targetRoom.upArrival;
        }
        else if (direction == "RIGHT" && rightTransition != null)
        {
            if (targetRoom.leftArrival == null) Debug.LogError($"Target Room {targetRoom.gameObject.name} is missing LEFT ARRIVAL/SPAWN point!");
            rightTransition.targetTeleportPoint = targetRoom.leftArrival;
        }
        else if (direction == "LEFT" && leftTransition != null)
        {
            if (targetRoom.rightArrival == null) Debug.LogError($"Target Room {targetRoom.gameObject.name} is missing RIGHT ARRIVAL/SPAWN point!");
            leftTransition.targetTeleportPoint = targetRoom.rightArrival;
        }
    }

    [ContextMenu("Auto Setup References")]
    private void AutoSetupReferences()
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            string n = child.name.ToUpper();

            if (n == "UP") upEnviro = child.gameObject;
            if (n == "DOWN") downEnviro = child.gameObject;
            if (n == "RIGHT") rightEnviro = child.gameObject;
            if (n == "LEFT") leftEnviro = child.gameObject;

            if (n.Contains("UP DOOR")) upDoorVisual = child.gameObject;
            if (n.Contains("DOWN DOOR")) downDoorVisual = child.gameObject;
            if (n.Contains("RIGHT DOOR")) rightDoorVisual = child.gameObject;
            if (n.Contains("LEFT DOOR")) leftDoorVisual = child.gameObject;

            if (n == "UP ENT") upTransition = child.GetComponent<RoomTransitionKret>();
            if (n == "DOWN ENT") downTransition = child.GetComponent<RoomTransitionKret>();
            if (n == "RIGHT ENT") rightTransition = child.GetComponent<RoomTransitionKret>();
            if (n == "LEFT ENT") leftTransition = child.GetComponent<RoomTransitionKret>();

            if (n.Contains("UP") && (n.Contains("ARRIVAL") || n.Contains("SPAWN"))) upArrival = child;
            if (n.Contains("DOWN") && (n.Contains("ARRIVAL") || n.Contains("SPAWN"))) downArrival = child;
            if (n.Contains("RIGHT") && (n.Contains("ARRIVAL") || n.Contains("SPAWN"))) rightArrival = child;
            if (n.Contains("LEFT") && (n.Contains("ARRIVAL") || n.Contains("SPAWN"))) leftArrival = child;
        }
    }
}