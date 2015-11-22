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

	/// <summary>
	/// This class represents a specific game services provider (for example, GameCenter, GPGS, etc).
	/// Each game services provider needs to implement the functions in this class.
	/// </summary>
	public abstract class GameServicesProvider : AuthProvider
	{
		public delegate void FailureHandler(string message);
		public delegate void SocialPageDataSuccess<T>(SocialPageData<T> contactsData);
		public delegate void SingleObjectSuccess<T>(T result);

		/// <summary>
		/// See docs in <see cref="SoomlaProfile.GetContacts"/>
		/// </summary>
		public abstract void GetContacts(bool fromStart, SocialPageDataSuccess<UserProfile> success, FailureHandler fail);

		public abstract void GetLeaderboards(bool fromStart, SocialPageDataSuccess<Leaderboard> success, FailureHandler fail);

		public abstract void GetScores(Leaderboard owner, bool fromStart, SocialPageDataSuccess<Score> success, FailureHandler fail);

		public abstract void ReportScore(Leaderboard owner, int value, SingleObjectSuccess<Score> success, FailureHandler fail);


		/// <summary>
		/// The place, where you can configure the provider, using params passed by user.
		/// It's relevant for non-native providers only.
		/// </summary>
		/// <param name="providerParams">Params of this provider, passed during Profile initialization.</param>
		public virtual void Configure(Dictionary<string, string> providerParams) {}

		// TODO: irrelevant for now. Will be updated soon.
		//		public abstract void GetFeed(FeedSuccess success, FeedFailed fail);

		public abstract bool IsNativelyImplemented();
	}
}

