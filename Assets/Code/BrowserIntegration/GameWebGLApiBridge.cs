// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

#if UNITY_WEBGL

using Game.Logic;
using Metaplay.Core;
using Metaplay.Core.Client;
using Metaplay.Core.Json;
using Metaplay.Core.Model;
using Metaplay.Unity;

[MetaWebApiBridge]
public static class GameWebGLApiBridge
{
    public class ChatDialogueEvent
    {
        public string Speaker { get; set; }
        public string DialogueText { get; set; }
    }

    [MetaImportBrowserMethod]
    public static void UpdateChatDialogue(string speaker, string text)
    {
        int methodId = WebApiBridge.GetBrowserMethodId(nameof(GameWebGLApiBridge), nameof(UpdateChatDialogue));
        string requestJson = JsonSerialization.SerializeToString(new ChatDialogueEvent()
        {
            Speaker      = speaker,
            DialogueText = text,
        });
        _ = WebApiBridge.JsonCallSync(methodId, requestJson);
    }

    [MetaImportBrowserMethod]
    public static void UpdateInfoUrl(string url)
    {
        if (url == "null")
            return;

        EnvironmentConfig currentEnvironmentConfig = MetaplaySDK.CurrentEnvironmentConfig;
        if (url.Contains("{dashboardUrl}")) {
            string dashboardUrl = "";
            switch (currentEnvironmentConfig.Id)
            {
                case "localhost": dashboardUrl = "http://dashboard.orca.localhost"; break;
                case "demo":      dashboardUrl = "https://orca-demo-admin.p1.metaplay.io"; break;
                case "sales":     dashboardUrl = "https://deep-clowns-brake-briskly-admin.p2-eu.metaplay.dev"; break;
                case "develop":   dashboardUrl = "https://deep-clowns-brake-nimbly-admin.p2-eu.metaplay.dev"; break;
            }

            url = url.Replace("{dashboardUrl}", dashboardUrl);
        }

        if (url.Contains("{PlayerId}")) {
            url = url.Replace("{PlayerId}", MetaplayClient.PlayerModel.PlayerId.ToString());
        }

        if (url.Contains("{SpreadsheetId}")) {
            string spreadsheetId = "";
            switch (currentEnvironmentConfig.Id)
            {
                case "localhost":
                case "develop":
                    spreadsheetId = "1n7xqND50q3iad6uYKqTI3VsU9g6m3dSSH3JsVvAePFg"; break;
                case "sales":
                case "demo":
                    spreadsheetId = "1X5v-ZsD98JGoK7s5kopOzVKm9wsQmo9pfwUnlWOE74U"; break;
            }

            url = url.Replace("{SpreadsheetId}", spreadsheetId);
        }

        int methodId = WebApiBridge.GetBrowserMethodId(nameof(GameWebGLApiBridge), nameof(UpdateInfoUrl));
        string requestJson = JsonSerialization.SerializeToString(url);
        _ = WebApiBridge.JsonCallSync(methodId, requestJson);
    }

    [MetaImportBrowserMethod]
    public static void UnityApplicationPaused(bool isPaused)
    {
        int methodId = WebApiBridge.GetBrowserMethodId(nameof(GameWebGLApiBridge), nameof(UnityApplicationPaused));
        string requestJson = JsonSerialization.SerializeToString(isPaused);
        _ = WebApiBridge.JsonCallSync(methodId, requestJson);
    }

    [MetaImportBrowserMethod]
    public static void UnityApplicationFocused(bool isFocused)
    {
        int methodId = WebApiBridge.GetBrowserMethodId(nameof(GameWebGLApiBridge), nameof(UnityApplicationFocused));
        string requestJson = JsonSerialization.SerializeToString(isFocused);
        _ = WebApiBridge.JsonCallSync(methodId, requestJson);
    }

    /// <summary>
    /// Example of invoking a method on the browser, in this case to update the current
    /// game state that is shown in the browser.
    /// </summary>
    /// <param name="player"></param>
    // [MetaImportBrowserMethod]
    // public static void UpdateBrowserGameState(PlayerModel player)
    // {
    //     int methodId = WebApiBridge.GetBrowserMethodId(nameof(GameWebGLApiBridge), nameof(UpdateBrowserGameState));
    //     string requestJson = JsonSerialization.SerializeToString(new BrowserStateUpdatedEvent() { NumClicks = player.NumClicks });
    //     _ = WebApiBridge.JsonCallSync(methodId, requestJson);
    // }

    /// <summary>
    /// Example of a method that can be invoked from the browser. In this case, it invokes
    /// a PlayerAction just like the in-game button does.
    /// </summary>
    // [MetaExportToBrowser]
    // public static string ExecuteClickButtonAction(string args)
    // {
    //     // \note Assume MetaplayClient.PlayerContext is valid as the button is only enabled after connection to the server is established
    //     MetaActionResult result = MetaplayClient.PlayerContext.ExecuteAction(new PlayerClickButton());
    //     bool isSuccess = result == MetaActionResult.Success;
    //     return isSuccess ? "true" : "false"; // \note ApiBridge return values must be valid JSON
    // }


}

#endif
