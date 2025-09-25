using BepInEx;
using ComputerInterface;
using ComputerInterface.Extensions;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;
using ComputerInterface.Views.GameSettings;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace CI_Background_Switcher
{
    [BepInPlugin("ngbatz.cibackgroundswitcher", "CI Background Switcher", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {

        public static Texture2D currentTexture;

        void Start()
        {
            var harmony = Harmony.CreateAndPatchAll(GetType().Assembly, "ngbatz.cibackgroundswitcher");
            GorillaTagger.OnPlayerSpawned(OnGameInitialized);
        }

        void OnGameInitialized()
        {
            Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "Backgrounds"));
        }
    }

    public class GameSettingsEntry : IComputerModEntry
    {
        public string EntryName => "Background Switcher";
        public Type EntryViewType => typeof(JohnView);
    }

    public class BGView : ComputerView
    {
        private UIElementPageHandler<Tuple<string, string>> _pageHandler;
        private UISelectionHandler _selectionHandler;

        private List<Tuple<string, string>> _johnViews;

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            _johnViews = new List<Tuple<string, string>>();

            foreach(string file in Directory.GetFiles(Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "Backgrounds"), "*.png", SearchOption.AllDirectories))
            {
                _johnViews.Add(new(Path.GetFileNameWithoutExtension(file), file));
            }

            _pageHandler = new UIElementPageHandler<Tuple<string, string>>(EKeyboardKey.Left, EKeyboardKey.Right)
            {
                Footer = "<color=#ffffff50>{0}{1}        <align=\"right\"><margin-right=2em>page {2}/{3}</margin></align></color>",
                NextMark = "▼",
                PrevMark = "▲",
                EntriesPerPage = 11
            };
            _pageHandler.SetElements(_johnViews.ToArray());

            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter);
            _selectionHandler.OnSelected += ItemSelected;
            _selectionHandler.MaxIdx = _johnViews.Count - 1;
            _selectionHandler.ConfigureSelectionIndicator("<color=#ed6540>></color> ", "", "  ", "");
            Redraw();
        }

        private void ItemSelected(int obj)
        {
            var tex = new Texture2D(2, 2);
            tex.LoadImage(File.ReadAllBytes(_johnViews[obj].Item2));

            Plugin.currentTexture = tex;

            ComputerInterface.Plugin.CustomComputer.SetBGImage(new ComputerViewChangeBackgroundEventArgs(tex));
        }

        private void Redraw()
        {
            StringBuilder str = new();

            str.AppendClr("== ", "ffffff50").Append("Background Switcher").AppendClr(" ==", "ffffff50").EndAlign().AppendLines(2);

            int lineIdx = _pageHandler.MovePageToIdx(_selectionHandler.CurrentSelectionIndex);

            _pageHandler.EnumarateElements((entry, idx) =>
            {
                str.Append(_selectionHandler.GetIndicatedText(idx, lineIdx, entry.Item1));
                str.AppendLine();
            });

            for (int i = 0; i < _pageHandler.EntriesPerPage - _pageHandler.ItemsOnScreen; i++)
            {
                str.AppendLine();
            }
            str.Append($"<color=#ffffff50><align=\"center\"><  {_pageHandler.CurrentPage + 1}/{_pageHandler.MaxPage + 1}  ></align></color>");

            Text = str.ToString();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_selectionHandler.HandleKeypress(key))
            {
                Redraw();
                return;
            }

            switch (key)
            {
                case EKeyboardKey.Back:
                    ShowView<JohnView>();
                    break;
            }
        }
    }
    public class BGCRView : ComputerView
    {
        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            Redraw();
        }

        private void Redraw()
        {
            StringBuilder str = new();

            str.AppendClr("== ", "ffffff50").Append("Credits").AppendClr(" ==", "ffffff50").EndAlign().AppendLines(2);

            str.Append("Ngbatz - Made the mod\nRainwave - Idea");

            Text = str.ToString();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            switch (key)
            {
                case EKeyboardKey.Back:
                    ShowView<JohnView>();
                    break;
            }
        }
    }

    public class JohnView : ComputerView
    {
        private readonly UIElementPageHandler<Tuple<string, Type>> _pageHandler;
        private readonly UISelectionHandler _selectionHandler;

        private readonly List<Tuple<string, Type>> _johnViews;

        public JohnView()
        {
            _johnViews = new List<Tuple<string, Type>>
            {
                new("Backgrounds", typeof(BGView)),
                new("Credits    ", typeof(BGCRView))
            };

            _pageHandler = new UIElementPageHandler<Tuple<string, Type>>(EKeyboardKey.Left, EKeyboardKey.Right)
            {
                Footer = "<color=#ffffff50>{0}{1}        <align=\"right\"><margin-right=2em>page {2}/{3}</margin></align></color>",
                NextMark = "▼",
                PrevMark = "▲",
                EntriesPerPage = 11
            };
            _pageHandler.SetElements(_johnViews.ToArray());

            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter);
            _selectionHandler.OnSelected += ItemSelected;
            _selectionHandler.MaxIdx = _johnViews.Count - 1;
            _selectionHandler.ConfigureSelectionIndicator("<color=#ed6540>></color> ", "", "  ", "");
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            Redraw();
        }

        private void Redraw()
        {
            StringBuilder str = new();

            str.BeginCenter().AppendClr("== ", "ffffff50").Append("Background Switcher").AppendClr(" ==", "ffffff50").EndAlign().AppendLines(2);

            int lineIdx = _pageHandler.MovePageToIdx(_selectionHandler.CurrentSelectionIndex);

            _pageHandler.EnumarateElements((entry, idx) =>
            {
                str.Append(_selectionHandler.GetIndicatedText(idx, lineIdx, entry.Item1));
                str.AppendLine();
            });

            for (int i = 0; i < _pageHandler.EntriesPerPage - _pageHandler.ItemsOnScreen; i++)
            {
                str.AppendLine();
            }
            str.Append($"<color=#ffffff50><align=\"center\"><  {_pageHandler.CurrentPage + 1}/{_pageHandler.MaxPage + 1}  ></align></color>");

            Text = str.ToString();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_selectionHandler.HandleKeypress(key))
            {
                Redraw();
                return;
            }

            switch (key)
            {
                case EKeyboardKey.Back:
                    ReturnToMainMenu();
                    break;
            }
        }

        private void ItemSelected(int idx)
        {
            ShowView(_johnViews[_selectionHandler.CurrentSelectionIndex].Item2);
        }
    }

    [HarmonyPatch(typeof(CustomComputer), nameof(CustomComputer.SetBGImage))]
    public static class BGPatch
    {
        [HarmonyPrefix]
        public static void Prefix(ref ComputerViewChangeBackgroundEventArgs args, CustomComputer __instance)
        {
            if (Plugin.currentTexture != null)
            {
                args = new ComputerViewChangeBackgroundEventArgs(Plugin.currentTexture);
            }
        }
    }
}