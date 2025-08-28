using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BepInEx.Logging;
using HarmonyLib.Tools;
using System;

namespace PEAKapalooza;

class UIHelper(string internalName, UIHelper.UIType type, string textInfo, GameObject textObject, GameObject interactObject, int index=0, float xpos = 0, float ypos = 0)
{
    public enum UIType
    {
        BUTTON,
        SLIDER,
        PAGE,
        NAVBUTTON
    }

    public int maxRows = 8;
    

    public UnityEngine.Vector3 topText = new(27f, 130f, 0f);
    public UnityEngine.Vector2 topInteract = new(175f, -105);

    public UnityEngine.Vector2 hold;


    public void CreateTextElement()
    {
        textObject.name = internalName + "OptionText";
        textObject.transform.SetParent(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG").transform);
        textObject.transform.localScale = new(0.66f, .66f, .66f);

        hold = new UnityEngine.Vector2(topText.x + (200 * (index / maxRows)), topText.y + (-40 * (index % maxRows)));


        textObject.GetComponent<RectTransform>().anchoredPosition = hold;


        GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + textObject.name).GetComponent<TextMeshProUGUI>().text = textInfo;
        textObject.SetActive(true);

    }

    public void CreateInteractElement()
    {
        if (type == UIType.BUTTON)
        {
            interactObject.transform.SetParent(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG").transform);
            interactObject.name = internalName + "OptionButton";

            interactObject.transform.localScale = new UnityEngine.Vector3(.4f, .4f, .4f);
            interactObject.GetComponent<RectTransform>().anchoredPosition = new UnityEngine.Vector2(topInteract.x + (200 * (index / maxRows)), topInteract.y + (-40 * (index % maxRows)));

            interactObject.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            interactObject.GetComponent<Button>().onClick.AddListener(OptionListener);
            switch (internalName)
            {
                case "SnowDisable":
                    GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(PEAKapalooza.toggleSnowDisable);
                    break;
                case "RainDisable":
                    GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(PEAKapalooza.toggleRainDisable);
                    break;
                case "TornadoDisable":
                    GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(PEAKapalooza.toggleTornadoDisable);
                    break;
                case "TornadoFaster":
                    GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(PEAKapalooza.toggleTornadoFaster);
                    break;
                case "FogDisable":
                    GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(PEAKapalooza.toggleFogDisable);
                    break;
                case "FogFaster":
                    GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(PEAKapalooza.toggleFogFaster);
                    break;
                case "PeakToBeach":
                    GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(PEAKapalooza.togglePeakToBeach);
                    break;
                case "SnowInfinite":
                    GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(PEAKapalooza.toggleSnowInfinite);
                    break;
                case "RainInfinite":
                    GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(PEAKapalooza.toggleRainInfinite);
                    break;
                case "ForceAlpine":
                    GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(PEAKapalooza.toggleForceAlpine);
                    break;
                case "ForceMesa":
                    GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(PEAKapalooza.toggleForceMesa);
                    break;
                case "AlpineAndMesa":
                    GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(PEAKapalooza.toggleAlpineAndMesa);
                    break;
                case "SkyJungle":
                    GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(PEAKapalooza.toggleSkyJungle);
                    break;
                case "HotSunDisable":
                    GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(PEAKapalooza.toggleHotSunDisable);
                    break;
                case "LavaRisingDisable":
                    GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(PEAKapalooza.toggleLavaRisingDisable);
                    break;
                case "LavaRisingFaster":
                    GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(PEAKapalooza.toggleLavaRisingFaster);
                    break;
            }
            

        }
        else if (type == UIType.PAGE)
        {
            interactObject.name = internalName;
            interactObject.transform.SetParent(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel").transform);
            interactObject.transform.localScale = new UnityEngine.Vector3(1, 1, 1);
            interactObject.transform.localPosition = new UnityEngine.Vector3(0f, -228.05f, 0f);
            interactObject.SetActive(false);
            UnityEngine.Object.Destroy(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/Portrait"));
            UnityEngine.Object.Destroy(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/Tabs"));
            UnityEngine.Object.Destroy(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/Options"));
            UnityEngine.Object.Destroy(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/Text"));
        }
        else if (type == UIType.NAVBUTTON)
        {
            interactObject.name = internalName;
            if (internalName == "PrevPageButton")
            {
                interactObject.transform.SetParent(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2").transform);
            }
            else
            {
                interactObject.transform.SetParent(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel").transform);
            }
            interactObject.transform.localScale = new UnityEngine.Vector3(0.66f, .66f, .66f);
            interactObject.GetComponent<RectTransform>().anchoredPosition = new UnityEngine.Vector2(xpos, ypos);


            interactObject.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            interactObject.GetComponent<Button>().onClick.AddListener(OptionListener);



            if (internalName == "PrevPageButton")
            {
                UnityEngine.Object.DestroyImmediate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/" + internalName + "/Box/Icon").GetComponent<RawImage>());

                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/" + internalName + "/Box/Icon").AddComponent<TextMeshProUGUI>();
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/" + internalName + "/Box/Icon").GetComponent<TextMeshProUGUI>().text = textInfo;
                interactObject.SetActive(false);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/" + internalName + "/Box/Icon").GetComponent<RawImage>());

                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/" + internalName + "/Box/Icon").AddComponent<TextMeshProUGUI>();
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/" + internalName + "/Box/Icon").GetComponent<TextMeshProUGUI>().text = textInfo;
            }
        }
    }

    public void OptionListener() {

        switch (internalName)
        {
            case "SnowDisable":
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(!PEAKapalooza.toggleSnowDisable);
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/SnowInfiniteOptionButton/Box/Icon").SetActive(false);
                PEAKapalooza.toggleSnowDisable = !PEAKapalooza.toggleSnowDisable;
                PEAKapalooza.toggleSnowInfinite = false;
                break;
            case "RainDisable":
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(!PEAKapalooza.toggleRainDisable);
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/RainInfiniteOptionButton/Box/Icon").SetActive(false);
                PEAKapalooza.toggleRainDisable = !PEAKapalooza.toggleRainDisable;
                PEAKapalooza.toggleRainInfinite = false;
                break;
            case "TornadoDisable":
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(!PEAKapalooza.toggleTornadoDisable);
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/TornadoFasterOptionButton/Box/Icon").SetActive(false);
                PEAKapalooza.toggleTornadoDisable = !PEAKapalooza.toggleTornadoDisable;
                PEAKapalooza.toggleTornadoFaster = false;
                break;
            case "TornadoFaster":
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(!PEAKapalooza.toggleTornadoFaster);
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/TornadoDisableOptionButton/Box/Icon").SetActive(false);
                PEAKapalooza.toggleTornadoFaster = !PEAKapalooza.toggleTornadoFaster;
                PEAKapalooza.toggleTornadoDisable = false;
                break;
            case "FogDisable":
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(!PEAKapalooza.toggleFogDisable);
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/FogFasterOptionButton/Box/Icon").SetActive(false);
                PEAKapalooza.toggleFogDisable = !PEAKapalooza.toggleFogDisable;
                PEAKapalooza.toggleFogFaster = false;
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/PeakToBeachOptionButton/Box/Icon").SetActive(false);
                PEAKapalooza.togglePeakToBeach = false;
                break;
            case "FogFaster":
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(!PEAKapalooza.toggleFogFaster);
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/FogDisableOptionButton/Box/Icon").SetActive(false);
                PEAKapalooza.toggleFogFaster = !PEAKapalooza.toggleFogFaster;
                PEAKapalooza.toggleFogDisable = false;
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/PeakToBeachOptionButton/Box/Icon").SetActive(false);
                PEAKapalooza.togglePeakToBeach = false;
                break;
            case "PeakToBeach":
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(!PEAKapalooza.togglePeakToBeach);
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/FogFasterOptionButton/Box/Icon").SetActive(false);
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/FogDisableOptionButton/Box/Icon").SetActive(true);
                PEAKapalooza.togglePeakToBeach = !PEAKapalooza.togglePeakToBeach;
                PEAKapalooza.toggleFogDisable = true;
                PEAKapalooza.toggleFogFaster = false;
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/LavaRisingDisableOptionButton/Box/Icon").SetActive(true);
                PEAKapalooza.toggleLavaRisingDisable = true;
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/LavaRisingFasterOptionButton/Box/Icon").SetActive(false);
                PEAKapalooza.toggleLavaRisingFaster = false;
                break;
            case "NextPageButton":
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel").SetActive(false);
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2").SetActive(true);
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/NextPageButton").SetActive(false);
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/PrevPageButton").SetActive(true);
                break;
            case "PrevPageButton":
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel").SetActive(true);
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2").SetActive(false);
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/PrevPageButton").SetActive(false);
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/Panel/NextPageButton").SetActive(true);
                break;
            case "SnowInfinite":
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(!PEAKapalooza.toggleSnowInfinite);
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/SnowDisableOptionButton/Box/Icon").SetActive(false);
                PEAKapalooza.toggleSnowInfinite = !PEAKapalooza.toggleSnowInfinite;
                PEAKapalooza.toggleSnowDisable = false;
                break;
            case "RainInfinite":
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(!PEAKapalooza.toggleRainInfinite);
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/RainDisableOptionButton/Box/Icon").SetActive(false);
                PEAKapalooza.toggleRainInfinite = !PEAKapalooza.toggleRainInfinite;
                PEAKapalooza.toggleRainDisable = false;
                break;
            case "ForceAlpine":
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(!PEAKapalooza.toggleForceAlpine);
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/ForceMesaOptionButton/Box/Icon").SetActive(false);
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/AlpineAndMesaOptionButton/Box/Icon").SetActive(false);
                PEAKapalooza.toggleForceAlpine = !PEAKapalooza.toggleForceAlpine;
                PEAKapalooza.toggleAlpineAndMesa = false;
                PEAKapalooza.toggleForceMesa = false;
                break;
            case "ForceMesa":
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(!PEAKapalooza.toggleForceMesa);
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/ForceAlpineOptionButton/Box/Icon").SetActive(false);
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/AlpineAndMesaOptionButton/Box/Icon").SetActive(false);
                PEAKapalooza.toggleForceMesa = !PEAKapalooza.toggleForceMesa;
                PEAKapalooza.toggleAlpineAndMesa = false;
                PEAKapalooza.toggleForceAlpine = false;
                break;
            case "AlpineAndMesa":
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(!PEAKapalooza.toggleAlpineAndMesa);
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/ForceAlpineOptionButton/Box/Icon").SetActive(false);
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/ForceMesaOptionButton/Box/Icon").SetActive(false);
                PEAKapalooza.toggleForceAlpine = false;
                PEAKapalooza.toggleForceMesa = false;
                PEAKapalooza.toggleAlpineAndMesa = !PEAKapalooza.toggleAlpineAndMesa;
                break;
            case "SkyJungle":
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(!PEAKapalooza.toggleSkyJungle);
                PEAKapalooza.toggleSkyJungle = !PEAKapalooza.toggleSkyJungle;
                break;
            case "HotSunDisable":
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(!PEAKapalooza.toggleHotSunDisable);
                PEAKapalooza.toggleHotSunDisable = !PEAKapalooza.toggleHotSunDisable;
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/ForceAlpineOptionButton/Box/Icon").SetActive(false);
                PEAKapalooza.toggleForceAlpine = false;
                break;
            case "LavaRisingDisable":
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(!PEAKapalooza.toggleLavaRisingDisable);
                PEAKapalooza.toggleLavaRisingDisable = !PEAKapalooza.toggleLavaRisingDisable;
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/PeakToBeachOptionButton/Box/Icon").SetActive(false);
                PEAKapalooza.togglePeakToBeach = false;
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/LavaRisingFasterOptionButton/Box/Icon").SetActive(false);
                PEAKapalooza.toggleLavaRisingFaster = false;
                break;
            case "LavaRisingFaster":
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/" + interactObject.name + "/Box/Icon").SetActive(!PEAKapalooza.toggleLavaRisingFaster);
                PEAKapalooza.toggleLavaRisingFaster = !PEAKapalooza.toggleLavaRisingFaster;
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/LavaRisingDisableOptionButton/Box/Icon").SetActive(false);
                PEAKapalooza.toggleLavaRisingDisable = false;
                GameObject.Find("GAME/PassportManager/PassportUI/Canvas/Panel/2/BG/PeakToBeachOptionButton/Box/Icon").SetActive(false);
                PEAKapalooza.togglePeakToBeach = false;
                break;

        }
    }
}