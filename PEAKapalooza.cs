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



    //Used for PeakToBeach mode in checking win
    public static bool wonGame = false;

    //Used to check for keypress
    public static bool keypress = false;

    //Set for PeakToBeach to work.
    public static int currentSegment = 3;

    //Used to make sure we don't try to load PeakToBeach stuff in Airport
    public static bool startingRun = false;


    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"\n _____  ______          _  __                 _                       _\n|  __ \\|  ____|   /\\   | |/ /                | |                     | |\n| |__) | |__     /  \\  | ' / __ _ _ __   __ _| | ___   ___ ______ _  | |\n|  ___/|  __|   / /\\ \\ |  < / _` | '_ \\ / _` | |/ _ \\ / _ \\_  / _` | | |\n| |    | |____ / ____ \\| . \\ (_| | |_) | (_| | | (_) | (_) / / (_| | |_|\n|_|    |______/_/    \\_\\_|\\_\\__,_| .__/ \\__,_|_|\\___/ \\___/___\\__,_| (_)\n                                 | |                                    \n                                 |_|                                    \n");
        SceneManager.sceneLoaded += OnSceneChange;

        Harmony.CreateAndPatchAll(typeof(PEAKapalooza));
    }

     
    
    //DEBUG Collection of debug keybinds
    public void Update()
    {

        if (debug)
        {

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha1) && keypress == false)
            {
                keypress = true;

                MapHandler.JumpToSegment(Segment.Beach);
            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha2) && keypress == false)
            {
                keypress = true;

                MapHandler.JumpToSegment(Segment.Tropics);
            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha3) && keypress == false)
            {
                keypress = true;

                MapHandler.JumpToSegment(Segment.Alpine);
            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha4) && keypress == false)
            {
                keypress = true;

                MapHandler.JumpToSegment(Segment.Caldera);
            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha5) && keypress == false)
            {
                keypress = true;

                MapHandler.JumpToSegment(Segment.TheKiln);
            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha6) && keypress == false)
            {
                keypress = true;

                MapHandler.JumpToSegment(Segment.Peak);
            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha7) && keypress == false)
            {
                keypress = true;

                Character.localCharacter.WarpPlayer(MapHandler.Instance.segments[4].reconnectSpawnPos.position, true);
            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha8) && keypress == false)
            {
                keypress = true;

                GameObject.Find("FogSphereSystem").SetActive(false);
            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha9) && keypress == false)
            {
                keypress = true;
                Character.localCharacter.WarpPlayer(new Vector3(16f, 1235f, 2239f), true);
            }

            if (!Input.GetKey(KeyCode.LeftControl))
            {
                keypress = false;
            }
        }
        
    }



    

    //DEBUG used to teleport where looking
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



    //Sets up run when PeakToBech is enabled
    [HarmonyPatch(typeof(RunManager), "StartRun")]
    [HarmonyPrefix]
    public static bool Prefix_PeakToBeach_StartRun(RunManager __instance)
    {
        
        if (togglePeakToBeach && startingRun)
        {
            __instance.runStarted = true;
            __instance.StartCoroutine(Teleport());

            Destroy(GameObject.Find("Map/Biome_4/Volcano/Peak/Box"));
            currentSegment = 3;
            return false;
        }
        return true;
          
    }


    //Used to teleport players within coroutine
    public static IEnumerator Teleport()
    {
        
        
        while (Vector3.Distance(GameObject.Find("Map/Biome_4/Volcano/Peak/Flag_planted_seagull/Flag Pole").transform.position, Character.localCharacter.Center) > 50)
        {
            MapHandler.JumpToSegment(Segment.TheKiln);
            Character.localCharacter.WarpPlayer(new Vector3(16f, 1235f, 2239f), true);
            yield return new WaitForSeconds(2);
        }
        
        if(Vector3.Distance(GameObject.Find("Backpack(Clone)").transform.position, Character.localCharacter.Center) > 50f) Character.localCharacter.refs.items.SpawnItemInHand("Backpack");
        yield return new WaitForSeconds(1);
    
    }


    

    //Handles loading previous zone when lighting Campfire
    [HarmonyPatch(typeof(Campfire), "Light_Rpc")]
    [HarmonyPrefix]
    public static bool Prefix_PeakToBeach_Light_Rpc(Campfire __instance)
    {
        if (togglePeakToBeach)
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
                if (toggleForceAlpine)
                {
                    GameObject.Find("Map/Biome_3/Snow").SetActive(true);
                    GameObject.Find("Map/Biome_3/Desert").SetActive(false);
                }
                if (toggleForceMesa)
                {
                    GameObject.Find("Map/Biome_3/Snow").SetActive(false);
                    GameObject.Find("Map/Biome_3/Desert").SetActive(true);
                }
                if (toggleAlpineAndMesa) {
                    GameObject.Find("Map/Biome_3/Snow").SetActive(true);
                    GameObject.Find("Map/Biome_3/Desert").SetActive(true);
                }

            }
            else if (currentSegment == 2)
            {
                currentSegment--;
                MapHandler.JumpToSegment(Segment.Alpine);
                if (toggleForceAlpine)
                {
                    GameObject.Find("Map/Biome_3/Snow").SetActive(true);
                    GameObject.Find("Map/Biome_3/Desert").SetActive(false);
                }
                if (toggleForceMesa)
                {
                    GameObject.Find("Map/Biome_3/Snow").SetActive(false);
                    GameObject.Find("Map/Biome_3/Desert").SetActive(true);
                }
                if (toggleAlpineAndMesa)
                {
                    GameObject.Find("Map/Biome_3/Snow").SetActive(true);
                    GameObject.Find("Map/Biome_3/Desert").SetActive(true);
                }



            }
            else if (currentSegment == 1)
            {
                currentSegment--;
                MapHandler.JumpToSegment(Segment.Tropics);
                GameObject.Find("Map/Biome_2/Jungle/Jungle_Segment/Ground").SetActive(false);
                GameObject.Find("Map/Biome_2/Jungle/Jungle_Segment/SkyJungle").SetActive(true);

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


    [HarmonyPatch(typeof(MapHandler), "GoToSegment")]
    [HarmonyPostfix]
    public static void Prefix_GenOptions_Logic(Segment s, MapHandler __instance)
    {
        Logger.LogMessage(s);
        if (toggleSkyJungle)
        {
            if (s == Segment.Tropics)
            {
                GameObject.Find("Map/Biome_2/Jungle/Jungle_Segment/Ground").SetActive(false);
                GameObject.Find("Map/Biome_2/Jungle/Jungle_Segment/SkyJungle").SetActive(true);
            }
        }
        if (toggleForceAlpine)
        {
            if (s == Segment.Alpine)
            {
                GameObject.Find("Map/Biome_3/Snow").SetActive(true);
                GameObject.Find("Map/Biome_3/Desert").SetActive(false);
            }
            
        }
        if (toggleForceMesa)
        {
            if (s == Segment.Alpine)
            {
                GameObject.Find("Map/Biome_3/Snow").SetActive(false);
                GameObject.Find("Map/Biome_3/Desert").SetActive(true);
            }
        }
        if (toggleAlpineAndMesa)
        {
            if (s == Segment.Alpine)
            {
                GameObject.Find("Map/Biome_3/Snow").SetActive(true);
                GameObject.Find("Map/Biome_3/Desert").SetActive(true);
            }
        }
    }


    //Handles loading previous zone without teleporting player
    [HarmonyPatch(typeof(MapHandler), "JumpToSegmentLogic")]
    [HarmonyPrefix]
    public static bool Prefix_PeakToBeach_JumpToSegmentLogic(Segment segment, HashSet<int> playersToTeleport, bool sendToEveryone, MapHandler __instance)
    {
        if (togglePeakToBeach)
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
            return false;
        }

        return true;
    }





    
    //Flare use next to BingBong wins the game
    [HarmonyPatch(typeof(Flare), "Update")]
    [HarmonyPrefix]
    public static bool Prefix_PeakToBeach_Update(Flare __instance)
    {
        if (togglePeakToBeach)
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
    public static bool Prefix_FogFaster_WaitToMove(OrbFogHandler __instance)
    {
        if (toggleFogFaster)
        {
            __instance.photonView.RPC("StartMovingRPC", RpcTarget.All, Array.Empty<object>());
            return false;
        }
        return true;
    }


    [HarmonyPatch(typeof(Tornado), "Movement")]
    [HarmonyPrefix]
    public static bool Prefix_TornadoFaster_Movement(Tornado __instance)
    {
        if (toggleTornadoFaster)
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



    [HarmonyPatch(typeof(HotSun), "Update")]
    [HarmonyPrefix]
    public static bool Prefix_HotSunDisable_Update(HotSun __instance)
    {
        if (toggleHotSunDisable) {
            return false;
        }
        return true;
    }


    [HarmonyPatch(typeof(LavaRising), "Update")]
    [HarmonyPrefix]
    public static bool Prefix_LavaRisingDisable_Update(LavaRising __instance)
    {
        if (toggleLavaRisingDisable) {
            return false;
        }

        if (toggleLavaRisingFaster) {
            __instance.travelTime = 30f;
        }
        return true;
    }




    [HarmonyPatch(typeof(OrbFogHandler), "Move")]
    [HarmonyPrefix]
    public static bool Prefix_FogFaster_Move(OrbFogHandler __instance)
    {
        if (toggleFogFaster)
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
            numberOfElements = 0;
            currentSegment = 3;
            startingRun = false;
        }
        else if (scene.name.StartsWith("Level_"))
        {
            startingRun = true;
            Peaking();
        }
    }



    public static bool setupComplete = false;
    
    public static bool toggleRainDisable = false;
    public static bool toggleSnowDisable = false;
    public static bool toggleTornadoDisable = false;
    public static bool toggleFogFaster = false;
    public static bool toggleFogDisable = false;
    public static bool toggleTornadoFaster = false;
    public static bool togglePeakToBeach = false;
    public static bool toggleSnowInfinite = false;
    public static bool toggleRainInfinite = false;
    public static bool toggleForceAlpine = false;
    public static bool toggleForceMesa = false;
    public static bool toggleAlpineAndMesa = false;
    public static bool toggleSkyJungle = false;
    public static bool toggleHotSunDisable = false;
    public static bool toggleLavaRisingDisable = false;
    public static bool toggleLavaRisingFaster = false;




    public static int numberOfElements = 0;

    [HarmonyPatch(typeof(PassportManager), "ToggleOpen")]
    [HarmonyPostfix]
    public static void Postfix_PassportOpen_ToggleOpen(PassportManager __instance)
    {
        if (!setupComplete)
        {
            
            UIHelper passportPage = new("2",UIHelper.UIType.PAGE,"",null,Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel")));
            passportPage.CreateInteractElement();


            UIHelper nextPage = new("NextPageButton",UIHelper.UIType.NAVBUTTON,">",null,Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")),0,570f,-61f);
            nextPage.CreateInteractElement();



            UIHelper prevPage = new("PrevPageButton", UIHelper.UIType.NAVBUTTON,"<",null,Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")),0,70,-61f);
            prevPage.CreateInteractElement();



            UIHelper snowDisable = new("SnowDisable", UIHelper.UIType.BUTTON, "Disable Blizzards:", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")), numberOfElements);
            
            snowDisable.CreateTextElement();
            snowDisable.CreateInteractElement();
            numberOfElements++;


            UIHelper snowInfinite = new("SnowInfinite", UIHelper.UIType.BUTTON, "Infinite Blizzard: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")),Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")),numberOfElements);

            snowInfinite.CreateTextElement();
            snowInfinite.CreateInteractElement();

            numberOfElements++;


            UIHelper rainDisable = new("RainDisable",UIHelper.UIType.BUTTON,"Disable Rain: ",Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")),numberOfElements);

            
            rainDisable.CreateTextElement();
            rainDisable.CreateInteractElement();

            numberOfElements++;


            UIHelper rainInfinite = new("RainInfinite",UIHelper.UIType.BUTTON,"Infinite Rain: ",Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")),numberOfElements);

            
            rainInfinite.CreateTextElement();
            rainInfinite.CreateInteractElement();

            numberOfElements++;


            UIHelper tornadoFaster = new("TornadoFaster",UIHelper.UIType.BUTTON, "Tornado 10x Faster: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")),numberOfElements);
            
            tornadoFaster.CreateTextElement();
            tornadoFaster.CreateInteractElement();

            numberOfElements++;

            UIHelper tornadoDisable = new("TornadoDisable",UIHelper.UIType.BUTTON, "Disable Tornados: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")),numberOfElements);
            
            tornadoDisable.CreateTextElement();
            tornadoDisable.CreateInteractElement();

            numberOfElements++;


            UIHelper hotSunDisable = new("HotSunDisable",UIHelper.UIType.BUTTON, "Disable Sun Heat: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")),numberOfElements);
            
            hotSunDisable.CreateTextElement();
            hotSunDisable.CreateInteractElement();

            numberOfElements++;


            UIHelper fogFaster = new("FogFaster",UIHelper.UIType.BUTTON, "Fog 2x Faster: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")),numberOfElements);
            
            fogFaster.CreateTextElement();
            fogFaster.CreateInteractElement();

            numberOfElements++;

            UIHelper fogDisable = new("FogDisable",UIHelper.UIType.BUTTON, "Disable Fog: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")),numberOfElements);
            
            fogDisable.CreateTextElement();
            fogDisable.CreateInteractElement();

            numberOfElements++;


            UIHelper lavaRisingDisable = new("LavaRisingDisable",UIHelper.UIType.BUTTON, "Disable Rising Lava: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")),numberOfElements);
            
            lavaRisingDisable.CreateTextElement();
            lavaRisingDisable.CreateInteractElement();

            numberOfElements++;


            UIHelper lavaRisingFaster = new("LavaRisingFaster",UIHelper.UIType.BUTTON, "Lava Rises 2x Faster: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")),numberOfElements);
            
            lavaRisingFaster.CreateTextElement();
            lavaRisingFaster.CreateInteractElement();

            numberOfElements++;


            UIHelper forceAlpine = new("ForceAlpine",UIHelper.UIType.BUTTON, "Force Alpine: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")),numberOfElements);
            
            forceAlpine.CreateTextElement();
            forceAlpine.CreateInteractElement();

            numberOfElements++;


            UIHelper forceMesa = new("ForceMesa",UIHelper.UIType.BUTTON, "Force Mesa: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")),numberOfElements);
            
            forceMesa.CreateTextElement();
            forceMesa.CreateInteractElement();

            numberOfElements++;


            UIHelper alpineAndMesa = new("AlpineAndMesa",UIHelper.UIType.BUTTON, "Alpine And Mesa Gen: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")),numberOfElements);
            
            alpineAndMesa.CreateTextElement();
            alpineAndMesa.CreateInteractElement();

            numberOfElements++;


            UIHelper skyJungle = new("SkyJungle",UIHelper.UIType.BUTTON, "Sky Jungle Gen: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")),numberOfElements);
            
            skyJungle.CreateTextElement();
            skyJungle.CreateInteractElement();

            numberOfElements++;

            

            UIHelper peakToBeach = new("PeakToBeach",UIHelper.UIType.BUTTON, "Peak To Beach: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")),numberOfElements);
            
            peakToBeach.CreateTextElement();
            peakToBeach.CreateInteractElement();

            numberOfElements++;
            


            setupComplete = true;
        }

    }





    public static WindChillZone rainZone;
    public static WindChillZone snowZone;
    public static TornadoSpawner tornados;

    public static void Peaking()
    {

        rainZone = GameObject.Find("Map/Biome_2/Jungle/RainStorm").GetComponent<WindChillZone>();
        snowZone = GameObject.Find("Map/Biome_3/Snow/SnowStorm").GetComponent<WindChillZone>();
        tornados = GameObject.Find("Map/Biome_3/Desert/Desert_Segment/Misc/Tornados").GetComponent<TornadoSpawner>();

        if (toggleRainDisable)
        {
            rainZone.windTimeRangeOff = new Vector2(float.MaxValue, float.MaxValue);
            if (rainZone.windActive)
            {
                rainZone.windActive = false;
            }
        }
        if (toggleSnowDisable)
        {
            snowZone.windTimeRangeOff = new Vector2(float.MaxValue, float.MaxValue);
            if (snowZone.windActive)
            {
                snowZone.windActive = false;
            }
        }
        if (toggleAlpineAndMesa || toggleForceAlpine) {
            snowZone.windChillPerSecond = 0.05f;
        }
        if (toggleTornadoDisable)
        {
            tornados.minSpawnTime = float.MaxValue;

            for (int i = 0; i < UnityEngine.Object.FindObjectsByType<Tornado>(0).Length; i++)
            {
                PhotonView.Destroy(GameObject.Find("Tornado(Clone)"));
            }

        }
        if (toggleSnowInfinite)
        {
            snowZone.windTimeRangeOn = new Vector2(float.MaxValue, float.MaxValue);
            snowZone.windTimeRangeOff = new Vector2(0f, 0f);
            if (!snowZone.windActive)
            {
                snowZone.windActive = true;
            }
        }
        if (toggleRainInfinite)
        {
            rainZone.windTimeRangeOn = new Vector2(float.MaxValue, float.MaxValue);
            rainZone.windTimeRangeOff = new Vector2(0, 0);
            if (!rainZone.windActive)
            {
                rainZone.windActive = true;
            }
        }
        if (toggleFogDisable) {
            GameObject.Find("FogSphereSystem").SetActive(false);
        }
        
    }




}
