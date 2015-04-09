﻿using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

public class HumanPlayer : Player
{
    public static HumanPlayer _instance;
    private const string COMMAND_SHIP_TAG = "CommandShip";
    private const string SQUAD_TAG = "Squad";
    private const string TILE_TAG = "Tile";
    private const string SECTOR_TAG = "Sector";
    private const string COMMAND_SHIP_PREFAB = "CommandShip";
    private const string MOUSE_SCROLLWHEEL = "Mouse ScrollWheel";
    private readonly Vector3 CAMERA_OFFSET = new Vector3(0, 20, -13);

    private Vector3 _currentCameraDistance;
    public static HumanPlayer Instance { get { return _instance; } } // move this to a GameManager registry!
    public Squad Squad { get { return _controlledSquad; } }
    public Tile Tile { get { return _controlledTile; } }

    public override void Init(Team team)
    {
        _instance = this;

        base.Init(team);
        _currentCameraDistance = (_commandShipSquad.transform.position + CAMERA_OFFSET) - _commandShipSquad.transform.position; 

        // create command ship, look at it, control it 
        var squad = CreateNewSquad(new Vector3(0, 0, 10.5f), null);
        squad.Ships.Add(_shipDefinitions["Base"].Copy());
        squad.Ships.Add(_shipDefinitions["Gathering Complex"].Copy());
        squad.Ships.Add(_shipDefinitions["Military Complex"].Copy());
        squad.Ships.Add(_shipDefinitions["Research Complex"].Copy());

        Ship b, t, t1, t2, t3, t4, f, r, r1;
        squad = CreateNewSquad(new Vector3(0, 0, 11f), null);
        squad.Ships.Add(_shipDefinitions["Fighter"].Copy());
        squad.Ships.Add(t = _shipDefinitions["Transport"].Copy());
        squad.Ships.Add(t1 = _shipDefinitions["Transport"].Copy());
        squad.Ships.Add(t2 = _shipDefinitions["Transport"].Copy());
        squad.Ships.Add(t3 = _shipDefinitions["Transport"].Copy());
        squad.Ships.Add(t4 = _shipDefinitions["Transport"].Copy());
        squad.Ships.Add(f = _shipDefinitions["Heavy Fighter"].Copy());
        squad.Ships.Add(b = _shipDefinitions["Behemoth"].Copy());
        squad.Ships.Add(r = _shipDefinitions["Resource Transport"].Copy());
        squad.Ships.Add(r1 = _shipDefinitions["Resource Transport"].Copy());
        b.PrimitivePopulation = 25;
        b.IndustrialPopulation = 15;
        b.SpaceAgePopulation = 10;
        f.SpaceAgePopulation = 3;
        t.PrimitivePopulation = t1.PrimitivePopulation = t2.PrimitivePopulation = t3.PrimitivePopulation = t4.PrimitivePopulation = 50;
        r.Resources[Resource.Ore] = 100;
        r.Resources[Resource.Oil] = 50;
        r.Resources[Resource.Forest] = 25;
        r.Resources[Resource.Asterminium] = 25;
        r1.Resources[Resource.Ore] = 25;
        r1.Resources[Resource.Oil] = 25;
        r1.Resources[Resource.Forest] = 10;
        r1.Resources[Resource.Asterminium] = 10;
        /* debug */

        _controlledIsWithinRange = true;
        Camera.main.transform.position = _commandShipSquad.transform.position + CAMERA_OFFSET;
        Camera.main.transform.LookAt(_commandShipSquad.transform);
        GUIManager.Instance.SetSquadList(false);
        GUIManager.Instance.SetTileList(false);
        GUIManager.Instance.SetScreen("MainUI");
    }

    void Start()
    {
    }

    void Update()
    {
        if (GameManager.Instance.Paused || _turnEnded)
            return;

        // right click - control
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            EventSystem eventSystem = EventSystem.current;
            if (Physics.Raycast(ray, out hit) && !eventSystem.IsPointerOverGameObject())
            {
                switch(hit.collider.tag)
                {
                    case SQUAD_TAG:
                        Control(hit.collider.gameObject);
                        break;
                    case SECTOR_TAG:
                        var sector = hit.collider.gameObject.GetComponent<Sector>();
                        var tile = sector.GetTileAtPosition(hit.point);
                        if(tile != null)
                            Control(tile.gameObject);
                        break;
                }

                ReloadGameplayUI();
            }
        }

