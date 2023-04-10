using System;
using HarmonyLib;
using Newtonsoft.Json;
using Steamworks;
using UnityEngine;

namespace HyperbolusCore
{
    public class Patches
    {
        public static HyperbolusCore core;
        
        #region Multiplayer

        public static void MainGame_Update(MainGame __instance)
        {
            
        }
        
        public static void MainGame_Start(MainGame __instance)
        {
            HyperbolusCore.MainGame = __instance;
        }

        #endregion

        #region Leaderboard

        public static void GameData_recordScore(GameData __instance, LevelData.levelType type, string lvl, string grd, float scr, bool hun, int hrts, int ver, int addPlays = 1)
        {
            object data = new
            {
                plugin = HyperbolusCore.PluginGuid,
                version = HyperbolusCore.PluginVersion,
                token = "test",
                data = new
                {
                    level = lvl,
                    grade = grd,
                    score = scr,
                    hundo = hun,
                    hearts = hrts,
                    version = ver
                }
            };

            string json = JsonConvert.SerializeObject(data);
                            
            HyperbolusCore.ws.Send(json);
        }
        
        #endregion
        
        #region Editor
        public static void clicked_original(EditorMain instance, GameObject obj, int btn, int layer, float oldPerc)
        {
            var trav = Traverse.Create(instance);
            
            trav.Field("waveDragVel").SetValue(0.0f);
            instance.clickPos = instance.mousePos();
            switch (btn)
            {
                case 0:
                    if (obj.name == "waveform" && instance.mode != EditorMain.modes.editPrefab || instance.mode == EditorMain.modes.editPrefab && obj.name == "prefab")
                    {
                        float perc = instance.mousePercX();
                        if (instance.currTool == "add")
                        {
                            if (instance.waveDisplay == EditorMain.displays.bullets)
                            {
                                AccessTools.Method(typeof(EditorMain), "unhilight").Invoke(instance, new object[] {true});
                                if (instance.mode == EditorMain.modes.main)
                                {
                                    var addMarker = AccessTools.Method(typeof(EditorMain), "addMarker");
                                    //instance.updateMarkerTime(instance.addMarker(perc, instance.yPosToLayer(instance.mousePosY()))); original
                                    instance.updateMarkerTime((BulletMarker)addMarker.Invoke(instance, new object[]{oldPerc, layer, null, true, true}));
                                }
                                else if (instance.mode == EditorMain.modes.editPrefab)
                                {
                                    // instance.expandedPrefab.addMarker(perc, instance.yPosToLayer(instance.mousePosY()));
                                }
                                else if (instance.waveDisplay == EditorMain.displays.events)
                                {
                                    //instance.snapAnchor(instance.addAnchor(perc, instance.mousePosY(), true, true));
                                }
                                else if (instance.waveDisplay == EditorMain.displays.extras)
                                {
                                    var ypos = AccessTools.Method(typeof(EditorMain), "yPosToLayer");
                                    var mousey = AccessTools.Method(typeof(EditorMain), "mousePosY");
                                    AccessTools.Method(typeof(EditorMain), "addExtra").Invoke(instance, new object[] {perc, new ExtraMarker((int)ypos.Invoke(instance, new object[] {mousey.Invoke(instance, new object[] {})}), OmniDeck.types.none), instance, true, true, true, true});
                                    //instance.addExtra(perc, new ExtraMarker(instance.yPosToLayer(instance.mousePosY()), OmniDeck.types.none), instance.localSnap(), select: true, sort: true);
                                }

                                instance.madeChange();
                                break;
                            }
                            break;
                        }
                    }

                    if (obj.name == "enemyToggle")
                    {
                        trav.Field("enemyPainting").SetValue(instance.deck.updateEnemy(obj) ? 1 : -1);
                        instance.madeChange();
                        break;
                    }

                    if (obj.name.StartsWith("tog"))
                    {
                        if (instance.waveDisplay != EditorMain.displays.events)
                        {
                            int num = int.Parse(obj.name.Substring(3));
                            //bool[] flagArray = instance.waveDisplay == EditorMain.displays.bullets ? instance.layerVis : instance.extraLayerVis;
                            //flagArray[num - 1] = !flagArray[num - 1];
                            //instance.toggleBtnAlpha(obj, flagArray[num - 1] ? 1f : 0.0f);
                            //instance.canvas.transform.Find("layerVisBars/bar" + (object) num).gameObject.SetActive(!flagArray[num - 1]);

                            if (instance.waveDisplay == EditorMain.displays.extras)
                                instance.refreshExtras();
                        }
                        else
                        {
                            EventData.eventType type = obj.GetComponent<EventInfo>().type;
                            bool flag = true;
                            if (instance.hiddenEvents.Contains(type))
                            {
                                instance.hiddenEvents.Remove(type);
                            }
                            else
                            {
                                instance.hiddenEvents.Add(type);
                                flag = false;
                            }

                            instance.toggleBtnAlpha(obj, flag ? 1f : 0.0f);
                            //instance.canvas.transform.Find("tools/Etoggles/E" + type.ToString()).GetComponent<CanvasGroup>().alpha = flag ? 1f : 0.25f;
                            if (!flag)
                                instance.main.resetEvent(type, false);
                        }

                        //instance.hiddenLayersCheck();
                        instance.madeChange();
                        break;
                    }

                    if (!obj.name.StartsWith("bg"))
                        break;

                    AccessTools.Method(typeof(EditorMain), "deselectAll").Invoke(instance, new object[] {});
                    break;
                case 2:
                    if (obj.name == "enemyToggle")
                    {
                        instance.deck.updateEnemy(obj, true);
                        instance.madeChange();
                        break;
                    }

                    if (!(obj.name == "timebarBG"))
                        break;
                    
                    AccessTools.Method(typeof(EditorMain), "timeJumpToMouse").Invoke(instance, new object[] {});
                    break;
            }
        }

