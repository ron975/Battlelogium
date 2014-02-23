﻿using Battlelogium.Core.Javascript;
using Battlelogium.Core.UI;
using CefSharp;
using CefSharp.Wpf;
using System;
using System.IO;
using System.Net;

namespace Battlelogium.Core
{
    public class Battlelog : IDisposable
    {

        public WebView battlelogWebview;
        public JavascriptObject javascriptObject;
        

        public string battlelogURL;
        public string battlefieldName;
        public string battlefieldShortname;
        public string executableName;
        public string originCode;
        public string javascriptURL;
        
        public Battlelog(string battlelogURL, string battlefieldName, string battlefieldShortname, string executableName, string originCode, string javascriptPath, JavascriptObject battlelogiumApp)
        {
            this.javascriptObject = battlelogiumApp;
            this.javascriptURL = javascriptPath;

            this.battlelogURL = battlelogURL;
            this.battlefieldName = battlefieldName;
            this.battlefieldShortname = battlefieldShortname;
            this.executableName = executableName;
            this.originCode = originCode;

            this.SetupWebview(true); //we're debugging.

        }

        public Battlelog(string battlelogURL, string battlefieldName, string battlefieldShortname, string executableName, string originCode, string javascriptPath, UIWindow battlelogiumWindow) : this(battlelogURL, battlefieldName, battlefieldShortname, executableName, originCode , javascriptPath, new JavascriptObject(battlelogiumWindow)) { }

        protected void SetupWebview(bool debug=false)
        {
            Settings settings = new Settings
            {
                PackLoadingDisabled = !debug,
                CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache")
            };

            CEF.Initialize(settings);

            BrowserSettings browserSettings = new BrowserSettings
            {
                FileAccessFromFileUrlsAllowed = true,
                UniversalAccessFromFileUrlsAllowed = true,
                DeveloperToolsDisabled = !debug,
                UserStyleSheetEnabled = true,
                //UserStyleSheetLocation = "data:text/css;charset=utf-8;base64,Ojotd2Via2l0LXNjcm9sbGJhcnt2aXNpYmlsaXR5OmhpZGRlbn0NCiNjb21tdW5pdHktYmFyIC5vdXRlcmFycm93e2Rpc3BsYXk6bm9uZX0="
                UserStyleSheetLocation = "data:text/css;charset=utf-8;base64,I2NvbW11bml0eS1iYXIgLm91dGVyYXJyb3d7ZGlzcGxheTpub25lfQ0KI2NvbW11bml0eS1iYXJ7cGFkZGluZzo1cHggMCFpbXBvcnRhbnR9DQo6Oi13ZWJraXQtc2Nyb2xsYmFye3dpZHRoOjZweDtoZWlnaHQ6NnB4O2JhY2tncm91bmQ6cmdiYSgxOSwyMiwyNiwwLjQpfQ0KOjotd2Via2l0LXNjcm9sbGJhci10cmFja3tiYWNrZ3JvdW5kOnJnYmEoMCwwLDAsMC4xKX0NCjo6LXdlYmtpdC1zY3JvbGxiYXItdGh1bWJ7YmFja2dyb3VuZDpyZ2JhKDAsMCwwLDAuMyl9DQo6Oi13ZWJraXQtc2Nyb2xsYmFyLXRodW1iOmhvdmVye2JhY2tncm91bmQ6cmdiYSgwLDAsMCwwLjQpfQ0KOjotd2Via2l0LXNjcm9sbGJhci10aHVtYjphY3RpdmV7YmFja2dyb3VuZDpyZ2JhKDAsMCwwLC42KX0=",
                /* UserStyleSheetLocation is the following data encoded in utf8 base64 data URI
                 * ::-webkit-scrollbar{visibility:hidden}
                 * #community-bar .outerarrow{display:none}
                 */

            };           
            this.battlelogWebview = new WebView(this.battlelogURL, browserSettings);
            this.battlelogWebview.RegisterJsObject("app", javascriptObject);
            this.battlelogWebview.LoadCompleted += this.LoadCompleted;
            this.battlelogWebview.PropertyChanged += battlelogWebview_PropertyChanged;
        }
        
        private void battlelogWebview_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Address")
            {
                this.battlelogWebview.ExecuteScript("runCustomJS()");
                if (!this.battlelogWebview.Address.Contains(battlelogURL)) this.battlelogWebview.Load(battlelogURL);
            }
        }

        private void LoadCompleted(object sender, EventArgs e)
        {
            this.battlelogWebview.ExecuteScript(
                @"
                    if (document.getElementById('_inject') == null) {
                        var script = document.createElement('script');
    	                script.setAttribute('src', '"+this.javascriptURL+@"');
    	                script.setAttribute('id', '_inject');
    	                document.getElementsByTagName('head')[0].appendChild(script);
                    }"
            );
            this.battlelogWebview.ExecuteScript("runCustomJS();");
        }
    
        public static bool CheckBattlelogConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://battlelog.battlefield.com/"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException(); //TODO implement Dispose properly
        }

    }
}
