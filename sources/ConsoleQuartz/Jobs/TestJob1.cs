using Quartz;

namespace ConsoleQuartz.Jobs;
[DisallowConcurrentExecution]
public class TestJob1:IJob
{
    
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            JobExecutionRun job 
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}