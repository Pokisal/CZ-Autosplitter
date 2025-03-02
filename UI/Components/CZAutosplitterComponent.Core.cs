﻿using CZAutosplitter.Memory;
using LiveSplit.Model;
using LiveSplit.Model.Input;
using LiveSplit.UI;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows.Forms;

namespace CZAutosplitter.UI.Components
{
    public partial class CZAutosplitter : LiveSplit.UI.Components.IComponent
    {
        public string ComponentName => "CZ Autosplitter";

        public float HorizontalWidth => 0;
        public float MinimumHeight => 0;
        public float VerticalHeight => 0;
        public float MinimumWidth => 0;
        public float PaddingTop => 0;
        public float PaddingBottom => 0;
        public float PaddingLeft => 0;
        public float PaddingRight => 0;

        public IDictionary<string, Action> ContextMenuControls => null;
        public void DrawHorizontal(System.Drawing.Graphics g, LiveSplitState state, float height, System.Drawing.Region clipRegion) { }
        public void DrawVertical(System.Drawing.Graphics g, LiveSplitState state, float width, System.Drawing.Region clipRegion) { }

        public AutosplitterSettings Settings { get; set; }

        public LiveSplitState State { get; set; }
        public TimerModel Timer { get; set; }

        public EventHandler StartEvent => (sender, e) => { OnStart(); };
        public EventHandlerT<TimerPhase> ResetEvent => (sender, e) => { OnReset(); };
        public EventHandler SplitEvent => (sender, e) => { OnSplit(); };

        public CZAutosplitter(LiveSplitState state)
        {
            State = state;
            Timer = new TimerModel() { CurrentState = state };
            Settings = new AutosplitterSettings();
            State.OnStart += StartEvent;
            State.OnReset += ResetEvent;
            Timer.OnSplit += SplitEvent;
            Startup();
        }

        public void Dispose()
        {
            Shutdown();
            State.OnStart -= StartEvent;
            State.OnReset -= ResetEvent;
            Timer.OnSplit -= SplitEvent;
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            if (Settings == null) throw new NullReferenceException("Settings control is not initialised");
            return Settings.GetSettings(document);
        }

        public Control GetSettingsControl(LayoutMode mode)
        {
            Settings.Mode = mode;
            return Settings;
        }

        public void SetSettings(XmlNode settings)
        {
            if (Settings == null) throw new NullReferenceException("Settings control is not initialised");
            Settings.SetSettings(settings);
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (!TCPFunctions.IsGameRunning()) return;
            if (!Update()) return;
            if (Timer.CurrentState.CurrentPhase == TimerPhase.Running)
            {
                Timer.CurrentState.IsGameTimePaused = IsLoading();
                Timer.CurrentState.SetGameTime(GameTime());
                if (Settings.Reset && Reset()) Timer.Reset();
                else if (Settings.Split && Split()) Timer.Split();
            }
            else if (Settings.Start && Start())
            {
                Timer.CurrentState.IsGameTimePaused = true;
                Timer.Start();
            }
        }
    }
}
