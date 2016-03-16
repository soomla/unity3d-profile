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

#if SOOMLA_FACEBOOK

using System;
using UnityEngine;
using System.Collections.Generic;
using Facebook.MiniJSON;
using Facebook.Unity;

namespace Soomla.Profile
{
	/// <summary>
	/// This class represents the social provider Facebook. The functions implemented below are 
	/// Facebook-specific. 
	/// </summary>
	public partial class FBSocialProvider : IGameServicesProvider
	{
		/// <summary>
		/// See docs in <see cref="SoomlaProfile.GetLeaderboards"/>
		/// </summary>
		public void GetLeaderboards(SocialPageDataSuccess<Leaderboard> success, FailureHandler fail) {
			var leaderboardJson = new JSONObject();
			leaderboardJson.AddField(PJSONConsts.UP_IDENTIFIER, "main");
			leaderboardJson.AddField(PJSONConsts.UP_PROVIDER, Provider.FACEBOOK.ToString());
			var leaderboard = new Leaderboard(leaderboardJson);

			var pageData = new SocialPageData<Leaderboard>();
			pageData.HasMore = false;
			pageData.PageNumber = 1;
			pageData.PageData = new List<Leaderboard>() {leaderboard};
			success(pageData);
		}

		/// <summary>
		/// See docs in <see cref="SoomlaProfile.GetScores"/>
		/// </summary>
		public void GetScores(Leaderboard owner, bool fromStart, SocialPageDataSuccess<Score> success, FailureHandler fail) {
			FB.API("/app/scores?fields=score,user",
				HttpMethod.GET,
				(IGraphResult result) => {
					if (result.Error != null) {
						SoomlaUtils.LogDebug(TAG, "GetScoreCallback[result.Error]: " + result.Error);
						fail(result.Error);
					}
					else {
						SoomlaUtils.LogDebug(TAG, "GetScoreCallback[result.Text]: " + result.RawResult);
						JSONObject jsonFeed = new JSONObject(result.RawResult);

						SocialPageData<Score> resultData = new SocialPageData<Score>(); 
						resultData.PageData = ScoreFromFBJsonObjs(owner, jsonFeed["data"].list);
						resultData.PageNumber = 1;
						resultData.HasMore = false;

						success(resultData);
					}
				});
		}

		/// <summary>
		/// See docs in <see cref="SoomlaProfile.SubmitScore"/>
		/// </summary>
		public void SubmitScore(Leaderboard targetLeaderboard, int value, SingleObjectSuccess<Score> success, FailureHandler fail) {
			checkPublishPermission(() => {

				var formData = new Dictionary<string, string>
				{
					{ "score", value.ToString() }
				};

				FB.API("/me/scores",
					HttpMethod.POST,
					(IGraphResult result) => {
						if (result.Error != null) {
							SoomlaUtils.LogDebug(TAG, "SubmitScoreCallback[result.Error]: " + result.Error);
							fail(result.Error);
						}
						else {
							SoomlaUtils.LogDebug(TAG, "SubmitScoreCallback[result.Text]: " + result.RawResult);

							JSONObject jsonFeed = new JSONObject(result.RawResult);
							if (jsonFeed["success"].b) {

								var userJson = new JSONObject();
								userJson.AddField(PJSONConsts.UP_USERNAME, "me");
								userJson.AddField(PJSONConsts.UP_PROVIDER, Provider.FACEBOOK.ToString());
								userJson.AddField(PJSONConsts.UP_PROFILEID, "0");

								var scoreJsonObj = new JSONObject();
								scoreJsonObj.AddField(PJSONConsts.UP_LEADERBOARD, targetLeaderboard.toJSONObject());
								scoreJsonObj.AddField(PJSONConsts.UP_USER_PROFILE, userJson);
								scoreJsonObj.AddField(PJSONConsts.UP_SCORE_RANK, 0);
								scoreJsonObj.AddField(PJSONConsts.UP_SCORE_VALUE, value);

								var score = new Score(scoreJsonObj);

								success(score);
							} else {
								fail("Unable to submit score");
							}
						}
					}, formData);
			}, (string errorMessage) => {
				fail(errorMessage);
			});
		}

		/// <summary>
		/// See docs in <see cref="SoomlaProfile.ShowLeaderboards"/>
		/// </summary>
		public void ShowLeaderboards() {
			SoomlaUtils.LogError(TAG, "Can't show leaderboards from facebook");
		}

		/** PRIVATE FUNCTIONS **/

		/// <summary>
		/// JSON object from Facebok Score record, which includes only name and profile ID
		/// </summary>
		private static JSONObject ScoreUserFromFBJsonObj(JSONObject fbUser) {
			var json = new JSONObject();
			json.AddField(PJSONConsts.UP_PROVIDER, Provider.FACEBOOK.ToString());
			json.AddField(PJSONConsts.UP_USERNAME, fbUser["name"].str);
			json.AddField(PJSONConsts.UP_PROFILEID, fbUser["id"].str);
			return json;
		}

		/// <summary>
		/// Parses scores from FB json objects.
		/// </summary>
		private static List<Score> ScoreFromFBJsonObjs(Leaderboard leaderboard, List<JSONObject> fbScoreObjects) {
			List<Score> scores = new List<Score>();
			var leaderboardJson = leaderboard.toJSONObject();
			var previousScore = -1;
			var rank = 1;
			foreach (JSONObject scoreObj in fbScoreObjects) {
				if (scoreObj["score"] != null) {
					var user = ScoreUserFromFBJsonObj(scoreObj["user"]);
					var scoreValue = (long)(scoreObj["score"].n);
					var scoreJSON = new JSONObject();
					scoreJSON.AddField(PJSONConsts.UP_LEADERBOARD, leaderboardJson);
					scoreJSON.AddField(PJSONConsts.UP_USER_PROFILE, user);
					scoreJSON.AddField(PJSONConsts.UP_SCORE_RANK, rank);
					scoreJSON.AddField(PJSONConsts.UP_SCORE_VALUE, scoreValue);
					scores.Add(new Score(scoreJSON));

					if (scoreValue > previousScore) {
						rank++;
					}
				}	
			}
			return scores;
		}
	}
}

#endif
