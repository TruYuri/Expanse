﻿using UnityEngine;
using System.Collections;

public class BuildEvent : GameEvent
{
    private Tile _tile;
    private Ship _ship;
    private Player _player;
    private Structure _structure;

    public BuildEvent(int turns, Player player, Tile tile, Ship ship)
        : base(turns)
    {
        _tile = tile;
        _ship = ship;
        _structure = tile.Structure;
        _player = player;
    }

    public override void Progress()
    {
        base.Progress();

        if (_remainingTurns > 0)
            return;

        _player.AddShip(_tile.Squad, _ship);
    }

    public override bool AssertValid()
    {
        if (_tile.Team == _player.Team && _tile.Structure == _structure)
            return true;
        return false;
    }
}
