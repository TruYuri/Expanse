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

    [SerializeField] private float _cameraSpeed = 0;
    [SerializeField] private Vector3 _cameraOffset = Vector3.zero;
    [SerializeField] private float _minCamDistance = 0;
    [SerializeField] private float _maxCamDistance = 0;

    private Vector3 _currentCameraDistance;
    private Dictionary<Sector, bool> _exploredSectors;

    public static HumanPlayer Instance { get { return _instance; } } // move this to a GameManager registry!
    public Squad Squad { get { return _controlledSquad; } }
    public Tile Tile { get { return _controlledTile; } }
    public Dictionary<Sector, bool> ExploredSectors { get { return _exploredSectors; } }
    public Squad CommandSquad { get { return _commandShipSquad; } }

    public override void Init(Team team)
    {
        _instance = this;

        base.Init(team);
        _currentCameraDistance = _cameraOffset;
        _exploredSectors = new Dictionary<Sector, bool>();
        // create command ship, look at it, control it 
        var squad = CreateNewSquad(new Vector3(0, 0, 10.5f), null);
        AddShip(squad, "Base");
        AddShip(squad, "Gathering Complex");
        AddShip(squad, "Military Complex");
        AddShip(squad, "Research Complex");

        squad = CreateNewSquad(new Vector3(0, 0, 11f), null);
        AddShip(squad, "Fighter");
        AddShip(squad, "Transport");
        var t1 = AddShip(squad, "Transport");
        AddShip(squad, "Transport");
        AddShip(squad, "Transport");
        var t4 = AddShip(squad, "Transport");
        AddShip(squad, "Heavy Fighter");
        AddShip(squad, "Behemoth");
        var r = AddShip(squad, "Resource Transport");
        var r1 = AddShip(squad, "Resource Transport");

        AddResources(r, Resource.Ore, 100);
        AddResources(r, Resource.Oil, 50);
        AddResources(r, Resource.Forest, 25);
        AddResources(r, Resource.Asterminium, 25);

        AddResources(r1, Resource.Ore, 25);
        AddResources(r1, Resource.Oil, 25);
        AddResources(r1, Resource.Forest, 10);
        AddResources(r1, Resource.Asterminium, 10);

        AddSoldiers(t1, Inhabitance.Primitive, 50);
        AddSoldiers(t1, Inhabitance.Industrial, 50);
        AddSoldiers(t1, Inhabitance.SpaceAge, 50);

        AddSoldiers(t4, Inhabitance.Primitive, 5);
        AddSoldiers(t4, Inhabitance.Industrial, 10);
        AddSoldiers(t4, Inhabitance.SpaceAge, 15);
        /* debug */

        _controlledIsWithinRange = true;
        Camera.main.transform.position = _commandShipSquad.transform.position + _currentCameraDistance;
        Camera.main.transform.LookAt(_commandShipSquad.transform);
        _currentCameraDistance = Camera.main.transform.position - _commandShipSquad.transform.position;
        GUIManager.Instance.SetSquadList(false);
        GUIManager.Instance.SetTileList(false);
        GUIManager.Instance.SetWarpList(false);
        GUIManager.Instance.SetScreen("MainUI");
    }

    void Start()
    {
        // claim a starting tile



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


        if (_controlledSquad != null)
        {
            var scrollChange = -Input.GetAxis(MOUSE_SCROLLWHEEL);

            float s = 0;
            var dir = -Camera.main.transform.forward;
            var dist = (_controlledSquad.transform.position - Camera.main.transform.position).magnitude;
            if (scrollChange < 0)
                s = Mathf.Min(_cameraSpeed, dist - _minCamDistance);
            else if (scrollChange > 0)
                s = Mathf.Min(_cameraSpeed, _maxCamDistance - dist);

            var change = s * scrollChange * dir;

            _currentCameraDistance += change;
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position,
                _controlledSquad.transform.position + _currentCameraDistance, Mathf.Clamp01(Time.deltaTime * _cameraSpeed));

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
                && _controlledSquad.Team == _team && _controlledSquad.Mission == null)
            {
                Sector sector = null;

                switch(hit.collider.tag)
                {
                    case SQUAD_TAG:
                        sector = hit.collider.GetComponent<Squad>().Sector;
                        break;
                    case SECTOR_TAG:
                        sector = hit.collider.GetComponent<Sector>();
                        break;
                }

                if(sector != null)
                    CreateTravelEvent(_controlledSquad, sector, hit.point, 10.0f);
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
            if (Physics.Raycast(ray, out hit, float.PositiveInfinity, ~LayerMask.NameToLayer("Sector")))
            {
                float speed = 25.0f;

                var dir = hit.point - _commandShipSquad.transform.position;
                dir.Normalize();
                _commandShipSquad.transform.position += dir * speed * Time.deltaTime;

                _commandShipSquad.transform.position = new Vector3(_commandShipSquad.transform.position.x, 0.0f, _commandShipSquad.transform.position.z);
                _commandShipSquad.transform.LookAt(hit.point);
            }
        }

        GUIManager.Instance.SetSquadControls(_controlledSquad);
    }

    public void DisplayResearch(string type, string name, GameObject panel)
    {
        if(type == "Military")
        {
            _militaryTree.GetResearch(name).Display(panel, _resourceRegistry[Resource.Stations]);
        }
        else if(type == "Scientific")
        {
            _scienceTree.GetResearch(name).Display(panel, _resourceRegistry[Resource.Stations]);
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
                GUIManager.Instance.TileSelected(_controlledTile, _shipDefinitions);
                break;
            case SQUAD_TAG:
            case COMMAND_SHIP_TAG:
                GUIManager.Instance.SquadSelected(_controlledSquad);
                break;
        }
    }

    public override void Control(GameObject gameObject)
    {
        if (_controlledSquad == null || (_controlledSquad != null && gameObject != _controlledSquad.gameObject))
        {
            base.Control(gameObject);
            transform.position = _controlledSquad.transform.position + _currentCameraDistance;
        }

        if (_controlledSquad.Sector != null && _controlledSquad.Sector != null)
        {
            var colors = new Dictionary<Sector, Color>() { { _controlledSquad.Sector, Color.black } };

            if (colors.ContainsKey(_commandShipSquad.Sector))
                colors[_commandShipSquad.Sector] = Color.magenta;
            else
                colors.Add(_commandShipSquad.Sector, Color.magenta);

            var minimap = MapManager.Instance.GenerateMap(colors);
            GUIManager.Instance.UpdateMinimap(minimap, _controlledSquad.transform.position, _controlledSquad.Sector);
        }

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

