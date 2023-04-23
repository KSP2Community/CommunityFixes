using HarmonyLib;
using KSP.Game;
using KSP.Messages;
using KSP.Sim.impl;
using KSP.UI;
using UnityEngine;

namespace CommunityFixes.Fix.F2HideUIFix;

[Fix("F2 Hide UI Fix")]
public class F2HideUIFix : BaseFix
{
    public override void OnInitialized()
    {
        HarmonyInstance.PatchAll(typeof(F2HideUIFix));

        _hideMapView = UIStateViews.HideFlightView.DeepCopy();

        Game.Messages.Subscribe<GameStateEnteredMessage>(msg =>
        {
            var message = (GameStateEnteredMessage)msg;
            if (message.StateBeingEntered is GameState.FlightView or GameState.Map3DView)
            {
                GameManager.Instance.Game.UI.GetRootCanvas()?.gameObject.transform.Find(
                    "FlightHudRoot(Clone)/group_flightstaging(Clone)/StagingUI/Staging Stack"
                )?.gameObject.SetActive(true);
            }
        });
    }

    private static UIView _hideMapView;

    [HarmonyPatch(typeof(UIStateViews), nameof(UIStateViews.HideFlightView), MethodType.Getter)]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void UIStateViews_HideFlightView_get(ref UIView __result)
    {
        __result.ViewDefinitions.Add(new UIViewElementDefinition
        {
            ID = "StagingStackOAB",
            State = ViewState.Hidden
        });
    }

    [HarmonyPatch(typeof(FlightInputHandler), nameof(FlightInputHandler.OnToggleFlightHUD))]
    [HarmonyPostfix]
    public static void FlightInputHandler_OnToggleFlightHUD()
    {
        var viewController = GameManager.Instance.Game.UI.ViewController;
        if (viewController.CurrentView == UIStateViews.FlightMapView)
        {
            viewController.PushView(_hideMapView);
        }
        else if (viewController.CurrentView == _hideMapView)
        {
            viewController.PushView(UIStateViews.FlightMapView);
        }

        var stagingStack = GameManager.Instance.Game.UI.GetRootCanvas().gameObject.transform.Find(
            "FlightHudRoot(Clone)/group_flightstaging(Clone)/StagingUI/Staging Stack"
        ).gameObject;
        stagingStack.SetActive(!stagingStack.activeSelf);
    }
}