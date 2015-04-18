﻿using UnityEngine;
using System.Collections.Generic;

public class Research
{
    protected string name;
    protected int level;
    protected Dictionary<string, int> upgrades;
    protected List<Research> prereqs;
    protected Dictionary<string, Dictionary<Resource, int>> costs;
    protected bool unlocked;

    public bool Unlocked { get { return unlocked; } }
    public string Name { get { return name; } }

    public Research(string name, int level, List<Research> prereqs)
    {
        this.costs = new Dictionary<string, Dictionary<Resource, int>>();
        this.upgrades = new Dictionary<string, int>();
        this.prereqs = prereqs;
        this.level = level;
        this.name = name;
    }

    protected bool CanUpgrade(string name, Dictionary<Resource, int> resources, float reduction)
    {
        var invalidLevel = upgrades[name] >= 10;
        var invalidStations = resources[Resource.Stations] < (upgrades[name] + 1) * (level);

        if (invalidLevel || invalidStations)
            return false;

        if (resources[Resource.Asterminium] >= costs[name][Resource.Asterminium] * (1.0f - reduction) &&
           resources[Resource.Ore] >= costs[name][Resource.Ore] * (1.0f - reduction) &&
           resources[Resource.Oil] >= costs[name][Resource.Oil] * (1.0f - reduction) &&
           resources[Resource.Forest] >= costs[name][Resource.Forest] * (1.0f - reduction))
            return true;

        return false;
    }

    public virtual Dictionary<Resource, int> UpgradeResearch(string name, float reduction) { return new Dictionary<Resource, int>(); }

    public virtual Dictionary<Resource, int> Unlock(float reduction)
    {
        unlocked = true;
        return new Dictionary<Resource, int>();
    }

    public virtual bool CanUnlock(Dictionary<Resource, int> resources, float reduction)
    {
        return true;
    }

    public virtual void Display(GameObject panel, Dictionary<Resource, int> resources, float reduction) { }
}
