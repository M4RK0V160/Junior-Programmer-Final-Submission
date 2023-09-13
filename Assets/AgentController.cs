using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class AgentController : MonoBehaviour
{
    private MapManager mapManager;
    private Cell target;
    private Vector2Int targetPosition;
    private List<Cell> path;
    private AStarPath pathfinder;
    private Cell occupiedCell;


    void Start()
    {
        mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();
        pathfinder = new AStarPath();
        target = mapManager.target;
        occupiedCell = mapManager.cells[(int)transform.position.x, (int)transform.position.y];
        targetPosition = new Vector2Int(target.GetPosition().x, target.GetPosition().y);
        path = pathfinder.CalculatePath(occupiedCell, target);
    }

    void Update()
    {
        target = mapManager.target;
        occupiedCell = mapManager.cells[(int)transform.position.x, (int)transform.position.y];

        if (target.GetPosition() != targetPosition)
        {
            path = pathfinder.CalculatePath(occupiedCell, target);
        }

        if (Input.GetKeyUp(KeyCode.S))
        {
            Debug.Log("Steping");
            Step();
        }
        
        if (path != null)
        {
            foreach (Cell cell in path)
            {
                if (cell != path.Last())
                {
                    mapManager.map.SetTile(cell.GetPosition3(), mapManager.Red);
                }
            }
        }
        



    }


    private void Step()
    {
        var nextCell = path.First();
        transform.position = new Vector3(nextCell.GetPosition().x, nextCell.GetPosition().y);
        path.Remove(nextCell);

    }
}