        if (Input.GetKey(KeyCode.C))
        {
            Control(_commandShipSquad.gameObject);
            ReloadGameplayUI();
        }

        var scrollChange = Input.GetAxis(MOUSE_SCROLLWHEEL);
        var change = 50.0f * scrollChange * Camera.main.transform.forward;
        Camera.main.transform.position += change;
        _currentCameraDistance += change;

        if (_controlledSquad != null)
        {
            transform.position = _controlledSquad.transform.position + _currentCameraDistance;
            switch (_controlledSquad.tag)
            {
                case TILE_TAG:
                    UpdateSelectedPlanet();
                    break;
                case SQUAD_TAG:
                    if (_commandShipSquad == _controlledSquad)
                        UpdateCommandShip();
                    else
                        UpdateSquad();
                    break;
            }
        }
    }

    private void UpdateSelectedPlanet()
    {
        GUIManager.Instance.SetSquadControls(_controlledSquad);
    }

    private void UpdateSquad()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            EventSystem eventSystem = EventSystem.current;
            if (Physics.Raycast(ray, out hit) && !eventSystem.IsPointerOverGameObject()
                && _controlledSquad.Team == _team && _controlledSquad.OnMission == false)
            {
                CreateTravelEvent(_controlledSquad, hit.collider.gameObject.GetComponent<Sector>(), hit.point, 10.0f);
            }
            
        }

        GUIManager.Instance.SetSquadControls(_controlledSquad);
    }

    private void UpdateCommandShip()
    {
        EventSystem eventSystem = EventSystem.current;
        if (Input.GetMouseButton(0) && !eventSystem.IsPointerOverGameObject())
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                float speed = 25.0f;

                var dir = hit.point - _commandShipSquad.transform.position;
                dir.Normalize();
                _commandShipSquad.transform.position += dir * speed * Time.deltaTime;

                _commandShipSquad.transform.position = new Vector3(_commandShipSquad.transform.position.x, 0.0f, _commandShipSquad.transform.position.z);
                _commandShipSquad.transform.LookAt(hit.point);
                transform.position = _commandShipSquad.transform.position + _currentCameraDistance;
            }
        }

        GUIManager.Instance.SetSquadControls(_controlledSquad);
    }

    public void DisplayResearch(string type, string name, GameObject panel)
    {
        if(type == "Military")
        {
            _militaryTree.GetResearch(name).Display(panel, _numResearchStations);
        }
        else if(type == "Scientific")
        {
            _scienceTree.GetResearch(name).Display(panel, _numResearchStations);
        }
    }

    public Ship GetShipDefinition(string name)
    {
        return _shipDefinitions[name];
    }

    public override void TurnEnd()
    {
        base.TurnEnd();
        ReloadGameplayUI();
    }

    public void ReloadGameplayUI()
    {
        if (_controlledSquad == null)
            return;

        switch(_controlledSquad.tag)
        {
            case TILE_TAG:
                GUIManager.Instance.TileSelected(_controlledTile, _numResearchStations, _shipDefinitions);
                break;
            case SQUAD_TAG:
            case COMMAND_SHIP_TAG:
                GUIManager.Instance.SquadSelected(_controlledSquad);
                break;
        }
    }

    public override void Control(GameObject gameObject)
    {
        base.Control(gameObject);
        transform.position = _controlledSquad.transform.position + _currentCameraDistance;
        ReloadGameplayUI();
    }

    public override float PrepareBattleConditions(Squad squad1, Squad squad2, BattleType battleType)
    {
        _winChance = base.PrepareBattleConditions(squad1, squad2, battleType);
        Control(_playerSquad.gameObject);
        GUIManager.Instance.ConfigureBattleScreen(_winChance, _playerSquad, _enemySquad);
        return _winChance;
    }

    public override KeyValuePair<KeyValuePair<Team, BattleType>, Dictionary<string, int>> Battle(float playerChance, BattleType battleType, Squad player, Squad enemy)
    {
        return base.Battle(_winChance, _currentBattleType, _playerSquad, _enemySquad);
    }
}

