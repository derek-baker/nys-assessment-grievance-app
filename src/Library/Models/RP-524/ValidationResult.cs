namespace Library.Models
{
    public class ValidationResult
    {
        public bool IsValid { get; }
        public string FirstInvalidFieldMessage { get; }

        public ValidationResult(bool isValid, string message)
        {
            IsValid = isValid;
            FirstInvalidFieldMessage = message;
        }
    }
}
