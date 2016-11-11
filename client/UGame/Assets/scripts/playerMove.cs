﻿using UnityEngine;
using System.Collections;

public class playerMove : MonoBehaviour {
	private bool isMoving;
	//private Vector3 beginxy;
	private Vector3 endxy;
	public float speed;
	public float step;
	public OBJTYPEData MapOBJ;
	// Use this for initialization

	private GameObject map;

	private GameObject role;
	private GameObject weapon;

	//player初始位置
	private int[] iniCell;
	private Vector2 iniPos;

	//player动画控制
	private Animator animator; 
	private Animator weaponAnimator;

	private int roleOrder;
	private int weaponOrder;

	private int pathid;
	//存储player的行，列，朝向；在移动的时候变化
	public int row;
	public int column;
	private string orientation;



//	public Sprite weaponTileH;
//	public Sprite weaponTileV;
	private Astar astar;
	void Awake(){
		map = GameObject.Find ("map");
		role = transform.Find ("man").gameObject;
		weapon = transform.Find ("weapon").gameObject;
		//OBJTYPEList obj_list  = map.GetComponent<RandomDungeonCreator>().obj_list;//获取object列表
		///row = obj_list.getListByType (OBJTYPE.OBJTYPE_SPAWNPOINT) [0].row;
		//column = obj_list.getListByType (OBJTYPE.OBJTYPE_SPAWNPOINT) [0].column;
		//int[] p=map.GetComponent<TilesManager>().posTransform2(transform.position.x,transform.position.y);
		//row = p [0];
		//column =p[1];	

	}
	public void set(int irow ,int icolumn){
		orientation = "DOWN";
		iniCell = new int[2];
		iniCell [0] = irow;
		iniCell [1] = icolumn;
		row = irow;
		column = icolumn;
		iniPos = map.GetComponent<TilesManager>().posTransform(row,column);
		Debug.Log (row);
		Debug.Log (column);
		//初始化位置
		transform.position = iniPos;
		astar= new Astar();
		isMoving = false;
		endxy = transform.position;
		animator = role.GetComponent<Animator>();
		weaponAnimator = weapon.GetComponent<Animator>();
	}
	void Start () {
		

	}

	private void PlaceRoleBehindWeapon(){
		roleOrder = role.GetComponent<SpriteRenderer> ().sortingOrder;
		weapon.GetComponent<SpriteRenderer> ().sortingOrder = roleOrder +1;
	}

	private void PlaceRoleBeforeWeapon(){
		roleOrder = role.GetComponent<SpriteRenderer> ().sortingOrder;
		weapon.GetComponent<SpriteRenderer> ().sortingOrder = roleOrder -1;
	}

	//根据单元格做碰撞检测
	private void AttemptMove(string dir,int i,int j){
		string tileType = map.GetComponent<RandomDungeonCreator>().getMapTileType(i,j);
		switch (tileType) {
			default:

				if (isMoving)
					return;
				switch (dir) {
				case "UP":
					endxy = new Vector3 (transform.position.x, transform.position.y + step, 0);
					row--;
//				Debug.Log (row + "," + column + " UP");
					break;
				case "DOWN":
					endxy = new Vector3 (transform.position.x, transform.position.y - step, 0);
					row++;
//				Debug.Log (row + "," + column + " DOWN");
					break;
				case "LEFT":
					endxy = new Vector3 (transform.position.x - step, transform.position.y, 0);
					column--;
//				Debug.Log (row + "," + column + " LEFT");
					break;
				case "RIGHT":
					endxy = new Vector3 (transform.position.x + step, transform.position.y, 0);
					column++;
//				Debug.Log (row + "," + column + " RIGHT");
					break;
				}
				isMoving = true;
			break;
			case "WALL":
				Debug.Log ("cnot go");
			break;

		}
		return;
	}

