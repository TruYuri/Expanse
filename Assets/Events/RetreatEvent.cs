﻿using UnityEngine;
using System.Collections.Generic;

public class RetreatEvent : GameEvent 
{
    private readonly Vector3 TOP_RIGHT_OFFSET = new Vector3(98.3f, 0.0f, 149.0f);
    private readonly Vector3 RIGHT_OFFSET = new Vector3(196.5f, 0, 0.0f);
    private readonly Vector3 BOTTOM_RIGHT_OFFSET = new Vector3(98.3f, 0.0f, -149.0f);
    private readonly Vector3 BOTTOM_LEFT_OFFSET = new Vector3(-98.3f, 0.0f, -149.0f);
    private readonly Vector3 LEFT_OFFSET = new Vector3(-196.5f, 0, 0);
    private readonly Vector3 TOP_LEFT_OFFSET = new Vector3(-98.3f, 0.0f, 149.0f);

    private Squad _squad;

    // turn parameter = turns until command begins. 
    // calculate travel turns - 1 turn per sector, swap out remaining turns when initial == 0
    public RetreatEvent(Squad squad) : base(1)
    {
        _squad = squad;
        _squad.Mission = this;
    }

    // when travelling, travel between planets (x = 10x, y = 10y)
    // travel from one waypoint to the next

    public override void Progress()
    {
        base.Progress();

        var closest = MapManager.Instance.FindNearestSector(_squad.Sector, _squad.transform.position);
        var pos = (closest.transform.position + _squad.Sector.transform.position) / 2.0f;

        _squad.transform.position = pos;
        _squad.Mission = null;
    }

    public override void Update()
    {
    }

    public override bool AssertValid()
    {
        if (_squad != null && _squad.gameObject != null && _squad.Mission == this)
            return true;
        return false;
    }
}
