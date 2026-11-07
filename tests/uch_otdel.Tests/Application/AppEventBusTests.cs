using uch_otdel.Application.Events;
using uch_otdel.Application.Workload;

namespace uch_otdel.Tests.Application;

public class AppEventBusTests
{
    [Fact]
    public void Event_reaches_every_subscriber()
    {
        var bus = new InProcessAppEventBus();
        var first = new List<AppEvent>();
        var second = new List<AppEvent>();
        using var s1 = bus.Subscribe(first.Add);
        using var s2 = bus.Subscribe(second.Add);

        bus.Publish(new AppEvent("Начат расчёт нагрузки"));

        Assert.Single(first);
        Assert.Single(second);
    }

    [Fact]
    public void Disposed_subscription_gets_nothing()
    {
        var bus = new InProcessAppEventBus();
        var received = new List<AppEvent>();
        var subscription = bus.Subscribe(received.Add);

        subscription.Dispose();
        subscription.Dispose();
        bus.Publish(new AppEvent("Событие"));

        Assert.Empty(received);
    }

    [Fact]
    public void Failing_subscriber_does_not_block_others()
    {
        var bus = new InProcessAppEventBus();
        var received = new List<AppEvent>();
        using var broken = bus.Subscribe(_ => throw new InvalidOperationException("вкладка закрыта"));
        using var working = bus.Subscribe(received.Add);

        bus.Publish(new AppEvent("Событие"));

        Assert.Single(received);
    }

    [Fact]
    public async Task Calculation_cannot_start_twice_and_reports_start_and_finish()
    {
        var bus = new InProcessAppEventBus();
        var finished = new TaskCompletionSource<AppEvent>(TaskCreationOptions.RunContinuationsAsynchronously);
        var received = new List<AppEvent>();
        using var subscription = bus.Subscribe(e =>
        {
            lock (received)
            {
                received.Add(e);
            }
            if (e.Title.Contains("завершён"))
            {
                finished.TrySetResult(e);
            }
        });
        var calculation = new CalculationService(bus) { DemoDuration = TimeSpan.FromMilliseconds(200) };

        Assert.True(calculation.TryStart("Методист"));
        Assert.False(calculation.TryStart("Заведующий"));

        await finished.Task.WaitAsync(TimeSpan.FromSeconds(10));

        lock (received)
        {
            Assert.Equal(2, received.Count);
            Assert.All(received, e => Assert.True(e.Important));
        }
    }
}
