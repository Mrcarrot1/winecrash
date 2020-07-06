﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winecrash.Engine;
using System.Threading;
using Newtonsoft.Json;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Winecrash.Client
{
    public class ChunkEventArgs : EventArgs
    {
        public Chunk Chunk { get; } = null;

        public ChunkEventArgs(Chunk chunk) : base()
        {
            this.Chunk = chunk;
        }
    }

    public delegate void ChunkLoadDelegate(ChunkEventArgs args);

    public class Chunk : Module
    {

#region Constants
        public const int Width = 16;
        public const int Height = 256;
        public const int Depth = 16;

        public const int TotalBlocks = Width * Height * Depth;
#endregion

#region Blocks Getters/Setters
        public Block this[int x, int y, int z]
        {
            get
            {
                return this.GetBlock(x,y,z);
            }

            set
            {
                this.SetBlock(x,y,z, value);
            }
        }

        public void SetBlock(int x, int y, int z, Block b)
        {
            this._Blocks[x + Width * y + Width * Height * z] = ItemCache.GetIndex(b);
        }
        public void SetBlock(int x, int y, int z, string identifier)
        {
            this._Blocks[x + Width * y + Width * Height * z] = ItemCache.GetIndex(identifier);
        }
        public void SetBlock(int x, int y, int z, ushort cacheIndex)
        {
            this._Blocks[x + Width * y + Width * Height * z] = cacheIndex;
        }

        public string GetBlockIdentifier(int x, int y, int z)
        {
            return ItemCache.GetIdentifier(this._Blocks[x + Width * y + Width * Height * z]);
        }
        public ushort GetBlockIndex(int x, int y, int z)
        {
            return this._Blocks[x + Width * y + Width * Height * z];
        }

        public Block GetBlock(int x, int y, int z)
        {
            return ItemCache.Get<Block>(this._Blocks[x + Width * y + Width * Height * z]);
        }



        #endregion


        public const int UintSize = 32;
        public const int LightDataSize = 4;
        public const int LightPackSize = UintSize / LightDataSize;
        public const uint MaxLight = 0XF;
        private uint[] _Light = new uint[8192];

        public void SetLightLevel(int x, int y, int z, uint level)
        {
            level = WMath.Clamp(level, 0u, 15u);

            int baseFullIndex = x + Chunk.Width * y + Chunk.Width * Chunk.Height * z;
            
            int basePackedIndex = baseFullIndex / LightPackSize;
            int shiftPackedIndex = baseFullIndex % LightPackSize;

            //Debug.Log("full: " + baseFullIndex + " | packed: " + basePackedIndex + " | shift: " + shiftPackedIndex + " | x;y;z: " + x + ";" + y + ";" + z + "");

            _Light[basePackedIndex] |= level << (LightDataSize * shiftPackedIndex);

        }

        public uint GetLightLevel(int x, int y, int z)
        {
            int baseFullIndex = x + Chunk.Width * y + Chunk.Width * Chunk.Height * z;

            int basePackedIndex = baseFullIndex / LightPackSize;
            int shiftPackedIndex = baseFullIndex % LightPackSize;

            uint mask = 0x0;

            switch (shiftPackedIndex)
            {
                case 0:
                    mask = 0xF;
                    break;
                case 1:
                    mask = 0xF0;
                    break;
                case 2:
                    mask = 0xF00;
                    break;
                case 3:
                    mask = 0xF000;
                    break;
                case 4:
                    mask = 0xF0000;
                    break;
                case 5:
                    mask = 0xF00000;
                    break;
                case 6:
                    mask = 0xF000000;
                    break;
                case 7:
                    mask = 0xF0000000;
                    break;
            }

            return (_Light[basePackedIndex] & mask) >> (LightDataSize * shiftPackedIndex);
        }

        /// <summary>
        /// The blocks references of this chunk.
        /// </summary>
        private ushort[] _Blocks = new ushort[Width * Height * Depth];
        /// <summary>
        /// The ticket responsible of this chunk.
        /// </summary>
        public Ticket Ticket { get; internal set; } = null;
        /// <summary>
        /// If the chunk has at least been built once.
        /// </summary>
        public bool BuiltOnce { get; private set; } = false;
        /// <summary>
        /// The position of the chunk in chunk scale.
        /// X and Y are North/East and Z is the dimension.
        /// </summary>
        public Vector3I Position { get; private set; }

        public static Texture ChunkTexture { get; set; }

        public static int TexWidth;
        public static int TexHeight;

#region Neighbors
        /// <summary>
        /// Northern neighbor chunk
        /// </summary>
        public Chunk NorthNeighbor { get; internal set; } = null;
        /// <summary>
        /// Southern neighbor chunk
        /// </summary>
        public Chunk SouthNeighbor { get; internal set; } = null;
        /// <summary>
        /// Eastern neighbor chunk
        /// </summary>
        public Chunk EastNeighbor { get; internal set; } = null;
        /// <summary>
        /// Western neighbor chunk
        /// </summary>
        public Chunk WestNeighbor { get; internal set; } = null;
#endregion

        /// <summary>
        /// All the chunks loaded into the game.
        /// </summary>
        public static List<Chunk> Chunks { get; private set; } = new List<Chunk>(1000);

#region Loading Properties
        /// <summary>
        /// The maximum chunk loading rate, in number of chunks at once.
        /// </summary>
        public static int LoadRate { get; set; } = 1000;
        /// <summary>
        /// The amount of chunks loading right now.
        /// </summary>
        public static int Loading { get; private set; } = 0;
        /// <summary>
        /// The amount of chunks waiting to be loaded.
        /// </summary>
        public static int LoadWait { get; private set; } = 0;
#endregion

        /// <summary>
        /// The renderer of this chunk.
        /// </summary>
        public MeshRenderer Renderer { get; private set; } = null;
        public bool Constructing { get; private set; } = false;


#region Events
        /// <summary>
        /// The event triggered when any chunk has been built for the first time.
        /// </summary>
        public static event ChunkLoadDelegate AnyChunkFirstBuilt;
        /// <summary>
        /// The event triggered when any chunk is created.
        /// </summary>
        public static event ChunkLoadDelegate AnyChunkCreated;
        /// <summary>
        /// The event triggered when any chunk is deleted.
        /// </summary>
        public static event ChunkLoadDelegate AnyChunkDeleted;
        #endregion

        #region Subscribers
        private void OnAnyChunkCreated(ChunkEventArgs args)
        {
            Chunk other = args.Chunk;
            if (other == this) return;

            if (!NorthNeighbor && other.Position == this.Position + Vector3I.Up)
            {
                NorthNeighbor = other;
            }
            else if (!SouthNeighbor && other.Position == this.Position + Vector3I.Down)
            {
                SouthNeighbor = other;
            }
            else if (!WestNeighbor && other.Position == this.Position + Vector3I.Left)
            {
                WestNeighbor = other;
            }
            else if (!EastNeighbor && other.Position == this.Position + Vector3I.Right)
            {
                EastNeighbor = other;
            }
        }
        private void OnAnyChunkDeleted(ChunkEventArgs args)
        {
            Chunk other = args.Chunk;
            if (other == this) return;

            if (NorthNeighbor && other == NorthNeighbor) NorthNeighbor = null;
            else if (SouthNeighbor && other == SouthNeighbor) SouthNeighbor = null;
            else if (WestNeighbor && other == WestNeighbor) WestNeighbor = null;
            else if (EastNeighbor && other == EastNeighbor) EastNeighbor = null;
        }
        #endregion

        public void Tick()
        {
            for (int i = 0; i < Chunk.TotalBlocks; i += Chunk.TotalBlocks / 16)
            {
                for (int j = 0; j < GameManager.RandomTickSpeed; j++)
                {
                    //_blocks[World.WorldRandom.Next(i, ChunkTotalBlocks)].Tick();
                }
            }
        }

        public string ToJSON()
        {
            Dictionary<string, int> distinctIDs = new Dictionary<string, int>(64);

            int[] blocksRef = new int[Chunk.TotalBlocks];

            int chunkIndex = 0;
            int paletteIndex = 0;

            for (int z = 0; z < Chunk.Depth; z++)
            {
                for (int y = 0; y < Chunk.Height; y++)
                {
                    for (int x = 0; x < Chunk.Width; x++)
                    {
                        string id = this[x, y, z].Identifier;

                        if (!distinctIDs.ContainsKey(id))
                        {
                            distinctIDs.Add(id, paletteIndex++);
                        }

                        blocksRef[chunkIndex++] = distinctIDs[id];
                    }
                }
            }

            return JsonConvert.SerializeObject(new JSONChunk()
            {
                Palette = distinctIDs.Keys.ToArray(),
                Data = blocksRef
            }, Formatting.None);
        }

#region Game Logic

        int lightSSBO = -1;

        protected override void Creation()
        {
            AnyChunkCreated += OnAnyChunkCreated;
            AnyChunkDeleted += OnAnyChunkDeleted;

            MeshRenderer mr = this.Renderer = this.WObject.AddModule<MeshRenderer>();

            mr.Material = new Material(Shader.Find("Chunk"));
            mr.Material.SetData<Texture>("albedo", Chunk.ChunkTexture);
            mr.Material.SetData<Vector4>("color", new Color256(1, 1, 1, 1));

            mr.Material.SetData<float>("minLight", 0.2F);
            mr.Material.SetData<float>("maxLight", 0.8F);

            Viewport.DoOnce += () =>
            {
                lightSSBO = GL.GenBuffer();
            };

            this.Renderer.OnRender += () =>
            {
                if (lightSSBO == -1) return;

                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, lightSSBO);

                int idx = GL.GetProgramResourceIndex(Renderer.Material.Shader.Handle, ProgramInterface.ShaderStorageBlock, "chunk_lights");

                if (idx != -1)
                {
                    GL.ShaderStorageBlockBinding(Renderer.Material.Shader.Handle, idx, 10);
                    GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 10, lightSSBO);
                }

                if(NorthNeighbor && NorthNeighbor.lightSSBO != -1)
                {
                    GL.BindBuffer(BufferTarget.ShaderStorageBuffer, NorthNeighbor.lightSSBO);

                    idx = GL.GetProgramResourceIndex(Renderer.Material.Shader.Handle, ProgramInterface.ShaderStorageBlock, "north_chunk_lights");

                    if (idx != -1)
                    {
                        GL.ShaderStorageBlockBinding(Renderer.Material.Shader.Handle, idx, 11);
                        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 11, NorthNeighbor.lightSSBO);
                    }
                }

                if(SouthNeighbor && SouthNeighbor.lightSSBO != -1)
                {
                    GL.BindBuffer(BufferTarget.ShaderStorageBuffer, SouthNeighbor.lightSSBO);

                    idx = GL.GetProgramResourceIndex(Renderer.Material.Shader.Handle, ProgramInterface.ShaderStorageBlock, "south_chunk_lights");

                    if (idx != -1)
                    {
                        GL.ShaderStorageBlockBinding(Renderer.Material.Shader.Handle, idx, 12);
                        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 12, SouthNeighbor.lightSSBO);
                    }
                }

                if(EastNeighbor && EastNeighbor.lightSSBO != -1)
                {
                    GL.BindBuffer(BufferTarget.ShaderStorageBuffer, EastNeighbor.lightSSBO);

                    idx = GL.GetProgramResourceIndex(Renderer.Material.Shader.Handle, ProgramInterface.ShaderStorageBlock, "east_chunk_lights");

                    if (idx != -1)
                    {
                        GL.ShaderStorageBlockBinding(Renderer.Material.Shader.Handle, idx, 13);
                        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 13, EastNeighbor.lightSSBO);
                    }
                }

                if(WestNeighbor && WestNeighbor.lightSSBO != -1)
                {
                    GL.BindBuffer(BufferTarget.ShaderStorageBuffer, WestNeighbor.lightSSBO);

                    idx = GL.GetProgramResourceIndex(Renderer.Material.Shader.Handle, ProgramInterface.ShaderStorageBlock, "west_chunk_lights");

                    if (idx != -1)
                    {
                        GL.ShaderStorageBlockBinding(Renderer.Material.Shader.Handle, idx, 14);
                        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 14, WestNeighbor.lightSSBO);
                    }
                }
            };

            Chunks.Add(this);
        }
        protected override void Start()
        {

            while(this.Ticket == null)  // Make sure the ticket is actually assigned.
                Thread.Sleep(1);        // I should be, but sometime the assignation
                                        // is not done at the right time due to the
                                        // multithreaded nature of the engine.

            this.Position = new Vector3I(this.Ticket.Position.X, this.Ticket.Position.Y, 0);
            this.WObject.Position = new Vector3F(this.Position.X * Width, 0, this.Position.Y * Depth);

            this._Blocks = Generator.GetChunk(this.Position.X, this.Position.Y, out _);

            AnyChunkCreated?.Invoke(new ChunkEventArgs(this));

            while (Loading >= LoadRate)  // Wait for other chunks to build.
                Thread.Sleep(1);

            Loading++;

            NorthNeighbor = Ticket.GetTicket(this.Position.XY + Vector2I.Up)?.Chunk;
            SouthNeighbor = Ticket.GetTicket(this.Position.XY + Vector2I.Down)?.Chunk;
            EastNeighbor = Ticket.GetTicket(this.Position.XY + Vector2I.Right)?.Chunk;
            WestNeighbor = Ticket.GetTicket(this.Position.XY + Vector2I.Left)?.Chunk;


            this.Construct();
            this.BuiltOnce = true;

            Loading--;
            
            AnyChunkFirstBuilt?.Invoke(new ChunkEventArgs(this));

            
            this.RunAsync = false;      // Be sure to undo the asynchronous run mode.
        }
        protected override void OnDelete()
        {
            Chunks.Remove(this);

            this.Ticket = null;
            this._Blocks = null;

            AnyChunkDeleted?.Invoke(new ChunkEventArgs(this));

            GL.DeleteBuffer(lightSSBO);

            base.OnDelete();
        }
