﻿using UnityEngine;
using System.Collections;

public class WarpPortal : Ship
{
    private int range;
    private float defense;

    public int Range
    {
        get { return range; }
        set { range = value; }
    }

    public float Defense
    {
        get { return defense; }
        set { defense = value; }
    }

    public WarpPortal(string name, float hull, float firepower, float speed, int capacity, int range, float defense)
        : base(name, hull, firepower, speed, capacity)
    {
        this.range = range;
        this.defense = range;
    }

    public override Ship Copy()
    {
        var ship = new WarpPortal(name, baseHull, baseFirepower, baseSpeed, baseCapacity, range, defense);
        ship.Hull = hull;
        ship.Firepower = firepower;
        ship.Speed = speed;
        ship.Capacity = capacity;
        ship.Protection = protection;

        return ship;
    }
}