using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;
using static UnityEditor.Progress;

public class MapManager: MonoBehaviour
    {
     
    

    [Header("Map Dimensions")]
    public MapManager Instance;
    [SerializeField]  public int mapWidth;
    [SerializeField]  public int mapHeight;


    [Header("Generation Parameters")]
    [Header("0-10")][SerializeField] float threshold;
    public float scale = 1.0F;

    [Space(20)]


    [Header("Tiles")]
    [SerializeField] private Tile Black;
    [SerializeField] private Tile White;
    [SerializeField] public Tile Red;
    [SerializeField] private Tile yellow;


    
    public Tilemap map;
    public Cell[,] cells;
    public List<List<Cell>> rooms;
    public HashSet<Cell> floorCells;


    //Pathfinding
    public Cell target;
    public GameObject Agent;


    
    // The origin of the sampled area in the perlin noise plane, randomized each generation to get a diferent map
    float x0;
    float y0;



    private void Awake()
    {
        cells = new Cell[mapWidth, mapHeight];
        floorCells = new HashSet<Cell>();
        map = GameObject.Find("Grid").transform.Find("Tilemap").GetComponent<Tilemap>();
    }


    public void generateTerrain()
    {

        //do required initial steps 
        InitTerrainGeneration();


        //iterate mapWidth and Height
        float y = 0.0F;
        while( y < mapHeight )
        {
            float x = 0.0F;
            while (x < mapWidth)
            {
                //exclude edges
                if (x < 2.0F || x > mapWidth - 3 || y < 2.0F || y > mapHeight - 3)
                {
                    map.SetTile(new Vector3Int((int)x, (int)y), Black);
                    cells[(int)x, (int)y] = new Cell(new Vector2Int((int)x, (int)y), true, false);
                }
                else 
                {
                    //calculate the noise value for the corresponding cell and decide if its ground or not.
                    CalcNoise(x, y);
                }
                x++;
            }
            y++;
        }

        //add neighbours to the cells
        for(int y2 = 2; y2 < mapHeight - 2; y2++)
        {
            for(int x2 = 2; x2 < mapWidth - 2; x2++)
            {
                FindNeighbors(cells[x2, y2]);
            }
        }

        placeTarget();

        placeAgent();
        
        

    }

    public void InitTerrainGeneration()
    {
        //threshold max Value is 10, as its meant to truncate at a point bwtween 0.4 and 0.5
        if (Mathf.Abs(threshold) > 10)
        {
            threshold = 10;
        }

        removeAgents();
        //randomize starting point in the perlin noise 1.0 by 1.0 plane 
        x0 = Random.value * scale;
        y0 = Random.value * scale;


        //initially fill the map with black cells
        for (int y = 0; y < mapWidth; y++)
        {
            for (int x = 0; x < mapHeight; x++)
            {
                cells[(int)x, (int)y] = new Cell(new Vector2Int(x, y), true, true);
                map.SetTile(new Vector3Int(x, y), Black);
            }
        }
    }


    private void CalcNoise(float x, float y)
    {
        
        //using the origin coordinates and the scale value compute the real grid position of the cell
        float xCoord =x0 + x/ mapWidth * scale;
        float yCoord =y0 + y/ mapHeight * scale;

        //use the threshold value tu truncate the noise
        if( Mathf.PerlinNoise(xCoord, yCoord) > (0.4f + (threshold/100)))
        {
            //set the tile
            map.SetTile(new Vector3Int((int)x, (int)y), White);
 
            //create the asociated cell
            cells[(int)x, (int)y] = new Cell(new Vector2Int((int)x, (int)y), false, false);
             
            floorCells.Add(cells[(int)x, (int)y]);

        }
    }

    private void FindNeighbors(Cell cell)
    {
        //|-1,-1| 0,-1| +1,-1|

        //|-1, 0| 0, 0| +1, 0|

        //|-1,+1| 0,+1| +1,+1|

       
        /*| -1,-1 |*/ 
        //if(cells[cell.GetPosition().x - 1, cell.GetPosition().y - 1].IsWalkable()) cell.AddNeighbor(cells[cell.GetPosition().x - 1, cell.GetPosition().y - 1]);

        /*|  0,-1 |*/
        if (cells[cell.GetPosition().x   , cell.GetPosition().y - 1].IsWalkable()) cell.AddNeighbor(cells[cell.GetPosition().x    , cell.GetPosition().y - 1]);

        /*| +1,-1 |*/
        //if (cells[cell.GetPosition().x + 1, cell.GetPosition().y - 1].IsWalkable()) cell.AddNeighbor(cells[cell.GetPosition().x + 1, cell.GetPosition().y - 1]);

        /*| -1, 0 |*/
        if (cells[cell.GetPosition().x - 1, cell.GetPosition().y    ].IsWalkable()) cell.AddNeighbor(cells[cell.GetPosition().x - 1, cell.GetPosition().y    ]);

        /*| +1, 0 |*/
        if (cells[cell.GetPosition().x + 1, cell.GetPosition().y    ].IsWalkable()) cell.AddNeighbor(cells[cell.GetPosition().x + 1, cell.GetPosition().y    ]);

        /*| -1,+1 |*/
        //if (cells[cell.GetPosition().x - 1, cell.GetPosition().y + 1].IsWalkable()) cell.AddNeighbor(cells[cell.GetPosition().x - 1, cell.GetPosition().y + 1]);

        /*|  0,+1 |*/
        if (cells[cell.GetPosition().x    , cell.GetPosition().y + 1].IsWalkable()) cell.AddNeighbor(cells[cell.GetPosition().x    , cell.GetPosition().y + 1]);

        /*| +1,+1 |*/
        //if (cells[cell.GetPosition().x + 1, cell.GetPosition().y + 1].IsWalkable()) cell.AddNeighbor(cells[cell.GetPosition().x + 1, cell.GetPosition().y + 1]);
    }


    //UNDER DEVELOPMENT
    /* 
    private void FloodRooms()
    {
        
        List<Cell> openList = new List<Cell>();
        foreach (Cell cell in cells)
        {
            openList.Add(cell);
        }

        while(openList.Count > 0)
        {

            HashSet<Cell> room = new HashSet<Cell>();
            HashSet<Cell> workingSet = new HashSet<Cell>();
            Cell initialCell = openList[Random.Range(0, openList.Count-1)];
            openList.Remove(initialCell);
            room.Add(initialCell);
            var roomFlooded = false;

            while (roomFlooded == false)
            {
                var initialCount = room.Count;
                foreach(Cell cell in room)
                {
                    workingSet.UnionWith(cell.GetNeighbors());
                }

                room.UnionWith(workingSet);
         
                foreach(Cell cell in room)
                {
                    openList.Remove(cell);
                    map.SetTile(new Vector3Int(cell.GetPosition().x , cell.GetPosition().y), Red);
                }
                var passCount = room.Count;
                if (passCount == initialCount)
                {
                    roomFlooded = true;
                }
            }
        }
    }
   */

    //==================
    // Aux Functions
    //=================
    private void removeAgents()
    {
        GameObject[] agents = GameObject.FindGameObjectsWithTag("Agent");
        foreach (GameObject agent in agents)
        {
            Destroy(agent);
        }
    }
    private void placeTarget()
    {
        //find a tile to be the target
        do { target = floorCells.ElementAt(Random.Range(0, floorCells.Count - 1)); }
        while (map.GetTile(target.GetPosition3()).Equals(Black));

        map.SetTile(target.GetPosition3(), yellow);

    }

    private void placeAgent()
    {
        Cell agentCell;
        do { agentCell = floorCells.ElementAt(Random.Range(0, floorCells.Count - 1)); }
        while (map.GetTile(agentCell.GetPosition3()).Equals(Black));
        Instantiate(Agent, new Vector3(agentCell.GetPosition().x + 0.5f, agentCell.GetPosition().y + 0.5f), new Quaternion(0, 0, 0, 0));
    }
}