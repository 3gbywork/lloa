namespace OfficeAutomationClient.OA
{
    internal enum TMessengerToken
    {
        LoginUserChanged,
        EmailUserChanged,
        ShowConfirmation,
    }

    internal enum EmailValidationResult
    {
        OK,
        ConnectionError,
        ProtocolError,
        AuthenticationFailed,
        Failed,
    }
}