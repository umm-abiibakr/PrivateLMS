using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PrivateLMS.Data;
using PrivateLMS.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PrivateLMS.HostedServices
{
    public class DueDateReminderService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;

        public DueDateReminderService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var nextRun = now.Date.AddDays(1);
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

                var reminderDate = DateTime.UtcNow.Date.AddDays(2);
                var loans = await context.LoanRecords
                    .Include(lr => lr.User)
                    .Include(lr => lr.Book)
                    .Where(lr => lr.DueDate.HasValue &&
                                 lr.DueDate.Value.Date == reminderDate &&
                                 lr.ReturnDate == null &&
                                 !lr.IsRenewed)
                    .ToListAsync();

                foreach (var loan in loans)
                {
                    try
                    {
                        var emailBody = $@"
                            <h2>Due Date Reminder</h2>
                            <p>Dear {loan.User.FirstName} {loan.User.LastName},</p>
                            <p>This is a reminder that the following book is due soon:</p>
                            <ul>
                                <li><strong>Book Title:</strong> {loan.Book.Title}</li>
                                <li><strong>Due Date:</strong> {loan.DueDate.Value.ToString("MMMM dd, yyyy")}</li>
                            </ul>
                            <p>Please return the book by the due date to avoid fines (1000 Naira per day, starting at 1000 Naira). You may also request a one-week renewal if eligible.</p>
                            <p>Baarakallaahu Feekum,<br/>Admin@WarathatulAmbiya</p>";
                        await emailService.SendEmailAsync(loan.User.Email, "Book Due Date Reminder", emailBody);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send reminder email for LoanRecordId {loan.LoanRecordId}: {ex.Message}");
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