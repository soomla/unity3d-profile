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
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

namespace Soomla.Profile {

	/// <summary>
	/// This class holds information about the user for a specific <c>Provider</c>.
	/// </summary>
	public class UserProfile : SoomlaEntity<UserProfile> {

		private const string TAG = "SOOMLA UserProfile";

		/// <summary>
		/// The provider that this user profile belongs to, such as Facebook, Twitter, etc.
		/// </summary>
		public Provider Provider;

		/** User profile information **/
		public string ProfileId;
		public string Email;
		public string Username;
		public string FirstName;
		public string LastName;
		public string AvatarLink;
		public string Location;
		public string Gender;
		public string Language;
		public string Birthday;
		public readonly Dictionary<String, JSONObject> Extra;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="provider">The provider this <c>UserProfile</c> belongs to.</param>
		/// <param name="profileId">A unique ID that identifies the current user with the provider.</param>
		/// <param name="username">The username of the current user in the provider.</param>
		/// <param name="extra">Additional info provided by SN.</param>
		protected UserProfile(Provider provider, string profileId, string username, Dictionary<String, JSONObject> extra)
		{
			this.Provider = provider;
			this.ProfileId = profileId;
			this.Username = username;
			this.Extra = extra;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="provider">The provider this <c>UserProfile</c> belongs to.</param>
		/// <param name="profileId">A unique ID that identifies the current user with the provider.</param>
		/// <param name="username">The username of the current user in the provider.</param>
		protected UserProfile(Provider provider, string profileId, string username) 
			: this(provider, profileId, username, new Dictionary<String, JSONObject>())
		{
		}

		/// <summary>
		/// Constructor.
		/// Generates an instance of <c>UserProfile</c> from the given <c>JSONObject</c>.
		/// </summary>
		/// <param name="jsonUP">A JSONObject representation of the wanted <c>UserProfile</c>.</param>
		public UserProfile(JSONObject jsonUP) : base(jsonUP) {
			this.Provider = Provider.fromString(jsonUP[PJSONConsts.UP_PROVIDER].str);
			this.Username = JSONObject.DecodeJsString(jsonUP[PJSONConsts.UP_USERNAME].str);
			this.ProfileId = JSONObject.DecodeJsString(jsonUP[PJSONConsts.UP_PROFILEID].str);

			if (jsonUP[PJSONConsts.UP_FIRSTNAME] != null && jsonUP[PJSONConsts.UP_FIRSTNAME].type == JSONObject.Type.STRING) {
				this.FirstName = JSONObject.DecodeJsString(jsonUP[PJSONConsts.UP_FIRSTNAME].str);
			} else {
				this.FirstName = "";
			}
			if (jsonUP[PJSONConsts.UP_LASTNAME] != null && jsonUP[PJSONConsts.UP_LASTNAME].type == JSONObject.Type.STRING) {
				this.LastName = JSONObject.DecodeJsString(jsonUP[PJSONConsts.UP_LASTNAME].str);
			} else {
				this.LastName = "";
			}
			if (jsonUP[PJSONConsts.UP_EMAIL] != null && jsonUP[PJSONConsts.UP_EMAIL].type == JSONObject.Type.STRING) {
				this.Email = JSONObject.DecodeJsString(jsonUP[PJSONConsts.UP_EMAIL].str);
			} else {
				this.Email = "";
			}
			if (jsonUP[PJSONConsts.UP_AVATAR] != null && jsonUP[PJSONConsts.UP_AVATAR].type == JSONObject.Type.STRING) {
				this.AvatarLink = JSONObject.DecodeJsString(jsonUP[PJSONConsts.UP_AVATAR].str);
			} else {
				this.AvatarLink = "";
			}
			if (jsonUP[PJSONConsts.UP_LOCATION] != null && jsonUP[PJSONConsts.UP_LOCATION].type == JSONObject.Type.STRING) {
				this.Location = JSONObject.DecodeJsString(jsonUP[PJSONConsts.UP_LOCATION].str);
			} else {
				this.Location = "";
			}
			if (jsonUP[PJSONConsts.UP_GENDER] != null && jsonUP[PJSONConsts.UP_GENDER].type == JSONObject.Type.STRING) {
				this.Gender = JSONObject.DecodeJsString(jsonUP[PJSONConsts.UP_GENDER].str);
			} else {
				this.Gender = "";
			}
			if (jsonUP[PJSONConsts.UP_LANGUAGE] != null && jsonUP[PJSONConsts.UP_LANGUAGE].type == JSONObject.Type.STRING) {
				this.Language = JSONObject.DecodeJsString(jsonUP[PJSONConsts.UP_LANGUAGE].str);
			} else {
				this.Language = "";
			}
			if (jsonUP[PJSONConsts.UP_BIRTHDAY] != null && jsonUP[PJSONConsts.UP_BIRTHDAY].type == JSONObject.Type.STRING) {
				this.Birthday = JSONObject.DecodeJsString(jsonUP[PJSONConsts.UP_BIRTHDAY].str);
			} else {
				this.Birthday = "";
			}
			this.Extra = new Dictionary<String, JSONObject>();
			if (jsonUP[PJSONConsts.UP_EXTRA] != null && jsonUP[PJSONConsts.UP_EXTRA].type == JSONObject.Type.OBJECT) {
				foreach (String key in jsonUP[PJSONConsts.UP_EXTRA].keys) {
					this.Extra.Add(key, jsonUP[PJSONConsts.UP_EXTRA][key]);
				}
			}
		}
		
		/// <summary>
		/// Converts the current <c>UserProfile</c> to a JSONObject.
		/// </summary>
		/// <returns>A <c>JSONObject</c> representation of the current <c>UserProfile</c>.</returns>
		public virtual JSONObject toJSONObject() {
			JSONObject obj = base.toJSONObject();
			obj.AddField(PJSONConsts.UP_PROVIDER, this.Provider.ToString());
			obj.AddField(PJSONConsts.UP_USERNAME, this.Username);
			obj.AddField(PJSONConsts.UP_PROFILEID, this.ProfileId);
			obj.AddField(PJSONConsts.UP_FIRSTNAME, this.FirstName);
			obj.AddField(PJSONConsts.UP_LASTNAME, this.LastName);
			obj.AddField(PJSONConsts.UP_EMAIL, this.Email);
			obj.AddField(PJSONConsts.UP_AVATAR, this.AvatarLink);
			obj.AddField(PJSONConsts.UP_LOCATION, this.Location);
			obj.AddField(PJSONConsts.UP_GENDER, this.Gender);
			obj.AddField(PJSONConsts.UP_LANGUAGE, this.Language);
			obj.AddField(PJSONConsts.UP_BIRTHDAY, this.Birthday);
			obj.AddField(PJSONConsts.UP_EXTRA, new JSONObject(this.Extra));
			
			return obj;
		}

	}
}

