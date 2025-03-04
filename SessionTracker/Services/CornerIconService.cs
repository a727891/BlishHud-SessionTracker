﻿using System;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework.Graphics;
using SessionTracker.Controls;
using SessionTracker.Settings.Window;

namespace SessionTracker.Services
{
    public class CornerIconService : IDisposable
    {
        public CornerIconService(SettingEntry<bool> cornerIconIsVisibleSetting,
                                 EntriesContainer entriesContainer,
                                 SettingsWindowService settingsWindowService,
                                 EventHandler<MouseEventArgs> cornerIconClickEventHandler,
                                 TextureService textureService)
        {
            _settingsWindowService       = settingsWindowService;
            _cornerIconIsVisibleSetting  = cornerIconIsVisibleSetting;
            _entriesContainer            = entriesContainer;
            _cornerIconClickEventHandler = cornerIconClickEventHandler;
            _cornerIconTexture           = textureService.CornerIconTexture;
            _cornerIconHoverTexture      = textureService.CornerIconHoverTexture;

            cornerIconIsVisibleSetting.SettingChanged += OnCornerIconIsVisibleSettingChanged;

            if (cornerIconIsVisibleSetting.Value)
                CreateCornerIcon();
        }

        public void Dispose()
        {
            _cornerIconIsVisibleSetting.SettingChanged -= OnCornerIconIsVisibleSettingChanged;

            if (_cornerIcon != null)
                RemoveCornerIcon();
        }

        private void CreateCornerIcon()
        {
            RemoveCornerIcon();

            _cornerIcon = new CornerIcon
            {
                Icon             = _cornerIconTexture,
                HoverIcon        = _cornerIconHoverTexture,
                BasicTooltipText = TOOLTIP_TEXT,
                Parent           = GameService.Graphics.SpriteScreen,
            };

            _cornerIcon.Menu  =  CreateContextMenu();
            _cornerIcon.Click += _cornerIconClickEventHandler;
        }

        private ContextMenuStrip CreateContextMenu()
        {
            _contextMenuStrip                   =  new ContextMenuStrip();
            _settingsContextMenuStripItem       =  _contextMenuStrip.AddMenuItem("Settings");
            _resetContextMenuStripItem          =  _contextMenuStrip.AddMenuItem("Reset session stats to 0");
            _settingsContextMenuStripItem.Click += OnSettingsContextMenuStripItemClick;
            _resetContextMenuStripItem.Click    += OnResetContextMenuStripItemClick;
            return _contextMenuStrip;
        }

        private void OnResetContextMenuStripItemClick(object sender, MouseEventArgs e)
        {
            _entriesContainer.ResetSession();
        }

        private void OnSettingsContextMenuStripItemClick(object sender, MouseEventArgs e)
        {
            _settingsWindowService.ShowWindow();
        }

        private void RemoveCornerIcon()
        {
            if (_cornerIcon != null)
            {
                _settingsContextMenuStripItem.Click -= OnSettingsContextMenuStripItemClick;
                _resetContextMenuStripItem.Click    -= OnResetContextMenuStripItemClick;
                _settingsContextMenuStripItem.Dispose();
                _resetContextMenuStripItem.Dispose();
                CreateContextMenu()?.Dispose();
                _cornerIcon.Click -= _cornerIconClickEventHandler;
                _cornerIcon.Dispose();
                _cornerIcon = null;
            }
        }

        private void OnCornerIconIsVisibleSettingChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            if (e.NewValue)
                CreateCornerIcon();
            else
                RemoveCornerIcon();
        }

        private readonly Texture2D _cornerIconTexture;
        private readonly Texture2D _cornerIconHoverTexture;
        private readonly SettingEntry<bool> _cornerIconIsVisibleSetting;
        private readonly EntriesContainer _entriesContainer;
        private readonly EventHandler<MouseEventArgs> _cornerIconClickEventHandler;
        private readonly SettingsWindowService _settingsWindowService;
        private CornerIcon _cornerIcon;
        private ContextMenuStripItem _settingsContextMenuStripItem;
        private ContextMenuStripItem _resetContextMenuStripItem;
        private ContextMenuStrip _contextMenuStrip;

        private const string TOOLTIP_TEXT = "Left click:\n" +
                                            "Show or hide the session tracker UI. " +
                                            "Whether the UI is really shown depends on further visibility settings. " +
                                            "e.g. when 'on world map' is unchecked, clicking this menu icon will still not show the UI on the world map.\n\n" +
                                            "Right click:\n" +
                                            "Open context menu for more options\n\n" +
                                            "This menu icon can be hidden by module settings.";
    }
}