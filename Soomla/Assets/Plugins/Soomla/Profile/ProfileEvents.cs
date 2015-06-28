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

namespace Soomla.Profile {

	/// <summary>
	/// This class provides functions for event handling. To handle various events, just add your 
	/// game-specific behavior to the delegates below.
	/// </summary>
	public class ProfileEvents : MonoBehaviour {

		private const string TAG = "SOOMLA ProfileEvents";

		private static ProfileEvents instance = null;
		#pragma warning disable 414
		private static ProfileEventPusher pep = null;
		#pragma warning restore 414

		/// <summary>
		/// Initializes the game state before the game starts.
		/// </summary>
		void Awake(){
			if(instance == null){ 	// making sure we only initialize one instance.
				#if DEBUG_SOOMLA
				SoomlaUtils.LogDebug(TAG, "Initializing ProfileEvents (Awake)");
				#endif

				instance = this;
                gameObject.name = "ProfileEvents";
				GameObject.DontDestroyOnLoad(this.gameObject);
				Initialize();
				// now we initialize the event pusher
				#if UNITY_ANDROID && !UNITY_EDITOR
				pep = new ProfileEventPusherAndroid();
				#elif UNITY_IOS && !UNITY_EDITOR
				pep = new ProfileEventPusherIOS();
				#endif

			} else {				// Destroying unused instances.
				GameObject.Destroy(this.gameObject);
			}
		}

		public static void Initialize() {
			#if DEBUG_SOOMLA
			SoomlaUtils.LogDebug (TAG, "Initializing ProfileEvents ...");
			#endif
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

		/// <summary>
		/// Handles an <c>onSoomlaProfileInitialized</c> event, which is fired when SoomlaProfile
		/// has been initialzed
		/// </summary>
		public void onSoomlaProfileInitialized()
		{
			#if DEBUG_SOOMLA
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onSoomlaProfileInitialized");
			#endif

			SoomlaProfile.TryFireProfileInitialized();
		}

		/// <summary>
		/// Handles an <c>onUserRatingEvent</c> event
		/// </summary>
		public void onUserRatingEvent()
		{
			#if DEBUG_SOOMLA
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onUserRatingEvent");
			#endif

			ProfileEvents.OnUserRatingEvent ();
		}

		/// <summary>
		/// Handles an <c>onUserProfileUpdated</c> event
		/// </summary>
		/// <param name="message">Will contain a JSON representation of a <c>UserProfile</c></param>
		public void onUserProfileUpdated(String message)
		{
			#if DEBUG_SOOMLA
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onUserProfileUpdated");
			#endif

			JSONObject eventJson = new JSONObject(message);
			UserProfile userProfile = new UserProfile (eventJson ["userProfile"]);

			ProfileEvents.OnUserProfileUpdated (userProfile);
		}

		/// <summary>
		/// Handles an <c>onLoginStarted</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c> 
		/// as well as payload </param>
		public void onLoginStarted(String message)
		{
			#if DEBUG_SOOMLA
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onLoginStarted");
			#endif

			JSONObject eventJson = new JSONObject(message);
			Provider provider = Provider.fromInt((int)(eventJson["provider"].n));

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);
			ProfileEvents.OnLoginStarted (provider, ProfilePayload.GetUserPayload(payloadJSON));
		}

		/// <summary>
		/// Handles an <c>onLoginFinished</c> event
		/// </summary>
		/// <param name="message">Will contain a JSON representation of a <c>UserProfile</c> and payload</param>
		public void onLoginFinished(String message)
		{
			#if DEBUG_SOOMLA
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onLoginFinished");
			#endif

			JSONObject eventJson = new JSONObject(message);

			UserProfile userProfile = new UserProfile (eventJson ["userProfile"]);

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			//give a reward
			Reward reward = Reward.GetReward(ProfilePayload.GetRewardId(payloadJSON));
			if (reward !=null)
				reward.Give();

			ProfileEvents.OnLoginFinished (userProfile, ProfilePayload.GetUserPayload(payloadJSON));
		}

		/// <summary>
		/// Handles an <c>onLoginCancelled</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c> 
		/// as well as payload </param>
		public void onLoginCancelled(String message)
		{
			#if DEBUG_SOOMLA
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onLoginCancelled");
			#endif

			JSONObject eventJson = new JSONObject(message);

			Provider provider = Provider.fromInt((int)(eventJson["provider"].n));

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			ProfileEvents.OnLoginCancelled (provider, ProfilePayload.GetUserPayload(payloadJSON));
		}

		/// <summary>
		/// Handles an <c>onLoginFailed</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c> 
		/// ,error message and payload </param>
		public void onLoginFailed(String message)
		{
			#if DEBUG_SOOMLA
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onLoginFailed");
			#endif

			JSONObject eventJson = new JSONObject(message);

			Provider provider = Provider.fromInt((int)(eventJson["provider"].n));
			String errorMessage = eventJson["message"].str;

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			ProfileEvents.OnLoginFailed(provider, errorMessage, ProfilePayload.GetUserPayload(payloadJSON));
		}

