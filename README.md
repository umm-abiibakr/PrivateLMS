# PrivateLMS

This is a web-based Private Library Management System designed to manage and recommend books from a personal Islamic book collection. The system supports user registration, book borrowing, fine tracking, and intelligent book recommendations using an AI-driven approach with ANFIS.

## Features

- User authentication and profile management
- Book catalog with search, ratings, and availability tracking
- Loan management with due dates and fine calculations
- Admin dashboard for managing users, books, loans, and site content
- AI-based book recommendation engine (ANFIS)
- Dynamic site pages (Privacy Policy, FAQ, About) editable by admin
- Chatbot-style support widget
- Email notifications for account approvals, email verifications, borrowing, returns, and profile updates

## Technologies Used

- ASP.NET Core MVC (C#)
- Entity Framework Core
- ONNX Runtime for AI model integration
- SQL Server
- HTML, CSS (custom poetic theme), Bootstrap
- Python (model training in Google Colab using PyTorch & Optuna)
- GitHub for version control

## Email Functionality

The system includes email notification features. However, the GitHub version **does not contain API keys or credentials** for email services, for security reasons.

As a result:

- Email-related features will not function without valid API credentials.
- You may encounter errors when performing actions that depend on email (e.g., borrowing a book, profile update requests).
- I have **commentted out the email-related setup in `Program.cs`** to stop the application from failing to run due to missing email configuration. However, this will not prevent errors on other actions that rely on email functionality.

> Mailjet was used as the email provider in this application.

If needed, you may request sample credentials for testing or insert your own.

## Running the Project

1. Clone the repository.
2. Update `appsettings.json` with your SQL Server connection string.
3. Add your email API credentials (e.g., Mailjet) in the appropriate configuration section.
4. Build and run the project using Visual Studio 2022 or later.

## Note

If you require help running the project or testing features that require email functionality, feel free to reach out.
