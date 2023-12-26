using System.Reflection;
using KSP.Game;
using KSP.UI.Binding;
using UnityEngine;
using static KSP.UI.Binding.UIValue_ReadNumber_TextUnits;

namespace CommunityFixes.Fix.VelocityDisplayPrecisionFix;

[Fix("Velocity Display Precision Fix")]
public class VelocityDisplayPrecisionFix : BaseFix
{
    private bool _fixed = false;

    private FieldInfo unitEntryArrayField = typeof(UIValue_ReadNumber_TextUnits).GetField("unitEntryArray", BindingFlags.NonPublic | BindingFlags.Instance);

    public void Update()
    {
        var gameState = Game?.GlobalGameState?.GetGameState();
        if (gameState == null || gameState.GameState is GameState.MainMenu or GameState.WarmUpLoading or GameState.Loading)
            _fixed = false;

        if (_fixed) return;

        var velocityValue = GameObject.Find("GameManager/Default Game Instance(Clone)/UI Manager(Clone)/" +
                                            "Scaled Main Canvas/FlightHudRoot(Clone)/group_navball(Clone)/Container/" +
                                            "GRP-VEL/Container/DataContainer/Items/Value");
        if (velocityValue == null) return;
        var entries = unitEntryArrayField.GetValue(velocityValue.GetComponent<UIValue_ReadNumber_TextUnits>()) as UnitEntry[];
        for (int i = 0; i < entries.Length; ++i)
            entries[i].dontTruncateValue = true;

        _fixed = true;
    }
}