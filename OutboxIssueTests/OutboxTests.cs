using FluentAssertions;
using MassTransit;
using OutboxIssue;
using Phoenix.Quotation.Api.Tests;
using Xunit;

namespace OutboxIssueTests;

public class OutboxTests
{
    [Fact]
    public async Task Test1()
    {
        // Arrange
        var webApplicationFactory = new CustomWebApplicationFactory();
        await webApplicationFactory.ServiceBusHarness.Start();
        var sendEndpoint = await webApplicationFactory.ServiceBusHarness.Bus.GetSendEndpoint(new Uri("queue:message-a"));

        // Act
        await sendEndpoint.Send(new MessageA { Text = "Hello" });
        (await webApplicationFactory.ServiceBusHarness.Consumed.Any<MessageA>()).Should().Be(true);

        // Assert
        GlobalStateForAssertions.ActualSendEndpointProviderType?.Name.Should().Contain("MassTransit.EntityFrameworkCoreIntegration.DbContextOutbox");
    }
}
