using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Game : MonoBehaviour
{
    public float holeProbability;
    public int w, h, x, y;
    public bool[,] horizontalWalls, verticalWalls;
    public GameObject Wall;
    public GameObject Floor;
    
    public Transform Level;
    public Transform Player;
    public Transform Goal;

    public CinemachineVirtualCamera virtualCamera;

    void Start()
    {
        GenerateMaze();
    }

    void GenerateMaze()
    {
        foreach (Transform child in Level)
            Destroy(child.gameObject);

        horizontalWalls = new bool[w + 1, h];
        verticalWalls = new bool[w, h + 1];
        var stateCell = new int[w, h];

        void depthFirstSearch(int x, int y)
        {
            stateCell[x, y] = 1;
            Instantiate(Floor, new Vector3(x, y), Quaternion.identity, Level);

            var dirs = new[]
            {
                (x - 1, y, horizontalWalls, x, y, Vector3.right, 90, KeyCode.A),
                (x + 1, y, horizontalWalls, x + 1, y, Vector3.right, 90, KeyCode.D),
                (x, y - 1, verticalWalls, x, y, Vector3.up, 0, KeyCode.S),
                (x, y + 1, verticalWalls, x, y + 1, Vector3.up, 0, KeyCode.W),
            };
            foreach (var (newX, newY, wall, wallX, wallY, sh, angle, k) in dirs.OrderBy(d => Random.value))
                if (!(0 <= newX && newX < w && 0 <= newY && newY < h) || (stateCell[newX, newY] == 2 && Random.value > holeProbability))
                {
                    wall[wallX, wallY] = true;
                    Instantiate(Wall, new Vector3(wallX, wallY) - sh / 2, Quaternion.Euler(0, 0, angle), Level);
                }
                else if (stateCell[newX, newY] == 0) depthFirstSearch(newX, newY);
            stateCell[x, y] = 2;
        }
        
        depthFirstSearch(0, 0);

        x = Random.Range(0, w);
        y = Random.Range(0, h);
        Player.position = new Vector3(x, y);
        do Goal.position = new Vector3(Random.Range(0, w), Random.Range(0, h));
        while (Vector3.Distance(Player.position, Goal.position) < (w + h) / 4);
        virtualCamera.m_Lens.OrthographicSize = Mathf.Pow(w / 3 + h / 2, 0.7f) + 1;
    }

    void Update()
    {
        var dirs = new[]
        {
            (x - 1, y, horizontalWalls, x, y, Vector3.right, 90, KeyCode.A),
            (x + 1, y, horizontalWalls, x + 1, y, Vector3.right, 90, KeyCode.D),
            (x, y - 1, verticalWalls, x, y, Vector3.up, 0, KeyCode.S),
            (x, y + 1, verticalWalls, x, y + 1, Vector3.up, 0, KeyCode.W),
        };
        foreach (var (nx, ny, wall, wx, wy, sh, ang, k) in dirs.OrderBy(d => Random.value))
            if (Input.GetKeyDown(k))
                if (wall[wx, wy])
                    Player.position = Vector3.Lerp(Player.position, new Vector3(nx, ny), 0.1f);
                else (x, y) = (nx, ny);

        Player.position = Vector3.Lerp(Player.position, new Vector3(x, y), Time.deltaTime * 12);
        if (Vector3.Distance(Player.position, Goal.position) < 0.12f)
        {
            if (Random.Range(0, 5) < 3) w++;
            else h++;
            GenerateMaze();
        }
    }
}
