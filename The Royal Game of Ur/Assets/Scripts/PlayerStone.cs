using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStone : MonoBehaviour {

	// Use this for initialization
	void Start () {
        theStateManager = GameObject.FindObjectOfType<StateManager>();

        targetPosition = this.transform.position;
    }

    public Tile StartingTile;
    public Tile CurrentTile { get; protected set; }

    public int PlayerId;
    public StoneStorage MyStoneStorage;

    bool scoreMe = false;
    StateManager theStateManager;

    Tile[] moveQueue;
    int moveQueueIndex;
    bool isAnimating = false;

    Vector3 targetPosition;
    Vector3 velocity = Vector3.zero;
    float smoothTime = 0.25f;
    float smoothTimeVertical = 0.1f;
    float smoothDistance = 0.01f;
    float smoothHeight = 0.5f;

    PlayerStone stoneToBop;
    // Update is called once per frame
    void Update()
    {
        if(isAnimating == false)
        {
            return;
        }
        if (Vector3.Distance(new Vector3(this.transform.position.x, targetPosition.y, this.transform.position.z), targetPosition) < smoothDistance)
        {
            //we have reached the target. do we still have moves in the queue?
            if((moveQueue == null || moveQueueIndex ==(moveQueue.Length)) 
                && (this.transform.position.y-smoothDistance) > targetPosition.y)
            {
                //we are above our target

                
                    //we are totally out of moves, only thing to do is drop down.
                this.transform.position = Vector3.SmoothDamp(this.transform.position,
                new Vector3(this.transform.position.x, targetPosition.y, this.transform.position.z), ref velocity, smoothTimeVertical);
                //check for bops
                if (stoneToBop != null)
                {
                    stoneToBop.ReturnToStorage();
                    stoneToBop = null;
                }

            }


            else
            {
                //right pos, heigh -- advance queue
                AdvanceMoveQueue();
            }

        }
        


        // we want to rise up before we move sideways
        else if (this.transform.position.y < (smoothHeight - smoothDistance))
        {
            this.transform.position = Vector3.SmoothDamp(this.transform.position,
                new Vector3(this.transform.position.x, smoothHeight, this.transform.position.z), ref velocity, smoothTimeVertical);

        }
        else
        {
            this.transform.position = Vector3.SmoothDamp(this.transform.position, 
                new Vector3(targetPosition.x, smoothHeight, targetPosition.z), ref velocity, smoothTime);
        }
    }


    void AdvanceMoveQueue()
    {
        if (moveQueue != null && moveQueueIndex<moveQueue.Length)
            {
                Tile nextTile = moveQueue[moveQueueIndex];
                if(nextTile == null)
                {
                    //we are probably being scored
                    //todo move toscore pile
                    SetNewTargetPosition(this.transform.position + Vector3.right* 10f);
}
                else
                {
                    SetNewTargetPosition(nextTile.transform.position);
                    moveQueueIndex++;
                }
               
            }
        else
        {
            Debug.Log("Done animating");
            this.isAnimating = false;
            theStateManager.AnimationsPlaying --;

            //are we on a roll again space?
            if(CurrentTile != null && CurrentTile.IsRollAgain)
            {
                theStateManager.RollAgain();
            }
        }
    }
    void SetNewTargetPosition(Vector3 pos)
    {
        targetPosition = pos;
        velocity = Vector3.zero;
        isAnimating = true;
    }

    void OnMouseUp()
    {
        Debug.Log("Clicked");
        MoveMe();
    }



    public void MoveMe()
    {
        

        //is this the correct plauer?
        if(theStateManager.CurrentPlayerId != PlayerId)
        {
            return;
        }

        if(theStateManager.IsDoneRolling == false)
        {
            //we can't move yet
            return;
        }
        if(theStateManager.IsDoneClicking == true)
        {
            //we've already done move
            return;
        }
        int spacesToMove = theStateManager.DiceTotal;

        if(spacesToMove==0)
        {
            return;
        }

        //where should we end up?

        //if(spacesToMove ==0)
        //{
        //    return;
        //}
        
        moveQueue = GetTilesAhead(spacesToMove);

        Tile finalTile = moveQueue[moveQueue.Length - 1];
        if(finalTile == null)
        {
            //Hey, we're scoring this stone
            scoreMe = true;
        }
        if(finalTile !=null)
        {
            if(CanLegallyMoveTo(finalTile) == false)
            {
                //not allowed
                finalTile = CurrentTile;
                moveQueue = null;
                return;
            }
            //If there is an enemy tile in our legal space, then we kick it out.
            if(finalTile.PlayerStone != null)
            {
                //finalTile.PlayerStone.ReturnToStorage();
                stoneToBop = finalTile.PlayerStone;
                stoneToBop.CurrentTile.PlayerStone = null;
                stoneToBop.CurrentTile = null;

            }

        }

        this.transform.SetParent(null);//we have no parent

        if(CurrentTile !=null)
        {
            CurrentTile.PlayerStone = null;

        }
        CurrentTile = finalTile;

        if (finalTile.IsScoringSpace == false)// Scoring tiles are always empty
        {
            
            finalTile.PlayerStone = this;

        }

        //Even before the animation is done, set our current tile to  the new tile

        
        moveQueueIndex = 0;

        theStateManager.IsDoneClicking = true;
        this.isAnimating = true;
        theStateManager.AnimationsPlaying++;


    }

    public Tile[] GetTilesAhead(int spacesToMove)
    {
        if (spacesToMove == 0)
        {
            return null;
        }

        //where should we end up?

     
        Tile[] listOfTiles = new Tile[spacesToMove];
        Tile finalTile = CurrentTile;
        for (int i = 0; i < spacesToMove; i++)
        {
            if (finalTile == null )
            {
                finalTile = StartingTile;
            }
            else
            {
                if (finalTile.NextTiles == null || finalTile.NextTiles.Length == 0)
                {
                    //this means we are overshooting the victory so jusyt return some nulls in the array
                    //finalTile = null;
                    //just break and we'll return the array
                    break;
                }
                else if (finalTile.NextTiles.Length > 1)
                {
                    //branch based on player id
                    finalTile = finalTile.NextTiles[PlayerId];
                }
                else
                {
                    finalTile = finalTile.NextTiles[0];

                }
            }
            listOfTiles[i] = finalTile;
        }
        return listOfTiles;
    }

    public Tile GetTileAhead()
    {
        return GetTileAhead(theStateManager.DiceTotal);
    }




    public Tile GetTileAhead(int spacesToMove)
    {
        Tile[] tiles = GetTilesAhead(spacesToMove);

        if(tiles == null)
        {
            //we arent moving at all 
            return CurrentTile;
        }
        return tiles[tiles.Length -1];
    }
    public bool CanLegallyMoveAhead(int spacesToMove)
    {
        Tile theTile = GetTileAhead(spacesToMove);

        

        return CanLegallyMoveTo(theTile);
    }


    bool CanLegallyMoveTo( Tile destinationTile)
    {
        //Debug.Log("CanLegallyMoveTo: " + destinationTile);
        if(destinationTile == null)
        {
            //NOTE A null tile means we are overshooting the victory roll
            //and this is not legal in the royal game of ur
            return false;


            //we are trying to move off boarda and score
            //Debug.Log(" we are trying to move off boarda and score");
            //return true;
        }
        //is tile empty?
        if(destinationTile.PlayerStone == null)
        {
            return true;
        }
        //is it our stone?

        if(destinationTile.PlayerStone.PlayerId == this.PlayerId)
        {
            //we cant land on our own stone
            return false;
        
        }
        if(destinationTile.IsRollAgain==true)
        {
            //cant bop someone on a safe tile
            return false;
        }
        //if its enemy's.. is it in a safe square
        //if we've gotten here it means we can legally land on enemy stone and kic it off board
        return true;
    }

    public void ReturnToStorage()
    {
        Debug.Log("return to storage");
        //currentTile.PlayerStone = null;
        //currentTile = null;

        this.isAnimating = true;
        theStateManager.AnimationsPlaying++;


        moveQueue = null;

        //save our current position
        Vector3 savePosition = this.transform.position;
        MyStoneStorage.AddStoneToStorage(this.gameObject);

        //set our newposition to animation traget
        SetNewTargetPosition(this.transform.position);

        //restore oure saved position
        this.transform.position = savePosition;
        //TODO: Animate to the storage location
    }
}
