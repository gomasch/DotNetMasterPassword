﻿using System;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using MasterPassword.Crypto;

namespace MasterPassword.Core
{
	/// <summary>
	/// contains the core algorithm for Master Password
	/// 
	/// http://masterpasswordapp.com/algorithm.html
	/// </summary>
	public static class Algorithm
	{
		/// <summary>
		/// Calculates the master key.
		/// </summary>
		/// <returns>The master key.</returns>
		/// <param name="userName">User name.</param>
		/// <param name="masterPassword">Master password.</param>
		public static byte[] CalcMasterKey(string userName, string masterPassword )
		{
			byte[] result = new byte[64];

			var masterBytes = UTF8Encoding.UTF8.GetBytes (masterPassword);
			var salt = Combine (UTF8Encoding.UTF8.GetBytes ("com.lyndir.masterpassword"), GetBytes (userName.Length), UTF8Encoding.UTF8.GetBytes (userName));
			SCrypt.ComputeKey (
				masterBytes,  
				salt,
				32768,
				8,
				2,
				64,
				result
			);
				
			return result;
		}

		private static byte[] GetBytes(int value)
		{
			return GetBytes ((uint)value);
		}

		private static byte[] GetBytes(uint value)
		{
			//32-bit unsigned integers in network byte order. (big endian)
			byte[] result = new byte[4];
			result[0] = (byte)((value >> 24) & 0xFFu);
			result[1] = (byte)((value >> 16) & 0xFFu);
			result[2] = (byte)((value >> 8) & 0xFFu);
			result[3] = (byte)(value & 0xFFu);

			return result;
		}

		private static byte [] Combine(params byte[][] arrays)
		{
			int len = 0;
			for (int i = 0; i < arrays.Length; i++) {
				len += arrays[i].Length;
			}

			byte[] rv = new byte[ len ];
			int offset = 0;
			foreach ( byte[] array in arrays ) {
				System.Buffer.BlockCopy( array, 0, rv, offset, array.Length );
				offset += array.Length;
			}
			return rv;
		}
			
		/// <summary>
		/// Calculates the template seed for a site. The template seed is essentially the site-specific secret in binary form.
		/// </summary>
		/// <returns>The template seed.</returns>
		/// <param name="masterKey">Master key.</param>
		/// <param name="siteName">Site name.</param>
		/// <param name="counter">Counter.</param>
		public static byte[] CalcTemplateSeed(byte[] masterKey, string siteName, int counter)
		{

			var hash = new HMACSHA256(masterKey);

			return hash.ComputeHash(
				Combine(
					UTF8Encoding.UTF8.GetBytes("com.lyndir.masterpassword"), 
					GetBytes(siteName.Length), 
					UTF8Encoding.UTF8.GetBytes(siteName), 
					GetBytes(counter))); 
		}

		/// <summary>
		/// Calculate the password for the site based on the password type.
		/// </summary>
		/// <returns>The site password.</returns>
		/// <param name="templateSeedForPassword">Template seed for password.</param>
		/// <param name="typeOfPassword">Type of password.</param>
		public static string CalcPassword(byte[] templateSeedForPassword, PasswordType typeOfPassword)
		{
			string[] templates = TemplateForType[typeOfPassword];
			string template = templates[templateSeedForPassword[0] % templates.Length];

			char[] password = new char[template.Length];

			for (int i = 0; i < password.Length; i++) {
				string characterGroup = CharacterGroup[template [i]]; // get charactergroup from template 

				char charHere = characterGroup [templateSeedForPassword [i + 1] % characterGroup.Length];
				password[i] = charHere; 
			}

			return new string (password); // convert character array to string
		}

		/// <summary>
		/// Template for the password type. Each letter in the template stands for a character group.
		/// </summary>
		public static Dictionary<PasswordType, string[]> TemplateForType = new Dictionary<PasswordType, string[]> 
		{
			{ PasswordType.MaximumSecurityPassword, new string[] { "anoxxxxxxxxxxxxxxxxx", "axxxxxxxxxxxxxxxxxno"} },
			{ PasswordType.LongPassword, new string[] 
				{ 
					"CvcvnoCvcvCvcv",
					"CvcvCvcvnoCvcv",
					"CvcvCvcvCvcvno",
					"CvccnoCvcvCvcv",
					"CvccCvcvnoCvcv",
					"CvccCvcvCvcvno",
					"CvcvnoCvccCvcv",
					"CvcvCvccnoCvcv",
					"CvcvCvccCvcvno",
					"CvcvnoCvcvCvcc",
					"CvcvCvcvnoCvcc",
					"CvcvCvcvCvccno",
					"CvccnoCvccCvcv",
					"CvccCvccnoCvcv",
					"CvccCvccCvcvno",
					"CvcvnoCvccCvcc",
					"CvcvCvccnoCvcc",
					"CvcvCvccCvccno",
					"CvccnoCvcvCvcc",
					"CvccCvcvnoCvcc",
					"CvccCvcvCvccno"} },
			{ PasswordType.MediumPassword, new string[] { "CvcnoCvc", "CvcCvcno"} },
			{ PasswordType.ShortPassword, new string[] { "Cvcn"} },
			{ PasswordType.BasicPassword, new string[] { "aaanaaan", "aannaaan", "aaannaaa"} },
			{ PasswordType.PIN, new string[] { "nnnn"} }
		};

		/// <summary>
		/// Content of the character groups
		/// </summary>
		public static Dictionary<char, string> CharacterGroup = new Dictionary<char, string> {
			{ 'V', "AEIOU" }, 
			{ 'C', "BCDFGHJKLMNPQRSTVWXYZ" }, 
			{ 'v', "aeiou" }, 
			{ 'c', "bcdfghjklmnpqrstvwxyz" }, 
			{ 'A', "AEIOUBCDFGHJKLMNPQRSTVWXYZ" }, 
			{ 'a', "AEIOUaeiouBCDFGHJKLMNPQRSTVWXYZbcdfghjklmnpqrstvwxyz" }, 
			{ 'n', "0123456789" }, 
			{ 'o', "@&%?,=[]_:-+*$#!'^~;()/." }, 
			{ 'x', "AEIOUaeiouBCDFGHJKLMNPQRSTVWXYZbcdfghjklmnpqrstvwxyz0123456789!@#$%^&*()" }  // Typo in spec, stated X instead of x
		};

	}
}
