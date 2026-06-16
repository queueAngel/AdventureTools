using AdventureTools.WorldNPCs;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace AdventureTools.Utilities;

public sealed class MusicToBoxMapper
{
    public static int GetBox(int music)
    {
        var savedMusic = Main.curMusic;
        var musicBox = new Item(ItemID.MusicBox);
        var r = Main.rand;
        int savedInext = r.inext;
        int savedInextp = r.inextp;

        int idx1 = savedInext + 1;
        if (idx1 >= 56) idx1 = 1;

        int idx2 = savedInextp + 1;
        if (idx2 >= 56) idx2 = 1;

        int savedSeed1 = r.SeedArray[idx1];
        int savedSeed2 = r.SeedArray[idx2];

        r.SeedArray[idx2] = r.SeedArray[idx1];

        var d = WorldNPC.Dummy;
        var savedWAI = d.whoAmI;
        d.whoAmI = Main.myPlayer;
        Main.curMusic = music;
        var savedAudio = SoundEngine.IsAudioSupported;
        SoundEngine.IsAudioSupported = false;
        d.ApplyEquipFunctional(musicBox, default);
        SoundEngine.IsAudioSupported = savedAudio;
        Main.curMusic = savedMusic;
        d.whoAmI = savedWAI;

        r.SeedArray[idx1] = savedSeed1;
        r.SeedArray[idx2] = savedSeed2;
        r.inext = savedInext;
        r.inextp = savedInextp;

        return musicBox.type;
    }
}
