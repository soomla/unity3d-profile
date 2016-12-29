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
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Soomla.Singletons;

namespace Soomla.Profile {

	/// <summary>
	/// This class provides functions for event handling. To handle various events, just add your
	/// game-specific behavior to the delegates below.
	/// </summary>
	public class ProfileEvents : CodeGeneratedSingleton {

		private const string TAG = "SOOMLA ProfileEvents";

		public static ProfileEvents Instance = null;

        protected override bool DontDestroySingleton
        {
            get { return true; }
        }

        /// <summary>
        /// Initializes the game state before the game starts.
        /// </summary>
        protected override void InitAfterRegisteringAsSingleInstance()
        {
            base.InitAfterRegisteringAsSingleInstance();

            // now we initialize the event pusher
#if UNITY_ANDROID && !UNITY_EDITOR
			pep = new ProfileEventPusherAndroid();
#elif UNITY_IOS && !UNITY_EDITOR
			pep = new ProfileEventPusherIOS();
#endif
        }

        //		private static ProfileEvents instance = null;
#pragma warning disable 414
        private static ProfileEventPusher pep = null;
		#pragma warning restore 414


		public static void Initialize() {
			if (Instance == null) {
				CoreEvents.Initialize();
				Instance = GetSynchronousCodeGeneratedInstance<ProfileEvents>();
				
				SoomlaUtils.LogDebug (TAG, "Initializing ProfileEvents ...");
#if UNITY_ANDROID && !UNITY_EDITOR
				AndroidJNI.PushLocalFrame(100);
				//init ProfileEventHandler
				using(AndroidJavaClass jniEventHandler = new AndroidJavaClass("com.soomla.profile.unity.ProfileEventHandler")) {
					jniEventHandler.CallStatic("initialize");
				}
				AndroidJNI.PopLocalFrame(IntPtr.Zero);
#elif UNITY_IOS && !UNITY_EDITOR
				// On iOS, this is initialized inside the bridge library when we call "soomlaProfile_Initialize" in SoomlaProfileIOS
#endif
			}
		}

		/// <summary>
		/// Handles an <c>onSoomlaProfileInitialized</c> event, which is fired when SoomlaProfile
		/// has been initialzed
		/// </summary>
		public void onSoomlaProfileInitialized()
		{
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onSoomlaProfileInitialized");

			SoomlaProfile.nativeModulesInitialized = true;
			SoomlaProfile.TryFireProfileInitialized();
		}

		/// <summary>
		/// Handles an <c>onUserRatingEvent</c> event
		/// </summary>
		public void onUserRatingEvent()
		{
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onUserRatingEvent");

			ProfileEvents.OnUserRatingEvent ();
			//ProfileEvents.OnUserRatingEvent (new UserRatingEvent());
		}

		/// <summary>
		/// Handles an <c>onUserProfileUpdated</c> event
		/// </summary>
		/// <param name="message">Will contain a JSON representation of a <c>UserProfile</c></param>
		public void onUserProfileUpdated(String message)
		{
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onUserProfileUpdated");

			JSONObject eventJson = new JSONObject(message);
			UserProfile userProfile = new UserProfile (eventJson ["userProfile"]);

			ProfileEvents.OnUserProfileUpdated (userProfile);
			//ProfileEvents.OnUserProfileUpdated (new UserProfileUpdatedEvent(userProfile));
		}

		/// <summary>
		/// Handles an <c>onLoginStarted</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c> 
		/// as well as payload </param>
		public void onLoginStarted(String message)
		{
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onLoginStarted");

			JSONObject eventJson = new JSONObject(message);
			Provider provider = Provider.fromInt((int)(eventJson["provider"].n));
			bool autoLogin = eventJson["autoLogin"].b;

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);
			string payload = ProfilePayload.GetUserPayload(payloadJSON);
			ProfileEvents.OnLoginStarted(provider, autoLogin, payload);
			//ProfileEvents.OnLoginStarted(new LoginStartedEvent(provider, autoLogin, payload));
		}

		/// <summary>
		/// Handles an <c>onLoginFinished</c> event
		/// </summary>
		/// <param name="message">Will contain a JSON representation of a <c>UserProfile</c> and payload</param>
		public void onLoginFinished(String message)
		{
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onLoginFinished");

			JSONObject eventJson = new JSONObject(message);

			UserProfile userProfile = new UserProfile (eventJson ["userProfile"]);

			bool autoLogin = eventJson["autoLogin"].b;

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			//give a reward
			Reward reward = Reward.GetReward(ProfilePayload.GetRewardId(payloadJSON));
			if (reward !=null)
				reward.Give();

			ProfileEvents.OnLoginFinished(userProfile, autoLogin, ProfilePayload.GetUserPayload(payloadJSON));
			//ProfileEvents.OnLoginFinished(new LoginFinishedEvent(userProfile, autoLogin, ProfilePayload.GetUserPayload(payloadJSON)));
		}

