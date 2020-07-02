﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using OpenTK.Graphics.OpenGL4;

namespace Winecrash.Engine
{
    public sealed class Texture : BaseObject
    {
        internal int Handle { get; private set; }

        internal byte[] Data { get; set; }

        public void SetPixel(int x, int y, Color32 color)
        {
            int i = x + Size.X * y;
            this.Data[i] = color.R;
            this.Data[i + 1] = color.G;
            this.Data[i + 2] = color.B;
            this.Data[i + 3] = color.A;
        }

        public Color32 GetPixel(int x, int y)
        {
            Color32 col = new Color32();
            int i = x + Size.X * y;
            col.R = this.Data[i];
            col.G = this.Data[i + 1];
            col.B = this.Data[i + 2];
            col.A = this.Data[i + 3];
            return col;
        }

        public void SetPixels(int x, int y, int width, int height, Color32[] colors)
        {
            if (colors == null) return;

            int iColor = 0;
            for (int texy = 0; texy < height; texy++)
            {
                for (int texx = 0; texx < width; texx++)
                {
                    int i = (x + texx) + Size.X * (y + texy);

                    this.Data[i] = colors[iColor].R;
                    this.Data[i + 1] = colors[iColor].G;
                    this.Data[i + 2] = colors[iColor].B;
                    this.Data[i + 3] = colors[iColor].A;

                    iColor++;
                }
            }
        }

        public Color32[] GetPixels(int x, int y, int width, int height)
        {
            Color32[] colors = new Color32[width * height];

            int iColor = 0;
            for (int texy = 0; texy < height; texy++)
            {
                for (int texx = 0; texx < width; texx++)
                {
                    int i = (x + texx) + Size.X * (y + texy);

                    colors[iColor] = new Color32(this.Data[i], this.Data[i + 1], this.Data[i + 2], this.Data[i + 3]);

                    iColor++;
                }
            }

            return colors;
        }

        public void Apply()
        {
            Viewport.DoOnce += () =>
            {
                this.Use();

                GL.TexImage2D(TextureTarget.Texture2D,
                            0,
                            PixelInternalFormat.Rgba,
                            this.Width,
                            this.Height,
                            0,
                            OpenTK.Graphics.OpenGL4.PixelFormat.Rgba,
                            PixelType.UnsignedByte,
                            this.Data);
            };
        }



        public Vector2I Size { get; private set; }
        public int Width
        {
            get
            {
                return this.Size.X;
            }
        }
        public int Height
        {
            get
            {
                return this.Size.Y;
            }
        }

        internal static List<Texture> Cache { get; set; } = new List<Texture>();

        public static Texture Find(string name)
        {
            return Cache.FirstOrDefault(t => t.Name == name);
        }

        public static Texture Blank { get; set; }
        /// <summary>
        /// DO NOT USE ! USED BY ACTIVATOR.
        /// </summary>
        [Obsolete("Use Texture(string, string) instead.\nThis .ctor is meant to be used by Material's Activator")]
        public Texture()
        {
            if(Blank != null)
            {
                this.Delete();
                return;
            }

            Blank = this;

            this.Name = "Blank";
            this.Size = new Vector2I(1, 1);
            byte[] blank = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                blank[i] = 255;
            }

            this.Data = blank;

            this.Handle = GL.GenTexture();

            this.Use();

            GL.TexImage2D(TextureTarget.Texture2D,
                        0,
                        PixelInternalFormat.Rgba,
                        this.Width,
                        this.Height,
                        0,
                        OpenTK.Graphics.OpenGL4.PixelFormat.Bgra,
                        PixelType.UnsignedByte,
                        blank);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            Blank = this;
            Cache.Add(this);
        }

        public Texture(int width, int height)
        {
            this.Size = new Vector2I(width, height);
        }

        public static Texture GetOrCreate(string path)
        {
            string name = path.Split('/', '\\').Last().Split('.')[0];
            Texture tex = Texture.Find(name);
            if(tex == null)
            {
                tex = new Texture(path, name);
            }

            return tex;
        }

        public Texture(string path, string name = null) : base(name)
        {
            if(name == null)
                this.Name = path.Split('/', '\\').Last().Split('.')[0];

            this.Handle = GL.GenTexture();

            this.Use();

            try
            {
                using (Bitmap img = new Bitmap(path))
                {
                    var data = img.LockBits(
                        new Rectangle(0, 0, img.Width, img.Height), 
                        ImageLockMode.ReadOnly, 
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    GL.TexImage2D(TextureTarget.Texture2D,
                        0,
                        PixelInternalFormat.Rgba,
                        img.Width,
                        img.Height,
                        0,
                        OpenTK.Graphics.OpenGL4.PixelFormat.Bgra,
                        PixelType.UnsignedByte,
                        data.Scan0);

                    this.Size = new Vector2I(img.Width, img.Height);
                }

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                Cache.Add(this);
            }
            catch(Exception e)
            {
                Debug.LogError("Error when loading texture at " + path + " : " + e.Message + "\n" + "Source: " + e.Source + "\n"  + e.StackTrace);
                this.Delete();
            }
        }

        internal void Use(TextureUnit unit = TextureUnit.Texture0)
        {
            if(this.Deleted) return;

            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }

        public override void Delete()
        {
            Cache.Remove(this);

            GL.DeleteTexture(Handle);
            this.Handle = -1;

            base.Delete();
        }
    }
}