        public static bool EditorMain_clicked(EditorMain __instance, GameObject obj, int btn)
        {
            bool cancel = false;
            switch (btn)
            {
                case 0:
                    if (obj.name == "waveform" && __instance.mode != EditorMain.modes.editPrefab ||
                        __instance.mode == EditorMain.modes.editPrefab && obj.name == "prefab")
                    {
                        if (__instance.currTool == "add")
                        {
                            cancel = true;
                            break;
                        }

                        break;
                    }

                    if (obj.name == "enemyToggle")
                    {
                        cancel = true;
                        break;
                    }

                    if (obj.name.StartsWith("tog"))
                    {
                        cancel = true;
                        break;
                    }

                    break;
                case 2:
                    if (obj.name == "enemyToggle")
                    {
                        cancel = true;
                        break;
                    }

                    break;
            }

            if (cancel)
            {
                var mouseY = AccessTools.Method(typeof(EditorMain), "mousePosY", new Type[] {});
                var yPosToLayer = AccessTools.Method(typeof(EditorMain), "yPosToLayer", new Type[] {typeof(float)});
                float pos = (float)mouseY.Invoke(__instance, new object[] { });
                int layer = (int) yPosToLayer.Invoke(__instance, new object[] { pos });
                float perc = (float)AccessTools.Method(typeof(EditorMain), "mousePercX").Invoke(__instance, new object[] {false, false});
                
                HyperbolusCore.ws.Send("Editor|clicked:" + obj.name + "," + btn + "," + layer + "," + perc);
                return false;
            }

            return true;
        }

        public static void EditorMain_Update(EditorMain __instance)
        {
            // Iterate through queued
            if (HyperbolusCore.actions.Count > 0)
            {
                lock (HyperbolusCore.actions)
                {
                    while (HyperbolusCore.actions.Count > 0)
                    {
                        HyperbolusCore.onMessage(HyperbolusCore.actions.Dequeue());
                    }
                }
            }
        }

        public static void EditorMain_Start(EditorMain __instance)
        {
            HyperbolusCore.editor = __instance;
        }
        #endregion
    }
}