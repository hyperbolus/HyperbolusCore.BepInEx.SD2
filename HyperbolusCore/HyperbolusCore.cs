using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using WebSocketSharp;

namespace HyperbolusCore
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class HyperbolusCore : BaseUnityPlugin
    {
        // Plugin metadata
        public const string PluginName = "Hyperbolus Core";
        public const string PluginGuid = "net.hyperbolus.s2.mods.leaderboards";
        public const string PluginVersion = "0.1.0-alpha";
        
        public static WebSocket ws;
        public static MainGame MainGame;
        public static EditorMain editor;
        
        // Incoming WebSocket events. Queued until Update() Patch
        public static Queue<MessageEventArgs> actions = new Queue<MessageEventArgs>();
        
        public struct Patch
        {
            public Type type;
            public Type[] parameters;
            
        }
        public void Awake()
        {
            Logger.LogInfo(PluginName + " " + PluginVersion);
            Harmony harmony = new Harmony(PluginGuid);

            ws = new WebSocket("ws://127.0.0.1:55037");
            ws.OnMessage += (sender, e) =>
            {
                actions.Enqueue(e);
            };
            ws.Connect();
            ws.Send("Main|Init:");

            Patches.core = this;

            #region Patches
            Dictionary<string, Patch> patches = new Dictionary<string, Patch>()
            {
                /*
                 * Multiplayer
                 */
                
                {
                    "MainGame.Start",
                    new Patch() {
                        type = typeof(MainGame),
                        parameters = new Type[] {}
                            
                    }
                },
                
                /*
                 * Leaderboards
                 */
                
                {
                    "MainGame.Update",
                    new Patch() {
                        type = typeof(MainGame),
                        parameters = new Type[] {}
                            
                    }
                },
                {
                    "GameData.recordScore",
                    new Patch() {
                        type = typeof(GameData),
                        parameters = new Type[] {typeof(LevelData), typeof(string), typeof(string), typeof(float), typeof(bool), typeof(int), typeof(int), typeof(int)}
                            
                    }
                },
                
                /*
                 * Editor Stuff
                 */
                
                // {
                //     "EditorMain.Update",
                //     new Patch() {
                //         type = typeof(EditorMain),
                //         parameters = new Type[] {}
                //         
                //     }
                // },
                // {
                //     "EditorMain.Start",
                //     new Patch() {
                //         type = typeof(EditorMain),
                //         parameters = new Type[] {}
                //         
                //     }
                // },
                // "EditorMain.addBookmark",
                // "EditorMain.adjustGrid",
                // "EditorMain.breakPrefab",
                // {
                //     "EditorMain.clicked",
                //     new Patch() {
                //         type = typeof(EditorMain),
                //         method = "clicked",
                //         parameters = new Type[] {typeof(GameObject), typeof(int)}
                //     }
                // }
                // "EditorMain.createPrefab",
                // "EditorMain.delete",
                // "EditorMain.distribute",
                // "EditorMain.multiDragOthers",
                // "EditorMain.pasteSelection",
                // "EditorMain.removeBookmark",
                // "EditorMain.resetAnchors",
                // "EditorMain.resizeDurBar",
                // "EditorMain.resizePrefab",
                // "EditorMain.reverseAnchors",
                // "EditorMain.saveData",
                // "EditorMain.shiftSelection",
                // "EditorMain.timeBarDrag",
                // "EditorMain.toggleColors",
                // "EditorMain.toggleSettings",
                // "EditorMain.toolClicked",
                // "MarkerDeck.clicked",
                // "MarkerDeck.DropdownChanged",
                // "MarkerDeck.inputTyped",
                // "MarkerDeck.sliderToInput",
                // "OmniDeck.clicked",
                // "OmniDeck.dropdownChanged",
                // "OmniDeck.inputTyped",
                // "OmniDeck.sliderChanged"
            };

            //Harmony.ReversePatch(AccessTools.Method(typeof(EditorMain), "clicked", new Type[] {typeof(GameObject), typeof(int)}), new HarmonyMethod(AccessTools.Method(typeof(Patches), "clicked_original")));
            
            foreach (KeyValuePair<string, Patch> entry in patches)
            {
                string method = entry.Key.Split('.')[1];
                Logger.LogInfo("Patching " + entry.Value.type + " method " + method);
                
                MethodInfo original = AccessTools.Method(entry.Value.type, method, entry.Value.parameters);
                MethodInfo patched = AccessTools.Method(typeof(Patches), entry.Key.Split('.')[0] + '_' + method);
            
                harmony.Patch(original, new HarmonyMethod(patched));
            }
            #endregion
        }
        
        public static void onMessage(MessageEventArgs e)
        {
            string[] command = e.Data.ToString().Split(':')[0].Split('|');
            string[] parameters = e.Data.ToString().Split(':')[1].Split(',');

            Patches.core.Logger.LogInfo("[server] " + e.Data.ToString());

            switch (command[0])
            {
                case "Editor":
                    switch (command[1])
                    {
                        case "clicked":
                            GameObject obj = GameObject.Find(parameters[0]);
                            Patches.clicked_original(editor, obj, int.Parse(parameters[1]), int.Parse(parameters[2]), float.Parse(parameters[3]));
                            break;
                    }
                    break;
                case "Main":
                    break;
                case "Error":
                    break;
                case "Info":
                    break;
            }
        }

        public void log(string message)
        {
            Logger.LogInfo(message);
        }
    }
}