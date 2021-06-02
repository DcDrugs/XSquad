using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using System;

public class HexUnit : MonoBehaviour
{
	short action;
	public short Action
	{
		get
		{
			return action;
		}
		set
		{
			int tmp = action - value;
			if (tmp > 0)
				HexGrid.Instance().AllAction -= tmp;
			action = value;
		}
	}

	public Transform Position { get; private set; }

	public const short MaxAction = 2;

	const float rotationSpeed = 180f;
	const float travelSpeed = 4f;

	public static HexUnit unitPrefab;
	public List<MeshRenderer> allMesh;

	Renderer render;

	public Color UnitColor
	{
		get
		{
			return render.material.color;
		}

		set
		{
			render.material.color = value;

		}
	}

	public Text Label { get; set; }

	float orientation;

	List<HexCell> pathToTravel;

	TeamTypes team;

	IGun gun = null;

	public int Kills { get; private set; } = 0;
	public IGun Gun
	{ get
		{
			return gun;
		}
		set
		{
			if (value == null)
				return;

			if (gun != null)
				foreach (MeshRenderer mesh in gun.GetAllMesh())
					allMesh.Remove(mesh);
			gun = value;
			gun.Instance(this);
			foreach (MeshRenderer mesh in gun.GetAllMesh())
				allMesh.Add(mesh);
		}
	}

	int health;
	public int Health
	{
		get
		{
			return health;
		}
		private set
		{
			health = value;
		}
	}
	public const int maxHealth = 5;

	bool isSleep = false;

	public TeamTypes Team {
		get
		{
			return team;
		}
		set
		{
			Sleep();
			team = value;
		}
	}

	HexUnit[] enemies = new HexUnit[0];
	public int LookerCount { get; set; } = 0;

	public List<HexUnit> Enemies 
	{
		get
		{
			return new List<HexUnit>(enemies);
		}
		set
        {
			if(HexGrid.Instance().Team == Team)
            {
				for (int i = 0; i < enemies.Length; i++)
				{
					enemies[i].LookerCount--;
					if (enemies[i].LookerCount <= 0)
					{
						enemies[i].DisenableMesh();
						enemies[i].LookerCount = 0;
					}
				}
				for (int i = 0; i < value.Count; i++)
				{
					value[i].LookerCount++;
					value[i].EnableMesh();
				}
			}
			enemies = value.ToArray();
		}
	}

	public List<IAbility> abilities = new List<IAbility>();

	HexCell location, currentTravelLocation;

	public float Orientation
	{
		get
		{
			return orientation;
		}
		set
		{
			orientation = value;
			transform.localRotation = Quaternion.Euler(0f, value, 0f);
		}
	}

	public int Speed
	{
		get
		{
			return 10;
		}
	}

	public int VisionRange
	{
		get
		{
			return 10;
		}
	}

	public HexCell Location
	{
		get
		{
			return location;
		}
		set
		{
			if (location && !isSleep)
			{
				HexGrid.Instance().DecreaseVisibility(this, location, VisionRange);
				isSleep = true;
				location.Unit = null;
			}
			location = value;
			if (location)
			{
				value.Unit = this;
				if (isSleep)
				{
					HexGrid.Instance().IncreaseVisibility(this, value, VisionRange);
					isSleep = false;
				}
				SetUnitTransform(value.LocalPosition);
			}
			else
            {
				isSleep = true;
            }
		}
	}

	public ProtectiveType GetProtectionFrom(HexUnit unit)
	{
		HexDirection direction = HexCoordinates.GetDirection(unit, this);
		HexCell cell = location.GetNeighbor(direction);
		ProtectiveType wall = ProtectiveType.NONE;
		if (cell == null || location.Elevation > cell.Elevation)
			return wall;

		if (cell.Feature != null && cell.Feature.protective > wall)
			wall = cell.Feature.protective;
		if (location.GetEdgeType(direction) == HexEdgeType.Cliff && ProtectiveType.FULL > wall)
			wall = ProtectiveType.FULL;

		return wall;
	}

    private void SetUnitTransform(Vector3 value)
    {
        Vector3 position = Vector3.zero;
        position.y += transform.localScale.y * 0.5f;
        transform.localPosition = value + position;
    }

    private void Awake()
    {
		Label = transform.GetChild(1).GetChild(0).GetComponent<Text>();
		render = GetComponent<Renderer>();
		Position = transform.GetChild(2);
	}
    public void ValidateLocation()
	{
        SetUnitTransform(location.LocalPosition);
	}


