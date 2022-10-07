using MelonLoader;
using UnityEngine;
using System.Reflection;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Chaos_Edition
{
    public class Class1 : MelonMod
    {
        internal bool inLevel;
        private float intensity = -1;
        private float nextEvent = 1;
        private Camera cam;

        private float speed = 25;

        private float redgemTimer;
        private float newTimeSpeed = 1;

        private bool second;
        private PlayerBase oldPlayer;

        private GameData.GlobalData gemData;
        private int[] obtainedGems;
        private List<bool> enabledGems;
        private bool gotGems;

        private Transform uiHolder;
        private Text intenUI;
        private string intenString;
        private Font font;

        private bool menuListener;

        private bool debug = false;

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);

            if (sceneName.EndsWith("_sn") || sceneName.EndsWith("_tl") || sceneName.EndsWith("_sd") || sceneName.EndsWith("_rg"))
            {
                inLevel = true;
                obtainedGems = new int[0];
                enabledGems = new List<bool>();
                gotGems = false;
                intenUI = null;
                uiHolder = null;
                menuListener = false;
            } 
            else
            {
                inLevel = false;
                newTimeSpeed = 1;
                speed = 25;
                second = false;
                obtainedGems = null;
                enabledGems = null;
                intenUI = null;
                uiHolder = null;
                menuListener = false;

                if (sceneName == "MainMenu" || sceneName == "SelectStage")
                {
                    switch (sceneName)
                    {
                        case "MainMenu":
                            uiHolder = GameObject.Find("Canvas").transform;
                            if (font == null)
                            {
                                font = GameObject.Find("single_player").GetComponent<Text>().font;
                            }
                            break;
                        case "SelectStage":
                            uiHolder = GameObject.Find("CanvasSelectStage").transform;
                            break;
                    }
                    
                    intenUI = UnityEngine.Object.Instantiate(new GameObject(), uiHolder).AddComponent<Text>();
                    intenUI.rectTransform.offsetMax = new Vector2(-23, 198.5f);
                    intenUI.rectTransform.offsetMin = new Vector2(-457, 168.5f);
                    intenUI.alignment = TextAnchor.MiddleLeft;
                    intenUI.fontSize = 27;
                    intenUI.font = font;
                    intenUI.color = Color.black;
                    intenUI.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    intenUI.rectTransform.anchoredPosition = new Vector2(-240f, 183.5f);
                    intenUI.rectTransform.sizeDelta = new Vector2(750f, 30f);
                    intenUI.transform.localPosition = new Vector3(-82f, 320.83f, 0);
                    switch (intensity)
                    {
                        case 7:
                            intenString = "Low";
                            break;
                        case 4:
                            intenString = "Normal";
                            break;
                        case 1:
                            intenString = "High";
                            break;
                        case 0.5f:
                            intenString = "Very High";
                            break;
                        case 0:
                            intenString = "Max";
                            break;
                        case -1:
                            intenString = "Off";
                            break;
                    }
                    intenUI.text = "Current Chaos Intensity: " + intenString;

                    Text changetext = UnityEngine.Object.Instantiate(new GameObject(), uiHolder).AddComponent<Text>();
                    changetext.rectTransform.offsetMax = new Vector2(-23, 198.5f);
                    changetext.rectTransform.offsetMin = new Vector2(-457, 168.5f);
                    changetext.alignment = TextAnchor.MiddleLeft;
                    changetext.fontSize = 13;
                    changetext.font = font;
                    changetext.color = Color.black;
                    changetext.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    changetext.rectTransform.anchoredPosition = new Vector2(-240f, 183.5f);
                    changetext.rectTransform.sizeDelta = new Vector2(750f, 30f);
                    changetext.transform.localPosition = new Vector3(-82f, 299.55f, 0);
                    changetext.text = "Press F1 To Change Intensity";

                    menuListener = true;
                }
            }
            LoggerInstance.Msg(sceneName + " " + inLevel);
        }

        public override void OnFixedUpdate()
        {
            if (!inLevel) return;

            PlayerBase pb = GameObject.FindObjectOfType<PlayerBase>();

            Type playerb = typeof(PlayerBase);

            FieldInfo field = playerb.GetField("MaximumSpeed", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(pb, speed);

            Time.timeScale = newTimeSpeed;
        }

        public override void OnLateUpdate()
        {
            if (menuListener && Input.GetKeyDown(KeyCode.F1))
            {
                switch (intensity)
                {
                    case 7:
                        intensity = 4;
                        intenString = "Normal";
                        break;
                    case 4:
                        intensity = 1;
                        intenString = "High";
                        break;
                    case 1:
                        intensity = 0.5f;
                        intenString = "Very High";
                        break;
                    case 0.5f:
                        intensity = 0;
                        intenString = "Max";
                        break;
                    case 0:
                        intensity = -1;
                        intenString = "Off";
                        break;
                    case -1:
                        intensity = 7;
                        intenString = "Low";
                        break;
                }
                intenUI.text = "Current Chaos Intensity: " + intenString;
                nextEvent = intensity;
            }

            if (!inLevel || intensity == -1) return;

            if (cam == null)
            {
                PlayerCamera c2 = GameObject.FindObjectOfType<PlayerCamera>();
                cam = c2.GetComponent<Camera>();
            }

            if (!gotGems)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player.GetComponent<SonicNew>())
                {
                    gemData = new GameData.GlobalData();
                    Type t = player.GetComponent<SonicNew>().GetType();
                    FieldInfo field = t.GetField("GemData", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.GetField);
                    gemData = (GameData.GlobalData)field.GetValue(player.GetComponent<SonicNew>());
                    obtainedGems = gemData.ObtainedGems.ToArray();
                    for (int i = 0; i < obtainedGems.Length; i++)
                    {
                        enabledGems.Add(new bool());
                        enabledGems[i] = true;
                    }

                    if (obtainedGems.Length > 0) gotGems = true;
                }
            }

            if (debug)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeSize();
                if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeCharacter();
                if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeSpeed();
                if (Input.GetKeyDown(KeyCode.Alpha4)) RedGem();
                if (Input.GetKeyDown(KeyCode.Alpha5)) SpawnSecond();
                if (Input.GetKeyDown(KeyCode.Alpha6)) RemoveGems();
                if (Input.GetKeyDown(KeyCode.Alpha7)) Stop();
                if (Input.GetKeyDown(KeyCode.Alpha8)) SuperSpring();
                if (Input.GetKeyDown(KeyCode.Alpha9)) SpawnEnemy();
                if (Input.GetKeyDown(KeyCode.Alpha0)) SpawnBoxTomb();
                if (Input.GetKeyDown(KeyCode.O)) SpawnExplosiveRain();
                if (Input.GetKeyDown(KeyCode.I)) CheckTeleport();
                if (Input.GetKeyDown(KeyCode.U)) MoveEnemy();
                if (Input.GetKeyDown(KeyCode.Y)) ChangeRings();
                if (Input.GetKeyDown(KeyCode.T)) HalfScore();

                if (intensity == 7) return;
            }

            if (nextEvent > 0)
            {
                nextEvent -= Time.deltaTime;
                return;
            }

            nextEvent = intensity;

            int imsorry = UnityEngine.Random.Range(0, 20);

            if (redgemTimer > 0)
            {
                redgemTimer -= Time.deltaTime;
            }
            else if (redgemTimer > -1)
            {
                newTimeSpeed = 1;
                redgemTimer = -2;
            }

            switch (imsorry)
            {
                case 0:
                    ChangeSize();
                break;
                case 1:
                    ChangeCharacter();
                break;
                case 2:
                    ChangeSpeed();
                    break;
                case 3:
                    RedGem();
                    break;
                case 4:
                    SpawnSecond();
                    break;
                case 5:
                    RemoveGems();
                    break;
                case 6:
                    Stop();
                    break;
                case 7:
                    SuperSpring();
                    break;
                case 8:
                    SpawnEnemy();
                    break;
                case 9:
                    SpawnBoxTomb();
                    break;
                case 10:
                    SpawnExplosiveRain();
                    break;
                case 11:
                    CheckTeleport();
                    break;
                case 12:
                    MoveEnemy();
                    break;
                case 13:
                    ChangeRings();
                    break;
                case 14:
                    HalfScore();
                    break;
                default:
                return;
            }
        }

        private void Stop()
        {
            PlayerBase pb = GameObject.FindObjectOfType<PlayerBase>();
            LoggerInstance.Msg(pb._Rigidbody.velocity);
            pb._Rigidbody.velocity = pb._Rigidbody.velocity * -1;

            Type playerb = typeof(PlayerBase);
            FieldInfo field = playerb.GetField("TargetDirection", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.GetField);
            Vector3 tDir = (Vector3)field.GetValue(pb);
            tDir *= -1;
            field.SetValue(pb, tDir);
            field = playerb.GetField("WorldVelocity", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.GetField);
            tDir = (Vector3)field.GetValue(pb);
            tDir *= -1;
            field.SetValue(pb, tDir);
            field = playerb.GetField("CurSpeed", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.GetField);
            float cur = (float)field.GetValue(pb);
            cur *= -1;
            field.SetValue(pb, cur);
        }

        private void ChangeCharacter()
        {
            int newChar = UnityEngine.Random.Range(0, 10);

            PlayerBase pb = GameObject.FindObjectOfType<PlayerBase>();

            switch (newChar)
            {
                case 0:
                    CharChanger("sonic_new", 0, 0);
                    break;
                case 1:
                    CharChanger("shadow", 5, 0);
                    break;
                case 2:
                    CharChanger("silver", 1, 0);
                    break;
                case 3:
                    CharChanger("tails", 2, 0);
                    break;
                case 4:
                    CharChanger("knuckles", 3, 0);
                    break;
                case 5:
                    CharChanger("rouge", 7, 0);
                    break;
                case 6:
                    CharChanger("omega", 6, 0);
                    break;
                case 7:
                    CharChanger("sonic_fast", 0, 0);
                    break;
                case 8:
                    CharChanger("princess", 4, 0);
                    break;
                case 9:
                    CharChanger("snow_board", 0, 0);
                    break;
            }

            UnityEngine.Object.Destroy(pb.gameObject);
        }

        private void CharChanger(string name, int ID, float xOff)
        {
            PlayerBase pb = GameObject.FindObjectOfType<PlayerBase>();
            Vector3 vel = pb._Rigidbody.velocity;
            Vector3 pos = pb.transform.position;
            pos.x += xOff;
            PlayerBase newchar = (UnityEngine.Object.Instantiate(Resources.Load("DefaultPrefabs/Player/" + name), pos, pb.transform.rotation) as GameObject).GetComponent<PlayerBase>();
            newchar.SetPlayer(ID, name);
            newchar._Rigidbody.velocity = vel;
            newchar.StartPlayer(false);
        }

        private void RemoveGems()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Type t = player.GetComponent<SonicNew>().GetType();
            FieldInfo field = t.GetField("ActiveGem", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.GetField);
            field.SetValue(player.GetComponent<SonicNew>(), SonicNew.Gem.None);

            if (enabledGems.Count == 1) return;
            int val = UnityEngine.Random.Range(0, obtainedGems.Length);
            if (val == 0) return;
            if (enabledGems[val] == true)
            {
                gemData.ObtainedGems[val] = 0;
                enabledGems[val] = false;
            } else
            {
                gemData.ObtainedGems[val] = obtainedGems[val];

                enabledGems[val] = true;
            }
            field = t.GetField("GemData", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.GetField);
            field.SetValue(player.GetComponent<SonicNew>(), gemData);
        }

        private void SpawnEnemy()
        {
            int enemyID = UnityEngine.Random.Range(0, 10);

            GameObject enemy = new GameObject();

            switch (enemyID)
            {
                case 0:
                    enemy = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/enemy/eGunner"));
                    enemy.GetComponent<eGunner>().SetParameters(false, true, "", 0f, "", Vector3.zero);
                    int gunnerMode = UnityEngine.Random.Range(0, 7);

                    switch (gunnerMode)
                    {
                        case 0:
                            enemy.GetComponent<eGunner>().RobotMode = eGunner.Mode.Chase;
                            break;
                        case 1:
                            enemy.GetComponent<eGunner>().RobotMode = eGunner.Mode.Fix;
                            break;
                        case 2:
                            enemy.GetComponent<eGunner>().RobotMode = eGunner.Mode.Fix_Vulcan;
                            break;
                        case 3:
                            enemy.GetComponent<eGunner>().RobotMode = eGunner.Mode.Fix_Rocket;
                            break;
                        case 4:
                            enemy.GetComponent<eGunner>().RobotMode = eGunner.Mode.Fix_Missile;
                            break;
                        case 5:
                            enemy.GetComponent<eGunner>().RobotMode = eGunner.Mode.Normal;
                            break;
                        case 6:
                            enemy.GetComponent<eGunner>().RobotMode = eGunner.Mode.Trans;
                            break;
                        default:
                            enemy.GetComponent<eGunner>().RobotMode = eGunner.Mode.Chase;
                            break;
                    }

                    break;
                case 1:
                    enemy = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/enemy/ctaker"));
                    enemy.GetComponent<cTaker>().CreatureMode = cTaker.Mode.Normal;
                    break;
                case 2:
                    enemy = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/enemy/cbiter"));
                    enemy.GetComponent<cBiter>().CreatureMode = cBiter.Mode.Normal;
                    break;
                case 3:
                    enemy = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/enemy/cgolem"));
                    enemy.GetComponent<cGolem>().CreatureMode = cGolem.Mode.Normal;
                    break;
                case 4:
                    enemy = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/enemy/ccrawler"));
                    enemy.GetComponent<cCrawler>().CreatureMode = cCrawler.Mode.Normal;
                    break;
                case 5:
                    enemy = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/enemy/eCannon"));
                    enemy.GetComponent<eCannon>().RobotMode = eCannon.Mode.Normal;
                    break;
                case 6:
                    enemy = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/enemy/eBomber"));
                    enemy.GetComponent<eBomber>().RobotMode = eBomber.Mode.Fix;
                    break;
                case 7:
                    enemy = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/enemy/eSweeper"));
                    enemy.GetComponent<eBomber>().RobotMode = eBomber.Mode.Fix;
                    break;
                case 8:
                    enemy = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/enemy/eFlyer"));
                    enemy.GetComponent<eFlyer>().RobotMode = eFlyer.Mode.Fix_Rocket;
                    break;
                case 9:
                    enemy = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/enemy/ebluster"));
                    enemy.GetComponent<eFlyer>().RobotMode = eFlyer.Mode.Fix_Vulcan;
                    break;
            }

            float xR = UnityEngine.Random.Range(-10, 11);
            float yR = UnityEngine.Random.Range(0, 11);
            float zR = UnityEngine.Random.Range(-10, 11);

            Transform player = GameObject.FindGameObjectWithTag("Player").transform;

            enemy.transform.position = new Vector3(player.position.x + xR, player.position.y + yR, player.position.z + zR);
        }

        private void Damage()
        {

        }

        private void ChangeSpeed()
        {
            PlayerBase pb = GameObject.FindObjectOfType<PlayerBase>();

            float f = UnityEngine.Random.Range(1, 100);

            Type playerb = typeof(PlayerBase);
            FieldInfo field = playerb.GetField("TopSpeed", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(pb, f);

            speed = f;
        }

        private void SuperSpring()
        {
            if (UnityEngine.Random.Range(0, 4) == 1) return;
            if(intensity <= 0.5f && UnityEngine.Random.Range(0, 4) != 1) return;
            PlayerBase pb = GameObject.FindObjectOfType<PlayerBase>();
            GameObject spring = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/objects/common/spring"), pb.transform.position, pb.transform.rotation);
            spring.GetComponent<Spring>().SetParameters(100, 10, new Vector3(pb.transform.position.x, pb.transform.position.y + 1000, pb.transform.position.z));
            spring.GetComponent<Spring>().SpringMode = 0;
        }

        private void SpawnSecond()
        {
            int newChar = UnityEngine.Random.Range(0, 10);

            PlayerBase pb = GameObject.FindObjectOfType<PlayerBase>();
            if (!second)
            {
                switch (newChar)
                {
                    case 0:
                        CharChanger("sonic_new", 0, 1);
                        break;
                    case 1:
                        CharChanger("shadow", 5, 1);
                        break;
                    case 2:
                        CharChanger("silver", 1, 1);
                        break;
                    case 3:
                        CharChanger("tails", 2, 1);
                        break;
                    case 4:
                        CharChanger("knuckles", 3, 1);
                        break;
                    case 5:
                        CharChanger("rouge", 7, 1);
                        break;
                    case 6:
                        CharChanger("omega", 6, 1);
                        break;
                    case 7:
                        CharChanger("sonic_fast", 0, 1);
                        break;
                    case 8:
                        CharChanger("princess", 4, 1);
                        break;
                    case 9:
                        CharChanger("snow_board", 0, 1);
                        break;
                }
                second = true;
                oldPlayer = pb;
            }
            else
            {
                if (UnityEngine.Random.Range(1, 5) == 2)
                {
                    UnityEngine.Object.Destroy(oldPlayer);
                    second = false;
                }
            }
        }

        private void ChangeSize()
        {
            Transform tran = GameObject.FindObjectOfType<PlayerBase>().transform;
            float newsize = UnityEngine.Random.Range(0.2f, 2.5f);
            tran.localScale = new Vector3(newsize, newsize, newsize);
        }

        private void SpawnBoxTomb()
        {
            Transform tran = GameObject.FindObjectOfType<PlayerBase>().transform;
            GameObject box = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/objects/common/WoodBox"));
            box.transform.position = new Vector3(tran.position.x, tran.position.y + 1, tran.position.z + 2);
            box = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/objects/common/WoodBox"));
            box.transform.position = new Vector3(tran.position.x, tran.position.y + 1, tran.position.z - 2);
            box = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/objects/common/WoodBox"));
            box.transform.position = new Vector3(tran.position.x + 2, tran.position.y + 1, tran.position.z);
            box = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/objects/common/WoodBox"));
            box.transform.position = new Vector3(tran.position.x - 2, tran.position.y + 1, tran.position.z);
            box = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/objects/common/WoodBox"));
            box.transform.position = new Vector3(tran.position.x, tran.position.y + 3, tran.position.z + 1f);
            box = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/objects/common/WoodBox"));
            box.transform.position = new Vector3(tran.position.x, tran.position.y + 3, tran.position.z - 1f);
        }

        private void RedGem()
        {
            redgemTimer = 10;
            newTimeSpeed = UnityEngine.Random.Range(0f, 2f);
        }

        private void SpawnExplosiveRain()
        {
            Transform tran = GameObject.FindObjectOfType<PlayerBase>().transform;
            GameObject box = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/objects/common/BombBox"));
            box.transform.position = new Vector3(tran.position.x, tran.position.y + 5, tran.position.z + 6);
            box = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/objects/common/BombBox"));
            box.transform.position = new Vector3(tran.position.x, tran.position.y + 5, tran.position.z + 2);
            box = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/objects/common/BombBox"));
            box.transform.position = new Vector3(tran.position.x, tran.position.y + 5, tran.position.z - 2);
            box = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/objects/common/BombBox"));
            box.transform.position = new Vector3(tran.position.x, tran.position.y + 5, tran.position.z - 6);
            box = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/objects/common/BombBox"));
            box.transform.position = new Vector3(tran.position.x + 6, tran.position.y + 5, tran.position.z);
            box = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/objects/common/BombBox"));
            box.transform.position = new Vector3(tran.position.x - 6, tran.position.y + 5, tran.position.z);
            box = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/objects/common/BombBox"));
            box.transform.position = new Vector3(tran.position.x + 2, tran.position.y + 5, tran.position.z);
            box = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/objects/common/BombBox"));
            box.transform.position = new Vector3(tran.position.x - 2, tran.position.y + 5, tran.position.z);
            box = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/objects/common/BombBox"));
            box.transform.position = new Vector3(tran.position.x + 4, tran.position.y + 5, tran.position.z + 4);
            box = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/objects/common/BombBox"));
            box.transform.position = new Vector3(tran.position.x - 4, tran.position.y + 5, tran.position.z + 4);
            box = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/objects/common/BombBox"));
            box.transform.position = new Vector3(tran.position.x + 4, tran.position.y + 5, tran.position.z - 4);
            box = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("defaultprefabs/objects/common/BombBox"));
            box.transform.position = new Vector3(tran.position.x - 4, tran.position.y + 5, tran.position.z - 4);
        }

        private void CheckTeleport()
        {
            Checkpoint[] points = GameObject.FindObjectsOfType<Checkpoint>();
            int point = UnityEngine.Random.Range(0, points.Length);
            GameObject.FindObjectOfType<PlayerBase>().transform.position = points[point].transform.position;
        }

        private void MoveEnemy()
        {
            Transform tran = GameObject.FindObjectOfType<PlayerBase>().transform;
            GameObject[] enemies = FindGameObjectsWithLayer(11);
            int en = UnityEngine.Random.Range(0, enemies.Length);
            enemies[en].transform.position = new Vector3(tran.position.x + 4, tran.position.y, tran.position.z);
            en = UnityEngine.Random.Range(0, enemies.Length);
            enemies[en].transform.position = new Vector3(tran.position.x - 4, tran.position.y, tran.position.z);
            en = UnityEngine.Random.Range(0, enemies.Length);
            enemies[en].transform.position = new Vector3(tran.position.x, tran.position.y, tran.position.z + 4);
            en = UnityEngine.Random.Range(0, enemies.Length);
            enemies[en].transform.position = new Vector3(tran.position.x, tran.position.y, tran.position.z - 4);
        }
        private GameObject[] FindGameObjectsWithLayer(int layer){
            var goArray = GameObject.FindObjectsOfType<GameObject>();
            var goList = new List<GameObject>();
            for (int i = 0; i < goArray.Length; i++) {
                if (goArray[i].layer == layer) {
                goList.Add(goArray[i]);
                }
            }
            if (goList.Count == 0)
            {
                return null;
            }
            return goList.ToArray();
        }

        private void ChangeRings()
        {
            int newCount = UnityEngine.Random.Range(-100, 1000);
            if (newCount <= 0)
            {
                Singleton<GameManager>.Instance._PlayerData.rings = 0;
                return;
            }
            Singleton<GameManager>.Instance._PlayerData.rings = newCount;
        }

        private void HalfScore()
        {
            Singleton<GameManager>.Instance._PlayerData.score = Singleton<GameManager>.Instance._PlayerData.score / 2;
        }
    }
}
