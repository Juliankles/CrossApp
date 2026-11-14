namespace Core.Domain;

// Видання (книга як твір): одне видання може мати кілька примірників BookCopy з тим самим ISBN
public sealed class Book
{
    public string Isbn { get; }
    public string Title { get; }
    public string Author { get; }
    public int Year { get; }
    public string Genre { get; }

    private Book(string isbn, string title, string author, int year, string genre)
    {
        Isbn = isbn;
        Title = title;
        Author = author;
        Year = year;
        Genre = genre;
    }

    public static Book Create(string isbn, string title, string author, int year, string genre)
    {
        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN не може бути порожнім", nameof(isbn));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Назва видання обов'язкова", nameof(title));

        if (string.IsNullOrWhiteSpace(genre))
            throw new ArgumentException("Жанр обов'язковий", nameof(genre));

        if (year < 1450 || year > DateTime.Today.Year)
            throw new ArgumentOutOfRangeException(nameof(year), year, "Рік видання поза допустимими межами");

        return new Book(isbn.Trim(), title.Trim(), (author ?? "").Trim(), year, genre.Trim());
    }

    public override string ToString() => $"{Title} ({Author}, {Year})";
}
