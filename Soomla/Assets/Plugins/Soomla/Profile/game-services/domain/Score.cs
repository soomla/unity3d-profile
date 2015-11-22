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

using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

namespace Soomla.Profile {

	/// <summary>
	/// This class holds information about the user for a specific <c>Provider</c>.
	/// </summary>
	public class Score : SoomlaEntity<Score> {

		private const string TAG = "SOOMLA UserProfile";

		public Leaderboard Leaderboard;
		public UserProfile Player;
		public Int64 Rank;
		public Int64 Value;

		public Score(JSONObject jsonSC) : base(jsonSC) {
			this.Leaderboard = new Leaderboard(jsonSC[PJSONConsts.UP_LEADERBOARD].obj);
			this.Player = new UserProfile(jsonSC[PJSONConsts.UP_USER_PROFILE].obj);
			this.Rank = jsonSC[PJSONConsts.UP_SCORE_RANK].n;
			this.Value = jsonSC[PJSONConsts.UP_SCORE_VALUE].n;
		}

		public override JSONObject toJSONObject() {
			JSONObject obj = base.toJSONObject();
			obj.AddField(PJSONConsts.UP_LEADERBOARD, this.Leaderboard.toJSONObject());
			obj.AddField(PJSONConsts.UP_USER_PROFILE, this.Player.toJSONObject());
			obj.AddField(PJSONConsts.UP_SCORE_RANK, this.Rank);
			obj.AddField(PJSONConsts.UP_SCORE_VALUE, this.Value);
		}
	}
}