	public void AddAbility(IAbility ability)
    {
		abilities.Add(ability);
		float abilityImageSize = ability.Spriteprefab.rectTransform.rect.width 
			+ ability.Spriteprefab.rectTransform.rect.width / 4;
		int MaxOffset = -Mathf.RoundToInt(abilityImageSize) * abilities.Count / 2;
		for (int i = 0; i < abilities.Count; i++)
        {
			transform.GetChild(0).GetChild(i).transform.localPosition 
				= new Vector3(MaxOffset + abilityImageSize * i , -340, 0);
        }
    }

	public void Sleep()
    {
		action = 0;
		if (location && !isSleep)
			HexGrid.Instance().DecreaseVisibility(this, location, VisionRange);
		else if(location)
		{
			HexGrid.Instance().IncreaseVisibility(this, location, VisionRange);
			HexGrid.Instance().DecreaseVisibility(this, location, VisionRange);
		}
		isSleep = true;
	}

	public void WakeUp(short action = 0)
	{
		this.action = action;
		EnableMesh();
		if (location && isSleep)
			HexGrid.Instance().IncreaseVisibility(this, location, VisionRange);
		else if (location)
		{
			HexGrid.Instance().DecreaseVisibility(this, location, VisionRange);
			HexGrid.Instance().IncreaseVisibility(this, location, VisionRange);
		}
		isSleep = false;
	}

    public void DisenableMesh()
    {
        for (int i = 0; i < allMesh.Count; i++)
        {
            allMesh[i].enabled = false;
        }
    }

    public void EnableMesh()
    {
        for (int i = 0; i < allMesh.Count; i++)
        {
            allMesh[i].enabled = true;
        }
    }

    public bool IsValidDestination (HexCell cell) {
		return !cell.IsUnderWater && !cell.Unit;
	}

	public void Travel (List<HexCell> path) {
		location.Unit = null;
		location = path[path.Count - 1];
		location.Unit = this;
		pathToTravel = path;
		StopAllCoroutines();
		StartCoroutine(TravelPath());
		Action--;
	}

	IEnumerator TravelPath () {
		Vector3 a, b, c = pathToTravel[0].LocalPosition;
		yield return LookAt(pathToTravel[1].LocalPosition);
		HexGrid.Instance().DecreaseVisibility(this,
			currentTravelLocation ? currentTravelLocation : pathToTravel[0],
			VisionRange
		);

		float t = Time.deltaTime * travelSpeed;
		for (int i = 1; i < pathToTravel.Count; i++) {
			currentTravelLocation = pathToTravel[i];
			a = c;
			b = pathToTravel[i - 1].LocalPosition;
			c = (b + currentTravelLocation.LocalPosition) * 0.5f;
			HexGrid.Instance().IncreaseVisibility(this, pathToTravel[i], VisionRange);
			for (; t < 1f; t += Time.deltaTime * travelSpeed) {
                SetUnitTransform(Bezier.GetPoint(a, b, c, t));
				Vector3 d = Bezier.GetDerivative(a, b, c, t);
				d.y = 0f;
				transform.localRotation = Quaternion.LookRotation(d);
				yield return null;
			}
			HexGrid.Instance().DecreaseVisibility(this, pathToTravel[i], VisionRange);
			t -= 1f;
		}
		currentTravelLocation = null;

		a = c;
		b = location.LocalPosition;
		c = b;
		HexGrid.Instance().IncreaseVisibility(this, location, VisionRange);
		for (; t < 1f; t += Time.deltaTime * travelSpeed) {
            SetUnitTransform(Bezier.GetPoint(a, b, c, t));
			Vector3 d = Bezier.GetDerivative(a, b, c, t);
			d.y = 0f;
			transform.localRotation = Quaternion.LookRotation(d);
			yield return null;
		}

        SetUnitTransform(location.LocalPosition);
		orientation = transform.localRotation.eulerAngles.y;
		ListPool<HexCell>.Add(pathToTravel);
		pathToTravel = null;
	}

	IEnumerator LookAt (Vector3 point) {
		point.y = transform.localPosition.y;
		Quaternion fromRotation = transform.localRotation;
		Quaternion toRotation =
			Quaternion.LookRotation(point - transform.localPosition);
		float angle = Quaternion.Angle(fromRotation, toRotation);

		if (angle > 0f) {
			float speed = rotationSpeed / angle;
			for (
				float t = Time.deltaTime * speed;
				t < 1f;
				t += Time.deltaTime * speed
			) {
				transform.localRotation =
					Quaternion.Slerp(fromRotation, toRotation, t);
				yield return null;
			}
		}

		transform.LookAt(point);
		orientation = transform.localRotation.eulerAngles.y;
	}

	public void CheckEnemies(HexCell cell = null)
    {
		if (cell == null)
			cell = this.location;

		enemies = HexGrid.Instance().GetVisibleEnemies(this, cell).ToArray();
	}

