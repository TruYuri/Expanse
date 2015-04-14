﻿using UnityEngine;
using System.Collections.Generic;

public class TravelEvent : GameEvent 
{
    private Squad _squad;
    private Vector3 _destination;
    private float _velocity;
    private List<Sector> _destinationSectors;
    private List<Vector3> _turnDestinations;
    private int _travelTurns;

    // turn parameter = turns until command begins. 
    // calculate travel turns - 1 turn per sector, swap out remaining turns when initial == 0
    public TravelEvent(int turns, Squad squad, Sector destinationSector, Vector3 destination, float velocity) : base(turns)
    {
        _squad = squad;
        _destination = destination;
        _velocity = velocity;
        _squad.Mission = this;
        _destinationSectors = MapManager.Instance.AStarSearch(squad.Sector, destinationSector);
        _travelTurns = _destinationSectors.Count;
    }

    // when travelling, travel between planets (x = 10x, y = 10y)
    // travel from one waypoint to the next

    public override void Progress()
    {
        base.Progress();

        if (_remainingTurns > 0 && _travelTurns > 0) // waiting for command to reach the squad
            return;

        if(_remainingTurns <= 0 && _travelTurns > 0) // swap to travelling
        {
            _remainingTurns = _travelTurns;
            _travelTurns = 0;
            _stage = GameEventStage.Continue;
        }
        else if(_remainingTurns <= 0)
        {
            _squad.transform.position = _destination;
            _squad.Mission = null;
            return;
        }

        if (_turnDestinations != null && _turnDestinations.Count > 0)
            _squad.transform.position = _turnDestinations[_turnDestinations.Count - 1];

        _turnDestinations = new List<Vector3>();

        Sector cur = null;
        if (_destinationSectors.Count > 1)
        {
            cur = _destinationSectors[0];
            _destinationSectors.RemoveAt(0);
        }

        if (_destinationSectors.Count >= 1 && cur != null)
        {
            var next = _destinationSectors[0];

            // add final corner first
            var dir = (cur.transform.position - next.transform.position).normalized;
            _turnDestinations.Add((cur.transform.position + next.transform.position) / 2.0f + dir * 2.0f);

            // determine columns/rows to traverse to reach the corner - avoiding planets

            
        }
        else if (_destinationSectors.Count == 1)
        {
            // add final destination
            _turnDestinations.Add(_destination);

            // determine coumns/rows to traverse to reach the destination - avoiding planets
        }
    }

    public override void Update()
    {
        if (_travelTurns > 0 || _turnDestinations == null || _turnDestinations.Count == 0)
            return;

        _squad.transform.position = Vector3.MoveTowards(_squad.transform.position, _turnDestinations[0], _velocity * Time.deltaTime);

        var diff = _squad.transform.position - _turnDestinations[0];
        if (diff.magnitude < 0.1f)
            _turnDestinations.RemoveAt(0);
    }

    public override bool AssertValid()
    {
        if (_squad != null && _squad.gameObject != null && _squad.Mission == this)
            return true;
        return false;
    }
}
