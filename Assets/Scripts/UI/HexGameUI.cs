using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class HexGameUI : MonoBehaviour
{
	static HexGameUI instance = null;
	public HexGrid grid;
	public GameObject HUD;

	HexCell currentCell;
	HexUnit selectedUnit;

	public Button enemyIconButtonBrefab;
	List<EnemyIconManager> enemiesImages;
	const int imagesCount = 20;

	bool isChanged = false;

	public static HexGameUI Instance()
    {
		return instance;
    }

	private void Awake()
	{
		instance = this;
		var enemiesTransform = this.transform.GetChild(0).transform;
 		enemiesImages = new List<EnemyIconManager>(imagesCount);
		for (int i = 0; i < imagesCount; i++)
		{
			Button button = Instantiate(enemyIconButtonBrefab);
			button.transform.SetParent(enemiesTransform, false);
			var enemy = button.GetComponent<EnemyIconManager>();
			enemy.icon.color = Color.red;
			enemy.gameObject.SetActive(false);
			int posX = i, posY = 0;
			if(i >= 5)
            {
				posX = i % 5;
				posY = i / 5;
            }
            enemy.transform.localPosition = new Vector3(10 + 20 * posX, 100 - 25 * posY, 0);
			enemy.transform.localScale = new Vector3(5, 5, 1);
            enemiesImages.Add(enemy);
        }
    }

    void Update()
	{
		if (!EventSystem.current.IsPointerOverGameObject() && !HexMapCamera.Locked)
		{
			if (Input.GetMouseButtonDown(0))
			{
				ShowEnemies(selectedUnit);
				DoSelection();
			}
			else if (selectedUnit && selectedUnit.Action > 0 && selectedUnit.Team == grid.Team)
			{
				if (Input.GetMouseButtonDown(1))
				{
					DoMove();
				}
				else
				{
					DoPathfinding();
                    if (Input.GetKey(KeyCode.LeftControl))
                    {
						selectedUnit.CheckEnemies(currentCell);
                        ShowEnemies(selectedUnit);
						isChanged = true;
                    }
					else
                    {
						if(isChanged)
                        {
							selectedUnit.CheckEnemies();
							EraseEnemies();
							ShowEnemies(selectedUnit);
							isChanged = false;
						}
					}
                }
			}
		}
		if(!selectedUnit)
			EraseEnemies();

		if (selectedUnit && (selectedUnit.Action == 0 || selectedUnit.Team != grid.Team))
		{
			grid.ClearPath();
			EraseEnemies();
			DestoyAbilities(selectedUnit);
			selectedUnit = null;
		}
	}

	bool UpdateCurrentCell()
	{
		HexCell cell =
			grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
		if (cell != currentCell)
		{
			currentCell = cell;
			return true;
		}
		return false;
	}

	void DoSelection()
	{
		grid.ClearPath();
		UpdateCurrentCell();
		if (currentCell)
		{
			EraseEnemies();
			if (selectedUnit)
			{
				DestoyAbilities(selectedUnit);
			}
			selectedUnit = currentCell.Unit;
			if (selectedUnit)
			{
				if (selectedUnit.Team != grid.Team || selectedUnit.Action == 0)
				{
					selectedUnit = null;
					return;
				}
				ShowEnemies(selectedUnit);
				ShowAbilities(selectedUnit);
			}
		}
	}

	void DoPathfinding()
	{
		if (UpdateCurrentCell())
		{
			if (currentCell && selectedUnit.IsValidDestination(currentCell))
			{
				grid.FindPath(selectedUnit.Location, currentCell, selectedUnit.Speed);
			}
			else
			{
				grid.ClearPath();
			}
		}
	}

	public void SetEditMode(bool toggle)
	{
		enabled = !toggle;
		grid.ShowUI(!toggle);
		grid.ClearPath();
	}

	void DoMove()
	{
		if (grid.HasPath)
		{
			selectedUnit.Travel(grid.GetPath());
			EraseEnemies();
			ShowEnemies(selectedUnit);
			grid.ClearPath();
		}
	}

	public HexUnit GetSelectedUnit()
    {
		return selectedUnit;
    }

	public void EraseEnemies()
	{
		for (int i = 0; i < enemiesImages.Count; i++)
		{
			if (enemiesImages[i].gameObject.activeSelf == false)
				return;
			enemiesImages[i].gameObject.SetActive(false);
		}
	}

	public void ShowEnemies(HexUnit unit)
    {
		EraseEnemies();
		if (unit != null)
		{
			for (int i = 0; i < unit.Enemies.Count; i++)
			{
				enemiesImages[i].gameObject.SetActive(true);
				enemiesImages[i].Enemies = unit.Enemies;
				enemiesImages[i].indexEnemy = i;
				if (unit.Enemies[i].GetProtectionFrom(unit) == ProtectiveType.NONE)
					enemiesImages[i].icon.color = Color.yellow;
				else
					enemiesImages[i].icon.color = Color.red;
			}
		}
    }

    public void ShowAbilities(HexUnit unit)
    {
        for(int i = 0; i < unit.abilities.Count; i++)
        {
			unit.abilities[i].ButtonAbility.enabled = true;
			unit.abilities[i].Spriteprefab.enabled = true;
		}
    }

    public void DestoyAbilities(HexUnit unit)
    {
		for (int i = 0; i < unit.abilities.Count; i++)
		{
			unit.abilities[i].ButtonAbility.enabled = false;
			unit.abilities[i].Spriteprefab.enabled = false;
		}
	}
}
