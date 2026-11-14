namespace Core.Reports;

// Рядок звіту «топ читачів»: хто, скільки всього видач, скільки книжок зараз на руках
public sealed record ReaderLine(string ReaderId, int Loans, int Open);
