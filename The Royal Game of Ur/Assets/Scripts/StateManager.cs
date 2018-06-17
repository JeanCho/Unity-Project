using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour {

	// Use this for initialization
	void Start () {

        PlayerAIs = new BasicAI[NumberOfPlayer];

        PlayerAIs[0] = new AIPlayer_UtilityAI(); //is human player
        PlayerAIs[1] = new BasicAI();

    }

    public int NumberOfPlayer = 2;
    public int CurrentPlayerId = 0;

    //bool[] PlayerIsAI;
    BasicAI[] PlayerAIs;

    public int DiceTotal;

    public bool IsDoneRolling = false;
    public bool IsDoneClicking = false;
    //public bool IsDoneAnimating = false;
    public int AnimationsPlaying = 0;
    
    public GameObject NoLegalMovesPopup;
    public GameObject RollAgainPopup;
    public void NewTurn()
    {
        //this is the start of a player's turn.
        //we don't have a roll for them yet.
        IsDoneRolling = false;
        IsDoneClicking = false;
        //IsDoneAnimating = false;

        CurrentPlayerId = (CurrentPlayerId + 1) % NumberOfPlayer; ;
    }


    public void RollAgain()
    {
        
        StartCoroutine( RollAgainCoroutine());
        //Debug.Log("Roll Again");
        IsDoneRolling = false;
        IsDoneClicking = false;
        //IsDoneAnimating = false;
        
    }
    // Update is called once per frame
    void Update () {
        

        //Is the turn done?
        if(IsDoneRolling && IsDoneClicking && AnimationsPlaying==0)
        {
            //Debug.Log("Turn is done");
            NewTurn();
            return;
        }
        if(PlayerAIs[CurrentPlayerId]!=null)
        {
            PlayerAIs[CurrentPlayerId].DoAI();
        }
		
	}

    public void CheckLegalMoves()
    {
        if(DiceTotal == 0)
        {
            StartCoroutine(NoLegalMoveCoroutine());
            return;
        }


        // Loop through all of a player's stone
        PlayerStone[] pss = GameObject.FindObjectsOfType<PlayerStone>();
        bool hasLegalMove = false;
        foreach(PlayerStone ps in pss)
        {
            if(ps.PlayerId == CurrentPlayerId)
            {


                if(ps.CanLegallyMoveAhead(DiceTotal))
                {
                    //highlight the stones that can be moved
                    hasLegalMove = true;
                }
            }
        }

        //highlight stones that can be legally moved
        //if no legal moves are possible wait a sec then move to next player
        if(hasLegalMove ==false)
        {
            StartCoroutine(NoLegalMoveCoroutine());
            return;
        }
    }

    IEnumerator NoLegalMoveCoroutine()
    {
        //display message
        //wait 1 sec
        NoLegalMovesPopup.SetActive(true);
        yield return new WaitForSeconds(1f);
        NoLegalMovesPopup.SetActive(false);

        NewTurn();
    }

    IEnumerator RollAgainCoroutine()
    {
        RollAgainPopup.SetActive(true);
        yield return new WaitForSeconds(1f);
        RollAgainPopup.SetActive(false);
    }
}
