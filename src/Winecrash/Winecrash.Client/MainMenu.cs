﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Winecrash.Engine;
using Winecrash.Engine.GUI;

namespace Winecrash.Game
{
    public static class MainMenu
    {
        private static WObject MenuWobject = null;
        private static WObject MainMenuPanel = null;
        private static WObject MenuBGWObject = null;
        private static WObject OptionPanel = null;
        private static void CreateMenu()
        {
            MenuWobject = new WObject("Main Menu");
            MenuBGWObject = new WObject("Main Menu Background Object");
            MenuBGWObject.Parent = MenuWobject;
            MenuBGWObject.AddModule<MenuBackgroundControler>();

            WObject bgPanel = new WObject("Background Panel") { Parent = MenuWobject };
            bgPanel.Parent = MenuWobject;

            Label lbVersion = bgPanel.AddModule<Label>();
            lbVersion.Text = "Winecrash Alpha 0.0.1";
            lbVersion.Aligns = TextAligns.Down | TextAligns.Left;
            lbVersion.AutoSize = true;
            lbVersion.MinAnchor = new Vector2F(0.0F, 0.0F);
            lbVersion.MaxAnchor = new Vector2F(1.0F, 0.05F);
            lbVersion.Color = Color256.White;
            lbVersion.Left = 20.0D;

            Label lbCopyright = bgPanel.AddModule<Label>();
            lbCopyright.Text = "Copyright Arekva 2020\nAll Rights Reserved";
            lbCopyright.Aligns = TextAligns.Down | TextAligns.Right;
            lbCopyright.AutoSize = true;
            lbCopyright.MinAnchor = new Vector2F(0.0F, 0.0F);
            lbCopyright.MaxAnchor = new Vector2F(1.0F, 0.1F);
            lbCopyright.Color = Color256.White;
            lbCopyright.Right = 20.0D;


            CreateMain();
            CreateOptions();
        }

        private static void CreateMain()
        {
            WObject mainPanel = MainMenuPanel = new WObject("Main UI Panel") { Parent = MenuWobject };
            Image mainPanelImg = mainPanel.AddModule<Image>();
            mainPanelImg.Color = new Color256(1.0, 0.0, 1.0, 0.0);
            mainPanelImg.MinAnchor = new Vector2F(0.225F, 0.10F);
            mainPanelImg.MaxAnchor = new Vector2F(0.775F, 0.95F);
            mainPanelImg.MinSize = new Vector3F(400.0F, 400.0F, Single.PositiveInfinity);
            mainPanelImg.MaxSize = new Vector3F(800.0F, 800.0F, Single.PositiveInfinity);
            mainPanelImg.KeepRatio = true;

            WObject logo = new WObject("Game Text Logo") { Parent = mainPanel };
            Image logoImage = logo.AddModule<Image>();
            logoImage.Picture = new Texture("assets/textures/logo.png");
            logoImage.MinAnchor = new Vector2F(0.0F, 0.8F);
            logoImage.MaxAnchor = new Vector2F(1.0F, 1.0F);
            logoImage.KeepRatio = true;

            Label lbTip = mainPanel.AddModule<Label>();
            lbTip.ParentGUI = logoImage;
            lbTip.Text = "Minecraft";
            lbTip.Color = new Color256(1.0, 1.0, 0.0, 1.0);
            lbTip.Aligns = TextAligns.Middle;
            lbTip.AutoSize = true;
            lbTip.MinAnchor = new Vector2F(0.7F, 0.0F);
            lbTip.MaxAnchor = new Vector2F(1.1F, 0.4F);
            lbTip.Rotation = -20.0D;


            MenuTip tip = MenuWobject.AddModule<MenuTip>();
            tip.ReferenceLabel = lbTip;
            lbTip.Text = tip.SelectRandom();


            WObject btnPanel = new WObject("Main UI Button Panel") { Parent = mainPanel };
            Image btnPanelImg = btnPanel.AddModule<Image>();
            btnPanelImg.Color = new Color256(1.0, 0.0, 1.0, 0.0);
            btnPanelImg.MinAnchor = new Vector2F(0.075F, 0.0F);
            btnPanelImg.MaxAnchor = new Vector2F(0.925F, 0.6F);

            WObject single = new WObject("Singleplayer Button") { Parent = btnPanel };
            UI.LargeButton btnSingle = single.AddModule<UI.LargeButton>();
            btnSingle.Button.MinAnchor = new Vector2F(0.0F, 0.9F);
            btnSingle.Button.MaxAnchor = new Vector2F(1.0F, 1.0F);
            btnSingle.Button.Label.Text = "Singleplayer";
            btnSingle.Button.OnClick += () => { Graphics.Window.InvokeUpdate(() => Program.RunGameDebug()); };

            WObject mult = new WObject("Multiplayer Button") { Parent = btnPanel };
            UI.LargeButton btnMult = mult.AddModule<UI.LargeButton>();
            btnMult.Button.MinAnchor = new Vector2F(0.0F, 0.7F);
            btnMult.Button.MaxAnchor = new Vector2F(1.0F, 0.8F);
            btnMult.Button.Label.Text = "Multiplayer";
            btnMult.Button.Locked = true;

            WObject mods = new WObject("Mods Button") { Parent = btnPanel };
            UI.LargeButton btnMods = mods.AddModule<UI.LargeButton>();
            btnMods.Button.MinAnchor = new Vector2F(0.0F, 0.5F);
            btnMods.Button.MaxAnchor = new Vector2F(1.0F, 0.6F);
            btnMods.Button.Label.Text = "Mods and Texture Packs";
            btnMods.Button.Locked = true;

            WObject options = new WObject("Options Button") { Parent = btnPanel };
            UI.SmallButton btnOptions = options.AddModule<UI.SmallButton>();
            btnOptions.Button.MinAnchor = new Vector2F(0.0F, 0.2F);
            btnOptions.Button.MaxAnchor = new Vector2F(0.45F, 0.3F);
            btnOptions.Button.Label.Text = "Options";
            btnOptions.Button.OnClick += () => ShowOptions();

            WObject quit = new WObject("Quit Button") { Parent = btnPanel };
            UI.SmallButton btnQuit = quit.AddModule<UI.SmallButton>();
            btnQuit.Button.MinAnchor = new Vector2F(0.55F, 0.2F);
            btnQuit.Button.MaxAnchor = new Vector2F(1.0F, 0.3F);
            btnQuit.Button.Label.Text = "Quit Game";
            btnQuit.Button.OnClick += () => WEngine.Stop();
        }

        public static void CreateOptions()
        {
            OptionPanel = new WObject("Main Menu Options");

        }

        private static void ShowOptions()
        {
            OptionPanel.Enabled = true;
            HideMain();
        }

        private static void HideOptions()
        {
            OptionPanel.Enabled = false;
        }
        private static void ShowMain()
        {
            MainMenuPanel.Enabled = true;
            HideOptions();
        }
        private static void HideMain()
        {
            MainMenuPanel.Enabled = false;
        }

        public static void Show()
        {
            if (!MenuWobject)
            {
                CreateMenu();
                CreateOptions();
            }


            Camera.Main.FOV = 80.0D;
            Camera.Main.WObject.LocalRotation = new Quaternion(25, 0, 0);

            ShowMain();
        }

        public static void Hide()
        {
            Camera.Main.FOV = 45.0D;
            Camera.Main.WObject.LocalRotation = new Quaternion(0, 0, 0);
            MainMenuPanel.Enabled = false;
        }
    }
}
