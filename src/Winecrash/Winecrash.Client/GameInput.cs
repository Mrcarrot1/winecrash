﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winecrash.Engine;

namespace Winecrash.Game
{
    public static class GameInput
    {
        private static Dictionary<string, Keys?> _GameKeys = new Dictionary<string, Keys?>()
        {
            { "Forward", Keys.Z },
            { "Backward", Keys.S },
            { "Left", Keys.Q },
            { "Right", Keys.D },
            { "Jump", Keys.Space },
            { "Run", Keys.LeftControl },
            { "Destroy", Keys.MouseLeftButton },
            { "Interract", Keys.MouseRightButton },
            { "Debug", Keys.F3 },
            { "Paused", Keys.Escape }
        };

        public static Keys Key(string name)
        {
            if(_GameKeys.ContainsKey(name))
            {
                Keys? k = _GameKeys[name];

                if(k != null)
                {
                    return k.Value;
                }
            }

            throw new Exception($"No key is corresponding to \"{name}\"");
        }

        public static void EditKey(string name, Keys? key)
        {
            if (_GameKeys.ContainsKey(name))
            {
                _GameKeys[name] = key;
            }

            else
            {
                _GameKeys.Add(name, key);
            }
        }

        public static void EditKey(string name, Keys key)
        {
            EditKey(name, new Keys?(key));
        }
    }
}