	public void DieBy(HexUnit unit) {
		if (location && !isSleep) {
			HexGrid.Instance().DecreaseVisibility(this, location, VisionRange);
			isSleep = true;
		}
		unit.Kills++;

		location.Unit = null;
		Destroy(gameObject);
	}

	public void DieYourSelf()
	{
		DieBy(null);
	}

	public void Save (BinaryWriter writer) {
		if (location)
			location.coordinates.Save(writer);
		writer.Write((short)Team);
		writer.Write(orientation);
		writer.Write(Action);
		writer.Write(UnitColor.r + "," + UnitColor.g + "," + UnitColor.b + "," + UnitColor.a);
		GunManager.Save(writer, gun);
		writer.Write(Health);
		writer.Write(Kills);
	}

	static void Fill(HexUnit unit, int health, float orientation, TeamTypes type, short maxAction, Color color, int kills)
    {
		unit.Team = type;
		unit.location = null;
		unit.Orientation = orientation;
		unit.Action = maxAction;
		unit.UnitColor = color;
		unit.Health = health;
		unit.Kills = kills;
		AbilityManager.SetAbilityOnUnit<ShootAbility>(unit);
		AbilityManager.SetAbilityOnUnit<OverWatchAbility>(unit);
		AbilityManager.SetAbilityOnUnit<SkipAbility>(unit);
	}

	public static HexUnit Load(BinaryReader reader)
    {
		TeamTypes type = (TeamTypes)reader.ReadInt16();
		float orientation = reader.ReadSingle();
		short action = reader.ReadInt16();
		var unit = Instantiate(unitPrefab);
		string[] colors = reader.ReadString().Split(',');
		var color = new Color(float.Parse(colors[0]), float.Parse(colors[1]), float.Parse(colors[2]), float.Parse(colors[3]));
		unit.Gun = GunManager.Load(reader);
		var health = reader.ReadInt32();
		var kills = reader.ReadInt32();
		Fill(unit, health, orientation, type, action, color, kills);
		
		return unit;
	}

	public static void Load(BinaryReader reader, HexGrid grid)
	{
		HexCoordinates coordinates = HexCoordinates.Load(reader);
		var unit = Load(reader);
		HexUnit.AddUnitOnGrid(grid, unit, grid.GetCell(coordinates));
	}

	public static HexUnit GenerateUnit(HexUnit prefab, IGun gun, float orientation, TeamTypes type, short maxAction)
    {
		var unit = Instantiate(prefab);
		unit.Gun = gun;

		Fill(unit, maxHealth, orientation, type, maxAction, (type == TeamTypes.BLUE)? Color.blue : Color.red, 0);

		return unit;
	}

	public static void AddUnitOnGrid(HexGrid grid, HexUnit unit, HexCell cell)
	{
		unit.transform.position = Vector3.zero;
		unit.transform.rotation = Quaternion.identity;
		unit.transform.localScale = new Vector3(1, 5, 1);
		grid.AddUnit(unit, cell);
	}

	public static void CreateUnit(HexGrid grid, HexUnit prefab, IGun gun, HexCell cell, float orientation, TeamTypes type, short maxAction)
    {
		HexUnit unit = HexUnit.GenerateUnit(prefab, gun, orientation, type, maxAction);
		HexUnit.AddUnitOnGrid(grid, unit, cell);
	}

	void OnEnable () {
		if (location) {
            SetUnitTransform(location.LocalPosition);
			if (currentTravelLocation && !isSleep) {
				HexGrid.Instance().IncreaseVisibility(this, location, VisionRange);
				HexGrid.Instance().DecreaseVisibility(this, currentTravelLocation, VisionRange);
				currentTravelLocation = null;
			}
		}
	}

	public void GetDamageBy(int damage, HexUnit killer)
    {
		health -= damage;
		if (health < 0)
			HexGrid.Instance().RemoveUnit(this, killer);
	}
	public int TakeDamage()
    {
		return gun.GetDamage();
    }

//	void OnDrawGizmos () {
//		if (pathToTravel == null || pathToTravel.Count == 0) {
//			return;
//		}
//
//		Vector3 a, b, c = pathToTravel[0].Position;
//
//		for (int i = 1; i < pathToTravel.Count; i++) {
//			a = c;
//			b = pathToTravel[i - 1].Position;
//			c = (b + pathToTravel[i].Position) * 0.5f;
//			for (float t = 0f; t < 1f; t += 0.1f) {
//				Gizmos.DrawSphere(Bezier.GetPoint(a, b, c, t), 2f);
//			}
//		}
//
//		a = c;
//		b = pathToTravel[pathToTravel.Count - 1].Position;
//		c = b;
//		for (float t = 0f; t < 1f; t += 0.1f) {
//			Gizmos.DrawSphere(Bezier.GetPoint(a, b, c, t), 2f);
//		}
//	}
}