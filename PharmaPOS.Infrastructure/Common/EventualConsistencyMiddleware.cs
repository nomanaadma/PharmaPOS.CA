using MediatR;
using Microsoft.AspNetCore.Http;
using PharmaPOS.Domain.Common;
using PharmaPOS.Infrastructure.Common.Persistence;

namespace PharmaPOS.Infrastructure.Common;

public class EventualConsistencyMiddleware(RequestDelegate next)
{

	public async Task InvokeAsync(HttpContext context, IPublisher publisher, ApplicationDbContext dbContext)
	{
		var transaction = await dbContext.Database.BeginTransactionAsync();

		context.Response.OnCompleted(async () =>
		{
			try
			{
				if (context.Items.TryGetValue("DomainEventsQueue", out var value) &&
				    value is Queue<IDomainEvent> domainEventsQueue)
				{
					while (domainEventsQueue!.TryDequeue(out var domainEvent))
					{
						await publisher.Publish(domainEvent);
					}
				}

				await transaction.CommitAsync();
			}
			catch (Exception)
			{
				// notify the client that even though they got a good response, the changes didn't take place
				// due to an unexpected error
			}
			finally
			{
				await transaction.DisposeAsync();
			}

		});

		await next(context);
	}
}