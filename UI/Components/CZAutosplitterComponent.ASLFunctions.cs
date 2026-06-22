using CZAutosplitter.Memory;
using LiveSplit.ComponentUtil;
using LiveSplit.Web.Share;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Data.OleDb;
using System.Dynamic;
using LiveSplit.Options;
using System.Windows.Forms;

namespace CZAutosplitter.UI.Components
{
    public partial class CZAutosplitter : LiveSplit.UI.Components.IComponent
    {
        private static ProcessMemory GameMemory;
        private static Process GameProcess;

        public long RamBase = 0x200000000;

        public byte[] CutsceneID = new byte[4] { 0, 0, 0, 0 };
        public bool InLoad;
        public bool InCutscene;
        public bool OldInLoad;
        public bool OldInCutscene;
        public string CutsceneIDString;
        public string OldCutsceneIDString;

        public HashSet<string> Splits = new HashSet<string>();
        public Dictionary<string, string> Cutscenes = new Dictionary<string, string>
        {
            { "703_", "Case0-1" },
            { "703a", "Case0-2" },
            { "704_", "Case0-3" },
            { "706_", "Case0-4" },
            { "707_", "Zombrex1" },
            { "708_", "Jed" },
            { "710_", "EndingA" }
        };
        public void Startup()
        {
        }

        public void Init()
        {
        }

        public IntPtr GetIntPtr(long input)
        {
            if (input >= 0xC0000000)
            {
                input += 0x4E000;
            }
            return (IntPtr)(RamBase + input);
        }

        public void StartProcessActions()
        {
            if (GameMemory != null && !GameMemory.CheckProcess())
            {
                GameMemory = null;
                return;
            }
            if (GameMemory == null)
            {
                GameMemory = new ProcessMemory(GameProcess);
            }
            if (!GameMemory.IsProcessStarted())
            {
                GameMemory.StartProcess();
            }
        }

        public void Update()
        {
            OldCutsceneIDString = CutsceneIDString;
            OldInLoad = InLoad;

            Process[] processesByName = Process.GetProcessesByName("xenia");
            Process[] processesByName2 = Process.GetProcessesByName("xenia_canary");
            processesByName = processesByName.Concat(processesByName2).ToArray();

            if (processesByName.Length != 0 && (GameProcess == null || processesByName[0].Id.ToString("X8") != GameProcess.Id.ToString("X8")))
            {
                GameProcess = processesByName[0];
            }

            if (processesByName.Length == 0)
            {
                CutsceneID = TCPFunctions.RequestMemory(0xC8E63EBC, 4, CutsceneID);
                InLoad = TCPFunctions.RequestMemory(0xC8E63FB8, 1, BitConverter.GetBytes(InLoad)).ElementAt(0) != 0;
                /// For whatever reason this InCutscene address does not work on Xenia so I have to use the old one there.
                InCutscene = TCPFunctions.RequestMemory(0xC301B497, 1, BitConverter.GetBytes(InCutscene)).ElementAt(0) != 0;
                CutsceneIDString = Encoding.UTF8.GetString(CutsceneID);
            }
            else
            {
                StartProcessActions();
                try
                {
                    var offset = GameMemory.ReadShort(GetIntPtr(0x82000000));

                    if (offset != 23117)
                    {
                        RamBase = 0x100000000;
                    }

                    CutsceneIDString = GameMemory.ReadStringAscii(GetIntPtr(0xC8E63EBC), 4);
                    InLoad = GameMemory.ReadByte(GetIntPtr(0xC8E63FB8)) != 0;
                    InCutscene = GameMemory.ReadByte(GetIntPtr(0xCA474D0F)) != 0;
                }
                catch (NullReferenceException)
                {
                    return;
                }
            }
        }
        public bool Start()
        {
            if (OldCutsceneIDString != "701_" && CutsceneIDString == "701_")
            {
                return true;
            }
            return false;
        }

        public void OnStart()
        {
        }

        public bool IsLoading()
        {
            if (InCutscene || InLoad)
            {
                return true;
            }
            return false;
        }

        public TimeSpan? GameTime()
        {
            return null;
        }

        public bool Reset()
        {
            if (CutsceneIDString == "700_" && OldCutsceneIDString != "700_")
            {
                return true;
            }
            return false;
        }

        public void OnReset()
        {
            Splits.Clear();
        }

        public bool Split()
        {
            for (int i = 0; i < Cutscenes.Count; ++i)
            {
                if (CutsceneIDString == Cutscenes.Keys.ElementAt(i) && Settings.ContainsSplit(Cutscenes.Values.ElementAt(i)) && !Splits.Contains(Cutscenes.Values.ElementAt(i)))
                {
                    Splits.Add(Cutscenes.Values.ElementAt(i));
                    return true;
                }
            }
            return false;
        }

        public void OnSplit()
        {
        }

        public void Exit()
        {
        }

        public void Shutdown()
        {
        }

    }
}