#endregion


#region Generation
        public void DiffuseLight(int basex, int basey, int basez, uint baseLevel,  BlockFaces comingFrom = BlockFaces.Up)
        {
            if (baseLevel == 0) return;

            int x = 0, y = 0, z = 0;
            uint level = baseLevel - 1;
            BlockFaces to;
            BlockFaces from = BlockFaces.Up;

            Chunk c;

            for (int i = 0; i < 6; i++)
            {
                to = (BlockFaces)i;

                if (to == comingFrom) continue;

                switch(to)
                {
                    case BlockFaces.Up:
                        {
                            x = basex;
                            y = basey + 1;
                            z = basez;

                            from = BlockFaces.Down;
                        }
                        break;

                    case BlockFaces.Down:
                        {
                            x = basex;
                            y = basey - 1;
                            z = basez;

                            from = BlockFaces.Up;
                        }
                        break;

                    case BlockFaces.West:
                        {
                            x = basex - 1;
                            y = basey;
                            z = basez;

                            from = BlockFaces.East;
                        }
                        break;

                    case BlockFaces.East:
                        {
                            x = basex + 1;
                            y = basey;
                            z = basez;

                            from = BlockFaces.West;
                        }
                        break;

                    case BlockFaces.North:
                        {
                            x = basex;
                            y = basey;
                            z = basez + 1;

                            from = BlockFaces.South;
                        }
                        break;

                    case BlockFaces.South:
                        {
                            x = basex;
                            y = basey;
                            z = basez - 1;

                            from = BlockFaces.North;
                        }
                        break;
                }

                if (x < 0 || x > 15 || y < 0 || y > 255 || z < 0 || z > 15) return;

                /*if (y < 0 || y > Chunk.Height - 1) return;


                if (x < 0 && WestNeighbor != null)
                {
                    c = WestNeighbor;
                    x = 15;
                }

                else if (x > Chunk.Width - 1 && EastNeighbor != null)
                {
                    c = EastNeighbor;
                    x = 0;
                }

                else if (z < 0 && SouthNeighbor != null)
                {
                    c = SouthNeighbor;
                    z = 15;
                }

                else if (z > Chunk.Depth - 1 && NorthNeighbor != null)
                {
                    c = NorthNeighbor;
                    z = 0;
                }

                else
                {*/
                    //c = this;
                //}


                uint lvl = GetLightLevel(x, y, z);
                if (!this[x,y,z].Transparent || lvl > level - 1) continue;

                SetLightLevel(x, y, z, level);

                DiffuseLight(x, y, z, level, from);
            }
        }
        public void GenerateLights()
        {
            this._Light = new uint[8192];
            Block b;
            for (int z = 0; z < Chunk.Depth; z++)
            {
                for (int x = 0; x < Chunk.Width; x++)
                {
                    int y = Chunk.Height - 1;

                    b = this[x, y, z];

                    //while sky, light coming from sky, do not spread
                    while (y > -1 && b.Transparent)
                    {
                        SetLightLevel(x, y, z, 15);
                        y--;
                        b = this[x, y, z];
                    }

                    //then diffuse around..
                    DiffuseLight(x, y, z, 15, BlockFaces.Up);
                }
            }

            Viewport.DoOnceRender += () =>
            {
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, lightSSBO);
                GL.BufferData(BufferTarget.ShaderStorageBuffer, sizeof(uint) * 8192, this._Light, BufferUsageHint.DynamicCopy);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
            };
            //this.Renderer.Material.SetData<byte[]>("light", this._Light);
        }

        public void StopCurrentConstruction()
        {
            if(Constructing)
                StopCurrentConstruct = true;
        }
        private bool StopCurrentConstruct = false;

        public void Edit(int x, int y, int z, Block b = null)
        {
            PrivateEdit(x,y,z,b);
        }
        private void PrivateEdit(int x, int y, int z, Block b = null)
        {
            if (b == null) b = ItemCache.Get<Block>("winecrash:air");

            this[x, y, z] = b;

            if (x == 0)
            {
                this.WestNeighbor?.Construct();
            }
            else if (x == 15)
            {
                this.EastNeighbor?.Construct();
            }

            if (z == 0)
            {
                this.SouthNeighbor?.Construct();
            }
            else if (z == 15)
            {
                this.NorthNeighbor?.Construct();
            }

            this.Construct();
        }
        public void Construct()
        {
            if (Constructing) return;

            GenerateLights();

            Constructing = true;

            List<Vector3F> vertices = new List<Vector3F>();
            List<Vector2F> uv = new List<Vector2F>();
            List<Vector3F> normals = new List<Vector3F>();
            List<uint> triangles = new List<uint>();

            //No tangents yet
            //Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
            //Vector4[] tan = new Vector4[6] { tangent, tangent, tangent, tangent, tangent, tangent };

            Stack<BlockFaces> faces = new Stack<BlockFaces>(6);
            uint triangle = 0;
            Item item = null;
            ushort index = 0;

            for (int z = 0; z < Depth; z++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        if (StopCurrentConstruct)
                        {
                            StopCurrentConstruct = false;
                            return;
                        }

                        index = GetBlockIndex(x,y,z);

                        item = ItemCache.Get<Item>(index);

                        if (item.Identifier == "winecrash:air") continue; // ignore if air

                        if (IsTranparent(x, y + 1, z)) faces.Push(BlockFaces.Up);   // up
                        if (IsTranparent(x, y - 1, z)) faces.Push(BlockFaces.Down); // down
                        if (IsTranparent(x - 1, y, z)) faces.Push(BlockFaces.West); // west
                        if (IsTranparent(x + 1, y, z)) faces.Push(BlockFaces.East); // east
                        if (IsTranparent(x, y, z + 1)) faces.Push(BlockFaces.North);// north
                        if (IsTranparent(x, y, z - 1)) faces.Push(BlockFaces.South);// south

                        foreach (BlockFaces face in faces)
                        {
                            CreateVerticesCube(x, y, z, face, vertices);

                            CreateUVsCube(face, uv, index);
                            //uv.AddRange(new Vector2F[6]); // no uv yet.

                            CreateNormalsCube(face, normals);

                            triangles.AddRange(new uint[6] { triangle, triangle + 1, triangle + 2, triangle + 3, triangle + 4, triangle + 5 });
                            triangle += 6;
                        }

                        faces.Clear();
                    }
                }
            }

            if(vertices.Count != 0)
            {
                if(Renderer.Mesh == null)
                {
                    Mesh m = new Mesh("Chunk Mesh");

                    m.Vertices = vertices.ToArray();
                    m.Triangles = triangles.ToArray();
                    m.UVs = uv.ToArray();
                    m.Normals = normals.ToArray();
                    m.Tangents = new Vector4F[vertices.Count];

                    m.Apply(true);

                    Renderer.Mesh = m;
                }

                else
                {
                    Mesh m = Renderer.Mesh;

                    m.Vertices = vertices.ToArray();
                    m.Triangles = triangles.ToArray();
                    m.UVs = uv.ToArray();
                    m.Normals = normals.ToArray();
                    m.Tangents = new Vector4F[vertices.Count];

                    m.Apply(true);
                }

                

                vertices = null;
                triangles = null;
                uv = null;
                normals = null;
            }

            cwest = ceast = cnorth = csouth = null;

            Constructing = false;
        }


        private static Vector3F up = Vector3F.Up;
        private static Vector3F down = Vector3F.Down;
        private static Vector3F left = Vector3F.Left;
        private static Vector3F right = Vector3F.Right;
        private static Vector3F forward = Vector3F.Forward;
        private static Vector3F south = Vector3F.Backward;


        private static void CreateUVsCube(BlockFaces face, List<Vector2F> uvs, int cubeIdx)
        {
            float faceIDX = 0;

            switch(face)
            {
                case BlockFaces.East:
                    faceIDX = 0;
                    break;

                case BlockFaces.West:
                    faceIDX = 1;
                    break;

                case BlockFaces.Up:
                    faceIDX = 2;
                    break;

                case BlockFaces.Down:
                    faceIDX = 3;
                    break;

                case BlockFaces.North:
                    faceIDX = 4;
                    break;

                case BlockFaces.South:
                    faceIDX = 5;
                    break;
            }

            float idx = (float)cubeIdx;

            switch(face)
            {
                case BlockFaces.Up:
                    {
                        uvs.AddRange(new[]
                        {
                            new Vector2F(faceIDX * ItemCache.TextureSize / TexWidth, idx * ItemCache.TextureSize / TexHeight), //0
                            new Vector2F(faceIDX * ItemCache.TextureSize / TexWidth, (idx + 1) * ItemCache.TextureSize / TexHeight), //1
                            new Vector2F((faceIDX + 1) * ItemCache.TextureSize / TexWidth, idx * ItemCache.TextureSize / TexHeight), //2
                            
                            new Vector2F((faceIDX + 1) * ItemCache.TextureSize / TexWidth, idx * ItemCache.TextureSize / TexHeight), //3
                            new Vector2F(faceIDX * ItemCache.TextureSize / TexWidth, (idx + 1) * ItemCache.TextureSize / TexHeight), //4
                            new Vector2F((faceIDX + 1) * ItemCache.TextureSize / TexWidth, (idx + 1) * ItemCache.TextureSize / TexHeight), //5
                        });
                    }
                    break;

                case BlockFaces.Down:
                    {
                        uvs.AddRange(new[]
                        {
                            new Vector2F((faceIDX + 1) * ItemCache.TextureSize / TexWidth, (idx + 1)* ItemCache.TextureSize / TexHeight), //top left
                            new Vector2F((faceIDX + 1) * ItemCache.TextureSize / TexWidth, idx * ItemCache.TextureSize / TexHeight), //bottom left
                            new Vector2F(faceIDX * ItemCache.TextureSize / TexWidth, (idx + 1) * ItemCache.TextureSize / TexHeight), //top right

                            new Vector2F(faceIDX * ItemCache.TextureSize / TexWidth, (idx + 1) * ItemCache.TextureSize / TexHeight), //top right
                            new Vector2F((faceIDX + 1) * ItemCache.TextureSize / TexWidth, idx * ItemCache.TextureSize / TexHeight), //bottom left
                            new Vector2F((faceIDX ) * ItemCache.TextureSize / TexWidth, (idx)* ItemCache.TextureSize / TexHeight), //top left
                        });
                    }
                    break;

                case BlockFaces.North:
                    {
                        uvs.AddRange(new[]
                        {
                            new Vector2F((faceIDX + 1) * ItemCache.TextureSize / TexWidth, idx * ItemCache.TextureSize / TexHeight), //5
                            new Vector2F(faceIDX * ItemCache.TextureSize / TexWidth, idx * ItemCache.TextureSize / TexHeight), //4
                            new Vector2F((faceIDX + 1) * ItemCache.TextureSize / TexWidth, (idx + 1) * ItemCache.TextureSize / TexHeight), //3

                            new Vector2F((faceIDX + 1) * ItemCache.TextureSize / TexWidth, (idx + 1) * ItemCache.TextureSize / TexHeight), //2
                            new Vector2F(faceIDX * ItemCache.TextureSize / TexWidth, idx * ItemCache.TextureSize / TexHeight), //1
                            new Vector2F(faceIDX * ItemCache.TextureSize / TexWidth, (idx + 1) * ItemCache.TextureSize / TexHeight), //0
                        });
                    }
                    break;

                case BlockFaces.South:
                    {
                        uvs.AddRange(new[]
                        {
                            new Vector2F((faceIDX + 1) * ItemCache.TextureSize / TexWidth, (idx + 1) * ItemCache.TextureSize / TexHeight), //0 topleft
                            new Vector2F((faceIDX + 1) * ItemCache.TextureSize / TexWidth, idx * ItemCache.TextureSize / TexHeight), //2 bottom left
                            new Vector2F(faceIDX * ItemCache.TextureSize / TexWidth, (idx + 1) * ItemCache.TextureSize / TexHeight), //1 top right
                            
                            new Vector2F(faceIDX * ItemCache.TextureSize / TexWidth, (idx + 1) * ItemCache.TextureSize / TexHeight), // 4 top right
                            new Vector2F((faceIDX + 1) * ItemCache.TextureSize / TexWidth, idx * ItemCache.TextureSize / TexHeight), //3 bottom left
                            new Vector2F(faceIDX * ItemCache.TextureSize / TexWidth, idx * ItemCache.TextureSize / TexHeight), //5 bottom right
                        });
                    }
                    break;


                case BlockFaces.West:
                    {
                        uvs.AddRange(new[]
                        {
                            new Vector2F((faceIDX + 1) * ItemCache.TextureSize / TexWidth, idx * ItemCache.TextureSize / TexHeight), //5
                            new Vector2F(faceIDX * ItemCache.TextureSize / TexWidth, idx * ItemCache.TextureSize / TexHeight), //4
                            new Vector2F((faceIDX + 1) * ItemCache.TextureSize / TexWidth, (idx + 1) * ItemCache.TextureSize / TexHeight), //3

                            new Vector2F((faceIDX + 1) * ItemCache.TextureSize / TexWidth, (idx + 1) * ItemCache.TextureSize / TexHeight), //2
                            new Vector2F(faceIDX * ItemCache.TextureSize / TexWidth, idx * ItemCache.TextureSize / TexHeight), //1
                            new Vector2F(faceIDX * ItemCache.TextureSize / TexWidth, (idx + 1) * ItemCache.TextureSize / TexHeight), //0
                        });
                    }
                    break;

                case BlockFaces.East:
                    {
                        uvs.AddRange(new[]
                        {
                            new Vector2F((faceIDX + 1) * ItemCache.TextureSize / TexWidth, (idx + 1) * ItemCache.TextureSize / TexHeight), //5
                            new Vector2F((faceIDX + 1) * ItemCache.TextureSize / TexWidth, idx * ItemCache.TextureSize / TexHeight), //3
                            new Vector2F(faceIDX * ItemCache.TextureSize / TexWidth, (idx + 1) * ItemCache.TextureSize / TexHeight), //4
                            
                            new Vector2F(faceIDX * ItemCache.TextureSize / TexWidth, (idx + 1) * ItemCache.TextureSize / TexHeight), //1
                            new Vector2F((faceIDX + 1) * ItemCache.TextureSize / TexWidth, idx * ItemCache.TextureSize / TexHeight), //2
                            new Vector2F(faceIDX * ItemCache.TextureSize / TexWidth, idx * ItemCache.TextureSize / TexHeight), //0
                        });
                    }
                    break;
            }
            
        }


        private static void CreateNormalsCube(BlockFaces face, List<Vector3F> normals)
        {
            switch (face)
            {
                case BlockFaces.Up:
                    {
                        normals.AddRange(new[]
                        {
                            up, up, up, up, up, up
                        });
                    }
                    break;

                case BlockFaces.Down:
                    {
                        normals.AddRange(new[]
                        {
                            down, down, down, down, down, down
                        });
                    }
                    break;

                case BlockFaces.West:
                    {
                        normals.AddRange(new[]
                        {
                            left, left, left, left, left, left
                        });
                    }
                    break;

                case BlockFaces.East:
                    {
                        normals.AddRange(new[]
                        {
                            right, right, right, right, right, right
                        });
                    }
                    break;

                case BlockFaces.North:
                    {
                        normals.AddRange(new[]
                        {
                            forward, forward, forward, forward, forward, forward
                        });
                    }
                    break;

                case BlockFaces.South:
                    {
                        normals.AddRange(new[]
                        {
                            south, south, south, south, south, south
                        });
                    }
                    break;
            }
        }


        /// <summary>
        /// Creates the vertices required to display a cube.
        /// </summary>
        /// <param name="x">The X position of the cube.</param>
        /// <param name="y">The Y position of the cube.</param>
        /// <param name="z">The Z position of the cube.</param>
        /// <param name="face">The orientation of the face.</param>
        /// <param name="vertices">The vertice list to add to.</param>
        private static void CreateVerticesCube(int x, int y, int z, BlockFaces face, List<Vector3F> vertices)
        {
            switch (face)
            {
                case BlockFaces.Up:
                    {
                        vertices.AddRange(new[] {
                            new Vector3F(x, y + 1, z),
                            new Vector3F(x, y + 1, z + 1),
                            new Vector3F(x + 1, y + 1, z),
                            new Vector3F(x + 1, y + 1, z),
                            new Vector3F(x, y + 1, z + 1),
                            new Vector3F(x + 1, y + 1, z + 1)
                        });
                    }
                    break;

                case BlockFaces.Down:
                    {
                        vertices.AddRange(new[] {
                            new Vector3F(x, y, z + 1),
                            new Vector3F(x, y, z),
                            new Vector3F(x + 1, y, z + 1),
                            new Vector3F(x + 1, y, z + 1),
                            new Vector3F(x, y, z),
                            new Vector3F(x + 1, y, z)
                        });
                    }
                    break;

                case BlockFaces.West:
                    {
                        vertices.AddRange(new[] {
                            new Vector3F(x, y, z),
                            new Vector3F(x, y, z + 1),
                            new Vector3F(x, y + 1, z),
                            new Vector3F(x, y + 1, z),
                            new Vector3F(x, y, z + 1),
                            new Vector3F(x, y + 1, z + 1)
                        });
                    }
                    break;

                case BlockFaces.East:
                    {
                        vertices.AddRange(new[]
                        {
                            new Vector3F(x + 1, y + 1, z + 1),
                            new Vector3F(x + 1, y, z + 1),
                            new Vector3F(x + 1, y + 1, z),
                            new Vector3F(x + 1, y + 1, z),
                            new Vector3F(x + 1, y, z + 1),
                            new Vector3F(x + 1, y, z)
                        });
                    }
                    break;

                case BlockFaces.North:
                    {
                        vertices.AddRange(new[]
                        {
                            new Vector3F(x, y, z + 1),
                            new Vector3F(x + 1, y, z + 1),
                            new Vector3F(x, y + 1, z + 1),
                            new Vector3F(x, y + 1, z + 1),
                            new Vector3F(x + 1, y, z + 1),
                            new Vector3F(x + 1, y + 1, z + 1)
                        });
                    }
                    break;

                case BlockFaces.South:
                    {
                        vertices.AddRange(new[]
                        {
                            new Vector3F(x + 1, y + 1, z),
                            new Vector3F(x + 1, y, z),
                            new Vector3F(x, y + 1, z),
                            new Vector3F(x, y + 1, z),
                            new Vector3F(x + 1, y, z),
                            new Vector3F(x, y, z)
                        });
                    }
                    break;
            }
        }

        JSONChunk cwest;
        JSONChunk ceast;
        JSONChunk cnorth;
        JSONChunk csouth;

        private bool IsTranparent(int x, int y, int z)
        {
            //if outside world, yes
            if (y < 0 || y > 255) return true;

            //if not in chunk
            if (x < 0 || x > 15 || z < 0 || z > 15)
            {
                if (x < 0) // check west neighbor
                {
                    if(this.WestNeighbor != null && this.WestNeighbor._Blocks != null)
                    {
                        return this.WestNeighbor[15, y, z].Transparent;
                    }

                    else
                    {
                        return false;
                        if(cwest == null)
                        {
                            cwest = (JSONChunk)JsonConvert.DeserializeObject(File.ReadAllText("save/" + $"c{this.Position.X - 1}_{this.Position.Y}.json"), typeof(JSONChunk));
                        }

                        return ItemCache.Get<Block>(cwest.Data[15 + Width * y + Width * Height * z]).Transparent;
                    }
                }

                else if (x > 15) // check east neighbor
                {

                    if (this.EastNeighbor != null && this.EastNeighbor._Blocks != null)
                    {
                        return this.EastNeighbor[0, y, z].Transparent;
                    }

                    else
                    {
                        return false;
                        if (ceast == null)
                        {
                            ceast = (JSONChunk)JsonConvert.DeserializeObject(File.ReadAllText("save/" + $"c{this.Position.X + 1}_{this.Position.Y}.json"), typeof(JSONChunk));
                        }

                        return ItemCache.Get<Block>(ceast.Data[0 + Width * y + Width * Height * z]).Transparent;
                    }
                }

                else if (z < 0) //check south neighbor
                {
                    if (this.SouthNeighbor != null && this.SouthNeighbor._Blocks != null)
                    {
                        return this.SouthNeighbor[x, y, 15].Transparent;
                    }

                    else
                    {
                        return false;
                        if (csouth == null)
                        {
                            csouth = (JSONChunk)JsonConvert.DeserializeObject(File.ReadAllText("save/" + $"c{this.Position.X}_{this.Position.Y - 1}.json"), typeof(JSONChunk));
                        }

                        return ItemCache.Get<Block>(csouth.Data[x + Width * y + Width * Height * 15]).Transparent;
                    }
                }

                else
                {
                    if (this.NorthNeighbor != null && this.NorthNeighbor._Blocks != null)
                    {
                        return this.NorthNeighbor[x, y, 0].Transparent;
                    }

                    
                    else
                    {
                        return false;
                        if (cnorth == null)
                        {
                            cnorth = (JSONChunk)JsonConvert.DeserializeObject(File.ReadAllText("save/" + $"c{this.Position.X}_{this.Position.Y + 1}.json"), typeof(JSONChunk));
                        }

                        return ItemCache.Get<Block>(cnorth.Data[x + Width * y + Width * Height * 0]).Transparent;
                    }
                }
            }

            else
            {
                return this[x, y, z].Transparent;
            }
        }

        #endregion

    }
}