		/// <summary>
		/// Handles an <c>onLoginCancelled</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c> 
		/// as well as payload </param>
		public void onLoginCancelled(String message)
		{
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onLoginCancelled");

			JSONObject eventJson = new JSONObject(message);

			Provider provider = Provider.fromInt((int)(eventJson["provider"].n));

			bool autoLogin = eventJson["autoLogin"].b;

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			ProfileEvents.OnLoginCancelled (provider, autoLogin, ProfilePayload.GetUserPayload(payloadJSON));
			//ProfileEvents.OnLoginCancelled (new LoginCancelledEvent(provider, autoLogin, ProfilePayload.GetUserPayload(payloadJSON)));
		}

		/// <summary>
		/// Handles an <c>onLoginFailed</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c> 
		/// ,error message and payload </param>
		public void onLoginFailed(String message)
		{
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onLoginFailed");

			JSONObject eventJson = new JSONObject(message);

			Provider provider = Provider.fromInt((int)(eventJson["provider"].n));
			String errorMessage = eventJson["message"].str;

			bool autoLogin = eventJson["autoLogin"].b;

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			ProfileEvents.OnLoginFailed(provider, errorMessage, autoLogin, ProfilePayload.GetUserPayload(payloadJSON));
			//ProfileEvents.OnLoginFailed(new LoginFailedEvent(provider, errorMessage, autoLogin, ProfilePayload.GetUserPayload(payloadJSON)));
		}

		/// <summary>
		/// Handles an <c>onLogoutStarted</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c></param>
		public void onLogoutStarted(String message)
		{
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onLogoutStarted");

			JSONObject eventJson = new JSONObject(message);

			Provider provider = Provider.fromInt ((int)(eventJson["provider"].n));

			ProfileEvents.OnLogoutStarted (provider);
			//ProfileEvents.OnLogoutStarted (new LogoutStartedEvent(provider));
		}

		/// <summary>
		/// Handles an <c>onLogoutFinished</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c></param>
		public void onLogoutFinished(String message)
		{
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onLogoutFinished");

			JSONObject eventJson = new JSONObject(message);
			
			Provider provider = Provider.fromInt ((int)(eventJson["provider"].n));

			ProfileEvents.OnLogoutFinished(provider);
			//ProfileEvents.OnLogoutFinished(new LogoutFinishedEvent(provider));
		}

		/// <summary>
		/// Handles an <c>onLogoutFailed</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c> 
		/// and payload</param>
		public void onLogoutFailed(String message)
		{
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onLogoutFailed");

			JSONObject eventJson = new JSONObject(message);

			Provider provider = Provider.fromInt ((int)(eventJson["provider"].n));
			String errorMessage = eventJson["message"].str;

			ProfileEvents.OnLogoutFailed (provider, errorMessage);
			//ProfileEvents.OnLogoutFailed (new LogoutFailedEvent(provider, errorMessage));
		}

		/// <summary>
		/// Handles an <c>onSocialActionStarted</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c> 
		/// numeric representation of <c>SocialActionType</c> and payload</param>
		public void onSocialActionStarted(String message)
		{
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onSocialActionStarted");

			JSONObject eventJson = new JSONObject(message);

			Provider provider = Provider.fromInt ((int)(eventJson["provider"].n));
			SocialActionType socialAction = SocialActionType.fromInt ((int)eventJson["socialActionType"].n);

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			ProfileEvents.OnSocialActionStarted (provider, socialAction, ProfilePayload.GetUserPayload(payloadJSON));
			//ProfileEvents.OnSocialActionStarted (new SocialActionStartedEvent(provider, socialAction, ProfilePayload.GetUserPayload(payloadJSON)));
		}

		/// <summary>
		/// Handles an <c>onSocialActionFinished</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c> 
		/// numeric representation of <c>SocialActionType</c> and payload</param>
		public void onSocialActionFinished(String message)
		{
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onSocialActionFinished");

			JSONObject eventJson = new JSONObject(message);
			
			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);
			SocialActionType socialAction = SocialActionType.fromInt ((int)eventJson["socialActionType"].n);

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			//give a reward
			Reward reward = Reward.GetReward(ProfilePayload.GetRewardId(payloadJSON));
			if (reward != null)
				reward.Give();

