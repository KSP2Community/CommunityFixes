﻿using HarmonyLib;
using KSP.Game;
using UnityEngine.UI;

namespace CommunityFixes.Fix.SaveLoadDateTimeFix;

internal class SaveLoadDateTimeFix_Patch
{
    [HarmonyPatch(typeof(SaveLoadDialogFileEntry), nameof(SaveLoadDialogFileEntry.Initialize), new Type[] { typeof(ExtendedSaveFileInfo), typeof(bool), typeof(bool) })]
    [HarmonyPrefix]
    public static void SaveLoadDialogFileEntry_Initialize(SaveLoadDialogFileEntry __instance, ExtendedSaveFileInfo fileInfo, bool loading, bool isLastPlayed)
    {
        Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture;
    }

    [HarmonyPatch(typeof(SaveLoadDialog), nameof(SaveLoadDialog.UpdateLoadMenuGameInformation), new Type[] { typeof(ExtendedSaveFileInfo), typeof(Image) })]
    [HarmonyPrefix]
    public static void SaveLoadDialog_UpdateLoadMenuGameInformation(SaveLoadDialog __instance, ExtendedSaveFileInfo fileInfo, Image thumnailScreenshot)
    {
        Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture;
    }

    [HarmonyPatch(typeof(CampaignLoadMenu), nameof(CampaignLoadMenu.UpdateLoadMenuGameInformation), new Type[] { typeof(ExtendedSaveFileInfo), typeof(Image) })]
    [HarmonyPrefix]
    public static void CampaignLoadMenu_UpdateLoadMenuGameInformation(CampaignLoadMenu __instance, ExtendedSaveFileInfo fileInfo, Image thumnailScreenshot)
    {
        Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture;
    }

    [HarmonyPatch(typeof(CampaignTileEntry), nameof(CampaignTileEntry.Initialize), new Type[] { typeof(ExtendedSaveFileInfo), typeof(CampaignLoadMenu), typeof(CampaignMenu) })]
    [HarmonyPrefix]
    public static void CampaignTileEntry_UpdateLoadMenuGameInformation(CampaignTileEntry __instance, ExtendedSaveFileInfo fileInfo, CampaignLoadMenu loadMenu, CampaignMenu campaignMenu)
    {
        Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture;
    }
}