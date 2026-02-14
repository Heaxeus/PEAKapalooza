/**
Notes
Lava in Caldera rises using LavaRising class (Map/Biome_4/Volcano/Volcano_Segment/Mechanics/RisingLava), has a Start Position, End Position denoted by TopTransform position, Time to Move, and will lerp between them within timeframe.

NetworkConnector class has methods that are called when players join lobby,etc. Use as refertence for distribting selected options.

For New players:
    MapHandler does alot, CharacterSpawner has compiler methods that call SpawnSelfAtBaseCamp and SpawnSelfAtSpecificPosition, the former being used for new players, latter for returning


For wind direction, Vector is either .9701 0 .2425 (facing mountain, right), or -.9701 0 .2425 (facing mountain, left)

Line 632, ONE OF THOSE WORKS! Test to see if recompiling shaders or if rebaking all lights works, pretty sure it's shaders. Will need to be done when the Mesa/Alpine area loads.

**/

/**
The file globalgamemanagers in PEAK_DATA can be replaced to remove almost all of the white screen on load. Windows (I think) still lets a little flash of white get through.
**/



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
using Peak.Network;
using System.Drawing;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System.ComponentModel;

namespace PEAKapalooza;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("PEAK.exe")]
public class PEAKapalooza : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    public static bool debug = true;



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

    /**
     _____  ______ ____  _    _  _____ 
    |  __ \|  ____|  _ \| |  | |/ ____|
    | |  | | |__  | |_) | |  | | |  __ 
    | |  | |  __| |  _ <| |  | | | |_ |
    | |__| | |____| |_) | |__| | |__| |
    |_____/|______|____/ \____/ \_____|
                                             
    **/

    // Collection of debug keybinds
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
                // Character.localCharacter.WarpPlayer(new Vector3(16f, 1235f, 2239f), true);
                GUIManager.instance.SetHeroTitle("Peak to Beach", null);
            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha0) && keypress == false)
            {
                keypress = true;
                foreach (LightVolume lv in FindObjectsByType<LightVolume>(FindObjectsSortMode.None))
                {
                    lv.Bake(null);
                }

            }

            if (!Input.GetKey(KeyCode.LeftControl))
            {
                keypress = false;
            }

        }

    }


    //Used to teleport where looking
    [HarmonyPatch(typeof(PointPinger), "ReceivePoint_Rpc")]
    [HarmonyPostfix]
    public static void Postfix_PingWarp_ReceivePoint_Rpc(PointPinger __instance)
    {
        if (debug)
        {
            RaycastHit raycastHit;
            if (Camera.main.ScreenPointToRay(Input.mousePosition).Raycast(out raycastHit, HelperFunctions.LayerType.TerrainMap.ToLayerMask(), -1f) && __instance.photonView.IsMine)
            {
                __instance.character.WarpPlayer(raycastHit.point, true);
            }
        }
    }

    /**
     _____  ______          _  __  _______ ____    ____  ______          _____ _    _ 
    |  __ \|  ____|   /\   | |/ / |__   __/ __ \  |  _ \|  ____|   /\   / ____| |  | |
    | |__) | |__     /  \  | ' /     | | | |  | | | |_) | |__     /  \ | |    | |__| |
    |  ___/|  __|   / /\ \ |  <      | | | |  | | |  _ <|  __|   / /\ \| |    |  __  |
    | |    | |____ / ____ \| . \     | | | |__| | | |_) | |____ / ____ \ |____| |  | |
    |_|    |______/_/    \_\_|\_\    |_|  \____/  |____/|______/_/    \_\_____|_|  |_|
                                                                                                                                                                
    **/

    //Sets up run when PeakToBeach is enabled
    //TODO need to have PhotonInterfacer be added to Character on game join, not when run starts. Otherwise, returning players don't have it
    // [HarmonyPatch(typeof(LoadingScreen), "LoadingRoutine")]
    // [HarmonyPostfix]
    // public static IEnumerator Postfix_PeakToBeach_LoadingRoutine(IEnumerator __result, LoadingScreen __instance)
    // {
    //     if (togglePeakToBeach)
    //     {
    //         while (__result.MoveNext())
    //         {
    //             yield return __result.Current;
    //         }

    //     }
    // }

    [HarmonyPatch(typeof(PlayerHandler), "RegisterPlayerImpl")]
    [HarmonyPostfix]
    public static void Postfix_PeakToBeach_StartPassedOutOnTheBeach(PlayerHandler __instance)
    {

        if (PhotonNetwork.IsMasterClient && startingRun)
        {
            GameUtils.instance.gameObject.AddComponent<PhotonInterfacer>();
            foreach (Character character in FindObjectsByType<Character>(FindObjectsSortMode.None))
            {
                if (character.isBot == false)
                {
                    character.gameObject.AddComponent<PhotonInterfacer>();
                    Logger.LogMessage("Added interface to: " + character.name);
                }
            }

            Character.localCharacter.view.RPC("RPC_Start_Peak_To_Beach", RpcTarget.All, []);
        }
    }

    public static bool shownStartTitle = false;
    public static EyeBlinkController ebc = null;
    public static bool temp = false;

    [HarmonyPatch(typeof(Character), "Update")]
    [HarmonyPostfix]
    public static void Postfix_PeakToBeach_Update(Character __instance)
    {
        if (togglePeakToBeach && !shownStartTitle && startingRun)
        {
            if (ebc != null && ebc.eyeOpenValue > 0.999)
            {
                Logger.LogMessage("IT SHOULD PLAY HERE!");
                shownStartTitle = true;
                GUIManager.instance.SetHeroTitle("Peak To Beach", null);
            }
        }
    }


    public class PhotonInterfacer : MonoBehaviour
    {

        [PunRPC]
        public void RPC_Send_Current_Segment(int segmentNum)
        {
            Logger.LogMessage("Segment sent to all players!");
            currentSegment = segmentNum;
        }


        [PunRPC]
        public void RPC_Start_Peak_To_Beach()
        {
            MapHandler.JumpToSegment(Segment.TheKiln);
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Instantiate("0_Items/Backpack", new Vector3(16f, 1235f, 2239f), Quaternion.identity, 0, null).GetComponent<Item>();
            }
        }

        //Changes terrain for each player
        [PunRPC]
        public void RPC_Next_Section_Peak_To_Beach()
        {
            if (currentSegment == 3)
            {
                GUIManager.instance.SetHeroTitle(Singleton<MountainProgressHandler>.Instance.progressPoints[(int)Segment.Caldera].localizedTitle, Singleton<MountainProgressHandler>.Instance.progressPoints[(int)Segment.Caldera].clip);
                MapHandler.JumpToSegment(Segment.Caldera);
                if (toggleForceAlpine)
                {
                    GameObject.Find("Map/Biome_3/Alpine").SetActive(true);
                    GameObject.Find("Map/Biome_3/Mesa").SetActive(false);
                }
                if (toggleForceMesa)
                {
                    GameObject.Find("Map/Biome_3/Alpine").SetActive(false);
                    GameObject.Find("Map/Biome_3/Mesa").SetActive(true);
                }
                if (toggleAlpineAndMesa)
                {
                    GameObject.Find("Map/Biome_3/Alpine").SetActive(true);
                    GameObject.Find("Map/Biome_3/Mesa").SetActive(true);
                }
            }
            else if (currentSegment == 2)
            {
                GUIManager.instance.SetHeroTitle(Singleton<MountainProgressHandler>.Instance.progressPoints[(int)Segment.Alpine].localizedTitle, Singleton<MountainProgressHandler>.Instance.progressPoints[(int)Segment.Alpine].clip);
                MapHandler.JumpToSegment(Segment.Alpine);
                if (toggleForceAlpine)
                {
                    GameObject.Find("Map/Biome_3/Alpine").SetActive(true);
                    GameObject.Find("Map/Biome_3/Mesa").SetActive(false);
                }
                if (toggleForceMesa)
                {
                    GameObject.Find("Map/Biome_3/Alpine").SetActive(false);
                    GameObject.Find("Map/Biome_3/Mesa").SetActive(true);
                }
                if (toggleAlpineAndMesa)
                {
                    GameObject.Find("Map/Biome_3/Alpine").SetActive(true);
                    GameObject.Find("Map/Biome_3/Mesa").SetActive(true);
                }
            }
            else if (currentSegment == 1)
            {
                GUIManager.instance.SetHeroTitle(Singleton<MountainProgressHandler>.Instance.progressPoints[(int)Segment.Tropics].localizedTitle, Singleton<MountainProgressHandler>.Instance.progressPoints[(int)Segment.Tropics].clip);
                MapHandler.JumpToSegment(Segment.Tropics);
                if (toggleSkyJungle)
                {
                    GameObject.Find("Map/Biome_2/Tropics/Jungle_Segment/Ground").SetActive(false);
                    GameObject.Find("Map/Biome_2/Tropics/Jungle_Segment/SkyJungle").SetActive(true);
                }
            }
            else if (currentSegment == 0)
            {
                GUIManager.instance.SetHeroTitle(Singleton<MountainProgressHandler>.Instance.progressPoints[(int)Segment.Beach].localizedTitle, Singleton<MountainProgressHandler>.Instance.progressPoints[(int)Segment.Beach].clip);
                MapHandler.JumpToSegment(Segment.Beach);
                GameObject.Find("Map/Biome_1/Beach/Beach_Segment/crashed plane").SetActive(false);
            }
        }

        [PunRPC]
        public void HandleAssignedViewIDs_RPC(int[] ids)
        {
            Logger.LogWarning("12.1");
            if (PhotonNetwork.IsMasterClient)
            {
                return;
            }
            Logger.LogWarning("12.2");
            PhotonView[] source = (from v in Singleton<MapHandler>.Instance.gameObject.GetComponentsInChildren<PhotonView>(true)
                                   where v.ViewID == 0
                                   select v).ToArray();
            Logger.LogWarning("12.3");
            PhotonView[] array = source.OrderBy(delegate (PhotonView v)
            {
                Logger.LogWarning("12.4");
                GameObject gameObject = v.gameObject;
                List<string> list = [];
                Transform val = gameObject.transform;
                while ((UnityEngine.Object)(object)val != null)
                {
                    list.Add(val.name);
                    val = val.parent;
                }
                Logger.LogWarning("12.5");
                list.Reverse();
                return string.Join("/", list);
            }).ToArray();
            Logger.LogWarning("12.6");
            int num = Math.Min(array.Length, ids.Length);
            for (int i = 0; i < num; i++)
            {
                try
                {
                    array[i].ViewID = ids[i];
                    Logger.LogWarning("12.7." + "i");
                }
                catch (Exception arg)
                {
                    Logger.LogError($"[Gen] Failed to assign view id on client: {arg}");
                }
            }
        }
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
            if (PhotonNetwork.IsMasterClient)
            {
                Logger.LogWarning("CURRENT SEGMENT: " + currentSegment);
                Character.localCharacter.view.RPC("RPC_Next_Section_Peak_To_Beach", RpcTarget.All, []);
                Singleton<MapHandler>.Instance.currentSegment = currentSegment;
                if (currentSegment != 0)
                {
                    currentSegment--;
                    Character.localCharacter.view.RPC("RPC_Send_Current_Segment", RpcTarget.All, [currentSegment]);
                }
            }
            foreach (Luggage luggage in Luggage.ALL_LUGGAGE)
            {
                if (luggage.state == Luggage.LuggageState.Open)
                {
                    luggage.anim.Play("Luggage_Open");
                }
            }
            return false;
        }
        return true;
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
            if (segment == Segment.TheKiln)
            {
                mapSegment2.reconnectSpawnPos.position = new Vector3(16f, 1235f, 2239f);
            }

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
                    Debug.Log(text + text2 + text3 + (type?.ToString()));
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
            if (PhotonNetwork.IsMasterClient && segment == Segment.TheKiln)
            {
                Debug.Log(string.Format("Teleporting all players to {0} campfire..", segment));
                foreach (Character character in PlayerHandler.GetAllPlayerCharacters())
                {
                    if (playersToTeleport.Contains(character.photonView.Owner.ActorNumber) && character.isBot == false)
                    {
                        __instance.StartCoroutine(PlayerWarpHelper(character));
                    }
                }
            }
            if (sendToEveryone)
            {
                CustomCommands<CustomCommandType>.SendPackage(new SyncMapHandlerDebugCommandPackage(segment, Array.Empty<int>()), ReceiverGroup.Others);
            }
            return false;
        }
        return true;
    }

    public static IEnumerator PlayerWarpHelper(Character character)
    {
        yield return new WaitForSeconds(3f);
        Character.localCharacter.photonView.RPC("WarpPlayerRPC", RpcTarget.All, new object[] { new Vector3(16f, 1235f, 2239f), false });
        yield break;
    }



    [HarmonyPatch(typeof(Luggage), "OpenLuggageRPC")]
    [HarmonyPostfix]
    public static void Postfix_FixLuggageState_OpenLuggageRPC(bool spawnItems, Luggage __instance)
    {
        if (togglePeakToBeach)
        {
            Luggage.ALL_LUGGAGE.Add(__instance);
        }
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
                foreach (Character character in PlayerHandler.GetAllPlayerCharacters())
                {
                    character.RPCEndGame_ForceWin();
                }
                wonGame = true;
            }
            return false;
        }
        return true;
    }


    [HarmonyPatch(typeof(GUIManager), "SetHeroTitle")]
    [HarmonyPrefix]
    public static bool Prefix_CustomHeroTitle_SetHeroTitle(string text, AudioClip stinger, GUIManager __instance)
    {
        if (togglePeakToBeach)
        {
            Debug.Log("Set hero title: " + text);
            if (__instance._heroRoutine != null)
            {
                __instance.StopCoroutine(__instance._heroRoutine);
            }
            if (__instance.stingerSound && stinger != null)
            {
                __instance.stingerSound.clip = stinger;
                __instance.stingerSound.Play();
            }
            __instance._heroRoutine = __instance.StartCoroutine(CustomHeroTitle(text, __instance));
            return false;
        }
        return true;
    }

    public static IEnumerator CustomHeroTitle(string heroString, GUIManager __instance)
    {
        __instance.heroCanvasObject.gameObject.SetActive(true);
        yield return null;

        string dayString;
        if (heroString == "Peak to Beach")
        {
            dayString = "Light a Flare near BingBong!";
        }
        else
        {
            dayString = DayNightManager.instance.DayCountString();
        }

        string timeOfDayString = DayNightManager.instance.TimeOfDayString();
        __instance.heroObject.gameObject.SetActive(true);
        __instance.heroImage.color = new UnityEngine.Color(__instance.heroImage.color.r, __instance.heroImage.color.g, __instance.heroImage.color.b, 1f);
        __instance.heroShadowImage.color = new UnityEngine.Color(__instance.heroShadowImage.color.r, __instance.heroShadowImage.color.g, __instance.heroShadowImage.color.b, 0.12f);
        __instance.heroDayText.text = "";
        __instance.heroTimeOfDayText.text = "";
        __instance.heroBG.color = new UnityEngine.Color(0f, 0f, 0f, 0f);
        __instance.heroBG.DOFade(0.5f, 0.5f);
        int num;
        for (int i = 0; i < heroString.Length; i = num + 1)
        {
            __instance.heroText.text = heroString.Substring(0, i + 1);
            __instance.heroCamera.Render();
            yield return new WaitForSeconds(0.1f);
            num = i;
        }
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < dayString.Length; i = num + 1)
        {
            __instance.heroDayText.text = dayString.Substring(0, i + 1);
            __instance.heroCamera.Render();
            yield return new WaitForSeconds(0.066f);
            num = i;
        }
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < timeOfDayString.Length; i = num + 1)
        {
            __instance.heroTimeOfDayText.text = timeOfDayString.Substring(0, i + 1);
            __instance.heroCamera.Render();
            yield return new WaitForSeconds(0.066f);
            num = i;
        }
        yield return new WaitForSeconds(1.5f);
        __instance.heroImage.DOFade(0f, 2f);
        __instance.heroShadowImage.DOFade(0f, 1f);
        __instance.heroBG.DOFade(0f, 2f);
        yield return new WaitForSeconds(2f);
        __instance.heroObject.gameObject.SetActive(false);
        __instance.heroCanvasObject.gameObject.SetActive(false);
        yield break;
    }





    /**
     __  __  ____  _____ _____ ______ _____ ______ _____   _____ 
    |  \/  |/ __ \|  __ \_   _|  ____|_   _|  ____|  __ \ / ____|
    | \  / | |  | | |  | || | | |__    | | | |__  | |__) | (___  
    | |\/| | |  | | |  | || | |  __|   | | |  __| |  _  / \___ \ 
    | |  | | |__| | |__| || |_| |     _| |_| |____| | \ \ ____) |
    |_|  |_|\____/|_____/_____|_|    |_____|______|_|  \_\_____/ 
                                                              
    **/

    [HarmonyPatch(typeof(Campfire), "Light_Rpc")]
    [HarmonyPostfix]
    public static void Postfix_LightingUpdates_Light_Rpc(Campfire __instance)
    {
        if (toggleAlpineAndMesa && __instance.advanceToSegment == Segment.Alpine)
        {
            if (__instance.advanceToSegment == Segment.Alpine)
            {
                GameObject.Find("Map/Biome_3/Alpine/Snow_Segment").SetActive(true);
                GameObject.Find("Map/Biome_3/Mesa/Desert_Segment").SetActive(true);
                GameObject.Find("Map/Biome_3/Mesa/Desert_Campfire").SetActive(true);
                foreach (LightVolume lv in FindObjectsByType<LightVolume>(FindObjectsSortMode.None))
                {
                    lv.Bake(null);
                }
            }
        }

    }






    [HarmonyPatch(typeof(ConnectionHandler), "Awake")]
    [HarmonyPostfix]
    public static void Postfix_DontKickMe_Awake(ConnectionHandler __instance)
    {
        __instance.KeepAliveInBackground = Int32.MaxValue;
    }


    [HarmonyPatch(typeof(Pretitle), "Update")]
    [HarmonyPrefix]
    public static bool Prefix_StopBlindingMe_Update(Pretitle __instance)
    {
        __instance.allowedToSwitch = true;
        return false;
    }

    [HarmonyPatch(typeof(PropSpawner), "SpawnNew")]
    [HarmonyPrefix]
    public static bool Prefix_SpawnProps_SpawnNew(bool executeDeferredImmediately, PropSpawner __instance)
    {
        if (toggleAlpineAndMesa)
        {
            if (__instance.chanceToUseSpawner < 0.999f && UnityEngine.Random.value > __instance.chanceToUseSpawner)
            {
                return false;
            }
            int num = __instance.nrOfSpawns;
            if (__instance.randomSpawns)
            {
                num = UnityEngine.Random.Range(__instance.minSpawnCount, __instance.nrOfSpawns);
            }
            int num2 = 6000;
            int num3 = 1000;
            int num4 = 0;
            while (num4 < num && num2 > 0 && num3 > 0)
            {
                num2--;
                num3--;
                if (__instance.TryToSpawn(num4))
                {
                    num4++;
                    num3 = 1000;
                    if (__instance.syncTransforms)
                    {
                        Physics.SyncTransforms();
                    }
                }
            }
            if (num2 == 0)
            {
                Debug.LogError("Max attempts reached in PropSpawner, could not spawn all props!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", __instance.gameObject);
            }
            if (num3 == 0)
            {
                Debug.LogError("Max attempts IN A ROW reached in PropSpawner, could not spawn all props!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", __instance.gameObject);
            }
            __instance.currentSpawns = __instance.transform.childCount;
            // __instance.SpawnDecor();
            foreach (PostSpawnBehavior postSpawnBehavior in __instance.postSpawnBehaviors)
            {
                if (!postSpawnBehavior.mute)
                {
                    if (executeDeferredImmediately || postSpawnBehavior.DeferredTiming != DeferredStepTiming.AfterCurrentGroupTiming)
                    {
                        postSpawnBehavior.RunBehavior(__instance.SpawnedProps);
                    }
                    else
                    {
                        __instance._deferredSteps.Add(postSpawnBehavior.ConstructDeferred(__instance));
                    }
                }
            }
            return false;
        }
        return true;
    }

    //TODO redo default biome props, shut off area rendering when starting run, shut off area rendering when moving to next area.
    public static void Generate_Terrain(MapHandler.MapSegment segment)
    {
        PropGrouper pgrouper = null;
        PropGrouper pgrouperCampfire = null;
        PropGrouper pgrouperProps = null;
        PropGrouper pgrouperCampfireProps = null;
        if (defaultBiomes.Contains(Biome.BiomeType.Alpine))
        {
            GameObject.Find("Map/Biome_3/Mesa").SetActive(true);
            segment.segmentParent.SetActive(true);
            segment.segmentCampfire.SetActive(true);
            pgrouper = segment.segmentParent.transform.GetComponent<PropGrouper>() ?? segment.segmentParent.transform.gameObject.AddComponent<PropGrouper>();
            pgrouper.RunAll(false);
            Logger.LogWarning("4.4");
            pgrouperCampfire = segment.segmentCampfire.GetComponent<PropGrouper>() ?? segment.segmentCampfire.AddComponent<PropGrouper>();
            pgrouperCampfire.RunAll();
            Logger.LogWarning("4.5");
            pgrouperProps = GameObject.Find("Map/Biome_3/Alpine/Snow_Segment").transform.GetComponent<PropGrouper>() ?? GameObject.Find("Map/Biome_3/Alpine/Snow_Segment").transform.gameObject.AddComponent<PropGrouper>();
            pgrouperCampfireProps = GameObject.Find("Map/Biome_3/Alpine/Snow_Campfire").GetComponent<PropGrouper>() ?? GameObject.Find("Map/Biome_3/Alpine/Snow_Campfire").AddComponent<PropGrouper>();
        }
        else if (defaultBiomes.Contains(Biome.BiomeType.Mesa))
        {
            GameObject.Find("Map/Biome_3/Alpine").SetActive(true);
            GameObject segment2 = GameObject.Find("Map/Biome_3/Alpine/Snow_Segment");
            GameObject segmentCampfire2 = GameObject.Find("Map/Biome_3/Alpine/Snow_Campfire");

            Logger.LogWarning("4.6");

            segment2.SetActive(true);
            segmentCampfire2.SetActive(true);
            pgrouper = segment2.transform.GetComponent<PropGrouper>() ?? segment2.transform.gameObject.AddComponent<PropGrouper>();
            //false doesn't update lightmap
            Logger.LogWarning("4.7");

            pgrouper.RunAll(false);
            Logger.LogWarning("4.8");

            pgrouperCampfire = segmentCampfire2.GetComponent<PropGrouper>() ?? segmentCampfire2.AddComponent<PropGrouper>();
            pgrouperCampfire.RunAll();
            Logger.LogWarning("4.9");

            pgrouperProps = GameObject.Find("Map/Biome_3/Mesa/Desert_Segment").transform.GetComponent<PropGrouper>() ?? GameObject.Find("Map/Biome_3/Mesa/Desert_Segment").transform.gameObject.AddComponent<PropGrouper>();
            pgrouperCampfireProps = GameObject.Find("Map/Biome_3/Mesa/Desert_Campfire").GetComponent<PropGrouper>() ?? GameObject.Find("Map/Biome_3/Mesa/Desert_Campfire").AddComponent<PropGrouper>();
        }
        segment.segmentParent.SetActive(true);
        segment.segmentCampfire.SetActive(true);
        Generate_Late_Props(pgrouper);
        Generate_Late_Props(pgrouperCampfire);
        Generate_Late_Props(pgrouperProps);
        Generate_Late_Props(pgrouperCampfireProps);
    }

    public static List<LevelGenStep> lgs = [];

    public static void Generate_Late_Props(PropGrouper pgrouper)
    {
        lgs.Clear();
        //might need to be false
        LevelGenStep[] genSteps = pgrouper.GetComponentsInChildren<LevelGenStep>(true);
        foreach (LevelGenStep step in genSteps)
        {
            PropGrouper parent = step.GetComponentInParent<PropGrouper>();
            try
            {
                if (parent != null && parent.timing == PropGrouper.PropGrouperTiming.Late)
                {
                    lgs.Add(step);
                }
            }
            catch
            {
                if (parent == pgrouper)
                {
                    lgs.Add(step);
                }
            }
        }
        foreach (LevelGenStep step in lgs)
        {
            try
            {
                step.Execute();
            }
            catch
            {
                Logger.LogMessage("Could not execute level gen step!");
            }
        }
    }


    [HarmonyPatch(typeof(MapHandler), "Start")]
    [HarmonyPostfix]
    public static IEnumerator Postfix_GenOptions_RunAll(IEnumerator __result)
    {
        if (toggleAlpineAndMesa)
        {
            while (__result.MoveNext())
            {
                yield return __result.Current;
            }
            Singleton<MapHandler>.Instance.StartCoroutine(MH_Start_Gen());
        }
    }

    public static IEnumerator MH_Start_Gen()
    {
        List<int> assignedViewIDs = [];
        Logger.LogWarning("1");
        MapHandler.MapSegment[] segments = Singleton<MapHandler>.Instance.segments;

        if (!PhotonNetwork.IsMasterClient)
        {
            yield break;
        }
        Logger.LogWarning("2");
        foreach (MapHandler.MapSegment segment in segments)
        {
            Logger.LogWarning("3");
            GameObject segmentParent = segment.segmentParent;
            Logger.LogWarning("4");
            Transform segmentParentTransform = segmentParent.transform;
            Logger.LogMessage(segmentParentTransform.gameObject.name);
            Logger.LogWarning("4.1");


            Logger.LogWarning("5");

            Logger.LogWarning("6");
            GameObject segmentCampfire = segment.segmentCampfire;
            Logger.LogWarning("7");
            if (segmentCampfire == null)
            {
                Logger.LogWarning("7.1");
                foreach (Transform val in segmentParentTransform.GetComponentsInChildren<Transform>(true))
                {
                    if (val.gameObject.name.Contains("Campfire"))
                    {
                        Logger.LogWarning("7.2");
                        segmentCampfire = val.gameObject;
                        break;
                    }
                }
                Logger.LogWarning("7.3");
            }
            if (toggleAlpineAndMesa)
            {
                if (segmentParentTransform.gameObject.name.Contains("Desert") && defaultBiomes.Contains(Biome.BiomeType.Alpine))
                {
                    Generate_Terrain(segment);
                }
                else if (segmentParentTransform.gameObject.name.Contains("Desert") && defaultBiomes.Contains(Biome.BiomeType.Mesa))
                {
                    Generate_Terrain(segment);
                }
            }
            Logger.LogWarning("7.4");
            List<PhotonView> list3 =
            [
                .. from v in segmentParentTransform.GetComponentsInChildren<PhotonView>(true)
                where v.ViewID == 0
                select v,
            ];
            if (segmentCampfire != null)
            {
                list3.AddRange(from v in segmentCampfire.GetComponentsInChildren<PhotonView>(true)
                               where v.ViewID == 0
                               select v);
            }
            Logger.LogWarning("8");
            PhotonView[] array = [.. from v in list3
                                                    where v != null
                                                    orderby GetHierarchyPath(v.gameObject)
                                                    select v];
            Logger.LogWarning("START PRINT ARRAY");
            foreach (PhotonView pv in array)
            {
                Logger.LogMessage(pv.gameObject.name);
            }
            Logger.LogWarning("END PRINT ARRAY");
            Logger.LogWarning("9");
            PhotonView[] array2 = array;
            foreach (PhotonView view in array2)
            {
                assignedViewIDs.Add(AssignMasterClientViewID(view.gameObject));
            }
            Logger.LogWarning("10");

        }
        Logger.LogWarning("11");
        if (assignedViewIDs.Count > 0)
        {
            Logger.LogWarning("12");
            Logger.LogMessage(assignedViewIDs);
            GameObject.Find("GAME").AddComponent<PhotonInterfacer>();
            GameObject.Find("GAME").GetPhotonView().RPC("HandleAssignedViewIDs_RPC", RpcTarget.All, [assignedViewIDs.ToArray()]);
            Logger.LogWarning("13");
        }
        GameObject.Find("Map/Biome_3/Alpine/Snow_Segment").SetActive(false);
        GameObject.Find("Map/Biome_3/Alpine/Snow_Campfire").SetActive(false);
        GameObject.Find("Map/Biome_3/Mesa/Desert_Segment").SetActive(false);
        GameObject.Find("Map/Biome_3/Mesa/Desert_Campfire").SetActive(false);
    }



    public static string GetHierarchyPath(GameObject go)
    {
        List<string> list = new List<string>();
        Transform val = go.transform;
        while ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
        {
            list.Add(((UnityEngine.Object)val).name);
            val = val.parent;
        }
        list.Reverse();
        return string.Join("/", list);
    }

    public static int AssignMasterClientViewID(GameObject go)
    {
        int num = PhotonNetwork.AllocateViewID(false);
        PhotonView component = go.GetComponent<PhotonView>();
        component.ViewID = num;
        Logger.LogMessage("GameObject: " + go.name + " given ViewID: " + num);
        return num;
    }


    [HarmonyPatch(typeof(MapHandler), "Awake")]
    [HarmonyPostfix]
    public static void Postfix_GenOptions_Awake(MapHandler __instance)
    {
        if (toggleAlpineAndMesa)
        {
            __instance.DetectBiomes();
        }
    }

    public static Biome.BiomeType[] defaultBiomes = [];


    [HarmonyPatch(typeof(MapHandler), "DetectBiomes")]
    [HarmonyPrefix]
    public static bool Prefix_GenOptions_DetectBiomes(MapHandler __instance)
    {
        if (toggleAlpineAndMesa)
        {
            defaultBiomes = __instance.biomes.ToArray();
            __instance.biomes.Clear();
            for (int i = 0; i < __instance.transform.childCount; i++)
            {
                Transform child = __instance.transform.GetChild(i);
                for (int j = 0; j < child.childCount; j++)
                {
                    Logger.LogMessage("NAME OF GAME OBJECT: " + child.GetChild(j).gameObject.name);
                    if (toggleAlpineAndMesa)
                    {
                        if (child.GetChild(j).gameObject.name == "Alpine" || child.GetChild(j).gameObject.name == "Mesa")
                        {
                            child.GetChild(j).gameObject.SetActive(true);
                        }
                    }
                    Biome biome;
                    if (child.GetChild(j).gameObject.activeInHierarchy && child.GetChild(j).TryGetComponent<Biome>(out biome))
                    {
                        Logger.LogMessage("BIOME ADDED: " + biome.biomeType);
                        __instance.biomes.Add(biome.biomeType);
                    }
                }
            }
            foreach (Biome.BiomeType b in __instance.biomes)
            {
                Logger.LogMessage(b);
            }
            return false;
        }
        else
        {
            return true;
        }
    }




    [HarmonyPatch(typeof(MapHandler), "GoToSegment")]
    [HarmonyPostfix]
    public static void Postfix_GenOptions_GoToSegment(Segment s, MapHandler __instance)
    {
        Logger.LogMessage(s);
        if (toggleSkyJungle)
        {
            if (s == Segment.Tropics)
            {
                GameObject.Find("Map/Biome_2/Tropics/Jungle_Segment/Ground").SetActive(false);
                GameObject.Find("Map/Biome_2/Tropics/Jungle_Segment/SkyJungle").SetActive(true);
            }
        }
        if (toggleForceAlpine)
        {
            if (s == Segment.Alpine)
            {
                GameObject.Find("Map/Biome_3/Alpine").SetActive(true);
                GameObject.Find("Map/Biome_3/Mesa").SetActive(false);
            }

        }
        if (toggleForceMesa)
        {
            if (s == Segment.Alpine)
            {
                GameObject.Find("Map/Biome_3/Alpine").SetActive(false);
                GameObject.Find("Map/Biome_3/Mesa").SetActive(true);
            }
        }
        if (toggleAlpineAndMesa)
        {
            if (s == Segment.Alpine)
            {
                if (GameObject.Find("Map/Biome_3/Alpine").activeSelf)
                {
                    GameObject.Find("Map/Biome_3/Mesa").SetActive(true);
                    // foreach (PropSpawner ps in GameObject.Find("Map/Biome_3/Mesa").GetComponentsInChildren<PropSpawner>())
                    // {
                    //     Logger.LogError(ps.transform.parent.name);
                    //     ps.SpawnNew();
                    // }
                }
                else
                {
                    GameObject.Find("Map/Biome_3/Alpine").SetActive(true);
                    // foreach (PropSpawner ps in GameObject.Find("Map/Biome_3/Alpine").GetComponentsInChildren<PropSpawner>())
                    // {
                    //     ps.SpawnNew();
                    // }
                }


            }
        }
    }


    [HarmonyPatch(typeof(PropSpawner), "SpawnNew")]
    [HarmonyPrefix]
    public static bool Prefix_DebugLogging_SpawnNew(bool executeDeferredImmediately, PropSpawner __instance)
    {
        if (__instance.chanceToUseSpawner < 0.999f && UnityEngine.Random.value > __instance.chanceToUseSpawner)
        {
            return false;
        }
        int num = __instance.nrOfSpawns;
        if (__instance.randomSpawns)
        {
            num = UnityEngine.Random.Range(__instance.minSpawnCount, __instance.nrOfSpawns);
        }
        int num2 = 25000;
        int num3 = 5000;
        int num4 = 0;
        while (num4 < num && num2 > 0 && num3 > 0)
        {
            num2--;
            num3--;
            if (__instance.TryToSpawn(num4))
            {
                num4++;
                num3 = 5000;
                if (__instance.syncTransforms)
                {
                    Physics.SyncTransforms();
                }
            }
        }
        if (num2 == 0)
        {
            Debug.LogError("Max attempts reached in PropSpawner, could not spawn all props!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", __instance.gameObject);
            Logger.LogWarning(__instance.gameObject.name);
        }
        if (num3 == 0)
        {
            Debug.LogError("Max attempts IN A ROW reached in PropSpawner, could not spawn all props!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", __instance.gameObject);
            Logger.LogWarning(__instance.gameObject.name);
        }
        __instance.currentSpawns = __instance.transform.childCount;
        __instance.SpawnDecor();
        foreach (PostSpawnBehavior postSpawnBehavior in __instance.postSpawnBehaviors)
        {
            if (!postSpawnBehavior.mute)
            {
                if (executeDeferredImmediately || postSpawnBehavior.DeferredTiming != DeferredStepTiming.AfterCurrentGroupTiming)
                {
                    postSpawnBehavior.RunBehavior(__instance.SpawnedProps);
                }
                else
                {
                    __instance._deferredSteps.Add(postSpawnBehavior.ConstructDeferred(__instance));
                }
            }
        }
        return false;
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
        if (toggleHotSunDisable)
        {
            return false;
        }
        return true;
    }





    [HarmonyPatch(typeof(BakedVolumeLight), "Rebake")]
    [HarmonyPostfix]
    public static void Postfix_RebakeAllLights_Rebake(BakedVolumeLight __instance)
    {
        if (toggleAlpineAndMesa)
        {
            foreach (LightVolume lv in FindObjectsByType<LightVolume>(FindObjectsSortMode.None))
            {
                lv.Bake(null);
            }
        }
    }


    [HarmonyPatch(typeof(LavaRising), "Update")]
    [HarmonyPrefix]
    public static bool Prefix_LavaRisingDisable_Update(LavaRising __instance)
    {
        if (toggleLavaRisingDisable)
        {
            return false;
        }

        if (toggleLavaRisingFaster)
        {
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
            __instance.currentSize -= __instance.speed * 2 * Time.deltaTime;
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
            // if (toggleAlpineAndMesa)
            // {

            //     if (GameObject.Find("Map/Biome_3/Alpine").activeSelf)
            //     {
            //         GameObject.Find("Map/Biome_3/Mesa").SetActive(true);
            //         foreach (PropSpawner ps in GameObject.Find("Map/Biome_3/Mesa").GetComponentsInChildren<PropSpawner>())
            //         {
            //             ps.SpawnNew();
            //         }
            //     }
            //     else
            //     {
            //         GameObject.Find("Map/Biome_3/Alpine").SetActive(true);
            //         foreach (PropSpawner ps in GameObject.Find("Map/Biome_3/Alpine").GetComponentsInChildren<PropSpawner>())
            //         {
            //             ps.SpawnNew();
            //         }
            //     }

            // }
            startingRun = true;
            Peaking();
        }
        else if (scene.name == "Title")
        {
            GameObject.Find("MainMenu/Canvas/MainPage/WhiteFade").GetComponent<Image>().color = UnityEngine.Color.black;
        }
    }

    /**
     _____         _____ _____ _____   ____  _____ _______   _    _ _____ 
    |  __ \ /\    / ____/ ____|  __ \ / __ \|  __ \__   __| | |  | |_   _|
    | |__) /  \  | (___| (___ | |__) | |  | | |__) | | |    | |  | | | |  
    |  ___/ /\ \  \___ \\___ \|  ___/| |  | |  _  /  | |    | |  | | | |  
    | |  / ____ \ ____) |___) | |    | |__| | | \ \  | |    | |__| |_| |_ 
    |_| /_/    \_\_____/_____/|_|     \____/|_|  \_\ |_|     \____/|_____|
                                                                       
                                                                       
    **/
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

            UIHelper passportPage = new("2", UIHelper.UIType.PAGE, "", null, Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel")));
            passportPage.CreateInteractElement();


            UIHelper nextPage = new("NextPageButton", UIHelper.UIType.NAVBUTTON, ">", null, Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")), 0, 570f, -61f);
            nextPage.CreateInteractElement();



            UIHelper prevPage = new("PrevPageButton", UIHelper.UIType.NAVBUTTON, "<", null, Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")), 0, 70, -61f);
            prevPage.CreateInteractElement();



            UIHelper snowDisable = new("SnowDisable", UIHelper.UIType.BUTTON, "Disable Blizzards:", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")), numberOfElements);

            snowDisable.CreateTextElement();
            snowDisable.CreateInteractElement();
            numberOfElements++;


            UIHelper snowInfinite = new("SnowInfinite", UIHelper.UIType.BUTTON, "Infinite Blizzard: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")), numberOfElements);

            snowInfinite.CreateTextElement();
            snowInfinite.CreateInteractElement();

            numberOfElements++;


            UIHelper rainDisable = new("RainDisable", UIHelper.UIType.BUTTON, "Disable Rain: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")), numberOfElements);


            rainDisable.CreateTextElement();
            rainDisable.CreateInteractElement();

            numberOfElements++;


            UIHelper rainInfinite = new("RainInfinite", UIHelper.UIType.BUTTON, "Infinite Rain: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")), numberOfElements);


            rainInfinite.CreateTextElement();
            rainInfinite.CreateInteractElement();

            numberOfElements++;


            UIHelper tornadoFaster = new("TornadoFaster", UIHelper.UIType.BUTTON, "Tornado 10x Faster: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")), numberOfElements);

            tornadoFaster.CreateTextElement();
            tornadoFaster.CreateInteractElement();

            numberOfElements++;

            UIHelper tornadoDisable = new("TornadoDisable", UIHelper.UIType.BUTTON, "Disable Tornados: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")), numberOfElements);

            tornadoDisable.CreateTextElement();
            tornadoDisable.CreateInteractElement();

            numberOfElements++;


            UIHelper hotSunDisable = new("HotSunDisable", UIHelper.UIType.BUTTON, "Disable Sun Heat: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")), numberOfElements);

            hotSunDisable.CreateTextElement();
            hotSunDisable.CreateInteractElement();

            numberOfElements++;


            UIHelper fogFaster = new("FogFaster", UIHelper.UIType.BUTTON, "Fog 2x Faster: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")), numberOfElements);

            fogFaster.CreateTextElement();
            fogFaster.CreateInteractElement();

            numberOfElements++;

            UIHelper fogDisable = new("FogDisable", UIHelper.UIType.BUTTON, "Disable Fog: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")), numberOfElements);

            fogDisable.CreateTextElement();
            fogDisable.CreateInteractElement();

            numberOfElements++;


            UIHelper lavaRisingDisable = new("LavaRisingDisable", UIHelper.UIType.BUTTON, "Disable Rising Lava: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")), numberOfElements);

            lavaRisingDisable.CreateTextElement();
            lavaRisingDisable.CreateInteractElement();

            numberOfElements++;


            UIHelper lavaRisingFaster = new("LavaRisingFaster", UIHelper.UIType.BUTTON, "Lava Rises 2x Faster: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")), numberOfElements);

            lavaRisingFaster.CreateTextElement();
            lavaRisingFaster.CreateInteractElement();

            numberOfElements++;


            UIHelper forceAlpine = new("ForceAlpine", UIHelper.UIType.BUTTON, "Force Alpine: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")), numberOfElements);

            forceAlpine.CreateTextElement();
            forceAlpine.CreateInteractElement();

            numberOfElements++;


            UIHelper forceMesa = new("ForceMesa", UIHelper.UIType.BUTTON, "Force Mesa: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")), numberOfElements);

            forceMesa.CreateTextElement();
            forceMesa.CreateInteractElement();

            numberOfElements++;


            UIHelper alpineAndMesa = new("AlpineAndMesa", UIHelper.UIType.BUTTON, "Alpine And Mesa Gen: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")), numberOfElements);

            alpineAndMesa.CreateTextElement();
            alpineAndMesa.CreateInteractElement();

            numberOfElements++;


            UIHelper skyJungle = new("SkyJungle", UIHelper.UIType.BUTTON, "Sky Jungle Gen: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")), numberOfElements);

            skyJungle.CreateTextElement();
            skyJungle.CreateInteractElement();

            numberOfElements++;



            UIHelper peakToBeach = new("PeakToBeach", UIHelper.UIType.BUTTON, "Peak To Beach: ", Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/Text/Name/Text")), Instantiate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/BG/UI_Close")), numberOfElements);

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

        rainZone = GameObject.Find("Map/Biome_2/Tropics/RainStorm").GetComponent<WindChillZone>();
        snowZone = GameObject.Find("Map/Biome_3/Alpine/SnowStorm").GetComponent<WindChillZone>();
        tornados = GameObject.Find("Map/Biome_3/Mesa/Desert_Segment/Misc/Tornados").GetComponent<TornadoSpawner>();

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
        if (toggleFogDisable)
        {
            GameObject.Find("FogSphereSystem").SetActive(false);
        }

    }




}
