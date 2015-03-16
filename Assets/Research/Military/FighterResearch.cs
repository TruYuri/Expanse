﻿using UnityEngine;
using System.Collections;

public class FighterResearch : Research
{
    private const string ARMOR = "Armor";
    private const string PLATING = "Asterminium Plating";
    private const string PLASMAS = "Plasmas";
    private const string THRUSTERS = "Thrusters";

    private Ship fighterShip;

    public FighterResearch(Ship ship) : base(ship.Name, 1)
    {
        this.fighterShip = ship;
        upgrades.Add(ARMOR, 0);
        upgrades.Add(PLATING, 0);
        upgrades.Add(PLASMAS, 0);
        upgrades.Add(THRUSTERS, 0);
    }

    public override void UpgradeResearch(string name, int stations) 
    {
        base.UpgradeResearch(name, stations);

        switch(name)
        {
            case ARMOR:
                UpgradeArmor();
                break;
            case PLATING:
                UpgradePlating();
                break;
            case PLASMAS:
                UpgradePlasmas();
                break;
            case THRUSTERS:
                UpgradeThrusters();
                break;
        }
    }

    private void UpgradeArmor()
    {
        upgrades[ARMOR]++;
        fighterShip.Hull += 0.25f;
    }

    private void UpgradePlating()
    {
        if (upgrades[ARMOR] < 5)
            return;

        upgrades[PLATING]++;
        fighterShip.Protection = upgrades[PLATING] * 0.02f;
    }

    private void UpgradePlasmas()
    {
        upgrades[PLASMAS]++;
        fighterShip.Firepower += 0.25f;
    }

    private void UpgradeThrusters()
    {
        upgrades[THRUSTERS]++;
        fighterShip.Speed += 1.0f;
    }

    public override bool Unlock() 
    { 
        return true;
    }

    public override void Display() 
    {
    }
}