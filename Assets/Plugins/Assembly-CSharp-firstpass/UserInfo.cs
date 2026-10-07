using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class UserInfo
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string firstName;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string lastName;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string photoUrl;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string userId;

	public string FirstName
	{
		get
		{
			return GetFirstName();
		}
		private set
		{
			SetFirstName(value);
		}
	}

	public string LastName
	{
		get
		{
			return GetLastName();
		}
		private set
		{
			SetLastName(value);
		}
	}

	public string PhotoUrl
	{
		get
		{
			return GetPhotoUrl();
		}
		private set
		{
			SetPhotoUrl(value);
		}
	}

	public string UserID
	{
		get
		{
			return GetUserId();
		}
		private set
		{
			SetUserId(value);
		}
	}

	internal UserInfo(string BBNKIBKPBLO)
	{
		string[] array = BBNKIBKPBLO.Split('|');
		SetUserId(array[0]);
		SetPhotoUrl(array[1]);
		if (array[2].Contains(" "))
		{
			string[] array2 = array[2].Split(' ');
			SetFirstName(array2[0]);
			SetLastName(array2[1]);
		}
		else
		{
			SetFirstName(array[2]);
			SetLastName(string.Empty);
		}
	}

	internal UserInfo(string MEEFALMGOMC, string NEEAGKJGKDM, string HGLELKMAJMJ, string PDJEDKAFEAK)
	{
		SetFirstName(PDJEDKAFEAK);
		SetLastName(HGLELKMAJMJ);
		SetPhotoUrl(NEEAGKJGKDM);
		SetUserId(MEEFALMGOMC);
	}

	private UserInfo()
	{
	}

	public string GetFirstName()
	{
		return firstName;
	}

	private void SetFirstName(string value)
	{
		firstName = value;
	}

	public string GetLastName()
	{
		return lastName;
	}

	private void SetLastName(string value)
	{
		lastName = value;
	}

	public string GetPhotoUrl()
	{
		return photoUrl;
	}

	private void SetPhotoUrl(string value)
	{
		photoUrl = value;
	}

	public string GetUserId()
	{
		return userId;
	}

	private void SetUserId(string value)
	{
		userId = value;
	}

	internal static Dictionary<string, UserInfo> GetInfos(string BBNKIBKPBLO)
	{
		Dictionary<string, UserInfo> dictionary = new Dictionary<string, UserInfo>();
		string[] array = BBNKIBKPBLO.Split(new char[1] { '^' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			UserInfo jPKEEFNNAAP = new UserInfo(array[i]);
			if (!dictionary.ContainsKey(jPKEEFNNAAP.GetUserId()))
			{
				dictionary.Add(jPKEEFNNAAP.GetUserId(), jPKEEFNNAAP);
			}
			else
			{
				dictionary[jPKEEFNNAAP.GetUserId()] = jPKEEFNNAAP;
			}
		}
		return dictionary;
	}

	[SpecialName]
	public static bool op_Implicit(UserInfo KEJDJHAGBMK)
	{
		return KEJDJHAGBMK != null;
	}
}
