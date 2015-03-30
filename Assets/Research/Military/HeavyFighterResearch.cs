﻿using UnityEngine;
using System.Collections;

public class HeavyFighterResearch : Research
{
    private const string ARMOR = "Armor";
    private const string PLATING = "Asterminium Plating";
    private const string PLASMAS = "Plasmas";
    private const string TORPEDOES = "Torpedoes";
    private const string THRUSTERS = "Thrusters";
    private const string CAPACITY = "Capacity";

    private Ship heavyFighterShip;

    public HeavyFighterResearch(Ship ship) : base(ship.Name, 4)
    {
        this.heavyFighterShip = ship;
        upgrades.Add(ARMOR, 0);
        upgrades.Add(PLATING, 0);
        upgrades.Add(PLASMAS, 0);
        upgrades.Add(TORPEDOES, 0);
        upgrades.Add(THRUSTERS, 0);
        upgrades.Add(CAPACITY, 0);
    }

    public override bool UpgradeResearch(string name, int stations)
    {
        var meetsCriteria = base.UpgradeResearch(name, stations);

        if (!meetsCriteria)
            return false;

        switch (name)
        {
            case ARMOR:
                UpgradeArmor();
                break;
            case PLATING:
                return UpgradePlating();
            case PLASMAS:
                UpgradePlasmas();
                break;
            case TORPEDOES:
                return UpgradeTorpedoes();
            case THRUSTERS:
                UpgradeThrusters();
                break;
            case CAPACITY:
                UpgradeCapacity();
                break;
        }

        return true;
    }

    private void UpgradeArmor()
    {
        upgrades[ARMOR]++;
        heavyFighterShip.Hull += 0.5f;
    }

    private bool UpgradePlating()
    {
        if (upgrades[ARMOR] < 5)
            return false;

        upgrades[PLATING]++;
        heavyFighterShip.Protection = upgrades[PLATING] * 0.02f;
        return true;
    }

    private void UpgradePlasmas()
    {
        upgrades[PLASMAS]++;
        heavyFighterShip.Firepower += 0.75f;
    }

    private bool UpgradeTorpedoes()
    {
        if (upgrades[PLASMAS] < 5)
            return false;

        heavyFighterShip.Firepower -= upgrades[TORPEDOES] * 0.02f;
        upgrades[TORPEDOES]++;
        heavyFighterShip.Firepower += upgrades[TORPEDOES] * 0.02f;
        return true;
    }

    private void UpgradeThrusters()
    {
        upgrades[THRUSTERS]++;
        heavyFighterShip.Speed += 0.5f;
    }

    private void UpgradeCapacity()
    {
        upgrades[CAPACITY]++;
        heavyFighterShip.Capacity += 10;
    }

    public override bool Unlock()
    {
        heavyFighterShip.Unlocked = true;
        return heavyFighterShip.Unlocked;
    }

    public override void Display()
    {
    }
}
