using UnityEngine;
using System.Collections.Generic;
public class AIPlayer_UtilityAI : BasicAI
{
    Dictionary<Tile, float> tileDanger;


    //float aggressivenessBonus = 0.25f;// Gets added for bops, and removed for staying on safe spaces


    override protected PlayerStone PickStoneToMove(PlayerStone[] legalStones)
    {
        Debug.Log("AIPlayer_UtilityAI");


        if(legalStones==null || legalStones.Length ==0)
        {
            Debug.LogError("Why are we being asked to pick from no stones");
            return null;
        }

        CalcTileDanger(legalStones[0].PlayerId);

        //For each stone we rank how good it would be to pick it, where 1 is good -1 is bad
        PlayerStone bestStone = null;
        float goodness = -Mathf.Infinity;

        foreach(PlayerStone ps in legalStones)
        {
            float g = GetStoneGoodness(ps, ps.CurrentTile, ps.GetTileAhead());
            if(bestStone == null || g> goodness)
            {
                bestStone = ps;
                goodness = g;
            }
        }

        Debug.Log("Chosen Stone Goodness: " + goodness);
        return bestStone;
    }

    virtual protected void CalcTileDanger(int myPlayerId)
    {
        tileDanger = new Dictionary<Tile, float>();
        Tile[] tiles = GameObject.FindObjectsOfType<Tile>();

        foreach(Tile t in tiles)
        {
            tileDanger[t] = 0;
        }

        PlayerStone[] allStones = GameObject.FindObjectsOfType<PlayerStone>();
        foreach(PlayerStone stone in allStones)
        {

            if (stone.PlayerId == myPlayerId)
                continue;
            //if this  is an enemy stone,add a "danger" value to tiles in front of it

            for (int i = 1; i <=4 ; i++)
            {
                Tile t = stone.GetTileAhead(i);
                if(t==null)
                {
                    //this tile are invalid so we can just bail
                    break;
                }
                if(t.IsScoringSpace||t.IsSideLine||t.IsRollAgain)
                {
                    //this tile is not a danger zone so wecan ignore it
                    continue;
                }

                //okay this tile is within bopping range of an enemy so its dangerous
                if(i ==2)
                {
                    //2 tiles is most likely so mosot dangerous
                    tileDanger[t] += 0.3f;
                }
                else
                {
                    tileDanger[t]  += 0.2f;

                }



            }

        }
    }



    virtual protected float GetStoneGoodness(PlayerStone stone, Tile currentTile, Tile futureTile)
    {
        float goodness = Random.Range(-0.1f,0.1f);

        if(currentTile == null)
        {
            //we arent on the board yet and its alaways nice to add more to the board
            goodness += 0.2f;
        }

        if(currentTile != null && (currentTile.IsRollAgain == true&& currentTile.IsSideLine==false))
        {
           //we are sitting on a roll again space in the middle. it is better to resist moving
            goodness -= 0.10f;
        }
        if(futureTile.IsRollAgain == true)
        {

            goodness += 0.50f;
        }

        if (futureTile.PlayerStone != null && futureTile.PlayerStone.PlayerId != stone.PlayerId)
        {
            //there's an enemy stone to bop
            goodness += 0.50f;
        }


        if (futureTile.IsScoringSpace == true)
        {
            goodness += 0.50f;
        }

        float currentDanger = 0;
        if(currentTile != null)
        {
            currentDanger = tileDanger[currentTile];
        }
        goodness +=currentDanger- tileDanger[futureTile];
        
        //Add goodness for tiles in our private sidelines
        //TODO: add goodness ofremoving stone forward when we block friendly.

        return goodness;
    }
}
