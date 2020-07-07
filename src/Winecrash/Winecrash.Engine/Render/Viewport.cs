﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Graphics;
using OpenTK.Input;
using System.Diagnostics;
using System.Drawing;

namespace Winecrash.Engine
{
    public class Viewport : GameWindow
    {
        public static Viewport Instance { get; private set; }

        internal static Thread ThreadRunner { get; set; }

        public static event UpdateEventHandler Update;
        public static event UpdateEventHandler Render;
        public static event ViewportDoOnceDelegate DoOnce;
        public static event ViewportDoOnceDelegate DoOnceRender;

        public static event ViewportLoadDelegate OnLoaded;

        public delegate void ViewportLoadDelegate();
        public delegate void ViewportDoOnceDelegate();

        MouseState _PreviousState = new MouseState();

        /// <summary>
        /// https://github.com/opentk/LearnOpenTK/blob/master/Chapter1/4-Textures/Window.cs
        /// </summary>
        public Viewport(int width, int height, string title, Icon icon = null) : base(width, height, GraphicsMode.Default, title) 
        {
            if(icon != null)
            {
                this.Icon = icon;
            }
            Instance = this;
        }

        protected override void OnLoad(EventArgs e)
        {
            new Texture();
            WObject camWobj = new WObject("Main Camera");
            camWobj.AddModule<Camera>();

            Shader.CreateError();

            GL.FrontFace(FrontFaceDirection.Cw);
            GL.CullFace(CullFaceMode.Front);
            GL.Enable(EnableCap.CullFace);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            new Shader("assets/shaders/Standard/Standard.vert", "assets/shaders/Standard/Standard.frag");
            new Shader("assets/shaders/Unlit/Unlit.vert", "assets/shaders/Unlit/Unlit.frag");

            OnLoaded?.Invoke();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            DoOnceRender?.Invoke();
            DoOnceRender = null;
            Render?.Invoke(new UpdateEventArgs(e.Time));
            
            SwapBuffers();

            base.OnRenderFrame(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            //GL.DeleteBuffer(_VertexBufferObject);
            //GL.DeleteVertexArray(_VertexArrayObject);

            MeshRenderer[] renderers = MeshRenderer.MeshRenderers.ToArray();
            if (renderers != null)
                for (int i = 0; i < renderers.Length; i++)
                    if (renderers[i] != null)
                        renderers[i].Delete();

            
            Shader[] shaders = Shader.Cache.ToArray();
            if(shaders != null)
                for (int i = 0; i < shaders.Length; i++)
                    if (shaders[i] != null)
                        shaders[i].Delete();
            
            Texture[] textures = Texture.Cache.ToArray();

            if (textures != null)
                for (int i = 0; i < textures.Length; i++)
                    if (textures[i] != null)
                        textures[i].Delete();

            base.OnUnload(e);
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            WEngine.Stop(this);
        }
        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);

            base.OnResize(e);
        }
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
        }

        static Stopwatch updatesw = new Stopwatch();
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            DoOnce?.Invoke();
            DoOnce = null;
            //Debug.Log((1D/e.Time).ToString("C0").Split('¤')[1] + " FPS");
            Time.DeltaTime = e.Time;
            MouseState ms = Mouse.GetState();
            Vector2D delta = new Vector2D(this._PreviousState.X - ms.X, this._PreviousState.Y - ms.Y);
            //Input.MouseDelta = Focused ? delta : Vector2D.Zero;
            this._PreviousState = ms;
            /*if (Input.LockMode == CursorLockModes.Lock && Focused)
            {
                Mouse.SetPosition(X + Width / 2f, Y + Height / 2f);
            }*/

            
            Update?.Invoke(new UpdateEventArgs(e.Time));
            
            GC.Collect();
           
            base.OnUpdateFrame(e);
        }
    }
}
