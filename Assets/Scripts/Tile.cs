﻿using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Tile : MonoBehaviour, ListableObject
{
    private const string TILE_LISTING_PREFAB = "TileListing";
    private const string CIRCLE_PREFAB = "Circle";
    private const string SQUAD_COUNT_PREFAB = "ShipCountListing";

    private float _radius;
    private float _clickRadius;
    private string _name;
    private string _planetType;
    private int _population;
    private Inhabitance _planetInhabitance;
    private Resource _resourceType;
    private int _resourceCount;
    private Team _team;
    private Structure _structure;
    private Squad _squad;
    private Dictionary<Team, bool> _diplomacy;
    private GameObject _circle;

    public string Name { get { return _name; } }
    public Team Team { get { return _team; } }
    public Structure Structure { get { return _structure; } }
    public string Type { get { return _planetType; } }
    public Squad Squad { get { return _squad; } }
    public float Radius { get { return _radius; } }
    public int Population 
    { 
        get { return _population; }
        set { _population = value; }
    }
    public Inhabitance PopulationType {  get { return _planetInhabitance; } }
    public GameObject Ring { get { return _circle; } }

	// New Generation code - handled in Sector/GenerateTile, sent here
    public void Init(Sector sector, string type, string name, Inhabitance pType, int p, Resource rType, int rCount, TileSize size, Team team)
    {
        _squad = this.GetComponent<Squad>();
        _squad.Init(Team.Uninhabited, sector, name);
        _diplomacy = new Dictionary<global::Team, bool>();
        transform.SetParent(sector.transform);

        _planetType = type;
        _name = name;
        _planetInhabitance = pType;
        _resourceType = rType;
        _resourceCount = rCount;
        _team = team;
        _squad.Team = team;

        var mapManager = MapManager.Instance;
        var system = GetComponent<ParticleSystem>();
        var renderer = system.GetComponent<Renderer>();

        _radius = 5.0f;
        _clickRadius = 1.5f;
        system.startSize = 5.0f;
        if (size == TileSize.Small)
        {
            system.startSize *= 0.5f;
            _radius *= 0.5f;
            _clickRadius *= 0.5f;
        }

        renderer.material.mainTexture = mapManager.PlanetTextureTable[_planetType].Texture;
        renderer.material.mainTextureOffset = mapManager.PlanetTextureTable[_planetType].TextureOffset;
        renderer.material.mainTextureScale = mapManager.PlanetTextureTable[_planetType].TextureScale;

        system.enableEmission = true;
        renderer.enabled = true;

        if (_team != Team.Uninhabited)
        {
            var pl = GameManager.Instance.Players[_team];
            if(_planetInhabitance != Inhabitance.Uninhabited)
                pl.AddSoldiers(this, _planetInhabitance, p);
            
            // generate random defenses if space age or non-player team
            if((_team == Team.Indigenous && _planetInhabitance == Inhabitance.SpaceAge) || (_team != HumanPlayer.Instance.Team && _team != Team.Indigenous))
            {
                PopulateRandomSquad(_squad);

                var strs = pl.ShipDefinitions.Where(t => (t.Value.ShipProperties & ShipProperties.GroundStructure) != 0).ToList();
                var s = GameManager.Generator.Next(0, strs.Count);
                var sh = pl.AddShip(_squad, strs[s].Key);
                _squad.Deploy(sh as Structure, this);

                // populate structure
                // pl.AddSoldiers(_structure)
            }
        }

        GameManager.Instance.Players[_team].ClaimTile(this);

        var circle = Resources.Load(CIRCLE_PREFAB);
        _circle = GameObject.Instantiate(circle, this.transform.position, Quaternion.Euler(90f, 0, 0)) as GameObject;
        _circle.transform.localScale = new Vector3(_radius * 2 + 0.5f, _radius * 2 + 0.5f, _radius * 2 + 0.5f);
        _circle.transform.parent = this.transform.parent;
        _circle.GetComponent<Renderer>().material.SetColor("_Color", GameManager.Instance.PlayerColors[_team]);
    }

    private void PopulateRandomSquad(Squad squad)
    {
        var pl = GameManager.Instance.Players[_team];
        int n = GameManager.Generator.Next(5, 26);
        for (int i = 0; i < n; i++)
            pl.AddShip(_squad, "Fighter");

        n = GameManager.Generator.Next(0, 11);
        for (int i = 0; i < n; i++)
            pl.AddShip(_squad, "Transport");

        n = GameManager.Generator.Next(2, 5);
        for (int i = 0; i < n; i++)
            pl.AddShip(_squad, "Guard Satellite");

        n = GameManager.Generator.Next(0, 11);
        for (int i = 0; i < n; i++)
            pl.AddShip(_squad, "Heavy Fighter");

        /*
        n = GameManager.Generator.Next(0, 6);
        for (int i = 0; i < n; i++)
            pl.AddShip(_squad, "Behemoth");*/
    }

    void Start()
    {
    }

	// Update is called once per frame
	void Update () 
    {

        if (this.GetComponent<Renderer>().isVisible)
        {
            this.GetComponent<ParticleSystem>().enableEmission = true;
        }
        else
        {
            this.GetComponent<ParticleSystem>().enableEmission = false;
        }
	}

    public void Claim(Team team)
    {
        _circle.GetComponent<Renderer>().material.SetColor("_Color", GameManager.Instance.PlayerColors[team]);
        GameManager.Instance.Players[_team].RelinquishTile(this);

        if (_team == Team.Plinthen)
        {
            _resourceCount -= Mathf.CeilToInt(_resourceCount * 1.15f);
            _resourceCount = (_resourceCount < 0 ? 0 : _resourceCount);
        }

        _team = team;

        if (_team == Team.Plinthen)
            _resourceCount += Mathf.CeilToInt(_resourceCount * 1.15f);

        GameManager.Instance.Players[_team].ClaimTile(this);
        _squad.Team = _team;

        var teams = _diplomacy.Keys.ToList();
        foreach(var t in teams)
        {
            _diplomacy[t] = false;
        }

        HumanPlayer.Instance.Control(HumanPlayer.Instance.Squad.gameObject);
    }

    public string Undeploy(bool destroyStructure)
    {
        if (_structure == null)
            return "";

        _squad.Ships.Add(_structure);
        _structure.Undeploy(this);

        if (destroyStructure)
            GameManager.Instance.Players[_team].RemoveShip(_squad, _structure);

        if(MapManager.Instance.DeploySpawnTable.ContainsKey(_planetType))
        {
            _squad.Sector.UnregisterSpaceStructure(_team, _structure);
            _squad.Sector.DeleteTile(this);
        }

        _structure = null;

        return _planetType;
    }

    public void Deploy(Structure ship, ShipProperties structureType, Team team)
    {
        if(_team != team)
            Claim(team);
        _structure = ship;
        _structure.Deploy(this);
    }

    public void GatherAndGrow()
    {
        if (_structure == null)
        {
            _population += Mathf.CeilToInt(_population * 0.05f);
            return;
        }
        else if(_planetInhabitance != Inhabitance.Uninhabited)
        {
            _population += Mathf.CeilToInt((_population + _structure.Population[_planetInhabitance]) * 0.05f);
        }

        var gathered = _structure.Gather(_resourceType, _resourceCount, _planetInhabitance, _population, _team);

        foreach (var resource in gathered)
        {
            switch (resource.Key)
            {
                case ResourceGatherType.None:
                case ResourceGatherType.Soldiers:
                case ResourceGatherType.Research:
                    break;
                case ResourceGatherType.Natural:
                    _resourceCount -= resource.Value;
                    break;
            }
        }
    }

    public float CalculateDefensivePower()
    {
        var power = 0f;
        var bonuses = new Dictionary<Inhabitance, float>()
        {
            { Inhabitance.Primitive, 1f },
            { Inhabitance.Industrial, 1.5f },
            { Inhabitance.SpaceAge, 1.75f }
        };
        
        if(_planetInhabitance != Inhabitance.Uninhabited)
            power += _population * bonuses[_planetInhabitance];
        if (_structure != null)
        {
            foreach(var bonus in bonuses)
                power += _structure.Population[bonus.Key] * bonus.Value;
        }

        return power * (_team == Team.Kharkyr ? 1.15f : 1f);
    }

    public void SetDiplomaticEffort(Team team)
    {
        if (!_diplomacy.ContainsKey(team))
            _diplomacy.Add(team, true);
        else
            _diplomacy[team] = true;
    }

    public void EndDiplomaticEffort(Team team)
    {
        _diplomacy[team] = false;
    }

    public void PopulateInfoPanel(GameObject panel)
    {
        var tileRenderer = this.GetComponent<ParticleSystem>().GetComponent<Renderer>();
        var uiRenderer = panel.transform.FindChild("PlanetIcon").GetComponent<RawImage>();
        uiRenderer.texture = tileRenderer.material.mainTexture;
        uiRenderer.uvRect = new Rect(tileRenderer.material.mainTextureOffset.x,
                                     tileRenderer.material.mainTextureOffset.y,
                                     tileRenderer.material.mainTextureScale.x,
                                     tileRenderer.material.mainTextureScale.y);
        panel.transform.FindChild("PlanetName").GetComponent<Text>().text = _name;
        panel.transform.FindChild("TeamName").GetComponent<Text>().text = _team.ToString();
        panel.transform.FindChild("TeamIcon").GetComponent<Image>().sprite = GUIManager.Instance.Icons[_team.ToString()];

        if(_team == Team.Indigenous)
        {
            if(_diplomacy.ContainsKey(HumanPlayer.Instance.Team) && _diplomacy[HumanPlayer.Instance.Team])
            {
                panel.transform.FindChild("Diplomacy").gameObject.SetActive(false);
                panel.transform.FindChild("DiplomacyInProgress").gameObject.SetActive(true);
            }
            else
            {
                panel.transform.FindChild("Diplomacy").gameObject.SetActive(true);
                panel.transform.FindChild("DiplomacyInProgress").gameObject.SetActive(false);
            }
        }
        else
        {
            panel.transform.FindChild("Diplomacy").gameObject.SetActive(false);
            panel.transform.FindChild("DiplomacyInProgress").gameObject.SetActive(false);
        }

        if (_resourceType != Resource.NoResource)
        {
            var name = panel.transform.FindChild("ResourceName");
            var icon = panel.transform.FindChild("ResourceIcon");
            var amount = panel.transform.FindChild("ResourceCount");
            name.gameObject.SetActive(true);
            icon.gameObject.SetActive(true);
            name.GetComponent<Text>().text = _resourceType.ToString();
            amount.GetComponent<Text>().text = _resourceCount.ToString();
            icon.GetComponent<Image>().sprite = GUIManager.Instance.Icons[_resourceType.ToString()];
        }
        else
        {
            panel.transform.FindChild("ResourceName").gameObject.SetActive(false);
            panel.transform.FindChild("ResourceIcon").gameObject.SetActive(false);
        }

        var populations = new Dictionary<Inhabitance, int>() 
        {
            { Inhabitance.Primitive, 0 },
            { Inhabitance.Industrial, 0 },
            { Inhabitance.SpaceAge, 0 }
        };
        if(_structure != null)
            populations = new Dictionary<Inhabitance, int>(_structure.Population);
        if(_planetInhabitance != Inhabitance.Uninhabited)
            populations[_planetInhabitance] += _population;

        int total = 0;
        foreach(var pop in populations)
        {
            panel.transform.FindChild(pop.Key.ToString() + "Population").GetComponent<Text>().text = pop.Value.ToString();
            total += pop.Value;
        }

        panel.transform.FindChild("TotalPopulation").GetComponent<Text>().text = total.ToString();
    }

    public bool IsInRange(Squad squad)
    {
        return (squad.transform.position - transform.position).sqrMagnitude <= (_radius * _radius);
    }

    public bool IsInClickRange(Vector3 click)
    {
        return (click - transform.position).sqrMagnitude <= (_clickRadius * _clickRadius);
    }

    public void PopulateCountList(GameObject list, BattleType bType)
    {
        var squadEntry = Resources.Load<GameObject>(SQUAD_COUNT_PREFAB);

        var counts = new Dictionary<string, int>()
        {
            { _planetInhabitance.ToString(), _population }
        };

        if (_planetInhabitance == Inhabitance.Uninhabited)
            counts.Remove(_planetInhabitance.ToString());

        if(_structure != null)
            foreach (var i in _structure.Population)
            {
                if (!counts.ContainsKey(i.Key.ToString()) && i.Value > 0)
                    counts.Add(i.Key.ToString(), i.Value);
                else if (counts.ContainsKey(i.Key.ToString()))
                    counts[i.Key.ToString()] += i.Value;
            }

        foreach (var count in counts)
        {
            var entry = Instantiate(squadEntry) as GameObject;
            entry.transform.FindChild("Name").GetComponent<Text>().text = count.Key;
            entry.transform.FindChild("Icon").GetComponent<Image>().sprite = GUIManager.Instance.Icons[count.Key];
            entry.transform.FindChild("Count").FindChild("Number").GetComponent<Text>().text = count.Value.ToString();
            entry.transform.SetParent(list.transform);
        }
    }

    // for info lists later
    GameObject ListableObject.CreateListEntry(string listName, int index, System.Object data) 
    {
        var tileEntry = Resources.Load<GameObject>(TILE_LISTING_PREFAB);
        var entry = Instantiate(tileEntry) as GameObject;

        entry.transform.FindChild("Name").GetComponent<Text>().text = _name;
        var tileRenderer = this.GetComponent<ParticleSystem>().GetComponent<Renderer>();
        var uiRenderer = entry.transform.FindChild("Icon").GetComponent<RawImage>();
        uiRenderer.texture = tileRenderer.material.mainTexture;
        uiRenderer.uvRect = new Rect(tileRenderer.material.mainTextureOffset.x,
                                     tileRenderer.material.mainTextureOffset.y,
                                     tileRenderer.material.mainTextureScale.x,
                                     tileRenderer.material.mainTextureScale.y);
        entry.GetComponent<CustomUIAdvanced>().data = listName + "|" + index;

        return entry;
    }

    GameObject ListableObject.CreateBuildListEntry(string listName, int index, System.Object data) { return null; }
    void ListableObject.PopulateBuildInfo(GameObject popUp, System.Object data) { }
    void ListableObject.PopulateGeneralInfo(GameObject popUp, System.Object data) { }
}
