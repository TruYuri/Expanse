﻿using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Tile : MonoBehaviour, ListableObject
{
    private const string PLANET_SMALL_SPAWN_DETAIL = "SmallSizeSpawnRate";
    private const string PLANET_SMALL_RESOURCE_MIN_DETAIL = "ResourceAmountSmallMinimum";
    private const string PLANET_SMALL_RESOURCE_MAX_DETAIL = "ResourceAmountSmallMaximum";
    private const string PLANET_SMALL_POPULATION_MIN_DETAIL = "PopulationAmountSmallMinimum";
    private const string PLANET_SMALL_POPULATION_MAX_DETAIL = "PopulationAmountSmallMaximum";

    private const string PLANET_LARGE_SPAWN_DETAIL = "LargeSizeSpawnRate";
    private const string PLANET_LARGE_RESOURCE_MIN_DETAIL = "ResourceAmountLargeMinimum";
    private const string PLANET_LARGE_RESOURCE_MAX_DETAIL = "ResourceAmountLargeMaximum";
    private const string PLANET_LARGE_POPULATION_MIN_DETAIL = "PopulationAmountLargeMinimum";
    private const string PLANET_LARGE_POPULATION_MAX_DETAIL = "PopulationAmountLargeMaximum";

    private const string TILE_LISTING_PREFAB = "TileListing";

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

    public string Name { get { return _name; } }
    public Team Team { get { return _team; } }
    public Structure Structure { get { return _structure; } }
    public Squad Squad { get { return _squad; } }
    public float Radius { get { return _radius; } }
    public int Population 
    { 
        get { return _population; }
        set { _population = value; }
    }
    public Inhabitance PopulationType {  get { return _planetInhabitance; } }

	// Use this for initialization
    public void Init(string type, string name, Sector sector)
    {
        _name = name;
        _planetType = type;
        _squad = this.GetComponent<Squad>();
        _squad.Init();
        _diplomacy = new Dictionary<global::Team, bool>();
        transform.SetParent(sector.transform);
    }

	void Start () 
    {
        // Determine tile type
        var mapManager = MapManager.Instance;
        // Determine Size, Population, and Resource Amount

        // Determine Inhabitance
        var chance = (float)GameManager.Generator.NextDouble();
        foreach (var inhabit in mapManager.PlanetInhabitanceSpawnTable[_planetType])
        {
            if (chance <= inhabit.Value)
            {
                _planetInhabitance = inhabit.Key;
                break;
            }
        }

        chance = (float)GameManager.Generator.NextDouble();
        var small = true;
        if (chance < float.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_SMALL_SPAWN_DETAIL]))
        {
            small = true;

            int minimum, maximum;
            if (_planetInhabitance != Inhabitance.Uninhabited)
            {
                minimum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_SMALL_POPULATION_MIN_DETAIL]);
                maximum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_SMALL_POPULATION_MAX_DETAIL]);
                _population = GameManager.Generator.Next(minimum, maximum + 1);
            }

            minimum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_SMALL_RESOURCE_MIN_DETAIL]);
            maximum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_SMALL_RESOURCE_MAX_DETAIL]);
            _resourceCount = GameManager.Generator.Next(minimum, maximum + 1);
        }
        else
        {
            small = false;

            int minimum, maximum;
            if (_planetInhabitance != Inhabitance.Uninhabited)
            {
                minimum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_LARGE_POPULATION_MIN_DETAIL]);
                maximum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_LARGE_POPULATION_MAX_DETAIL]);
                _population = GameManager.Generator.Next(minimum, maximum + 1);
            }

            minimum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_LARGE_RESOURCE_MIN_DETAIL]);
            maximum = int.Parse(mapManager.PlanetSpawnDetails[_planetType][PLANET_LARGE_RESOURCE_MAX_DETAIL]);
            _resourceCount = GameManager.Generator.Next(minimum, maximum + 1);
        }

        // Determine Resource Type
        _resourceType = Resource.NoResource;
        chance = (float)GameManager.Generator.NextDouble();
        foreach (var resource in mapManager.PlanetResourceSpawnTable[_planetType])
        {
            if (chance <= resource.Value)
            {
                _resourceType = resource.Key;
                break;
            }
        }

        if(mapManager.PlanetTextureTable[_planetType].Texture != null)
        {
            var system = GetComponent<ParticleSystem>();
            var renderer = system.GetComponent<Renderer>();

            _radius = 2.0f;
            _clickRadius = 1.5f;
            if (small)
            {
                system.startSize *= 0.5f;
                _radius = 1.0f;
                _clickRadius = 1.0f;
            }

            renderer.material.mainTexture = mapManager.PlanetTextureTable[_planetType].Texture;
            renderer.material.mainTextureOffset = mapManager.PlanetTextureTable[_planetType].TextureOffset;
            renderer.material.mainTextureScale = mapManager.PlanetTextureTable[_planetType].TextureScale;

            system.enableEmission = true;
            renderer.enabled = true;

            if (_population > 0)
            {
                _team = Team.Indigenous;
                _squad.Team = Team.Indigenous;

                // generate random defenses if space age
            }

            _squad.Init();
        }
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

    public void Relinquish()
    {
        GameManager.Instance.Players[_team].Tiles.Remove(this);
    }

    public void Claim(Team team)
    {
        _team = team;
        GameManager.Instance.Players[_team].Tiles.Add(this);
        _squad.Team = _team;

        var teams = _diplomacy.Keys.ToList();
        foreach(var t in teams)
        {
            _diplomacy[t] = false;
        }
    }

    public string Undeploy(bool destroy)
    {
        if (_structure == null)
            return "";

        if (!destroy && _structure != null)
            _squad.Ships.Add(_structure);
        _structure.Undeploy(this);
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

    public void Gather()
    {
        if (_structure == null)
            return;

        var gathered = _structure.Gather(_resourceType, _resourceCount, _planetInhabitance, _population);

        foreach (var resource in gathered)
        {
            switch (resource.Key)
            {
                case ResourceGatherType.None:
                case ResourceGatherType.Soldiers:
                    _population -= resource.Value;
                    break;
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
        if (_team == Team.Indigenous) // use Indigenous
        {
            switch(_planetInhabitance)
            {
                case Inhabitance.Uninhabited:
                    break;
                case Inhabitance.Primitive:
                    power = _population;
                    break;
                case Inhabitance.Industrial:
                    power = _population * 1.5f;
                    break;
                case Inhabitance.SpaceAge:
                    power = _population * 1.75f;
                    break;
            }
        }
        else if (_structure == null)
        {
            var primitive = _structure.PrimitivePopulation;
            var industrial = _structure.IndustrialPopulation;
            var spaceAge = _structure.SpaceAgePopulation;

            power = primitive + industrial * 1.5f + spaceAge * 1.75f + _structure.Defense;
        }

        return power;
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

        if(_team == global::Team.Indigenous)
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

        int primitive = 0, industrial = 0, spaceAge = 0;

        switch(_planetInhabitance)
        {
            case Inhabitance.Primitive:
                primitive += _population;
                break;
            case Inhabitance.Industrial:
                industrial += _population;
                break;
            case Inhabitance.SpaceAge:
                spaceAge += _population;
                break;
        }
        
        if(_structure != null)
        {
            primitive += _structure.PrimitivePopulation;
            industrial += _structure.IndustrialPopulation;
            spaceAge += _structure.SpaceAgePopulation;
        }

        panel.transform.FindChild("TotalPopulation").GetComponent<Text>().text = (primitive + industrial + spaceAge).ToString();
        panel.transform.FindChild("PrimitivePopulation").GetComponent<Text>().text = primitive.ToString();
        panel.transform.FindChild("IndustrialPopulation").GetComponent<Text>().text = industrial.ToString();
        panel.transform.FindChild("SpaceAgePopulation").GetComponent<Text>().text = spaceAge.ToString();
    }

    public bool IsInRange(Squad squad)
    {
        return (squad.transform.position - transform.position).sqrMagnitude <= (_radius * _radius);
    }

    public bool IsInClickRange(Vector3 click)
    {
        return (click - transform.position).sqrMagnitude <= (_clickRadius * _clickRadius);
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
