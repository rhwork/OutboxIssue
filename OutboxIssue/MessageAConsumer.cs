using MassTransit;

namespace OutboxIssue;

public class MessageA
{
    public string Text { get; set; }
}

public class MessageAConsumer : IConsumer<MessageA>
{
    public MessageAConsumer(ADbContext aDbContext, ISendEndpointProvider sendEndpointProvider)
    {
        ADbContext=aDbContext;
    }

    public ADbContext ADbContext { get; }

    public async Task Consume(ConsumeContext<MessageA> context)
    {
        ADbContext.AEntities.Add(new AEntity { Description = context.Message.Text });
        await ADbContext.SaveChangesAsync();
    }
}
