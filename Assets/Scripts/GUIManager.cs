﻿using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GUIManager : MonoBehaviour 
{
    private const string INI_PATH = "/Resources/Ships.ini";
    private const string SHIP_SECTION_HEADER = "[Ships]";
    private const string SQUAD_COUNT_PREFAB = "ShipCountListing";

    private static GUIManager _instance;
    private Dictionary<string, CustomUI> _interface;
    private Dictionary<string, Sprite> _icons;
    private Dictionary<string, int> _indices;
    private Dictionary<string, string> _descriptions; // duplicating that much text for each ship is terrible for memory.
    private float _popUpTimer;

    // when done with GUIManager, add a ton more of these
    private const string UI_ICONS_PATH = "UI Icons/";

    public Dictionary<string, Sprite> Icons { get { return _icons; } }
    public static GUIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<GUIManager>();
            }
            return _instance;
        }
    }

    public void Init(Dictionary<string, string> descriptions)
    {
        _descriptions = descriptions;
    }

    void Awake()
    {
        _instance = this;

        if (_interface == null)
            _interface = new Dictionary<string, CustomUI>();

        _indices = new Dictionary<string, int>()
        {
            { "MainShipList", -1 },
            { "Constructables", -1 },
            { "AltSquadList", -1 },
            { "AltShipList", -1 },
            { "SelectedShipList", -1 },
            { "SquadList", -1 },
            { "TileList", -1 }
        };

        _icons = new Dictionary<string, Sprite>()
        {
            { "Oil", Resources.Load<Sprite>(UI_ICONS_PATH + "OilIcon") },
            { "Asterminium", Resources.Load<Sprite>(UI_ICONS_PATH + "AsterminiumIcon") },
            { "Ore", Resources.Load<Sprite>(UI_ICONS_PATH + "OreIcon") },
            { "Forest", Resources.Load<Sprite>(UI_ICONS_PATH + "ForestIcon") },
            { "NoResource", Resources.Load<Sprite>(UI_ICONS_PATH + "NoResourceIcon") },
            { "Indigenous", Resources.Load<Sprite>(UI_ICONS_PATH + "IndigenousIcon") },
            { "Uninhabited", Resources.Load<Sprite>(UI_ICONS_PATH + "UninhabitedIcon") },
            { "Plinthen", Resources.Load<Sprite>(UI_ICONS_PATH + "PlinthenIcon") },
            { "Union", Resources.Load<Sprite>(UI_ICONS_PATH + "UnionIcon") },
            { "Kharkyr", Resources.Load<Sprite>(UI_ICONS_PATH + "KharkyrIcon") },
            { "Primitive", Resources.Load<Sprite>(UI_ICONS_PATH + "PrimitivePopulationIcon") },
            { "Industrial", Resources.Load<Sprite>(UI_ICONS_PATH + "IndustrialPopulationIcon") },
            { "Space Age", Resources.Load<Sprite>(UI_ICONS_PATH + "SpaceAgePopulationIcon") }
        };
    }

    void Start()
    {
    }

    void Update()
    {
    }

    public void Register(string name, CustomUI btn, bool disable)
    {
        if (_interface == null)
            _interface = new Dictionary<string, CustomUI>();

        if (!_interface.ContainsKey(name) && btn != null && btn.gameObject != null)
        {
            _interface.Add(name, btn);
            _interface[name].gameObject.SetActive(!disable);
        }
    }

    public void SetScreen(string screen)
    {
        var player = HumanPlayer.Instance;
        switch (screen)
        {
            case "MainUI":
                _interface["MainUI"].gameObject.SetActive(true);
                _interface["ScientificResearch"].gameObject.SetActive(false);
                _interface["MilitaryResearch"].gameObject.SetActive(false);
                break;
            case "Scientific":
                _interface["MainUI"].gameObject.SetActive(false);
                _interface["ScientificResearch"].gameObject.SetActive(true);
                _interface["MilitaryResearch"].gameObject.SetActive(false);

                player.DisplayResearch("Scientific", "Command Ship", _interface["Command Ship"].gameObject);
                player.DisplayResearch("Scientific", "Efficiency", _interface["Efficiency"].gameObject);
                player.DisplayResearch("Scientific", "Complex", _interface["Complex"].gameObject);
                player.DisplayResearch("Scientific", "Relay", _interface["Relay"].gameObject);
                player.DisplayResearch("Scientific", "Warp Portal", _interface["Warp Portal"].gameObject);
                break;
            case "Military":
                _interface["MainUI"].gameObject.SetActive(false);
                _interface["ScientificResearch"].gameObject.SetActive(false);
                _interface["MilitaryResearch"].gameObject.SetActive(true);

                player.DisplayResearch("Military", "Fighter", _interface["Fighter"].gameObject);
                player.DisplayResearch("Military", "Transport", _interface["Transport"].gameObject);
                player.DisplayResearch("Military", "Guard Satellite", _interface["Guard Satellite"].gameObject);
                player.DisplayResearch("Military", "Heavy Fighter", _interface["Heavy Fighter"].gameObject);
                player.DisplayResearch("Military", "Behemoth", _interface["Behemoth"].gameObject);
                break;
        }
    }

    public void SetSquadControls(Squad squad)
    {
        if (_interface["SquadMenu"].gameObject.activeInHierarchy == false)
            return;

        var manage = _interface["Manage"].gameObject.GetComponent<Button>();
        var deploy = _interface["SquadAction"].gameObject.GetComponent<Button>();
        var type = deploy.GetComponent<CustomUI>().data;
        var tile = squad.Tile;

        AutoSelectIndex<Ship>("MainShipList", squad.Ships);
        var index = _indices["MainShipList"];

        // note: i fucking hate this bit
        var click = false;
        if (HumanPlayer.Instance.Team == squad.Team && HumanPlayer.Instance.ControlledIsWithinRange)
        {
            if (type == "Deploy" && index != -1 && squad.OnMission == false) // deploy
            {
                if (tile != null && (squad.Ships[index].ShipProperties & ShipProperties.GroundStructure) > 0
                    && squad.Tile.IsInRange(squad) && (tile.Team == squad.Team || tile.Team == Team.Uninhabited) && tile.Structure == null) // existing planet
                    click = true;
                else if (tile == null && (squad.Ships[index].ShipProperties & ShipProperties.SpaceStructure) > 0 &&
                    squad.Sector.IsValidLocation(squad.transform.position))// empty space
                    click = true;
            }
            else if (type == "Undeploy" && tile != null && squad == tile.Squad && tile.Structure != null)// undeploy
                click = true;
            else if (type == "Invade" && squad.CalculateTroopPower() > 0f)
                click = true;
            else if (type == "Warp") ;

            deploy.interactable = click;
            manage.interactable = squad.Ships.Count > 0 || squad.Colliders.Count > 0;
        }
        else
        {
            deploy.interactable = false;
            manage.interactable = false;
        }
    }

    public void SetSquadList(bool set)
    {
        _interface["SquadListControl"].gameObject.SetActive(set);

        if (set)
            ReloadSquadList();
    }

    public void SetTileList(bool set)
    {
        _interface["TileListControl"].gameObject.SetActive(set);

        if (set)
            ReloadTileList();
    }

    private void ReloadSquadList()
    {
        PopulateList<Squad>(HumanPlayer.Instance.Squads, "SquadList", ListingType.Info, null);
    }

    private void ReloadTileList()
    {
        PopulateList<Tile>(HumanPlayer.Instance.Tiles, "TileList", ListingType.Info, null);
    }

    public void SetUIElements(bool squad, bool battle, bool tile, bool manage, bool lists)
    {
        _interface["SquadMenu"].gameObject.SetActive(squad);
        _interface["BattleMenu"].gameObject.SetActive(battle);
        _interface["PlanetInfo"].gameObject.SetActive(tile);
        _interface["ManageMenu"].gameObject.SetActive(manage);
        _interface["MenuControl"].gameObject.SetActive(lists);
    }

    public void ItemClicked(string data)
    {
        var split = data.Split('|');
        switch(split[0])
        {
            case "MainShipList":
                _indices[split[0]] = int.Parse(split[1]);
                break;
            case "AltShipList":
                _indices[split[0]] = int.Parse(split[1]);
                UpdateTransferInterface(false, true, false, true);
                break;
            case "SelectedShipList":
                _indices[split[0]] = int.Parse(split[1]);
                UpdateTransferInterface(false, false, true, true);
                break;
            case "AltSquadList":
                _indices[split[0]] = int.Parse(split[1]);
                UpdateTransferInterface(false, true, false, true);
                break;
            case "Constructables":
                HumanPlayer.Instance.CreateBuildEvent(split[1]);
                break;
            case "SquadList":
                _indices[split[0]] = int.Parse(split[1]);
                HumanPlayer.Instance.Control(HumanPlayer.Instance.Squads[_indices[split[0]]].gameObject);
                break;
            case "TileList":
                _indices[split[0]] = int.Parse(split[1]);
                HumanPlayer.Instance.Control(HumanPlayer.Instance.Tiles[_indices[split[0]]].gameObject);
                break;
        }
    }
    
    public void UIHighlighted(string data)
    {
        _popUpTimer = 1.0f;
    }

    public void UIHighlightStay(string data)
    {
        var split = data.Split('|');
        if (_popUpTimer >= 0.0f)
        {
            _popUpTimer -= Time.deltaTime;

            if (_popUpTimer >= 0.0f)
                return;

            switch (split[0])
            {
                case "MainShipList":
                case "SelectedShipList":
                    var i = int.Parse(split[1]);
                    var ship1 = HumanPlayer.Instance.Squad.Ships[i];
                    (ship1 as ListableObject).PopulateGeneralInfo(
                        _interface["ShipInfo"].gameObject, _descriptions[ship1.Name]);
                    _interface["ShipInfo"].gameObject.SetActive(true);
                    break;
                case "AltShipList":
                    var j = int.Parse(split[1]);
                    var ship2 = HumanPlayer.Instance.Squad.Colliders[_indices["AltSquadList"]].Ships[j];
                    (ship2 as ListableObject).PopulateGeneralInfo(
                        _interface["ShipInfo"].gameObject, _descriptions[ship2.Name]);
                    _interface["ShipInfo"].gameObject.SetActive(true);
                    break;
                case "Constructables":
                    (HumanPlayer.Instance.GetShipDefinition(split[1]) as ListableObject).PopulateBuildInfo(
                        _interface["ConstructionInfo"].gameObject, _descriptions[split[1]]);
                    _interface["ConstructionInfo"].gameObject.SetActive(true);
                    break;
                case "AltSquadList":
                    var k = int.Parse(split[1]);
                    ClearList("SquadInfoList");
                    (HumanPlayer.Instance.Squad.Colliders[k] as ListableObject).PopulateGeneralInfo(
                        _interface["SquadInfo"].gameObject, null);
                    _interface["SquadInfo"].gameObject.SetActive(true);
                    break;
                case "SquadList":
                    var l = int.Parse(split[1]);
                    ClearList("SquadInfoList");
                    (HumanPlayer.Instance.Squads[l] as ListableObject).PopulateGeneralInfo(
                        _interface["SquadInfo"].gameObject, null);
                    _interface["SquadInfo"].gameObject.SetActive(true);
                    break;
                case "TileList":
                    break;
            }
        }

        switch (split[0])
        {
            case "MainShipList":
            case "SelectedShipList":
            case "AltShipList":
                _interface["ShipInfo"].transform.position = Input.mousePosition;
                break;
            case "Constructables":
                _interface["ConstructionInfo"].transform.position = Input.mousePosition;
                break;
            case "AltSquadList":
            case "SquadList":
                _interface["SquadInfo"].transform.position = Input.mousePosition;
                break;
            case "TileList":
                break;
        }
    }

    public void UIDehighlighted(string data)
    {
        var split = data.Split('|');
        switch (split[0])
        {
            case "MainShipList":
            case "SelectedShipList":
            case "AltShipList":
                _interface["ShipInfo"].gameObject.SetActive(false);
                break;
            case "Constructables":
                _interface["ConstructionInfo"].gameObject.SetActive(false);
                break;
            case "AltSquadList":
            case "SquadList":
                _interface["SquadInfo"].gameObject.SetActive(false);
                break;
            case "TileList":
                break;
        }

        _popUpTimer = 1.0f;
    }

    public void PopulateManageLists()
    {
        SetUIElements(false, false, false, true, false);
        _indices["MainShipList"] = -1;
        ClearList("MainShipList");

        var squad = HumanPlayer.Instance.Squad;
        // temporarily add itself to colliders
        squad.Colliders.Add(HumanPlayer.Instance.Squad);

        // add deployed structures to the nearby tile if necessary
        if (squad.Tile != null && squad.Tile.Structure != null && squad.Tile.IsInRange(squad))
            squad.Tile.Squad.Ships.Add(squad.Tile.Structure);

        UpdateTransferInterface(true, true, true, true);
    }

    private void ClearList(string indexName)
    {
        foreach (Transform child in _interface[indexName].transform)
            GameObject.Destroy(child.gameObject);
    }

    private void PopulateList<T>(List<T> source, string indexName, ListingType type, System.Object data) where T : ListableObject
    {
        ClearList(indexName);

        for(var i = 0; i < source.Count; i++)
        {
            GameObject entry = null;
            switch (type)
            {
                case ListingType.Info:
                    entry = source[i].CreateListEntry(indexName, i, data);
                    break;
                case ListingType.Build:
                    entry = source[i].CreateBuildListEntry(indexName, i, data);
                    break;
            }

            if(entry != null)
                entry.transform.SetParent(_interface[indexName].transform);
        }
    }

    private void AutoSelectIndex<T>(string name, List<T> list)
    {
        if (list.Count == 0)
            _indices[name] = -1;
        else if (_indices[name] >= list.Count)
            _indices[name] = list.Count - 1;
        else if (_indices[name] < 0)
            _indices[name] = 0;
    }

    public void SquadSelected(Squad squad)
    {
        var keys = new List<string>(_indices.Keys);
        foreach (var index in keys)
            _indices[index] = -1;

        PopulateList<Ship>(squad.Ships, "MainShipList", ListingType.Info, false);
        var text = _interface["SquadActionText"].GetComponent<Text>();
        var data = _interface["SquadAction"].GetComponent<CustomUI>();

        text.text = data.data = "Deploy";

        if (squad.Tile != null && squad.Tile.IsInRange(squad))
        {
            if(squad.Tile.Team != squad.Team && squad.Tile.Team != Team.Uninhabited)
                text.text = data.data = "Invade";
            else if(squad.Tile.Team == squad.Team && squad.Tile.Structure != null && squad.Tile.Structure.Name == "Warp Portal")
                text.text = data.data = "Warp";
        }

        _interface["SquadTeamIcon"].GetComponent<Image>().sprite = _icons[squad.Team.ToString()];
        _interface["SquadTeamName"].GetComponent<Text>().text = squad.name;
        SetUIElements(true, false, false, false, true);
        SetSquadControls(squad);

        ReloadSquadList();
        ReloadTileList();
    }

    public void TileSelected(Tile tile, int playerStations, Dictionary<string, Ship> defs)
    {
        var squad = tile.gameObject.GetComponent<Squad>();
        SquadSelected(squad);

        tile.PopulateInfoPanel(_interface["PlanetInfo"].gameObject);

        // move to structure
        var playerStructure = tile.Team == HumanPlayer.Instance.Team && tile.Structure != null;
        var deployText = playerStructure ? "Undeploy" : "Deploy";

        _interface["ConstBar"].gameObject.SetActive(playerStructure);
        _interface["Structure"].gameObject.SetActive(playerStructure);
        _interface["ConstructionList"].gameObject.SetActive(playerStructure);
        _interface["SquadActionText"].GetComponent<Text>().text = deployText;
        _interface["SquadAction"].GetComponent<CustomUI>().data = deployText;

        if (playerStructure)
        {
            // populate structure info
            tile.Structure.Resources.Remove(Resource.Stations);
            tile.Structure.Resources.Add(Resource.Stations, playerStations);
            tile.Structure.PopulateStructurePanel(_interface["Structure"].gameObject);

            var buildList = new List<Ship>();
            foreach(var construct in tile.Structure.Constructables)
                buildList.Add(defs[construct]);
            PopulateList<Ship>(buildList, "Constructables", ListingType.Build, tile.Structure.Resources);
        }

        SetUIElements(true, false, true, false, true);
        SetSquadControls(squad);
    }

    public void SquadAction(string data)
    {
        switch(data)
        {
            case "Deploy":
                HumanPlayer.Instance.CreateDeployEvent(_indices["MainShipList"]);
                break;
            case "Undeploy":
                HumanPlayer.Instance.CreateUndeployEvent(false);
                break;
            case "Invade":
                HumanPlayer.Instance.CreateBattleEvent(HumanPlayer.Instance.Squad, HumanPlayer.Instance.Squad.Tile);
                break;
            case "Warp":

                break;
        }
    }

    private void UpdateTransferInterface(bool squads, bool squadShips, bool ships, bool other)
    {
        UpdateTransferShipInterface(squads, squadShips, ships);
        if (other && _interface["SoldierManager"].gameObject.activeInHierarchy)
            UpdateTransferSoldierInterface();
        else if (other && _interface["ResourceManager"].gameObject.activeInHierarchy)
            UpdateTransferResourceInterface();
    }

    private void UpdateTransferShipInterface(bool squads, bool squadShips, bool ships)
    {
        var squad = HumanPlayer.Instance.Squad;
        AutoSelectIndex<Squad>("AltSquadList", squad.Colliders);
        if (_indices["AltSquadList"] != -1)
            AutoSelectIndex<Ship>("AltShipList", squad.Colliders[_indices["AltSquadList"]].Ships);
        AutoSelectIndex<Ship>("SelectedShipList", squad.Ships);

        if (squads)
            PopulateList<Squad>(squad.Colliders, "AltSquadList", ListingType.Info, true);
        if (squadShips && _indices["AltSquadList"] > -1 && _indices["AltSquadList"] < squad.Colliders.Count)
            PopulateList<Ship>(squad.Colliders[_indices["AltSquadList"]].Ships, "AltShipList", ListingType.Info, true);
        if (ships)
            PopulateList<Ship>(squad.Ships, "SelectedShipList", ListingType.Info, true);

        var right = _interface["Right"].GetComponent<Button>();
        var dright = _interface["DoubleRight"].GetComponent<Button>();
        var left = _interface["Left"].GetComponent<Button>();
        var dleft = _interface["DoubleLeft"].GetComponent<Button>();

        var altsquad = _indices["AltSquadList"];
        var altships = _indices["AltShipList"];
        var selected = _indices["SelectedShipList"];

        if (altsquad == -1 || squad == squad.Colliders[altsquad])
        {
            right.interactable = left.interactable = dright.interactable = dleft.interactable = false;
        }
        else
        {
            if (altships != -1)
            {
                var ship = HumanPlayer.Instance.Squad.Colliders[altsquad].Ships[altships];
                right.interactable = (ship.ShipProperties & ShipProperties.Untransferable) == 0 ? true : false;
                dright.interactable = true;
            }

            if (selected != -1)
            {
                var ship = HumanPlayer.Instance.Squad.Ships[selected];
                left.interactable = (ship.ShipProperties & ShipProperties.Untransferable) == 0 ? true : false;
                dleft.interactable = true;
            }
        }
    }

    public void SwapManager(string data)
    {
        if(data == "Resources")
        {
            _interface["ResourceManager"].gameObject.SetActive(true);
            _interface["SoldierManager"].gameObject.SetActive(false);
        }
        else if(data == "Soldiers")
        {
            _interface["ResourceManager"].gameObject.SetActive(false);
            _interface["SoldierManager"].gameObject.SetActive(true);
        }

        UpdateTransferInterface(false, false, false, true);
    }

    private void UpdateTransferResourceInterface()
    {
        var oright = _interface["OreRight"].GetComponent<Button>();
        var lright = _interface["OilRight"].GetComponent<Button>();
        var fright = _interface["ForestRight"].GetComponent<Button>();
        var aright = _interface["AsterminiumRight"].GetComponent<Button>();
        var oleft = _interface["OreLeft"].GetComponent<Button>();
        var lleft = _interface["OilLeft"].GetComponent<Button>();
        var fleft = _interface["ForestLeft"].GetComponent<Button>();
        var aleft = _interface["AsterminiumLeft"].GetComponent<Button>();

        var odright = _interface["OreRightAll"].GetComponent<Button>();
        var ldright = _interface["OilRightAll"].GetComponent<Button>();
        var fdright = _interface["ForestRightAll"].GetComponent<Button>();
        var adright = _interface["AsterminiumRightAll"].GetComponent<Button>();
        var odleft = _interface["OreLeftAll"].GetComponent<Button>();
        var ldleft = _interface["OilLeftAll"].GetComponent<Button>();
        var fdleft = _interface["ForestLeftAll"].GetComponent<Button>();
        var adleft = _interface["AsterminiumLeftAll"].GetComponent<Button>();

        var olCount = _interface["OreLeftCount"].GetComponent<Text>();
        var llCount = _interface["OilLeftCount"].GetComponent<Text>();
        var flCount = _interface["ForestLeftCount"].GetComponent<Text>();
        var alCount = _interface["AsterminiumLeftCount"].GetComponent<Text>();
        var orCount = _interface["OreRightCount"].GetComponent<Text>();
        var lrCount = _interface["OilRightCount"].GetComponent<Text>();
        var frCount = _interface["ForestRightCount"].GetComponent<Text>();
        var arCount = _interface["AsterminiumRightCount"].GetComponent<Text>();

        var altsquad = _indices["AltSquadList"];
        var altships = _indices["AltShipList"];
        var selected = _indices["SelectedShipList"];

        if (selected != -1 && altships != -1) // manage soldiers!
        {
            var aship = HumanPlayer.Instance.Squad.Colliders[altsquad].Ships[altships];
            var sship = HumanPlayer.Instance.Squad.Ships[selected];
            var aShipTotal = aship.Resources[Resource.Ore] + aship.Resources[Resource.Oil] + aship.Resources[Resource.Forest] + aship.Resources[Resource.Asterminium];
            var sShipTotal = sship.Resources[Resource.Ore] + sship.Resources[Resource.Oil] + sship.Resources[Resource.Forest] + sship.Resources[Resource.Asterminium];
            var aShipFull = aShipTotal == aship.ResourceCapacity;
            var sShipFull = sShipTotal == sship.ResourceCapacity;
            var same = aship == sship;

            olCount.text = aship.Resources[Resource.Ore].ToString();
            llCount.text = aship.Resources[Resource.Oil].ToString();
            flCount.text = aship.Resources[Resource.Forest].ToString();
            alCount.text = aship.Resources[Resource.Asterminium].ToString();

            orCount.text = sship.Resources[Resource.Ore].ToString();
            lrCount.text = sship.Resources[Resource.Oil].ToString();
            frCount.text = sship.Resources[Resource.Forest].ToString();
            arCount.text = sship.Resources[Resource.Asterminium].ToString(); ;

            oright.interactable = odright.interactable = aship.Resources[Resource.Ore] > 0 && !sShipFull && !same;
            lright.interactable = ldright.interactable = aship.Resources[Resource.Oil] > 0 && !sShipFull && !same;
            fright.interactable = fdright.interactable = aship.Resources[Resource.Forest] > 0 && !sShipFull && !same;
            aright.interactable = adright.interactable = aship.Resources[Resource.Asterminium] > 0 && !sShipFull && !same;

            oleft.interactable = odleft.interactable = sship.Resources[Resource.Ore] > 0 && !aShipFull && !same;
            lleft.interactable = ldleft.interactable = sship.Resources[Resource.Oil] > 0 && !aShipFull && !same;
            fleft.interactable = fdleft.interactable = sship.Resources[Resource.Forest] > 0 && !aShipFull && !same;
            aleft.interactable = adleft.interactable = sship.Resources[Resource.Asterminium] > 0 && !aShipFull && !same;
        }
        else
        {
            odright.interactable = ldright.interactable = fdright.interactable = adright.interactable = false;
            odleft.interactable = ldleft.interactable = fdleft.interactable = adleft.interactable = false;
            oright.interactable = lright.interactable = fright.interactable = aright.interactable = false;
            oleft.interactable = lleft.interactable = fleft.interactable = aleft.interactable = false;
            olCount.text = llCount.text = flCount.text = alCount.text = Convert.ToString(0);
            orCount.text = lrCount.text = frCount.text = arCount.text = Convert.ToString(0); 
        }
    }

    private void UpdateTransferSoldierInterface()
    {
        var pright = _interface["PrimRight"].GetComponent<Button>();
        var iright = _interface["IndRight"].GetComponent<Button>();
        var sright = _interface["SpaceRight"].GetComponent<Button>();
        var pleft = _interface["PrimLeft"].GetComponent<Button>();
        var ileft = _interface["IndLeft"].GetComponent<Button>();
        var sleft = _interface["SpaceLeft"].GetComponent<Button>();

        var pdright = _interface["PrimRightAll"].GetComponent<Button>();
        var idright = _interface["IndRightAll"].GetComponent<Button>();
        var sdright = _interface["SpaceRightAll"].GetComponent<Button>();
        var pdleft = _interface["PrimLeftAll"].GetComponent<Button>();
        var idleft = _interface["IndLeftAll"].GetComponent<Button>();
        var sdleft = _interface["SpaceLeftAll"].GetComponent<Button>();

        var plCount = _interface["PrimLeftCount"].GetComponent<Text>();
        var ilCount = _interface["IndLeftCount"].GetComponent<Text>();
        var slCount = _interface["SpaceLeftCount"].GetComponent<Text>();
        var prCount = _interface["PrimRightCount"].GetComponent<Text>();
        var irCount = _interface["IndRightCount"].GetComponent<Text>();
        var srCount = _interface["SpaceRightCount"].GetComponent<Text>();

        var altsquad = _indices["AltSquadList"];
        var altships = _indices["AltShipList"];
        var selected = _indices["SelectedShipList"];

        if (selected != -1 && altships != -1) // manage soldiers!
        {
            var aship = HumanPlayer.Instance.Squad.Colliders[altsquad].Ships[altships];
            var sship = HumanPlayer.Instance.Squad.Ships[selected];
            var aShipTotal = aship.PrimitivePopulation + aship.IndustrialPopulation + aship.SpaceAgePopulation;
            var sShipTotal = sship.PrimitivePopulation + sship.IndustrialPopulation + sship.SpaceAgePopulation;
            var aShipFull = aShipTotal == aship.Capacity;
            var sShipFull = sShipTotal == sship.Capacity;
            var same = aship == sship;

            plCount.text = aship.PrimitivePopulation.ToString();
            ilCount.text = aship.IndustrialPopulation.ToString();
            slCount.text = aship.SpaceAgePopulation.ToString();
            prCount.text = sship.PrimitivePopulation.ToString();
            irCount.text = sship.IndustrialPopulation.ToString();
            srCount.text = sship.SpaceAgePopulation.ToString();

            pright.interactable = pdright.interactable = aship.PrimitivePopulation > 0 && !sShipFull && !same;
            iright.interactable = idright.interactable = aship.IndustrialPopulation > 0 && !sShipFull && !same;
            sright.interactable = sdright.interactable = aship.SpaceAgePopulation > 0 && !sShipFull && !same;
            pleft.interactable = pdleft.interactable = sship.PrimitivePopulation > 0 && !aShipFull && !same;
            ileft.interactable = idleft.interactable = sship.IndustrialPopulation > 0 && !aShipFull && !same;
            sleft.interactable = sdleft.interactable = sship.SpaceAgePopulation > 0 && !aShipFull && !same;
        }
        else
        {
            pdright.interactable = idright.interactable = sdright.interactable = pdleft.interactable = idleft.interactable = sdleft.interactable = false;
            pright.interactable = iright.interactable = sright.interactable = pleft.interactable = ileft.interactable = sleft.interactable = false;
            plCount.text = ilCount.text = slCount.text = prCount.text = irCount.text = srCount.text = Convert.ToString(0);
        }
    }

    public Ship Transfer(Squad from, string fromName, Squad to, string toName)
    {
        var fromIndex = _indices[fromName];
        var ship = from.Ships[fromIndex];
        from.Ships.RemoveAt(fromIndex);

        if((ship.ShipProperties & ShipProperties.Untransferable) > 0)
            return ship;

        to.Ships.Add(ship);
        return null;
    }

    public void TransferAll(Squad from, string fromName, Squad to, string toName)
    {
        var untransferables = new List<Ship>();
        while(from.Ships.Count > 0)
        {
            AutoSelectIndex<Ship>(fromName, from.Ships);
            var ship = Transfer(from, fromName, to, toName);
            if (ship != null)
                untransferables.Add(ship);
        }

        from.Ships.AddRange(untransferables);
    }

    public void TransferToControlledSquad()
    {
        var index = _indices["AltSquadList"];
        var squad = HumanPlayer.Instance.Squad.Colliders[index];
        var ship = Transfer(squad, "AltShipList", HumanPlayer.Instance.Squad, "SelectedShipList");
        if (ship != null) squad.Ships.Insert(index, ship);
        UpdateTransferInterface(false, true, true, true);
    }

    public void TransferToSelectedSquad()
    {
        var index = _indices["SelectedShipList"];
        var squad = HumanPlayer.Instance.Squad;
        var ship = Transfer(squad, "SelectedShipList", HumanPlayer.Instance.Squad.Colliders[_indices["AltSquadList"]], "AltShipList");
        if (ship != null) squad.Ships.Insert(index, ship);
        UpdateTransferInterface(false, true, true, true);
    }

    public void TransferAllToControlledSquad()
    {
        TransferAll(HumanPlayer.Instance.Squad.Colliders[_indices["AltSquadList"]], "AltShipList", HumanPlayer.Instance.Squad, "SelectedShipList");
        UpdateTransferInterface(false, true, true, true);
    }

    public void TransferAllToSelectedSquad()
    {
        TransferAll(HumanPlayer.Instance.Squad, "SelectedShipList", HumanPlayer.Instance.Squad.Colliders[_indices["AltSquadList"]], "AltShipList");
        UpdateTransferInterface(false, true, true, true);
    }

    public void NewSquad()
    {
        var squad = HumanPlayer.Instance.CreateNewSquad(HumanPlayer.Instance.Squad);
        var listing = (ListableObject)squad;

        listing.CreateListEntry("AltSquadList", HumanPlayer.Instance.Squad.Colliders.Count - 1, true).transform.SetParent(_interface["AltSquadList"].transform);
        UpdateTransferInterface(true, true, false, true);
    }

    private void TransferSoldier(string type, Ship from, Ship to)
    {
        switch(type)
        {
            case "Primitive":
                from.PrimitivePopulation--;
                to.PrimitivePopulation++;
                break;
            case "Industrial":
                from.IndustrialPopulation--;
                to.IndustrialPopulation++;
                break;
            case "SpaceAge":
                from.SpaceAgePopulation--;
                to.SpaceAgePopulation++;
                break;
        }
    }

    private void TransferAllSoldiers(string type, Ship from, Ship to)
    {
        switch (type)
        {
            case "Primitive":
                while (from.PrimitivePopulation > 0 && to.PrimitivePopulation + to.IndustrialPopulation + to.SpaceAgePopulation < to.Capacity)
                    TransferSoldier(type, from, to);
                break;
            case "Industrial":
                while (from.IndustrialPopulation > 0 && to.PrimitivePopulation + to.IndustrialPopulation + to.SpaceAgePopulation < to.Capacity)
                    TransferSoldier(type, from, to);
                break;
            case "SpaceAge":
                while (from.SpaceAgePopulation > 0 && to.PrimitivePopulation + to.IndustrialPopulation + to.SpaceAgePopulation < to.Capacity)
                    TransferSoldier(type, from, to);
                break;
        }
    }

    public void SoldierTransfer(string data)
    {
        var aship = HumanPlayer.Instance.Squad.Colliders[_indices["AltSquadList"]].Ships[_indices["AltShipList"]];
        var sship = HumanPlayer.Instance.Squad.Ships[_indices["SelectedShipList"]];

        switch(data)
        {
            case "PrimRight":
                TransferSoldier("Primitive", aship, sship);
                break;
            case "IndRight":
                TransferSoldier("Industrial", aship, sship);
                break;
            case "SpaceRight":
                TransferSoldier("SpaceAge", aship, sship);
                break;
            case "PrimLeft":
                TransferSoldier("Primitive", sship, aship);
                break;
            case "IndLeft":
                TransferSoldier("Industrial", sship, aship);
                break;
            case "SpaceLeft":
                TransferSoldier("SpaceAge", sship, aship);
                break;
            case "PrimRightAll":
                TransferAllSoldiers("Primitive", aship, sship);
                break;
            case "IndRightAll":
                TransferAllSoldiers("Industrial", aship, sship);
                break;
            case "SpaceRightAll":
                TransferAllSoldiers("SpaceAge", aship, sship);
                break;
            case "PrimLeftAll":
                TransferAllSoldiers("Primitive", sship, aship);
                break;
            case "IndLeftAll":
                TransferAllSoldiers("Industrial", sship, aship);
                break;
            case "SpaceLeftAll":
                TransferAllSoldiers("SpaceAge", sship, aship);
                break;
        }

        UpdateTransferInterface(false, false, false, true);
    }

    public void ResourceTransfer(string data)
    {
        var newString = Regex.Replace(data, "([a-z])([A-Z])", "$1 $2");
        var s = newString.Split(' ').ToList();
        var resource = (Resource)Enum.Parse(typeof(Resource), s[0]);
        
        // assume going right
        Ship from = HumanPlayer.Instance.Squad.Colliders[_indices["AltSquadList"]].Ships[_indices["AltShipList"]];
        Ship to = HumanPlayer.Instance.Squad.Ships[_indices["SelectedShipList"]];
        if(s[1] == "Left")
        {
            to = HumanPlayer.Instance.Squad.Colliders[_indices["AltSquadList"]].Ships[_indices["AltShipList"]];
            from = HumanPlayer.Instance.Squad.Ships[_indices["SelectedShipList"]];
        }

        if(s[s.Count - 1] == "All")
        {
            while(from.Resources[resource] > 0 &&
                to.Resources[Resource.Ore] + to.Resources[Resource.Oil] + 
                to.Resources[Resource.Forest] + to.Resources[Resource.Asterminium] < to.ResourceCapacity)
            {
                from.Resources[resource]--;
                to.Resources[resource]++;
            }
        }
        else
        {
            from.Resources[resource]--;
            to.Resources[resource]++;
        }

        UpdateTransferInterface(false, false, false, true);
    }

    public void ExitManage()
    {
        var squad = HumanPlayer.Instance.Squad;
        squad.Colliders.Remove(squad); // remove self
        if (squad.Tile != null && squad.Tile.IsInRange(squad)) // remove any deployed structures
            squad.Tile.Squad.Ships.Remove(squad.Tile.Structure);

        ClearList("AltSquadList");
        ClearList("AltShipList");
        ClearList("SelectedShipList");
        HumanPlayer.Instance.CleanSquad(HumanPlayer.Instance.Squad);
        HumanPlayer.Instance.ReloadGameplayUI();
        AutoSelectIndex<Ship>("MainShipList", HumanPlayer.Instance.Squad.Ships);
    }

    //
    // These functions update and display the menu when a command is given
    //

    // Notes: Tiles have squads, but they may be empty.
    // Tiles and squads should check for enemy/friendly before choosing how to make the new menu
    public void SquadToSpace(Squad squad, Vector3 coords)
    {
        // ask to confirm/cancel movement
    }

    public void SquadToTile(Squad squad, Tile tile)
    {
        if (tile.Team == HumanPlayer.Instance.Team) // ask to confirm, cancel
        {

        }
        else // enemy - ask to confirm move order, cancel, etc
        {

        }
    }

    // note: "squad2" isn't necessarily hostile. Handle that stuff here.
    public void SquadToSquad(Squad playerSquad, Squad squad2)
    {
        if (squad2.Team == HumanPlayer.Instance.Team) // ask to confirm movement, etc
        {

        }
        else // enemy - ask to chase, ignore, cancel, etc
        {

        }
    }

    //
    // These functions update and enable UI when squads collide with the appropriate object
    //

    public void ConfigureBattleScreen(float WC, Squad squad1, Squad squad2)
    {
        var t1 = squad1.Team;

        Squad player = squad2;
        Squad enemy = squad1;

        if (t1 == HumanPlayer.Instance.Team)
        {
            player = squad1;
            enemy = squad2;
        }

        var pt = player.GetComponent<Tile>();
        var et = enemy.GetComponent<Tile>();

        _interface["BattleChance"].GetComponent<Text>().text = WC.ToString("P");
        _interface["PlayerBattleIcon"].GetComponent<Image>().sprite = _icons[player.Team.ToString()];
        _interface["EnemyBattleIcon"].GetComponent<Image>().sprite = _icons[enemy.Team.ToString()];

        ClearList("PlayerSquadInfoList");
        player.PopulateCountList(_interface["PlayerSquadInfoList"].gameObject);
        ClearList("EnemySquadInfoList");
        enemy.PopulateCountList(_interface["EnemySquadInfoList"].gameObject);

        // for detailed battle screen - current blows
        if(pt != null)
        {
            // player tile vs. enemy squad
        }
        else if(et != null)
        {
            // player squad vs. enemy tile
        }
        else // squad vs. squad
        {

        }

        SetUIElements(false, true, false, false, false);
    }

    public void Battle()
    {
        var winner = HumanPlayer.Instance.Battle(0f, BattleType.Invasion, null, null);

        if(winner.Key.Key == HumanPlayer.Instance.Team)
        {
            if(winner.Key.Value == BattleType.Invasion)
            {

            }
            else if(winner.Key.Value == BattleType.Space)
            {

            }

            _interface["BattleWon"].gameObject.SetActive(true);
            ClearList("ShipsLostList");
            var squadEntry = Resources.Load<GameObject>(SQUAD_COUNT_PREFAB);
            foreach(var lost in winner.Value)
            {
                var entry = Instantiate(squadEntry) as GameObject;
                entry.transform.FindChild("Name").GetComponent<Text>().text = lost.Key;
                switch(lost.Key)
                {
                    case "Primitive":
                        entry.transform.FindChild("Icon").GetComponent<Image>().sprite = _icons["Primitive"];
                        break;
                    case "Industrial":
                        entry.transform.FindChild("Icon").GetComponent<Image>().sprite = _icons["Industrial"];
                        break;
                    case "Space Age":
                        entry.transform.FindChild("Icon").GetComponent<Image>().sprite = _icons["Space Age"];
                        break;
                    default:
                        entry.transform.FindChild("Icon").GetComponent<Image>().sprite = HumanPlayer.Instance.GetShipDefinition(lost.Key).Icon;
                        break;
                }
                entry.transform.FindChild("Count").FindChild("Number").GetComponent<Text>().text = lost.Value.ToString();
                entry.transform.SetParent(_interface["ShipsLostList"].transform);
            }
        }
        else if(winner.Key.Key == Team.Uninhabited) // draw
        {

        }
        else
        {
            _interface["BattleLost"].gameObject.SetActive(true);

            if(winner.Key.Value == BattleType.Space)
            {
                _interface["ShipsText"].gameObject.SetActive(true);
                _interface["SoldiersText"].gameObject.SetActive(false);
            }
            else
            {
                _interface["ShipsText"].gameObject.SetActive(false);
                _interface["SoldiersText"].gameObject.SetActive(true);
            }
        }

        SetUIElements(false, false, false, false, false);
    }

    public void ContinueAfterBattle(bool win)
    {
        if(win)
            _interface["BattleWon"].gameObject.SetActive(false);
        else
            _interface["BattleLost"].gameObject.SetActive(false);
        HumanPlayer.Instance.EndBattleConditions(win);
    }
}
