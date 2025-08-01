using Microsoft.Extensions.Hosting;
using Quartz;
namespace ConsoleQuartz
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseContentRoot(Directory.GetCurrentDirectory())
            .ConfigureServices((hostContext, services) =>
            {
                try
                {
                    ConfigureDecryptionContex(hostContext, services);

                    // Register Email notifcation.
                    services.AddSingleton<IEmailNotificaiton, EmailNotification>();

                    JobSettings jobSettings = new JobSettings();
                    hostContext.Configuration.Bind("JobSettings", jobSettings);
                    services.AddSingleton(jobSettings);

                    AppConfig appConfig = new AppConfig();
                    hostContext.Configuration.Bind("AppConfig", appConfig);
                    services.AddSingleton(appConfig);

                    string defaultConnString = services.BuildServiceProvider().GetService<ConnectionStrings>().DefaultConnection;
                    services.Configure<QuartzOptions>(hostContext.Configuration.GetSection("Quartz"));
                    services.AddQuartz(q =>
                    {
                        q.Properties.Add("quartz.dataSource.default.connectionString", defaultConnString);
                        //q.UseMicrosoftDependencyInjectionScopedJobFactory();
                        q.AddJobAndTrigger<Mercer.HB.Scheduler.Jobs.BenefitsYouCensusJob>(jobSettings.BenefitsYouCensusJobSettings.CronExpression);
                        q.AddJobAndTrigger<Mercer.HB.Scheduler.Jobs.BenefitsYouPolicyJob>(jobSettings.BenefitsYouPolicyJobSettings.CronExpression);
                        q.AddJobAndTrigger<Mercer.HB.Scheduler.Jobs.BenefitsYouInvoiceJob>(jobSettings.BenefitsYouInvoiceJobSettings.CronExpression);
                        q.AddJobAndTrigger<Mercer.HB.Scheduler.Jobs.BenefitsYouPlanJob>(jobSettings.BenefitsYouPlanJobSettings.CronExpression);
                    });
                    services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
                   
                    
                    ConfigureDBContex(services, defaultConnString);
                }
                catch (Exception ex)
                {
                    // Send E-mail notification for exception occured.
                    Notification.SendEmail(ex.Message);
                    using EventLog eventLog = new EventLog();
                    eventLog.Source = "Mercer.HB.Scheduler";
                    eventLog.WriteEntry($"Error: {ex.Message}; StackTrace: {ex.StackTrace}", EventLogEntryType.Error);
                }
            });
    }
}
