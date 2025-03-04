using CZAutosplitter.Memory;
using LiveSplit.ComponentUtil;
using LiveSplit.Web.Share;
using System;
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

        public bool Update()
        {
            OldCutsceneIDString = CutsceneIDString;
            CutsceneID = TCPFunctions.RequestMemory(0xC8E63EBC, 4);
            InLoad = TCPFunctions.RequestMemory(0xC8E63FB8, 1).ElementAt(0) != 0;
            InCutscene = TCPFunctions.RequestMemory(0xC9355B3E, 1).ElementAt(0) != 0;
            CutsceneIDString = Encoding.UTF8.GetString(CutsceneID);
            return true;
        }

        public bool Start()
        {
            if (OldCutsceneIDString == "700_" && CutsceneIDString != "700_")
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
                Splits.Clear();
                return true;
            }
            return false;
        }

        public void OnReset()
        {
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
