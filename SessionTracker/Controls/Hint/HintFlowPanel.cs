﻿using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using SessionTracker.Models;
using SessionTracker.Services;
using SessionTracker.Settings;
using SessionTracker.Settings.SettingEntries;
using SessionTracker.Settings.Window;

namespace SessionTracker.Controls.Hint
{
    public class HintFlowPanel : FlowPanel
    {
        public HintFlowPanel(List<Entry> entries,
                             SettingsWindowService settingsWindowService,
                             TextureService textureService,
                             SettingService settingService,
                             Container parent)
        {
            _entries        = entries;
            _parent         = parent;
            _settingService = settingService;

            Parent = parent;

            _hiddenByZeroSessionValuesImage = new Image(textureService.HiddenStatsTexture)
            {
                Size = new Point(30),
                BasicTooltipText = "All stats are hidden because their current session values are 0.\n" +
                                   "Stats will be visible again when the session value is not 0 anymore.\n" +
                                   "This hidden-when-zero-feature can be turned off in the session tracker module settings."
            };

            var hiddenByUserHintText = "No stats selected.\n" +
                                       "Select the stats you want to see\n" +
                                       "in the session tracker module settings. :)";

            _hiddenByUserLabel = new Label()
            {
                Text             = hiddenByUserHintText,
                BasicTooltipText = hiddenByUserHintText,
                ShowShadow       = true,
                AutoSizeHeight   = true,
                AutoSizeWidth    = true,
            };

            _openSettingsButton = new OpenSettingsButton(settingsWindowService);

            OnFontSizeIndexSettingChanged(null, null);
            settingService.FontSizeIndexSetting.SettingChanged += OnFontSizeIndexSettingChanged;
        }

        protected override void DisposeControl()
        {
            _settingService.FontSizeIndexSetting.SettingChanged -= OnFontSizeIndexSettingChanged;

            _hiddenByZeroSessionValuesImage?.Dispose();
            _hiddenByUserLabel?.Dispose();
            _openSettingsButton?.Dispose();
            base.DisposeControl();
        }

        public void ShowHintWhenAllEntriesAreHidden()
        {
            // remove all from parent to prevent messing up their order
            _hiddenByZeroSessionValuesImage.Parent = null;
            _hiddenByUserLabel.Parent              = null;
            _openSettingsButton.Parent                 = null;

            var hintType = DetermineWhichHintToShow(_entries, _settingService.HideStatsWithValueZeroSetting.Value);

            switch (hintType)
            {
                case HintType.AllStatsHiddenByUser:
                    _hiddenByZeroSessionValuesImage.Parent = null;
                    _hiddenByUserLabel.Parent              = this;
                    _openSettingsButton.Parent                 = this;
                    Show();
                    break;
                case HintType.AllStatsHiddenBecauseOfZeroValue:
                    _hiddenByZeroSessionValuesImage.Parent = this;
                    _hiddenByUserLabel.Parent              = null;
                    _openSettingsButton.Parent                 = null;
                    Show();
                    break;
                case HintType.None:
                default:
                    _hiddenByZeroSessionValuesImage.Parent = null;
                    _hiddenByUserLabel.Parent              = null;
                    _openSettingsButton.Parent                 = null;
                    Hide();
                    break;
            }
        }

        public override void Show()
        {
            Parent = _parent;
            base.Show();
        }

        public override void Hide()
        {
            Parent = null;
            base.Hide();
        }

        private void OnFontSizeIndexSettingChanged(object sender, ValueChangedEventArgs<int> e)
        {
            var fontSizeIndex = _settingService.FontSizeIndexSetting.Value;
            _hiddenByUserLabel.Font              = FontService.Fonts[fontSizeIndex];
            _hiddenByZeroSessionValuesImage.Size = new Point(5 * fontSizeIndex);
        }

        private static HintType DetermineWhichHintToShow(List<Entry> entries, bool hideStatsWithValueZero)
        {
            var allHiddenByUser = entries.Any(e => e.IsVisible) == false;

            if (allHiddenByUser)
                return HintType.AllStatsHiddenByUser;

            var allHiddenBecauseOfZeroValue = entries.Any(e => e.IsVisible && e.Value.Session != 0) == false;

            if (hideStatsWithValueZero && allHiddenBecauseOfZeroValue)
                return HintType.AllStatsHiddenBecauseOfZeroValue;

            return HintType.None;
        }

        private readonly SettingService _settingService;
        private readonly List<Entry> _entries;
        private readonly Container _parent;
        private readonly Label _hiddenByUserLabel;
        private readonly Image _hiddenByZeroSessionValuesImage;
        private readonly OpenSettingsButton _openSettingsButton;
    }
}