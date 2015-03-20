﻿using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    private const string MILITARY = "Military";
    private const string SCIENCE = "Scientific";

    protected Team _team;
    protected Squad _controlledSquad;
    protected Dictionary<string, Ship> _shipDefinitions;
    protected List<Squad> _squads;
    protected int _numResearchStations;
    protected ResearchTree _militaryTree;
    protected ResearchTree _scienceTree;

    public Team Team { get { return _team; } }

	void Start () 
    {
        _shipDefinitions = GameManager.Instance.GenerateShipDefs();
        _militaryTree = GameManager.Instance.GenerateMilitaryTree(_shipDefinitions);
        _scienceTree = GameManager.Instance.GenerateScienceTree(_shipDefinitions);
	}
	
	public virtual void Control(Squad gameObject)
    {
        _controlledSquad = gameObject;
    }

	// Update is called once per frame
	void Update () 
    {
        if (GameManager.Instance.Paused)
            return;
	}

    public bool UpgradeResearch(string type, string research, string property)
    {
        if(type == MILITARY)
            return _militaryTree.GetResearch(research).UpgradeResearch(property, _numResearchStations);
        else if(type == SCIENCE)
            return _scienceTree.GetResearch(research).UpgradeResearch(property, _numResearchStations);
        return false;
    }

    public virtual void Deploy(int shipIndex)
    {
        var tile = _controlledSquad.Deploy(shipIndex);
        Control(tile.Squad);
    }

    public virtual void Battle()
    {
    }
}
