﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BehemothResearch : Research
{
    private const string ARMOR = "Armor";
    private const string PLATING = "Asterminium Plating";
    private const string PLASMAS = "Plasmas";
    private const string TORPEDOES = "Torpedoes";
    private const string THRUSTERS = "Thrusters";
    private const string CAPACITY = "Capacity";

    private Ship behemothShip;

    public BehemothResearch(Ship ship, List<Research> prereqs)
        : base(ship.Name, 5, prereqs)
    {
        this.behemothShip = ship;
        upgrades.Add(ARMOR, 0);
        upgrades.Add(PLATING, 0);
        upgrades.Add(PLASMAS, 0);
        upgrades.Add(TORPEDOES, 0);
        upgrades.Add(THRUSTERS, 0);
        upgrades.Add(CAPACITY, 0);

        foreach (var upgrade in upgrades)
        {
            costs.Add(upgrade.Key, new Dictionary<Resource, int>()
                { 
                    { Resource.Asterminium, 0 },
                    { Resource.Forest, 0 },
                    { Resource.Oil, 0 },
                    { Resource.Ore, 0 },
                    { Resource.Stations, 0 }
                });
        }

        RecalculateResourceCosts();
    }

    public override Dictionary<Resource, int> UpgradeResearch(string name)
    {
        switch (name)
        {
            case ARMOR:
                upgrades[ARMOR]++;
                behemothShip.Hull += 3.0f;
                break;
            case PLATING:
                upgrades[PLATING]++;
                behemothShip.Protection = upgrades[PLATING] * 0.02f;
                behemothShip.Plating++;
                break;
            case PLASMAS:
                upgrades[PLASMAS]++;
                behemothShip.Firepower += 2.0f;
                break;
            case TORPEDOES:
                behemothShip.Firepower -= upgrades[TORPEDOES] * 0.02f;
                upgrades[TORPEDOES]++;
                behemothShip.Firepower += upgrades[TORPEDOES] * 0.02f;
                break;
            case THRUSTERS:
                upgrades[THRUSTERS]++;
                behemothShip.Speed += 0.25f;
                break;
            case CAPACITY:
                upgrades[CAPACITY]++;
                behemothShip.Capacity += 50;
                break;
        }

        behemothShip.RecalculateResources();
        var r = costs[name];
        RecalculateResourceCosts();
        return r;
    }

    private void RecalculateResourceCosts()
    {
        costs[ARMOR] = new Dictionary<Resource, int>()
        {
            { Resource.Ore, Mathf.CeilToInt((upgrades[ARMOR] + 1) * 3 * behemothShip.Hull) }
        };

        costs[PLATING] = new Dictionary<Resource, int>()
        {
            { Resource.Asterminium, Mathf.CeilToInt((upgrades[PLATING] + 1) * behemothShip.Hull) }
        };

        costs[PLASMAS] = new Dictionary<Resource, int>()
        {
            { Resource.Ore, Mathf.CeilToInt((upgrades[PLASMAS] + 1) * 2 * behemothShip.Firepower) },
            { Resource.Oil, Mathf.CeilToInt((upgrades[PLASMAS] + 1) * 2 * behemothShip.Firepower) }
        };

        costs[TORPEDOES] = new Dictionary<Resource, int>()
        {
            { Resource.Asterminium, Mathf.CeilToInt((upgrades[TORPEDOES] + 1) * behemothShip.Firepower) }
        };

        costs[THRUSTERS] = new Dictionary<Resource, int>()
        {
            { Resource.Ore, Mathf.CeilToInt((upgrades[THRUSTERS] + 1) * 0.25f * behemothShip.Speed * 10f) },
            { Resource.Oil, Mathf.CeilToInt((upgrades[THRUSTERS] + 1) * 0.25f * behemothShip.Speed * 10f) }
        };

        costs[CAPACITY] = new Dictionary<Resource, int>()
        {
            { Resource.Ore, Mathf.CeilToInt((upgrades[CAPACITY] + 1) * 50f * behemothShip.Hull / 2.0f) },
            { Resource.Forest, Mathf.CeilToInt((upgrades[CAPACITY] + 1) * 50f * behemothShip.Hull / 2.0f) }
        };
    }

    public override Dictionary<Resource, int> Unlock()
    {
        base.Unlock();
        behemothShip.Unlocked = true;
        return new Dictionary<Resource, int>();
    }

    public override bool CanUnlock(Dictionary<Resource, int> resources)
    {
        if (unlocked || behemothShip.Unlocked)
            return true;

        bool unlock = true;

        foreach (var p in prereqs)
            unlock = unlock && p.Unlocked;
        unlock = unlock && behemothShip.CanConstruct(resources, 5);

        return unlock;
    }

    public override void Display(GameObject panel, Dictionary<Resource, int> resources)
    {
        var items = new Dictionary<string, Transform>()
        {
            { TORPEDOES, panel.transform.FindChild("BehemothTorpedoesButton") },
            { THRUSTERS, panel.transform.FindChild("BehemothThrustersButton") },
            { CAPACITY, panel.transform.FindChild("BehemothCapacityButton") },
            { ARMOR, panel.transform.FindChild("BehemothArmorButton") },
            { PLATING, panel.transform.FindChild("BehemothAsterminiumButton") },
            { PLASMAS, panel.transform.FindChild("BehemothPlasmasButton") }
        };

        var p2 = panel.transform.FindChild("BehemothUnlocked");
        var p1 = panel.transform.FindChild("Behemoth");

        p1.gameObject.SetActive(false);
        p2.gameObject.SetActive(false);

        if (unlocked)
        {
            p2.gameObject.SetActive(true);
            p2.FindChild("StatsSpeedText").GetComponent<Text>().text = "Speed: " + behemothShip.Speed.ToString();
            p2.FindChild("StatsFirepowerText").GetComponent<Text>().text = "Firepower: " + behemothShip.Firepower.ToString();
            p2.FindChild("StatsHullText").GetComponent<Text>().text = "Hull: " + behemothShip.Hull.ToString();
            p2.FindChild("StatsCapacityText").GetComponent<Text>().text = "Capacity: " + behemothShip.Capacity.ToString();
        }
        else
        {
            p1.gameObject.SetActive(true);
            p1.GetComponentInChildren<Button>().interactable = CanUnlock(resources);
        }

        foreach(var item in items)
        {
            item.Value.FindChild("CountText").GetComponent<Text>().text = upgrades[item.Key].ToString() + "/10";
            if (CanUpgrade(item.Key, resources[Resource.Stations]) && unlocked)
                item.Value.GetComponent<Button>().interactable = true;
            else
                item.Value.GetComponent<Button>().interactable = false;
        }

        if (upgrades[ARMOR] < 5)
            items[PLATING].GetComponent<Button>().interactable = false;

        if (upgrades[PLASMAS] < 5)
            items[TORPEDOES].GetComponent<Button>().interactable = false;
    }
}