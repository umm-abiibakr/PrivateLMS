using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PrivateLMS.Data;
using PrivateLMS.Models;
using PrivateLMS.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PrivateLMS.HostedServices
{
    public class OverdueReminderAndFineService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;

        public OverdueReminderAndFineService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var nextRun = now.Date.AddDays(1); // Next midnight
            var delay = nextRun - now;

            _timer = new Timer(DoWork, null, delay, TimeSpan.FromDays(1));
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                // Find overdue loans (DueDate < today, not returned)
                var today = DateTime.UtcNow.Date;
                var overdueLoans = await context.LoanRecords
                    .Include(lr => lr.User)
                    .Include(lr => lr.Book)
                    .Include(lr => lr.Fines)
                    .Where(lr => lr.DueDate.HasValue &&
                                 lr.DueDate.Value.Date < today &&
                                 lr.ReturnDate == null)
                    .ToListAsync();

                foreach (var loan in overdueLoans)
                {
                    try
                    {
                        // Calculate days overdue
                        var daysOverdue = (today - loan.DueDate.Value.Date).Days;
                        // Calculate fine: 1000 Naira base + 1000 Naira per day
                        var fineAmount = 1000 + (daysOverdue * 1000);

                        // Check if a fine already exists for today
                        var existingFine = loan.Fines.FirstOrDefault(f => f.IssuedDate.Date == today);
                        if (existingFine == null)
                        {
                            // Create new fine
                            var fine = new Fine
                            {
                                LoanId = loan.LoanRecordId,
                                Amount = fineAmount,
                                IssuedDate = today
                            };
                            context.Fines.Add(fine);
                        }
                        else
                        {
                            // Update existing fine if amount changed
                            if (existingFine.Amount != fineAmount)
                            {
                                existingFine.Amount = fineAmount;
                                context.Fines.Update(existingFine);
                            }
                        }

                        // Save changes to database
                        await context.SaveChangesAsync();

                        // Send reminder email to user
                        var emailBody = $@"
                            <h2>Overdue Book Reminder</h2>
                            <p>Dear {loan.User.FirstName} {loan.User.LastName},</p>
                            <p>The following book is overdue:</p>
                            <ul>
                                <li><strong>Book Title:</strong> {loan.Book.Title}</li>
                                <li><strong>Due Date:</strong> {loan.DueDate.Value.ToString("MMMM dd, yyyy")}</li>
                                <li><strong>Days Overdue:</strong> {daysOverdue}</li>
                                <li><strong>Total Fine:</strong> {fineAmount} Naira</li>
                            </ul>
                            <p>Please return the book as soon as possible to avoid additional fines (1000 Naira per day, starting at 1000 Naira). Contact us at admin@warathatulambiya.com if you have any issues.</p>
                            <p>Baarakallaahu Feekum,<br/>Admin@WarathatulAmbiya</p>";
                        await emailService.SendEmailAsync(loan.User.Email, "Overdue Book Reminder", emailBody);
                    }
                    catch (Exception ex)
                    {
                        // Log error (consider using a logging framework like Serilog)
                        Console.WriteLine($"Failed to process overdue reminder for LoanRecordId {loan.LoanRecordId}: {ex.Message}");
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}