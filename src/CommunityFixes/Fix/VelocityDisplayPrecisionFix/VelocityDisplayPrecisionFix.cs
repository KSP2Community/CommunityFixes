using KSP.Game;
using KSP.UI.Binding;
using UnityEngine;

namespace CommunityFixes.Fix.VelocityDisplayPrecisionFix;

[Fix("Velocity Display Precision Fix")]
public class VelocityDisplayPrecisionFix : BaseFix
{
    private bool _fixed;

    public void Update()
    {
        var gameState = Game == null ? null : Game.GlobalGameState?.GetGameState();
        if (gameState == null ||
            gameState.GameState is GameState.MainMenu or GameState.WarmUpLoading or GameState.Loading)
        {
            _fixed = false;
        }

        if (_fixed)
        {
            return;
        }

        var velocityValue = GameObject.Find(
            "GameManager/Default Game Instance(Clone)/UI Manager(Clone)/Scaled Main Canvas/FlightHudRoot(Clone)/" +
            "group_navball(Clone)/Container/GRP-VEL/Container/DataContainer/Items/Value"
        );
        if (velocityValue == null)
        {
            return;
        }

        var entries = velocityValue.GetComponent<UIValue_ReadNumber_TextUnits>().unitEntryArray;
        for (int i = 0; i < entries.Length; ++i)
        {
            entries[i].dontTruncateValue = true;
        }

        _fixed = true;
    }
}