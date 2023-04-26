using System.Collections;
using System.Collections.Generic;
using Assets._Scripts.DungeonGenerator;
using UnityEngine;
using UnityEngine.Tilemaps;

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
        var map = Generator.GenerateMap(null,
                                        RoomMinWidth..RoomMaxWidth,
                                        RoomMinHeight..RoomMaxHeight,
                                        RoomMinCount..RoomMaxCount,
                                        MinDistanceRooms..MaxDistanceRooms,
                                        MinPassWidth..MaxPassWidth,
                                        PassClearPercent,
                                        Seed);
        var mapArray = map.GetMapArray(OuterCount);
        GroundTilemap.ClearAllTiles();
        WallTilemap.ClearAllTiles();
        for (var i = 0; i < mapArray.GetLength(0); i++)
        for (var j = 0; j < mapArray.GetLength(1); j++)
        {
            switch (mapArray[i, j])
            {
                case 0:
                    GroundTilemap.SetTile(new Vector3Int(i, -j), GroundTile);
                    break;
                case 1:
                    WallTilemap.SetTile(new Vector3Int(i, -j), WallTile);
                    break;
            }
        }

        GameObject.FindWithTag("Player").transform.position = new Vector3(map.Rooms[0].MidPoint.X + OuterCount, -map.Rooms[0].MidPoint.Y - OuterCount);
    }
}
