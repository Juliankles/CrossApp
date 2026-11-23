namespace Core.Dto;

// Видання для експорту/імпорту каталогу (варіант «Бібліотека»): isbn, title, author, year, copies
public sealed record EditionDto(string Isbn, string Title, string Author, int Year, int Copies);
