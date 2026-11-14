namespace Core.Reports;

// Рядок звіту «прострочені видачі»: яка видача, який примірник, хто взяв, до коли мав повернути і на скільки днів запізнюється
public sealed record OverdueLine(string LoanId, string CopyId, string ReaderId, DateOnly DueDate, int DaysOverdue);
