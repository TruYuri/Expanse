﻿using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Override of the Player class for AI behavior.
/// </summary>
class AIPlayer : Player
{
    private Dictionary<Sector, Dictionary<Tile, Squad>> _defensiveSquads; // map of owned sectors and their defending tile squads
    private Dictionary<Sector, Squad> _sectorDefensiveSquads; 
    private Dictionary<Sector, Squad> _sectorSquad;
    private Dictionary<Squad, Sector> _squadSector;

    /// <summary>
    /// Initializes the AI Player
    /// </summary>
    /// <param name="team">The AI player's team.</param>
    public override void Init(Team team)
    {
        base.Init(team);
        _defensiveSquads = new Dictionary<Sector, Dictionary<Tile, Squad>>();
        _sectorSquad = new Dictionary<Sector, Squad>();
        _sectorDefensiveSquads = new Dictionary<Sector, Squad>();
        _squadSector = new Dictionary<Squad, Sector>();
    }

    /// <summary>
    /// Adds a new squad for this AI to manage in a sector
    /// </summary>
    /// <param name="s">The sector to register in.</param>
    /// <param name="sq">The squad to register.</param>
    public void RegisterSectorSquad(Sector s, Squad sq)
    {
        if (!_sectorSquad.ContainsKey(s))
            _sectorSquad.Add(s, sq);
        else
            _sectorSquad[s] = sq;

        _squadSector.Add(sq, s);
    }

    /// <summary>
    /// Adds a new squad for this AI to manage in a sector, around a tile
    /// </summary>
    /// <param name="s">The sector to register in.</param>
    /// <param name="t">The tile to register at.</param>
    /// <param name="sq">The squad to register.</param>
    public void RegisterDefensiveSquad(Sector s, Tile t, Squad sq)
    {
        if (t != null)
        {
            if (!_defensiveSquads.ContainsKey(s))
                _defensiveSquads.Add(s, new Dictionary<Tile, Squad>());
            if (!_defensiveSquads[s].ContainsKey(t))
                _defensiveSquads[s].Add(t, sq);
            else
                _defensiveSquads[s][t] = sq;
        }
        else
        {
            if (!_sectorDefensiveSquads.ContainsKey(s))
                _sectorDefensiveSquads.Add(s, sq);
            else
                _sectorDefensiveSquads[s] = sq;
        }
    }

    /// <summary>
    /// Updates an AI squad's behavior.
    /// </summary>
    /// <param name="sq">The squad to update.</param>
    public void UpdateAI(Squad sq)
    {
        if (_squadSector.ContainsKey(sq) && (sq.Mission == null || sq.Mission.GetType() == typeof(BattleEvent)))
        {
            var sc = sq.CountSoldiers();

            if (sq.Tile != null)
            {
                if (sq.Tile.Team != sq.Tile.Squad.Team && sq.Tile.Squad.Ships.Count > 0)
                    CreateBattleEvent(sq, sq.Tile.Squad);
                else if (sq.Tile.Team != _team && sq.Tile.IsInRange(sq))
                {
                    if (sc.Count > 0)
                        CreateBattleEvent(sq, sq.Tile);
                    else
                        CreateTravelEvent(sq, _squadSector[sq], _squadSector[sq].transform.position, 25f);
                }
                else if (sq.Tile.Team == _team)
                {
                    _squadSector.Remove(sq);
                    RegisterDefensiveSquad(sq.Sector, sq.Tile, sq);
                }
            }
            else if (sq.Sector != _squadSector[sq])
                CreateTravelEvent(sq, _squadSector[sq], _squadSector[sq].transform.position, 25f);
            else
            {
                _squadSector.Remove(sq);
                RegisterDefensiveSquad(sq.Sector, null, sq);
            }
        }
    }

    /// <summary>
    /// Updates player squad behaviors based on an enemy squad and location.
    /// </summary>
    /// <param name="s">The active sector to use squads in.</param>
    /// <param name="sq">The enemy squad to consider.</param>
    public void SetSquadBehaviors(Sector s, Squad sq)
    {
        if(_defensiveSquads.ContainsKey(s))
        {
            var tDelete = new List<Tile>();
            foreach (var d in _defensiveSquads[s])
            {
                if (d.Key.Team != _team || d.Value == null || d.Value.gameObject == null)
                    tDelete.Add(d.Key);
                else if (d.Value != null && d.Value.gameObject != null && d.Value.Mission == null
                    && (d.Key.transform.position - sq.transform.position).sqrMagnitude < d.Key.DefensiveRange * d.Key.DefensiveRange)
                    CreateChaseEvent(d.Value, sq, d.Key, d.Key.DefensiveRange, 25f);
            }

            foreach (var t in tDelete)
                _defensiveSquads[s].Remove(t);
            if (_defensiveSquads[s].Count == 0)
                _defensiveSquads.Remove(s);
        }

        if (_sectorDefensiveSquads.ContainsKey(s) && _sectorDefensiveSquads[s].Mission == null)
            CreateChaseEvent(_sectorDefensiveSquads[s], sq, null, float.PositiveInfinity, 25f);
        

        if(_sectorSquad.ContainsKey(s) && (_sectorSquad[s] == null || _sectorSquad[s].gameObject == null))
            _sectorSquad.Remove(s);
    }

    /// <summary>
    /// Populates a squad with random numbers of ships based on rules.
    /// </summary>
    /// <param name="squad">The squad to populate.</param>
    public void PopulateRandomSquad(Squad squad)
    {
        int n = GameManager.Generator.Next(5, 26);
        for (int i = 0; i < n; i++)
            AddShip(squad, "Fighter");

        n = GameManager.Generator.Next(0, 11);
        for (int i = 0; i < n; i++)
        {
            var a = AddShip(squad, "Transport");
            a.Population[Inhabitance.SpaceAge] = GameManager.Generator.Next(0, a.Capacity);
        }

        n = GameManager.Generator.Next(2, 5);
        for (int i = 0; i < n; i++)
            AddShip(squad, "Guard Satellite");

        n = GameManager.Generator.Next(0, 11);
        for (int i = 0; i < n; i++)
        {
            var a = AddShip(squad, "Heavy Fighter");
            a.Population[Inhabitance.SpaceAge] = GameManager.Generator.Next(0, a.Capacity);
        }

        /*
        n = GameManager.Generator.Next(0, 6);
        for (int i = 0; i < n; i++)
            AddShip(_squad, "Behemoth");*/
    }
}
