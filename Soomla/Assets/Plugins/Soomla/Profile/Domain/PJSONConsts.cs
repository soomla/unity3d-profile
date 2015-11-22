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

using System;

namespace Soomla.Profile {

	/// <summary>
	/// This class contains all string names of the keys/vals in the JSON being parsed all around the SDK.
	/// </summary>
	public static class PJSONConsts
	{

		/** UserProfile **/

		public const string UP_PROVIDER         = "provider";
		public const string UP_PROFILEID        = "profileId";
		public const string UP_USERNAME         = "username";
		public const string UP_EMAIL            = "email";
		public const string UP_FIRSTNAME        = "firstName";
		public const string UP_LASTNAME         = "lastName";
		public const string UP_AVATAR           = "avatarLink";
		public const string UP_LOCATION         = "location";
		public const string UP_GENDER           = "gender";
		public const string UP_LANGUAGE         = "language";
		public const string UP_BIRTHDAY         = "birthday";
		public const string UP_EXTRA            = "extra";

		/** Game Services **/

		public const string UP_IDENTIFIER = "id";
		public const string UP_LEADERBOARD = @"leaderboard";
		public const string UP_USER_PROFILE = "userProfile";
		public const string UP_SCORE_VALUE = "scoreValue";
		public const string UP_SCORE_RANK = "scoreRank";

		/** Reward **/
		
		public const string BP_REWARDS           = "rewards";
		public const string BP_REWARD_REWARDID   = "rewardId";
		public const string BP_REWARD_AMOUNT     = "amount";
		public const string BP_REWARD_ICONURL    = "iconUrl";
		public const string BP_REWARD_REPEAT     = "repeatable";

		/** Global **/
		
		public const string BP_ASSOCITEMID       = "associatedItemId";
		public const string BP_ASSOCSCOREID      = "associatedScoreId";
		public const string BP_ASSOCWORLDID      = "associatedWorldId";
		public const string BP_DESIRED_RECORD    = "desiredRecord";
		public const string BP_DESIRED_BALANCE   = "desiredBalance";
		public const string BP_NAME              = "name";
		public const string BP_TYPE              = "jsonType";
	}
}