		/// <summary>
		/// Handles an <c>onLogoutStarted</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c></param>
		public void onLogoutStarted(String message)
		{
			#if DEBUG_SOOMLA
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onLogoutStarted");
			#endif

			JSONObject eventJson = new JSONObject(message);

			Provider provider = Provider.fromInt ((int)(eventJson["provider"].n));

			ProfileEvents.OnLogoutStarted (provider);
		}

		/// <summary>
		/// Handles an <c>onLogoutFinished</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c></param>
		public void onLogoutFinished(String message)
		{
			#if DEBUG_SOOMLA
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onLogoutFinished");
			#endif

			JSONObject eventJson = new JSONObject(message);
			
			Provider provider = Provider.fromInt ((int)(eventJson["provider"].n));

			ProfileEvents.OnLogoutFinished(provider);
		}

		/// <summary>
		/// Handles an <c>onLogoutFailed</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c> 
		/// and payload</param>
		public void onLogoutFailed(String message)
		{
			#if DEBUG_SOOMLA
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onLogoutFailed");
			#endif

			JSONObject eventJson = new JSONObject(message);

			Provider provider = Provider.fromInt ((int)(eventJson["provider"].n));
			String errorMessage = eventJson["message"].str;

			ProfileEvents.OnLogoutFailed (provider, errorMessage);
		}

		/// <summary>
		/// Handles an <c>onSocialActionStarted</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c> 
		/// numeric representation of <c>SocialActionType</c> and payload</param>
		public void onSocialActionStarted(String message)
		{
			#if DEBUG_SOOMLA
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onSocialActionStarted");
			#endif

			JSONObject eventJson = new JSONObject(message);

			Provider provider = Provider.fromInt ((int)(eventJson["provider"].n));
			SocialActionType socialAction = SocialActionType.fromInt ((int)eventJson["socialActionType"].n);

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			ProfileEvents.OnSocialActionStarted (provider, socialAction, ProfilePayload.GetUserPayload(payloadJSON));
		}

		/// <summary>
		/// Handles an <c>onSocialActionFinished</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c> 
		/// numeric representation of <c>SocialActionType</c> and payload</param>
		public void onSocialActionFinished(String message)
		{
			#if DEBUG_SOOMLA
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onSocialActionFinished");
			#endif

			JSONObject eventJson = new JSONObject(message);
			
			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);
			SocialActionType socialAction = SocialActionType.fromInt ((int)eventJson["socialActionType"].n);

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			//give a reward
			Reward reward = Reward.GetReward(ProfilePayload.GetRewardId(payloadJSON));
			if (reward != null)
				reward.Give();

			ProfileEvents.OnSocialActionFinished (provider, socialAction, ProfilePayload.GetUserPayload(payloadJSON));
		}

