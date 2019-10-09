using UnityEngine;
using System.Collections;
using Com.game;

public class Model : MonoBehaviour
{
    void Start()
    {
        GameSceneController my = GameSceneController.GetInstance();
        my.setBaseCode(this);
    }
}

namespace Com.game
{
    //实现接口
    public interface IUserAction
    {
        void priest_SOnB();
        void priest_EOnB();
        void devil_SOnB();
        void devil_EOnB();
        void moveBoat();
        void offBoatS();
        void offBoatE();
        void restart();
    }

    //public enum State { START, STOE, ETOS, END, WIN, LOSE }; //游戏的六种当前状态

        //裁判类，向控制器发送游戏信息
    public interface Judgement
    {
        void Set_move(bool s);
        bool Is_moving();
        void Set_message(string s);
        string Is_message();
    }

    public class GameSceneController : System.Object, IUserAction, Judgement
    {

        private static GameSceneController instance;
        private Model base_code;
        private Controller game_obj;
        private bool move = false;
        private string mes = "";

        public static GameSceneController GetInstance()
        {
            if (null == instance)
            {
                instance = new GameSceneController();
            }
            return instance;
        }

        public Model getBaseCode()
        {
            return base_code;
        }

        internal void setBaseCode(Model c)
        {
            if (null == base_code)
            {
                base_code = c;
            }
        }

        public Controller getGenGameObject()
        {
            return game_obj;
        }

        internal void setGenGameObject(Controller go)
        {
            if (null == game_obj)
            {
                game_obj = go;
            }
        }

        public void priest_SOnB()
        {
            game_obj.priestStartOnBoat();
        }

        public void priest_EOnB()
        {
            game_obj.priestEndOnBoat();
        }

        public void devil_SOnB()
        {
            game_obj.devilStartOnBoat();
        }

        public void offBoatS()
        {
            game_obj.getOffTheBoat(1);
        }

        public void offBoatE()
        {
            game_obj.getOffTheBoat(0);
        }

        public void devil_EOnB()
        {
            game_obj.devilEndOnBoat();
        }

        public void moveBoat()
        {
            game_obj.moveBoat();
        }

        public void Set_message(string m)
        {
            mes = m;
        }

        public bool Is_moving()
        {
            return move;
        }

        public void Set_move(bool s)
        {
            move = s;
        }

        public string Is_message()
        {
            return mes;
        }

        public void restart()
        {
            move = false;
            mes = "";
            Application.LoadLevel(Application.loadedLevelName);
            //state = State.START;
        }
    }

    public interface ActionCompleted
    {
        void OnActionCompleted(UAction a);
    }

    //动作管理接口
    public class ActionManager : System.Object
    {
        private static ActionManager ins;

        public static ActionManager get_instance()
        {
            if (ins == null)
            {
                ins = new ActionManager();
            }
            return ins;
        }


        public UAction ApplyMoveToAction(GameObject obj, Vector3 target, float speed, ActionCompleted completed)
        {
            MoveToAction ac = obj.AddComponent<MoveToAction>();
            ac.setting(target, speed, completed);
            return ac;
        }

        public UAction ApplyMoveToAction(GameObject obj, Vector3 target, float speed)
        {
            return ApplyMoveToAction(obj, target, speed, null);
        }

        //对象在平面上的平移动作
        public UAction ApplyMoveToTrans(GameObject obj, Vector3 target, float speed, ActionCompleted completed)
        {
            MoveToTrans ac = obj.AddComponent<MoveToTrans>();
            ac.setting(obj, target, speed, completed);
            return ac;
        }

        public UAction ApplyMoveToTrans(GameObject obj, Vector3 target, float speed)
        {
            return ApplyMoveToTrans(obj, target, speed, null);
        }
    }

    public class UActionException : System.Exception { }

    public class UAction : MonoBehaviour
    {
        public void Free()
        {
            Destroy(this);
        }
    }

    public class UActionAuto : UAction { }

    //public class UActionMan : UAction { }

    public class MoveToAction : UActionAuto
    {
        public Vector3 target;
        public float speed;

        private ActionCompleted monitor = null;

        public void setting(Vector3 target, float speed, ActionCompleted monitor)
        {
            this.target = target;
            this.speed = speed;
            this.monitor = monitor;
            GameSceneController.GetInstance().Set_move(true);
        }

        void Update()
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target, step);

            if (transform.position == target)
            {
                GameSceneController.GetInstance().Set_move(false);
                if (monitor != null)
                {
                    monitor.OnActionCompleted(this);
                }
                Destroy(this);
            }
        }
    }

    /* MoveToTrans is a combination of two MoveToAction(s)
	 * It moves on a single shaft(Y or Z) each time
	 */
    public class MoveToTrans : UActionAuto, ActionCompleted
    {
        public GameObject obj;
        public Vector3 target;
        public float speed;

        private ActionCompleted monitor = null;

        public void setting(GameObject obj, Vector3 target, float speed, ActionCompleted monitor)
        {
            this.obj = obj;
            this.target = target;
            this.speed = speed;
            this.monitor = monitor;
            GameSceneController.GetInstance().Set_move(true);

            /*obj比target高，先移动target.z, 再移动target.y
              obj比target低，先移动target.y，再移动target.z
             */
            if (target.y < obj.transform.position.y)
            {
                Vector3 targetZ = new Vector3(target.x, obj.transform.position.y, target.z);
                ActionManager.get_instance().ApplyMoveToAction(obj, targetZ, speed, this);
            }
            else
            {
                Vector3 targetY = new Vector3(target.x, target.y, obj.transform.position.z);
                ActionManager.get_instance().ApplyMoveToAction(obj, targetY, speed, this);
            }
        }

        public void OnActionCompleted(UAction action)
        {
            //用回调函数调用下一个动作
            ActionManager.get_instance().ApplyMoveToAction(obj, target, speed, null);
        }

        void Update()
        {
            if (obj.transform.position == target)
            {
                GameSceneController.GetInstance().Set_move(false);
                if (monitor != null)
                {
                    monitor.OnActionCompleted(this);
                }
                Destroy(this);
            }
        }
    }
}
