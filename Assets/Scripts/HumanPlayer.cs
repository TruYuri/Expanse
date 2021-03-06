﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The Human player override of the Player class.
/// </summary>
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
    private AudioSource _move;

    public static HumanPlayer Instance 
    { 
        get 
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<HumanPlayer>();
            }

            return _instance; 
        } 
    }

    public Squad Squad { get { return _controlledSquad; } }
    public Tile Tile { get { return _controlledTile; } }
    public Squad CommandSquad { get { return _commandShipSquad; } }

    /// <summary>
    /// Initializes the human player.
    /// </summary>
    /// <param name="team">The human player's team.</param>
    public override void Init(Team team)
    {
        _instance = this;

        base.Init(team);
        CreateNewCommandShip(null);

        _currentCameraDistance = _cameraOffset;
        _move = GetComponents<AudioSource>().Where(s => s.clip.name == "Engine Move").ToList()[0];

        var squad = CreateNewSquad(Vector3.zero, _commandShipSquad.Sector);
        AddShip(squad, "Warp Portal");
        AddShip(squad, "Warp Portal");
        AddShip(squad, "Relay");
        AddShip(squad, "Base");
        AddShip(squad, "Research Complex");
        AddShip(squad, "Gathering Complex");
        AddShip(squad, "Military Complex");
        var t = AddShip(squad, "Transport");
        var t2 = AddShip(squad, "Transport");
        var t3 = AddShip(squad, "Transport");
        var t4 = AddShip(squad, "Transport");
        var t5 = AddShip(squad, "Transport");
        var t6 = AddShip(squad, "Transport");
        var t7 = AddShip(squad, "Transport");
        var t8 = AddShip(squad, "Transport");

        AddSoldiers(t, Inhabitance.SpaceAge, 100);
        AddSoldiers(t2, Inhabitance.SpaceAge, 100);
        AddSoldiers(t3, Inhabitance.SpaceAge, 100);
        AddSoldiers(t4, Inhabitance.SpaceAge, 100);
        AddSoldiers(t5, Inhabitance.SpaceAge, 100);
        AddSoldiers(t6, Inhabitance.SpaceAge, 100);
        AddSoldiers(t7, Inhabitance.SpaceAge, 100);
        AddSoldiers(t8, Inhabitance.SpaceAge, 100);

        _controlledIsWithinRange = true;
        Camera.main.transform.position = _commandShipSquad.transform.position + _currentCameraDistance;
        Camera.main.transform.LookAt(_commandShipSquad.transform);
        _currentCameraDistance = Camera.main.transform.position - _commandShipSquad.transform.position;
        GUIManager.Instance.SetSquadList(false);
        GUIManager.Instance.SetTileList(false);
        GUIManager.Instance.SetWarpList(false);
        GUIManager.Instance.SetScreen("MainUI");
        GUIManager.Instance.SetZoom(null, true);
    }

    /// <summary>
    /// Updates the human player based on UI interaction.
    /// </summary>
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
                        if (hit.collider.gameObject.GetComponent<Squad>() != _controlledSquad)
                            GUIManager.Instance.PlaySound("SquadSelect");
                        Control(hit.collider.gameObject);
                        break;
                    case SECTOR_TAG:
                        var sector = hit.collider.gameObject.GetComponent<Sector>();
                        var tile = sector.GetTileAtPosition(hit.point);
                        if (tile != null)
                        {
                            if (tile != _controlledTile)
                                GUIManager.Instance.PlaySound("TileSelect");
                            Control(tile.gameObject);
                        }
                        break;
                }

                ReloadGameplayUI();
            }
        }

        if (Input.GetKey(KeyCode.C))
        {
            Control(_commandShipSquad.gameObject);
            GUIManager.Instance.PlaySound("SquadSelect");
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

    /// <summary>
    /// Updates the UI for the selected planet.
    /// </summary>
    private void UpdateSelectedPlanet()
    {
        GUIManager.Instance.SetSquadControls(_controlledSquad);
    }

    /// <summary>
    /// Updates the movement behavior and UI for the selected squad.
    /// </summary>
    private void UpdateSquad()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            EventSystem eventSystem = EventSystem.current;
            if (Physics.Raycast(ray, out hit) && !eventSystem.IsPointerOverGameObject()
                && _controlledSquad.Team == _team && _controlledSquad.Mission == null && _controlledIsWithinRange)
            {
                Sector sector = null;

                switch(hit.collider.tag)
                {
                    case SQUAD_TAG:
                        sector = hit.collider.GetComponent<Squad>().Sector;
                        GUIManager.Instance.PlaySound("Command");
                        break;
                    case SECTOR_TAG:
                        sector = hit.collider.GetComponent<Sector>();
                        GUIManager.Instance.PlaySound("Command");
                        break;
                }

                if(sector != null)
                    CreateTravelEvent(_controlledSquad, sector, hit.point, 25.0f);
            }
            
        }

        GUIManager.Instance.SetSquadControls(_controlledSquad);
    }

    /// <summary>
    /// Updates the behavior and UI for the selected command ship.
    /// </summary>
    private void UpdateCommandShip()
    {
        EventSystem eventSystem = EventSystem.current;
        if (Input.GetMouseButton(0) && !eventSystem.IsPointerOverGameObject())
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                if (!_move.isPlaying)
                    _move.Play();

                float speed = 25.0f;
                if(Input.GetKey(KeyCode.Space))
                    speed = 100f;

                var dir = hit.point - _commandShipSquad.transform.position;
                dir.Normalize();
                _commandShipSquad.transform.position += dir * speed * Time.deltaTime;

                _commandShipSquad.transform.position = new Vector3(_commandShipSquad.transform.position.x, 0.0f, _commandShipSquad.transform.position.z);
                _commandShipSquad.transform.LookAt(hit.point);
            }
            else
                _move.Stop();            
        }
        else
            _move.Stop();

        GUIManager.Instance.SetSquadControls(_controlledSquad);
    }

    /// <summary>
    /// Populates a UI panel with this player's resource counts.
    /// </summary>
    /// <param name="panel"></param>
    public void DisplayResources(GameObject panel)
    {
        panel.transform.FindChild("OilText").GetComponent<Text>().text = _resourceRegistry[Resource.Oil].ToString();
        panel.transform.FindChild("OreText").GetComponent<Text>().text = _resourceRegistry[Resource.Ore].ToString();
        panel.transform.FindChild("AsterminiumText").GetComponent<Text>().text = _resourceRegistry[Resource.Asterminium].ToString();
        panel.transform.FindChild("ForestText").GetComponent<Text>().text = _resourceRegistry[Resource.Forest].ToString();
        panel.transform.FindChild("ResearchText").GetComponent<Text>().text = _resourceRegistry[Resource.Stations].ToString();
    }

    /// <summary>
    /// Populates a UI panel with new info about a research branch.
    /// </summary>
    /// <param name="type">The research type (military, scientific)</param>
    /// <param name="name">The specific research to update.</param>
    /// <param name="panel">The UI panel to update.</param>
    public void DisplayResearch(string type, string name, GameObject panel)
    {
        if(type == "Military")
        {
            _militaryTree.GetResearch(name).Display(panel, _resourceRegistry, _rcostReduction);
        }
        else if(type == "Scientific")
        {
            _scienceTree.GetResearch(name).Display(panel, _resourceRegistry, _rcostReduction);
        }
    }

    /// <summary>
    /// Populates a popup with info about a particular research.
    /// </summary>
    /// <param name="type">The research type (military, scientific)</param>
    /// <param name="name">The specific research to get info on.</param>
    /// <param name="property">The research's details to get info on.</param>
    /// <param name="panel">The UI panel to update.</param>
    public void PopulateResearchPanel(string type, string name, string property, GameObject panel)
    {
        if (type == "Military")
        {
            _militaryTree.GetResearch(name).DisplayPopup(panel, property, _rcostReduction);
        }
        else if (type == "Scientific")
        {
            _scienceTree.GetResearch(name).DisplayPopup(panel, property, _rcostReduction);
        }
    }

    /// <summary>
    /// Ends the player's turn. Overrides to reload the UI.
    /// </summary>
    public override void TurnEnd()
    {
        base.TurnEnd();
        ReloadGameplayUI();
    }

    /// <summary>
    /// Calls appropriate GUIManager functionality to reload the current UI.
    /// </summary>
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

    /// <summary>
    /// Controls a selected gameobject. Redraws the minimap if necessary.
    /// </summary>
    /// <param name="gameObject">The object to control.</param>
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

            if (_commandShipSquad != null)
            {
                if (colors.ContainsKey(_commandShipSquad.Sector))
                    colors[_commandShipSquad.Sector] = Color.magenta;
                else
                    colors.Add(_commandShipSquad.Sector, Color.magenta);
            }

            var minimap = MapManager.Instance.GenerateMap(_controlledSquad.Sector, colors);
            GUIManager.Instance.SetZoom(null, false);
            GUIManager.Instance.UpdateMinimap(minimap, _controlledSquad.transform.position, _controlledSquad.Sector);
        }

        ReloadGameplayUI();
    }

    /// <summary>
    /// Establish human-specific battle conditions.
    /// </summary>
    /// <param name="squad1">The first battle squad.</param>
    /// <param name="squad2">The second battle squad.</param>
    /// <param name="battleType">The type of battle.</param>
    /// <returns>The player's chance of winning.</returns>
    public override float PrepareBattleConditions(Squad squad1, Squad squad2, BattleType battleType)
    {
        _winChance = base.PrepareBattleConditions(squad1, squad2, battleType);
        Control(_playerSquad.gameObject);
        GUIManager.Instance.ConfigureBattleScreen(_winChance, _playerSquad, _enemySquad, battleType);
        return _winChance;
    }

    /// <summary>
    /// Extends the battle functionality in Player to update certain UI elements.
    /// </summary>
    /// <param name="playerChance">The odds of this player's success.</param>
    /// <param name="battleType">The type of battle.</param>
    /// <param name="player">This player's squad.</param>
    /// <param name="enemy">The combating squad.</param>
    /// <returns>The winning team and battle type, as well as the list of units lost.</returns>
    public override KeyValuePair<KeyValuePair<Team, BattleType>, Dictionary<string, int>> Battle(float playerChance, BattleType battleType, Squad player, Squad enemy)
    {
        var w = base.Battle(_winChance, _currentBattleType, _playerSquad, _enemySquad);

        enemy = _enemySquad;
        player = _playerSquad;

        if(w.Key.Key == _team)
        {
            if(w.Key.Value == BattleType.Invasion)
            {
                GUIManager.Instance.AddEvent("Invasion victory at " + enemy.Tile.Name + "!");
            }
            else
            {
                if(_enemySquad.Tile != null)
                    GUIManager.Instance.AddEvent("Orbital victory at " + enemy.Tile.Name + "!");
                else
                    GUIManager.Instance.AddEvent("Space victory against " + enemy.Team.ToString() + "!");
            }
        }
        else
        {
            if (w.Key.Value == BattleType.Invasion)
            {
                GUIManager.Instance.AddEvent("Invasion defeated at " + enemy.Tile.Name + "!");
            }
            else
            {
                if (_enemySquad.Tile != null)
                    GUIManager.Instance.AddEvent("Orbital defeat at " + enemy.Tile.Name + "!");
                else
                    GUIManager.Instance.AddEvent("Space defeat against " + enemy.Team.ToString() + "!");
            }
        }

        return w;
    }
}

