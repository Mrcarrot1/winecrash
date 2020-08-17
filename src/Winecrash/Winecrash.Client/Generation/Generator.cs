﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LibNoise;

namespace Winecrash.Game
{
    public static class Generator
    {
        private static Random GeneratorRandom = new Random();
        public static ushort[] GetChunk(int x, int y, out bool generated)
        {
#if RELEASE
            string fileName = "save/" + $"c{x}_{y}.json";


            if (File.Exists(fileName))
            {
                generated = false;
                return LoadFromSave(fileName);
            }
            else
            {
#endif
            generated = true;
            return CreateTerrain(x, y);

#if RELEASE
            }
#endif

        }

        static LibNoise.Primitive.SimplexPerlin plains = new LibNoise.Primitive.SimplexPerlin("lol".GetHashCode(), NoiseQuality.Standard);
        static LibNoise.Primitive.SimplexPerlin mountains = new LibNoise.Primitive.SimplexPerlin("lol".GetHashCode(), NoiseQuality.Standard);
        static LibNoise.Primitive.ImprovedPerlin caves = new LibNoise.Primitive.SimplexPerlin("lol".GetHashCode(), NoiseQuality.Standard);

        public static void Populate(ushort[] blocks, int chunkx, int chunky, bool save = false, bool erase = false)
        {
            string id;
            ushort cacheindex;
            Dictionary<ushort, string> idscache = new Dictionary<ushort, string>();

            const double TreeSpawnChance = 0.01D;

            for (int z = 0; z < Chunk.Depth; z++)
            {
                for (int y = 0; y < Chunk.Height; y++)
                {
                    for (int x = 0; x < Chunk.Width; x++)
                    {
                        cacheindex = blocks[x + Chunk.Width * y + Chunk.Width * Chunk.Height * z];

                        if (!idscache.TryGetValue(cacheindex, out id))
                        {
                            id = ItemCache.GetIdentifier(cacheindex);
                            idscache.Add(cacheindex, id);
                        }

                        if (id == "winecrash:grass" && GeneratorRandom.NextDouble() < TreeSpawnChance)
                        {

                        }
                    }
                }
            }
        }

        public static ushort[] CreateTerrain(int chunkx, int chunky, bool save = false, bool erase = false)
        {
            ushort[] blocks = new ushort[Chunk.Width * Chunk.Height * Chunk.Depth];

            string id;
            ushort cacheindex = 0;
            Dictionary<string, ushort> idscache = new Dictionary<string, ushort>();

            for (int z = 0; z < Chunk.Depth; z++)
            {
                for (int y = 0; y < Chunk.Height; y++)
                {
                    for (int x = 0; x < Chunk.Width; x++)
                    {
                        id = "winecrash:air";

                        const float scale = 0.015F;
                        const float shiftX = 0; //Début des farlands : 16000000 | Grosses Farlands : 200000000
                        const float shiftZ = 0;

                        const float caveScale = 0.1F;
                        const float thresold = 0.3F;

                        //const float torsadeScale = 0.1F;
                        //const float torsadeThresold = 0.4F;

                        int height = (int)(plains.GetValue((chunkx * Chunk.Width + shiftX + x) * scale, (chunky * Chunk.Depth + shiftZ + z) * scale) * 15) + 64;
                        



                        bool isCave = (((caves.GetValue((chunkx * Chunk.Width + shiftX + (float)x) * caveScale, y * caveScale, (chunky * Chunk.Depth + shiftZ + (float)z) * caveScale)) + 1) /2.0F) < thresold;


                        bool waterlevel = height < 64;


                        if (y == height)
                        {
                            if (waterlevel)
                            {
                                id = "winecrash:sand"; //sand
                            }
                            else
                            {
                                id = "winecrash:grass"; //grass
                            }
                        }
                        else if(y < height)
                        {
                            if (y > height - 3)
                            {
                                if (waterlevel)
                                {
                                    id = "winecrash:sand"; //sand
                                }
                                else
                                {
                                    id = "winecrash:dirt"; //dirt
                                }
                            }
                            else
                                id = "winecrash:stone";
                        }

                        if(isCave)
                        {
                            id = "winecrash:air";
                        }

                        if (y == 2)
                        {
                            if (World.WorldRandom.NextDouble() < 0.33D)
                            {
                                id = "winecrash:bedrock";
                            }
                        }
                        else if (y == 1)
                        {
                            if (World.WorldRandom.NextDouble() < 0.66D)
                            {
                                id = "winecrash:bedrock";
                            }
                        }
                        else if (y == 0)
                        {
                            id = "winecrash:bedrock";
                        }

                        if(!idscache.TryGetValue(id, out cacheindex))
                        {
                            cacheindex = ItemCache.GetIndex(id);
                            idscache.Add(id, cacheindex);
                        }
                        //Server.Log(id);
                        blocks[x + Chunk.Width * y + Chunk.Width * Chunk.Height * z] = cacheindex;//new Block(id);
                    }
                }
            }

            if(save)
            {
                string fileName = "save/" + $"c{chunkx}_{chunky}.json";

                if(erase)
                    File.WriteAllText(fileName, ToJSON(blocks));

                else if(!File.Exists(fileName))
                {
                    File.WriteAllText(fileName, ToJSON(blocks));
                }
            }

            return blocks;
        }
        private static ushort[] LoadFromSave(string path)
        {
            JsonSerializer serializer = new JsonSerializer();
            using (StreamReader sr = File.OpenText(path))
            using (JsonTextReader jtr = new JsonTextReader(sr))
            {
                JSONChunk dc = (JSONChunk)serializer.Deserialize(jtr, typeof(JSONChunk));

                ushort[] blocks = new ushort[Chunk.Width * Chunk.Height * Chunk.Depth];
                int chunkindex = 0;

                for (int z = 0; z < Chunk.Depth; z++)
                {
                    for (int y = 0; y < Chunk.Height; y++)
                    {
                        for (int x = 0; x < Chunk.Width; x++)
                        {
                            blocks[x + Chunk.Width * y + Chunk.Width * Chunk.Height * z] = ItemCache.GetIndex(dc.Palette[dc.Data[chunkindex++]]);
                        }
                    }
                }

                return blocks;
            }
        }

        private static string ToJSON(ushort[] blocks)
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
                        string id = ItemCache.GetIdentifier(blocks[x + Chunk.Width * y + Chunk.Width * Chunk.Height * z]);

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
    }
}