			ProfileEvents.OnSocialActionFinished (provider, socialAction, ProfilePayload.GetUserPayload(payloadJSON));
			//ProfileEvents.OnSocialActionFinished (new SocialActionFinishedEvent(provider, socialAction, ProfilePayload.GetUserPayload(payloadJSON)));
		}

		/// <summary>
		/// Handles an <c>onSocialActionCancelled</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c> 
		/// numeric representation of <c>SocialActionType</c> and payload</param>
		public void onSocialActionCancelled(String message)
		{
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onSocialActionCancelled");
			
			JSONObject eventJson = new JSONObject(message);
			
			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);
			SocialActionType socialAction = SocialActionType.fromInt ((int)eventJson["socialActionType"].n);

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);
			
			ProfileEvents.OnSocialActionCancelled (provider, socialAction, ProfilePayload.GetUserPayload(payloadJSON));
			//ProfileEvents.OnSocialActionCancelled (new SocialActionCancelledEvent(provider, socialAction, ProfilePayload.GetUserPayload(payloadJSON)));
		}

		/// <summary>
		/// Handles an <c>onSocialActionFailed</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c> 
		/// numeric representation of <c>SocialActionType</c>, 
		/// error message and payload</param>
		public void onSocialActionFailed(String message)
		{
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onSocialActionFailed");

			JSONObject eventJson = new JSONObject(message);
			
			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);
			SocialActionType socialAction = SocialActionType.fromInt ((int)eventJson["socialActionType"].n);
			String errorMessage = eventJson["message"].str;

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			ProfileEvents.OnSocialActionFailed (provider, socialAction, errorMessage, ProfilePayload.GetUserPayload(payloadJSON));
			//ProfileEvents.OnSocialActionFailed (new SocialActionFailedEvent(provider, socialAction, errorMessage, ProfilePayload.GetUserPayload(payloadJSON)));
		}

		/// <summary>
		/// Handles an <c>onGetContactsStarted</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c>, 
		/// and payload</param>
		public void onGetContactsStarted(String message)
		{
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGetContactsStarted");

			JSONObject eventJson = new JSONObject(message);

			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			bool fromStart = eventJson["fromStart"].b;

			ProfileEvents.OnGetContactsStarted (provider, fromStart, ProfilePayload.GetUserPayload (payloadJSON));
			//ProfileEvents.OnGetContactsStarted (new GetContactsStartedEvent(provider, fromStart, ProfilePayload.GetUserPayload (payloadJSON)));
		}

		/// <summary>
		/// Handles an <c>onGetContactsFinished</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c>, 
		/// JSON array of <c>UserProfile</c>s and payload</param>
		public void onGetContactsFinished(String message)
		{
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGetContactsFinished");

			JSONObject eventJson = new JSONObject(message);
			
			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);

			bool hasMore = eventJson["hasMore"].b;

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			JSONObject userProfilesArray = eventJson ["contacts"];
			List<UserProfile> userProfiles = new List<UserProfile>();
			foreach (JSONObject userProfileJson in userProfilesArray.list) {
				userProfiles.Add(new UserProfile(userProfileJson));
			}

			SocialPageData<UserProfile> data = new SocialPageData<UserProfile>();
			data.PageData = userProfiles;
			data.PageNumber = 0;
			data.HasMore = hasMore;
				                
			ProfileEvents.OnGetContactsFinished(provider, data, ProfilePayload.GetUserPayload(payloadJSON));
			//ProfileEvents.OnGetContactsFinished(new GetContactsFinishedEvent(provider, data, ProfilePayload.GetUserPayload(payloadJSON)) );
		}

		/// <summary>
		/// Handles an <c>onGetContactsFailed</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c>,
		/// error message payload</param>
		public void onGetContactsFailed(String message)
		{
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGetContactsFailed");

			JSONObject eventJson = new JSONObject(message);
			
			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);
			String errorMessage = eventJson["message"].str;

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			bool fromStart = eventJson["fromStart"].b;

			ProfileEvents.OnGetContactsFailed (provider, errorMessage, fromStart, ProfilePayload.GetUserPayload(payloadJSON));
			//ProfileEvents.OnGetContactsFailed (new GetContactsFailedEvent(provider, errorMessage, fromStart, ProfilePayload.GetUserPayload(payloadJSON)));
		}

		/// <summary>
		/// Handles an <c>onGetFeedStarted</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c>,
		/// and payload</param>
		public void onGetFeedStarted(String message)
		{
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGetFeedStarted");

			JSONObject eventJson = new JSONObject(message);
			
			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);

			ProfileEvents.OnGetFeedStarted (provider);
			//ProfileEvents.OnGetFeedStarted (new GetFeedStartedEvent(provider));
		}

		/// <summary>
		/// Handles an <c>onGetFeedFinished</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c>,
		/// json array of feeds</param>
		public void onGetFeedFinished(String message)
		{
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGetFeedFinished");

			JSONObject eventJson = new JSONObject(message);
			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);
			JSONObject feedsJson = eventJson ["feeds"];
			List<String> feeds = new List<String>();
			foreach (JSONObject feedVal in feedsJson.list) {
				//iterate "feed" keys
				feeds.Add(feedVal.str);
			}

			bool hasMore = eventJson["hasMore"].b;
			
			SocialPageData<String> result = new SocialPageData<String>();
			result.PageData = feeds;
			result.PageNumber = 0;
			result.HasMore = hasMore;
			ProfileEvents.OnGetFeedFinished (provider, result);
			//ProfileEvents.OnGetFeedFinished (new GetFeedFinishedEvent(provider, result));
		}

		/// <summary>
		/// Handles an <c>onGetFeedFailed</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c>,
		/// and an error message</param>
		public void onGetFeedFailed(String message)
		{
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGetFeedFailed");

			JSONObject eventJson = new JSONObject(message);
			
			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);
			String errorMessage = eventJson["message"].str;

			ProfileEvents.OnGetFeedFailed (provider, errorMessage);
			//ProfileEvents.OnGetFeedFailed (new GetFeedFailedEvent(provider, errorMessage));
		}

		/// <summary>
		/// Handles an <c>onInviteStarted</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c> 
		/// numeric representation of <c>SocialActionType</c> and payload</param>
		public void onInviteStarted(String message)
		{
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onInviteStarted");
			
			JSONObject eventJson = new JSONObject(message);
			
			Provider provider = Provider.fromInt ((int)(eventJson["provider"].n));
			
			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);
			
			ProfileEvents.OnInviteStarted (provider, ProfilePayload.GetUserPayload(payloadJSON));
			//ProfileEvents.OnInviteStarted (new InviteStartedEvent(provider, ProfilePayload.GetUserPayload(payloadJSON)));
		}
		
		/// <summary>
		/// Handles an <c>onInviteFinished</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c> 
		/// numeric representation of <c>SocialActionType</c>
		/// id of given request, list of invite recipients
		/// and payload</param>
		public void onInviteFinished(String message)
		{
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onInviteFinished");
			
			JSONObject eventJson = new JSONObject(message);
			
			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);

			String requestId = eventJson["requestId"].str;
			JSONObject recipientsJson = eventJson ["invitedIds"];
			List<String> recipients = new List<String>();
			foreach (JSONObject recipientVal in recipientsJson.list) {
				//iterate "feed" keys
				recipients.Add(recipientVal.str);
			}

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);
			
			//give a reward
			Reward reward = Reward.GetReward(ProfilePayload.GetRewardId(payloadJSON));
			if (reward != null)
				reward.Give();
			
			ProfileEvents.OnInviteFinished (provider, requestId, recipients, ProfilePayload.GetUserPayload(payloadJSON));
			//ProfileEvents.OnInviteFinished (new InviteFinishedEvent(provider, requestId, recipients, ProfilePayload.GetUserPayload(payloadJSON)));
		}
		
		/// <summary>
		/// Handles an <c>onInviteCancelled</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c> 
		/// numeric representation of <c>SocialActionType</c> and payload</param>
		public void onInviteCancelled(String message)
		{
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onInviteCancelled");
			
			JSONObject eventJson = new JSONObject(message);
			
			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);
			
			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);
			
			ProfileEvents.OnInviteCancelled (provider, ProfilePayload.GetUserPayload(payloadJSON));
			//ProfileEvents.OnInviteCancelled (new InviteCancelledEvent(provider, ProfilePayload.GetUserPayload(payloadJSON) ) );
		}
		
		/// <summary>
		/// Handles an <c>onInviteFailed</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c> 
		/// numeric representation of <c>SocialActionType</c>, 
		/// error message and payload</param>
		public void onInviteFailed(String message)
		{
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onInviteFailed");
			
			JSONObject eventJson = new JSONObject(message);
			
			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);
			String errorMessage = eventJson["message"].str;
			
			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);
			
			ProfileEvents.OnInviteFailed (provider, errorMessage, ProfilePayload.GetUserPayload(payloadJSON));
			//ProfileEvents.OnInviteFailed (new InviteFailedEvent(provider, errorMessage, ProfilePayload.GetUserPayload(payloadJSON) ) );
		}

		/// <summary>
		/// Handles an <c>onGetLeaderboardsStarted</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c>,
		/// and payload</param>
		public void onGetLeaderboardsStarted(String message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGetLeaderboardsStarted");

			JSONObject eventJson = new JSONObject(message);

			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);
			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			ProfileEvents.OnGetLeaderboardsStarted(new GetLeaderboardsStartedEvent(provider, ProfilePayload.GetUserPayload(payloadJSON)));
		}

		/// <summary>
		/// Handles an <c>onGetLeaderboardsFinished</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c>,
		/// and payload</param>
		public void onGetLeaderboardsFinished(String message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGetLeaderboardsFinished");

			JSONObject eventJson = new JSONObject(message);

			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);


			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			JSONObject leaderboardsArray = eventJson ["leaderboards"];
			List<Leaderboard> leaderboards = new List<Leaderboard>();
			foreach (JSONObject leaderboardJson in leaderboardsArray.list) {
				leaderboards.Add(new Leaderboard(leaderboardJson));
			}

			SocialPageData<Leaderboard> data = new SocialPageData<Leaderboard>();
			data.PageData = leaderboards;
			data.PageNumber = 0;
			data.HasMore = false;

			ProfileEvents.OnGetLeaderboardsFinished(new GetLeaderboardsFinishedEvent(provider, data, ProfilePayload.GetUserPayload(payloadJSON)));
		}

		/// <summary>
		/// Handles an <c>onGetLeaderboardsFailed</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c>,
		/// and payload</param>
		public void onGetLeaderboardsFailed(String message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGetLeaderboardsFailed");

			JSONObject eventJson = new JSONObject(message);

			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);
			String errorMessage = eventJson["message"].str;

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			ProfileEvents.OnGetLeaderboardsFailed(new GetLeaderboardsFailedEvent(provider, errorMessage, ProfilePayload.GetUserPayload(payloadJSON)));
		}

		/// <summary>
		/// Handles an <c>onGetScoresStarted</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c>,
		/// and payload</param>
		public void onGetScoresStarted(String message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGetScoresStarted");

			JSONObject eventJson = new JSONObject(message);

			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);
			bool fromStart = eventJson["fromStart"].b;
			Leaderboard owner = new Leaderboard(eventJson["leaderboard"]);
			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			ProfileEvents.OnGetScoresStarted(new GetScoresStartedEvent(provider, owner, fromStart, ProfilePayload.GetUserPayload(payloadJSON)));
		}

		/// <summary>
		/// Handles an <c>onGetScoresFinished</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c>,
		/// and payload</param>
		public void onGetScoresFinished(String message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGetScoresFinished");

			JSONObject eventJson = new JSONObject(message);

			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);
			Leaderboard owner = new Leaderboard(eventJson["leaderboard"]);

			bool hasMore = eventJson["hasMore"].b;

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			JSONObject scoresArray = eventJson ["scores"];
			List<Score> scores = new List<Score>();
			foreach (JSONObject scoreJson in scoresArray.list) {
				scores.Add(new Score(scoreJson));
			}

			SocialPageData<Score> data = new SocialPageData<Score>();
			data.PageData = scores;
			data.PageNumber = 0;
			data.HasMore = hasMore;

			ProfileEvents.OnGetScoresFinished(new GetScoresFinishedEvent(provider, owner, data, ProfilePayload.GetUserPayload(payloadJSON)));
		}

		/// <summary>
		/// Handles an <c>onGetScoresFailed</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c>,
		/// and payload</param>
		public void onGetScoresFailed(String message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGetScoresFailed");

			JSONObject eventJson = new JSONObject(message);

			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);
			Leaderboard owner = new Leaderboard(eventJson["leaderboard"]);
			String errorMessage = eventJson["message"].str;

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			bool fromStart = eventJson["fromStart"].b;

			ProfileEvents.OnGetScoresFailed(new GetScoresFailedEvent(provider, owner, fromStart, errorMessage, ProfilePayload.GetUserPayload(payloadJSON)));
		}

		/// <summary>
		/// Handles an <c>onSubmitScoreStarted</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c>,
		/// and payload</param>
		public void onSubmitScoreStarted(String message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onSubmitScoreStarted");

			JSONObject eventJson = new JSONObject(message);

			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);
			Leaderboard owner = new Leaderboard(eventJson["leaderboard"]);
			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			ProfileEvents.OnSubmitScoreStarted(new SubmitScoreStartedEvent(provider, owner, ProfilePayload.GetUserPayload(payloadJSON)));
		}

		/// <summary>
		/// Handles an <c>onSubmitScoreFinished</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c>,
		/// and payload</param>
		public void onSubmitScoreFinished(String message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onSubmitScoreFinished");

			JSONObject eventJson = new JSONObject(message);

			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);
			Leaderboard owner = new Leaderboard(eventJson["leaderboard"]);
			Score score = new Score(eventJson["score"]);

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			ProfileEvents.OnSubmitScoreFinished(new SubmitScoreFinishedEvent(provider, owner, score, ProfilePayload.GetUserPayload(payloadJSON)));
		}

		/// <summary>
		/// Handles an <c>onSubmitScoreFailed</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c>,
		/// and payload</param>
		public void onSubmitScoreFailed(String message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onSubmitScoreFailed");

			JSONObject eventJson = new JSONObject(message);

			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);
			Leaderboard owner = new Leaderboard(eventJson["leaderboard"]);
			String errorMessage = eventJson["message"].str;

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			ProfileEvents.OnSubmitScoreFailed(new SubmitScoreFailedEvent(provider, owner, errorMessage, ProfilePayload.GetUserPayload(payloadJSON)));
		}

		/// <summary>
		/// Handles an <c>onShowLeaderboards</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c>
		/// </param>
		public void onShowLeaderboards(String message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onShowLeaderboards");

			JSONObject eventJson = new JSONObject(message);

			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);
			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			ProfileEvents.OnShowLeaderboards(new ShowLeaderboardsEvent(provider, ProfilePayload.GetUserPayload(payloadJSON)));
		}

		public delegate void Action();
		public delegate void Action<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

		public static Action OnSoomlaProfileInitialized = delegate {};
		//public static Action<ProfileInitializedEvent> OnSoomlaProfileInitialized = delegate {};

		public static Action OnUserRatingEvent =delegate {};
		//public static Action<UserRatingEvent> OnUserRatingEvent =delegate {};

		public static Action<UserProfile> OnUserProfileUpdated = delegate {};
		//public static Action<UserProfileUpdatedEvent> OnUserProfileUpdated = delegate {};

		public static Action<Provider, string, bool, string> OnLoginFailed = delegate {};
		//public static Action<LoginFailedEvent> OnLoginFailed = delegate {};

		public static Action<UserProfile, bool, string> OnLoginFinished = delegate {};
		//public static Action<LoginFinishedEvent> OnLoginFinished = delegate {};

		public static Action<Provider, bool, string> OnLoginStarted = delegate {};
		//public static Action<LoginStartedEvent> OnLoginStarted = delegate {};

		public static Action<Provider, bool, string> OnLoginCancelled = delegate {};
		//public static Action<LoginCancelledEvent> OnLoginCancelled = delegate {};

		public static Action<Provider, string> OnLogoutFailed = delegate {};
		//public static Action<LogoutFailedEvent> OnLogoutFailed = delegate {};

		public static Action<Provider> OnLogoutFinished = delegate {};
		//public static Action<LogoutFinishedEvent> OnLogoutFinished = delegate {};

		public static Action<Provider> OnLogoutStarted = delegate {};
		//public static Action<LogoutStartedEvent> OnLogoutStarted = delegate {};

		public static Action<Provider, SocialActionType, string, string> OnSocialActionFailed = delegate {};
		//public static Action<SocialActionFailedEvent> OnSocialActionFailed = delegate {};

		public static Action<Provider, SocialActionType, string> OnSocialActionFinished = delegate {};
		//public static Action<SocialActionFinishedEvent> OnSocialActionFinished = delegate {};

		public static Action<Provider, SocialActionType, string> OnSocialActionStarted = delegate {};
		//public static Action<SocialActionStartedEvent> OnSocialActionStarted = delegate {};

		public static Action<Provider, SocialActionType, string> OnSocialActionCancelled = delegate {};
		//public static Action<SocialActionCancelledEvent> OnSocialActionCancelled = delegate {};

		public static Action<Provider, string, bool, string> OnGetContactsFailed = delegate {};
		//public static Action<GetContactsFailedEvent> OnGetContactsFailed = delegate {};

		public static Action<Provider, SocialPageData<UserProfile>, string> OnGetContactsFinished = delegate {};
		//public static Action<GetContactsFinishedEvent> OnGetContactsFinished = delegate {};

		public static Action<Provider, bool, string> OnGetContactsStarted = delegate {};
		//public static Action<GetContactsStartedEvent> OnGetContactsStarted = delegate {};

		public static Action<Provider, string> OnGetFeedFailed = delegate {};
		//public static Action<GetFeedFailedEvent> OnGetFeedFailed = delegate {};

		public static Action<Provider, SocialPageData<String>> OnGetFeedFinished = delegate {};
		//public static Action<GetFeedFinishedEvent> OnGetFeedFinished = delegate {};

		public static Action<Provider> OnGetFeedStarted = delegate {};
		//public static Action<GetFeedStartedEvent> OnGetFeedStarted = delegate {};

		public static Action<Provider> OnAddAppRequestStarted = delegate {};
		//public static Action<AddAppRequestStartedEvent> OnAddAppRequestStarted = delegate {};

		public static Action<Provider, string> OnAddAppRequestFinished = delegate {};
		//public static Action<AddAppRequestFinishedEvent> OnAddAppRequestFinished = delegate {};

		public static Action<Provider, string> OnAddAppRequestFailed = delegate {};
		//public static Action<AddAppRequestFailedEvent> OnAddAppRequestFailed = delegate {};

		public static Action<Provider, string> OnInviteStarted = delegate {};
		//public static Action<InviteStartedEvent> OnInviteStarted = delegate {};

		public static Action<Provider, string, List<string>, string> OnInviteFinished = delegate {};
		//public static Action<InviteFinishedEvent> OnInviteFinished = delegate {};

		public static Action<Provider, string, string> OnInviteFailed = delegate {};
		//public static Action<InviteFailedEvent> OnInviteFailed = delegate {};

		public static Action<Provider, string> OnInviteCancelled = delegate {};
		//public static Action<InviteCancelledEvent> OnInviteCancelled = delegate {};

		public static Action<GetLeaderboardsStartedEvent> OnGetLeaderboardsStarted = delegate {};
		public static Action<GetLeaderboardsFinishedEvent> OnGetLeaderboardsFinished = delegate {};
		public static Action<GetLeaderboardsFailedEvent> OnGetLeaderboardsFailed = delegate {};

		public static Action<GetScoresStartedEvent> OnGetScoresStarted = delegate {};
		public static Action<GetScoresFinishedEvent> OnGetScoresFinished = delegate {};
		public static Action<GetScoresFailedEvent> OnGetScoresFailed = delegate {};

		public static Action<SubmitScoreStartedEvent> OnSubmitScoreStarted = delegate {};
		public static Action<SubmitScoreFinishedEvent> OnSubmitScoreFinished = delegate {};
		public static Action<SubmitScoreFailedEvent> OnSubmitScoreFailed = delegate {};

		public static Action<ShowLeaderboardsEvent> OnShowLeaderboards = delegate {};

		public class ProfileEventPusher {

			/// <summary>
			/// Registers all events. 
			/// </summary>
			public ProfileEventPusher() {
                ProfileEvents.OnLoginCancelled += _pushEventLoginCancelled;
                ProfileEvents.OnLoginFailed += _pushEventLoginFailed;
				ProfileEvents.OnLoginFinished += _pushEventLoginFinished;
				ProfileEvents.OnLoginStarted += _pushEventLoginStarted;
				ProfileEvents.OnLogoutFailed += _pushEventLogoutFailed;
				ProfileEvents.OnLogoutFinished += _pushEventLogoutFinished;
				ProfileEvents.OnLogoutStarted += _pushEventLogoutStarted;
				ProfileEvents.OnSocialActionCancelled += _pushEventSocialActionCancelled;
				ProfileEvents.OnSocialActionFailed += _pushEventSocialActionFailed;
				ProfileEvents.OnSocialActionFinished += _pushEventSocialActionFinished;
				ProfileEvents.OnSocialActionStarted += _pushEventSocialActionStarted;
				ProfileEvents.OnGetContactsStarted += _pushEventGetContactsStarted;
				ProfileEvents.OnGetContactsFinished += _pushEventGetContactsFinished;
				ProfileEvents.OnGetContactsFailed += _pushEventGetContactsFailed;
				ProfileEvents.OnInviteStarted += _pushEventInviteStarted;
				ProfileEvents.OnInviteFinished += _pushEventInviteFinished;
				ProfileEvents.OnInviteFailed += _pushEventInviteFailed;
				ProfileEvents.OnInviteCancelled += _pushEventInviteCancelled;
				ProfileEvents.OnGetLeaderboardsStarted += _pushEventGetLeaderboardsStarted;
				ProfileEvents.OnGetLeaderboardsFinished += _pushEventGetLeaderboardsFinished;
				ProfileEvents.OnGetLeaderboardsFailed += _pushEventGetLeaderboardsFailed;
				ProfileEvents.OnGetScoresStarted += _pushEventGetScoresStarted;
				ProfileEvents.OnGetScoresFinished += _pushEventGetScoresFinished;
				ProfileEvents.OnGetScoresFailed += _pushEventGetScoresFailed;
				ProfileEvents.OnSubmitScoreStarted += _pushEventSubmitScoreStarted;
				ProfileEvents.OnSubmitScoreFinished += _pushEventSubmitScoreFinished;
				ProfileEvents.OnSubmitScoreFailed += _pushEventSubmitScoreFailed;
				ProfileEvents.OnShowLeaderboards += _pushEventShowLeaderboards;
			}

			// Event pushing back to native (when using FB Unity SDK)
			/*
			protected virtual void _pushEventLoginStarted(LoginStartedEvent e) {}
			protected virtual void _pushEventLoginFinished(LoginFinishedEvent e){}
			protected virtual void _pushEventLoginFailed(LoginFailedEvent e){}
			protected virtual void _pushEventLoginCancelled(LoginCancelledEvent e){}
			protected virtual void _pushEventLogoutStarted(LogoutStartedEvent e){}
			protected virtual void _pushEventLogoutFinished(LogoutFinishedEvent e){}
			protected virtual void _pushEventLogoutFailed(LogoutFailedEvent e){}
			protected virtual void _pushEventSocialActionStarted(SocialActionStartedEvent e){}
			protected virtual void _pushEventSocialActionFinished(SocialActionFinishedEvent e){}
			protected virtual void _pushEventSocialActionCancelled(SocialActionCancelledEvent e){}
			protected virtual void _pushEventSocialActionFailed(SocialActionFailedEvent e){}
			protected virtual void _pushEventGetContactsStarted(GetContactsStartedEvent e){}
			protected virtual void _pushEventGetContactsFinished(GetContactsFinishedEvent e){}
			protected virtual void _pushEventGetContactsFailed(GetContactsFailedEvent e){}
			protected virtual void _pushEventGetFeedFinished(GetFeedFinishedEvent e) {}
			protected virtual void _pushEventGetFeedFailed(GetFeedFailedEvent e) {}

			protected virtual void _pushEventInviteStarted(InviteStartedEvent e){}
			protected virtual void _pushEventInviteFinished(InviteFinishedEvent e){}
			protected virtual void _pushEventInviteFailed(InviteFailedEvent e){}
			protected virtual void _pushEventInviteCancelled(InviteCancelledEvent e){}
			*/
			protected virtual void _pushEventLoginStarted(Provider provider, bool autoLogin, string payload) {}
			protected virtual void _pushEventLoginFinished(UserProfile userProfileJson, bool autoLogin, string payload){}
			protected virtual void _pushEventLoginFailed(Provider provider, string message, bool autoLogin, string payload){}
			protected virtual void _pushEventLoginCancelled(Provider provider, bool autoLogin, string payload){}
			protected virtual void _pushEventLogoutStarted(Provider provider){}
			protected virtual void _pushEventLogoutFinished(Provider provider){}
			protected virtual void _pushEventLogoutFailed(Provider provider, string message){}
			protected virtual void _pushEventSocialActionStarted(Provider provider, SocialActionType actionType, string payload){}
			protected virtual void _pushEventSocialActionFinished(Provider provider, SocialActionType actionType, string payload){}
			protected virtual void _pushEventSocialActionCancelled(Provider provider, SocialActionType actionType, string payload){}
			protected virtual void _pushEventSocialActionFailed(Provider provider, SocialActionType actionType, string message, string payload){}
			protected virtual void _pushEventGetContactsStarted(Provider provider, bool fromStart, string payload){}
			protected virtual void _pushEventGetContactsFinished(Provider provider, SocialPageData<UserProfile> contactsPage, string payload){}
			protected virtual void _pushEventGetContactsFailed(Provider provider, string message, bool fromStart, string payload){}
			protected virtual void _pushEventGetFeedFinished(Provider provider, SocialPageData<String> feedPage, string payload) {}
			protected virtual void _pushEventGetFeedFailed(Provider provider, string message, bool fromStart, string payload) {}

			protected virtual void _pushEventInviteStarted(Provider provider, string payload){}
			protected virtual void _pushEventInviteFinished(Provider provider, string requestId, List<string> invitedIds, string payload){}
			protected virtual void _pushEventInviteFailed(Provider provider, string message, string payload){}
			protected virtual void _pushEventInviteCancelled(Provider provider, string payload){}

			protected virtual void _pushEventGetLeaderboardsStarted(GetLeaderboardsStartedEvent getLeaderboardsStartedEvent) {}
			protected virtual void _pushEventGetLeaderboardsFinished(GetLeaderboardsFinishedEvent getLeaderboardsFinishedEvent) {}
			protected virtual void _pushEventGetLeaderboardsFailed(GetLeaderboardsFailedEvent getLeaderboardsFailedEvent) {}

			protected virtual void _pushEventGetScoresStarted(GetScoresStartedEvent getScoresStartedEvent) {}
			protected virtual void _pushEventGetScoresFinished(GetScoresFinishedEvent getScoresFinishedEvent) {}
			protected virtual void _pushEventGetScoresFailed(GetScoresFailedEvent getScoresFailedEvent) {}

			protected virtual void _pushEventSubmitScoreStarted(SubmitScoreStartedEvent submitScoreStartedEvent) {}
			protected virtual void _pushEventSubmitScoreFinished(SubmitScoreFinishedEvent submitScoreFinishedEvent) {}
			protected virtual void _pushEventSubmitScoreFailed(SubmitScoreFailedEvent submitScoreFailedEvent) {}

			protected virtual void _pushEventShowLeaderboards(ShowLeaderboardsEvent showLeaderboardsEvent) {}
		}
	}
}