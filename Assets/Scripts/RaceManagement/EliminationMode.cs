using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliminationMode : IGameMode
{
    private RaceManager raceManager;

    public EliminationMode(RaceManager raceManager)
    {
        this.raceManager = raceManager;
    }
    public void Start()
    {
        raceManager.SetRunClock();
        raceManager.SetAlertGameModeInterval(raceManager.eliminationInterval);
    }

    public void Pluse()
    {
        ArcadeKart ToEliminate = raceManager.GetLastPlace();
        if(ToEliminate == raceManager.playerKart)
        {
            // eliminate Player.
            ToEliminate.enabled = false;
            raceManager.ActiveRacers--;
            raceManager.ui.ShowText("You lose!");
            raceManager.StopAlertGameMode();
        }
        else
        {
            // eliminate AI - not so big a deal.
            ToEliminate.gameObject.SetActive(false);
            raceManager.ActiveRacers--;
            if (raceManager.ActiveRacers == 1)
            {
                // if we get here the play must have won.
                raceManager.ui.ShowText("You Win!");
                raceManager.StopAlertGameMode();
            }
        }
    }

    public void End()
    {
        raceManager.SetRunClock(false);
    }

}

public class LapMode : IGameMode
{
    private RaceManager raceManager;

    public LapMode(RaceManager raceManager)
    {
        this.raceManager = raceManager;
    }

    public void Start()
    {
        raceManager.SetRunClock();
        raceManager.StopAlertGameMode();
    }

    public void Pluse()
    {
        
    }

    public void End()
    {
        raceManager.SetRunClock(false);
    }

}

public interface IGameMode
{
    void Start();
    void End();
    void Pluse();
}

public enum GameModes
{
    Elimination,
    Laps
}

public static class GameModeFactory
{
    public static IGameMode CreateGameMode(RaceManager raceManager, GameModes mode)
    {
        return mode switch
        {
            GameModes.Elimination => new EliminationMode(raceManager),
            GameModes.Laps => new LapMode(raceManager),
            _ => null,
        };
    }
}