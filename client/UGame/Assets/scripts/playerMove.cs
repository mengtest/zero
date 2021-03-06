﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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

	public int MAXSTEP;
	private string orientation;

	public GameObject TanhaoPb;
	public GameObject ZhandouPb;
	public GameObject WenhaoPb;
	public GameObject MovabletilePb;
	public GameObject MovabletileClickPb;
	public GameObject AttackabletilePb;
	public GameObject AttackableClickePb;
	private GameObject Tanhao;
	private GameObject Wenhao;
	private GameObject Zhandou;
	private List<GameObject> Movabletiles;
	private GameObject MovabletileClick;
	private List<GameObject> Attackabletiles;
	private GameObject AttackableClick;
	public float action_anime_time;
//	public Sprite weaponTileH;
//	public Sprite weaponTileV;
	public Astar astar;
	void Awake(){
		map = GameObject.Find ("map");
		role = transform.Find ("man").gameObject;
		weapon = transform.Find ("weapon").gameObject;
		Movabletiles=new List<GameObject>();
		Attackabletiles=new List<GameObject>();
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
	//决策移动的回合切换
	public void moveTo(int x,int y){
		int[] pos={x,y};
		astar= new Astar(row,column,pos[0],pos[1],map.GetComponent<RandomDungeonCreator>().getMap(),map.GetComponent<RandomDungeonCreator>().MapWidth,map.GetComponent<RandomDungeonCreator>().MapHeight);
		astar.isWalkableFunc = map.GetComponent<RandomDungeonCreator> ().MapWalkable;
		astar.Run ();
		pathid = astar.finalpath.Count-1;
		if (pathid >= 1) {
			if (map.GetComponent<RoundControler> ().CheckPlayerInBattle ()) {
				if (astar.finalpath.Count > transform.gameObject.GetComponent<playerStatus> ().MOV) {
					astar.finalpath.RemoveRange (0, pathid - transform.gameObject.GetComponent<playerStatus> ().MOV);
					pathid = astar.finalpath.Count - 1;

					//Debug.Log ("Path long = " + astar.finalpath.Count);
				}
			}
			if(!transform.GetComponent<playerStatus>().isAI())DrawMoveClick (astar.finalpath [0] [0], astar.finalpath [0] [1]);
			//Move行为触发：决策阶段-》行动动画阶段
			Action Mov = new Action (ACTION_TYPE.ACTION_MOVE, transform.gameObject);
			Mov.MOVEPOS [0] = x;
			Mov.MOVEPOS [1] = y;
			transform.GetComponent<PhaseHandler> ().state.handle (Mov);
		}

	}
	//决策移动的动画阶段
	public void Move_Actioning(){
		//如果是移动动画播放状态，进行位移一格
		if (isMoving) {
			transform.position = new Vector3 (Mathf.MoveTowards (transform.position.x, endxy.x, Time.deltaTime * speed), Mathf.MoveTowards (transform.position.y, endxy.y, Time.deltaTime * speed), 0);
			GameObject.Find ("light").GetComponent<ligthmap> ().reDrawLight ();
			//更新地图数据
			int[] v= GameObject.Find ("map").GetComponent<TilesManager> ().posTransform2 (transform.position.x,transform.position.y);
			MapOBJ.row =v[0];
			MapOBJ.column = v[1];
		}
		//当到达移动一格后
		if (transform.position == endxy) {
			transform.position = endxy;
			int[] v= GameObject.Find ("map").GetComponent<TilesManager> ().posTransform2 (transform.position.x,transform.position.y);
			MapOBJ.row =v[0];
			MapOBJ.column = v[1];
			//如果玩家进入任何敌人视野进入战斗模式，中断移动
			if (map.GetComponent<RoundControler> ().CheckPlayerInSeeSight ()&&!map.GetComponent<RoundControler> ().CheckPlayerInBattle()) {
				pathid = -1;
				//map.GetComponent<RoundControler> ().round = map.GetComponent<RoundControler> ().order [0];

			}
			else pathid--;
			//如果所有移动都结束，结束回合
			if (pathid < 0 && isMoving) {
				transform.gameObject.GetComponent<PhaseHandler> ().state.handle (new Action (ACTION_TYPE.ACTION_NULL, transform.gameObject));
				isMoving = false;
				//根据朝向设置 player的动画
				switch (orientation) {
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

			} else if (pathid >= 0) {  //如果下一步存在，则进行移动
				
				isMoving = false;
				if (astar.finalpath [pathid] [0] < row)
					moveUp ();
				if (astar.finalpath [pathid] [0] > row)
					moveDown ();
				if (astar.finalpath [pathid] [1] < column)
					moveLeft ();
				if (astar.finalpath [pathid] [1] > column)
					moveRight ();

			} else {
				pathid = -1;
			
			}

		}
		
	}
	//决策无行动的回合切换
	public void NOACTION(){
		Action no = new Action (ACTION_TYPE.ACTION_NULL,transform.gameObject);
		transform.GetComponent<PhaseHandler>().state.handle (no);
	}
	//无行动的动画阶段
	public void NOACTION_Actioning(){
		Action caction = transform.GetComponent<PhaseHandler> ().state.act;
		transform.GetComponent<PhaseHandler>().state.handle (caction);
	}
	//决策攻击的回合切换
	public void Attack(GameObject obj){
		Action Atk = new Action (ACTION_TYPE.ACTION_ATTACK,transform.gameObject);
		Atk.OBJECT = obj;
		transform.GetComponent<PhaseHandler>().state.handle (Atk);
	}
	//决策攻击的动画阶段
	public void Attack_Actioning(){
		action_anime_time -= Time.deltaTime;
		Vector2 op=GameObject.Find("map").GetComponent<TilesManager>().posTransform(row,column);
		transform.position = new Vector2 (Mathf.Sin(action_anime_time*100)*0.1f+op.x,op.y);
		if (action_anime_time <= 0) {
			action_anime_time = 0.2f;
			transform.position=GameObject.Find("map").GetComponent<TilesManager>().posTransform(row,column);
			Action caction = transform.GetComponent<PhaseHandler> ().state.act;
			transform.GetComponent<PhaseHandler>().state.handle (caction);
		}

	}
	//AI决策
	public void AI(){
		GameObject p = GameObject.Find ("map").GetComponent<RoundControler> ().returnPlayer ();
		int er = p.GetComponent<playerMove> ().row;
		int ec = p.GetComponent<playerMove> ().column;
		int dis = Mathf.Abs (transform.GetComponent<playerMove> ().row - er) + Mathf.Abs (transform.GetComponent<playerMove> ().column - ec);
		if (dis <= transform.GetComponent<playerStatus> ().ATKRange) {
			//在攻击范围优先攻击
			Debug.Log (transform.name + " decide to ATK!");
			Attack (p);
		} else {
			//能攻击到玩家的最近的空格移动
			//Debug.Log ("Find Closest Place");
			int minr = er+transform.GetComponent<playerStatus> ().ATKRange;
			int minc = ec+transform.GetComponent<playerStatus> ().ATKRange;
			int MIN = -1;
			for (int t1 = -transform.GetComponent<playerStatus> ().ATKRange; t1 <= transform.GetComponent<playerStatus> ().ATKRange; t1++)
				for (int t2 = -transform.GetComponent<playerStatus> ().ATKRange; t2 <= transform.GetComponent<playerStatus> ().ATKRange; t2++) {
					if (Mathf.Abs (t1) + Mathf.Abs (t2) > transform.GetComponent<playerStatus> ().ATKRange || Mathf.Abs (t1) + Mathf.Abs (t2) == 0)
						continue;
					if (map.GetComponent<RandomDungeonCreator> ().MapWalkable (er + t1, ec + t2)) {
						//Debug.Log ("Walkabe " + (er + t1) + "," + (ec + t2));
						astar= new Astar(row,column,er + t1,ec + t2,map.GetComponent<RandomDungeonCreator>().getMap(),map.GetComponent<RandomDungeonCreator>().MapWidth,map.GetComponent<RandomDungeonCreator>().MapHeight);
						astar.isWalkableFunc = map.GetComponent<RandomDungeonCreator> ().MapWalkable;
						astar.Run ();
						pathid = astar.finalpath.Count-1;
						if (pathid <= 0)
							continue;
						int mdis = Mathf.Abs (transform.GetComponent<playerMove> ().row - er - t1) + Mathf.Abs (transform.GetComponent<playerMove> ().column - ec - t2);
						if (MIN == -1) {
							MIN = mdis;
							minr = er + t1;
							minc = ec + t2;
						} else {
							if (MIN > mdis) {
								MIN = mdis;
								minr = er + t1;
								minc = ec + t2;
							}

						}
					} else {
						//Debug.Log ("UnWalkabe " + (er + t1) + "," + (ec + t2));
					}
				}
			if (MIN != -1) {
				Debug.Log (transform.name + " decide to MOVE to (" + minr + "," + minc + ")");
				//Debug.Log(factor[minf,0]+":"+factor[minf,1]);
				transform.GetComponent<playerMove> ().moveTo (minr, minc);
			} else {
				Debug.Log (transform.name + " decide to DO NOTHING!");
				transform.GetComponent<playerMove> ().NOACTION ();
			}
		}
	}
	//死亡处理
	public void Dead(){
		if (GameObject.Find ("map").GetComponent<RoundControler> ().returnPlayer().GetInstanceID () == transform.gameObject.GetInstanceID ()) {
			Debug.Log ("Game Over!");
			map.GetComponent<MapObjectGenerator> ().NewDungeon ();
		} else {
			Debug.Log (transform.name+" Dead!");
			GameObject.Find ("map").GetComponent<RoundControler>().RemoveGOFromRoundControler (transform.gameObject);
			GameObject.Find ("map").GetComponent<RandomDungeonCreator> ().obj_list.RemoveObjByID (MapOBJ.id);
			Destroy (transform.gameObject);
		}
	}
	// Update is called once per frame
	void Update () {


	}
	public void DrawMoveClick(int r,int c){
		if (GameObject.Find ("map").GetComponent<RoundControler> ().player == null)
			return;
		if (MovabletileClick!=null) {
			Destroy (MovabletileClick.gameObject);
		}
		Vector2 p = GameObject.Find ("map").GetComponent<TilesManager> ().posTransform (r, c);
		MovabletileClick=(GameObject)Instantiate (MovabletileClickPb,p,transform.rotation);
		MovabletileClick.layer =13;
	}
	public void RemoveMoveClick(){
		if (GameObject.Find ("map").GetComponent<RoundControler> ().player == null)
			return;
		if (MovabletileClick!=null) {
			Destroy (MovabletileClick.gameObject);
		}
	}
	public void DrawMovabletile(){
		if (GameObject.Find ("map").GetComponent<RoundControler> ().player == null)
			return;
		if (Movabletiles.Count > 0) {
			for (int i = 0; i < Movabletiles.Count; i++) {
				Destroy (Movabletiles[i].gameObject);
			}
		}
		int r = transform.GetComponent<playerMove> ().row;
		int c = transform.GetComponent<playerMove> ().column;
		for (int ir = r - transform.GetComponent<playerStatus>().MOV; ir <= r + transform.GetComponent<playerStatus>().MOV; ir++) {
			for (int ic = c - transform.GetComponent<playerStatus>().MOV; ic <= c + transform.GetComponent<playerStatus>().MOV; ic++) {
				GameObject map=GameObject.Find ("map");
				if (map.GetComponent<RandomDungeonCreator> ().MapWalkable (ir, ic)) {
					Astar astar= new Astar(r,c,ir ,ic,map.GetComponent<RandomDungeonCreator>().getMap(),map.GetComponent<RandomDungeonCreator>().MapWidth,map.GetComponent<RandomDungeonCreator>().MapHeight);
					astar.isWalkableFunc = map.GetComponent<RandomDungeonCreator> ().MapWalkable;
					astar.Run ();
					if (astar.finalpath.Count-1 <= 0||astar.finalpath.Count-1>transform.GetComponent<playerStatus>().MOV)
						continue;
					Vector2 p = GameObject.Find ("map").GetComponent<TilesManager> ().posTransform (ir, ic);
					GameObject a = (GameObject)Instantiate (MovabletilePb,p,transform.rotation);
					a.layer =13;
					Movabletiles.Add (a);
				}
			}
		}
		DrawAttackable ();
	}
	public void RemoveMovabletile(){
		if (GameObject.Find ("map").GetComponent<RoundControler> ().player == null)
			return;
		if (Movabletiles.Count > 0) {
			for (int i = 0; i < Movabletiles.Count; i++) {
				Destroy (Movabletiles[i].gameObject);
			}
		}
		Removettackabletile ();
	}
	public void DrawAttackable(){
		List<GameObject> enemy = GameObject.Find ("map").GetComponent<RoundControler> ().enemy;
		if (enemy.Count<=0)
			return;
		if (Attackabletiles.Count > 0) {
			for (int i = 0; i < Attackabletiles.Count; i++) {
				Destroy (Attackabletiles[i].gameObject);
			}
		}
		for (int i = 0; i < enemy.Count; i++) {
			int er = enemy [i].GetComponent<playerMove> ().row;
			int ec = enemy [i].GetComponent<playerMove> ().column;
			int dis = Mathf.Abs (er - row) + Mathf.Abs (ec - column);
			if (dis <= transform.GetComponent<playerStatus> ().ATKRange) {
				Vector2 p = GameObject.Find ("map").GetComponent<TilesManager> ().posTransform (er, ec);
				GameObject a = (GameObject)Instantiate (AttackabletilePb,p,transform.rotation);
				a.layer =13;
				Attackabletiles.Add (a);
			}
		
		}
	
	
	}
	public void Removettackabletile(){
		List<GameObject> enemy = GameObject.Find ("map").GetComponent<RoundControler> ().enemy;
		if (enemy.Count<=0)
			return;
		if (Attackabletiles.Count > 0) {
			for (int i = 0; i < Attackabletiles.Count; i++) {
				Destroy (Attackabletiles[i].gameObject);
			}
		}
	}
	public void initHeadIcon(){
		Vector3 newp = new Vector3(transform.position.x,transform.position.y+0.65f, 1);
		Tanhao=(GameObject)Instantiate (TanhaoPb,newp,transform.rotation);
		Tanhao.layer=5;
		Tanhao.GetComponent<SpriteRenderer> ().sortingOrder = 2;
		Tanhao.transform.SetParent (transform);
		Tanhao.SetActive (false);
		Tanhao.name=transform.name+" Tanhao";
		Vector3 newp2 = new Vector3(transform.position.x,transform.position.y+0.65f, 1);
		Zhandou=(GameObject)Instantiate (ZhandouPb,newp2,transform.rotation);
		Zhandou.layer = 5;
		Zhandou.GetComponent<SpriteRenderer> ().sortingOrder = 2;
		Zhandou.transform.SetParent (transform);
		Zhandou.SetActive (false);
		Zhandou.name=transform.name+" Zhandou";
		Vector3 newp3 = new Vector3(transform.position.x,transform.position.y+0.65f, 1);
		Wenhao=(GameObject)Instantiate (WenhaoPb,newp3,transform.rotation);
		Wenhao.layer = 5;
		Wenhao.GetComponent<SpriteRenderer> ().sortingOrder = 2;
		Wenhao.transform.SetParent (transform);
		Wenhao.SetActive (false);
		Wenhao.name=transform.name+" Wenhao";
	}
	public void setIconMode(int m){
		switch (m) {
		default:
			Tanhao.SetActive (false);
			Zhandou.SetActive (false);
			Wenhao.SetActive (false);
			break;
		case 1:
			Tanhao.SetActive (false);
			Zhandou.SetActive (false);
			Wenhao.SetActive (true);
			break;
		case 2:
			Tanhao.SetActive (true);
			Zhandou.SetActive (false);
			Wenhao.SetActive (false);
			break;
		case 3:
			Tanhao.SetActive (false);
			Zhandou.SetActive (true);
			Wenhao.SetActive (false);
			break;

		}
	}
	public void initStatus(){
		initHeadIcon ();
	}

}
