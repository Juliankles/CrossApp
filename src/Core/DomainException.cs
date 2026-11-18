namespace Core;

// Порушення бізнес-правила бібліотеки (наприклад, «примірник уже видано»).
// Некоректні аргументи — це ArgumentException, технічні збої — IOException / JsonException.
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }

    public DomainException(string message, Exception inner) : base(message, inner) { }
}
