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
        private static void CreateMenu()
        {
            MenuWobject = new WObject("Main Menu");

            //Panel
            WObject bgPanel = new WObject("Background Panel") { Parent = MenuWobject };
            bgPanel.Parent = MenuWobject;

            //BG Image
            Image bgImage = bgPanel.AddModule<Image>();
            bgImage.Picture = new Texture("assets/textures/menu_bg.png");
            bgImage.AutoTile = true;
            bgImage.AutoTileScale = 1/2.0F;

            Label lb = bgPanel.AddModule<Label>();
            lb.Text = "Winecrash Alpha 0.0.1";
            lb.Aligns = TextAligns.Up | TextAligns.Left;
            lb.AutoSize = true;
            lb.MinAnchor = new Vector2F(0.0F, 0.95F);
            lb.MaxAnchor = new Vector2F(1.0F, 1.0F);
            lb.Color = Color256.DarkGray * 1.25F;
            

            WObject mainPanel = new WObject("Main UI Panel") { Parent = MenuWobject };
            Image mainPanelImg = mainPanel.AddModule<Image>();
            mainPanelImg.Color = new Color256(1.0, 0.0, 1.0, 0.0);
            mainPanelImg.MinAnchor = new Vector2F(0.225F, 0.10F);
            mainPanelImg.MaxAnchor = new Vector2F(0.775F, 0.95F);
            mainPanelImg.MinSize = new Vector3F(400.0F, 400.0F, Single.PositiveInfinity);
            mainPanelImg.MaxSize = new Vector3F(800.0F, 800.0F, Single.PositiveInfinity);

            WObject logo = new WObject("Game Text Logo") { Parent = mainPanel };
            Image logoImage = logo.AddModule<Image>();
            logoImage.Picture = new Texture("assets/textures/logo.png");
            logoImage.MinAnchor = new Vector2F(0.0F, 0.8F);
            logoImage.MaxAnchor = new Vector2F(1.0F, 1.0F);
            logoImage.KeepRatio = true;

            WObject btnPanel = new WObject("Main UI Button Panel") { Parent = mainPanel };
            Image btnPanelImg = btnPanel.AddModule<Image>();
            btnPanelImg.Color = new Color256(1.0, 0.0, 1.0, 0.0);
            btnPanelImg.MinAnchor = new Vector2F(0.075F, 0.0F);
            btnPanelImg.MaxAnchor = new Vector2F(0.925F, 0.6F);

            Texture largeButtonTex = new Texture("assets/textures/button.png");
            Texture shortButtonTex = new Texture("assets/textures/short_button.png");
            WObject single = new WObject("Singleplayer Button") { Parent = btnPanel };
            Button btnSingle = single.AddModule<Button>();
            btnSingle.Background.Picture = largeButtonTex;
            btnSingle.MinAnchor = new Vector2F(0.0F, 0.9F);
            btnSingle.MaxAnchor = new Vector2F(1.0F, 1.0F);
            btnSingle.KeepRatio = true;
            btnSingle.Label.MinAnchor = new Vector2F(0.0F, 0.12F);
            btnSingle.Label.MaxAnchor = new Vector2F(1.0F, 0.88F);
            btnSingle.Label.Text = "Singleplayer";
            btnSingle.Label.Color = new Color256(0.95, 0.95, 0.95, 1.0);
            btnSingle.HoverColor = new Color256(204/255.0D, 252/255.0D, 255/255.0D, 255/255.0D);
            btnSingle.Label.AutoSize = true;
            btnSingle.Label.Aligns = TextAligns.Middle;
            btnSingle.OnClick += () => { Graphics.Window.InvokeUpdate(() => Program.RunGameDebug()); };
            btnSingle.OnHover += () => btnSingle.Label.Color = new Color256(249 / 255.0D, 255 / 255.0D, 191 / 255.0D, 1.0D);
            btnSingle.OnUnhover += () => btnSingle.Label.Color = new Color256(0.95, 0.95, 0.95, 1.0);


            WObject mult = new WObject("Multiplayer Button") { Parent = btnPanel };
            Button btnMult = mult.AddModule<Button>();
            btnMult.Background.Picture = largeButtonTex;
            btnMult.MinAnchor = new Vector2F(0.0F, 0.7F);
            btnMult.MaxAnchor = new Vector2F(1.0F, 0.8F);
            btnMult.KeepRatio = true;
            btnMult.Label.Text = "Multiplayer";
            btnMult.Label.Color = new Color256(0.65, 0.65, 0.65, 1.0);
            btnMult.Label.MinAnchor = new Vector2F(0.0F, 0.12F);
            btnMult.Label.MaxAnchor = new Vector2F(1.0F, 0.88F);
            btnMult.Label.AutoSize = true;
            btnMult.Label.Aligns = TextAligns.Middle;
            btnMult.Locked = true;
            btnMult.HoverColor = new Color256(195 / 255.0D, 226 / 255.0D, 235 / 255.0D, 255 / 255.0D);

            WObject mods = new WObject("Mods Button") { Parent = btnPanel };
            Button btnMods = mods.AddModule<Button>();
            btnMods.Background.Picture = largeButtonTex;
            btnMods.MinAnchor = new Vector2F(0.0F, 0.5F);
            btnMods.MaxAnchor = new Vector2F(1.0F, 0.6F);
            btnMods.KeepRatio = true;
            btnMods.Label.Text = "Mods and Texture Packs";
            btnMods.Label.Color = new Color256(0.65, 0.65, 0.65, 1.0);
            btnMods.Label.MinAnchor = new Vector2F(0.0F, 0.12F);
            btnMods.Label.MaxAnchor = new Vector2F(1.0F, 0.88F);
            btnMods.Label.AutoSize = true;
            btnMods.Locked = true;
            btnMods.Label.Aligns = TextAligns.Middle;
            btnMods.HoverColor = new Color256(195 / 255.0D, 226 / 255.0D, 235 / 255.0D, 255 / 255.0D);

            WObject options = new WObject("Options Button") { Parent = btnPanel };
            Button btnOptions = options.AddModule<Button>();
            btnOptions.Background.Picture = shortButtonTex;
            btnOptions.MinAnchor = new Vector2F(0.0F, 0.2F);
            btnOptions.MaxAnchor = new Vector2F(0.45F, 0.3F);
            btnOptions.KeepRatio = true;
            btnOptions.Label.Text = "Options...";
            btnOptions.Label.Color = new Color256(0.65, 0.65, 0.65, 1.0);
            btnOptions.Label.MinAnchor = new Vector2F(0.0F, 0.12F);
            btnOptions.Label.MaxAnchor = new Vector2F(1.0F, 0.88F);
            btnOptions.Label.AutoSize = true;
            btnOptions.Locked = true;
            btnOptions.Label.Aligns = TextAligns.Middle;
            btnOptions.HoverColor = new Color256(195 / 255.0D, 226 / 255.0D, 235 / 255.0D, 255 / 255.0D);

            WObject quit = new WObject("Quit Button") { Parent = btnPanel };
            Button btnQuit = quit.AddModule<Button>();
            btnQuit.Background.Picture = shortButtonTex;
            btnQuit.MinAnchor = new Vector2F(0.55F, 0.2F);
            btnQuit.MaxAnchor = new Vector2F(1.0F, 0.3F);
            btnQuit.KeepRatio = true;
            btnQuit.Label.Text = "Quit Game";
            btnQuit.Label.Color = new Color256(0.95, 0.95, 0.95, 1.0);
            btnQuit.Label.MinAnchor = new Vector2F(0.0F, 0.12F);
            btnQuit.Label.MaxAnchor = new Vector2F(1.0F, 0.88F);
            btnQuit.Label.AutoSize = true;
            btnQuit.OnClick += () => WEngine.Stop();
            btnQuit.Label.Aligns = TextAligns.Middle;
            btnQuit.HoverColor = new Color256(204 / 255.0D, 252 / 255.0D, 255 / 255.0D, 255 / 255.0D);

            btnQuit.OnHover += () => btnQuit.Label.Color = new Color256(249 / 255.0D, 255 / 255.0D, 191 / 255.0D, 1.0D);
            btnQuit.OnUnhover += () => btnQuit.Label.Color = new Color256(0.95, 0.95, 0.95, 1.0);


            /*
            WObject mult = new WObject("Multiplayer Button") { Parent = btnPanel };
            Button btnMult = mult.AddModule<Button>();
            btnMult.Background.Picture = largeButtonTex;
            btnMult.MinAnchor = new Vector2F(0.0F, 0.5F);
            btnMult.MaxAnchor = new Vector2F(1.0F, 1.0F);
            btnMult.KeepRatio = true;
            btnMult.HoverColor = new Color256(1.0, 1.0, 1.25, 1.0);
            btnMult.Label.Text = "           Multiplayer";
            btnMult.Label.Color = new Color256(0.5, 0.5, 0.5, 1.0);
            btnMult.Label.FontSize = 40.0F;
            //btnMult.OnHover += () => btnMult.Label.Color = new Color256(1.0, 0.98, 0.6, 1.0);
            //btnMult.OnUnhover += () => btnMult.Label.Color = new Color256(0.95, 0.95, 0.95, 1.0);

            btnMult.Locked = true;*/



            /*WObject mods = new WObject("Mods Button") { Parent = btnPanel };
            Button btnMods = mods.AddModule<Button>();
            btnMods.Background.Picture = largeButtonTex;
            btnMods.MinAnchor = new Vector2F(0.0F, 0.35F);
            btnMods.MaxAnchor = new Vector2F(1.0F, 0.425F);
            //btnMods.KeepRatio = true;
            btnMods.HoverColor = new Color256(1.0, 1.0, 1.25, 1.0);
            btnMods.Label.Text = "           Mods";
            btnMods.Label.Color = new Color256(0.5, 0.5, 0.5, 1.0);
            btnMods.Label.FontSize = 40.0F;
            //btnMult.OnHover += () => btnMult.Label.Color = new Color256(1.0, 0.98, 0.6, 1.0);
            //btnMult.OnUnhover += () => btnMult.Label.Color = new Color256(0.95, 0.95, 0.95, 1.0);

            btnMods.Locked = true;*/





            /* WObject options = new WObject("Mods Button") { Parent = btnPanel };
             Button btnOptions = options.AddModule<Button>();
             btnOptions.Background.Picture = largeButtonTex;
             btnOptions.MinAnchor = new Vector2F(0.0F, 0.2F);
             btnOptions.MaxAnchor = new Vector2F(1.0F, 0.275F);
             //btnOptions.KeepRatio = true;
             btnOptions.HoverColor = new Color256(1.0, 1.0, 1.25, 1.0);
             btnOptions.Label.Text = "           Options";
             btnOptions.Label.Color = new Color256(0.5, 0.5, 0.5, 1.0);
             btnOptions.Label.FontSize = 40.0F;
             //btnMult.OnHover += () => btnMult.Label.Color = new Color256(1.0, 0.98, 0.6, 1.0);
             //btnMult.OnUnhover += () => btnMult.Label.Color = new Color256(0.95, 0.95, 0.95, 1.0);

             btnOptions.Locked = true;*/

        }

        private static void Btn_OnClick()
        {
            throw new NotImplementedException();
        }

        public static void Show()
        {
            if (!MenuWobject) CreateMenu();


            MenuWobject.Enabled = true;
        }

        public static void Hide()
        {
            MenuWobject.Enabled = false;
        }
    }
}
