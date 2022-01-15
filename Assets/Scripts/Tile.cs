using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public enum DIRECTIONKEY
{
    KEY0,
    KEY1,
    KEY2,
    KEY3,
    KEY4,
    KEY5
}

public enum VALUE
{
    ROCK,
    AIR,
    GROUND
}

public class Tile
{
    public GameObject tile;
    public VALUE tileValue;

    public Dictionary<DIRECTIONKEY, List<VALUE>> tileInfo;

    public Tile(GameObject _tile, VALUE _tileValue)
    {
        tile = _tile;
        tileValue = _tileValue;
        switch (_tileValue)
        {
            case VALUE.ROCK:
                tileInfo = new Dictionary<DIRECTIONKEY, List<VALUE>>();
                
                List<VALUE> KEY0ValueR = new List<VALUE>();
                KEY0ValueR.Add(VALUE.AIR);
                KEY0ValueR.Add(VALUE.ROCK);
                KEY0ValueR.Add(VALUE.GROUND);
                tileInfo.Add(DIRECTIONKEY.KEY0, KEY0ValueR);

                List<VALUE> KEY1ValueR = new List<VALUE>();
                KEY1ValueR.Add(VALUE.AIR);
                KEY1ValueR.Add(VALUE.ROCK);
                KEY1ValueR.Add(VALUE.GROUND);
                tileInfo.Add(DIRECTIONKEY.KEY1, KEY1ValueR);
                
                List<VALUE> KEY2ValueR = new List<VALUE>();
                KEY2ValueR.Add(VALUE.AIR);
                KEY2ValueR.Add(VALUE.ROCK);
                KEY2ValueR.Add(VALUE.GROUND);
                tileInfo.Add(DIRECTIONKEY.KEY2, KEY2ValueR);
                
                List<VALUE> KEY3ValueR = new List<VALUE>();
                KEY3ValueR.Add(VALUE.AIR);
                KEY3ValueR.Add(VALUE.ROCK);
                KEY3ValueR.Add(VALUE.GROUND);
                tileInfo.Add(DIRECTIONKEY.KEY3, KEY3ValueR);
                
                List<VALUE> KEY4ValueR = new List<VALUE>();
                KEY4ValueR.Add(VALUE.AIR);
                tileInfo.Add(DIRECTIONKEY.KEY4, KEY4ValueR);
                
                List<VALUE> KEY5ValueR = new List<VALUE>();
                KEY5ValueR.Add(VALUE.GROUND);
                tileInfo.Add(DIRECTIONKEY.KEY5, KEY5ValueR);
                break;
            
            
            case VALUE.AIR:
                tileInfo = new Dictionary<DIRECTIONKEY, List<VALUE>>();
                
                List<VALUE> KEY0ValueA = new List<VALUE>();
                KEY0ValueA.Add(VALUE.AIR);
                KEY0ValueA.Add(VALUE.ROCK);
                tileInfo.Add(DIRECTIONKEY.KEY0, KEY0ValueA);

                List<VALUE> KEY1ValueA = new List<VALUE>();
                KEY1ValueA.Add(VALUE.AIR);
                KEY1ValueA.Add(VALUE.ROCK);
                tileInfo.Add(DIRECTIONKEY.KEY1, KEY1ValueA);
                
                List<VALUE> KEY2ValueA = new List<VALUE>();
                KEY2ValueA.Add(VALUE.AIR);
                KEY2ValueA.Add(VALUE.ROCK);
                tileInfo.Add(DIRECTIONKEY.KEY2, KEY2ValueA);
                
                List<VALUE> KEY3ValueA = new List<VALUE>();
                KEY3ValueA.Add(VALUE.AIR);
                KEY3ValueA.Add(VALUE.ROCK);
                tileInfo.Add(DIRECTIONKEY.KEY3, KEY3ValueA);
                
                List<VALUE> KEY4ValueA = new List<VALUE>();
                KEY4ValueA.Add(VALUE.AIR);
                tileInfo.Add(DIRECTIONKEY.KEY4, KEY4ValueA);
                
                List<VALUE> KEY5ValueA = new List<VALUE>();
                KEY5ValueA.Add(VALUE.ROCK);
                KEY5ValueA.Add(VALUE.AIR);
                tileInfo.Add(DIRECTIONKEY.KEY5, KEY5ValueA);
                break;
            
            
            case VALUE.GROUND:
                tileInfo = new Dictionary<DIRECTIONKEY, List<VALUE>>();
                
                List<VALUE> KEY0ValueG = new List<VALUE>();
                KEY0ValueG.Add(VALUE.ROCK);
                KEY0ValueG.Add(VALUE.GROUND);
                tileInfo.Add(DIRECTIONKEY.KEY0, KEY0ValueG);

                List<VALUE> KEY1ValueG = new List<VALUE>();
                KEY1ValueG.Add(VALUE.ROCK);
                KEY1ValueG.Add(VALUE.GROUND);
                tileInfo.Add(DIRECTIONKEY.KEY1, KEY1ValueG);
                
                List<VALUE> KEY2ValueG = new List<VALUE>();
                KEY2ValueG.Add(VALUE.ROCK);
                KEY2ValueG.Add(VALUE.GROUND);
                tileInfo.Add(DIRECTIONKEY.KEY2, KEY2ValueG);
                
                List<VALUE> KEY3ValueG = new List<VALUE>();
                KEY3ValueG.Add(VALUE.ROCK);
                KEY3ValueG.Add(VALUE.GROUND);
                tileInfo.Add(DIRECTIONKEY.KEY3, KEY3ValueG);
                
                List<VALUE> KEY4ValueG = new List<VALUE>();
                KEY4ValueG.Add(VALUE.ROCK);
                KEY4ValueG.Add(VALUE.GROUND);
                tileInfo.Add(DIRECTIONKEY.KEY4, KEY4ValueG);
                
                List<VALUE> KEY5ValueG = new List<VALUE>();
                KEY5ValueG.Add(VALUE.GROUND);
                tileInfo.Add(DIRECTIONKEY.KEY5, KEY5ValueG);
                break;
        }
    }
}
