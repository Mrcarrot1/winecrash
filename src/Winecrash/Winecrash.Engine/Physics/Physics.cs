﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winecrash.Engine
{
    public static class Physics
    {
        public static Vector3D Gravity { get; set; } = Vector3D.Down * 9.81D;
        /*public static bool Raycast(Ray ray, double length, out Vector3D position)
        {

        }
        private static bool Intersects(Ray ray, AABB aabb)
        {

        }*/
    }
}
