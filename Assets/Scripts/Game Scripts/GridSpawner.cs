using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum COLORS
{
    Red,Green, Blue
}
public class GridSpawner : MonoBehaviour
{
    float _buffer = 2f;
    public Camera _cam;

    [SerializeField]
    Grid Unity_Grid;
    
    public List<GameObject> Possible_Spawnable_Prefabs;

    public List<List<Cell>> grid = new List<List<Cell>>();
    public int Grid_size = 0;

    // Start is called before the first frame update
    void Start()
    {
        //GridBackend();
    }

    private void Update()
    {
        //CalculateOrthosize();
        //CustomGrid();
    }

    void GridBackend()
    {
        
        for (int i = 0; i < Grid_size; i++)
        {
            grid.Add(new List<Cell>(Grid_size));
            for (int j = 0; j < Grid_size; j++)
            {
                int selectedPrefab = UnityEngine.Random.Range(0, Possible_Spawnable_Prefabs.Count);
                GameObject spawned = SpawnGrid(selectedPrefab, i, j);
                spawned.SetActive(true);
                Cell cell = spawned.GetComponent<Cell>();
                cell.row = i;
                cell.column = j;
                int id = UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(COLORS)).Length);
                cell.id = id;
                grid[i].Add(cell);
            }
        }
    }

    GameObject SpawnGrid(int selectedPrefab, int row, int col)
    {
        var WorldPosition = Unity_Grid.GetCellCenterWorld(new Vector3Int(row, col));
        return Instantiate(Possible_Spawnable_Prefabs[selectedPrefab], WorldPosition, Quaternion.identity, Unity_Grid.gameObject.transform);
    }

    void CustomGrid()
    {
        Debug.Log(Screen.width + " x " + Screen.height);
        
    }

    private (Vector3 center, float size) CalculateOrthosize()
    {
        var bounds = new Bounds();
        foreach (var col in FindObjectsOfType<Collider2D>())
        {
            bounds.Encapsulate(col.bounds);
        }

        bounds.Expand(_buffer);

        var vertical = bounds.size.y;
        var horizontal = bounds.size.x * (_cam.pixelHeight / _cam.pixelWidth);

        var size = Mathf.Max(horizontal, vertical) * 0.5f;
        var center = bounds.center + new Vector3(0, 0, -10);

        return (center, size);
    }
}
