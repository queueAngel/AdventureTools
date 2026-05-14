using AdventureTools.Items;
using AdventureTools.UI;
using AdventureTools.Utilities;
using AdventureTools.WorldNPCs;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.IO;

namespace AdventureTools;

	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
public sealed class AdventureTools : Mod
{
	public static AdventureTools Instance;
	public AdventureTools() => Instance = this;
}
public sealed class BiomeSystem : ModSystem
{
    // list of every custom biome
	public static List<CustomBiome> customBiomes = [];
	public static JsonNode biomesNode;
	public const string defaultContent = """
			{
				"Biomes": [
				],
				"NPCs": [
				]
			}
			""";
	public static string ModDirectory => Path.Combine(Main.SavePath, nameof(AdventureTools));
    public static string WorldFileName => Main.ActiveWorldFileData.UniqueId.ToString() + ".json";
    public override void OnWorldLoad()
    {
		var dirPath = ModDirectory;
		Directory.CreateDirectory(dirPath);
		var filePath = Path.Combine(dirPath, WorldFileName);
		if (!File.Exists(filePath))
			File.WriteAllText(filePath, defaultContent);
		using var file = File.Open(filePath, FileMode.OpenOrCreate);
		biomesNode = JsonNode.Parse(file, documentOptions: new JsonDocumentOptions { AllowTrailingCommas = true });
		customBiomes.Clear();
    }
    public override void OnWorldUnload()
    {
		if (biomesNode is null)
			return;
        var dirPath = ModDirectory;
        Directory.CreateDirectory(dirPath);
        var filePath = Path.Combine(dirPath, WorldFileName);
		using var stream = File.Open(filePath, FileMode.OpenOrCreate);
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { SkipValidation = true });
		biomesNode.WriteTo(writer);
		biomesNode = null;
    }
    public override void SaveWorldData(TagCompound tag)
    {

    }
    public override void LoadWorldData(TagCompound tag)
    {

    }
	public static readonly Asset<Texture2D> _selectBox = ModContent.Request<Texture2D>(nameof(AdventureTools) + "/SelectBox");
    public override void PostDrawTiles()
    {
		var sb = Main.spriteBatch;
		sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		foreach (var biome in CollectionsMarshal.AsSpan(customBiomes))
		{
			//biome.RenderPreview(sb);
            var gon = biome.Area;
            for (int i = 0; i < gon.Length - 1; i++)
            {
                Utils.DrawLine(sb, gon[i].ToWorld(), gon[i + 1].ToWorld(), Color.Cyan, Color.Cyan, 2f);
            }
            Utils.DrawLine(sb, gon[^1].ToWorld(), gon[0].ToWorld(), Color.Cyan, Color.Cyan, 2f);
        }
		var processing = CadastralUIState._processingPolygon;
		if (processing.Count != 0)
		{
            for (int i = 0; i < processing.Count - 1; i++)
            {
                Utils.DrawLine(sb, processing[i].ToWorld(), processing[i + 1].ToWorld(), Color.White, Color.White, 2f);
            }
			var up = CadastralPlayer.realPosition;
			var procFinish = CadastralUIState._processingPolygonFinished;
			var col = procFinish ? Color.White : up == processing[0] ? Color.Lime : Color.Yellow;
            Utils.DrawLine(sb, processing[^1].ToWorld(), procFinish ? processing[0].ToWorld() : up.ToWorld(), col, col, 2f);
        }
		var sel = CadastralUIState._selectState;
		if (sel == SelectionState.Selecting)
		{
			var a = CadastralUIState._selectStart;
			var b = CadastralUIState._selectEnd;
			if (a.DistanceSQ(b) >= 64f)
			{
                var rect = GeometryUtils.RectangleFromPoints(a - Main.screenPosition, b - Main.screenPosition);
                DrawUtils.Draw9Slice(sb, _selectBox.Value, rect, true);
            }
        }
		else if (sel == SelectionState.Selected)
		{
			foreach (var element in CadastralUIState._selection)
			{
				if (element is CustomBiome)
					continue;
				var rect = element switch
				{
					NPC wN => wN.Hitbox,
					WorldNPCTileEntity wT => new Rectangle(wT.Position.X * 16, (wT.Position.Y - 1) * 16, 16, 32),
					_ => throw null
				};
				rect.X -= (int)Main.screenPosition.X;
				rect.Y -= (int)Main.screenPosition.Y;
				DrawUtils.Draw9Slice(sb, _selectBox.Value, rect, true);
			}
		}
		Main.LocalPlayer.GetModPlayer<CadastralPlayer>().RenderProbe(sb);
		sb.End();
    }
	private static Color _currentTileColor;
	private static Color _currentBgColor;
	private static float _currentBrightness = 1f;
    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
		var schema = Main.LocalPlayer.GetModPlayer<CustomBiomePlayer>().PreviousBiomeData;
		var targetLight = SchemaUtils.Hex(schema, "LightColor") ?? tileColor;
		var targetBgLight = SchemaUtils.Hex(schema, "BackgroundLightColor") ?? backgroundColor;
		var speed = 0.1f;
		tileColor = _currentTileColor = Color.Lerp(_currentTileColor, targetLight, speed);
		backgroundColor = _currentBgColor = Color.Lerp(_currentBgColor, targetBgLight, speed);
    }
    public override void ModifyLightingBrightness(ref float scale)
    {
        var schema = Main.LocalPlayer.GetModPlayer<CustomBiomePlayer>().PreviousBiomeData;
		var targetBright = scale;
		if (schema?.TryGetPropertyValue("Brightness", out var bNode) == true)
			targetBright = (float)bNode;
		scale = _currentBrightness = float.Lerp(_currentBrightness, targetBright, 0.1f);
    }
}
public sealed class CustomBiomePlayer : ModPlayer
{
	public JsonObject CurrentBiomeData;
	public JsonObject PreviousBiomeData;
	private int _savedMount = -1;
    public override void PostUpdateMiscEffects()
    {
		if (Player.whoAmI != Main.myPlayer)
			return;
		var schema = CurrentBiomeData;
        if (schema is null)
            return;

		// skies
		HandleFX("Skies", SkyManager.Instance, s => s.IsActive());
		// filters
		HandleFX("Filters", Filters.Scene, f => f.IsActive());
		// overlays
		HandleFX("Overlays", Overlays.Scene, o => o.Mode != OverlayMode.Inactive);

		void HandleFX<T>(string property, EffectManager<T> mgr, Func<T, bool> active) where T : GameEffect
		{
			if (!schema.TryGetPropertyValue(property, out var effectsNode))
				return;
			foreach (var effectNode in effectsNode.AsArray())
			{
				var effect = (string)effectNode;
				var fx = mgr[effect];
				if (fx != null && !active(fx))
				{
					mgr.OnActivate(fx, default);
					fx.Activate(default);
				}
			}
		}
    }
    public override void PreUpdate()
    {
		/*
		StaffofLandTenure._processingPolygon.Clear();
		StaffofLandTenure._processingPolygonFinished = false;
		*/
		var p = Player.Center;
		var currentCustomBiome = CustomBiome.AtPosition(p);
		CurrentBiomeData = currentCustomBiome?.Schema;
    }
	public override void PreUpdateBuffs()
	{
		 if (CurrentBiomeData is null)
			return;
		var buffs = CurrentBiomeData["Buffs"]?.AsArray();
		if (buffs != null)
		{
			for (int i = 0; i < buffs.Count; i++)
			{
                var buff = (string)buffs[i];
                if (BuffID.Search.TryGetId(buff, out var id))
                    Player.AddBuff(id, 2);
            }
		}
		var mount = CurrentBiomeData["Mount"];
		if (mount != null && MountID.Search.TryGetId((string)mount, out var mtId))
		{
            if (_savedMount == -1)
                _savedMount = Player.mount.Active ? Player.mount.Type : -2;
            Player.mount.SetMount(mtId, Player);
        }
	}
    public override void PostUpdate()
    {
        if (CurrentBiomeData is null)
		{
			if (PreviousBiomeData != null && PreviousBiomeData["Mount"] != null)
			{
                if (_savedMount >= 0)
                    Player.mount.SetMount(_savedMount, Player);
                else
                    Player.mount.Dismount(Player);
                _savedMount = -1;
                PreviousBiomeData = null;
            }
            return;
        }

		PreviousBiomeData = CurrentBiomeData;
        CurrentBiomeData = null;
    }
}
public sealed class CustomBiomeScene : ModSceneEffect
{
	public override bool IsSceneEffectActive(Player player) => player.Biomes().CurrentBiomeData != null;
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
	public override int Music
	{
		get
		{
			if (Main.LocalPlayer.Biomes().CurrentBiomeData.TryGetPropertyValue("Music", out var musicNode))
				if (MusicID.Search.TryGetId((string)musicNode, out var id))
					return id;
			return -1;
        }
	}
}
public sealed class CustomBiomeBackground : GlobalBackgroundStyle
{
    internal static readonly FrozenDictionary<string, byte> _surfaceBgMap;
    static CustomBiomeBackground()
    {
        var temp = new Dictionary<string, byte>();
        var fields = typeof(SurfaceBackgroundID).GetFields();
        for (int i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            temp.Add(field.Name, (byte)(int)field.GetValue(null));
        }
        _surfaceBgMap = temp.ToFrozenDictionary();
    }
    public override void ChooseSurfaceBackgroundStyle(ref int style)
    {
		if (Main.gameMenu)
			return;
		// this hook is called in the draw loop, so CurrentBiomeData is timed wrong and will always be null. so use PreviousBiomeData instead
		var biomeData = Main.LocalPlayer.Biomes().PreviousBiomeData;
		if (biomeData is null)
			return;
		if (biomeData.TryGetPropertyValue("SurfaceBackground", out var sbNode))
		{
			var path = (string)sbNode;
			if (!path.Contains('/'))
			{
				if (_surfaceBgMap.TryGetValue(path, out var id))
					style = id;
			}
			else if (ModContent.TryFind(path, out ModSurfaceBackgroundStyle moddedBg))
				style = moddedBg.Slot;
        }
    }
    public override void ChooseUndergroundBackgroundStyle(ref int style)
    {
        
    }
}
public sealed class CustomBiomeSpawns : GlobalNPC
{
    public override void OnSpawn(NPC npc, IEntitySource source)
    {
		/*
		var spawningBiome = CustomBiome.AtPosition(npc.Center);
		if (spawningBiome is null)
			return;
		var data = spawningBiome.Schema;
		if (data is null)
			return;
		ref var globalScaling = ref data.GlobalScaling;
		npc.lifeMax = globalScaling.Health.Apply(npc.lifeMax);
		npc.defense = globalScaling.Defense.Apply(npc.defense);
		npc.damage = globalScaling.Damage.Apply(npc.damage);
        // DEBUG ONLY!!!!!!
		// if there are multiple spawn rules that spawn this NPC, there is no way to differentiate them!!!
		// probably need custom spawning system until 1.4.5
        foreach (ref var entry in CollectionsMarshal.AsSpan(data.SpawnRules))
		{
			if (npc.type != entry.ID)
				continue;
			ref var individualScaling = ref entry.Scaling;
			npc.lifeMax = individualScaling.Health.Apply(npc.lifeMax);
			npc.defense = individualScaling.Defense.Apply(npc.defense);
			npc.damage = individualScaling.Damage.Apply(npc.damage);
			break;
		}
		*/
    }
    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
    {
		var biomeData = spawnInfo.Player.Biomes().PreviousBiomeData;
		if (biomeData is null)
			return;
		if (biomeData.TryGetPropertyValue("VanillaSpawnWeight", out var vswNode))
			pool[0] = (float)vswNode;
		if (!biomeData.TryGetPropertyValue("SpawnPool", out var spNode))
			return;
		foreach (var ruleNode in spNode.AsArray())
		{
			var rule = ruleNode.AsObject();
			if (!rule.TryGetPropertyValue("Type", out var typeNode) || !NPCID.Search.TryGetId((string)typeNode, out var type))
				continue;
			if (rule.TryGetPropertyValue("Tile", out var tileNode) && TileID.Search.TryGetId((string)tileNode, out var tileId))
				if (spawnInfo.SpawnTileType != tileId)
					continue;
			var chance = 1f;
			if (rule.TryGetPropertyValue("Rate", out var rateNode))
				chance = (float)rateNode;
			pool.Add(type, chance);
		}
    }
    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
    {
		/*
        var biomeData = player.Biomes().CurrentBiomeData;
		if (biomeData is null)
			return;
		maxSpawns = biomeData.MaxSpawns.Apply(maxSpawns);
		spawnRate = biomeData.SpawnRate.Apply(spawnRate);
		*/
    }
}
public enum RoundingType : byte
{
	None,
	Floor,
	Round,
	Ceiling,
}
public struct ScalingModule
{
	public RoundingType Rounding;
	public IntManipulator Health;
	public IntManipulator Defense;
	public IntManipulator Damage;
	public static ScalingModule FromJson(JsonObject root, string property)
	{
		if (!root.TryGetPropertyValue(property, out var j))
			return default;
		var json = j.AsObject();
		var rounding = RoundingType.None;
		if (json.TryGetPropertyValue("Rounding", out var roundProp))
			rounding = Enum.Parse<RoundingType>((string)roundProp);
		var health = IntManipulator.FromJson(json, "Health");
		var defense = IntManipulator.FromJson(json, "Defense");
		var damage = IntManipulator.FromJson(json, "Damage");
		return new ScalingModule
		{
			Rounding = rounding,
			Health = health,
			Defense = defense,
			Damage = damage,
		};
	}
	public static int Round(float value, RoundingType type) => type switch
	{
		RoundingType.None => (int)value,
		RoundingType.Floor => (int)MathF.Floor(value),
		RoundingType.Round => (int)MathF.Round(value),
		RoundingType.Ceiling => (int)MathF.Ceiling(value),
		_ => throw null,
	};
}
public struct IntManipulator
{
	public float? PreAdd;
	public float? Multiply;
	public float? Add;
	public int? Set;
	public static IntManipulator FromJson(JsonObject root, string property)
	{
		var i = new IntManipulator();
		if (!root.TryGetPropertyValue(property, out var j))
			return i;
		if (j.GetValueKind() is JsonValueKind.Number)
		{
			i.Multiply = (float)j;
			return i;
		}
        var json = j.AsObject();
        if (json.TryGetPropertyValue("Set", out var setProp))
        {
			i.Set = (int)setProp;
			return i;
        }
        if (json.TryGetPropertyValue("PreAdd", out var paProp))
			i.PreAdd = (float)paProp;
		if (json.TryGetPropertyValue("Multiply", out var mulProp))
			i.Multiply = (float)mulProp;
		if (json.TryGetPropertyValue("Add", out var addProp))
			i.Add = (float)addProp;
		return i;
	}
	public readonly int Apply(int value, RoundingType rounding = 0) 
	{
		if (Set.HasValue)
			return Set.Value;
		float val = value;
		if (PreAdd.HasValue)
			val += PreAdd.Value;
		if (Multiply.HasValue)
			val *= Multiply.Value;
		if (Add.HasValue)
			val += Add.Value;
		return ScalingModule.Round(val, rounding);
	}
}
public sealed class CustomBiomeData
{
    public string[] Filters;
	public int UndergroundBackground;
	public ScalingModule GlobalScaling;
	public IntManipulator MaxSpawns;
	public IntManipulator SpawnRate;
	public static CustomBiomeData FromJson(JsonObject json)
	{
		var scaling = ScalingModule.FromJson(json, "Scaling");
		var spawnRate = IntManipulator.FromJson(json, "SpawnRate");
		var maxSpawns = IntManipulator.FromJson(json, "MaxSpawns");

		return new CustomBiomeData
		{
			GlobalScaling = scaling,
			SpawnRate = spawnRate,
			MaxSpawns = maxSpawns,
		};
	}
}
public struct CustomSpawnRule
{
	public int ID;
	public float Chance;
	public int Tile;
	public ScalingModule Scaling;
}
public sealed class CustomBiome
{
	private static RenderTargetLease _lease;
	public JsonObject Schema;
	public UPoint16[] Area;
	public UQuadrat16 BoundingBox;
	public Rectangle WorldBox;
	public bool Contains(UPoint16 p) => GeometryUtils.IsInside(p, Area);
	public void RenderPreview(SpriteBatch sb)
	{
		_lease ??= ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice);
		using (_lease.Scope(clearColor: Color.Transparent))
		{
			DrawUtils.DrawPolygon(Area, Color.Cyan);
		}
		sb.Draw(_lease.Target, Vector2.Zero, Color.White);
	}
	public static CustomBiome AtPosition(Vector2 p)
	{
        if (p.X < 0 || p.X > (Main.maxTilesX + Main.offLimitBorderTiles) * 16 || p.Y < 0 || p.Y > Main.maxTilesY * 16)
            return null;
        var up = GeometryUtils.ToUPoint16(p);
        CustomBiome currentCustomBiome = null;
        foreach (var biome in CollectionsMarshal.AsSpan(BiomeSystem.customBiomes))
        {
            if (biome.Contains(up))
            {
                currentCustomBiome = biome;
                break;
            }
        }
		return currentCustomBiome;
    }
}
public struct UQuadrat16
{
	public ushort Left, Top, Right, Bottom;
	public readonly bool Contains(UPoint16 p) => p.X >= Left && p.X < Right && p.Y >= Top && p.Y < Bottom;
}
public unsafe struct UPoint16
{
	public ushort X;
	public ushort Y;
	public readonly uint GetPacked()
	{
		fixed (ushort* p = &X)
		{
			var ul = *(uint*)p;
			return BitConverter.IsLittleEndian ? ul : BinaryPrimitives.ReverseEndianness(ul);
		}
	}
	public void SetPacked(uint packed)
	{
		fixed (ushort* p = &X)
			*(uint*)p = BitConverter.IsLittleEndian ? packed : BinaryPrimitives.ReverseEndianness(packed);
	}
	public readonly Vector2 ToWorld() => new(X * 16, Y * 16);
    public readonly override int GetHashCode()
    {
		fixed (void* p = &X)
			return *(int*)p;
	}
	public static bool operator ==(UPoint16 lhs, UPoint16 rhs) => lhs.GetHashCode() == rhs.GetHashCode();
	public static bool operator !=(UPoint16 lhs, UPoint16 rhs) => !(lhs == rhs); 
}
public sealed class CustomBiomesConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;
	public static CustomBiomesConfig Instance;
}
