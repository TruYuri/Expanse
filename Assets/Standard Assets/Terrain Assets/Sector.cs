﻿using UnityEngine;
using System.Collections.Generic;

[System.Serializable] 
public class Sector : MonoBehaviour
{

    List<GameObject> Tiles;

	// Use this for initialization
	void Start () 
    {
        Tiles = new List<GameObject>();
        var baseTile = Resources.Load<GameObject>("Tile");
        
        // Generate center columns
        for(int i = -75; i <= 75; i += 10)
        {
            Vector3 position = this.transform.position + new Vector3(i, 0, -5);
            Tiles.Add(Instantiate(baseTile, position, Quaternion.identity) as GameObject);

            position = this.transform.position + new Vector3(i, 0, 5);
            Tiles.Add(Instantiate(baseTile, position, Quaternion.identity) as GameObject);
        }

        // Generate middle columns
        for (int n = 0; n < 3; n++)
        {
            int shift = 2 * n * 10;
            for (int i = -65 + n * 10; i <= 65 - n * 10; i += 10)
            {
                Vector3 position = this.transform.position + new Vector3(i, 0, -15 - shift);
                Tiles.Add(Instantiate(baseTile, position, Quaternion.identity) as GameObject);

                position = this.transform.position + new Vector3(i, 0, -25 - shift);
                Tiles.Add(Instantiate(baseTile, position, Quaternion.identity) as GameObject);

                position = this.transform.position + new Vector3(i, 0, 15 + shift);
                Tiles.Add(Instantiate(baseTile, position, Quaternion.identity) as GameObject);

                position = this.transform.position + new Vector3(i, 0, 25 + shift);
                Tiles.Add(Instantiate(baseTile, position, Quaternion.identity) as GameObject);
            }
        }

        // Generate outermost columns
        for (int i = -35; i <= 35; i += 10)
        {
            Vector3 position = this.transform.position + new Vector3(i, 0, -75);
            Tiles.Add(Instantiate(baseTile, position, Quaternion.identity) as GameObject);

            position = this.transform.position + new Vector3(i, 0, 75);
            Tiles.Add(Instantiate(baseTile, position, Quaternion.identity) as GameObject);
        }

        // DEBUG
        System.Random r = new System.Random();
        foreach(var tile in Tiles)
        {
            tile.renderer.material.color = new Color((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble(), 0.0f);
        }
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}
}
