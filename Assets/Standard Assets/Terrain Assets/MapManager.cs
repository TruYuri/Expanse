﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    public static GameObject Tile;
    public static GameObject Sector;

    private static Vector3 TOP_RIGHT_OFFSET = new Vector3(98.3f, 0.0f, 149.0f);
    private static Vector3 RIGHT_OFFSET = new Vector3(196.5f, 0, 0.0f);
    private static Vector3 BOTTOM_RIGHT_OFFSET = new Vector3(98.3f, 0.0f, -149.0f);
    private static Vector3 BOTTOM_LEFT_OFFSET = new Vector3(-98.3f, 0.0f, -149.0f);
    private static Vector3 LEFT_OFFSET = new Vector3(-196.5f, 0, 0);
    private static Vector3 TOP_LEFT_OFFSET = new Vector3(-98.3f, 0.0f, 149.0f);

    private static MapManager instance;
    private List<GameObject> Sectors;

    public static MapManager Instance
    {
        get 
        {
            if (instance == null)
            {
                var obj = Instantiate(Resources.Load<GameObject>("MapManager"), Vector3.zero, Quaternion.identity) as GameObject;
                instance = obj.GetComponent<MapManager>();
            }

            return instance;
        }
    }

	// Use this for initialization
	public void Start()
    {
        Sectors = new List<GameObject>();
        var sector = Resources.Load<GameObject>("Sector");
        Tile = Resources.Load<GameObject>("Tile");
        Sector = Instantiate(sector, Vector3.zero, Quaternion.identity) as GameObject;
        instance = this;
        Sectors.Add(Sector);
	}

    public void GenerateNewSectors(Sector origin)
    {
        var position = Vector3.zero;

        // Generate any needed immediate neighbors and link them
        if (origin.TopRight == null)
        {
            position = origin.transform.position + TOP_RIGHT_OFFSET;

            origin.TopRight = Instantiate(Sector, position, Quaternion.identity) as GameObject;
            origin.TopRight.GetComponent<Sector>().BottomLeft = origin.gameObject;

            Sectors.Add(origin.TopRight);
        }

        if (origin.Right == null)
        {
            position = origin.transform.position + RIGHT_OFFSET;

            origin.Right = Instantiate(Sector, position, Quaternion.identity) as GameObject;
            origin.Right.GetComponent<Sector>().Left = origin.gameObject;

            Sectors.Add(origin.Right);
        }

        if (origin.BottomRight == null)
        {
            position = origin.transform.position + BOTTOM_RIGHT_OFFSET;

            origin.BottomRight = Instantiate(Sector, position, Quaternion.identity) as GameObject;
            origin.BottomRight.GetComponent<Sector>().TopLeft = origin.gameObject;

            Sectors.Add(origin.BottomRight);
        }

        if (origin.BottomLeft == null)
        {
            position = origin.transform.position + BOTTOM_LEFT_OFFSET;

            origin.BottomLeft = Instantiate(Sector, position, Quaternion.identity) as GameObject;
            origin.BottomLeft.GetComponent<Sector>().TopRight = origin.gameObject;

            Sectors.Add(origin.BottomLeft);
        }

        if (origin.Left == null)
        {
            position = origin.transform.position + LEFT_OFFSET;

            origin.Left = Instantiate(Sector, position, Quaternion.identity) as GameObject;
            origin.Left.GetComponent<Sector>().Right = origin.gameObject;

            Sectors.Add(origin.Left);
        }

        if (origin.TopLeft == null)
        {
            position = origin.transform.position + TOP_LEFT_OFFSET;

            origin.TopLeft = Instantiate(Sector, position, Quaternion.identity) as GameObject;
            origin.TopLeft.GetComponent<Sector>().BottomRight = origin.gameObject;

            Sectors.Add(origin.TopLeft);
        }

        ResolveLooseConnections();
    }

    private void ResolveLooseConnections()
    {
        // resolve broken links
        foreach (var sector in Sectors)
        {
            var component = sector.GetComponent<Sector>();

            if (component.TopRight == null)
                component.TopRight = FindSectorAtPosition(sector.transform.position + TOP_RIGHT_OFFSET);
            if (component.Right == null)
                component.Right = FindSectorAtPosition(sector.transform.position + RIGHT_OFFSET);
            if (component.BottomRight == null)
                component.BottomRight = FindSectorAtPosition(sector.transform.position + BOTTOM_RIGHT_OFFSET);
            if (component.BottomLeft == null)
                component.BottomLeft = FindSectorAtPosition(sector.transform.position + BOTTOM_LEFT_OFFSET);
            if (component.Left == null)
                component.Left = FindSectorAtPosition(sector.transform.position + LEFT_OFFSET);
            if (component.TopLeft == null)
                component.TopLeft = FindSectorAtPosition(sector.transform.position + TOP_LEFT_OFFSET);
        }
    }
	
    private GameObject FindSectorAtPosition(Vector3 position)
    {
        foreach(var sector in Sectors)
        {
            if(Vector3.Distance(sector.transform.position, position) <= 1.0f)
                return sector;
        }

        return null;
    }
}
