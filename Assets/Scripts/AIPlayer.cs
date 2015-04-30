﻿using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

class AIPlayer : Player
{
    private Dictionary<Sector, Dictionary<Tile, Squad>> _defensiveSquads;

    public override void Init(Team team)
    {
        base.Init(team);
        _defensiveSquads = new Dictionary<Sector, Dictionary<Tile, Squad>>();
    }

    void Update()
    {
        if (GameManager.Instance.Paused || _turnEnded)
            return;
    }

    public void RegisterDefensiveSquad(Sector s, Tile t, Squad sq)
    {
        if (!_defensiveSquads.ContainsKey(s))
            _defensiveSquads.Add(s, new Dictionary<Tile, Squad>());
        if (!_defensiveSquads[s].ContainsKey(t))
            _defensiveSquads[s].Add(t, sq);
        else
            _defensiveSquads[s][t] = sq;
    }

    public void SetPotentialChase(Sector s, Squad sq)
    {
        if(_defensiveSquads.ContainsKey(s))
        {
            foreach (var d in _defensiveSquads[s])
                if (d.Key.Team == _team && d.Value != null && d.Value.gameObject != null && d.Value.Mission == null
                    && (d.Key.transform.position - sq.transform.position).sqrMagnitude < d.Key.DefensiveRange * d.Key.DefensiveRange)
                    CreateChaseEvent(d.Value, sq, d.Key, d.Key.DefensiveRange, 25f);
        }
    }


    public void PopulateRandomSquad(Squad squad)
    {
        int n = GameManager.Generator.Next(5, 26);
        for (int i = 0; i < n; i++)
            AddShip(squad, "Fighter");

        n = GameManager.Generator.Next(0, 11);
        for (int i = 0; i < n; i++)
            AddShip(squad, "Transport");

        n = GameManager.Generator.Next(2, 5);
        for (int i = 0; i < n; i++)
            AddShip(squad, "Guard Satellite");

        n = GameManager.Generator.Next(0, 11);
        for (int i = 0; i < n; i++)
            AddShip(squad, "Heavy Fighter");

        /*
        n = GameManager.Generator.Next(0, 6);
        for (int i = 0; i < n; i++)
            AddShip(_squad, "Behemoth");*/
    }
}
