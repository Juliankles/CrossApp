namespace Core.Reports;

// Група звіту «видання за жанрами»: жанр, кількість видань, найраніший і найпізніший рік
public sealed record GenreSummary(string Genre, int Editions, int FromYear, int ToYear);
