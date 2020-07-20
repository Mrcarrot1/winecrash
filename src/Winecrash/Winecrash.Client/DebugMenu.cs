﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winecrash.Engine;
using Winecrash.Engine.GUI;

namespace Winecrash.Client
{
    public sealed class DebugMenu : Module
    {
        public static DebugMenu Instance { get; private set; }

        private Label lbFPS;
        protected override void Creation()
        {
            if (Instance)
            {
                Delete();
                return;
            }

            Instance = this;

            this.WObject.Parent = Canvas.Main.WObject;
            lbFPS = this.WObject.AddModule<Label>();
            lbFPS.FontSize = 40.0F;
            lbFPS.MinAnchor = new Vector2F(0.0F, 0.0F);
            lbFPS.MaxAnchor = new Vector2F(1.0F, 1.0F);
            lbFPS.Left = 15;
        }

        int fpses;
        int frames;

        double timesincefpsupdate = 0.0F;
        double rate = 0.5F;

        string fpstext = "0 FPS";
        protected override void Update()
        {
            fpses += (int)(1D / Time.DeltaTime);
            timesincefpsupdate += Time.DeltaTime;
            frames++;


            if (timesincefpsupdate > rate)
            {
                timesincefpsupdate = 0.0D;

                int fps = (int)(fpses / frames);

                fpses = 0;
                frames = 0;

                fpstext = fps.ToString("D3") + " FPS        Winecrash Predev 0.2";
            }

            string txt = fpstext;

            Vector3F pos = Player.Instance.WObject.Position;
            World.GlobalToLocal(pos, out Vector3I cpos, out Vector3I lpos);

            txt += "\n";
            txt += "XYZ: " + pos.X.ToString("0.000") + " / " + pos.Y.ToString("0.000") + " / " + pos.Z.ToString("0.000") + "\n";
            if (Player.Instance.ViewRayHit != null)
            {
                txt += "Pointed Block: " + Player.Instance.ViewRayHit.Value.Block.Identifier + "\n";
                txt += "Block Location XYZ: " + Player.Instance.ViewRayHit.Value.GlobalPosition.X.ToString("0") + " / " + Player.Instance.ViewRayHit.Value.GlobalPosition.Y.ToString("0") + " / " + Player.Instance.ViewRayHit.Value.GlobalPosition.Z.ToString("0") + "\n";
            }

            lbFPS.Text = txt;

            if (Input.IsPressing(GameInput.Key("Debug")))
            {
                lbFPS.Enabled =! lbFPS.Enabled;
            }
        }
    }
}
