using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KogamaWorldEdit.Common;

public static class Materials
{
    public static readonly Dictionary<string, int> Map = new()
    {
        { "air", -1 },
        { "bright_red", 0 },
        { "red", 1 },
        { "dark_red", 2 },
        { "sand", 3 },
        { "light_purple_fabric", 4 },
        { "bright_blue", 5 },
        { "blue", 6 },
        { "dark_blue", 7 },
        { "caramel", 8 },
        { "purple_fabric", 9 },
        { "bright_green", 10 },
        { "green", 11 },
        { "dark_green", 12 },
        { "ceramic", 13 },
        { "dark_purple_fabric", 14 },
        { "yellow", 15 },
        { "bright_orange", 16 },
        { "orange", 17 },
        { "butter", 18 },
        { "sandstone", 19 },
        { "light_concrete", 20 },
        { "concrete", 21 },
        { "dark_concrete", 22 },
        { "black_concrete", 23 },
        { "khaki", 24 },
        { "ice", 25 },
        { "lava", 26 },
        { "bouncy", 27 },
        { "poison", 28 },
        { "parkour", 29 },
        { "bricks", 30 },
        { "bright_wood", 31 },
        { "cobblestone", 32 },
        { "cement", 33 },
        { "camouflage", 34 },
        { "green_pavement", 35 },
        { "ancient_cobblestone", 36 },
        { "red_bricks", 37 },
        { "yellow_bricks", 38 },
        { "zigzag", 39 },
        { "metal_pattern", 40 },
        { "metal", 41 },
        { "mushroom", 42 },
        { "black_ice", 43 },
        { "pink_fabric", 44 },
        { "red_grid", 45 },
        { "green_grid", 46 },
        { "circuit", 47 },
        { "grey_bricks", 48 },
        { "spotty", 49 },
        { "metal_scraps", 50 },
        { "slime", 51 },
        { "dark_wood", 53 },
        { "super_bouncy", 54 },
        { "cloud", 55 },
        { "soft_destructible", 56 },
        { "medium_destructible", 57 },
        { "hard_destructible", 58 },
        { "cracked_ice", 59 },
        { "striped_cement", 60 },
        { "machinery", 61 },
        { "embossed_metal", 62 },
        { "scrolling", 63 },
        { "kill", 64 },
        { "heal", 65 },
        { "slow", 66 },
        { "speed", 67 },
        { "crumble", 68 },
    };

    public static int GetMaterialId(string input)
    {
        if (int.TryParse(input, out int id))
            return id;

        if (Map.TryGetValue(input.ToLower(), out int materialId))
            return materialId;

        return -999;
    }
}

