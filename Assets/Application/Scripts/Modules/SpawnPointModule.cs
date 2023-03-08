using System;
using System.Collections.Generic;
using Bloktopia.Fusion.Rig;
using UnityEngine;

/// <summary>
/// This class extends the "BaseModule" class and provides module-specific implementation for spawning points for users.
/// </summary>
/// <see cref="BaseModule"/>
public class SpawnPointModule : BaseModule
{
    private new SpawnPointModuleData data;

    /// <summary>
    /// Creates a reference to the retrieved and apply module data
    /// It updates the position of the transform by adding the offset data to the current position.
    /// When environment has loaded it draws a spawn area and spawns users
    /// Finally, it invokes the callback function if it is not null.
    /// </summary>
    /// <param name="baseModuleData"></param>
    /// <param name="callback"></param>
    public override void Init(BaseModuleData baseModuleData, Action callback)
    {
        data = (SpawnPointModuleData)baseModuleData;
        data.origin += transform.position;
        LoadingManager.Instance.OnEnvironmentLoaded += OnEnvironmentLoaded;

        callback?.Invoke();
    }

    private void OnEnvironmentLoaded()
    {
        var spawnPosition = FindFreeCell();
        var playerTransform = XRRig.Instance.transform;
        playerTransform.position = spawnPosition;
        playerTransform.rotation = transform.rotation;
    }

    private List<Vector3> GetCells()
    {
        List<Vector3> cells = new List<Vector3>();


        for (int i = 0; i < data.dimensions.x; i++)
        {
            var cellX = data.origin.x + (data.cellSize * i) + data.cellSize / 2;

            for (int j = 0; j < data.dimensions.y; j++)
            {
                var cellZ = data.origin.z + (data.cellSize * j) + data.cellSize / 2;
                var pos = new Vector3(cellX, data.origin.y, cellZ);

                var rotatedPos = pos - data.origin;
                rotatedPos = transform.rotation * rotatedPos;
                rotatedPos = data.origin + rotatedPos;
                cells.Add(rotatedPos);
            }
        }

        return cells;
    }

    private Vector3 FindFreeCell()
    {
        var cells = GetCells();

        cells.Shuffle();

        foreach (var cell in cells)
        {
            RaycastHit hit;
            if (!Physics.SphereCast(cell, data.cellSize / 2, transform.forward, out hit))
            {
                return cell;
            }
        }

        return cells[0];
    }

    private void OnDrawGizmosSelected()
    {
        foreach (var cell in GetCells())
        {
            Gizmos.color = new Color(0, 1, 0, 0.5f);
            Gizmos.DrawSphere(cell, data.cellSize / 2);
        }
    }

    /// <summary>
    /// Unsubscribes from the previous subscription
    /// </summary>
    public override void Deinit()
    {
        LoadingManager.Instance.OnEnvironmentLoaded -= OnEnvironmentLoaded;
    }
}

/// <summary>
/// Data which is modifiable by users
/// </summary>
/// <see cref="BaseModuleData"/>
public class SpawnPointModuleData : BaseModuleData
{
    /// <summary>
    /// The origin
    /// </summary>
    public Vector3 origin;
    /// <summary>
    /// The vector
    /// </summary>
    public Vector2 dimensions = new Vector2(2, 2);
    /// <summary>
    /// The cell size
    /// </summary>
    public float cellSize = 1f;
}