using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace NoviCurrencyWallet.Jobs;

//using a static class which allows me to register the services from the Jobs project
public static class JobInfrastructure
{
	//extension method of the IServiceCollection and we will be calling it from the web api project to register anything to define in the Jobs Services
	public static void AddInfrastructure(this IServiceCollection services)
	{
		//Add the Quartz services that are required for it to run
		services.AddQuartz(options =>
		{
			var jobKey = JobKey.Create(nameof(RetrieveRatesBackgroundJob));   //job key will uniquely identify the background job - simple solution for naming the key is to use the name of the job

			//use Add job to configure my job
			options.AddJob<RetrieveRatesBackgroundJob>(options => options
				.WithIdentity(jobKey)
				.WithDescription("Retrieves and updates currency rates every 1 minute.")
			);

			options.AddTrigger(options => options                                        //Trigger that will execute the job 
				.WithIdentity($"{nameof(RetrieveRatesBackgroundJob)}-trigger")           //n Quartz.NET, when you configure a trigger inside services.AddQuartz, you need to explicitly specify the trigger identity and use the correct method chaining to apply the schedule.
				.ForJob(jobKey)
				.WithSimpleSchedule(schedule => schedule
					.WithIntervalInMinutes(1)                                        //WithCronSchedule("*/1****") alternative : trigger this job every one minute - cron syntax
					.RepeatForever()
				)
			 );

				               
		});

		//Hosted service implementation by Quartz - responsible to create a new instance of my background job when a trigger fires and then executing that background job
		services.AddQuartzHostedService(options =>
		{
			options.WaitForJobsToComplete = true; //this tells Quartz to wait for all jobs to complete before the app closes
		}
		);


	}
}




//video after 8 isolating the configuration for individual jobs