﻿using UnityEngine;
using System.Collections.Generic;

// This class upgrades various concepts but also acts as the base for the Resource Transport, thus is a military script.
public class EfficiencyResearch : Research
{
    private const string GATHERING = "Gathering";
    private const string RESEARCH = "Research";
    private const string MILITARY = "Military";
    private const string RESOURCE_TRANSPORT = "Resource Transport";

    private Dictionary<string, Ship> shipDefinitions;

    public EfficiencyResearch(Dictionary<string, Ship> shipDefinitions) : base("Efficiency", 2)
    {
        this.shipDefinitions = shipDefinitions;
        upgrades.Add(GATHERING, 0);
        upgrades.Add(RESEARCH, 0);
        upgrades.Add(MILITARY, 0);
        upgrades.Add(RESOURCE_TRANSPORT, 0);
    }

    public override void UpgradeResearch(string name)
    {
        switch(name)
        {
            case GATHERING:
                UpgradeGathering();
                break;
            case RESEARCH:
                UpgradeResearch();
                break;
            case MILITARY:
                UpgradeMilitary();
                break;
            case RESOURCE_TRANSPORT:
                UpgradeResourceTransport();
                break;
        }
    }

    private void UpgradeGathering()
    {
        upgrades[GATHERING]++;
    }

    private void UpgradeResearch()
    {
        upgrades[RESEARCH]++;
    }

    private void UpgradeMilitary()
    {
        List<string> militaryShipNames = new List<string> 
        { 
            "Fighter", 
            "Transport", 
            "Guard Satellite", 
            "Heavy Fighter", 
            "Behemoth" 
        };

        var oldBonus = upgrades[MILITARY] * 0.01f;
        upgrades[MILITARY]++;
        var newBonus = upgrades[MILITARY] * 0.01f;

        // not sure if these are being calculated correctly
        foreach(var ship in militaryShipNames)
        {
            // undo previous
            shipDefinitions[ship].Hull += newBonus - oldBonus;
            shipDefinitions[ship].Firepower += newBonus - oldBonus;
            shipDefinitions[ship].Speed += newBonus - oldBonus;
            // shipDefinitions[ship].Capacity += newBonus - oldBonus;
        }
    }

    private void UpgradeResourceTransport()
    {
        upgrades[RESOURCE_TRANSPORT]++;
        shipDefinitions[RESOURCE_TRANSPORT].Hull += 5;
        shipDefinitions[RESOURCE_TRANSPORT].Capacity += 0;
    }

    public override bool Unlock()
    {
        shipDefinitions["Resource Transport"].Unlocked = true;
        return shipDefinitions["Resource Transport"].Unlocked;
    }

    public override void Display(GameObject panel, int stations)
    {
        
    }
}
