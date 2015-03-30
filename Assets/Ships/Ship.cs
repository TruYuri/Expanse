﻿using UnityEngine;
using System.Collections;

public class Ship 
{
    protected bool unlocked;
    protected Sprite icon;
    protected string name;
    protected float hull;
    protected float firepower;
    protected float speed;
    protected int capacity;
    protected float protection;

    protected float baseHull;
    protected float baseFirepower;
    protected float baseSpeed;
    protected int baseCapacity;

    protected int totalPopulation;
    protected int primitivePopulation;
    protected int industrialPopulation;
    protected int spaceAgePopulation;

    protected int requiredOre;
    protected int requiredAsterminium;
    protected int requiredOil;
    protected int requiredForest;
    protected int requiredStations;

    protected ShipType shipType;

    public Sprite Icon { get { return icon; } }
    public string Name
    {
        get { return name; }
    }
    public float Hull 
    { 
        get { return hull; }
        set { hull = value; }
    }
    public float Firepower 
    { 
        get { return firepower; }
        set { firepower = value; }
    }
    public float Speed 
    { 
        get { return speed; }
        set { speed = value; }
    }
    public int Capacity 
    { 
        get { return capacity; }
        set { capacity = value; }
    }
    public float Protection 
    { 
        get { return protection; }
        set { protection = value; }
    }
    public int Population
    {
        get { return totalPopulation; }
        set { totalPopulation = value; }
    }
    public int PrimitivePopulation
    {
        get { return primitivePopulation; }
        set { primitivePopulation = value; }
    }
    public int IndustrialPopulation
    {
        get { return industrialPopulation; }
        set { industrialPopulation = value; }
    }
    public int SpaceAgePopulation
    {
        get { return spaceAgePopulation; }
        set { spaceAgePopulation = value; }
    }
    public bool Unlocked
    {
        get { return unlocked; }
        set { unlocked = value; }
    }

    public ShipType ShipType { get { return shipType; } }

    public Ship(Sprite icon, string name, float hull, float firepower, float speed, int capacity, ShipType shipType, 
        int ore, int oil, int asterminium, int forest, int stations)
    {
        this.name = name;
        this.hull = this.baseHull = hull;
        this.firepower = this.baseFirepower = firepower;
        this.speed = this.baseSpeed = speed;
        this.capacity = this.baseCapacity = capacity;
        this.shipType = shipType;
        this.icon = icon;
        this.requiredOre = ore;
        this.requiredOil = oil;
        this.requiredAsterminium = asterminium;
        this.requiredForest = forest;
        this.requiredStations = stations;
    }

    public virtual bool CanConstruct(int stations, Structure structure)
    {
        return unlocked && stations >= requiredStations &&
            structure.Resources[Resource.Ore] >= requiredOre &&
            structure.Resources[Resource.Oil] >= requiredOil &&
            structure.Resources[Resource.Asterminium] >= requiredAsterminium &&
            structure.Resources[Resource.Forest] >= requiredForest;
    }

    public virtual Ship Copy()
    {
        var ship = new Ship(icon, name, baseHull, baseFirepower, baseSpeed, baseCapacity, shipType, 
            requiredOre, requiredOil, requiredAsterminium, requiredForest, requiredStations);
        ship.Hull = hull;
        ship.Firepower = firepower;
        ship.Speed = speed;
        ship.Capacity = capacity;
        ship.Protection = protection;

        return ship;
    }
}
