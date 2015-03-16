﻿using UnityEngine;
using System.Collections;

public class CommandShipResearch : Research
{
    private const string ARMOR = "Armor";
    private const string PLATING = "Asterminium Plating";
    private const string PLASMAS = "Plasmas";
    private const string TORPEDOES = "Torpedoes";
    private const string THRUSTERS = "Thrusters";

    Ship commandShip;

    public CommandShipResearch(Ship ship) : base(ship.Name, 1)
    {
        this.commandShip = ship;
        upgrades.Add(ARMOR, 0);
        upgrades.Add(PLATING, 0);
        upgrades.Add(PLASMAS, 0);
        upgrades.Add(TORPEDOES, 0);
        upgrades.Add(THRUSTERS, 0);
    }

    public override void UpgradeResearch(string name, int stations)
    {
        base.UpgradeResearch(name, stations);

        switch (name)
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
            case TORPEDOES:
                UpgradeTorpedoes();
                break;
            case THRUSTERS:
                UpgradeThrusters();
                break;
        }
    }

    private void UpgradeArmor()
    {
        upgrades[ARMOR]++;
        commandShip.Hull += 3.0f;
    }

    private void UpgradePlating()
    {
        if (upgrades[ARMOR] < 5)
            return;

        upgrades[PLATING]++;
        commandShip.Protection = upgrades[PLATING] * 0.02f;
    }

    private void UpgradePlasmas()
    {
        upgrades[PLASMAS]++;
        commandShip.Firepower += 2.0f;
    }

    private void UpgradeTorpedoes()
    {
        if (upgrades[PLASMAS] < 5)
            return;

        commandShip.Firepower -= upgrades[TORPEDOES] * 0.02f;
        upgrades[TORPEDOES]++;
        commandShip.Firepower += upgrades[TORPEDOES] * 0.02f;
    }

    private void UpgradeThrusters()
    {
        upgrades[THRUSTERS]++;
        commandShip.Speed += 2.0f;
    }

    public override bool Unlock()
    {
        return true;
    }

    public override void Display()
    {
    }
}