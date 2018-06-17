using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    public Tile[] NextTiles;
    public PlayerStone PlayerStone;
    public bool IsScoringSpace;
    public bool IsRollAgain;
    public bool IsSideLine;// its part of player's safe side line


	// Update is called once per frame
	void Update () {
		
	}
}
