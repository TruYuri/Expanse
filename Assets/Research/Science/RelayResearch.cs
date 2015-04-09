﻿using UnityEngine;
using System.Collections.Generic;

// This class upgrades various concepts but also acts as the base for the Resource Transport, thus is a military script.
public class RelayResearch : Research
{
    private const string RANGE = "Range";
    private const string DEFENSE = "Defense";

    private Structure relay;

    public RelayResearch(Structure relay) : base(relay.Name, 3)
    {
        this.relay = relay;
        upgrades.Add(RANGE, 0);
        upgrades.Add(DEFENSE, 0);
    }

    public override void UpgradeResearch(string name)
    {
        switch(name)
        {
            case RANGE:
                UpgradeRange();
                break;
            case DEFENSE:
                UpgradeDefense();
                break;
        }
    }

    private void UpgradeRange()
    {
        upgrades[RANGE]++;
        relay.Range += 1;
    }

    private void UpgradeDefense()
    {
        upgrades[DEFENSE]++;
        relay.Hull += 5.0f;
    }

    public override bool Unlock()
    {
        relay.Unlocked = true;
        return relay.Unlocked;
    }

    public override void Display(GameObject panel, int stations)
    {
        
    }
}
