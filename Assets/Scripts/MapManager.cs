﻿using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    private readonly Vector3 TOP_RIGHT_OFFSET = new Vector3(93.1f, 0.0f, 140.6f);
    private readonly Vector3 RIGHT_OFFSET = new Vector3(186.2f, 0, 0.0f);
    private readonly Vector3 BOTTOM_RIGHT_OFFSET = new Vector3(93.1f, 0.0f, -140.6f);
    private readonly Vector3 BOTTOM_LEFT_OFFSET = new Vector3(-93.1f, 0.0f, -140.6f);
    private readonly Vector3 LEFT_OFFSET = new Vector3(-186.2f, 0, 0);
    private readonly Vector3 TOP_LEFT_OFFSET = new Vector3(-93.1f, 0.0f, 140.6f);
    private const string SECTOR_PREFAB = "Sector";
    private const string INI_PATH = "/Resources/Planets.ini";
    private const string MATERIALS_PATH = "PlanetTextures/";
    private const string PLANET_SECTION_HEADER = "[Planet Spawn Rates]";
    private const string DEPLOYABLE_SECTION_HEADER = "[Deployable Planets]";
    private const string PLANET_TEXTURE_DETAIL = "SpriteName";
    private const string PLANET_UNINHABITED_DETAIL = "Uninhabited";
    private const string PLANET_PRIMITIVE_DETAIL = "InhabitedPrimitive";
    private const string PLANET_INDUSTRIAL_DETAIL = "InhabitedIndustrial";
    private const string PLANET_SPACEAGE_DETAIL = "InhabitedSpaceAge";
    private const string PLANET_FOREST_DETAIL = "Forest";
    private const string PLANET_ORE_DETAIL = "Ore";
    private const string PLANET_OIL_DETAIL = "Oil";
    private const string PLANET_ASTERMINIUM_DETAIL = "Asterminium";

    private static MapManager _instance;
    private Dictionary<string, float> _planetTypeSpawnTable;
    private Dictionary<string, float> _deploySpawnTable;
    private Dictionary<string, TextureAtlasDetails> _planetTextureTable;
    private Dictionary<string, Dictionary<Inhabitance, float>> _planetInhabitanceSpawnTable;
    private Dictionary<string, Dictionary<Resource, float>> _planetResourceSpawnTable;
    private Dictionary<string, Dictionary<string, string>> _planetSpawnDetails;
    private Texture2D _textureAtlas;
    private Dictionary<int, Dictionary<int, GameObject>> _sectorMap;

    public static MapManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<MapManager>();
            }
            return _instance;
        }
    }

    public Dictionary<string, float> PlanetTypeSpawnTable { get { return _planetTypeSpawnTable; } }
    public Dictionary<string, float> DeploySpawnTable { get { return _deploySpawnTable; } }
    public Dictionary<string, TextureAtlasDetails> PlanetTextureTable { get { return _planetTextureTable; } }
    public Dictionary<string, Dictionary<Inhabitance, float>> PlanetInhabitanceSpawnTable { get { return _planetInhabitanceSpawnTable; } }
    public Dictionary<string, Dictionary<Resource, float>> PlanetResourceSpawnTable { get { return _planetResourceSpawnTable; } }
    public Dictionary<string, Dictionary<string, string>> PlanetSpawnDetails { get { return _planetSpawnDetails; } }
    public Dictionary<int, Dictionary<int, GameObject>> SectorMap { get { return _sectorMap; } }

    void Awake()
    {
        _instance = this;
        _planetTypeSpawnTable = new Dictionary<string, float>();
        _deploySpawnTable = new Dictionary<string, float>();
        _planetTextureTable = new Dictionary<string, TextureAtlasDetails>();
        _planetInhabitanceSpawnTable = new Dictionary<string, Dictionary<Inhabitance, float>>();
        _planetResourceSpawnTable = new Dictionary<string, Dictionary<Resource, float>>();
        _planetSpawnDetails = new Dictionary<string, Dictionary<string, string>>();
        _sectorMap = new Dictionary<int, Dictionary<int, GameObject>>();
        _planetSpawnDetails = new Dictionary<string, Dictionary<string, string>>();

        var parser = new INIParser(Application.dataPath + INI_PATH);
        var spawnTables = parser.ParseINI();
        var textures = new Texture2D[spawnTables.Count]; // to ensure correct order
        var planetNames = new string[spawnTables.Count]; // to ensure correct order
        var planetCount = 0;
        var runningTotal = 0.0f;

        // cache planet spawn probabilities
        foreach (var planet in spawnTables[PLANET_SECTION_HEADER])
        {
            // Generate upper limit in 0.0 - 1.0 spectrum for planet
            runningTotal += float.Parse(planet.Value);
            _planetTypeSpawnTable.Add(planet.Key, runningTotal);
        }
        foreach (var deploy in spawnTables[DEPLOYABLE_SECTION_HEADER])
        {
            _deploySpawnTable.Add(deploy.Key, 0);
        }
        spawnTables.Remove(PLANET_SECTION_HEADER);
        spawnTables.Remove(DEPLOYABLE_SECTION_HEADER);

        foreach (var planet in spawnTables)
        {
            var key = planet.Key.TrimStart('[');
            key = key.TrimEnd(']');
            _planetInhabitanceSpawnTable.Add(key, new Dictionary<Inhabitance, float>());
            _planetResourceSpawnTable.Add(key, new Dictionary<Resource, float>());

            // load texture for atlasing
            textures[planetCount] = Resources.Load<Texture2D>(MATERIALS_PATH + spawnTables[planet.Key][PLANET_TEXTURE_DETAIL]);
            planetNames[planetCount++] = key;

            // cache per-planet Inhabitance probabilities
            _planetInhabitanceSpawnTable[key].Add(Inhabitance.Uninhabited, runningTotal = float.Parse(spawnTables[planet.Key][PLANET_UNINHABITED_DETAIL]));
            _planetInhabitanceSpawnTable[key].Add(Inhabitance.Primitive, runningTotal += float.Parse(spawnTables[planet.Key][PLANET_PRIMITIVE_DETAIL]));
            _planetInhabitanceSpawnTable[key].Add(Inhabitance.Industrial, runningTotal += float.Parse(spawnTables[planet.Key][PLANET_INDUSTRIAL_DETAIL]));
            _planetInhabitanceSpawnTable[key].Add(Inhabitance.SpaceAge, runningTotal += float.Parse(spawnTables[planet.Key][PLANET_SPACEAGE_DETAIL]));

            // cache per-planet Resource probabilities
            _planetResourceSpawnTable[key].Add(Resource.Forest, runningTotal = float.Parse(spawnTables[planet.Key][PLANET_FOREST_DETAIL]));
            _planetResourceSpawnTable[key].Add(Resource.Ore, runningTotal += float.Parse(spawnTables[planet.Key][PLANET_ORE_DETAIL]));
            _planetResourceSpawnTable[key].Add(Resource.Oil, runningTotal += float.Parse(spawnTables[planet.Key][PLANET_OIL_DETAIL]));
            _planetResourceSpawnTable[key].Add(Resource.Asterminium, runningTotal += float.Parse(spawnTables[planet.Key][PLANET_ASTERMINIUM_DETAIL]));

            // remove used data from the table
            spawnTables[planet.Key].Remove(PLANET_UNINHABITED_DETAIL);
            spawnTables[planet.Key].Remove(PLANET_PRIMITIVE_DETAIL);
            spawnTables[planet.Key].Remove(PLANET_INDUSTRIAL_DETAIL);
            spawnTables[planet.Key].Remove(PLANET_SPACEAGE_DETAIL);

            spawnTables[planet.Key].Remove(PLANET_FOREST_DETAIL);
            spawnTables[planet.Key].Remove(PLANET_ORE_DETAIL);
            spawnTables[planet.Key].Remove(PLANET_OIL_DETAIL);
            spawnTables[planet.Key].Remove(PLANET_ASTERMINIUM_DETAIL);

            _planetSpawnDetails.Add(key, spawnTables[planet.Key]);
        }

        _textureAtlas = new Texture2D(0, 0);
        var atlasEntries = _textureAtlas.PackTextures(textures, 0);

        for (int i = 0; i < planetCount; i++)
        {
            _planetTextureTable.Add(planetNames[i],
                new TextureAtlasDetails((atlasEntries[i].width == 0 && atlasEntries[i].height == 0 ? null : _textureAtlas),
                                        new Vector2(atlasEntries[i].x, atlasEntries[i].y),
                                        new Vector2(atlasEntries[i].width, atlasEntries[i].height)));
            spawnTables["[" + planetNames[i]+ "]"].Remove(PLANET_TEXTURE_DETAIL);
        }

        parser.CloseINI();
        GenerateSector(Vector3.zero, 0, 0);
    }

	// Use this for initialization
	public void Start()
    {     
	}

    public void GenerateNewSectors(Vector3 realPosition, Vector2 gridPosition)
    {
        var position = realPosition;
        var v = (int)gridPosition.x;
        var h = (int)gridPosition.y;

        if (Mathf.Abs(v) % 2 == 0) // even grid row
        {
            GenerateSector(position + TOP_RIGHT_OFFSET, v + 1, h);
            GenerateSector(position + RIGHT_OFFSET, v, h + 1);
            GenerateSector(position + BOTTOM_RIGHT_OFFSET, v - 1, h);
            GenerateSector(position + BOTTOM_LEFT_OFFSET, v - 1, h - 1);
            GenerateSector(position + LEFT_OFFSET, v, h - 1);
            GenerateSector(position + TOP_LEFT_OFFSET, v + 1, h - 1);
        }
        else // odd row
        {
            GenerateSector(position + TOP_RIGHT_OFFSET, v + 1, h + 1);
            GenerateSector(position + RIGHT_OFFSET, v, h + 1);
            GenerateSector(position + BOTTOM_RIGHT_OFFSET, v - 1, h + 1);
            GenerateSector(position + BOTTOM_LEFT_OFFSET, v - 1, h);
            GenerateSector(position + LEFT_OFFSET, v, h - 1);
            GenerateSector(position + TOP_LEFT_OFFSET, v + 1, h);
        }
    }

    private void GenerateSector(Vector3 position, int vertical, int horizontal)
    {
        if (!_sectorMap.ContainsKey(vertical))
            _sectorMap.Add(vertical, new Dictionary<int, GameObject>());

        if(!_sectorMap[vertical].ContainsKey(horizontal))
        {
            var sectorPrefab = Resources.Load<UnityEngine.Object>(SECTOR_PREFAB);
            var sector = Instantiate(sectorPrefab, position, Quaternion.Euler(-90f, 0, 0)) as GameObject;
            var component = sector.GetComponent<Sector>();
            component.Init(new Vector2(vertical, horizontal));
            _sectorMap[vertical].Add(horizontal, sector);
        }
    }

    private class AStarSectorNode
    {
        public List<KeyValuePair<int, int>> sectorPath;
        public int g;
        public int h;
        public string path;

        public AStarSectorNode(List<KeyValuePair<int, int>> path, string val, int gf)
        {
            this.sectorPath = path;
            this.g = gf;
            this.h = path.Count; // heuristic = size of path.
            // alternative: h = sum of sqrt dist from goal
            this.path = val;
        }

        public List<AStarSectorNode> succ()
        {
            List<AStarSectorNode> children = new List<AStarSectorNode>();
            
            var v = sectorPath[sectorPath.Count - 1].Key;
            var h = sectorPath[sectorPath.Count - 1].Value;
            if(Mathf.Abs(v) % 2 == 0)
            {
                children.Add(New(v + 1, h, 'N'));
                children.Add(New(v, h + 1, 'R'));
                children.Add(New(v - 1, h, 'E'));
                children.Add(New(v - 1, h - 1, 'W'));
                children.Add(New(v, h - 1, 'L'));
                children.Add(New(v + 1, h - 1, 'S'));
            }
            else
            {
                children.Add(New(v + 1, h + 1, 'N'));
                children.Add(New(v, h + 1, 'R'));
                children.Add(New(v - 1, h + 1, 'E'));
                children.Add(New(v - 1, h, 'W'));
                children.Add(New(v, h - 1, 'L'));
                children.Add(New(v + 1, h, 'S'));
            }

            return children;
        }

        private AStarSectorNode New(int x, int y, char c)
        {
            var list = new List<KeyValuePair<int, int>>(sectorPath);
            list.Add(new KeyValuePair<int,int>(x, y));
            return new AStarSectorNode(list, path + c, g + 1);
        }
    }

    public List<KeyValuePair<int, int>> AStarSearch(Sector start, Sector goal)
    {
        var fringe = new List<AStarSectorNode>();
        var fringeSet = new Dictionary<string, AStarSectorNode>();
        var explored = new HashSet<string>();

        var startPos = new KeyValuePair<int, int>((int)start.GridPosition.x, (int)start.GridPosition.y);
        var endpos = new KeyValuePair<int, int>((int)goal.GridPosition.x, (int)goal.GridPosition.y);

        var initList = new List<KeyValuePair<int, int>>();
        initList.Add(startPos);

        AStarSectorNode init = new AStarSectorNode(initList, string.Empty, 0);
        fringe.Add(init);
        fringeSet.Add(init.path, init);

        while(fringe.Count > 0)
        {
            AStarSectorNode cur = fringe[0];

            var last = cur.sectorPath[cur.sectorPath.Count - 1];
            if (last.Key == endpos.Key && last.Value == endpos.Value)
                return cur.sectorPath;

            fringe.RemoveAt(0);
            fringeSet.Remove(cur.path);
            explored.Add(cur.path);

            List<AStarSectorNode> exp = cur.succ();

            for(int i = 0; i < 6; i++) // 6 possible movements
            {
                if(explored.Contains(exp[i].path))
                    continue;

                bool inFringe = fringeSet.ContainsKey(exp[i].path);
                if(!inFringe)
                {
                    fringe.Add(exp[i]);
                    fringe = fringe.OrderBy(o => o.h + o.g).ToList();
                    fringeSet.Add(exp[i].path, exp[i]);
                }
                else
                {
                    var val = fringeSet[exp[i].path];

                    if(exp[i].g + exp[i].h < val.g + val.h)
                    {
                        val.g = exp[i].g;
                        val.h = exp[i].h;
                        val.sectorPath = exp[i].sectorPath;
                    }
                }
            }
        }

        return null;
    }
}
