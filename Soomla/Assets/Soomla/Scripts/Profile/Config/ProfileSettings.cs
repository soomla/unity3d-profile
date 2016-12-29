/// Copyright (C) 2012-2014 Soomla Inc.
///
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
///
///      http://www.apache.org/licenses/LICENSE-2.0
///
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.

using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Soomla.Profile
{

	#if UNITY_EDITOR
	[InitializeOnLoad]
	#endif
	/// <summary>
	/// This class holds the store's configurations.
	/// </summary>
	public class ProfileSettings : ISoomlaSettings
	{
		private static string ProfileSettingsPrefix = "Profile";
		
		#if UNITY_EDITOR
		
		static ProfileSettings instance = new ProfileSettings();

		static string currentModuleVersion = "2.6.3";

		static ProfileSettings()
		{
			SoomlaEditorScript.addSettings(instance);

			List<string> additionalDependFiles = new List<string>(); //Add files that not tracked in file_list
			additionalDependFiles.Add("Assets/Plugins/iOS/Soomla/libSTTwitter.a");
			additionalDependFiles.Add("Assets/Plugins/iOS/Soomla/libSoomlaiOSProfileTwitter.a");
			additionalDependFiles.Add("Assets/Plugins/Android/Soomla/libs/AndroidProfileTwitter.jar");
			additionalDependFiles.Add("Assets/Plugins/Android/Soomla/libs/twitter4j-asyc-4.0.2.jar");
			additionalDependFiles.Add("Assets/Plugins/Android/Soomla/libs/twitter4j-core-4.0.2.jar");
			additionalDependFiles.Add("Assets/Plugins/iOS/Soomla/libSoomlaiOSProfileGoogle.a");
			additionalDependFiles.Add("Assets/Plugins/Android/Soomla/libs/AndroidProfileGoogle.jar");
			additionalDependFiles.Add("Assets/Plugins/iOS/Soomla/libSoomlaiOSProfileGameCenter.a");
			SoomlaEditorScript.addFileList("Profile", "Assets/Soomla/profile_file_list", additionalDependFiles.ToArray());
		}

		private static string googlePlusDependencySource = "https://s3.amazonaws.com/soomla-library/direct/unity3d-profile-google-plus.unitypackage";
		private static string googlePlusSDKPackagePath = "Assets/WebPlayerTemplates/SoomlaConfig/ios/ios-profile-google/sdk/GooglePlusSDK.unitypackage";

		private BuildTargetGroup[] supportedPlatforms =
		{
			BuildTargetGroup.Android,
			#if UNITY_5
			BuildTargetGroup.iOS,
			#else
			BuildTargetGroup.iPhone,
			#endif
			BuildTargetGroup.WebPlayer,
			BuildTargetGroup.Standalone
		};
		
		//		bool showAndroidSettings = (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android);
		//		bool showIOSSettings = (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iPhone);
		
		Dictionary<string, bool?> socialIntegrationState = new Dictionary<string, bool?>();
		Dictionary<string, Dictionary<string, string>> socialLibPaths = new Dictionary<string, Dictionary<string, string>>();

		GUIContent iTunesKeyLabel = new GUIContent("iTunes App ID [?]:", "iOS App ID given from iTunes Connect (required to use OpenAppRatingPage method).");

		
		GUIContent autoLoginContent = new GUIContent ("Auto Login [?]", "Should Soomla try to log in automatically on start, if user already was logged in in the previous sessions.");
		
		//		GUIContent fbAppId = new GUIContent("FB app Id:");
		//		GUIContent fbAppNS = new GUIContent("FB app namespace:");
		
		GUIContent fbPermissionsContent = new GUIContent ("Login Permissions [?]", "Permissions your app will request from users on login");
		
		GUIContent gpClientId = new GUIContent ("Client ID [?]", "Client id of your google+ app (iOS only)");
		GUIContent enableGPGS = new GUIContent ("Enable GPGS [?]", "Allows to use Google Play Game Services scope (leaderboards, scores, etc.)");
		
		GUIContent twCustKey = new GUIContent ("Consumer Key [?]", "Consumer key of your twitter app");
		GUIContent twCustSecret = new GUIContent ("Consumer Secret [?]", "Consumer secret of your twitter app");

		GUIContent profileVersion = new GUIContent("Profile Version [?]", "The SOOMLA Profile version.");
		
		private ProfileSettings()
		{
			ApplyCurrentSupportedProviders(socialIntegrationState);

			Dictionary<string, string> twitterPaths = new Dictionary<string, string>();
			twitterPaths.Add("/ios/ios-profile-twitter/libSTTwitter.a", "/iOS/Soomla/libSTTwitter.a");
			twitterPaths.Add("/ios/ios-profile-twitter/libSoomlaiOSProfileTwitter.a", "/iOS/Soomla/libSoomlaiOSProfileTwitter.a");
			twitterPaths.Add("/android/android-profile-twitter/AndroidProfileTwitter.jar", "/Android/Soomla/libs/AndroidProfileTwitter.jar");
			twitterPaths.Add("/android/android-profile-twitter/twitter4j-asyc-4.0.2.jar", "/Android/Soomla/libs/twitter4j-asyc-4.0.2.jar");
			twitterPaths.Add("/android/android-profile-twitter/twitter4j-core-4.0.2.jar", "/Android/Soomla/libs/twitter4j-core-4.0.2.jar");
			socialLibPaths.Add(Provider.TWITTER.ToString(), twitterPaths);

			Dictionary<string, string> googlePaths = new Dictionary<string, string>();
			googlePaths.Add("/ios/ios-profile-google/libSoomlaiOSProfileGoogle.a", "/iOS/Soomla/libSoomlaiOSProfileGoogle.a");
			googlePaths.Add("/android/android-profile-google/AndroidProfileGoogle.jar", "/Android/Soomla/libs/AndroidProfileGoogle.jar");
			googlePaths.Add("/android/android-profile-google/google-play-services_lib/", "/Android/google-play-services_lib");
			socialLibPaths.Add(Provider.GOOGLE.ToString(), googlePaths);

			Dictionary<string, string> gameCenterPaths = new Dictionary<string, string>();
			gameCenterPaths.Add("/ios/ios-profile-gamecenter/libSoomlaiOSProfileGameCenter.a", "/iOS/Soomla/libSoomlaiOSProfileGameCenter.a");
			socialLibPaths.Add(Provider.GAME_CENTER.ToString(), gameCenterPaths);
		}

		//Look for google-play-services_lib in the developers Android Sdk.
		//If not found, fallback to compilations path
//		private string GetGooglePlayServicesPath(){
//			string sdkPath = EditorPrefs.GetString ("AndroidSdkRoot") + "/extras/google/google_play_services/libproject/google-play-services_lib/";
//			string compilationsPath = compilationsRootPath + "/android/android-profile-google/google-play-services_lib/";
//			return System.IO.Directory.Exists (sdkPath) ? sdkPath : compilationsPath;
//		}

		private void WriteSocialIntegrationState()
		{
			List<string> savedStates = new List<string>();
			foreach (var entry in socialIntegrationState) {
				savedStates.Add(entry.Key + "," + ((entry.Value != null && entry.Value.Value) ? 1 : 0));
			}

			string result = string.Empty;
			if (savedStates.Count > 0) {
				result = string.Join(";", savedStates.ToArray());
			}

			SoomlaEditorScript.SetConfigValue(ProfileSettingsPrefix, "SocialIntegration", result);
			SoomlaEditorScript.DirtyEditor();
		}

		public void OnEnable() {
			// Generating AndroidManifest.xml
			//			ManifestTools.GenerateManifest();

			// This is here to make sure automatically detected platforms are
			// copied over at least when the settings pane is opened.
			// This is NOT in the IntegrationGUI method, since we don't want to
			// keep copying every GUI frame
			ReadSocialIntegrationState(socialIntegrationState);
			AutomaticallyIntegratedDetected(socialIntegrationState);
			UpdateIntegrations(socialIntegrationState);
		}

		public void OnModuleGUI() {
			IntegrationGUI();
		}

		public void OnInfoGUI() {
			SoomlaEditorScript.RemoveSoomlaModuleButton(profileVersion, currentModuleVersion, "Profile");
			EditorGUILayout.Space();
		}

		public void OnSoomlaGUI() {

		}

		public void OnAndroidGUI() {

		}

		public void OnIOSGUI(){
			EditorGUILayout.HelpBox("Profile Settings", MessageType.None);
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(iTunesKeyLabel, SoomlaEditorScript.FieldWidth, SoomlaEditorScript.FieldHeight);
			iTunesAppId = EditorGUILayout.TextField(iTunesAppId, SoomlaEditorScript.FieldHeight);
			EditorGUILayout.EndHorizontal();

			if (!iTunesAppId.All(Char.IsDigit)) {
				EditorGUILayout.HelpBox("iTunes App ID should be a number!", MessageType.Error);
			}
			EditorGUILayout.Space();
		}
		
		public void OnWP8GUI(){
			
		}

		void IntegrationGUI()
		{
			EditorGUILayout.LabelField("Social Platforms:", EditorStyles.boldLabel);

			ReadSocialIntegrationState(socialIntegrationState);

			EditorGUI.BeginChangeCheck();

			Dictionary<string, bool?>.KeyCollection keys = socialIntegrationState.Keys;
			for (int i = 0; i < keys.Count; i++) {
				string socialPlatform = keys.ElementAt(i);
				bool? socialPlatformState = socialIntegrationState[socialPlatform];

				EditorGUILayout.BeginHorizontal();

				bool update = false;
				bool doIntegrate = false;
				bool toggleResult = false;
				if (socialPlatformState != null) {
					toggleResult = EditorGUILayout.Toggle(Provider.fromString(socialPlatform).DisplayName, socialPlatformState.Value);
					if (toggleResult != socialPlatformState.Value) {
						socialIntegrationState[socialPlatform] = toggleResult;
						doIntegrate = toggleResult;
						update = true;
					}
				}
				else {
					doIntegrate = IsSocialPlatformDetected(socialPlatform);
					toggleResult = EditorGUILayout.Toggle(socialPlatform, doIntegrate);

					// User changed automatic value
					if (doIntegrate != toggleResult) {
						doIntegrate = toggleResult;
						socialIntegrationState[socialPlatform] = doIntegrate;
						update = true;
					}
				}

				if (update) {
					ApplyIntegrationState(socialPlatform, doIntegrate);
				}

				EditorGUILayout.EndHorizontal();
				DrawPlatformParams(socialPlatform, !toggleResult);
				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.Space();

			if (EditorGUI.EndChangeCheck()) {
				WriteSocialIntegrationState();
			}
		}

		void ApplyIntegrationState (string socialPlatform, bool doIntegrate)
		{
			foreach (var buildTarget in supportedPlatforms) {
				TryAddRemoveSocialPlatformFlag(buildTarget, socialPlatform, !doIntegrate);
			}

			ApplyIntegrationLibraries(socialPlatform, !doIntegrate);
		}

		void AutomaticallyIntegratedDetected (Dictionary<string, bool?> state)
		{
			Dictionary<string, bool?>.KeyCollection keys = state.Keys;
			for (int i = 0; i < keys.Count; i++) {
				string socialPlatform = keys.ElementAt(i);
				bool? socialPlatformState = state[socialPlatform];
				if (socialPlatformState == null) {
					bool doIntegrate = IsSocialPlatformDetected(socialPlatform);
					ApplyIntegrationState(socialPlatform, doIntegrate);
				}
			}
		}

		private void UpdateIntegrations(Dictionary<string, bool?> socialIntegrationState) {
			Dictionary<string, bool?>.KeyCollection keys = socialIntegrationState.Keys;
			for (int i = 0; i < keys.Count; i++) {
				string socialPlatform = keys.ElementAt(i);
				bool? socialPlatformState = socialIntegrationState[socialPlatform];
				ApplyIntegrationLibraries(socialPlatform, !(socialPlatformState != null && socialPlatformState == true));
			}
		}

		private string compilationsRootPath = Application.dataPath + "/WebPlayerTemplates/SoomlaConfig";
		private string pluginsRootPath = Application.dataPath + "/Plugins";

		void ApplyIntegrationLibraries (string socialPlatform, bool remove)
		{
			try {
				Dictionary<string, string> paths = null;
				socialLibPaths.TryGetValue(socialPlatform, out paths);
				if (paths != null) {
					if (remove) {
						//TODO: remove without hurting other components!
						foreach (var pathEntry in paths) {
							//maybe someone else needs google play services...
							if (!pathEntry.Value.Contains("google-play-services_lib") ){
								FileUtil.DeleteFileOrDirectory(pluginsRootPath + pathEntry.Value);
								FileUtil.DeleteFileOrDirectory(pluginsRootPath + pathEntry.Value + ".meta");
							}
						}
					} else {
						foreach (var pathEntry in paths) {
							if (!System.IO.File.Exists(pluginsRootPath + pathEntry.Value)) {
								FileUtil.CopyFileOrDirectory(compilationsRootPath + pathEntry.Key,
															 pluginsRootPath + pathEntry.Value);
							}
						}
					}
				}
			}catch { }
		}

		/** Profile Providers util functions **/

		private void TryAddRemoveSocialPlatformFlag(BuildTargetGroup buildTarget, string socialPlatform, bool remove) {
			string targetFlag = GetSocialPlatformFlag(socialPlatform);
			string scriptDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);
			List<string> flags = new List<string>(scriptDefines.Split(';'));

			if (flags.Contains(targetFlag)) {
				if (remove) {
					flags.Remove(targetFlag);
				}
			}
			else {
				if (!remove) {
					flags.Add(targetFlag);
				}
			}

			string result = string.Join(";", flags.ToArray());
			if (scriptDefines != result) {
				PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, result);
			}
		}

		private string GetSocialPlatformFlag(string socialPlatform) {
			return "SOOMLA_" + socialPlatform.ToUpper();
		}

		void DrawPlatformParams(string socialPlatform, bool isDisabled){
			switch(socialPlatform)
			{
			case "facebook":
				EditorGUI.BeginDisabledGroup(isDisabled);

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				EditorGUILayout.LabelField(fbPermissionsContent,  GUILayout.Width(150), SoomlaEditorScript.FieldHeight);
				FBPermissions = EditorGUILayout.TextField(FBPermissions, SoomlaEditorScript.FieldHeight);
				EditorGUILayout.EndHorizontal();

				if (!isDisabled && !validateFacebookPermissionsString(FBPermissions)) {
					EditorGUILayout.HelpBox("Please, make sure that your permissions strings is comma-separated and each permission consists of small letters and underscore symbol.", MessageType.Error);
				}

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(SoomlaEditorScript.EmptyContent, SoomlaEditorScript.SpaceWidth, SoomlaEditorScript.FieldHeight);
				FBAutoLogin = EditorGUILayout.Toggle(autoLoginContent, FBAutoLogin);
				EditorGUILayout.EndHorizontal();

				EditorGUI.EndDisabledGroup();
				break;
			case "google":
				EditorGUI.BeginDisabledGroup(isDisabled);

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				EditorGUILayout.LabelField(gpClientId,  GUILayout.Width(150), SoomlaEditorScript.FieldHeight);
				GPClientId = EditorGUILayout.TextField(GPClientId, SoomlaEditorScript.FieldHeight);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(SoomlaEditorScript.EmptyContent, SoomlaEditorScript.SpaceWidth, SoomlaEditorScript.FieldHeight);
				GPAutoLogin = EditorGUILayout.Toggle(autoLoginContent, GPAutoLogin);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(SoomlaEditorScript.EmptyContent, SoomlaEditorScript.SpaceWidth, SoomlaEditorScript.FieldHeight);
				GPEnableGS = EditorGUILayout.Toggle(enableGPGS, GPEnableGS);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Google Play iOS:", SoomlaEditorScript.FieldWidth, SoomlaEditorScript.FieldHeight);
				string buttonLabel = null;
				if ((new System.IO.FileInfo(googlePlusSDKPackagePath)).Exists) {
					GUI.enabled = false;
					buttonLabel = "Downloaded";
				} else {
					GUI.enabled = true;
					buttonLabel = "Download";
				}
				if (GUILayout.Button(buttonLabel)) {
					if (GUI.enabled) {
						var googlePlusDirectoryPath = googlePlusSDKPackagePath.Substring(0, googlePlusSDKPackagePath.LastIndexOf("/"));
						if (!System.IO.Directory.Exists(googlePlusDirectoryPath)) {
							System.IO.Directory.CreateDirectory(googlePlusDirectoryPath);
						}
						new System.Net.WebClient().DownloadFile(new Uri(googlePlusDependencySource), googlePlusSDKPackagePath);
						AssetDatabase.ImportPackage(googlePlusSDKPackagePath, false);
					}
				}
				GUI.enabled = true;
				EditorGUILayout.EndHorizontal();

				EditorGUI.EndDisabledGroup();
				break;
			case "twitter":
				EditorGUI.BeginDisabledGroup(isDisabled);

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				EditorGUILayout.LabelField(twCustKey, GUILayout.Width(150), SoomlaEditorScript.FieldHeight);
				TwitterConsumerKey = EditorGUILayout.TextField(TwitterConsumerKey, SoomlaEditorScript.FieldHeight);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				EditorGUILayout.LabelField(twCustSecret,  GUILayout.Width(150), SoomlaEditorScript.FieldHeight);
				TwitterConsumerSecret = EditorGUILayout.TextField(TwitterConsumerSecret, SoomlaEditorScript.FieldHeight);
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(SoomlaEditorScript.EmptyContent, SoomlaEditorScript.SpaceWidth, SoomlaEditorScript.FieldHeight);
				TwitterAutoLogin = EditorGUILayout.Toggle(autoLoginContent, TwitterAutoLogin);
				EditorGUILayout.EndHorizontal();

				EditorGUI.EndDisabledGroup();
				break;
			case "gameCenter":
					EditorGUI.BeginDisabledGroup(isDisabled);
					
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(SoomlaEditorScript.EmptyContent, SoomlaEditorScript.SpaceWidth, SoomlaEditorScript.FieldHeight);
					GameCenterAutoLogin = EditorGUILayout.Toggle(autoLoginContent, GameCenterAutoLogin);
					EditorGUILayout.EndHorizontal();
					
					EditorGUI.EndDisabledGroup();
				break;
			default:
				break;
			}
		}

		#endif

		/** Profile Specific Variables **/

		public static Dictionary<string, bool?> IntegrationState
		{
			get {
				Dictionary<string, bool?> result = new Dictionary<string, bool?>();
				ApplyCurrentSupportedProviders(result);

				Dictionary<string, bool?>.KeyCollection keys = result.Keys;
				for (int i = 0; i < keys.Count; i++) {
					string key = keys.ElementAt(i);
					result[key] = IsSocialPlatformDetected(key);
				}

				ReadSocialIntegrationState(result);
				return result;
			}
		}

		private static void ApplyCurrentSupportedProviders(Dictionary<string, bool?> target) {
			target.Add(Provider.FACEBOOK.ToString(), null);
			target.Add(Provider.TWITTER.ToString(), null);
			target.Add(Provider.GOOGLE.ToString(), null);
			target.Add(Provider.GAME_CENTER.ToString(), null);
		}

		private static bool IsSocialPlatformDetected(string platform)
		{
			if (Provider.fromString(platform) == Provider.FACEBOOK) {
				Type fbType = Type.GetType("FB");
				return (fbType != null);
			}

			return false;
		}

		private static void ReadSocialIntegrationState(Dictionary<string, bool?> toTarget)
		{
			string value = SoomlaEditorScript.GetConfigValue(ProfileSettingsPrefix, "SocialIntegration");

			if (value != null) {
				string[] savedIntegrations = value.Split(';');
				foreach (var savedIntegration in savedIntegrations) {
					string[] platformValue = savedIntegration.Split(',');
					if (platformValue.Length >= 2) {
						string platform = platformValue[0];
						int state = int.Parse(platformValue[1]);

						bool? platformState = null;
						if (toTarget.TryGetValue(platform, out platformState)) {
							toTarget[platform] = (state > 0);
						}
					}
				}
			}
			else {
				Dictionary<string, bool?>.KeyCollection keys = toTarget.Keys;
				for (int i = 0; i < keys.Count; i++) {
					string key = keys.ElementAt(i);
					toTarget[key] = null;
				}
			} 
		}

		/** Platform-dependent and SN-independent **/

		public static string ITUNESS_APP_ID = "";

		private static string itunesAppId;
		public static string iTunesAppId
		{
			get {
				if (itunesAppId == null) {
					itunesAppId = SoomlaEditorScript.GetConfigValue (ProfileSettingsPrefix, "iTunesAppId");
					if (itunesAppId == null) {
						itunesAppId = ITUNESS_APP_ID;
					}
				}
				return itunesAppId;
			}
			set {
				if (itunesAppId != value) {
					itunesAppId = value;
					SoomlaEditorScript.SetConfigValue(ProfileSettingsPrefix, "iTunesAppId", value);
					SoomlaEditorScript.DirtyEditor();
				}
			}
		}

		/** FACEBOOK **/

		public static string FB_APP_ID_DEFAULT = "YOUR FB APP ID";

		private static string fBAppId;
		public static string FBAppId
		{
			get {
				if (fBAppId == null) {
					fBAppId = SoomlaEditorScript.GetConfigValue (ProfileSettingsPrefix, "FBAppId");
					if (fBAppId == null) {
						fBAppId = FB_APP_ID_DEFAULT;
					}
				}
				return fBAppId;
			}
			set {
				if (fBAppId != value) {
					fBAppId = value;
					SoomlaEditorScript.SetConfigValue(ProfileSettingsPrefix, "FBAppId", value);
					SoomlaEditorScript.DirtyEditor();
				}
			}
		}

		public static string FB_APP_NS_DEFAULT = "YOUR FB APP ID";

		private static string fBAppNamespace;
		public static string FBAppNamespace
		{
			get {
				if (fBAppNamespace == null) {
					fBAppNamespace = SoomlaEditorScript.GetConfigValue (ProfileSettingsPrefix, "FBAppNS");
					if (fBAppNamespace == null) {
						fBAppNamespace = FB_APP_NS_DEFAULT;
					}
				}
				return fBAppNamespace;
			}
			set {
				if (fBAppNamespace != value) {
					fBAppNamespace = value;
					SoomlaEditorScript.SetConfigValue(ProfileSettingsPrefix, "FBAppNS", value);
					SoomlaEditorScript.DirtyEditor();
				}
			}
		}

		public static string FB_PERMISSIONS_DEFAULT = "email,user_birthday,user_photos,user_friends,user_posts";
		private static string FB_PERMISSIONS_STRING_REGEXP = "([a-z]+(_[a-z]+)*)+(,([a-z]+(_[a-z]+)*)+)*";

		private static string fBPermissions;
		public static string FBPermissions
		{
			get {
				if (fBPermissions == null) {
					fBPermissions = SoomlaEditorScript.GetConfigValue (ProfileSettingsPrefix, "FBPermissions");
					if (fBPermissions == null) {
						fBPermissions = FB_PERMISSIONS_DEFAULT;
					}
				}
				return fBPermissions;
			}
			set {
				if (fBPermissions != value){
					fBPermissions = value;
					SoomlaEditorScript.SetConfigValue(ProfileSettingsPrefix, "FBPermissions", value);
					SoomlaEditorScript.DirtyEditor();
				}
			}
		}

		private static string fBAutoLogin;
		public static bool FBAutoLogin
		{
			get {
				if (fBAutoLogin == null){
					fBAutoLogin = SoomlaEditorScript.GetConfigValue (ProfileSettingsPrefix, "FBAutoLogin");
					if (fBAutoLogin == null) {
						fBAutoLogin = false.ToString();
					}
				}
				return Convert.ToBoolean(fBAutoLogin);
			}
			set {
				if (fBAutoLogin != value.ToString()) {
					fBAutoLogin = value.ToString();
					SoomlaEditorScript.SetConfigValue(ProfileSettingsPrefix, "FBAutoLogin", value.ToString());
					SoomlaEditorScript.DirtyEditor();
				}
			}
		}

		private static bool validateFacebookPermissionsString(string permissionString) {
			return permissionString.Length > 0 &&
				((new System.Text.RegularExpressions.Regex(FB_PERMISSIONS_STRING_REGEXP)).Match(permissionString).Length == permissionString.Length);
		}


		/** GOOGLE+ **/

		public static string GP_CLIENT_ID_DEFAULT = "YOUR GOOGLE+ CLIENT ID";

		private static string gPClientId;
		public static string GPClientId
		{
			get {
				if (gPClientId == null) {
					gPClientId = SoomlaEditorScript.GetConfigValue (ProfileSettingsPrefix, "GPClientId");
					if (gPClientId == null) {
						gPClientId = GP_CLIENT_ID_DEFAULT;
					}
				}
				return gPClientId;
			}
			set {
				if (gPClientId != value){
					gPClientId = value;
					SoomlaEditorScript.SetConfigValue(ProfileSettingsPrefix, "GPClientId", value);
					SoomlaEditorScript.DirtyEditor();
				}
			}
		}

		private static string gPAutoLogin;
		public static bool GPAutoLogin
		{
			get {
				if (gPAutoLogin == null) {
					gPAutoLogin = SoomlaEditorScript.GetConfigValue (ProfileSettingsPrefix, "GoogleAutoLogin");
					if (gPAutoLogin == null) {
						gPAutoLogin = false.ToString();
					}
				}
				return Convert.ToBoolean(gPAutoLogin);
			}
			set {
				if (gPAutoLogin != value.ToString()) {
					gPAutoLogin = value.ToString();
					SoomlaEditorScript.SetConfigValue(ProfileSettingsPrefix, "GoogleAutoLogin", value.ToString());
					SoomlaEditorScript.DirtyEditor();
				}
			}
		}

		private static string gPEnableGS;
		public static bool GPEnableGS
		{
			get {
				if (gPEnableGS == null) {
					gPEnableGS = SoomlaEditorScript.GetConfigValue (ProfileSettingsPrefix, "GoogleEnableGPGS");
					if (gPEnableGS == null) {
					gPEnableGS = false.ToString();
					}
				}
				return Convert.ToBoolean(gPEnableGS);
			}
			set {
				if (gPEnableGS != value.ToString()) {
					gPEnableGS = value.ToString();
					SoomlaEditorScript.SetConfigValue(ProfileSettingsPrefix, "GoogleEnableGPGS", value.ToString());
					SoomlaEditorScript.DirtyEditor();
				}
			}
		}

		/** TWITTER **/

		public static string TWITTER_CONSUMER_KEY_DEFAULT = "YOUR TWITTER CONSUMER KEY";
		public static string TWITTER_CONSUMER_SECRET_DEFAULT = "YOUR TWITTER CONSUMER SECRET";

		private static string twitterConsumerKey;
		public static string TwitterConsumerKey
		{
			get {
				if (twitterConsumerKey == null){
					twitterConsumerKey = SoomlaEditorScript.GetConfigValue (ProfileSettingsPrefix, "TwitterConsumerKey");
					if (twitterConsumerKey == null) {
						twitterConsumerKey = TWITTER_CONSUMER_KEY_DEFAULT;
					}	
				}
				return twitterConsumerKey;
			}
			set
			{
				if (twitterConsumerKey != value) {
					twitterConsumerKey = value;
					SoomlaEditorScript.SetConfigValue(ProfileSettingsPrefix, "TwitterConsumerKey", value);
					SoomlaEditorScript.DirtyEditor();
				}
			}
		}

		private static string twitterConsumerSecret;
		public static string TwitterConsumerSecret
		{
			get {
				if (twitterConsumerSecret == null) {
					twitterConsumerSecret = SoomlaEditorScript.GetConfigValue (ProfileSettingsPrefix, "TwitterConsumerSecret");
					if (twitterConsumerSecret == null) {
						twitterConsumerSecret = TWITTER_CONSUMER_SECRET_DEFAULT;
					}
				}
				return twitterConsumerSecret;
			}
			set
			{
				if (twitterConsumerSecret != value) {
					twitterConsumerSecret = value;
					SoomlaEditorScript.SetConfigValue(ProfileSettingsPrefix, "TwitterConsumerSecret", value);
					SoomlaEditorScript.DirtyEditor();
				}
			}
		}
		
		private static string twitterAutoLogin;
		public static bool TwitterAutoLogin
		{
			get {
				if (twitterAutoLogin == null) {
					twitterAutoLogin = SoomlaEditorScript.GetConfigValue (ProfileSettingsPrefix, "TwitterAutoLogin");
					if (twitterAutoLogin == null) {
						twitterAutoLogin = false.ToString();
					}
				}
				return Convert.ToBoolean(twitterAutoLogin);
			}
			set {
				if (twitterAutoLogin != value.ToString()) {
					twitterAutoLogin = value.ToString();
					SoomlaEditorScript.SetConfigValue(ProfileSettingsPrefix, "TwitterAutoLogin", value.ToString());
					SoomlaEditorScript.DirtyEditor();
				}
			}
		}

		private static string gameCenterAutoLogin;
		public static bool GameCenterAutoLogin
		{
			get {
				if (gameCenterAutoLogin == null) {
					gameCenterAutoLogin = SoomlaEditorScript.GetConfigValue (ProfileSettingsPrefix, "GameCenterAutoLogin");
					if (gameCenterAutoLogin == null) {
						gameCenterAutoLogin = false.ToString();
					}
				}
				return Convert.ToBoolean(gameCenterAutoLogin);
			}
			set {
				if (gameCenterAutoLogin != value.ToString()) {
					gameCenterAutoLogin = value.ToString();
					SoomlaEditorScript.SetConfigValue(ProfileSettingsPrefix, "GameCenterAutoLogin", value.ToString());
					SoomlaEditorScript.DirtyEditor();
				}
			}
		}
	}
}
