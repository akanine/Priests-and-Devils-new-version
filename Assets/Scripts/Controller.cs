using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com.game;

public class Controller : MonoBehaviour
{

    GameObject[] boat = new GameObject[2];
    GameObject boat_obj;
    GameSceneController my;
    Stack<GameObject> p_start = new Stack<GameObject>();
    Stack<GameObject> p_end = new Stack<GameObject>();
    Stack<GameObject> d_start = new Stack<GameObject>();
    Stack<GameObject> d_end = new Stack<GameObject>();
    Vector3 bankStart = new Vector3(0, 0, -12);
    Vector3 priestStart = new Vector3(0, 2.8f, -11f);
    Vector3 devilStart = new Vector3(0, 2.8f, -16f);
    Vector3 priestEnd = new Vector3(0, 2.8f, 8f);
    Vector3 devilEnd = new Vector3(0, 2.8f, 13f);
    Vector3 boatStart = new Vector3(0, 0, -4);
    Vector3 bankEnd = new Vector3(0, 0, 12);
    Vector3 boatEnd = new Vector3(0, 0, 4);
    Vector3 River = new Vector3(0, -1.6f, 0f);
    Vector3 rightBpos = new Vector3(0, 1.2f, -1.2f);
    Vector3 leftBpos = new Vector3(0, 1.2f, 1.2f);
    public float speed = 50f;
    int side = 1;

    void Start()
    {
        my = GameSceneController.GetInstance();
        my.setGenGameObject(this);
        Screen();
    }

    //实例化游戏对象
    void Screen()
    {
        Instantiate(Resources.Load("Prefabs/Bank"), bankStart, Quaternion.identity);
        Instantiate(Resources.Load("Prefabs/Bank"), bankEnd, Quaternion.identity);
        Instantiate(Resources.Load("Prefabs/River"), River, Quaternion.identity);
        boat_obj = Instantiate(Resources.Load("Prefabs/Boat"), boatStart, Quaternion.identity) as GameObject;

        for (int i = 0; i < 3; ++i)
        {
            GameObject priest = Instantiate(Resources.Load("Prefabs/Priest")) as GameObject;
            priest.transform.position = getCharacterPosition(priestStart, i);
            priest.tag = "Priest";
            p_start.Push(priest);
            GameObject devil = Instantiate(Resources.Load("Prefabs/Devil")) as GameObject;
            devil.transform.position = getCharacterPosition(devilStart, i);
            priest.tag = "Priest";
            d_start.Push(devil);
        }
    }

    //检查游戏状态
    void check()
    {
        GameSceneController scene = GameSceneController.GetInstance();
        int p_on_Bank = 0, d_on_Bank = 0;
        int p_s = 0, d_s = 0, p_e = 0, d_e = 0;

        if (p_end.Count == 3 && d_end.Count == 3)
        {
            scene.Set_message("Win!");
            return;
        }

        for (int i = 0; i < 2; ++i)
        {
            if (boat[i] != null && boat[i].tag == "Priest")
                p_on_Bank++; //给牧师和恶魔添加Tag，以区分
            else if (boat[i] != null && boat[i].tag == "Devil")
                d_on_Bank++;
        }
        if (side == 1)
        {
            p_e = p_end.Count;
            d_e = d_end.Count;
            p_s = p_start.Count + p_on_Bank;
            d_s = d_start.Count + d_on_Bank;
        }
        else if (side == 2)
        {
            p_e = p_on_Bank + p_end.Count;
            d_e = d_on_Bank + d_end.Count;
            p_s = p_start.Count;
            d_s = d_start.Count;
        }
        if ((p_e < d_e && p_e != 0) || (p_s < d_s && p_s != 0))
        {
            scene.Set_message("Lose!");
        }
    }

        int boatCapacity()
    {
        int capacity = 0;
        for (int i = 0; i < 2; ++i)
        {
            if (boat[i] == null) capacity++;
        }
        return capacity;
    }
    //动作上船
    void getOnBoat(GameObject obj)
    {
        if (boatCapacity() != 0)
        {
            obj.transform.parent = boat_obj.transform; //把船设置为游戏角色的子对象
            Vector3 target = new Vector3();
            if (boat[0] == null)
            {
                boat[0] = obj;
                target = boat_obj.transform.position + leftBpos;
            }
            else
            {
                boat[1] = obj;
                target = boat_obj.transform.position + rightBpos;
            }
            ActionManager.get_instance().ApplyMoveToTrans(obj, target, speed);
        }
    }
    //动作开船
    public void moveBoat()
    {
        if (boatCapacity() != 2)
        {
            if (side == 1)
            {
                ActionManager.get_instance().ApplyMoveToAction(boat_obj, boatEnd, speed);
                side = 2;
            }
            else if (side == 2)
            {
                ActionManager.get_instance().ApplyMoveToAction(boat_obj, boatStart, speed);
                side = 1;
            }
        }
    }

    //动作下船
    public void getOffTheBoat(int s)
    {
        if (boat[s] != null)
        {
            Vector3 target = new Vector3();
            boat[s].transform.parent = null; //取消船和角色的父子关系
            if (side == 1)
            {
                if (boat[s].tag == "Priest")
                {
                    p_start.Push(boat[s]);
                    target = getCharacterPosition(priestStart, p_start.Count - 1);
                }
                else if (boat[s].tag == "Devil")
                {
                    d_start.Push(boat[s]);
                    target = getCharacterPosition(devilStart, d_start.Count - 1);
                }
            }
            else if (side == 2)
            {
                if (boat[s].tag == "Priest")
                {
                    p_end.Push(boat[s]);
                    target = getCharacterPosition(priestEnd, p_end.Count - 1);
                }
                else if (boat[s].tag == "Devil")
                {
                    d_end.Push(boat[s]);
                    target = getCharacterPosition(devilEnd, d_end.Count - 1);
                }
            }
            ActionManager.get_instance().ApplyMoveToTrans(boat[s], target, speed);
            boat[s] = null;
        }
    }

    void set_Position(Stack<GameObject> stack, Vector3 pos)
    {
        GameObject[] array = stack.ToArray();
        for (int i = 0; i < stack.Count; ++i)
        {
            array[i].transform.position = new Vector3(pos.x, pos.y, pos.z + 1.5f * i);
        }
    }

    Vector3 getCharacterPosition(Vector3 pos, int index)
    {
        return new Vector3(pos.x, pos.y, pos.z + 1.5f * index);
    }

    public void priestStartOnBoat()
    {
        if (side == 1 && p_start.Count != 0 && boatCapacity() != 0)
            getOnBoat(p_start.Pop());
    }

    public void devilStartOnBoat()
    {
        if (side == 1 && d_start.Count != 0 && boatCapacity() != 0)
            getOnBoat(d_start.Pop());
    }

    public void priestEndOnBoat()
    {
        if (side == 2 && p_end.Count != 0 && boatCapacity() != 0)
            getOnBoat(p_end.Pop());
    }

    public void devilEndOnBoat()
    {
        if (side == 2 && d_end.Count != 0 && boatCapacity() != 0)
            getOnBoat(d_end.Pop());
    }

    void Update()
    {        check();
    }
}
