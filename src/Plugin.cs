using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

#nullable enable
[BepInPlugin(GUID, NAME, VERSION)]
public class HKSS_IntroSkip : BaseUnityPlugin
{
	public const string GUID = "hkss_introskip";
	public const string NAME = "HKSS IntroSkip";
	public const string VERSION = "1.0.0";

	Harmony? harmony;

	// bool showIntroSequence = true;
	// /* 0x00282EA2 02           */ IL_025A: ldarg.0
	// /* 0x00282EA3 17           */ IL_025B: ldc.i4.1
	// /* 0x00282EA4 7DCB8C0004   */ IL_025C: stfld     bool StartManager/'<Start>d__8'::'<showIntroSequence>5__3'
	static readonly CodeMatch[] matches = [
		new CodeMatch(OpCodes.Ldc_I4_1),
		new CodeMatch(OpCodes.Stfld, typeof(StartManager).GetNestedType("<Start>d__8", BindingFlags.NonPublic)
			.GetField("<showIntroSequence>5__3", BindingFlags.NonPublic | BindingFlags.Instance))
	];

	void Awake()
	{
		harmony = new Harmony(GUID);

        System.Type targetType = typeof(StartManager).GetNestedType("<Start>d__8", BindingFlags.NonPublic);
        MethodInfo method = targetType.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        MethodInfo transpiler = typeof(HKSS_IntroSkip).GetMethod(nameof(Transpiler), BindingFlags.Static | BindingFlags.NonPublic);

		harmony.Patch(method, transpiler: new HarmonyMethod(transpiler));
	}

	void OnDestroy()
	{
		harmony?.UnpatchSelf();
	}

	static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		return new CodeMatcher(instructions)
			.MatchForward(false, matches)
			.SetInstruction(new CodeInstruction(OpCodes.Ldc_I4_0))
			.InstructionEnumeration();
	}
}