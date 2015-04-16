﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GuardSatelliteResearch : Research
{
    private const string ARMOR = "Armor";
    private const string PLATING = "Asterminium Plating";
    private const string PLASMAS = "Plasmas";
    private const string TORPEDOES = "Torpedoes";

    private Ship guardSatelliteShip;

    public GuardSatelliteResearch(Ship ship, List<Research> prereqs) 
        : base(ship.Name, 3, prereqs)
    {
        this.guardSatelliteShip = ship;
        upgrades.Add(ARMOR, 0);
        upgrades.Add(PLATING, 0);
        upgrades.Add(PLASMAS, 0);
        upgrades.Add(TORPEDOES, 0);
    }

    public override void UpgradeResearch(string name, Dictionary<Resource, int> resources)
    {
        switch (name)
        {
            case ARMOR:
                upgrades[ARMOR]++;
                guardSatelliteShip.Hull += 0.5f;
                break;
            case PLATING:
                upgrades[PLATING]++;
                guardSatelliteShip.Protection = upgrades[PLATING] * 0.02f;
                guardSatelliteShip.Plating++;
                break;
            case PLASMAS:
                upgrades[PLASMAS]++;
                guardSatelliteShip.Firepower += 1.0f;
                break;
            case TORPEDOES:
                guardSatelliteShip.Firepower -= upgrades[TORPEDOES] * 0.02f;
                upgrades[TORPEDOES]++;
                guardSatelliteShip.Firepower += upgrades[TORPEDOES] * 0.02f;
                break;
        }

        guardSatelliteShip.RecalculateResources();
    }

    public override void Unlock()
    {
        base.Unlock();
        guardSatelliteShip.Unlocked = true;
    }

    public override bool CanUnlock(Dictionary<Resource, int> resources)
    {
        if (unlocked || guardSatelliteShip.Unlocked)
            return true;

        bool unlock = true;

        foreach (var p in prereqs)
            unlock = unlock && p.Unlocked;
        unlock = unlock && guardSatelliteShip.CanConstruct(resources, 5);

        return unlock;
    }

    public override void Display(GameObject panel, Dictionary<Resource, int> resources)
    {
        var items = new Dictionary<string, Transform>()
        {
            { TORPEDOES, panel.transform.FindChild("GuardTorpedoesButton") },
            { ARMOR, panel.transform.FindChild("GuardArmorButton") },
            { PLATING, panel.transform.FindChild("GuardAsterminiumButton") },
            { PLASMAS, panel.transform.FindChild("GuardPlasmasButton") }
        };

        var p2 = panel.transform.FindChild("GuardSatelliteUnlocked");
        var p1 = panel.transform.FindChild("GuardSatellite");

        p1.gameObject.SetActive(false);
        p2.gameObject.SetActive(false);

        if (unlocked)
        {
            p2.gameObject.SetActive(true);
            p2.FindChild("StatsSpeedText").GetComponent<Text>().text = "Speed: " + guardSatelliteShip.Speed.ToString();
            p2.FindChild("StatsFirepowerText").GetComponent<Text>().text = "Firepower: " + guardSatelliteShip.Firepower.ToString();
            p2.FindChild("StatsHullText").GetComponent<Text>().text = "Hull: " + guardSatelliteShip.Hull.ToString();
            p2.FindChild("StatsCapacityText").GetComponent<Text>().text = "Capacity: " + guardSatelliteShip.Capacity.ToString();
        }
        else
        {
            p1.gameObject.SetActive(true);
            p1.GetComponentInChildren<Button>().interactable = CanUnlock(resources);
        }

        foreach (var item in items)
        {
            item.Value.FindChild("CountText").GetComponent<Text>().text = upgrades[item.Key].ToString() + "/10";
            if (CanUpgrade(item.Key, resources[Resource.Stations]) && CanUnlock(resources))
                item.Value.GetComponent<Button>().interactable = true;
            else
                item.Value.GetComponent<Button>().interactable = false;
        }

        if (upgrades[ARMOR] < 5)
            items[PLATING].GetComponent<Button>().interactable = false;
    }
}
