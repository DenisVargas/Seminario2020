using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CheckPoint
{
    int LevelID = 0;
    public List<Baboso> babosos = new List<Baboso>();
    public List<Grunt> grunts = new List<Grunt>();
    public List<IgnitableObject> ignitableObjects = new List<IgnitableObject>();
    public Controller playerData = null;

    public void SetCheckPoint(Level level, Controller playerController)
    {
        playerData = playerController;
        babosos = level.babosos;
        grunts = level.grunts;
        ignitableObjects = level.ignitableObjects;
    }
}
