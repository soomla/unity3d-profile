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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Soomla.Profile {

	//TODO: add invite push
	public class ProfileEventPusherIOS : Soomla.Profile.ProfileEvents.ProfileEventPusher {
#if UNITY_IOS && !UNITY_EDITOR

		// event pushing back to native (when using FB Unity SDK)
		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventLoginStarted(string provider, bool autoLogin, string payload);
		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventLoginFinished(string userProfileJson, bool autoLogin, string payload);
		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventLoginFailed(string provider, string message, bool autoLogin, string payload);
		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventLoginCancelled(string provider, bool autoLogin, string payload);
		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventLogoutStarted(string provider);
		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventLogoutFinished(string provider);
		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventLogoutFailed(string provider, string message);
		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventSocialActionStarted(string provider, string actionType, string payload);
		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventSocialActionFinished(string provider, string actionType, string payload);
		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventSocialActionCancelled(string provider, string actionType, string payload);
		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventSocialActionFailed(string provider, string actionType, string message, string payload);
		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventGetContactsStarted(string provider, bool fromStart, string payload);
		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventGetContactsFinished(string provider, string userProfilesJson, string payload, bool hasNext);
		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventGetContactsFailed(string provider, string message, bool fromStart, string payload);
		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventGetFeedFinished(string provider, string feedJson, string payload, bool hasNext);
		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventGetFeedFailed(string provider, string message, bool fromStart, string payload);
		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventInviteStarted(string provider, string actionType, string payload);
		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventInviteFinished(string provider, string actionType, string requestId, string invitedIdsJson, string payload);
		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventInviteCancelled(string provider, string actionType, string payload);
		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventInviteFailed(string provider, string actionType, string message, string payload);

		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventGetLeaderboardsStarted(string provider, bool fromStart, string payload);
		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventGetLeaderboardsFinished(string provider, string leaderboardsJson, string payload, bool hasNext);
		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventGetLeaderboardsFailed(string provider, string message, bool fromStart, string payload);

		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventGetScoresStarted(string provider, string fromJson, bool fromStart, string payload);
		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventGetScoresFinished(string provider, string fromJson, string scoresJson, string payload, bool hasNext);
		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventGetScoresFailed(string provider, string fromJson, string message, bool fromStart, string payload);

		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventReportScoreStarted(string provider, string fromJson, bool fromStart, string payload);
		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventReportScoreFinished(string provider, string fromJson, string scoreJson, string payload);
		[DllImport ("__Internal")]
		private static extern void soomlaProfile_PushEventReportScoreFailed(string provider, string fromJson, string message, bool fromStart, string payload);

		// event pushing back to native (when using FB Unity SDK)
		protected override void _pushEventLoginStarted(Provider provider, bool autoLogin, string payload) {
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			soomlaProfile_PushEventLoginStarted(provider.ToString(), autoLogin, payload);
		}

		protected override void _pushEventLoginFinished(UserProfile userProfile, bool autoLogin, string payload) { 
			if (SoomlaProfile.IsProviderNativelyImplemented(userProfile.Provider)) return;
			soomlaProfile_PushEventLoginFinished(userProfile.toJSONObject().print(), autoLogin, payload);
		}
		protected override void _pushEventLoginFailed(Provider provider, string message, bool autoLogin, string payload) {
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			soomlaProfile_PushEventLoginFailed(provider.ToString(), message, autoLogin, payload);
		}
		protected override void _pushEventLoginCancelled(Provider provider, bool autoLogin, string payload) { 
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			soomlaProfile_PushEventLoginCancelled(provider.ToString(), autoLogin, payload);
		}
		protected override void _pushEventLogoutStarted(Provider provider) { 
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			soomlaProfile_PushEventLogoutStarted(provider.ToString());
		}
		protected override void _pushEventLogoutFinished(Provider provider) { 
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			soomlaProfile_PushEventLogoutFinished(provider.ToString());
		}
		protected override void _pushEventLogoutFailed(Provider provider, string message) {
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			soomlaProfile_PushEventLogoutFailed(provider.ToString(), message);
		}
		protected override void _pushEventSocialActionStarted(Provider provider, SocialActionType actionType, string payload) { 
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			soomlaProfile_PushEventSocialActionStarted(provider.ToString(), actionType.ToString(), payload);
		}
		protected override void _pushEventSocialActionFinished(Provider provider, SocialActionType actionType, string payload) {
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			soomlaProfile_PushEventSocialActionFinished(provider.ToString(), actionType.ToString(), payload);
		}
		protected override void _pushEventSocialActionCancelled(Provider provider, SocialActionType actionType, string payload) {
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			soomlaProfile_PushEventSocialActionCancelled(provider.ToString(), actionType.ToString(), payload);
		}
		protected override void _pushEventSocialActionFailed(Provider provider, SocialActionType actionType, string message, string payload) { 
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			soomlaProfile_PushEventSocialActionFailed(provider.ToString(), actionType.ToString(), message, payload);
		}
		protected override void _pushEventGetContactsStarted (Provider provider, bool fromStart, string payload) {
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			soomlaProfile_PushEventGetContactsStarted(provider.ToString(), fromStart, payload);
		}
		protected override void _pushEventGetContactsFinished (Provider provider, SocialPageData<UserProfile> contactsPage, string payload) {
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			List<JSONObject> profiles = new List<JSONObject>();
			foreach (var profile in contactsPage.PageData) {
				profiles.Add(profile.toJSONObject());
			}
			JSONObject contacts = new JSONObject(profiles.ToArray());

			soomlaProfile_PushEventGetContactsFinished(provider.ToString(), contacts.ToString(), payload, contactsPage.HasMore);
		}
		protected override void _pushEventGetContactsFailed (Provider provider, string message, bool fromStart, string payload) {
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			soomlaProfile_PushEventGetContactsFailed(provider.ToString(), message, fromStart, payload);
		}
		protected override void _pushEventGetFeedFinished(Provider provider ,SocialPageData<String> feedPage, string payload) {
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			List<JSONObject> feeds = new List<JSONObject>();
			foreach (var feed in feedPage.PageData) {
				feeds.Add(JSONObject.StringObject(feed));
			}
			JSONObject jsonFeeds = new JSONObject(feeds.ToArray());
			soomlaProfile_PushEventGetFeedFinished(provider.ToString(), jsonFeeds.ToString(), payload, feedPage.HasMore);
		}
		protected override void _pushEventGetFeedFailed(Provider provider, string message, bool fromStart, string payload) {
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			soomlaProfile_PushEventGetFeedFailed(provider.ToString(), message, fromStart, payload);
		}

		protected override void _pushEventInviteStarted(Provider provider, string payload) { 
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			soomlaProfile_PushEventInviteStarted(provider.ToString(), SocialActionType.INVITE.ToString(), payload);
		}
		protected override void _pushEventInviteFinished(Provider provider, string requestId, List<string> invitedIds, string payload) {
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			List<JSONObject> invited = new List<JSONObject>();
			foreach (var id in invitedIds) {
				invited.Add(JSONObject.StringObject(id));
			}
			JSONObject jsonInvited = new JSONObject(invited.ToArray());
			soomlaProfile_PushEventInviteFinished(provider.ToString(), SocialActionType.INVITE.ToString(), requestId, jsonInvited.ToString(), payload);
		}
		protected override void _pushEventInviteCancelled(Provider provider, string payload) {
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			soomlaProfile_PushEventInviteCancelled(provider.ToString(), SocialActionType.INVITE.ToString(), payload);
		}
		protected override void _pushEventInviteFailed(Provider provider, string message, string payload) { 
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			soomlaProfile_PushEventInviteFailed(provider.ToString(), SocialActionType.INVITE.ToString(), message, payload);
		}

		protected override void _pushEventGetLeaderboardsStarted(Soomla.Profile.Provider provider, bool fromStart, string payload) {
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			soomlaProfile_PushEventGetLeaderboardsStarted(provider.ToString(), fromStart, payload);
		}

		protected override void _pushEventGetLeaderboardsFinished(Soomla.Profile.Provider provider, Soomla.Profile.SocialPageData<Soomla.Profile.Leaderboard> leaderboards, string payload) {
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			List<JSONObject> leaderboardList = new List<JSONObject>();
			foreach (var lb in leaderboards.PageData) {
				leaderboardList.Add(lb.toJSONObject());
			}
			JSONObject jsonLbs = new JSONObject(leaderboardList.ToArray());
			soomlaProfile_PushEventGetLeaderboardsFinished(provider.ToString(), jsonLbs.ToString(), payload, leaderboards.HasMore);
		}

		protected override void _pushEventGetLeaderboardsFailed(Soomla.Profile.Provider provider, string message, bool fromStart, string payload) {
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			soomlaProfile_PushEventGetLeaderboardsFailed(provider.ToString(), message, fromStart, payload);
		}

		protected override void _pushEventGetScoresStarted(Soomla.Profile.Provider provider, Soomla.Profile.Leaderboard from, bool fromStart, string payload) {
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			soomlaProfile_PushEventGetScoresStarted(provider.ToString(), from.toJSONObject().ToString(), fromStart, payload);
		}

		protected override void _pushEventGetScoresFinished(Soomla.Profile.Provider provider, Soomla.Profile.Leaderboard from, Soomla.Profile.SocialPageData<Soomla.Profile.Score> scores, string payload) {
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			List<JSONObject> scoreList = new List<JSONObject>();
			foreach (var sc in scores.PageData) {
				scoreList.Add(sc.toJSONObject());
			}
			JSONObject jsonSc = new JSONObject(scoreList.ToArray());
			soomlaProfile_PushEventGetScoresFinished(provider.ToString(), from.toJSONObject().ToString(), jsonSc.ToString(), payload, scores.HasMore);
		}

		protected override void _pushEventGetScoresFailed(Soomla.Profile.Provider provider, Soomla.Profile.Leaderboard from, string message, bool fromStart, string payload) {
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			soomlaProfile_PushEventGetScoresFailed(provider.ToString(), from.toJSONObject().ToString(), message, fromStart, payload);
		}

		protected override void _pushEventReportScoreStarted(Soomla.Profile.Provider provider, Soomla.Profile.Leaderboard owner, bool fromStart, string payload) {
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			soomlaProfile_PushEventReportScoreStarted(provider.ToString(), owner.toJSONObject().ToString(), fromStart, payload);
		}

		protected override void _pushEventReportScoreFinished(Soomla.Profile.Provider provider, Soomla.Profile.Leaderboard owner, Soomla.Profile.Score score, string payload) {
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			soomlaProfile_PushEventReportScoreFinished(provider.ToString(), owner.toJSONObject().ToString(), score.toJSONObject().ToString(), payload);
		}

		protected override void _pushEventReportScoreFailed(Soomla.Profile.Provider provider, Soomla.Profile.Leaderboard owner, string message, bool fromStart, string payload) {
			if (SoomlaProfile.IsProviderNativelyImplemented(provider)) return;
			soomlaProfile_PushEventReportScoreFailed(provider.ToString(), owner.toJSONObject().ToString(), message, fromStart, payload);
		}

#endif
	}
}