		/// <summary>
		/// Handles an <c>onSocialActionCancelled</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c> 
		/// numeric representation of <c>SocialActionType</c> and payload</param>
		public void onSocialActionCancelled(String message)
		{
			#if DEBUG_SOOMLA
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onSocialActionCancelled");
			#endif
			
			JSONObject eventJson = new JSONObject(message);
			
			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);
			SocialActionType socialAction = SocialActionType.fromInt ((int)eventJson["socialActionType"].n);

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);
			
			ProfileEvents.OnSocialActionCancelled (provider, socialAction, ProfilePayload.GetUserPayload(payloadJSON));
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
			#if DEBUG_SOOMLA
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onSocialActionFailed");
			#endif

			JSONObject eventJson = new JSONObject(message);
			
			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);
			SocialActionType socialAction = SocialActionType.fromInt ((int)eventJson["socialActionType"].n);
			String errorMessage = eventJson["message"].str;

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			ProfileEvents.OnSocialActionFailed (provider, socialAction, errorMessage, ProfilePayload.GetUserPayload(payloadJSON));
		}

		/// <summary>
		/// Handles an <c>onGetContactsStarted</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c>, 
		/// and payload</param>
		public void onGetContactsStarted(String message)
		{
			#if DEBUG_SOOMLA
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGetContactsStarted");
			#endif

			JSONObject eventJson = new JSONObject(message);

			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			bool fromStart = eventJson["fromStart"].b;

			ProfileEvents.OnGetContactsStarted (provider, fromStart, ProfilePayload.GetUserPayload (payloadJSON));
		}

		/// <summary>
		/// Handles an <c>onGetContactsFinished</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c>, 
		/// JSON array of <c>UserProfile</c>s and payload</param>
		public void onGetContactsFinished(String message)
		{
			#if DEBUG_SOOMLA
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGetContactsFinished");
			#endif

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
		}

		/// <summary>
		/// Handles an <c>onGetContactsFinished</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c>,
		/// error message payload</param>
		public void onGetContactsFailed(String message)
		{
			#if DEBUG_SOOMLA
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGetContactsFailed");
			#endif

			JSONObject eventJson = new JSONObject(message);
			
			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);
			String errorMessage = eventJson["message"].str;

			JSONObject payloadJSON = new JSONObject(eventJson ["payload"].str);

			bool fromStart = eventJson["fromStart"].b;

			ProfileEvents.OnGetContactsFailed (provider, errorMessage, fromStart, ProfilePayload.GetUserPayload(payloadJSON));
		}

		/// <summary>
		/// Handles an <c>onGetFeedStarted</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c>,
		/// and payload</param>
		public void onGetFeedStarted(String message)
		{
			#if DEBUG_SOOMLA
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGetFeedStarted");
			#endif

			JSONObject eventJson = new JSONObject(message);
			
			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);

			ProfileEvents.OnGetFeedStarted (provider);
		}

		/// <summary>
		/// Handles an <c>onGetFeedFinished</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c>,
		/// json array of feeds</param>
		public void onGetFeedFinished(String message)
		{
			#if DEBUG_SOOMLA
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGetFeedFinished");
			#endif
			
			JSONObject eventJson = new JSONObject(message);
			
			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);

			JSONObject feedsJson = eventJson ["feeds"];
			List<String> feeds = new List<String>();
			foreach (String key in feedsJson.keys) {
				//iterate "feed" keys
				feeds.Add(feedsJson[key].str);
			}

			ProfileEvents.OnGetFeedFinished (provider, feeds);
		}

		/// <summary>
		/// Handles an <c>onGetFeedFailed</c> event
		/// </summary>
		/// <param name="message">
		/// Will contain a numeric representation of <c>Provider</c>,
		/// and an error message</param>
		public void onGetFeedFailed(String message)
		{
			#if DEBUG_SOOMLA
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGetFeedFailed");
			#endif

			JSONObject eventJson = new JSONObject(message);
			
			Provider provider = Provider.fromInt ((int)eventJson["provider"].n);
			String errorMessage = eventJson["message"].str;

			ProfileEvents.OnGetFeedFailed (provider, errorMessage);
		}

		public delegate void Action();

		public static Action OnSoomlaProfileInitialized = delegate {};

		public static Action OnUserRatingEvent =delegate {};

		public static Action<Provider, string> OnLoginCancelled = delegate {};

		public static Action<UserProfile> OnUserProfileUpdated = delegate {};

		public static Action<Provider, string, string> OnLoginFailed = delegate {};

		public static Action<UserProfile, string> OnLoginFinished = delegate {};

		public static Action<Provider, string> OnLoginStarted = delegate {};

		public static Action<Provider, string> OnLogoutFailed = delegate {};
		
		public static Action<Provider> OnLogoutFinished = delegate {}; 

		public static Action<Provider> OnLogoutStarted = delegate {};

		public static Action<Provider, SocialActionType, string, string> OnSocialActionFailed = delegate {};

		public static Action<Provider, SocialActionType, string> OnSocialActionFinished = delegate {};

		public static Action<Provider, SocialActionType, string> OnSocialActionStarted = delegate {};

		public static Action<Provider, SocialActionType, string> OnSocialActionCancelled = delegate {};

		public static Action<Provider, string, bool, string> OnGetContactsFailed = delegate {};
		
		public static Action<Provider, SocialPageData<UserProfile>, string> OnGetContactsFinished = delegate {};
		
		public static Action<Provider, bool, string> OnGetContactsStarted = delegate {};

		public static Action<Provider, string> OnGetFeedFailed = delegate {};
		
		public static Action<Provider, List<string>> OnGetFeedFinished = delegate {};
		
		public static Action<Provider> OnGetFeedStarted = delegate {};

		public static Action<Provider> OnAddAppRequestStarted = delegate {};

		public static Action<Provider, string> OnAddAppRequestFinished = delegate {};

		public static Action<Provider, string> OnAddAppRequestFailed = delegate {};

		public static Action<Provider, string> OnInviteStarted = delegate {};

		public static Action<Provider, string, List<string>, string> OnInviteFinished = delegate {};

		public static Action<Provider, string, string> OnInviteFailed = delegate {};

		public static Action<Provider, string> OnInviteCancelled = delegate {};

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
			}

			// Event pushing back to native (when using FB Unity SDK)
			protected virtual void _pushEventLoginStarted(Provider provider, string payload) {}
			protected virtual void _pushEventLoginFinished(UserProfile userProfileJson, string payload){}
			protected virtual void _pushEventLoginFailed(Provider provider, string message, string payload){}
			protected virtual void _pushEventLoginCancelled(Provider provider, string payload){}
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
			protected virtual void _pushEventInviteStarted(Provider provider, string payload){}
			protected virtual void _pushEventInviteFinished(Provider provider, string requestId, List<string> invitedIds, string payload){}
			protected virtual void _pushEventInviteFailed(Provider provider, string message, string payload){}
			protected virtual void _pushEventInviteCancelled(Provider provider, string payload){}
		}
	}
}