	public void moveUp(){
		orientation = "UP";
//		Debug.Log ("UP");
		PlaceRoleBehindWeapon();
		animator.SetTrigger ("PlayerMoveUp");
		weaponAnimator.SetTrigger ("WeaponOnMoveUp");
		AttemptMove (orientation,row - 1, column);
	
	}
	public void moveDown(){
		orientation = "DOWN";
		//Debug.Log ("Down");
		PlaceRoleBeforeWeapon();
		animator.SetTrigger ("PlayerMoveDown");
		weaponAnimator.SetTrigger ("WeaponOnMoveDown");
		AttemptMove (orientation,row+1, column);

	}
	public void moveLeft(){
		orientation = "LEFT";
		PlaceRoleBeforeWeapon();
		animator.SetTrigger ("PlayerMoveLeft");
		weaponAnimator.SetTrigger ("WeaponOnMoveLeft");
		AttemptMove (orientation,row, column-1);

	}
	public void moveRight(){
		orientation = "RIGHT";
		PlaceRoleBeforeWeapon();
		animator.SetTrigger ("PlayerMoveRight");
		weaponAnimator.SetTrigger ("WeaponOnMoveRight");
		AttemptMove (orientation,row, column+1);
	}
	public void Actioning(){
		if (isMoving) {
			transform.position = new Vector3 (Mathf.MoveTowards (transform.position.x, endxy.x, Time.deltaTime * speed), Mathf.MoveTowards (transform.position.y, endxy.y, Time.deltaTime * speed), 0);
			GameObject.Find ("light").GetComponent<ligthmap> ().reDrawLight ();
			int[] v= GameObject.Find ("map").GetComponent<TilesManager> ().posTransform2 (transform.position.x,transform.position.y);
			MapOBJ.row =v[0];
			MapOBJ.column = v[1];
		}
		if (transform.position == endxy) {
			transform.position = endxy;
			int[] v= GameObject.Find ("map").GetComponent<TilesManager> ().posTransform2 (transform.position.x,transform.position.y);
			MapOBJ.row =v[0];
			MapOBJ.column = v[1];
			if (map.GetComponent<RoundControler> ().CheckPlayerInSeeSight ()&&!map.GetComponent<RoundControler> ().CheckPlayerInBattle()) {
				pathid = -1;
				map.GetComponent<RoundControler> ().round = map.GetComponent<RoundControler> ().order [0];

			}
			else pathid--;
			if (pathid < 0  && isMoving) {
				transform.gameObject.GetComponent<PhaseHandler>().state.handle (new Action (ACTION_TYPE.ACTION_NULL));
				isMoving = false;
				//				Debug.Log (orientation);
				//				根据朝向设置 player的动画
				switch (orientation){
				case "UP": 
					animator.SetTrigger ("PlayerIdleUp");
					weaponAnimator.SetTrigger ("WeaponOnIdleUp");
					break;
				case "DOWN": 
					animator.SetTrigger ("PlayerIdleDown");
					weaponAnimator.SetTrigger ("WeaponOnIdleDown");
					break;
				case "LEFT": 
					animator.SetTrigger ("PlayerIdleLeft");
					weaponAnimator.SetTrigger ("WeaponOnIdleLeft");
					break;
				case "RIGHT": 
					animator.SetTrigger ("PlayerIdleRight");
					weaponAnimator.SetTrigger ("WeaponOnIdleRight");
					break;
				default:
					animator.SetTrigger ("PlayerIdleDown");
					weaponAnimator.SetTrigger ("WeaponOnIdleDown");
					break;
				}

			}
			else if(pathid>=0){
				isMoving = false;
				//				Debug.Log ("path"+pathid+":"+row + "," + column + " to " + astar.finalpath [pathid] [0] + "," + astar.finalpath [pathid] [1]);
				if (astar.finalpath [pathid] [0] < row)
					moveUp ();
				if (astar.finalpath [pathid] [0] > row)
					moveDown ();
				if (astar.finalpath [pathid] [1] < column)
					moveLeft ();
				if (astar.finalpath [pathid] [1] > column)
					moveRight ();
				//Debug.Log (transform.position.x + "," + transform.position.y + " " + endxy.x + "," + endxy.y);
			}

		}
		
	}
	public void moveTo(int x,int y){
		Debug.Log ("MOVE");
		Debug.Log (x);Debug.Log (y);
		Debug.Log (row);Debug.Log (column);
		int[] pos={x,y};
		astar= new Astar(row,column,pos[0],pos[1],map.GetComponent<RandomDungeonCreator>().getMap(),32,32);
		astar.Run ();
	    Debug.Log ("Path long = " + astar.finalpath.Count);
		pathid = astar.finalpath.Count-1;
		if (pathid >= 1) {
			if (map.GetComponent<RoundControler> ().CheckPlayerInBattle()) {
				if (astar.finalpath.Count > transform.gameObject.GetComponent<playerStatus> ().MOV) {
					astar.finalpath.RemoveRange (0, pathid - transform.gameObject.GetComponent<playerStatus> ().MOV);
					pathid = astar.finalpath.Count - 1;
					Debug.Log ("Path long = " + astar.finalpath.Count);
				}
			}
			Action Mov = new Action (ACTION_TYPE.ACTION_MOVE);
			Mov.SUBJECT = transform.gameObject;
			Mov.MOVEPOS [0] = x;
			Mov.MOVEPOS [1] = y;
			transform.GetComponent<PhaseHandler>().state.handle (Mov);
			//Debug.Log ("path"+pathid+":"+row + "," + column + " to " + astar.finalpath [pathid] [0] + "," + astar.finalpath [pathid] [1]);
		}
	}
	public void AI(){
		int[] c=map.GetComponent<RandomDungeonCreator>().pickAplace();
		moveTo (c [0], c [1]);
		//transform.gameObject.GetComponent<PhaseHandler>().state.handle (new Action (ACTION_TYPE.ACTION_NULL));
		//transform.gameObject.GetComponent<PhaseHandler>().state.handle (new Action (ACTION_TYPE.ACTION_NULL));
	}
	// Update is called once per frame
	void Update () {
		
	}
	public void canSeeEnemy(){
		
	}
}
