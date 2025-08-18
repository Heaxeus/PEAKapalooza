using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using pworld.Scripts.Extensions;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using Zorro.Core;
using Photon.Pun;
using Zorro.PhotonUtility;
using Photon.Realtime;
using System.Collections;
using UnityEditor.Analytics;

namespace PEAKapalooza;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("PEAK.exe")]
public class PEAKapalooza : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    public static bool debug = false;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        SceneManager.sceneLoaded += OnSceneChange;

        Harmony.CreateAndPatchAll(typeof(PEAKapalooza));
    }

     
    public static bool c = false;

    public void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha1) && c == false)
        {
            c = true;

            Character.localCharacter.WarpPlayer(MapHandler.Instance.segments[4].reconnectSpawnPos.position, true);
        }else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha2) && c == false)
            {
                c = true;

                GameObject.Find("FogSphereSystem").SetActive(false);
            }
        if (debug)
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha1) && c == false)
            {
                c = true;

                MapHandler.JumpToSegment(Segment.Beach);
            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha2) && c == false)
            {
                c = true;

                MapHandler.JumpToSegment(Segment.Tropics);
            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha3) && c == false)
            {
                c = true;

                MapHandler.JumpToSegment(Segment.Alpine);
            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha4) && c == false)
            {
                c = true;


            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha5) && c == false)
            {
                c = true;

                MapHandler.JumpToSegment(Segment.Caldera);
            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha6) && c == false)
            {
                c = true;

                MapHandler.JumpToSegment(Segment.TheKiln);
            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha7) && c == false)
            {
                c = true;

                MapHandler.JumpToSegment(Segment.Peak);
            }

        }
        if (!Input.GetKey(KeyCode.LeftControl))
            {
                c = false;
            }
    }



    


    [HarmonyPatch(typeof(PointPinger), "ReceivePoint_Rpc")]
    [HarmonyPostfix]
    public static void Postfix_PingWarp_ReceivePoint_Rpc(PointPinger __instance)
    {
        if (debug)
        {
            RaycastHit raycastHit;
            if (Camera.main.ScreenPointToRay(Input.mousePosition).Raycast(out raycastHit, HelperFunctions.LayerType.TerrainMap.ToLayerMask(), -1f) && __instance.photonView.IsMine)
            {
                Logger.LogMessage(raycastHit.point);
                __instance.character.WarpPlayer(raycastHit.point, true);

            }
        }
    }


    public static bool initialSegmentJump = false;

    [HarmonyPatch(typeof(RunManager), "StartRun")]
    [HarmonyPrefix]
    public static bool Postfix_Abseiling_StartPassedOutOnTheBeach(RunManager __instance)
    {
        if (toggleAbseiling)
        {
            Logger.LogMessage("Intial Method");
            __instance.runStarted = true;
            __instance.StartCoroutine(Test());
            
            currentSegment = 3;
            return false;
        }
        return true;
       
    }


    public static IEnumerator Test()
    {
        yield return new WaitForSeconds(5);
        MapHandler.JumpToSegment(Segment.TheKiln);
        yield return new WaitForSeconds(5);
        foreach (Character character in PlayerHandler.GetAllPlayerCharacters())
        {
            yield return new WaitForSeconds(1);
            //character.WarpPlayer(new Vector3(0f, 350f, -150f), true);
            //character.WarpPlayer(new Vector3(0f, 300f, -150f), true);
            character.WarpPlayer(new Vector3(16f, 1235f, 2239f), true);
            //character.WarpPlayer(GameObject.Find("Map/Biome_4/Volcano/Peak/Flag_planted_seagull").transform.localPosition, true);
                yield return new WaitForSeconds(1);
        }
        //Destroy(GameObject.Find("Map/Biome_4/Volcano/Peak/Box"));
            yield return new WaitForSeconds(3);
            GameObject.Find("FogSphereSystem").SetActive(false);

    }


    public static int currentSegment = 3;



    [HarmonyPatch(typeof(Campfire), "Light_Rpc")]
    [HarmonyPrefix]
    public static bool Prefix_PingWarp_Light_Rpc(Campfire __instance)
    {
        if (toggleAbseiling)
        {
            __instance.state = Campfire.FireState.Lit;
            __instance.UpdateLit();
            __instance.smokeParticlesOff.Stop();
            __instance.smokeParticlesLit.Play();
            GUIManager.instance.RefreshInteractablePrompt();

            if (currentSegment == 3)
            {
                currentSegment--;
                MapHandler.JumpToSegment(Segment.Caldera);

            }
            else if (currentSegment == 2)
            {
                currentSegment--;
                MapHandler.JumpToSegment(Segment.Alpine);

            }
            else if (currentSegment == 1)
            {
                currentSegment--;
                MapHandler.JumpToSegment(Segment.Tropics);

            }
            else if (currentSegment == 0)
            {
                currentSegment--;
                MapHandler.JumpToSegment(Segment.Beach);
                GameObject.Find("Map/Biome_1/Beach/Beach_Segment/crashed plane").SetActive(false);
            }

            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(MapHandler), "JumpToSegmentLogic")]
    [HarmonyPrefix]
    public static bool Prefix_PingWarp_JumpToSegmentLogic(Segment segment, HashSet<int> playersToTeleport, bool sendToEveryone, MapHandler __instance)
    {
        if (toggleAbseiling)
        {
            Singleton<MapHandler>.Instance.currentSegment = (int)segment;
            Debug.Log(string.Format("Jumping to segment: {0}", segment));
            foreach (MapHandler.MapSegment mapSegment in Singleton<MapHandler>.Instance.segments)
            {
                mapSegment.segmentParent.SetActive(false);
                if (mapSegment.segmentCampfire)
                {
                    mapSegment.segmentCampfire.SetActive(false);
                }
                if (mapSegment.wallNext)
                {
                    mapSegment.wallNext.gameObject.SetActive(false);
                }
                if (mapSegment.wallPrevious)
                {
                    mapSegment.wallPrevious.gameObject.SetActive(false);
                }
            }
            int num = (int)segment;
            if (segment == Segment.Peak)
            {
                num--;
            }
            MapHandler.MapSegment mapSegment2 = Singleton<MapHandler>.Instance.segments[num];
            mapSegment2.segmentParent.SetActive(true);
            if (mapSegment2.segmentCampfire)
            {
                mapSegment2.segmentCampfire.SetActive(true);
            }
            if (mapSegment2.wallNext)
            {
                mapSegment2.wallNext.gameObject.SetActive(true);
            }
            if (mapSegment2.wallPrevious)
            {
                mapSegment2.wallPrevious.gameObject.SetActive(true);
            }
            Vector3 vector = mapSegment2.reconnectSpawnPos.position;
            if (segment == Segment.Peak)
            {
                vector = Singleton<MapHandler>.Instance.respawnThePeak.position;
            }
            if (num > 0)
            {
                MapHandler.MapSegment mapSegment3 = Singleton<MapHandler>.Instance.segments[num - 1];
                if (mapSegment3.segmentCampfire != null)
                {
                    mapSegment3.segmentCampfire.SetActive(true);
                }
            }
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log(string.Format("Spawning items in {0}. Parent: {1}", segment, mapSegment2.segmentParent.gameObject.name));
                ISpawner[] componentsInChildren = mapSegment2.segmentParent.GetComponentsInChildren<ISpawner>();
                int num2 = 0;
                foreach (ISpawner spawner in componentsInChildren)
                {
                    string text = "Spawning...";
                    string text2 = num2.ToString();
                    string text3 = " ";
                    Type type = spawner.GetType();
                    Debug.Log(text + text2 + text3 + ((type != null) ? type.ToString() : null));
                    spawner.TrySpawnItems();
                    num2++;
                }
                if (mapSegment2.segmentCampfire)
                {
                    Debug.Log("Spawning items in " + mapSegment2.segmentCampfire.gameObject.name);
                    ISpawner[] array2 = mapSegment2.segmentCampfire.GetComponentsInChildren<ISpawner>();
                    for (int i = 0; i < array2.Length; i++)
                    {
                        array2[i].TrySpawnItems();
                    }
                }
                else
                {
                    Debug.Log("NO CAMPFIRE SEGMENT");
                }
            }
            
            if (mapSegment2.dayNightProfile != null)
            {
                DayNightManager.instance.BlendProfiles(mapSegment2.dayNightProfile);
            }
            if (!initialSegmentJump)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    Debug.Log(string.Format("Teleporting all players to {0} campfire..", segment));
                    foreach (Character character in PlayerHandler.GetAllPlayerCharacters())
                    {
                        if (playersToTeleport.Contains(character.photonView.Owner.ActorNumber))
                        {
                            character.photonView.RPC("WarpPlayerRPC", RpcTarget.All, new object[] { vector, false });
                        }
                    }
                }
                if (sendToEveryone)
                {
                    CustomCommands<CustomCommandType>.SendPackage(new SyncMapHandlerDebugCommandPackage(segment, Array.Empty<int>()), ReceiverGroup.Others);
                }
                initialSegmentJump = true;
            }

            return false;
        }
        return true;
    }



    //float num = Vector3.Distance(base.transform.position, character.Center);
    public static bool wonGame = false;

    [HarmonyPatch(typeof(Flare), "Update")]
    [HarmonyPrefix]
    public static bool Prefix_PingWarp_Update(Flare __instance)
    {
        if (toggleAbseiling)
        {

            bool value = __instance.GetData<BoolItemData>(DataEntryKey.FlareActive).Value;
            __instance.item.UIData.canPocket = !value;
            if (value && !__instance.trackable.hasTracker)
            {
                __instance.EnableFlareVisuals();
            }
            


            
            if (value && __instance.item.holderCharacter && Vector3.Distance(GameObject.Find("BingBong(Clone)").transform.position, __instance.item.holderCharacter.Center) < 40f && wonGame == false)
            {
                foreach (Character character in PlayerHandler.GetAllPlayerCharacters()) {
                    character.RPCEndGame_ForceWin();
                }
                wonGame = true;
            }
            return false;
        }
        return true;
    }



    [HarmonyPatch(typeof(OrbFogHandler), "WaitToMove")]
    [HarmonyPrefix]
    public static bool Prefix_PingWarp_WaitToMove(OrbFogHandler __instance)
    {
        if (toggleFog)
        {
            __instance.photonView.RPC("StartMovingRPC", RpcTarget.All, Array.Empty<object>());
            return false;
        }
        return true;
    }


    [HarmonyPatch(typeof(Tornado), "Movement")]
    [HarmonyPrefix]
    public static bool Prefix_FasterTornados_Movement(Tornado __instance)
    {
        if (toggleTornadoSpeed)
        {
            if (__instance.target == null)
            {
                return false;
            }
            __instance.vel = FRILerp.Lerp(__instance.vel, (__instance.target.position - __instance.tornadoPos).Flat().normalized * 15f, 0.15f, true);
            __instance.tornadoPos += __instance.vel * 5 * Time.deltaTime;
            RaycastHit groundPosRaycast = HelperFunctions.GetGroundPosRaycast(__instance.tornadoPos + Vector3.up * 200f, HelperFunctions.LayerType.Terrain, 0f);
            if (groundPosRaycast.transform && Vector3.Distance(__instance.tornadoPos, groundPosRaycast.point) < 100f)
            {
                __instance.transform.position = groundPosRaycast.point;
                return false;
            }
            __instance.transform.position = __instance.tornadoPos;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(OrbFogHandler), "Move")]
    [HarmonyPrefix]
    public static bool Prefix_FasterFog_Move(OrbFogHandler __instance)
    {
        if (toggleFog)
        {
            __instance.sphere.REVEAL_AMOUNT = 0f;
            __instance.sphere.ENABLE = Mathf.MoveTowards(__instance.sphere.ENABLE, 1f, Time.deltaTime * 0.1f);
            __instance.currentSize -= __instance.speed *2 * Time.deltaTime;
            if (__instance.currentSize == 0f)
            {
                __instance.Stop();
            }
            return false;
        }
        return true;
    }



    private void OnSceneChange(Scene scene, LoadSceneMode mode)
    {
        Logger.LogMessage(scene.name);
        if (scene.name.StartsWith("Airport"))
        {
            setupComplete = false;
        }
        else if (scene.name.StartsWith("Level_"))
        {
            Peaking();
        }
    }



    public static bool setupComplete = false;
    public static GameObject passportPage;
    public static GameObject pageButtons;
    public static GameObject nextPageButton;
    public static GameObject prevPageButton;
    public static GameObject windChillOptionText;
    public static GameObject windChillOptionButton;
    public static GameObject rainOptionText;
    public static GameObject rainOptionButton;
    public static GameObject tornadoOptionText;
    public static GameObject tornadoOptionButton;
    public static GameObject fogOptionText;
    public static GameObject fogOptionButton;
    public static GameObject tornadoSpeedOptionText;
    public static GameObject tornadoSpeedOptionButton;
    public static GameObject abseilingOptionText;
    public static GameObject abseilingOptionButton;
    
    public static bool toggleRain = false;
    public static bool toggleSnow = false;
    public static bool toggleTornado = false;
    public static bool toggleFog = false;
    public static bool toggleTornadoSpeed = false;
    public static bool toggleAbseiling = false;



    public static int currentPage = 0;

    [HarmonyPatch(typeof(PassportManager), "ToggleOpen")]
    [HarmonyPostfix]
    public static void Postfix_PassportOpen_ToggleOpen(PassportManager __instance)
    {
        if (!setupComplete)
        {
            passportPage = Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel"));
            passportPage.name = "2";
            passportPage.transform.SetParent(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel").transform);
            passportPage.transform.localScale = new Vector3(1, 1, 1);
            passportPage.transform.localPosition = new Vector3(0f, -228.05f, 0f);
            Destroy(GameObject.Find("2/BG/Portrait"));
            Destroy(GameObject.Find("2/BG/Tabs"));
            Destroy(GameObject.Find("2/BG/Options"));
            Destroy(GameObject.Find("2/BG/Text"));
            passportPage.SetActive(false);


            pageButtons = new GameObject();
            pageButtons.name = "PageButtons";
            pageButtons.transform.SetParent(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel").transform);
            pageButtons.transform.localPosition = new Vector3(296.2772f, 602.6318f, 0f);





            nextPageButton = Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close"));
            nextPageButton.name = "NextPageButton";

            nextPageButton.transform.SetParent(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/PageButtons").transform);
            nextPageButton.transform.localScale = new Vector3(0.66f, .66f, .66f);
            nextPageButton.transform.localPosition = new Vector3(-84.4186f, -603.2728f, 0f);


            nextPageButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            nextPageButton.GetComponent<Button>().onClick.AddListener(NextPage);


            DestroyImmediate(GameObject.Find("NextPageButton/Box/Icon").GetComponent<RawImage>());
            GameObject.Find("NextPageButton/Box/Icon").AddComponent<TextMeshProUGUI>();
            GameObject.Find("NextPageButton/Box/Icon").GetComponent<TextMeshProUGUI>().text = ">";



            prevPageButton = Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close"));
            prevPageButton.name = "PrevPageButton";
            prevPageButton.transform.SetParent(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/PageButtons").transform);
            prevPageButton.transform.localScale = new Vector3(0.66f, .66f, .66f);
            prevPageButton.transform.localPosition = new Vector3(-856.7273f, -603.2728f, 0f);

            prevPageButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            prevPageButton.GetComponent<Button>().onClick.AddListener(PrevPage);

            DestroyImmediate(GameObject.Find("PrevPageButton/Box/Icon").GetComponent<RawImage>());
            GameObject.Find("PrevPageButton/Box/Icon").AddComponent<TextMeshProUGUI>();
            GameObject.Find("PrevPageButton/Box/Icon").GetComponent<TextMeshProUGUI>().text = "<";

            prevPageButton.SetActive(false);




            windChillOptionText = Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text"));
            windChillOptionText.name = "WindChillOptionText";
            windChillOptionText.transform.SetParent(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG").transform);
            windChillOptionText.transform.localScale = new Vector3(0.66f, .66f, .66f);
            windChillOptionText.transform.localPosition = new Vector3(-312.232f, 129.5556f, 0f);

            GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/WindChillOptionText").GetComponent<TextMeshProUGUI>().text = "Disable Blizzards:";
            windChillOptionText.SetActive(true);


            windChillOptionButton = Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close"));
            windChillOptionButton.transform.SetParent(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG").transform);
            windChillOptionButton.name = "WindChillOptionButton";

            windChillOptionButton.transform.localScale = new Vector3(.4f, .4f, .4f);
            windChillOptionButton.transform.localPosition = new Vector3(-189.97f, 129.5556f, 0f);

            windChillOptionButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            windChillOptionButton.GetComponent<Button>().onClick.AddListener(WindChillOption);
            GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/WindChillOptionButton/Box/Icon").SetActive(toggleSnow);







            rainOptionText = Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text"));
            rainOptionText.name = "RainOptionText";
            rainOptionText.transform.SetParent(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG").transform);
            rainOptionText.transform.localScale = new Vector3(0.66f, .66f, .66f);
            rainOptionText.transform.localPosition = new Vector3(-312.232f, 89.5556f, 0f);

            GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/RainOptionText").GetComponent<TextMeshProUGUI>().text = "Disable Rain:";
            rainOptionText.SetActive(true);


            rainOptionButton = Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close"));
            rainOptionButton.transform.SetParent(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG").transform);
            rainOptionButton.name = "RainOptionButton";

            rainOptionButton.transform.localScale = new Vector3(.4f, .4f, .4f);
            rainOptionButton.transform.localPosition = new Vector3(-189.97f, 89.5556f, 0f);

            rainOptionButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            rainOptionButton.GetComponent<Button>().onClick.AddListener(RainOption);
            GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/RainOptionButton/Box/Icon").SetActive(toggleRain);







            tornadoOptionText = Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text"));
            tornadoOptionText.name = "TornadoOptionText";
            tornadoOptionText.transform.SetParent(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG").transform);
            tornadoOptionText.transform.localScale = new Vector3(0.66f, .66f, .66f);
            tornadoOptionText.transform.localPosition = new Vector3(-312.232f, 49.5556f, 0f);

            GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/TornadoOptionText").GetComponent<TextMeshProUGUI>().text = "Disable Tornados:";
            tornadoOptionText.SetActive(true);


            tornadoOptionButton = Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close"));
            tornadoOptionButton.transform.SetParent(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG").transform);
            tornadoOptionButton.name = "TornadoOptionButton";

            tornadoOptionButton.transform.localScale = new Vector3(.4f, .4f, .4f);
            tornadoOptionButton.transform.localPosition = new Vector3(-189.97f, 49.5556f, 0f);

            tornadoOptionButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            tornadoOptionButton.GetComponent<Button>().onClick.AddListener(TornadoOption);
            GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/TornadoOptionButton/Box/Icon").SetActive(toggleTornado);








            fogOptionText = Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text"));
            fogOptionText.name = "FogOptionText";
            fogOptionText.transform.SetParent(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG").transform);
            fogOptionText.transform.localScale = new Vector3(0.66f, .66f, .66f);
            fogOptionText.transform.localPosition = new Vector3(-312.232f, 9.5556f, 0f);

            GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/FogOptionText").GetComponent<TextMeshProUGUI>().text = "Faster Fog:";
            fogOptionText.SetActive(true);


            fogOptionButton = Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close"));
            fogOptionButton.transform.SetParent(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG").transform);
            fogOptionButton.name = "FogOptionButton";

            fogOptionButton.transform.localScale = new Vector3(.4f, .4f, .4f);
            fogOptionButton.transform.localPosition = new Vector3(-189.97f, 9.5556f, 0f);

            fogOptionButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            fogOptionButton.GetComponent<Button>().onClick.AddListener(FogOption);
            
            GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/FogOptionButton/Box/Icon").SetActive(toggleFog);








            tornadoSpeedOptionText = Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text"));
            tornadoSpeedOptionText.name = "TornadoSpeedOptionText";
            tornadoSpeedOptionText.transform.SetParent(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG").transform);
            tornadoSpeedOptionText.transform.localScale = new Vector3(0.66f, .66f, .66f);
            tornadoSpeedOptionText.transform.localPosition = new Vector3(-312.232f, -31.5556f, 0f);

            GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/TornadoSpeedOptionText").GetComponent<TextMeshProUGUI>().text = "Faster Tornados:";
            tornadoSpeedOptionText.SetActive(true);


            tornadoSpeedOptionButton = Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close"));
            tornadoSpeedOptionButton.transform.SetParent(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG").transform);
            tornadoSpeedOptionButton.name = "TornadoSpeedOptionButton";

            tornadoSpeedOptionButton.transform.localScale = new Vector3(.4f, .4f, .4f);
            tornadoSpeedOptionButton.transform.localPosition = new Vector3(-189.97f, -31.5556f, 0f);

            tornadoSpeedOptionButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            tornadoSpeedOptionButton.GetComponent<Button>().onClick.AddListener(TornadoSpeedOption);
            
            GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/TornadoSpeedOptionButton/Box/Icon").SetActive(toggleTornadoSpeed);












            abseilingOptionText = Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text"));
            abseilingOptionText.name = "AbseilingOptionText";
            abseilingOptionText.transform.SetParent(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG").transform);
            abseilingOptionText.transform.localScale = new Vector3(0.66f, .66f, .66f);
            abseilingOptionText.transform.localPosition = new Vector3(-312.232f, -71.5556f, 0f);

            GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/AbseilingOptionText").GetComponent<TextMeshProUGUI>().text = "Peak->Beach:";
            abseilingOptionText.SetActive(true);


            abseilingOptionButton = Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close"));
            abseilingOptionButton.transform.SetParent(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG").transform);
            abseilingOptionButton.name = "AbseilingOptionButton";

            abseilingOptionButton.transform.localScale = new Vector3(.4f, .4f, .4f);
            abseilingOptionButton.transform.localPosition = new Vector3(-189.97f, -71.5556f, 0f);

            abseilingOptionButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            abseilingOptionButton.GetComponent<Button>().onClick.AddListener(AbseilingOption);
            
            GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/AbseilingOptionButton/Box/Icon").SetActive(toggleAbseiling);






            



            setupComplete = true;
        }

    }





    public static WindChillZone rainZone;
    public static WindChillZone snowZone;
    public static TornadoSpawner tornados;
    public static OrbFogHandler fog;

    public static void Peaking()
    {

        rainZone = GameObject.Find("Map/Biome_2/Jungle/RainStorm").GetComponent<WindChillZone>();
        snowZone = GameObject.Find("Map/Biome_3/Snow/SnowStorm").GetComponent<WindChillZone>();
        tornados = GameObject.Find("Map/Biome_3/Desert/Desert_Segment/Misc/Tornados").GetComponent<TornadoSpawner>();
        fog = GameObject.Find("FogSphereSystem/FogSphere").GetComponent<OrbFogHandler>();

        if (toggleRain)
        {
            rainZone.windTimeRangeOff = new Vector2(float.MaxValue, float.MaxValue);
            if (rainZone.windActive)
            {
                rainZone.windActive = false;
            }
            rainZone.windTimeRangeOff = new Vector2(5f, 5f);
            rainZone.windTimeRangeOn = new Vector2(float.MaxValue, float.MaxValue);
        }
        if (toggleSnow)
        {
            snowZone.windTimeRangeOff = new Vector2(float.MaxValue, float.MaxValue);
            if (snowZone.windActive)
            {
                snowZone.windActive = false;
            }
        }
        if (toggleTornado)
        {
            tornados.minSpawnTime = float.MaxValue;

            for (int i = 0; i < UnityEngine.Object.FindObjectsByType<Tornado>(0).Length; i++)
            {
                PhotonView.Destroy(GameObject.Find("Tornado(Clone)"));
            }

        }
        
    }



    



    public static void WindChillOption()
    {
        GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/WindChillOptionButton/Box/Icon").SetActive(!toggleSnow);
        toggleSnow = !toggleSnow;
    }

    public static void RainOption()
    {
        GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/RainOptionButton/Box/Icon").SetActive(!toggleRain);
        toggleRain = !toggleRain; 
    }

    public static void TornadoOption()
    {
        GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/TornadoOptionButton/Box/Icon").SetActive(!toggleTornado);
        toggleTornado = !toggleTornado; 
    }

    public static void FogOption()
    {
        GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/FogOptionButton/Box/Icon").SetActive(!toggleFog);
        toggleFog = !toggleFog; 
    }

    public static void TornadoSpeedOption()
    {
        GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/TornadoSpeedOptionButton/Box/Icon").SetActive(!toggleTornadoSpeed);
        toggleTornadoSpeed = !toggleTornadoSpeed; 
    }

    public static void AbseilingOption()
    {
        GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/AbseilingOptionButton/Box/Icon").SetActive(!toggleAbseiling);
        toggleAbseiling = !toggleAbseiling; 
    }



    public static void NextPage()
    {
        GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel").SetActive(false);
        passportPage.SetActive(true);
        nextPageButton.SetActive(false);
        currentPage++;
        if (!prevPageButton.activeSelf)
        {
            prevPageButton.SetActive(true);
        }
    }




    public static void PrevPage()
    {
        GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel").SetActive(true);
        passportPage.SetActive(false);
        nextPageButton.SetActive(true);
        currentPage++;
        if (prevPageButton.activeSelf)
        {
            prevPageButton.SetActive(false);
        }
    }



}
