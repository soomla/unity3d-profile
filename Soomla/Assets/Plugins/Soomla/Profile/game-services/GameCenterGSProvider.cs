/// Copyright (C) 2012-2015 Soomla Inc.
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
using System;
using UnityEngine;
using System.Collections.Generic;

namespace Soomla.Profile
{
#if UNITY_IOS || UNITY_EDITOR
	public class GameCenterGSProvider : GameServicesProvider
	{
		
		public GameCenterGSProvider () {
			SoomlaProfile.ProviderBecameReady(this);
		}

		/// <summary>
		/// See docs in <see cref="SoomlaProfile.Logout"/>
		/// </summary>
		public override void Logout(LogoutSuccess success, LogoutFailed fail) {}

		/// <summary>
		/// See docs in <see cref="SoomlaProfile.Login"/>
		/// </summary>
		public override void Login(LoginSuccess success, LoginFailed fail, LoginCancelled cancel) {}

		/// <summary>
		/// See docs in <see cref="SoomlaProfile.GetUserProfile"/>
		/// </summary>
		public override void GetUserProfile(GetUserProfileSuccess success, GetUserProfileFailed fail) {}

		/// <summary>
		/// See docs in <see cref="SoomlaProfile.IsLoggedIn"/>
		/// </summary>
		public override bool IsLoggedIn() {return false;}

		/// <summary>
		/// See docs in <see cref="SocialProvider.IsAutoLogin"/>
		/// </summary>
		/// <returns>value of autoLogin
		public override bool IsAutoLogin() {
			return false;
		}

		/// <summary>
		/// See docs in <see cref="SoomlaProfile.GetContacts"/>
		/// </summary>
		public override void GetContacts(bool fromStart, SocialPageDataSuccess<UserProfile> success, FailureHandler fail) {}

		public override void GetLeaderboards(bool fromStart, SocialPageDataSuccess<Leaderboard> success, FailureHandler fail) {}

		public override void GetScores(Leaderboard owner, bool fromStart, SocialPageDataSuccess<Score> success, FailureHandler fail) {}

		public override void ReportScore(Leaderboard owner, int value, SingleObjectSuccess<Score> success, FailureHandler fail) {}

		public override bool IsNativelyImplemented() {
				return true;
		}
	}
#endif
}


