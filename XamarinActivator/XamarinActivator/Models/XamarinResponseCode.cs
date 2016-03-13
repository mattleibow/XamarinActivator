namespace XamarinActivator.Models
{
	internal enum XamarinResponseCode
	{
		TooManyMachines = -4,
		CouldNotFindLicense = -3,
		UserLookupFailure = -2,
		MissingData = -1,
		Success = 0,
		ServerError = 1,
		DataError = 2,
		ActivationError = 3,
		InvalidProductVersion = 4
	}
}
