﻿using UnityEngine;
using System.Collections.Generic;

public class DiplomacyEvent : GameEvent 
{
    private Team _team;
    private Tile _tile;
    private int _diplomacyTurns;

    // turn parameter = turns until command begins. 
    // calculate travel turns - 1 turn per sector, swap out remaining turns when initial == 0
    public DiplomacyEvent(int turns, Team team, Tile tile) : base(turns)
    {
        _team = team;
        _tile = tile;
        _diplomacyTurns = 5;
        tile.SetDiplomaticEffort(team);
    }

    // when travelling, travel between planets (x = 10x, y = 10y)
    // travel from one waypoint to the next

    public override void Progress()
    {
        base.Progress();

        if (_remainingTurns > 0 && _diplomacyTurns > 0) // waiting for command to reach the squad
            return;

        if (_remainingTurns == 0 && _diplomacyTurns > 0) // swap to diplomacy
        {
            _remainingTurns = _diplomacyTurns;
            _diplomacyTurns = 0;
            _stage = GameEventStage.Continue;
        }
        else if(_remainingTurns == 0)
        {
            GameManager.Instance.Players[_team].DiplomaticEffort(_tile);
            return;
        }
    }

    public override void Update()
    {
    }

    public override bool AssertValid()
    {
        if (_tile.Team == Team.Indigenous)
            return true;
        return false;
    }
}