﻿using UnityEngine;
using System.Collections;

public class WarpPortal : Ship
{
    private int range;

    public int Range
    {
        get { return range; }
        set { range = value; }
    }

    public WarpPortal(Sprite icon, string name, float hull, float firepower, float speed, int capacity, int range)
        : base(icon, name, hull, firepower, speed, capacity, ShipType.WarpPortal)
    {
        this.range = range;
    }

    public override Ship Copy()
    {
        var ship = new WarpPortal(icon, name, baseHull, baseFirepower, baseSpeed, baseCapacity, range);
        ship.Hull = hull;
        ship.Firepower = firepower;
        ship.Speed = speed;
        ship.Capacity = capacity;
        ship.Protection = protection;

        return ship;
    }
}
