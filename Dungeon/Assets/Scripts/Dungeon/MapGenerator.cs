using System.Collections;
using System.Collections.Generic;
using Assets._Scripts.DungeonGenerator;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class CurrentMap
{
    public static Map    Map;
    public static int[,] MapArr;

    public static void SetMap(Map map, int extend = 0)
    {
        Map    = map;
        MapArr = Map.GetMapArray(extend);
    }
}

public class MapGenerator : MonoBehaviour
{
    public Tilemap  WallTilemap;
    public Tilemap  GroundTilemap;
    public TileBase WallTile;
    public TileBase GroundTile;
    public int      OuterCount = 50;

    public int RoomMinWidth     = 10;
    public int RoomMaxWidth     = 30;
    public int RoomMinHeight    = 10;
    public int RoomMaxHeight    = 30;
    public int RoomMinCount     = 5;
    public int RoomMaxCount     = 10;
    public int MinDistanceRooms = 15;
    public int MaxDistanceRooms = 30;
    public int MinPassWidth     = 1;
    public int MaxPassWidth     = 3;
    public int PassClearPercent = 50;
    public int Seed             = -1;

    void Start()
    {
        GenerateMapAndSetPlayer();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            GenerateMapAndSetPlayer();
    }

    private void GenerateMapAndSetPlayer()
    {
        CurrentMap.SetMap(Generator.GenerateMap(null,
                                        RoomMinWidth..RoomMaxWidth,
                                        RoomMinHeight..RoomMaxHeight,
                                        RoomMinCount..RoomMaxCount,
                                        MinDistanceRooms..MaxDistanceRooms,
                                        MinPassWidth..MaxPassWidth,
                                        PassClearPercent,
                                        Seed), OuterCount);
        GroundTilemap.ClearAllTiles();
        WallTilemap.ClearAllTiles();
        for (var i = 0; i < CurrentMap.MapArr.GetLength(0); i++)
        for (var j = 0; j < CurrentMap.MapArr.GetLength(1); j++)
        {
            switch (CurrentMap.MapArr[i, j])
            {
                case 1:
                    WallTilemap.SetTile(new Vector3Int(i, -j), WallTile);
                    break;
                    case 0:
                    GroundTilemap.SetTile(new Vector3Int(i, -j), GroundTile);
                    break;
            }
        }

        GameObject.FindWithTag("Player").transform.position = new Vector3(CurrentMap.Map.Rooms[0].MidPoint.X + OuterCount, -CurrentMap.Map.Rooms[0].MidPoint.Y - OuterCount);
        GameObject.FindWithTag("Enemy").transform.position  = new Vector3(CurrentMap.Map.Rooms[1].MidPoint.X + OuterCount, -CurrentMap.Map.Rooms[1].MidPoint.Y - OuterCount);
    }
}
