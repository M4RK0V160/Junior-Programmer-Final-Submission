
using System;
using Unity.VisualScripting;
using UnityEngine;



public enum State
{
    game,
    menu
}
public class mainManager : MonoBehaviour
{

    //Game Managing stuff
    public mainManager Instance;
    private State state = State.game;



    //Map Stuff
    private MapManager mapManager;
    public Grid grid;



    //Input Stuff
    InputActions inputActions;


    private void Awake()
    {
        inputActions = new InputActions();
    }

    private void OnEnable()
    {
        inputActions.BaseInput.Enable();
    }
    private void OnDisable()
    {
        inputActions.BaseInput.Disable();
    }



    void Start()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        grid = GameObject.Find("Grid").GetComponent<Grid>();
        mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            mapManager.generateTerrain();
        }

    }







    //==============
    //AUX FUNCTIONS
}
