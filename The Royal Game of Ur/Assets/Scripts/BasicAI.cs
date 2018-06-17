
using System.Collections.Generic;
using UnityEngine;

public class BasicAI  {

	public BasicAI()
    {
        stateManager = GameObject.FindObjectOfType<StateManager>();
    }

    StateManager stateManager;
    virtual public void DoAI()
    {
        //do the thing ofr the current sate we are in

        if(stateManager.IsDoneRolling == false)
        {
            //need to roll the dice

            DoRoll();
            return;
        }

        if(stateManager.IsDoneClicking == false)
        {
            //we have die roll, now we click the stone
            DoClick();
            return;
        }
    }

    virtual protected void DoRoll()
    {
        GameObject.FindObjectOfType<DiceRoller>().RollTheDice();
    }

    virtual protected void DoClick()
    {
        //pick stone and click it

        PlayerStone[] legalStones = GetLegalMoves();
        if(legalStones == null || legalStones.Length ==0)
        {
            //we have no legal moves how did we get here?
            //we might still be in a delayed corutine somewhere?
            return;
        }
        //basic AI simply picks a legal move at random

        PlayerStone pickedStone = PickStoneToMove(legalStones);

        pickedStone.MoveMe();

    }

    virtual protected PlayerStone PickStoneToMove(PlayerStone[] legalStones)
    {
        return legalStones[Random.Range(0, legalStones.Length)];
    }





    /// <summary>
    /// Returns a listof stones that can be legally moved
    /// </summary>

    protected PlayerStone[] GetLegalMoves()
    {
        List<PlayerStone> legalStones = new List<PlayerStone>();

        if (stateManager.DiceTotal == 0)
        {
            return legalStones.ToArray();
        }


        // Loop through all of a player's stone
        PlayerStone[] pss = GameObject.FindObjectsOfType<PlayerStone>();
        foreach (PlayerStone ps in pss)
        {
            if (ps.PlayerId == stateManager.CurrentPlayerId)
            {
                if (ps.CanLegallyMoveAhead(stateManager.DiceTotal))
                {
                    legalStones.Add(ps);
                }
            }
        }

        return legalStones.ToArray();
       
    }

}
